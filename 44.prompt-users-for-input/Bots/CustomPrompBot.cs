using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.BotBuilderSamples;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.DateTime;
using Microsoft.Recognizers.Text.Number;


namespace PromptUsersForInput.Bots
{
    public class CustomPrompBot : ActivityHandler
    {
        private readonly BotState _conversationState;
        static int AuxCoronavirus = 0;
        static int AuxAlergia = 0;
        static int AuxGripe = 0;
        static bool statusQuestion = false;
        event EventHandler ThresholdReached;
        string Question = "";


        public CustomPrompBot(ConversationState conversationState, UserState userState)
        {
            _conversationState = conversationState;
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var welcomeText = "Hello and welcome!";
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText, welcomeText), cancellationToken);
                }
            }           
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {

            var conversationStateAccessors = _conversationState.CreateProperty<ConversationFlow>(nameof(ConversationFlow));
            var flow = await conversationStateAccessors.GetAsync(turnContext, () => new ConversationFlow());
            ThresholdReached += async (sender, e) =>
            {
                await turnContext.SendActivityAsync(MessageFactory.Text(Question, Question), cancellationToken);
            };
            MakeDecisionTree().Evaluate();

            bool GetUserAnswer(string question)
            {
                Question = question;
                string userInput;
                while (true)
                {
                    userInput = turnContext.Activity.Text;
                    if (userInput == "si")
                        return true;
                    else
                    if (userInput == "no")
                        return false;
                    else
                        Console.WriteLine("Your answer is not supported, retry please." +
                                          Environment.NewLine + Environment.NewLine +
                                          question);
                }
            }
            DecisionTreeQuery MakeDecisionTree()
            {
                var queryNasalComun =
                 new DecisionTreeQuery("¿Presenta goteo nasal?",
                   new DecisionTreeResult("Posible caso de resfriado comun"),
                       new DecisionTreeResult("Consulte un medico?"), GetUserAnswer);

                var queryNasalAlergia =
                     new DecisionTreeQuery("¿Presenta goteo nasal?",
                       new DecisionTreeResult("Posible caso de alergia"),
                           new DecisionTreeResult("Consulte un medico?"), GetUserAnswer);

                var queryEstorComun =
                     new DecisionTreeQuery("¿Tiene Estornudos?",
                       queryNasalComun,
                           new DecisionTreeResult("Consulte un medico?"), GetUserAnswer);

                var queryEstorAlergia =
                      new DecisionTreeQuery("¿Tiene Estornudos?",
                        queryNasalAlergia,
                            new DecisionTreeResult("Consulte un medico?"), GetUserAnswer);

                var queryOjos =
                    new DecisionTreeQuery("¿Tiene ojos irritados?",
                       queryEstorAlergia,
                            queryEstorComun, GetUserAnswer);

                var queryGripeTox =
                     new DecisionTreeQuery("¿Presentas Tos?",
                     new DecisionTreeResult("Posible caso de gripe"),
                             new DecisionTreeResult("Ve a un Medico"), GetUserAnswer);

                var queryCoroAsistencia =
                     new DecisionTreeQuery("¿Has asistido a reunio mas de 20 personas en espacios cerrados?",
                    new DecisionTreeResult("Posible caso de coronavirus"),
                            new DecisionTreeResult("Ve a un Medico"), GetUserAnswer);

                var queryCoroTox =
                    new DecisionTreeQuery("¿Presentas Tos?",
                        queryCoroAsistencia,
                            new DecisionTreeResult("Ve a un Medico"), GetUserAnswer);

                var queryCoroFatiga =
                  new DecisionTreeQuery("¿Presenta debilida o fatiga?",
                    queryCoroTox,
                            new DecisionTreeResult("Ve a un Medico"), GetUserAnswer);

                var queryGripe =
                  new DecisionTreeQuery("¿Presenta debilida o fatiga?",
                    queryGripeTox,
                            new DecisionTreeResult("Ve a un Medico"), GetUserAnswer);

                var queryAire =
                  new DecisionTreeQuery("¿Esperimenta falta de aire?",
                      queryCoroFatiga,
                          queryGripe, GetUserAnswer);

                var queryFiebreMayor =
                    new DecisionTreeQuery("¿Presenta fiebre mayor a 38ºC?",
                                queryAire,
                                 queryOjos,
                                    GetUserAnswer);
                return queryFiebreMayor;
            }


        }
     




        private static bool ValidateAnswer(string input, out string output, out IMessageActivity message)
        {
            output = null;
            message = null;

            if (!(input.Contains("Si") || input.Contains("No") || input.Contains("Finalizar") || input.Equals("Iniciar")))
            {
                message = (ChoiceFactory.HeroCard(new List<Choice>()
                            { new Choice() { Value = "Si" }, new Choice() { Value = "No" } ,
                    new Choice() { Value = "Finalizar Consulta" } }, "Por favor seleccione unicamente las opciones que se muestran"));
            }
            else
            {
                output = input.ToUpper().Trim();
            }

            return message is null;
        }

    

        abstract public class DecisionTreeCondition
        {
            protected string Sentence { get; private set; }
            abstract public void Evaluate();
            public DecisionTreeCondition(string sentence)
            {
                Sentence = sentence;
            }
        }

        public class DecisionTreeQuery : DecisionTreeCondition
        {
            private DecisionTreeCondition Positive;
            private DecisionTreeCondition Negative;
            private Func<string, bool> UserAnswerProvider;
            public override void Evaluate()
            {
                if (UserAnswerProvider(Sentence))
                    Positive.Evaluate();
                else
                    Negative.Evaluate();
            }
            public DecisionTreeQuery(string sentence,
                                     DecisionTreeCondition positive,
                                     DecisionTreeCondition negative,
                                      Func<string, bool>  userAnswerProvider
                                     )
              : base(sentence)
            {
                Positive = positive;
                Negative = negative;
                UserAnswerProvider = userAnswerProvider;
            }
        }

        public class DecisionTreeResult : DecisionTreeCondition
        {
            public override void Evaluate()
            {
                //Console.WriteLine(Sentence);
            }
            public DecisionTreeResult(string sentence)
              : base(sentence)
            {
            }
        }


    }
}
