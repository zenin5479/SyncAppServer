using System;
using System.IO;
using System.Net;
using System.Text;

namespace SyncAppServer
{
   class Program
   {
      static void Main()
      {
         HttpListener listener = new HttpListener();
         // Указываем префиксы для прослушивания
         listener.Prefixes.Add("http://127.0.0.1:8080/");
         //listener.Prefixes.Add("http://127.0.0.1:8888/connection/");
         listener.Start();
         Console.WriteLine("Сервер запущен на http://127.0.0.1:8080/");
         // Синхронная обработка запросов в цикле

         bool exitLoop = false;
         Console.WriteLine("Цикл активен. Нажмите любую клавишу для проверки...");
         while (!exitLoop)
         {
            try
            {
               if (Console.KeyAvailable)
               {
                  ConsoleKey key = Console.ReadKey(true).Key;
                  Console.WriteLine("Нажата клавиша: {0} Продолжить? (Y/N)", key);

                  if (Console.ReadKey(true).Key == ConsoleKey.N)
                  {
                     exitLoop = true;
                     Console.WriteLine("Цикл прерван");
                  }
                  else
                  {
                     // Ожидаем входящий запрос (блокирующий вызов)
                     HttpListenerContext context = listener.GetContext();
                     ProcessRequest(context);
                  }
               }
            }
            catch (Exception ex)
            {
               Console.WriteLine("Ошибка: {0}", ex.Message);
            }



         }
      }

      static void ProcessRequest(HttpListenerContext context)
      {
         HttpListenerRequest request = context.Request;
         HttpListenerResponse response = context.Response;

         // Настройка ответа
         response.ContentType = "application/json";
         response.ContentEncoding = Encoding.UTF8;

         string responseString;
         try
         {
            // Обработка методов
            if (request.HttpMethod == "GET")
            {
               responseString = HandleGet(request);
               response.StatusCode = (int)HttpStatusCode.OK;
            }
            else if (request.HttpMethod == "POST")
            {
               responseString = HandlePost(request);
               response.StatusCode = (int)HttpStatusCode.Created;
            }
            else if (request.HttpMethod == "PUT")
            {
               responseString = HandlePut(request);
               response.StatusCode = (int)HttpStatusCode.OK;
            }
            else if (request.HttpMethod == "DELETE")
            {
               responseString = HandleDelete(request);
               response.StatusCode = (int)HttpStatusCode.NoContent;
            }
            else
            {
               responseString = "{\"error\":\"Method not supported\"}";
               response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
            }
         }
         catch (Exception ex)
         {
            responseString = string.Format("{{\"error\":\"{0}\"}}", ex.Message);
            response.StatusCode = (int)HttpStatusCode.InternalServerError;
         }

         // Запись ответа
         byte[] buffer = Encoding.UTF8.GetBytes(responseString);
         response.ContentLength64 = buffer.Length;
         response.OutputStream.Write(buffer, 0, buffer.Length);
         response.OutputStream.Close();

         // Логирование
         Console.WriteLine("{0} {1} {2} -> {3}", DateTime.Now, request.HttpMethod, request.Url, response.StatusCode);
      }

      static string HandleGet(HttpListenerRequest request)
      {
         // Пример: извлечение параметров запроса
         string name = request.QueryString["name"];
         if (name == null)
         {
            name = "world";
         }

         return string.Format("{{\"message\":\"Hello {0}\", \"method\":\"GET\", \"timestamp\":\"{1}\"}}",
            name,
            DateTime.Now);
      }

      static string HandlePost(HttpListenerRequest request)
      {
         // Чтение тела запроса
         string body;
         using (StreamReader reader = new StreamReader(request.InputStream, request.ContentEncoding))
         {
            body = reader.ReadToEnd();
         }

         return string.Format("{{\"message\":\"Data received\", \"method\":\"POST\", \"data\":{0}, \"timestamp\":\"{1}\"}}",
            body,
            DateTime.Now);
      }

      static string HandlePut(HttpListenerRequest request)
      {
         string body;
         using (StreamReader reader = new StreamReader(request.InputStream, request.ContentEncoding))
         {
            body = reader.ReadToEnd();
         }

         // Пример обработки (обычно обновление ресурса)
         return string.Format("{{\"message\":\"Resource updated\", \"method\":\"PUT\", \"data\":{0}, \"timestamp\":\"{1}\"}}",
            body,
            DateTime.Now);
      }

      static string HandleDelete(HttpListenerRequest request)
      {
         // Пример: удаление ресурса по ID из URL
         string resourceId;
         if (request.Url.Segments.Length > 1)
         {
            resourceId = request.Url.Segments[1];
         }
         else
         {
            resourceId = "unknown";
         }

         return string.Format("{{\"message\":\"Resource {0} deleted\", \"method\":\"DELETE\", \"timestamp\":\"{1}\"}}",
            resourceId,
            DateTime.Now);
      }
   }
}