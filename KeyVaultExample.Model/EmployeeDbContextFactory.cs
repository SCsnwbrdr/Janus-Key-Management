using JanusKeyManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;

namespace KeyVaultExample.Repository
{
    public class EmployeeDbContextFactory : IDesignTimeDbContextFactory<EmployeeContext>
    {
        private IJanusKeySet _janusKeyEngine;
        private readonly string _server;
        private readonly string _database;
        private const string SqlConnectionStringTemplate = "Server={0};Initial Catalog={1};Persist Security Info=False;User ID={2};Password={3};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        public string ContextName { get; }

        public EmployeeDbContextFactory(string server, string database)
        {
            _server = server;
            _database = database;
        }

        private string SqlConnectionString(string userName, string password)
        {
            return string.Format(SqlConnectionStringTemplate, _server, _database, userName, password);
        }

        public EmployeeContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<EmployeeContext>();
            optionsBuilder.UseSqlServer(SqlConnectionString(args[0], args[1]));
            return new EmployeeContext(optionsBuilder.Options);
        }

    }
}
