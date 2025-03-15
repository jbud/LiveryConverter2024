using System.Diagnostics;

namespace LiveryConverter2024
{
    internal class ExeClass(MainWindow mainWindowRef)
    {
        public MainWindow mainWindowRef = mainWindowRef;

        private void ConsoleWriteLine(string line)
        {
            mainWindowRef.Dispatcher.Invoke(() =>
            {
                mainWindowRef.DebugConsole = line + "\n";
            });
        }

        public async Task SpawnProc(string proc, string args)
        {
            using Process? p2 = new Process();
            p2.StartInfo.FileName = proc;
            p2.StartInfo.Arguments = args;
            p2.StartInfo.RedirectStandardOutput = true;
            p2.StartInfo.CreateNoWindow = true;
            p2.StartInfo.UseShellExecute = false;
            p2.EnableRaisingEvents = true;
            p2.OutputDataReceived += (object sender, DataReceivedEventArgs args) =>
            {
                ConsoleWriteLine(" " + args.Data);
            };
            p2.Start();
            p2.BeginOutputReadLine();
            await p2.WaitForExitAsync();
        }
        public async Task ProcMon(string processName)
        {
            Thread.Sleep(2000);
            Process[]? processes = Process.GetProcessesByName(processName);
            foreach (Process p in processes)
            {
                ConsoleWriteLine("Found Process " + p.ProcessName + " Waiting for exit...");
                await p.WaitForExitAsync();
            }
        }
    }
}
