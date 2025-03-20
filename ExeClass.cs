using System.Diagnostics;

namespace LiveryConverter2024
{
    internal class ExeClass(MainWindow mainWindowRef)
    {
        public MainWindow mainWindowRef = mainWindowRef;

        /// <summary>
        /// Forward to debug console in MainWindow
        /// </summary>
        /// <param name="line"></param>
        private void ConsoleWriteLine(string line)
        {
            mainWindowRef.Dispatcher.Invoke(() =>
            {
                mainWindowRef.DebugConsole = line + "\n";
            });
        }

        /// <summary>
        /// Spawn a process to be handled by a background thread. Process will spawn with no window and redirect stdout to debug console in MainWindow
        /// </summary>
        /// <param name="proc">path to filename for the process to spawn</param>
        /// <param name="args">arguments to the spawned process</param>
        /// <param name="sync">**overload** spawn synchronous process with special handling for "press any key to exit..." action requiremnents. (default: false)</param>
        /// <returns></returns>
        public async Task SpawnProc(string proc, string args, bool sync = false)
        {
            using Process? p2 = new Process();
            p2.StartInfo.FileName = proc;
            p2.StartInfo.Arguments = args;
            p2.StartInfo.RedirectStandardOutput = true;
            if (sync)
            {
                p2.StartInfo.RedirectStandardInput = true;
            }
            p2.StartInfo.CreateNoWindow = true;
            p2.StartInfo.UseShellExecute = false;
            p2.EnableRaisingEvents = true;
            if (!sync)
            {
                p2.OutputDataReceived += (object sender, DataReceivedEventArgs args) =>
                {
                    ConsoleWriteLine(" " + args.Data);
                };
            }
            p2.Start();
            if (!sync)
            {
                p2.BeginOutputReadLine();
            }
            if (sync) 
            {
                string outp = p2.StandardOutput.ReadToEnd();
                ConsoleWriteLine(outp); // force read console.
                if (outp.Contains("Press any key to exit...")) // Hardcoded for only sync proc.
                {
                    mainWindowRef.GeneralError = true;
                }
                p2.WaitForExit();
            }
            else
            {
                await p2.WaitForExitAsync();
            }
        }

        /// <summary>
        /// Find a background process and wait for it to complete
        /// </summary>
        /// <param name="processName">the name of the process (Ignoring path or extension)</param>
        /// <returns></returns>
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
