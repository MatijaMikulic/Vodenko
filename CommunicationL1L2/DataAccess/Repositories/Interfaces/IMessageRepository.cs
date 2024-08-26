using SharedLibrary.Entities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repositories.Interfaces
{
    public interface IMessageRepository : IRepository<Message>
    {
        public Task<IReadOnlyList<Message>> GetNewMessages(int status);
    }
}
