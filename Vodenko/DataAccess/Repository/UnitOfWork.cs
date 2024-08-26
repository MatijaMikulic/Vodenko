using DataAccess.Data;
using DataAccess.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private ApplicationDbContext _db;

        public IAlarmRepository AlarmRepository { get; private set; }
        public IMessageRepository MessageRepository { get; private set; }
        public IAuxValuesRepository AuxValuesRepository { get; private set; }
        public INotificationRepository NotificationRepository { get; private set; }

        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;
            AlarmRepository = new AlarmRepository(db);
            MessageRepository = new MessageRepository(db);
            AuxValuesRepository = new AuxValuesRepository(db);
            NotificationRepository = new NotificationRepository(db);
        }

        public Task<int> SaveAsync()
        {
            return _db.SaveChangesAsync();
        }
    }
}
