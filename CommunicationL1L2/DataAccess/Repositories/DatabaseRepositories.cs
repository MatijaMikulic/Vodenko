using DataAccess.Configurations;
using DataAccess.Repositories.Interfaces;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
    public class DatabaseRepositories
    {
        public DatabaseRepositories(IOptions<DBConfiguration> dapperOptions) 
        {
            MessageRepository = new MessageRepository(dapperOptions.Value);
            DynamicDataRepository = new DynamicDataRepository(dapperOptions.Value);
            AlarmRepository = new AlarmRepository(dapperOptions.Value);
            ModelParametersRepository = new ModelParametersRepository(dapperOptions.Value);
        }

        public IMessageRepository MessageRepository { get; }
        public IDynamicDataRepository DynamicDataRepository { get; }
        public IAlarmRepository AlarmRepository { get; }
        public IModelParametersRepository ModelParametersRepository { get; }


    }
}
