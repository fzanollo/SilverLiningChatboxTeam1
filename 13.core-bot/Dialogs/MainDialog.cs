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

namespace Microsoft.BotBuilderSamples.Dialogs
{

    public class MainDialog : ComponentDialog
    {
        private readonly MathBotRecognizer _luisRecognizer;
        protected readonly ILogger Logger;
            private GameDetails gameDetails = null;

        // Dependency injection uses this constructor to instantiate MainDialog
        public MainDialog(MathBotRecognizer luisRecognizer, GameDialog gameDialog, ILogger<MainDialog> logger)
            : base(nameof(MainDialog))
        {
            _luisRecognizer = luisRecognizer;
            Logger = logger;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(gameDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                IntroStepAsync,
                GreetingStepAsync,
                GameChoiceStepAsync,
                HandleResponseAsync,
                FinalStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (!_luisRecognizer.IsConfigured)
            {
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the appsettings.json file.", inputHint: InputHints.IgnoringInput), cancellationToken);

                return await stepContext.NextAsync(null, cancellationToken);
            }
            var attachments1 = new List<Attachment>();
            
            
            
            var reply1 = MessageFactory.Attachment(attachments1);
            
            reply1.Attachments.Add(Cards.GetAnimationCard().ToAttachment());
            await stepContext.Context.SendActivityAsync(reply1, cancellationToken);
            
            var messageText = stepContext.Options?.ToString() ?? $"Hi, my name is Mat, what is your name?";
            var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
        }

        private async Task<DialogTurnResult> GreetingStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var luisResult = await _luisRecognizer.RecognizeAsync<MathBot>(stepContext.Context, cancellationToken);

            switch (luisResult.TopIntent().intent)
            {

                case MathBot.Intent.Name:
                    string name = luisResult.NameEntity;
                    var getGreetingMessageText = $"Hello, {name}!";
                    var getGreetingMessage = MessageFactory.Text(getGreetingMessageText, getGreetingMessageText, InputHints.IgnoringInput);
                    await stepContext.Context.SendActivityAsync(getGreetingMessage, cancellationToken);
                    break;

                default:
                    // Catch all for unhandled intents
                    var didntUnderstandMessageText = $"Sorry, I didn't get that. Please try asking in a different way (intent was {luisResult.TopIntent().intent})";
                    var didntUnderstandMessage = MessageFactory.Text(didntUnderstandMessageText, didntUnderstandMessageText, InputHints.IgnoringInput);
                    await stepContext.Context.SendActivityAsync(didntUnderstandMessage, cancellationToken);
                    break;
            }
            var attachments = new List<Attachment>();
            
            var messageText = stepContext.Options?.ToString() ?? $"What type of challenge do you want to play? These are the options: ";
            
            var reply = MessageFactory.Attachment(attachments);
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            reply.Attachments.Add(Cards.GetThumbnailCard1().ToAttachment());
            reply.Attachments.Add(Cards.GetThumbnailCard2().ToAttachment()); 
            reply.Attachments.Add(Cards.GetThumbnailCard3().ToAttachment()); 
           
            
            var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
            await stepContext.Context.SendActivityAsync(reply, cancellationToken);
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
        }

        private async Task<DialogTurnResult> GameChoiceStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var luisResult = await _luisRecognizer.RecognizeAsync<MathBot>(stepContext.Context, cancellationToken);
         
