using SharedLibrary.Entities;

using DataAccess.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Threading.Tasks;
using DataAccess.Configurations;
using Dapper;

namespace DataAccess.Repositories
{
    public class ModelParametersRepository : IModelParametersRepository
    {
        private readonly DBConfiguration _configuration;

        public ModelParametersRepository(DBConfiguration options)
        {
            _configuration = options;
        }

        public async Task<int> AddAsync(ModelParameters entity)
        {
            using (IDbConnection db = new SqlConnection(_configuration.ConnectionString))
            {
                var sql = @"
                INSERT INTO ModelParameters (Theta1, Theta2, Theta3, Theta4, Theta5, Theta6, Theta7, dateTime)
                VALUES (@Theta1, @Theta2, @Theta3, @Theta4, @Theta5, @Theta6, @Theta7, @dateTime);
                SELECT CAST(SCOPE_IDENTITY() as int)";

                var id = await db.ExecuteScalarAsync<int>(sql, new
                {
                    entity.Theta1,
                    entity.Theta2,
                    entity.Theta3,
                    entity.Theta4,
                    entity.Theta5,
                    entity.Theta6,
                    entity.Theta7,
                    entity.dateTime
                });
                return id;
            }
        }

        public async Task<IReadOnlyList<ModelParameters>> GetAllAsync()
        {
            using (IDbConnection db = new SqlConnection(_configuration.ConnectionString))
            {
                var sql = "SELECT * FROM ModelParameters";
                var result = await db.QueryAsync<ModelParameters>(sql);
                return result.AsList();
            }
        }

        public async Task<ModelParameters> GetByIdAsync(int id)
        {
            using (IDbConnection db = new SqlConnection(_configuration.ConnectionString))
            {
                var sql = "SELECT * FROM ModelParameters WHERE Id = @Id";
                return await db.QueryFirstOrDefaultAsync<ModelParameters>(sql, new { Id = id });
            }
        }

        public async Task<int> RemoveAsync(ModelParameters entity)
        {
            using (IDbConnection db = new SqlConnection(_configuration.ConnectionString))
            {
                var sql = "DELETE FROM ModelParameters WHERE Id = @Id";
                return await db.ExecuteAsync(sql, new { Id = entity.Id });
            }
        }

        public async Task<int> RemoveAtAsync(int id)
        {
            using (IDbConnection db = new SqlConnection(_configuration.ConnectionString))
            {
                var sql = "DELETE FROM ModelParameters WHERE Id = @Id";
                return await db.ExecuteAsync(sql, new { Id = id });
            }
        }

        public async Task<int> UpdateAsync(ModelParameters entity)
        {
            using (IDbConnection db = new SqlConnection(_configuration.ConnectionString))
            {
                var sql = @"
                UPDATE ModelParameters
                SET 
                    Theta1 = @Theta1,
                    Theta2 = @Theta2,
                    Theta3 = @Theta3,
                    Theta4 = @Theta4,
                    Theta5 = @Theta5,
                    Theta6 = @Theta6,
                    Theta7 = @Theta7,
                    dateTime = @dateTime
                WHERE Id = @Id";

                return await db.ExecuteAsync(sql, new
                {
                    entity.Theta1,
                    entity.Theta2,
                    entity.Theta3,
                    entity.Theta4,
                    entity.Theta5,
                    entity.Theta6,
                    entity.Theta7,
                    entity.dateTime,
                    entity.Id
                });
            }
        }
    }
}
