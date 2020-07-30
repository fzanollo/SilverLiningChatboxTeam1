// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.Timers;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Luis;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;
using QuestionsOverview;

namespace Microsoft.BotBuilderSamples.Dialogs
{
    public class TimeDialog : CancelAndHelpDialog
    {
        private readonly MathBotRecognizer _luisRecognizer;
        private Questions gameObject = new Questions();
        private int questionNr = 0;
        private int points = 0;
        private bool boolEnd = false;
        private bool flag = false;
        private string currentQuestion;
        private int currentAnswer;

        public System.Timers.Timer aTimer = new System.Timers.Timer();
        

        public TimeDialog(MathBotRecognizer luisRecognizer)
            : base(nameof(TimeDialog))
        {
            _luisRecognizer = luisRecognizer;
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            
            aTimer.Elapsed += new ElapsedEventHandler(endGame);
            
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
              
                QuestionAsync,
                AnswerAsync,
                EndGameAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }


       

        private async Task<DialogTurnResult> QuestionAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var gameDetails = (GameDetails)stepContext.Options;
            var x = double.Parse(gameDetails.GameAmount)* 60000.0;
            if (!flag)
            {
                aTimer.Interval = x;
                aTimer.Enabled = true;
                flag = true;
            }
           
           

            var nextQuestion = gameObject.getQuestion();
            currentQuestion = $"Question number {questionNr+1}: {nextQuestion.Item1}";
            currentAnswer = nextQuestion.Item2;

            //var attachments = new List<Attachment>();

            //var reply = MessageFactory.Attachment(attachments);
            //reply.Attachments.Add(Cards.GetThumbnailCard4("Question "+ (questionNr+1).ToString(),nextQuestion.Item1).ToAttachment());
            //await stepContext.Context.SendActivityAsync(reply, cancellationToken);
            //prompt question and await answer
            var promptMessage = MessageFactory.Text(currentQuestion, currentQuestion, InputHints.ExpectingInput);
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
        }

        private async Task<DialogTurnResult> AnswerAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var luisResult = await _luisRecognizer.RecognizeAsync<MathBot>(stepContext.Context, cancellationToken);
            switch (luisResult.TopIntent().intent)
            {
                case MathBot.Intent.Answer:
                    questionNr++;

                    string answer = luisResult.NumberEntity;
                    var feedbackMessageText = "";
                    if (answer.Equals(currentAnswer.ToString()))
                    {
                        feedbackMessageText = $"{answer} is correct!, excellent";
                        points++;
                    }
                    else
                    {
                        feedbackMessageText = $"{answer} is wrong but keep trying, you got this!";
                    }

                    
                    

                    var getGreetingMessage = MessageFactory.Text(feedbackMessageText, feedbackMessageText, InputHints.IgnoringInput);
                    await stepContext.Context.SendActivityAsync(getGreetingMessage, cancellationToken);
                    break;

                default:
                    // Catch all for unhandled intents
                    var didntUnderstandMessageText = $"Sorry, I didn't get that. Please try asking in a different way (intent was {luisResult.TopIntent().intent})";
                    var didntUnderstandMessage = MessageFactory.Text(didntUnderstandMessageText, didntUnderstandMessageText, InputHints.IgnoringInput);
                    await stepContext.Context.SendActivityAsync(didntUnderstandMessage, cancellationToken);
                    break;
            }

            return await stepContext.NextAsync(null, cancellationToken);
        }

        private void endGame(object source, ElapsedEventArgs e)
        {
            boolEnd = true;
            //Console.WriteLine("Hi");
        }
        private async Task<DialogTurnResult> EndGameAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (!boolEnd)
            {
                return await stepContext.ReplaceDialogAsync(InitialDialogId, (GameDetails)stepContext.Options, cancellationToken);
            }
            else
            {
                questionNr = 0;
                var gameEndMessageText = $"Your time is over! You scored: {points}!. Type anything if you want to play again :)";
                var getEndMessage = MessageFactory.Text(gameEndMessageText, gameEndMessageText, InputHints.IgnoringInput);
                await stepContext.Context.SendActivityAsync(getEndMessage, cancellationToken);

                return await stepContext.EndDialogAsync("optional", cancellationToken);
            }
        }
    }
}
