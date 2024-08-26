using DataAccess.Data;
using SharedLibrary.Entities;
using DataAccess.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository
{
    public class AuxValuesRepository : Repository<AuxTable>, IAuxValuesRepository
    {
        private ApplicationDbContext _db;
        public AuxValuesRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task UpdateAsync(AuxTable alarm)
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
