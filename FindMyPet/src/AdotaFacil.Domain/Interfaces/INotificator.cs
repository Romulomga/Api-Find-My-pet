using AdotaFacil.Business.Models;
using System.Collections.Generic;

namespace AdotaFacil.Business.Interfaces
{
    public interface INotificator
    {
        bool HaveNotification();
        List<Notification> GetNotifications();
        void Handle(Notification notification);
    }
}