using AdotaFacil.Business.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace AdotaFacil.Business.Models
{
    public class Notificator : INotificator
    {
        private readonly List<Notification> Notifications;

        public Notificator()
        {
            Notifications = new List<Notification>();
        }

        public void Handle(Notification notification)
        {
            Notifications.Add(notification);
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
        public Notification(string message)
        {
            this.Message = message;
        }

        public string Message { get; }
    }
}