using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JanusKeyManagement;
using KeyVaultExample.Repository;
using KeyVaultExample.Repository.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace KeyVaultExample.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DbController : ControllerBase
    {
        private EmployeeResiliantDbContext<EmployeeContext> context { get; set; }

        public DbController(EmployeeResiliantDbContext<EmployeeContext> resiliantContext)
        {
            context = resiliantContext;
        }

        [HttpGet("Add")]
        public string Add()
        {
            context.Add(new Employee()
            {
                EmployeeId = DateTime.Now.Second,
                Name = DateTime.Now.Month.ToString() + " Jonathan"
            });
                return "Got It!";
        }
    }
}