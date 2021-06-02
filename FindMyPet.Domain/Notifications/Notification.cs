namespace FindMyPet.Business.Notifications
{
    public class Notification
    {
        public Notification(string Message)
        {
            this.Message = Message;
        }

        public string Message { get; }
    }
}