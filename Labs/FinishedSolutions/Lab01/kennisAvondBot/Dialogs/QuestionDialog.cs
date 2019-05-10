using kennisAvondBot.State;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System.Threading;
using System.Threading.Tasks;

namespace kennisAvondBot.Dialogs
{
    public class QuestionDialog : ComponentDialog
    {
        private readonly IStatePropertyAccessor<UserProfile> _userProfileAccessor;

        public QuestionDialog(UserState userState) : base(nameof(QuestionDialog))
        {
            _userProfileAccessor = userState.CreateProperty<UserProfile>("UserProfile");

            var waterfallSteps = new WaterfallStep[]
            {
                AskForQuestionStepAsync,
                AnswerQuestionStepAsync,
            };

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));

            AddDialog(new TextPrompt("QuestionPrompt"));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> AskForQuestionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            UserProfile userProfile = await _userProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);
            
            return await stepContext.PromptAsync("QuestionPrompt",
                new PromptOptions
                {
                    Prompt = MessageFactory.Text($"Wat is je vraag {userProfile.Name}?"),
                }, cancellationToken);
        }

        private async Task<DialogTurnResult> AnswerQuestionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string askedQuestion = (string)stepContext.Result;

            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Sorry, ik heb geen antwoord op de vraag '{askedQuestion}'."));
            return await stepContext.EndDialogAsync();
        }
    }
}
