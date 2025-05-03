using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ServerManager.Logger;
using ServerManager.Models;
using ServerManager.Utils;

namespace ServerManager.Server
{
    public class WebServer
    {
        private HttpListener _httpListener;
        private readonly ServerLogger _logger;
        private readonly JsonDataManager<ServerData> _store;

        public WebServer(ServerLogger logger, JsonDataManager<ServerData> store)
        {
            _logger = logger;
            _store = store ?? throw new ArgumentNullException(nameof(store));
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
                    _ = Task.Run(() => HandleRequestAsync(context)); // Maneja cada request por separado
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

            response.Headers.Add("Access-Control-Allow-Origin", "*");
            response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
            response.Headers.Add("Access-Control-Allow-Headers", "Content-Type");

            try
            {
                if (context.Request.Url.AbsolutePath == "/api/ping")
                {
                    response.ContentType = "application/json";
                    var responseString = "{\"status\":\"ok\"}";
                    var buffer = Encoding.UTF8.GetBytes(responseString);

                    response.ContentLength64 = buffer.Length;
                    await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                    response.OutputStream.Close();
                }
                else if (context.Request.Url.AbsolutePath == "/api/serverdata")
                {
                    await StartSseStream(context); // no cerrar aquí, se maneja dentro
                }
                else
                {
                    response.StatusCode = 404;
                    var responseString = "{\"status\":\"not found\"}";
                    var buffer = Encoding.UTF8.GetBytes(responseString);

                    response.ContentLength64 = buffer.Length;
                    await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                    response.OutputStream.Close();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error en HandleRequestAsync: {ex.Message}");
                response.StatusCode = 500;
                if (response.OutputStream.CanWrite)
                    response.OutputStream.Close();
            }
        }

        private async Task StartSseStream(HttpListenerContext context)
        {
            var response = context.Response;

            response.StatusCode = 200;
            response.SendChunked = true;
            response.ContentType = "text/event-stream";
            response.Headers.Add("Cache-Control", "no-cache");
            response.Headers.Add("Connection", "keep-alive");

            _logger.Log("SSE stream abierto");

            try
            {
                while (response.OutputStream.CanWrite)
                {
                    ServerData d = _store.Load();
                    string data = $"data: {JsonConvert.SerializeObject(d)}\n\n";
                    byte[] buffer = Encoding.UTF8.GetBytes(data);

                    await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                    await response.OutputStream.FlushAsync();
                    await Task.Delay(1000);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error en SSE: {ex.Message}");
            }
            finally
            {
                _logger.Log("Cerrando SSE stream");
                response.OutputStream.Close();
            }
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
    }
}
