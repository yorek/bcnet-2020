using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Data;
using Microsoft.Data.SqlClient;
using Dapper;

namespace Todo.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TodoController : ControllerBase
    {
        private readonly ILogger<TodoController> _logger;
        private readonly IConfiguration _config;

        public TodoController(IConfiguration config, ILogger<TodoController> logger)
        {
            _config = config;
            _logger = logger;
        }

        [HttpGet]
        [Route("{id?}")]
        public async Task<JToken> Get(int? id)
        {
            using(var conn = new SqlConnection(_config.GetConnectionString("AzureSQL")))
            {
                var dp = new DynamicParameters();
                if (id.HasValue) 
                    dp.Add("payload", (new JObject { ["id"] = id.Value }).ToString());                

                var qr = await conn.QuerySingleOrDefaultAsync<string>("web.get_todo", dp, commandType: CommandType.StoredProcedure);

                return JToken.Parse(qr ?? "[]");
            }
        }
    }
}
