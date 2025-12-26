using System;
using System.IO;
using System.Net;
using System.Text;

namespace SyncAppServer
{
   internal class Program
   {

      private static readonly HttpListener Listener = new HttpListener();

      static void Main()
      {
         // Добавляем обработку CORS для тестирования
         Listener.Prefixes.Add("http://localhost:8080/");
         Listener.Start();
         Console.WriteLine("Сервер запущен на http://localhost:8080/");
         Console.WriteLine("Ожидание запросов...\n");

         while (true)
         {
            try
            {
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

         // Добавляем CORS заголовки для тестирования
         response.AddHeader("Access-Control-Allow-Origin", "*");
         response.AddHeader("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
         response.AddHeader("Access-Control-Allow-Headers", "Content-Type");

         // Обработка preflight запросов OPTIONS
         if (request.HttpMethod == "OPTIONS")
         {
            response.StatusCode = (int)HttpStatusCode.OK;
            response.OutputStream.Close();
            Console.WriteLine($"{DateTime.Now} OPTIONS -> 200 OK");
            return;
         }

         response.ContentType = "application/json";
         response.ContentEncoding = Encoding.UTF8;

         string responseString = "";

         try
         {
            // Логируем входящий запрос
            Console.WriteLine($"{DateTime.Now} {request.HttpMethod} {request.Url}");
            if (request.HasEntityBody)
            {
               using (StreamReader reader = new StreamReader(request.InputStream, request.ContentEncoding))
               {
                  string body = reader.ReadToEnd();
                  Console.WriteLine($"Тело запроса: {body}");
                  // Восстанавливаем поток для дальнейшей обработки
                  request.InputStream.Position = 0;
               }
            }

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
                  response.StatusCode = (int)HttpStatusCode.OK; // Или 204 No Content
                  break;

               default:
                  responseString = "{\"error\":\"Method not supported\", \"supportedMethods\":[\"GET\",\"POST\",\"PUT\",\"DELETE\",\"OPTIONS\"]}";
                  response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
                  break;
            }
         }
         catch (Exception ex)
         {
            responseString = $"{{\"error\":\"{ex.Message}\", \"details\":\"{ex.GetType().Name}\"}}";
            response.StatusCode = (int)HttpStatusCode.InternalServerError;
         }

         // Запись ответа
         byte[] buffer = Encoding.UTF8.GetBytes(responseString);
         response.ContentLength64 = buffer.Length;
         response.OutputStream.Write(buffer, 0, buffer.Length);
         response.OutputStream.Close();

         Console.WriteLine($"  Ответ: {response.StatusCode} - {responseString}\n");
      }

      private static string HandleGet(HttpListenerRequest request)
      {
         var query = System.Web.HttpUtility.ParseQueryString(request.Url.Query);
         var name = query["name"] ?? "Guest";
         var city = query["city"] ?? "Unknown";

         return $"{{\"message\":\"Hello {name} from {city}\", \"method\":\"GET\", \"queryParams\":{JsonSerialize(query)}, \"timestamp\":\"{DateTime.Now:yyyy-MM-dd HH:mm:ss}\"}}";
      }

      private static string HandlePost(HttpListenerRequest request)
      {
         string body;
         using (StreamReader reader = new StreamReader(request.InputStream, request.ContentEncoding))
         {
            body = reader.ReadToEnd();
         }

         // Проверяем, является ли body валидным JSON
         try
         {
            if (!string.IsNullOrEmpty(body))
            {
               // Попытка парсинга JSON
               var obj = Newtonsoft.Json.Linq.JToken.Parse(body);
               return $"{{\"status\":\"success\", \"method\":\"POST\", \"receivedData\":{body}, \"processedAt\":\"{DateTime.Now:yyyy-MM-dd HH:mm:ss}\"}}";
            }
            return "{\"status\":\"success\", \"method\":\"POST\", \"message\":\"Empty body received\"}";
         }
         catch
         {
            return $"{{\"status\":\"success\", \"method\":\"POST\", \"rawData\":\"{body.Replace("\"", "\\\"")}\"}}";
         }
      }

      private static string HandlePut(HttpListenerRequest request)
      {
         string body;
         using (StreamReader reader = new StreamReader(request.InputStream, request.ContentEncoding))
         {
            body = reader.ReadToEnd();
         }

         return $"{{\"status\":\"updated\", \"method\":\"PUT\", \"data\":{body ?? "{}"}, \"updatedAt\":\"{DateTime.Now:yyyy-MM-dd HH:mm:ss}\"}}";
      }

      private static string HandleDelete(HttpListenerRequest request)
      {
         // Извлекаем ID из URL
         string resourceId = "unknown";
         if (request.Url.Segments.Length > 1)
         {
            resourceId = request.Url.Segments[^1]; // Последний сегмент
         }

         return $"{{\"status\":\"deleted\", \"method\":\"DELETE\", \"resourceId\":\"{resourceId}\", \"deletedAt\":\"{DateTime.Now:yyyy-MM-dd HH:mm:ss}\"}}";
      }

      private static string JsonSerialize(System.Collections.Specialized.NameValueCollection query)
      {
         var dict = new System.Collections.Generic.Dictionary<string, string>();
         foreach (string key in query.AllKeys)
         {
            dict[key] = query[key];
         }
         return Newtonsoft.Json.JsonConvert.SerializeObject(dict);
      }
   }
}