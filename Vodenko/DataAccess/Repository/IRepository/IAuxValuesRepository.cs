using SharedLibrary.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository.IRepository
{
    public interface IAuxValuesRepository:IRepository<AuxTable>
    {
        Task UpdateAsync(AuxTable alarm);

    }
}
