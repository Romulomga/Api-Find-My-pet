using System.Collections.Generic;
using FindMyPet.Business.Notifications;

namespace FindMyPet.Business.Interfaces
{
    public interface INotificator
    {
        bool haveNotification();
        List<Notification> getNotifications();
        void Handle(Notification notification);
    }
}