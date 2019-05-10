using kennisAvondBot.State;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System.Threading;
using System.Threading.Tasks;

namespace kennisAvondBot.Dialogs
{
    public class UserProfileDialog : ComponentDialog
    {
        private readonly IStatePropertyAccessor<UserProfile> _userProfileAccessor;

        public UserProfileDialog(UserState userState) : base(nameof(UserProfileDialog))
        {
            _userProfileAccessor = userState.CreateProperty<UserProfile>("UserProfile");

            var waterfallSteps = new WaterfallStep[]
            {
                AskForNameStepAsync,
                NameConfirmStepAsync,
            };

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));

            AddDialog(new TextPrompt("NamePrompt"));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> AskForNameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Hallo, volgens mij hebben wij elkaar nog niet ontmoet."));
            return await stepContext.PromptAsync("NamePrompt",
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Wat is je naam?"),
                }, cancellationToken);
        }

        private async Task<DialogTurnResult> NameConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            UserProfile userProfile = await _userProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);
            userProfile.Name = (string)stepContext.Result;

            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Super, leuk je te ontmoeten {userProfile.Name}."));
            return await stepContext.EndDialogAsync();
        }
    }
}
