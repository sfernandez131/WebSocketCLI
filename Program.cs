using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using WebSocketSharp;

namespace WebSocketCLI
{
    internal class Program
    {
        private static WebSocketSharp.WebSocket WebSocket { get; set; }

        private static void Main(string[] args)
        {
            CallWebSocket();
        }

        private static void CallWebSocket()
        {
            try
            {
                // Run socket connection.
                Console.WriteLine("STARTING...\n");
                WebSocket = new WebSocketSharp.WebSocket("ws://127.0.0.1:7889/Echo");
                WebSocket.OnMessage += WebSocket_OnMessage;
                WebSocket.Connect();

                WebSocket.Send("Hello!");

                Console.ReadKey();

                WebSocket.Close();
            }
            catch (Exception e)
            {
            }
        }

        private static void RunTimer(int time)
        {
            int c = 0;
            while (true)
            {
                Console.Write(" " + c + "\r");
                if (c >= time)
                    break;
                Thread.Sleep(1000);
                c++;
            }
        }

        private static void WebSocket_OnError(object sender, WebSocketSharp.ErrorEventArgs e)
        {
            Console.WriteLine($"ERROR:\n {e}\n");
            Console.ReadKey();
        }

        private static void WebSocket_OnMessage(object sender, MessageEventArgs e)
        {
            var testing = e.Data;
        }

        private static void WebSocket_OnOpen(object sender, EventArgs e)
        {
            WebSocket.Send(JsonConvert.SerializeObject("Hello!"));
        }
    }
}