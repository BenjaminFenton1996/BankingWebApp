﻿using Microsoft.EntityFrameworkCore;
using BankingApp.Entities.Models;

namespace BankingApp.Entities
{
    public class BankingAppDbContext : DbContext
    {
        public BankingAppDbContext(DbContextOptions<BankingAppDbContext> options)
            : base(options)
        { }

        public DbSet<User> Users { get; set; }
        public DbSet<BankAccount> BankAccounts { get; set; }
        public DbSet<BankTransferLog> BankTransferLogs { get; set; }
    }
}
