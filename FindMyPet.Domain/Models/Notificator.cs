using System.Collections.Generic;
using System.Linq;
using FindMyPet.Business.Interfaces;

namespace FindMyPet.Business.Models
{
    public class Notificator : INotificator
    {
        private readonly List<Notification> Notifications;

        public Notificator()
        {
            Notifications = new List<Notification>();
        }

        public void Handle(Notification Notification)
        {
            Notifications.Add(Notification);
        }

        public List<Notification> GetNotifications()
        {
            return Notifications;
        }

        public bool HaveNotification()
        {
            return Notifications.Any();
        }
    }
    public class Notification
    {
        public Notification(string Message)
        {
            this.Message = Message;
        }

        public string Message { get; }
    }
}