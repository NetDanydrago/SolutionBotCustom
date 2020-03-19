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


        public CustomPrompBot(ConversationState conversationState, UserState userState)
        {
            _conversationState = conversationState;
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {

            var conversationStateAccessors = _conversationState.CreateProperty<ConversationFlow>(nameof(ConversationFlow));
            var flow = await conversationStateAccessors.GetAsync(turnContext, () => new ConversationFlow());

            await FillOutUserProfileAsync(flow, turnContext);

            // Save changes.
            await _conversationState.SaveChangesAsync(turnContext);
        }

        private static async Task FillOutUserProfileAsync(ConversationFlow flow, ITurnContext turnContext)
        {

            string input = turnContext.Activity.Text?.Trim();
            IMessageActivity message;
            List<string> QuestionCoronavirus = new List<string>()
            { "Presenta fiebre mayor a 38 grados?", "Experimenta falta de aire?"
            ,"Presenta debilidad o fatiga", "Presenta tos?",  "Has asistido a reunio de mas de 20 personas en espacios cerrados?" };
            List<string> QuestionAlergia = new List<string>() { "¿Tienes Estornudos", "Presenta goteo nasal?" };
            List<string> QuestionGripe = new List<string>() { "Presenta tos?" };
            if (!statusQuestion)
            {
                input = "Si";
                statusQuestion = true;
            }
            if (ValidateAnswer(input, out string output, out message))
            {
                if (input.Equals("Finalizar Consulta"))
                {
                    AuxGripe = 0;
                    AuxCoronavirus = 0;
                    AuxAlergia = 0;
                    flow.LastQuestionAsked = ConversationFlow.Question.None;
                }

                switch (flow.LastQuestionAsked)
                {
                    case ConversationFlow.Question.None:
                        await turnContext.SendActivityAsync("Hola Bienvenido");
                        await turnContext.SendActivityAsync(ChoiceFactory.HeroCard(new List<Choice>()
                        { new Choice() { Value = "Si" }, new Choice() { Value = "No" } }, QuestionCoronavirus[0]));
                        AuxCoronavirus++;
                        flow.LastQuestionAsked = ConversationFlow.Question.SelectOption;
                        break;

                    case ConversationFlow.Question.SelectOption:
                        if (input.Contains("S"))
                        {
                            await turnContext.SendActivityAsync(ChoiceFactory.HeroCard(new List<Choice>()
                            { new Choice() { Value = "Si" }, new Choice() { Value = "No" },new Choice() { Value = "Finalizar Consulta" } }, QuestionCoronavirus[AuxCoronavirus]));
                            AuxCoronavirus++;
                            flow.LastQuestionAsked = ConversationFlow.Question.Coronavirus;
                        }
                        else
                        {
                            AuxCoronavirus = 0;
                            await turnContext.SendActivityAsync(ChoiceFactory.HeroCard(new List<Choice>()
                                 { new Choice() { Value = "Si" }, new Choice() { Value = "No" }, new Choice() { Value = "Finalizar Consulta" } }, "Tiene Ojos irritados"));
                            flow.LastQuestionAsked = ConversationFlow.Question.Alergia;
                        }
                        break;

                    case ConversationFlow.Question.Coronavirus:
                        if (input.Contains("S"))
                        {
                            if (AuxCoronavirus == QuestionCoronavirus.Count)
                            {
                                await turnContext.SendActivityAsync("Posible Caso de Coronavirus");
                                AuxCoronavirus = 0;
                                flow.LastQuestionAsked = ConversationFlow.Question.Finish;
                            }
                            else
                            {
                                await turnContext.SendActivityAsync(ChoiceFactory.HeroCard(new List<Choice>()
                                    { new Choice() { Value = "Si" }, new Choice() { Value = "No" } ,new Choice() { Value = "Finalizar Consulta" } }, QuestionCoronavirus[AuxCoronavirus]));
                                AuxCoronavirus++;
                                flow.LastQuestionAsked = ConversationFlow.Question.Coronavirus;
                            }
                        }
                        else
                        {
                            if (AuxCoronavirus == QuestionCoronavirus.Count)
                            {
                                await turnContext.SendActivityAsync("Consulte a un Medico");
                                AuxCoronavirus = 0;
                                flow.LastQuestionAsked = ConversationFlow.Question.Finish;
                            }
                            else
                            {
                                await turnContext.SendActivityAsync(ChoiceFactory.HeroCard(new List<Choice>()
                                    { new Choice() { Value = "Si" }, new Choice() { Value = "No" } , new Choice() { Value = "Finalizar Consulta" }}, QuestionCoronavirus[AuxCoronavirus]));
                                AuxCoronavirus++;
                                flow.LastQuestionAsked = ConversationFlow.Question.Gripe;
                            }
                        }
                        break;

                    case ConversationFlow.Question.ResfriadoComun:
                        if (input.Contains("S"))
                        {
                            if (AuxAlergia == QuestionAlergia.Count)
                            {
                                await turnContext.SendActivityAsync("Posible Caso de Resfriado Comun");
                                AuxAlergia = 0;
                                flow.LastQuestionAsked = ConversationFlow.Question.Finish;
                            }
                            else
                            {
                                await turnContext.SendActivityAsync(ChoiceFactory.HeroCard(new List<Choice>()
                             { new Choice() { Value = "Si" }, new Choice() { Value = "No" },new Choice() { Value = "Finalizar Consulta" }}, QuestionAlergia[AuxAlergia]));
                                AuxAlergia++;
                                flow.LastQuestionAsked = ConversationFlow.Question.ResfriadoComun;
                            }
                        }
                        else
                        {
                            if (AuxAlergia == QuestionAlergia.Count)
                            {
                                await turnContext.SendActivityAsync("Consulte a un medico");
                                AuxAlergia = 0;
                                flow.LastQuestionAsked = ConversationFlow.Question.Finish;
                            }
                            else
                            {
                                await turnContext.SendActivityAsync(ChoiceFactory.HeroCard(new List<Choice>()
                                    { new Choice() { Value = "Si" }, new Choice() { Value = "No" },new Choice() { Value = "Finalizar Consulta" }}, QuestionAlergia[AuxAlergia]));
                                AuxAlergia++;
                                flow.LastQuestionAsked = ConversationFlow.Question.ResfriadoComun;
                            }
                        }
                        break;

                    case ConversationFlow.Question.Gripe:
                        if (input.Contains("S"))
                        {
                            if (AuxGripe == QuestionGripe.Count)
                            {
                                await turnContext.SendActivityAsync("Posible Caso de gripe");
                                AuxGripe = 0;
                                flow.LastQuestionAsked = ConversationFlow.Question.Finish;
                            }
                            else
                            {
                                await turnContext.SendActivityAsync(ChoiceFactory.HeroCard(new List<Choice>()
                                  { new Choice() { Value = "Si" }, new Choice() { Value = "No" } , new Choice() { Value = "Finalizar Consulta" }}, QuestionGripe[AuxGripe]));
                                AuxGripe++;
                                flow.LastQuestionAsked = ConversationFlow.Question.Gripe;
                            }
                        }
                        else
                        {
                            if (AuxGripe == QuestionGripe.Count)
                            {
                                await turnContext.SendActivityAsync("Consulte a un Medico");
                                AuxGripe = 0;
                                flow.LastQuestionAsked = ConversationFlow.Question.Finish;
                            }
                            else
                            {
                                await turnContext.SendActivityAsync(ChoiceFactory.HeroCard(new List<Choice>()
                                  { new Choice() { Value = "Si" }, new Choice() { Value = "No" }, new Choice() { Value = "Finalizar Consulta" } }, QuestionGripe[AuxGripe]));
                                AuxGripe++;
                                flow.LastQuestionAsked = ConversationFlow.Question.Gripe;
                            }
                        }
                        break;

                    case ConversationFlow.Question.Alergia:
                        if (input.Contains("S"))
                        {
                            if (AuxAlergia == QuestionAlergia.Count)
                            {
                                await turnContext.SendActivityAsync("Posible Caso de Alergia");
                                AuxAlergia = 0;
                                flow.LastQuestionAsked = ConversationFlow.Question.Finish;
                            }
                            else
                            {
                                await turnContext.SendActivityAsync(ChoiceFactory.HeroCard(new List<Choice>()
                                 { new Choice() { Value = "Si" }, new Choice() { Value = "No" } , new Choice() { Value = "Finalizar Consulta" } }, QuestionAlergia[AuxAlergia]));
                                AuxAlergia++;
                                flow.LastQuestionAsked = ConversationFlow.Question.Alergia;
                            }
                        }
                        else
                        {
                            if (AuxAlergia == QuestionAlergia.Count)
                            {
                                await turnContext.SendActivityAsync("Consulte a un Medico");
                                AuxAlergia = 0;
                                flow.LastQuestionAsked = ConversationFlow.Question.Finish;
                            }
                            {
                                await turnContext.SendActivityAsync(ChoiceFactory.HeroCard(new List<Choice>()
                                     { new Choice() { Value = "Si" }, new Choice() { Value = "No" }, new Choice() { Value = "Finalizar Consulta" } }, QuestionAlergia[AuxAlergia]));
                                AuxAlergia++;
                                flow.LastQuestionAsked = ConversationFlow.Question.ResfriadoComun;
                            }
                        }
                        break;

                    case ConversationFlow.Question.Finish:
                        AuxGripe = 0;
                        AuxCoronavirus = 0;
                        AuxAlergia = 0;
                        await turnContext.SendActivityAsync(ChoiceFactory.HeroCard(new List<Choice>()
                             { new Choice() { Value = "Finalizar Consulta" } }, ""));
                        flow.LastQuestionAsked = ConversationFlow.Question.None;
                        break;

                }
            }
            else
            {
                await turnContext.SendActivityAsync(message);
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

    }
}
