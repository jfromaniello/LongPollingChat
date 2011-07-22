using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
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
            var waiting = new Queue<RequestContext>();

            using (var requests = new Anna.HttpServer("http://127.0.0.1:987/", eventLoop))
            {
                requests.GET("app.js")
                    .Subscribe(r => r.Respond(new StaticFileResponse("app.js")));

                requests.GET("index.html")
                    .Subscribe(r => r.Respond(new StaticFileResponse("index.html")));

                requests.POST("wait")
                    .Subscribe(r =>
                                   {
                                       Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
                                       waiting.Enqueue(r);
                                   });

                requests.POST("send")
                    .Subscribe(r => {
                                        string message;
                                        using(var sr = new StreamReader(r.Request.InputStream))
                                        {
                                            message = sr.ReadToEnd();
                                            
                                        }

                                        r.Respond(201);

                                        while (waiting.Count > 0)
                                        {
                                            waiting.Dequeue().Respond(new StringResponse(message));
                                        }
                    });

                Console.ReadLine();
            }
        }
    }
}
