using JanusKeyManagement;
using KeyVaultExample.Repository.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace KeyVaultExample.Repository
{
    public class EmployeeContext : DbContext
    {
        DbSet<Employee> Employees { get; set; }

        public EmployeeContext(DbContextOptions options) : base(options)
        {
        }
    }
}
