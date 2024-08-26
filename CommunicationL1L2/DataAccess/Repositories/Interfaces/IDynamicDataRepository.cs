using SharedLibrary.Entities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repositories.Interfaces
{
    public interface IDynamicDataRepository:IRepository<DynamicData>
    {
        public int BulkInsert(IEnumerable<DynamicData> dynamicData);
    }
}
