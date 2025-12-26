using System;
using System.IO;
using System.Net;

namespace SyncAppServer
{
   internal class Program
   {

      private static readonly HttpListener listener = new HttpListener();

      static void Main(string[] args)
      {
         // Добавляем обработку CORS для тестирования
         listener.Prefixes.Add("http://127.0.0.1:8888/");
         listener.Start();
         Console.WriteLine("Сервер запущен на http://127.0.0.1:8888");
         Console.WriteLine("Ожидание запросов...\n");

         while (true)
         {
            try
            {
               HttpListenerContext context = listener.GetContext();
               ProcessRequest(context);
            }
            catch (Exception ex)
            {
               Console.WriteLine($"Ошибка: {ex.Message}");
            }
         }
      }

      // Для этого примера требуются пространства имен System и System.Net
      public static void SimpleListenerExample(string[] prefixes)
      {
         if (!HttpListener.IsSupported)
         {
            Console.WriteLine("Для использования класса HttpListener требуется Windows XP с пакетом обновления 2 или Server 2003");
            return;
         }

         // Требуются префиксы URI, например "http://127.0.0.1:8888/connection/"
         if (prefixes == null || prefixes.Length == 0)
         {
            throw new ArgumentException("prefixes");
         }

         // Создайте прослушиватель
         HttpListener listener = new HttpListener();
         // Добавляем префиксы
         foreach (string s in prefixes)
         {
            listener.Prefixes.Add(s);
         }

         listener.Start();
         Console.WriteLine("Прослушивание...");
         // Примечание: Метод getContext блокируется во время ожидания запроса
         HttpListenerContext context = listener.GetContext();
         HttpListenerRequest request = context.Request;
         // Получаем объект ответа
         HttpListenerResponse response = context.Response;
         // Формулируем ответ
         string responseString = "<HTML><BODY> Hello world!</BODY></HTML>";
         byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
         // Получаем поток ответов и записываем ответ
         response.ContentLength64 = buffer.Length;
         Stream output = response.OutputStream;
         output.Write(buffer, 0, buffer.Length);
         // Закрываем выходной поток
         output.Close();
         listener.Stop();
      }
   }
}