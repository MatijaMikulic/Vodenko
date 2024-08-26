using Dapper;
using DataAccess.Configurations;
using SharedLibrary.Entities;

using DataAccess.Repositories.Interfaces;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DapperPlus;
using Z.Dapper.Plus;

namespace DataAccess.Repositories
{
    public class MessageRepository : IMessageRepository
    {
        private readonly DBConfiguration _configuration;

        public MessageRepository(DBConfiguration options)
        {
            _configuration = options;
        }

        public async Task<int> AddAsync(Message entity)
        {
            using (IDbConnection db = new SqlConnection(_configuration.ConnectionString))
            {
                const string sql = @"
            INSERT INTO Message 
            (MessageId, Status, EnqueueDT,DequeueDT,Payload, RetryCount, ErrorLog) 
            VALUES 
            (@MessageId, @Status, @EnqueueDT,@DequeueDT ,@Payload, @RetryCount, @ErrorLog);
            SELECT CAST(SCOPE_IDENTITY() as int);";  // Return the newly generated Id

                // Execute the insert query and return the new record's Id
                return await db.QuerySingleAsync<int>(sql, new
                {
                    entity.MessageId,
                    entity.Status,
                    entity.EnqueueDT,
                    entity.DequeueDT,
                    entity.Payload,
                    entity.RetryCount,
                    entity.ErrorLog
                });
            }
        }

        public Task<IReadOnlyList<Message>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Message> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<IReadOnlyList<Message>> GetNewMessages(int status)
        {
            using (IDbConnection db = new SqlConnection(_configuration.ConnectionString))
            {
                const string sql = "SELECT * FROM Message WHERE Status = 0";
                return (await db.QueryAsync<Message>(sql)).AsList();
            }
        }

        public Task<int> RemoveAsync(Message entity)
        {
            throw new NotImplementedException();
        }

        public Task<int> RemoveAtAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<int> UpdateAsync(Message entity)
        {
            using (IDbConnection db = new SqlConnection(_configuration.ConnectionString))
            {
                const string sql = "UPDATE Message SET Status = @Status, DequeueDT=@DequeueDT WHERE Id = @Id";
                return await db.ExecuteAsync(sql, entity);
            }
        }
    }

}
