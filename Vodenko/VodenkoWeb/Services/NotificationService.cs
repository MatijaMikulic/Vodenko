using DataAccess.Entities;
using DataAccess.Repository;
using DataAccess.Repository.IRepository;
//using VodenkoWeb.Model;

namespace VodenkoWeb.Services
{
    public class NotificationService:INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;

        public NotificationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task AddNotification(Notification notification)
        {
            await _unitOfWork.NotificationRepository.AddAsync(notification);
            await _unitOfWork.SaveAsync();
        }

        public async Task<List<Notification>> GetLatestNotifications(int count)
        {
            return await _unitOfWork.NotificationRepository.GetLatestNotificationsAsync(count);
        }

        public async Task<int> GetNewNotificationCount()
        {
            return await _unitOfWork.NotificationRepository.GetNewNotificationCountAsync();
        }

        public async Task MarkAllAsRead()
        {
            await _unitOfWork.NotificationRepository.MarkAllAsReadAsync();
        }
        public async Task MarkAsRead(int id)
        {
            await _unitOfWork.NotificationRepository.MarkAsReadAsync(id);
        }

        public async Task<List<Notification>> GetPagedNotifications(int pageNumber, int pageSize)
        {
            return await _unitOfWork.NotificationRepository.GetPagedNotificationsAsync(pageNumber, pageSize);
        }
    }
}
