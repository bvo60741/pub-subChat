using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using ZeroMQ;

namespace Examples
{
    static partial class Subscriber
    {
        public static void Main(string[] args)
        {
            string UserName;
            Console.Write("Enter your name: ");
            UserName = Console.ReadLine();

            
            using (var context = new ZContext())
            using (var subscriber = new ZSocket(context, ZSocketType.SUB))
            using (var requester = new ZSocket(context, ZSocketType.REQ))
            {
                // First, connect our subscriber socket
                subscriber.Connect("tcp://127.0.0.1:5561");
                subscriber.SubscribeAll();

                // 0MQ is so fast, we need to wait a while…
                Thread.Sleep(1);

                // Second, synchronize with publisher
                requester.Connect("tcp://127.0.0.1:5562");

                // - send a synchronization request
                requester.Send(new ZFrame());

                // - wait for synchronization reply
                requester.ReceiveFrame();

                // Third, get our updates and report how many we got

                while(true)
                {
                    string requestText;
                    Console.Write("Enter your message: ");
                    requestText = Console.ReadLine();
                    Console.WriteLine();
                    Console.WriteLine("Sending {0}: {1}...", UserName, requestText);

                    // Send
                    requester.Send(new ZFrame(requestText));




                    // Receive

                    using (ZFrame frame = subscriber.ReceiveFrame())
                    {
                        string text = frame.ReadString();
                        Console.WriteLine("Received {0} updates.", frame.ReadString());
                        if (text == "TEST")
                        {
                            Console.WriteLine(text);
                        }

                        frame.Position = 0;
                        Console.WriteLine("Receiving {0}…", frame.ReadInt32());
                    }
                    //Console.WriteLine("Received {0} updates.", i);
                }
            }
        }
    }
}