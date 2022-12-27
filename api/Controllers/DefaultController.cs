using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

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
            Console.WriteLine(body.Request);
            var response = new SqlResponse();
            response.Columns = new []{"Column1", "Column2"};
            response.Rows = new[] { new dynamic[] { "123", 1 }, new dynamic[] { "123", "2" } };
            response.SyntaxTree = "digraph { a -> b;}";
            response.QueryTree = "graph { a -- b;\n" +
                                 "bgcolor = transparent;\n" +
                                 "a -- c;\n" +
                                 "b -- d;}";
            response.PlannerTree = "digraph { b -> c;}";
            return response;
        }
    }
}