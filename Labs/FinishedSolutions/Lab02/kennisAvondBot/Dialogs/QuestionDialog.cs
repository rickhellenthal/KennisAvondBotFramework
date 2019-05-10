using kennisAvondBot.State;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace kennisAvondBot.Dialogs
{
    public class QuestionDialog : ComponentDialog
    {
        private readonly IStatePropertyAccessor<UserProfile> _userProfileAccessor;
        private readonly QnAMaker _kennisAvondBotQnA;

        public QuestionDialog(UserState userState, QnAMakerEndpoint endpoint) : base(nameof(QuestionDialog))
        {
            _userProfileAccessor = userState.CreateProperty<UserProfile>("UserProfile");
            _kennisAvondBotQnA = new QnAMaker(endpoint);

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
            var results = await _kennisAvondBotQnA.GetAnswersAsync(stepContext.Context);
            if (results.Any())
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(results.First().Answer), cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Sorry, ik heb geen antwoord op de vraag '{stepContext.Result}' kunnen vinden."), cancellationToken);
            }

            return await stepContext.EndDialogAsync();
        }
    }
}
