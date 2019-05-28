using JanusKeyManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;

namespace KeyVaultExample.Repository
{
    public class ExampleContextFactory : IDesignTimeDbContextFactory<ExampleContext>
    {
        private IJanusKeyEngine _janusKeyEngine;
        private readonly string _server;
        private readonly string _database;
        private const string SqlConnectionStringTemplate = "Server={0};Initial Catalog={1};Persist Security Info=False;User ID={2};Password={3};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        public string ContextName { get; }

        public ExampleContextFactory(IJanusKeyEngine janusKeyEngine, string server, string database)
        {
            _server = server;
            _database = database;
            _janusKeyEngine = janusKeyEngine;
        }

        private string SqlConnectionString()
        {
            return string.Format(SqlConnectionStringTemplate, _server, _database, _janusKeyEngine.ActiveCredential.Identifier, _janusKeyEngine.ActiveCredential.Token);
        }

        public ExampleContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ExampleContext>();
            optionsBuilder.UseSqlServer(SqlConnectionString());
            return new ExampleContext(optionsBuilder.Options);
        }

    }
}
