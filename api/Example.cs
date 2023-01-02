using System;

namespace Function {
    public class Request {
        public string httpMethod { get; set; }
        public string body { get; set; }
    }

    public class Response {
        public int StatusCode { get; set; }
        public string Body { get; set; }

        public Response(int statusCode, string body) {
            StatusCode = statusCode;
            Body = body;
        }
    }

    public class Handler {
        public Response FunctionHandler(Request request) {
            return new Response(200, "Hello, world!");
        }
    }
}
// Function.Handler