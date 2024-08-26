using SharedLibrary.Entities;

using DataAccess.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess.Configurations;
using Z.Dapper.Plus;
using Dapper;

namespace DataAccess.Repositories
{
    public class AlarmRepository : IAlarmRepository
    {
        private readonly DBConfiguration _configuration;

        public AlarmRepository(DBConfiguration options)
        {
            _configuration = options;
        }
        public async Task<int> AddAsync(Alarm entity)
        {
            using (IDbConnection db = new SqlConnection(_configuration.ConnectionString))
            {
                var sql = @"
                INSERT INTO Alarm (DateTime, AlarmCodeId, Comment)
                VALUES (@DateTime, @AlarmCodeId, @Comment);
                SELECT CAST(SCOPE_IDENTITY() as int)";

                var id = await db.ExecuteScalarAsync<int>(sql, new
                {
                    entity.DateTime,
                    entity.AlarmCode.Code,
                    entity.Comment
                });
                return id;
            }      
        }

        public Task<IReadOnlyList<Alarm>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Alarm> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<int> RemoveAsync(Alarm entity)
        {
            throw new NotImplementedException();
        }

        public Task<int> RemoveAtAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<int> UpdateAsync(Alarm entity)
        {
            throw new NotImplementedException();
        }
    }
}
