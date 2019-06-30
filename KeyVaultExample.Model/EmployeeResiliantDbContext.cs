using System;
using System.Collections.Generic;
using System.Text;
using JanusKeyManagement;
using Microsoft.EntityFrameworkCore.Design;

namespace KeyVaultExample.Repository
{
    public class EmployeeResiliantDbContext<T> : JanusResiliantDbContext<EmployeeContext>
    {
        public EmployeeResiliantDbContext(IDesignTimeDbContextFactory<EmployeeContext> DbContextFactory, IJanusKeySet keyEngine) : base(DbContextFactory, keyEngine)
        {
        }
    }
}
