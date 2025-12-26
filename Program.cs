using System;
using System.IO;
using System.Net;
using System.Text;

namespace SyncAppServer
{
   class Program
   {
      private static readonly HttpListener Listener = new HttpListener();

      static void Main(string[] args)
      {
         // Указываем префиксы для прослушивания
         Listener.Prefixes.Add("http://localhost:8080/");
         Listener.Start();
         Console.WriteLine("Сервер запущен на http://localhost:8080/");

         // Синхронная обработка запросов в цикле
         while (true)
         {
            try
            {
               // Ожидаем входящий запрос (блокирующий вызов)
               HttpListenerContext context = Listener.GetContext();
               ProcessRequest(context);
            }
            catch (Exception ex)
            {
               Console.WriteLine($"Ошибка: {ex.Message}");
            }
         }
      }

      private static void ProcessRequest(HttpListenerContext context)
      {
         HttpListenerRequest request = context.Request;
         HttpListenerResponse response = context.Response;

         // Настройка ответа
         response.ContentType = "application/json";
         response.ContentEncoding = Encoding.UTF8;

         string responseString = "";
         try
         {
            // Обработка методов
            switch (request.HttpMethod)
            {
               case "GET":
                  responseString = HandleGet(request);
                  response.StatusCode = (int)HttpStatusCode.OK;
                  break;

               case "POST":
                  responseString = HandlePost(request);
                  response.StatusCode = (int)HttpStatusCode.Created;
                  break;

               case "PUT":
                  responseString = HandlePut(request);
                  response.StatusCode = (int)HttpStatusCode.OK;
                  break;

               case "DELETE":
                  responseString = HandleDelete(request);
                  response.StatusCode = (int)HttpStatusCode.NoContent;
                  break;

               default:
                  responseString = "{\"error\":\"Method not supported\"}";
                  response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
                  break;
            }
         }
         catch (Exception ex)
         {
            responseString = $"{{\"error\":\"{ex.Message}\"}}";
            response.StatusCode = (int)HttpStatusCode.InternalServerError;
         }

         // Запись ответа
         byte[] buffer = Encoding.UTF8.GetBytes(responseString);
         response.ContentLength64 = buffer.Length;
         response.OutputStream.Write(buffer, 0, buffer.Length);
         response.OutputStream.Close();

         // Логирование
         Console.WriteLine($"{DateTime.Now} {request.HttpMethod} {request.Url} -> {response.StatusCode}");
      }

      private static string HandleGet(HttpListenerRequest request)
      {
         // Пример: извлечение параметров запроса
         string name = request.QueryString["name"] ?? "world";
         return $"{{\"message\":\"Hello {name}\", \"method\":\"GET\", \"timestamp\":\"{DateTime.Now}\"}}";
      }

      private static string HandlePost(HttpListenerRequest request)
      {
         // Чтение тела запроса
         string body;
         using (StreamReader reader = new StreamReader(request.InputStream, request.ContentEncoding))
         {
            body = reader.ReadToEnd();
         }

         return $"{{\"message\":\"Data received\", \"method\":\"POST\", \"data\":{body}, \"timestamp\":\"{DateTime.Now}\"}}";
      }

      private static string HandlePut(HttpListenerRequest request)
      {
         string body;
         using (StreamReader reader = new StreamReader(request.InputStream, request.ContentEncoding))
         {
            body = reader.ReadToEnd();
         }

         // Пример обработки (обычно обновление ресурса)
         return $"{{\"message\":\"Resource updated\", \"method\":\"PUT\", \"data\":{body}, \"timestamp\":\"{DateTime.Now}\"}}";
      }

      private static string HandleDelete(HttpListenerRequest request)
      {
         // Пример: удаление ресурса по ID из URL
         string resourceId = request.Url.Segments.Length > 1 ? request.Url.Segments[1] : "unknown";
         return $"{{\"message\":\"Resource {resourceId} deleted\", \"method\":\"DELETE\", \"timestamp\":\"{DateTime.Now}\"}}";
      }
   }
}