using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace TraceLib
{
    public class TraceHandler
    {
        private static List<string> Filepaths = new List<string>();   
        //add a file receiver to which you want to write before using the write function
        public async Task AddReceiverAsync(string filepath)
        {
            await Task.Run(() => {
                try
                {
                    if (Filepaths.Contains(filepath))
                    {
                        return;
                    }
                    FileStream fileStream = new FileStream(filepath, FileMode.OpenOrCreate | FileMode.Append);
                    TextWriterTraceListener textWriterTraceListener = new TextWriterTraceListener(fileStream);
                    Trace.Listeners.Add(textWriterTraceListener);
                    Trace.AutoFlush = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to initialise Tracing.\n" +
                        "Exception message: " + ex.Message + '\n' +
                        "Exception stack trace: " + ex.StackTrace + '\n');
                }
            });
        }
        public async Task WriteLineAsync(string message, Task taskToWaitFor = null)
        {
            await Task.Run(() =>
            {
                if (taskToWaitFor != null) taskToWaitFor.Wait();
                Trace.WriteLine(DateTime.Now + ": " + message);
            });
        }
    }
}
