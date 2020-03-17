using DocShareApp.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DocShareApp.Helpers
{
    //DbContext represents a database connection and my tables
    public class DataContext : DbContext
    {
        protected readonly IConfiguration Configuration;

        public DataContext(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            //connect to sql server database
            options.UseSqlServer(Configuration.GetConnectionString("DataBaseString"));
        }

        //This is a mandatory class representing an entity set that can be used 
        //for CRUD operations (a table in my database)
        //For example, without the DbSet I will not be able to 
        //query the database for a user.
        public DbSet<User> Users { get; set; }
    }
}
