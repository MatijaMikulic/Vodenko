using DataAccess.Entities;
using Microsoft.AspNetCore.SignalR;
//using VodenkoWeb.Model;
using VodenkoWeb.Services;

namespace VodenkoWeb.Hubs
{
    public class NotificationHub : Hub
    {
        public const string Url = "/notificationHub";

        private readonly INotificationService _notificationService;

        public NotificationHub(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public async Task SendNotification(string message)
        {
            var notification = new Notification
            {
                Message = message,
                Timestamp = DateTime.Now
            };

            await _notificationService.AddNotification(notification);
            await Clients.All.SendAsync("ReceiveNotification", notification);
        }
    }
}
