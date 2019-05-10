// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using kennisAvondBot.Dialogs;
using kennisAvondBot.State;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;

namespace kennisAvondBot
{
    public class KennisAvondBot : ActivityHandler
    {
        private BotState _conversationState;
        private BotState _userState;
        private DialogSet _dialogSet;

        public KennisAvondBot(ConversationState conversationState, UserState userState)
        {
            _conversationState = conversationState;
            _userState = userState;

            var dialogSet = new DialogSet(_conversationState.CreateProperty<DialogState>(nameof(DialogState)));
            dialogSet.Add(new UserProfileDialog(userState));
            dialogSet.Add(new QuestionDialog(userState));
            
            _dialogSet = dialogSet;
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            // Get UserProfile and ConversationData using the accessors.
            var conversationStateAccessors = _conversationState.CreateProperty<ConversationData>(nameof(ConversationData));
            var conversationData = await conversationStateAccessors.GetAsync(turnContext, () => new ConversationData());

            var userStateAccessors = _userState.CreateProperty<UserProfile>(nameof(UserProfile));
            var userProfile = await userStateAccessors.GetAsync(turnContext, () => new UserProfile());

            // Create the dialogContext
            var dialogContext = await _dialogSet.CreateContextAsync(turnContext, cancellationToken);
            var results = await dialogContext.ContinueDialogAsync(cancellationToken);
            bool activeDialog = results.Status != DialogTurnStatus.Empty;


            // Do nothing else besides continuing the active dialog if there is one.
            if (activeDialog) return;

            // Start the UserProfileDialog if the name of the user is unknown.
            if (string.IsNullOrEmpty(userProfile.Name))
            {
                await dialogContext.BeginDialogAsync(nameof(UserProfileDialog));
                return;
            }

            // Determine what to do based on user input.
            switch (turnContext.Activity.Text.Trim().ToLower())
            {
                case "vraag":
                    await dialogContext.BeginDialogAsync(nameof(QuestionDialog));
                    break;
                default:
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Hoi {userProfile.Name}"));
                    break;
            }
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await _userState.SaveChangesAsync(turnContext, false, cancellationToken);
        }
    }
}
