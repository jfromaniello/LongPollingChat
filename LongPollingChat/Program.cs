using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using Anna;
using Anna.Request;
using Anna.Responses;

namespace LongPollingChat
{
    class Program
    {
        static void Main()
        {
            var eventLoop = new EventLoopScheduler();
            
            using (var requests = new HttpServer("http://127.0.0.1:987/", eventLoop))
            {
                requests.GET("app.js")
                    .Subscribe(r => r.Respond(new StaticFileResponse("app.js")));

                requests.GET("index.html")
                    .Subscribe(r => r.Respond(new StaticFileResponse("index.html")));

                var messageStream = requests.POST("send")
                    .Select(r => {
                              string message;
                              using(var sr = new StreamReader(r.Request.InputStream))
                              {
                                  message = sr.ReadToEnd();
                              }
                              r.Respond(201);
                              return message;
                    }).Publish().RefCount();

                requests.POST("wait")
                        .SelectMany(subscriber => messageStream.Take(1),
                                   (subscriber, message) => new {subscriber, message})
                        .Subscribe(kp => kp.subscriber.Respond(new StringResponse(kp.message)));
                
                Console.ReadLine();
            }
        }
    }
}
