using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppTest.Models;

namespace AppTest.Models
{
    public class UserContext : DbContext
    {
        public UserContext(DbContextOptions<UserContext> options)
            : base(options)
        {
        }

        public DbSet<UserInfo> UserInfo{ get; set; }
        public DbSet<GroupInfo> GroupInfo { get; set; }
        public DbSet<WalletInfo> WalletInfo { get; set; }
        public DbSet<Group_User> Group_User { get; set; }
        public DbSet<Competition_User> Competition_User { get; set; }
        public DbSet<Competition_Quarter> Competition_Quarter { get; set; }
        public DbSet<CompetitionInfo> CompetitionInfo { get; set; }
    }
}
