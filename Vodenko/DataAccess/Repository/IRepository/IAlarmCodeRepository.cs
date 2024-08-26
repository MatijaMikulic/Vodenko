using SharedLibrary.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository.IRepository
{
    public interface IAlarmCodeRepository : IRepository<AlarmCode>
    {
        Task UpdateAsync(AlarmCode code);
    }
}
