using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace UnexpectedEndOfRequestError
{
    public class Startup
    {
        private ILogger _Logger;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            _Logger = app.ApplicationServices.GetService<ILogger<Startup>>();
            
            // Trying to enable req buffering using custom memory stream replacment
            //app.Use(async (c, next) =>
            //{
            //    var bodyStream = ReadRewindeRequestBody(c);
            //    await next.Invoke();
            //});


            // Trying to enable req buffering using built in helper
            app.Use(async (c, next) =>
            {
                c.Request.EnableBuffering();
                _Logger.LogInformation($"[{c.TraceIdentifier}] in middleware 1 len {c.Request.ContentLength}, buffering enabled");
                await next.Invoke();
            });


            app.Run(async c =>
            {
                var buffer = new byte[Convert.ToInt32(c.Request.ContentLength)];
                _Logger.LogInformation($"reading body with buffer capacity:[{buffer.Length}]");
                await c.Request.Body.ReadAsync(buffer, 0, buffer.Length);
                var body =  Encoding.UTF8.GetString(buffer);
                _Logger.LogInformation($"got body  {body}");
                await c.Response.WriteAsync("{}");
            });
        }

        private async Task<Stream> ReadRewindeRequestBody(HttpContext c)
        {
            var req = c.Request;
            int capacity = (int)req.ContentLength.GetValueOrDefault(1024);
            _Logger.LogInformation($"reading body with buffer capacity:[{capacity}]");
            var ms = new MemoryStream(capacity);
            await req.Body.CopyToAsync(ms);
            ms.Seek(0, SeekOrigin.Begin);
            var bodyTxt =  Encoding.UTF8.GetString(ms.ToArray());

            _Logger.LogInformation($"[{c.TraceIdentifier}] in request got body:[{bodyTxt}]");
            req.Body = ms;
            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }
    }
}
