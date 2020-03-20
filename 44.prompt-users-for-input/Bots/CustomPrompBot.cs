using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.BotBuilderSamples;

namespace PromptUsersForInput.Bots
{
    public class CustomPrompBot : ActivityHandler
    {

        private readonly BotState _conversationState;
        static DecisionTreeQuery TreeDecision;
        static byte Counter = 0;
        static List<byte> Answers = new List<byte>();
        List<String> Question = new List<String>()
        {
            "1.¿Presentas fiebre mayor a 38 grados?",
            "2.¿Tienes dolor de cabeza?",
            "3.¿Presentas tos seca?",
            "4.¿Tienes dolor de garganta?",
            "5.¿Tienes dolor de músculos y articulaciones?",
            "6.¿Tienes falta de aire (ansia)?",
            "7.¿Presentas debilidad o fatiga?",
            "8.¿Tienes estornudos?",
            "9.¿Presentas escurrimiento nasal?",
            "10.¿Tienes ojos irritados?",
            "11.¿Tienes congestión nasal?",
            "12.¿Tienes vómito o diarrea?",
            "13.¿Has estado en contacto con alguien infectado de COVID-19?"
        };


        public CustomPrompBot(ConversationState conversationState)
        {
            _conversationState = conversationState;
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var welcomeText = "Hello and welcome!";
            var Three = MakeDecisionTree();
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText, welcomeText), cancellationToken);
                    await turnContext.SendActivityAsync(MessageFactory.Text($"{Question[0]}", $"{Question[0]}"), cancellationToken);
                }
            }
            TreeDecision = Three;
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {

            //var conversationStateAccessors = _conversationState.CreateProperty<ConversationFlow>(nameof(ConversationFlow));
            await _conversationState.SaveChangesAsync(turnContext);
            string input = turnContext.Activity.Text?.Trim();
                switch (input.ToUpper())
                {
                    case "SI":
                             var Aux = Counter + 1;
                             Answers.Add(Convert.ToByte(Aux));
                             Counter++;
                            if (Counter == Question.Count)
                            {
                                await turnContext.SendActivityAsync($"Se Acabaron las preguntas");
                                var Awn = Answers;
                                HelloWorld();
                                  Answers.Clear();
                                 Counter = 0;
                                await turnContext.SendActivityAsync($"Presione f para inicar de nuevo");
                             }
                            else
                            {
                              await turnContext.SendActivityAsync($"{Question[Counter]}");
                            }       
                    break;
                    case "NO":
                              Counter++;
                             if (Counter == Question.Count)
                             {
                              await turnContext.SendActivityAsync($"Se Acabaron las preguntas");
                              var Awn = Answers;
                               HelloWorld();
                               Answers.Clear();
                                Counter = 0;
                               await turnContext.SendActivityAsync($"Presione f para inicar de nuevo");
                            }
                              else
                             {
                               await turnContext.SendActivityAsync($"{Question[Counter]}");
                             }
                    break;
                    case "F":
                        Counter = 0;
                        Answers.Clear();
                        await turnContext.SendActivityAsync("Comenzemos de nuevo");
                        await turnContext.SendActivityAsync($"{Question[Counter]}");

                        break;
                    default:
                        await turnContext.SendActivityAsync("Por favor seleccione unicamente las opciones 'si' , 'no' o 'f' para finalizar");
                        break;
                }
            
        }

        void HelloWorld()
        {

        }

        public DecisionTreeQuery MakeDecisionTree()
        {
            var queryNasalComun =
             new DecisionTreeQuery("¿Presenta goteo nasal?",
              new DecisionTreeQuery("Posible caso de resfriado comun", null, null),
                   new DecisionTreeQuery("Consulte a un Medico", null, null));

            var queryNasalAlergia =
                 new DecisionTreeQuery("¿Presenta goteo nasal?",
                   new DecisionTreeQuery("Posible caso de Alergia", null, null),
                       new DecisionTreeQuery("Consulte a un Medico", null, null));

            var queryEstorComun =
                 new DecisionTreeQuery("¿Tiene Estornudos?",
                   queryNasalComun,
                       new DecisionTreeQuery("Consulte a un Medico", null, null));

            var queryEstorAlergia =
                  new DecisionTreeQuery("¿Tiene Estornudos?",
                    queryNasalAlergia,
                        new DecisionTreeQuery("Consulte a un Medico",null,null));

            var queryOjos =
                new DecisionTreeQuery("¿Tiene ojos irritados?",
                   queryEstorAlergia,
                        queryEstorComun);

            var queryGripeTox =
                 new DecisionTreeQuery("¿Presentas Tos?",
                 new DecisionTreeQuery("Posible Caso de Gripe", null, null),
                        new DecisionTreeQuery("Consulte a un Medico", null, null));

            var queryCoroAsistencia =
                 new DecisionTreeQuery("¿Has asistido a reunio mas de 20 personas en espacios cerrados?",
                    new DecisionTreeQuery("Posible Caso de Coronavirus", null, null),
                        new DecisionTreeQuery("Consulte a un Medico", null, null));

            var queryCoroTox =
                new DecisionTreeQuery("¿Presentas Tos?",
                    queryCoroAsistencia,
                        new DecisionTreeQuery("Consulte a un Medico", null, null));

            var queryCoroFatiga =
              new DecisionTreeQuery("¿Presenta debilida o fatiga?",
                queryCoroTox,
                       new DecisionTreeQuery("Consulte a un Medico", null, null));

            var queryGripe =
              new DecisionTreeQuery("¿Presenta debilida o fatiga?",
                queryGripeTox,
                       new DecisionTreeQuery("Consulte a un Medico", null, null));

            var queryAire =
              new DecisionTreeQuery("¿Esperimenta falta de aire?",
                  queryCoroFatiga,
                      queryGripe);

            var queryFiebreMayor =
                new DecisionTreeQuery("¿Presenta fiebre mayor a 38ºC?",
                            queryAire,
                             queryOjos);
            return queryFiebreMayor;
        }

        abstract public class DecisionTreeCondition
        {
            protected string Sentence { get; private set; }
            public DecisionTreeCondition(string sentence)
            {
                Sentence = sentence;
            }
        }

        public class DecisionTreeQuery : DecisionTreeCondition
        {
            public string Question;
            public DecisionTreeCondition Positive;
            public DecisionTreeCondition Negative;

            public DecisionTreeQuery(string sentence,
                                     DecisionTreeCondition positive,
                                     DecisionTreeCondition negative)
                 : base(sentence)
            {
                Question = sentence;
                Positive = positive;
                Negative = negative;
            }

        }
    }
}
