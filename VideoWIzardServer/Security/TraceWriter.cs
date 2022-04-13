using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Reflection;
using System.Threading.Tasks;

namespace Restaurant.Helpers
{
    public class TraceWriter
    {
        public static async Task WriteLineToTraceAsync(string message)
        {
            await Task.Run(() =>
            {
                Assembly TraceAssembly = Assembly.Load("TraceLib");
                if (TraceAssembly == null)
                {
                    Console.WriteLine("Failed to load TraceLib.dll file," +
                        "please make sure it is placed next to the .exe.\n");
                }
                Console.WriteLine("Loaded TraceLib.dll successfully.\n");
                dynamic trLibObj = TraceAssembly.CreateInstance("TraceLib.TraceHandler");
                if (trLibObj == null)
                {
                    Console.WriteLine("Dynamic TraceLib.TraceHandler instance was null.\n");
                }
                Task addTask = Task.Run(() => trLibObj.AddReceiverAsync("D:/Rares Laptop/8 Uni/Materiale/Medii si instrumente de programare/laboratoare/Proiect3Medii/Restaurant/bin/Log.txt"));
                Task.Run(() => trLibObj.WriteLineAsync(message, addTask));
            });
        }
    }
}