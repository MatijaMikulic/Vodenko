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
    public class AlarmCodeRepository : Repository<AlarmCode>, IAlarmCodeRepository
    {
        private ApplicationDbContext _db;
        public AlarmCodeRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

      
        public async Task UpdateAsync(AlarmCode code)
        {
            var existingEntity = await _db.Alarm.FindAsync(code.Code);

            if (existingEntity == null)
            {
                throw new ArgumentException("Entity not found.");
            }

            _db.Alarm.Entry(existingEntity).CurrentValues.SetValues(code);
        }
    }

}
