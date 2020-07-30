// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
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
    public class GameDialog : ComponentDialog
    {
        private readonly MathBotRecognizer _luisRecognizer;
        private const string DestinationStepMsgText = "Where would you like to travel to?";
        private const string OriginStepMsgText = "Where are you traveling from?";
        private Questions gameObject = new Questions();

        public GameDialog(MathBotRecognizer luisRecognizer, CasualDialog casualDialog, PointsDialog pointsDialog)
            : base(nameof(GameDialog))
        {
            _luisRecognizer = luisRecognizer;
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(pointsDialog);
            AddDialog(casualDialog);
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
                return await stepContext.BeginDialogAsync(nameof(PointsDialog), gameDetails, cancellationToken); 
            }
            else if (gameDetails.GameType == "casual")
            {
                return await stepContext.BeginDialogAsync(nameof(CasualDialog), gameDetails, cancellationToken);
            }
            else //timed
            {

            }
            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> EndGameAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {


            return await stepContext.NextAsync(null, cancellationToken);
        }

        
    }
}
