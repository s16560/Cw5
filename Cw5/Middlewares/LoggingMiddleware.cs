using Microsoft.AspNetCore.Http;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Cw5.Middlewares
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        public LoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext httpContext)
        {
            httpContext.Request.EnableBuffering();

            if (httpContext.Request != null) 
            {
                string path = httpContext.Request.Path; // np: api/students
                string method = httpContext.Request.Method; // np: get, post
                string queryString = httpContext.Request.QueryString.ToString();
                string bodyStr = "";

                using (var reader = new StreamReader(httpContext.Request.Body,
                                                    Encoding.UTF8, true, 1024, true))
                {
                    bodyStr = await reader.ReadToEndAsync();
                    httpContext.Request.Body.Position = 0;
                }

                //zapis do pliku             
                StreamWriter sw = new StreamWriter("requestsLog.txt", true);
                sw.WriteLine("Path: " + path);
                sw.WriteLine("Query: " + queryString);
                sw.WriteLine("Method: " + method);
                sw.WriteLine("Body: " + bodyStr);
                sw.WriteLine();
                sw.Close();
            }
           
            if(_next != null) await _next(httpContext);
        }
    }
}
