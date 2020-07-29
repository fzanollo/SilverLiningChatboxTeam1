// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Luis;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;
using QuestionsOverview;

namespace Microsoft.BotBuilderSamples.Dialogs
{
    public class GameDialog : CancelAndHelpDialog
    {
        private readonly MathBotRecognizer _luisRecognizer;
        private const string DestinationStepMsgText = "Where would you like to travel to?";
        private const string OriginStepMsgText = "Where are you traveling from?";
        private Questions gameObject = new Questions();

        

        public GameDialog(MathBotRecognizer luisRecognizer)
            : base(nameof(GameDialog))
        {
            _luisRecognizer = luisRecognizer;
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                GameStepAsync,
                EndGameAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> GameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var gameDetails = (GameDetails)stepContext.Options;
            if (gameDetails.GameType == "points")
            {
                int curr_points = 0;
                int max_points = 10;
                //int max_points = gameDetails.maxPoints;
                while(curr_points < max_points)
                {
                    var nextQuestion = gameObject.getQuestion();
                    string question = nextQuestion.Item1;
                    int correct_answer = nextQuestion.Item2;
                    //prompt question and await answer
                    var promptMessage = MessageFactory.Text(question, question, InputHints.ExpectingInput);
                    await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
                    var luisResult = await _luisRecognizer.RecognizeAsync<MathBot>(stepContext.Context, cancellationToken);
                    //compare answer to correct answer
                    //check intent:
                        // if intent == answer
                            //ifincorrect -> prompt "Incorrect answer! Correct answer is {correct_answer}, -1 point
                            // if correct -> prompt "Correct Answer! +1
                        //if intent == stop
                            //end conversation, go to EndGameAsync
                    //if no more questions exist, prompt something and exit
                }
            }
            else if (gameDetails.GameType == "casual")
            {
                int curr_question = 0;
                int max_questions = 10;
                //int max_questions = gameDetails.maxQuestions
                while (curr_question < max_questions)
                {

                }
            }
            else
            {
                //else has to be timed
            }
            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> EndGameAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {


            return await stepContext.NextAsync(null, cancellationToken);
        }

        
    }
}
