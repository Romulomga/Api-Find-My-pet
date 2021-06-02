﻿using System.Collections.Generic;
using System.Linq;
using FindMyPet.Business.Interfaces;

namespace FindMyPet.Business.Notifications
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
}