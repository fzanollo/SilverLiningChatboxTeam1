// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using Luis;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using QuestionsOverview;

namespace Microsoft.BotBuilderSamples.Dialogs
{
    public class PointsDialog : CancelAndHelpDialog
    {
        private readonly MathBotRecognizer _luisRecognizer;
        private Questions gameObject = new Questions();
        private int questionNr = 0;
        private int points = 0;
        private int lives_remaining = 3;

        private string currentQuestion;
        private int currentAnswer;
        private bool started = false;

        public PointsDialog(MathBotRecognizer luisRecognizer)
            : base(nameof(PointsDialog))
        {
            _luisRecognizer = luisRecognizer;
            AddDialog(new TextPrompt(nameof(TextPrompt)));
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
            if (!started)
            {
                var gameDetails = (GameDetails)stepContext.Options;
                lives_remaining = int.Parse(gameDetails.GameAmount);
            }
            
            started = true;
            var nextQuestion = gameObject.getQuestion();
            currentQuestion = $"Question number {questionNr + 1}: {nextQuestion.Item1}";
            currentAnswer = nextQuestion.Item2;

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
                        lives_remaining--;
                        feedbackMessageText = $"{answer} is wrong but keep trying, you got this! Remember, you have {lives_remaining} lives left!";
                        if (lives_remaining < 1)
                        {
                            return await stepContext.NextAsync(1, cancellationToken);
                        }
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
        private async Task<DialogTurnResult> EndGameAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var gameEndMessageText = $"You scored: {points}!";
            var gameEndMessage = MessageFactory.Text(gameEndMessageText, gameEndMessageText, InputHints.IgnoringInput);
            if (lives_remaining < 1)
            {
                await stepContext.Context.SendActivityAsync(gameEndMessage, cancellationToken);
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }

            return await stepContext.ReplaceDialogAsync(InitialDialogId, gameEndMessage, cancellationToken);

        }
    }
}
