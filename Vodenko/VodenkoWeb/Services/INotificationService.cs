//using VodenkoWeb.Model;
using DataAccess.Entities;

namespace VodenkoWeb.Services
{
    public interface INotificationService
    {
        Task AddNotification(Notification notification);
        Task<List<Notification>> GetLatestNotifications(int count);
        Task<int> GetNewNotificationCount();
        Task MarkAllAsRead();
        Task MarkAsRead(int id);
        Task<List<Notification>> GetPagedNotifications(int pageNumber, int pageSize);
    }
}
