namespace GameplayIngredients.Actions
{
    public class SendMessageAction : ActionBase
    {
        public string MessageToSend = "Message";

        public override void Execute()
        {
            Messager.Send(MessageToSend);
        }
    }
}


