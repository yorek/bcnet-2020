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

                return EnrichResult(qr);
            }
        }

        [HttpDelete("{id?}")]
        public async Task<JToken> Delete(int? id)
        { 
            using(var conn = new SqlConnection(_config.GetConnectionString("AzureSQL")))
            {
                var dp = new DynamicParameters();
                if (id.HasValue) 
                    dp.Add("payload", (new JObject { ["id"] = id.Value }).ToString());                

                await conn.ExecuteAsync("web.delete_todo", dp, commandType: CommandType.StoredProcedure);

                return JToken.Parse("[]");
            }
        }

        [HttpPost]
        public async Task<JToken> Post([FromBody]JToken payload)
        {            
            using(var conn = new SqlConnection(_config.GetConnectionString("AzureSQL")))
            {
                var dp = new DynamicParameters();
                dp.Add("payload", payload.ToString());                

                var qr = await conn.QuerySingleOrDefaultAsync<string>("web.post_todo", dp, commandType: CommandType.StoredProcedure);

                return EnrichResult(qr);
            }
        }

        [HttpPatch("{id}")]
        public async Task<JToken> Patch(int id, [FromBody]JToken payload)
        {
            using(var conn = new SqlConnection(_config.GetConnectionString("AzureSQL")))
            {
                var dp = new DynamicParameters();
                var wrapper = new JObject
                {
                    ["id"] = id,
                    ["todo"] = payload
                };
                dp.Add("payload", wrapper.ToString());                

                var qr = await conn.QuerySingleOrDefaultAsync<string>("web.patch_todo", dp, commandType: CommandType.StoredProcedure);

                return EnrichResult(qr);
            }
        }

        private JToken EnrichResult(string source)
        {
            var jr = JToken.Parse(source ?? "[]");
            
            var baseUrl = (HttpContext != null) ? HttpContext.Request.Scheme + "://" + HttpContext.Request.Host : string.Empty;

            var AddUrl = new Action<JObject>(o => 
            {
                if (o == null) return;
                var todoUrl = $"{baseUrl}/todo/{o["id"]}";
                o["url"] = todoUrl;
            });
            
            if (jr is JArray ja)
                ja.ToList().ForEach(e => AddUrl(e as JObject));
            else if (jr is JObject jo)
                AddUrl(jo);
            else
                throw new ArgumentException($"{nameof(source)} is not an array or an object");

            return jr;
        }
    }
}
