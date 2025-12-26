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

      
   }
}