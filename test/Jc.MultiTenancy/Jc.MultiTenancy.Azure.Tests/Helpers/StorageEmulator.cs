using System.Diagnostics;
using System.Linq;

namespace Jc.MultiTenancy.Azure.Tests.Helpers
{
    public static class StorageEmulator
    {
        private const string command = @"C:\Program Files (x86)\Microsoft SDKs\Azure\Storage Emulator\AzureStorageEmulator.exe";


        public static Process Emulator { get; set; }

        public static void Start()
        {
            // Check if emulator already running
            var processes = Process.GetProcesses().OrderBy(p => p.ProcessName).ToList();
            if (processes.Any(process => process.ProcessName.Contains("DSServiceLDB")))
                return;

            Emulator = Process.Start(command, "start");
        }

        public static void Clear()
            => Process.Start(command, "clear all").WaitForExit();
    }
}
