using System.Collections.Generic;
using System.Linq;
using FindMyPet.Business.Interfaces;

namespace FindMyPet.Business.Notifications
{
    public class Notificator : INotificator
    {
        private List<Notification> Notifications;

        public Notificator()
        {
            Notifications = new List<Notification>();
        }

        public void Handle(Notification notification)
        {
            Notifications.Add(notification);
        }

        public List<Notification> getNotifications()
        {
            return Notifications;
        }

        public bool haveNotification()
        {
            return Notifications.Any();
        }
    }
}