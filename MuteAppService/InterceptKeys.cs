﻿using Microsoft.Extensions.Logging;
using MuteAnyApp.Core.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace MuteAppService
{
    internal class InterceptKeys
    {
        public const int WH_KEYBOARD_LL = 13;
        public const int WM_KEYDOWN = 0x0100;
        public static LowLevelKeyboardProc _proc = HookCallback;
        public static IntPtr _hookID = IntPtr.Zero;
        private static ILogger logger = null;

        //public static void Main()
        //{
        //    _hookID = SetHook(_proc);
        //    //Application.Run();
        //    UnhookWindowsHookEx(_hookID);
        //}

        public static IntPtr SetHook(LowLevelKeyboardProc proc, ILogger _logger)
        {
            logger = _logger;
            var userLibrary = LoadLibrary("User32");
            return SetWindowsHookEx(WH_KEYBOARD_LL, proc, userLibrary, 0);

            //using (Process curProcess = Process.GetCurrentProcess())
            //using (ProcessModule curModule = curProcess.MainModule)
            //{
            //    return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
            //        GetModuleHandle(curModule.ModuleName), 0);
            //}
        }

        public delegate IntPtr LowLevelKeyboardProc(
            int nCode, IntPtr wParam, IntPtr lParam);

        public static IntPtr HookCallback(
            int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                //Console.WriteLine((Keys)vkCode);
                if (logger != null)
                {
                    logger.LogTrace("HOOKERD!");
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        /// <summary>
        /// Loads the library.
        /// </summary>
        /// <param name="lpFileName">Name of the library</param>
        /// <returns>A handle to the library</returns>
        [DllImport("kernel32.dll")]
        private static extern IntPtr LoadLibrary(string lpFileName);
    }
}
