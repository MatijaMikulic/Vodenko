using DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository.IRepository
{
    public interface INotificationRepository:IRepository<Notification>
    {
        Task UpdateAsync(Notification notification);

        Task<List<Notification>> GetLatestNotificationsAsync(int count);

        Task<int> GetNewNotificationCountAsync();

        Task MarkAllAsReadAsync();

        Task MarkAsReadAsync(int id);

        Task<List<Notification>> GetPagedNotificationsAsync(int pageNumber, int pageSize);
    }
}
