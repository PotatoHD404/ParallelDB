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
        
        [HttpPost]
        public SqlResponse Post(SqlRequest body)
        {
            Console.WriteLine(body.Request);
            var response = new SqlResponse();
            response.Columns = new []{"Column1", "Column2"};
            response.Rows = new[] { new dynamic[] { "123", 1 }, new dynamic[] { "123", "2" } };
            response.SyntaxTree = "digraph { a -> b;}";
            return response;
        }
    }
}