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
using Dapper;
using Z.Dapper.Plus;

namespace DataAccess.Repositories
{
    public class DynamicDataRepository : IDynamicDataRepository
    {
        private readonly DBConfiguration _configuration;


        public DynamicDataRepository(DBConfiguration options)
        {
            _configuration = options;
        }
        public async Task<int> AddAsync(DynamicData entity)
        {
            using (IDbConnection db = new SqlConnection(_configuration.ConnectionString))
            {
                var sql = @"
                INSERT INTO DynamicData (ValvePositionFeedback, InletFlow, WaterLevelTank1, WaterLevelTank2, 
                                         InletFlowNonLinModel, WaterLevelTank1NonLinModel, WaterLevelTank2NonLinModel, 
                                         InletFlowLinModel, WaterLevelTank1LinModel, WaterLevelTank2LinModel,
                                         OutletFlow, DateTime, IsPumpActive, Sample, Target)
                VALUES (@ValvePositionFeedback, @InletFlow, @WaterLevelTank1, @WaterLevelTank2, 
                        @InletFlowNonLinModel, @WaterLevelTank1NonLinModel, @WaterLevelTank2NonLinModel, 
                        @InletFlowLinModel, @WaterLevelTank1LinModel, @WaterLevelTank2LinModel
                        @OutletFlow,@DateTime, @IsPumpActive, @Sample, @Target);
                SELECT CAST(SCOPE_IDENTITY() as int)";

                var id = await db.ExecuteScalarAsync<int>(sql, entity);
                return id;
            }

        }

        public int BulkInsert(IEnumerable<DynamicData> dynamicData)
        {
            using (IDbConnection db = new SqlConnection(_configuration.ConnectionString))
            {
                //db.BulkInsert(dynamicData);
                db.Open();
                db.InsertBulk(dynamicData);
                db.Close();
                return dynamicData.Count();
            }
        }

        public Task<IReadOnlyList<DynamicData>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<DynamicData> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<int> RemoveAsync(DynamicData entity)
        {
            throw new NotImplementedException();
        }

        public Task<int> RemoveAtAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<int> UpdateAsync(DynamicData entity)
        {
            throw new NotImplementedException();
        }
    }
}
