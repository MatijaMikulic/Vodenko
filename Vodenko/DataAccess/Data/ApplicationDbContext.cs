using Microsoft.EntityFrameworkCore;
using SharedLibrary.Entities;
using DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Data
{
    public class ApplicationDbContext:DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext>options):base(options)
        {  
        }
        public DbSet<AuxTable> Aux_table { get; set; }
        public DbSet<User> User { get; set; }
        public DbSet<AlarmCode> Alarm_Code { get; set; }
        public DbSet<MessageHeader> Message_Header{ get; set; }
        public DbSet<Alarm> Alarm { get; set; }
        public DbSet<Message> Message { get; set; }
        public DbSet<DynamicData> DynamicData { get; set; }
        public DbSet<Notification> Notification { get; set; }
        public DbSet<ModelParameters> ModelParameters { get; set; }

    }
}