            switch (luisResult.TopIntent().intent)
            {

                case MathBot.Intent.Game:
                    string challengeType = luisResult.ChallengeType;
                    var getGreetingMessageText = $"A {challengeType} challenge? awesome, let's go!";
                    var getGreetingMessage = MessageFactory.Text(getGreetingMessageText, getGreetingMessageText, InputHints.IgnoringInput);
                    await stepContext.Context.SendActivityAsync(getGreetingMessage, cancellationToken);


                    gameDetails = new GameDetails()
                    {
                        GameType = challengeType
                    };

                   
                    if(challengeType == "time")
                    {
                        var messageText = stepContext.Options?.ToString() ?? $"How long do you want to play for?";
                        var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
                        return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);

                    }
                    else
                    {
                        if(challengeType == "points")
                        {
                            var messageText = stepContext.Options?.ToString() ?? $"How many lives will you have?";
                            var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
                            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
                        }
                        else
                        {
                            if(challengeType == "casual")
                            {
                                var messageText = stepContext.Options?.ToString() ?? $"How many questions do you want?";
                                var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
                                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
                            }
                        }
                    }
                    /////

                    return await stepContext.BeginDialogAsync(nameof(GameDialog), gameDetails, cancellationToken);

                default:
                    // Catch all for unhandled intents
                    var didntUnderstandMessageText = $"Sorry, I didn't get that. Please try asking in a different way (intent was {luisResult.TopIntent().intent})";
                    var didntUnderstandMessage = MessageFactory.Text(didntUnderstandMessageText, didntUnderstandMessageText, InputHints.IgnoringInput);
                    await stepContext.Context.SendActivityAsync(didntUnderstandMessage, cancellationToken);
                    break;
            }

            return await stepContext.NextAsync(null, cancellationToken);
        }
        private async Task<DialogTurnResult> HandleResponseAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var messageTex = stepContext.Options?.ToString() ?? $"How many questions do?";
            var promptMessag = MessageFactory.Text(messageTex, messageTex, InputHints.ExpectingInput);

            var luisResult = await _luisRecognizer.RecognizeAsync<MathBot>(stepContext.Context, cancellationToken);
            //switch (luisResult.TopIntent().intent)
            //{
                //case MathBot.Intent.Answer:
                    //Initialize numberDetails with any entities we may have found in the response.
                    gameDetails.GameAmount = luisResult.NumberEntity;

                    var messageText = stepContext.Options?.ToString() ?? $"How many questions do you want?{gameDetails.GameAmount}";
                    var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
                    return await stepContext.BeginDialogAsync(nameof(GameDialog), gameDetails, cancellationToken);

                //default:
                    // Catch all for unhandled intents
                    //var didntUnderstandMessageText = $"Sorry, I didn't get that. Please try asking in a different way (intent was {luisResult.TopIntent().intent})";
                    //var didntUnderstandMessage = MessageFactory.Text(didntUnderstandMessageText, didntUnderstandMessageText, InputHints.IgnoringInput);
                    //await stepContext.Context.SendActivityAsync(didntUnderstandMessage, cancellationToken);
                    //break;
            //}
            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //// If the child dialog ("BookingDialog") was cancelled, the user failed to confirm or if the intent wasn't BookFlight
            //// the Result here will be null.
            //if (stepContext.Result is BookingDetails result)
            //{
            //    // Now we have all the booking details call the booking service.

            //    // If the call to the booking service was successful tell the user.

            //    var timeProperty = new TimexProperty(result.TravelDate);
            //    var travelDateMsg = timeProperty.ToNaturalLanguage(DateTime.Now);
            //    var messageText = $"Thanks for playing with me {}";
            //    var message = MessageFactory.Text(messageText, messageText, InputHints.IgnoringInput);
            //    await stepContext.Context.SendActivityAsync(message, cancellationToken);
            //}

            // Restart the main dialog with a different message the second time around
            var promptMessage = "What else can I do for you?";
            return await stepContext.ReplaceDialogAsync(InitialDialogId, promptMessage, cancellationToken);
        }

        private IList<Choice> GetChoices()
        {
            var cardOptions = new List<Choice>()
            {
                new Choice() { Value = "Time Challenge", Synonyms = new List<string>() { "time", "timed" } },
                new Choice() { Value = "Points Challenge", Synonyms = new List<string>() { "points" } },
                new Choice() { Value = "Casual", Synonyms = new List<string>() { "casual" } },
                
            };

            return cardOptions;
        }
    }
}
