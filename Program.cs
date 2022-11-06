using Newtonsoft.Json;
using SharpDX.DirectInput;
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
        private static string GamePadOutput { get; set; }

        private static void Main(string[] args)
        {
            ExecuteProg();
        }

        private static void ExecuteProg()
        {
            // Run socket connection.
            Console.WriteLine("STARTING...\nPress L+R+Select to restart socket.\n\n=============================================");

            OpenSocket();
            GamePadInputs();
        }

        private static void CallWebSocket(string message)
        {
            try
            {
                WebSocket.Send(message);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.ReadKey(false);
            }
        }

        private static void OpenSocket()
        {
            // Open socket
            WebSocket = new WebSocketSharp.WebSocket("ws://172.16.2.43:8080/Echo");
            WebSocket.OnMessage += WebSocket_OnMessage;
            WebSocket.Connect();
        }

        private static void CloseSocket()
        {
            WebSocket.Close();
        }

        private static void GamePadInputs()
        {
            try
            {
                // Initialize DirectInput
                var directInput = new DirectInput();

                // Find a Joystick Guid
                var joystickGuid = Guid.Empty;

                foreach (var deviceInstance in directInput.GetDevices(DeviceType.Gamepad,
                            DeviceEnumerationFlags.AllDevices))
                    joystickGuid = deviceInstance.InstanceGuid;

                // If Gamepad not found, look for a Joystick
                if (joystickGuid == Guid.Empty)
                    foreach (var deviceInstance in directInput.GetDevices(DeviceType.Joystick,
                            DeviceEnumerationFlags.AllDevices))
                        joystickGuid = deviceInstance.InstanceGuid;

                // If Joystick not found, throws an error
                if (joystickGuid == Guid.Empty)
                {
                    GamePadOutput = "No joystick/Gamepad found.";
                    Console.WriteLine(GamePadOutput);
                    CallWebSocket(GamePadOutput);
                    //Console.ReadKey();
                    //Environment.Exit(1);
                }

                // Instantiate the joystick
                var joystick = new Joystick(directInput, joystickGuid);

                GamePadOutput = joystickGuid.ToString();
                Console.WriteLine("Found Joystick/Gamepad with GUID: {0}", GamePadOutput);
                CallWebSocket(GamePadOutput);

                // Query all suported ForceFeedback effects
                var allEffects = joystick.GetEffects();
                foreach (var effectInfo in allEffects)
                {
                    GamePadOutput = effectInfo.Name;
                    Console.WriteLine("Effect available {0}", GamePadOutput);
                    CallWebSocket(GamePadOutput);
                }

                // Set BufferSize in order to use buffered data.
                joystick.Properties.BufferSize = 128;

                // Acquire the joystick
                joystick.Acquire();

                List<JoystickOffset> joystickOffsets = new();
                // Poll events from joystick
                while (true)
                {
                    try
                    {
                        joystick.Poll();
                        var datas = joystick.GetBufferedData();
                        foreach (var state in datas)
                        {
                            GamePadOutput = state.ToString();
                            Console.WriteLine(GamePadOutput);

                            // Create clear conditions
                            if (state.Offset == JoystickOffset.Buttons4 || state.Offset == JoystickOffset.Buttons5 || state.Offset == JoystickOffset.Buttons6)
                            {
                                joystickOffsets.Add(state.Offset);
                            }
                            else joystickOffsets.Clear();

                            // Check clear conditions
                            if (joystickOffsets.Contains(JoystickOffset.Buttons4) && joystickOffsets.Contains(JoystickOffset.Buttons5) && joystickOffsets.Contains(JoystickOffset.Buttons6))
                            {
                                CloseSocket();
                                ExecuteProg();
                            }
                            CallWebSocket(GamePadOutput);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        GamePadInputs();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                CloseSocket();
                ExecuteProg();
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
            WebSocket.Send(JsonConvert.SerializeObject("Socket Open!"));
        }
    }
}