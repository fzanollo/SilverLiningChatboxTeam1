﻿// Copyright (c) Microsoft Corporation. All rights reserved.
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
    public class CasualDialog : CancelAndHelpDialog
    {
        private readonly MathBotRecognizer _luisRecognizer;
        private Questions gameObject = new Questions();
        private int questionNr = 0;
        private int points = 0;

        private string currentQuestion;
        private int currentAnswer;

        public CasualDialog(MathBotRecognizer luisRecognizer)
            : base(nameof(CasualDialog))
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
                    if (answer.Equals(currentAnswer))
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
            private async Task<DialogTurnResult> EndGameAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var gameEndMessage = $"You scored: {points}!";
            return await stepContext.ReplaceDialogAsync(InitialDialogId, gameEndMessage, cancellationToken);
        }
    }
}
