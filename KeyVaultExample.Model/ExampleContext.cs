using JanusKeyManagement;
using KeyVaultExample.Repository.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace KeyVaultExample.Repository
{
    public class ExampleContext : DbContext
    {
        DbSet<Employee> Employees { get; set; }

        public ExampleContext(DbContextOptions options) : base(options)
        {
        }
    }
}
