using System.Collections.Generic;
using System.Linq;
using FindMyPet.Business.Interfaces;

namespace FindMyPet.Business.Notifications
{
    public class Notificator : INotificator
    {
        private List<Notification> _notifications;

        public Notificator()
        {
            _notifications = new List<Notification>();
        }

        public void Handle(Notification notificacao)
        {
            _notifications.Add(notificacao);
        }

        public List<Notification> getNotifications()
        {
            return _notifications;
        }

        public bool haveNotification()
        {
            return _notifications.Any();
        }
    }
}