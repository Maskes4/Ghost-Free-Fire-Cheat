using Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace External
{
    internal class GhostGoomen
    {
        class Ativar
        {
            private string pid = "";
            private readonly string Bluestack = "HD-Player";
            private readonly string Bluestack2 = "HD-Player.exe";
            private readonly string Memu = "MEmuHeadless";
            private readonly string Gameloop = "aow_exe";
            private readonly string SmartGaGA = "AndroidProcess.exe";
            private readonly string LD = "LdVBoxHeadless.exe";

            public bool IsAlreadyRunning { get; private set; } = false;
            public bool IsAlreadyRunning2 { get; private set; } = false;

            public Imps Dc { get; } = new Imps();
            public Mem MemLib { get; } = new Mem();
            private static string string_0;

            private async Task PutTaskDelayAsync(int time)
            {
                await Task.Delay(time);
            }

            private async Task<IntPtr> BoltPanelAsync(string cod, string cod2, string valor)
            {
                IntPtr intPtr = IntPtr.Zero;
                uint num = 0u;
                IntPtr snapshotHandle = CreateToolhelp32Snapshot(2u, 0u);

                if ((int)snapshotHandle > 0)
                {
                    ProcessEntry32 processEntry = default;
                    processEntry.dwSize = (uint)Marshal.SizeOf(processEntry);

                    for (int isFirstProcess = Process32First(snapshotHandle, ref processEntry); isFirstProcess == 1; isFirstProcess = Process32Next(snapshotHandle, ref processEntry))
                    {
                        if (ProcessMatchesCriteria(processEntry, ref num))
                        {
                            intPtr = (IntPtr)processEntry.th32ProcessID;
                        }
                    }

                    pid = intPtr.ToString();
                    await PutTaskDelayAsync(600);

                    if (valor == "1")
                    {
                        ApplyAobAsync(cod, cod2);
                    }
                    else
                    {
                        ApplyAob2Async(cod, cod2);
                    }
                }
                return intPtr;
            }

            private bool ProcessMatchesCriteria(ProcessEntry32 processEntry, ref uint maxThreadCount)
            {
                bool matches = false;

                if ((processEntry.szExeFile.Contains(Bluestack) || processEntry.szExeFile.Contains(Bluestack2) ||
                    processEntry.szExeFile.Contains(Memu) || processEntry.szExeFile.Contains(Gameloop) ||
                    processEntry.szExeFile.Contains(SmartGaGA) || processEntry.szExeFile.Contains(LD)) &&
                    processEntry.cntThreads > maxThreadCount)
                {
                    maxThreadCount = processEntry.cntThreads;
                    matches = true;
                }

                return matches;
            }

            public async void ApplyAsync(string cod, string cod2, string valor)
            {
                await BoltPanelAsync(cod, cod2, valor);
            }

            public Ativar(string cod, string cod2, string valor)
            {
                ApplyAsync(cod, cod2, valor);
            }

            private async Task ApplyAobAsync(string search, string replace)
            {
                await ApplyMemoryChangesAsync(search, replace, IsAlreadyRunning, setRunningState: true);
            }

            private async Task ApplyAob2Async(string search, string replace)
            {
                await ApplyMemoryChangesAsync(search, replace, IsAlreadyRunning2, setRunningState: false);
            }

            private async Task ApplyMemoryChangesAsync(string search, string replace, bool isRunning, bool setRunningState)
            {
                isRunning = true;

                if (Convert.ToInt32(pid) == 0)
                {
                    // Alert: Open Emulator!
                    return;
                }

                MemLib.OpenProcess(Convert.ToInt32(pid));
                IEnumerable<long> addresses = await MemLib.AoBScan(0L, 140737488355327L, search, writable: true, executable: true);

                if (addresses.Any())
                {
                    foreach (var address in addresses)
                    {
                        MemLib.WriteMemory(address.ToString("X"), "bytes", replace);
                    }
                    // Alert: Activation Success
                }
                else
                {
                    // Alert: Application Failed - try again!
                }

                MemLib.CloseProcess();
                isRunning = setRunningState;
            }

            private async Task ChangeMemoryAsync(string search, string replace)
            {
                await ChangeMemoryCoreAsync(search, replace);
            }

            private async Task ChangeMemory2Async(string search, string replace)
            {
                await ChangeMemoryCoreAsync(search, replace);
            }

            private async Task ChangeMemoryCoreAsync(string search, string replace)
            {
                bool success = false;

                if (Convert.ToInt32(pid) == 0)
                {
                    // Alert: Open Emulator!
                    return;
                }

                MemLib.OpenProcess(Convert.ToInt32(pid));
                IEnumerable<long> addresses = await MemLib.AoBScan(0L, 140737488355327L, search, true, true, string.Empty);
                string_0 = "0x" + addresses.FirstOrDefault().ToString("X");

                MemLib.ChangeProtection(string_0, Imps.MemoryProtection.ReadWrite, out _);

                foreach (var address in addresses)
                {
                    MemLib.WriteMemory(address.ToString("X"), "bytes", replace, string.Empty, null);
                    success = true;
                }

                if (success)
                {
                    // Alert: Activation Success
                }
                else
                {
                    // Alert: Application Failed - try again!
                }

                MemLib.ChangeProtection(string_0, Imps.MemoryProtection.ReadOnly, out _);
                MemLib.CloseProcess();
            }

            [DllImport("KERNEL32.DLL")]
            public static extern IntPtr CreateToolhelp32Snapshot(uint flags, uint processid);

            [DllImport("KERNEL32.DLL")]
            public static extern int Process32First(IntPtr handle, ref ProcessEntry32 pe);

            [DllImport("KERNEL32.DLL")]
            public static extern int Process32Next(IntPtr handle, ref ProcessEntry32 pe);

            public struct ProcessEntry32
            {
                public uint dwSize;
                public uint cntUsage;
                public uint th32ProcessID;
                public IntPtr th32DefaultHealabel1;
                public uint th32ModuleID;
                public uint cntThreads;
                public uint th32ParentProcessID;
                public int pcPriClassBase;
                public uint dwFlags;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
                public string szExeFile;
            }
        }
    }
}
