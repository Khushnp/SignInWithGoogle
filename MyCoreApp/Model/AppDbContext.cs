using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using MyCoreApp.Model;

namespace MyCoreApp.Model
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<MyCoreApp.Model.UserProfile> UserProfile { get; set; } = default!;
    }

}
