using System.Text.Json;
using Api.Controllers;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Api
{
// lambda handler function for asp.net core should trigger DefaultController
    public class Handler
    {
        public class Request
        {
            public string httpMethod { get; set; }
            public string body { get; set; }
        }

        private static readonly JsonSerializerSettings JsonSettings = new()
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            },
            Formatting = Formatting.Indented
        };

        private static readonly DefaultController Controller =
            new(new LoggerFactory().CreateLogger<DefaultController>());

        public string FunctionHandler(Request r)
        {
            var method = r.httpMethod;

            switch (method)
            {
                case "GET":
                    return Controller.Get();
                case "POST":
                {
                    var body = r.body;
                    SqlRequest? jsonDoc = JsonConvert.DeserializeObject<SqlRequest>(body, JsonSettings);
                    if (jsonDoc == null)
                    {
                        return "Invalid request";
                    }

                    return JsonConvert.SerializeObject(Controller.Post(jsonDoc));
                }
                default:
                    return "Method not supported";
            }
        }
    }
}