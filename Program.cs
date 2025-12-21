using System;
using System.Net;

namespace SyncAppServer
{
   internal class Program
   {
      static void Main(string[] args)
      {
         Console.WriteLine("Hello World!");
      }

      // Для этого примера требуются пространства имен System и System.Net
      public static void SimpleListenerExample(string[] prefixes)
      {
         if (!HttpListener.IsSupported)
         {
            Console.WriteLine("Для использования класса HttpListener требуется Windows XP с пакетом обновления 2 или Server 2003");
            return;
         }

         // Требуются префиксы URI, например "http://contoso.com:8080/index/"
         if (prefixes == null || prefixes.Length == 0)
            throw new ArgumentException("prefixes");

         // Создайте прослушиватель
         HttpListener listener = new HttpListener();
         //Добавьте префиксы.
         foreach (string s in prefixes)
         {
            listener.Prefixes.Add(s);
         }
         listener.Start();
         Console.WriteLine("Listening...");
         // Примечание: Метод getContext блокируется во время ожидания запроса.
         HttpListenerContext context = listener.GetContext();
         HttpListenerRequest request = context.Request;
         // Получите объект ответа.
         HttpListenerResponse response = context.Response;
         // Сформулируйте ответ.
         string responseString = "<HTML><BODY> Hello world!</BODY></HTML>";
         byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
         // Получите поток ответов и запишите ответ на него.
         response.ContentLength64 = buffer.Length;
         System.IO.Stream output = response.OutputStream;
         output.Write(buffer, 0, buffer.Length);
         // Вы должны закрыть выходной поток.
         output.Close();
         listener.Stop();
      }
   }
}