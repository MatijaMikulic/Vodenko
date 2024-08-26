using DataAccess.Data;
using SharedLibrary.Entities;
using DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository
{
    public class MessageRepository : Repository<Message>, IMessageRepository
    {
        private ApplicationDbContext _db;
        public MessageRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task UpdateAsync(Message message)
        {
            var existingEntity = await _db.Alarm.FindAsync(message.Id);

            if (existingEntity == null)
            {
                throw new ArgumentException("Entity not found.");
            }

            _db.Alarm.Entry(existingEntity).CurrentValues.SetValues(message);
        }

        public override async Task<IEnumerable<Message>> GetAllAsync()
        {
            IQueryable<Message> query = _dbSet;
            query = query.Include(a => a.MessageHeader);
            return await query.ToListAsync();
        }
    }
}
