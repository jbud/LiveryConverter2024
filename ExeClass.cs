using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveryConverter2024
{
    internal class ExeClass
    {
        public async Task SpawnProc(string proc, string args, MainWindow dbg)
        {
            using (Process? p2 = new Process())
            {
                p2.StartInfo.FileName = proc;
                p2.StartInfo.Arguments = args;
                p2.StartInfo.RedirectStandardOutput = true;
                p2.StartInfo.CreateNoWindow = true;
                p2.StartInfo.UseShellExecute = false;
                p2.EnableRaisingEvents = true;
                p2.OutputDataReceived += (object sender, DataReceivedEventArgs args) => 
                {
                    dbg.Dispatcher.Invoke(() =>
                    {
                        dbg.DebugConsole = " " + args.Data + "\n";
                    });
                    
                };
                p2.Start();
                p2.BeginOutputReadLine();
                await p2.WaitForExitAsync();
            }
        }
        public async Task ProcMon(string processName, MainWindow dbg)
        {
            Thread.Sleep(2000);
            Process[]? processes = Process.GetProcessesByName(processName);
            foreach (Process p in processes)
            {
                dbg.Dispatcher.Invoke(() =>
                {
                    dbg.DebugConsole = "Found Process " + p.ProcessName + " Waiting for exit...\n";
                });
                await p.WaitForExitAsync();
                
            }
        }
    }
}
