using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using ParallelDB;
using ParallelDB.Tables;

namespace Api.Controllers
{
    [ApiController]
    [Route("/")]
    public class DefaultController : ControllerBase
    {
        [HttpGet]
        public string Get()
        {
            return "Hello World";
        }

        private readonly ILogger<DefaultController> _logger;
        
        private readonly ParallelDb _db = new();

        public DefaultController(ILogger<DefaultController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public SqlResponse Post(SqlRequest body)
        {
            var response = new SqlResponse();
            try
            {
                string? sql = body.Request;
                if (sql is null)
                {
                    response.Error = "No SQL provided";
                    return response;
                }
                var result = _db.Execute(sql);
                if (result is bool)
                {
                    response.Columns = new[] { "Result" };
                    response.Rows = new[] { new[] { result.ToString() } };
                }
                else if (result is Table t)
                {
                    response.Columns = t.Columns.Select(c => c.Name).ToArray();
                    response.Rows = t.Rows.Select(r =>
                    {
                        var row = new dynamic?[t.Columns.Count];
                        for (var i = 0; i < t.Columns.Count; i++)
                        {
                            row[i] = r[i];
                        }

                        return row;
                    }).ToArray();
                }

                response.SyntaxTree = _db.GetSyntaxTree(sql);
                response.QueryTree = _db.GetQuery(sql).GetPlan();
                return response;
            }
            catch (Exception e)
            {
                response.Error = e.Message;
                return response;
            }
        }
    }
}