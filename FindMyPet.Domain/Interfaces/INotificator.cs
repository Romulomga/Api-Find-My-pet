using System.Collections.Generic;
using FindMyPet.Business.Models;

namespace FindMyPet.Business.Interfaces
{
    public interface INotificator
    {
        bool HaveNotification();
        List<Notification> GetNotifications();
        void Handle(Notification notification);
    }
}