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
                var sql = body.Request;

                var db = new ParallelDb();
                var result = db.Execute(sql);
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

                response.SyntaxTree = db.GetSyntaxTree(sql);
                response.QueryTree = db.GetQuery(sql).GetPlan();
                response.PlannerTree = "";
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