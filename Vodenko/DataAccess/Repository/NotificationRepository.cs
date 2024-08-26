using DataAccess.Data;
using DataAccess.Entities;
using DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository
{
    public class NotificationRepository:Repository<Notification>,INotificationRepository
    {

        private ApplicationDbContext _db;
        public NotificationRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task UpdateAsync(Notification notification)
        {
            var existingEntity = await _db.Alarm.FindAsync(notification.Id);

            if (existingEntity == null)
            {
                throw new ArgumentException("Entity not found.");
            }

            _db.Alarm.Entry(existingEntity).CurrentValues.SetValues(notification);
        }

        public async Task<List<Notification>> GetLatestNotificationsAsync(int count)
        {
            return await _dbSet
                .OrderByDescending(n => n.Timestamp)
                .Take(count)
                .ToListAsync();
        }

        public async Task<int> GetNewNotificationCountAsync()
        {
            return await _db.Notification.CountAsync(n => !n.IsRead);
        }

        public async Task MarkAllAsReadAsync()
        {
            var unreadNotifications = await _db.Notification.Where(n => !n.IsRead).ToListAsync();
            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
            }
            await _db.SaveChangesAsync();
        }

        public async Task MarkAsReadAsync(int id)
        {
            var notification = await _db.Notification.FindAsync(id);
            if (notification != null)
            {
                notification.IsRead = true;
                await _db.SaveChangesAsync();
            }
        }

        public async Task<List<Notification>> GetPagedNotificationsAsync(int pageNumber, int pageSize)
        {
            return await _dbSet
                .OrderByDescending(n => n.Timestamp)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

    }
}
