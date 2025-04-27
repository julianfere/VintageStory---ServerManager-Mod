using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ServerManager.Logger;
using ServerManager.Models;

namespace ServerManager.Server
{
    public class WebServer
    {
        private HttpListener _httpListener;
        private readonly ServerLogger _logger;

        public WebServer(ServerLogger logger)
        {
            _logger = logger;
        }

        public void StartAsync()
        {
            Task.Run(() => StartServer());
        }

        private async Task StartServer()
        {
            _httpListener = new HttpListener();
            _httpListener.Prefixes.Add("http://localhost:5000/");

            try
            {
                _httpListener.Start();
                _logger.Log("Starting web server...");

                while (_httpListener.IsListening)
                {
                    var context = await _httpListener.GetContextAsync();

                    await HandleRequestAsync(context);
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"Error starting web server: {e.Message}");
            }
        }

        private async Task HandleRequestAsync(HttpListenerContext context)
        {
            var response = context.Response;

            if (context.Request.Url.AbsolutePath == "/api/ping")
            {
                response.ContentType = "application/json";
                var responseString = "{\"status\":\"ok\"}";
                var buffer = Encoding.UTF8.GetBytes(responseString);

                response.ContentLength64 = buffer.Length;
                await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            }
            else if (context.Request.Url.AbsolutePath == "/api/serverdata")
            {
                await StartSseStream(context);
            }
            else
            {
                response.StatusCode = 404;
                var responseString = "{\"status\":\"not found\"}";
                var buffer = Encoding.UTF8.GetBytes(responseString);

                response.ContentLength64 = buffer.Length;
                await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            }

            response.OutputStream.Close();
        }

        public void StopAsync()
        {
            if (_httpListener != null)
            {
                _logger.Log("Stopping web server...");
                _httpListener.Stop();
                _httpListener = null;
            }
        }

        private async Task StartSseStream(HttpListenerContext context)
        {
            var response = context.Response;

            response.ContentType = "text/event-stream";
            response.Headers.Add("Cache-Control", "no-cache");
            response.Headers.Add("Connection", "keep-alive");

            response.StatusCode = 200;
            response.SendChunked = true;

            //while (true)
            //{
            //    string data = $"data: {JsonConvert.SerializeObject(ServerData.Instance, Formatting.Indented)}\n\n";

            //    var buffer = Encoding.UTF8.GetBytes(data);`
            //    await response.OutputStream.WriteAsync(buffer);

            //    await Task.Delay(1000);
            //}

            for (int i = 0; i < 10; i++)
            {
                string data = $"data: {JsonConvert.SerializeObject(ServerData.Instance, Formatting.Indented)}\n\n";

                var buffer = Encoding.UTF8.GetBytes(data);
                await response.OutputStream.WriteAsync(buffer);
            }
        }
    }
}
