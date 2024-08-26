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
    public class AlarmRepository : Repository<Alarm>, IAlarmRepository
    {
        private ApplicationDbContext _db;
        public AlarmRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public override async Task<IEnumerable<Alarm>> GetAllAsync()
        {
            IQueryable<Alarm> query = _dbSet;
            // Include AlarmCode for Alarm entity
            query = query.Include(a => a.AlarmCode);
            return await query.ToListAsync();
        }
        public async Task UpdateAsync(Alarm alarm)
        {
            var existingEntity = await _db.Alarm.FindAsync(alarm.Id);

            if (existingEntity == null)
            {
                throw new ArgumentException("Entity not found.");
            }

            _db.Alarm.Entry(existingEntity).CurrentValues.SetValues(alarm);
        }
    }
}
