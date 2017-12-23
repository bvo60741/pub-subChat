using System;
using System.Threading;
using ZeroMQ;

namespace Publisher
{
    static partial class Program
    {
        const int SyncPub_SubscribersExpected = 1;    // We wait for 1 subscribers

        public static void Main(string[] args)
        {
           Console.WriteLine("This is a server for our chat (kinda)");
           Console.WriteLine();

            // Socket to talk to clients and
            // Socket to receive signals
            using (var context = new ZContext())
            using (var publisher = new ZSocket(context, ZSocketType.PUB))
            using (var responder = new ZSocket(context, ZSocketType.REP))
            {
                publisher.SendHighWatermark = 1100000;
                publisher.Bind("tcp://*:8080");

                responder.Bind("tcp://*:8081");

                // Get synchronization from subscribers
                int subscribers = SyncPub_SubscribersExpected;
                do
                {
                    Console.WriteLine("Waiting for {0} subscriber" + (subscribers > 1 ? "s" : string.Empty) + "…", subscribers);

                    // - wait for synchronization request
                    responder.ReceiveFrame();

                    // - send synchronization reply
                    responder.Send(new ZFrame());
                }
                while (--subscribers > 0);

                while (true)
                {

                    // Receive
                    using (ZFrame request = responder.ReceiveFrame())
                    {
                        using (var message = new ZMessage())
                        {
                            Console.WriteLine("Received from user: {0} ", request.ReadString());

                            // Do some work
                            Thread.Sleep(1);

                            // Send
                            string sendText;
                            sendText = request.ReadString();

                            responder.Send(new ZFrame(sendText));
                            message.Add(new ZFrame(sendText));
                            publisher.Send(message);
                        }

                    }

                    /*using (ZFrame frame = publisher.ReceiveFrame())
                    {
                        string text = frame.ReadString();
                    }*/


                    // Now broadcast exactly 20 updates followed by END
                    /*Console.WriteLine("Broadcasting messages:");
                    for (int i = 0; i < 20; ++i)
                    {
                        Console.WriteLine("Sending {0}…", i);
                        publisher.Send(new ZFrame(i));
                    }
                    publisher.Send(new ZFrame("END"));*/

                }
            }
        }
    }
}