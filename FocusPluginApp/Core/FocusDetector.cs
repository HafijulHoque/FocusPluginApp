using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace FocusPluginApp.Core
{
    public static class FocusDetector  // ✅ Ensure this class is static
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern int GetWindowThreadProcessId(IntPtr hWnd, out int processId);

        public static string? GetActiveProcessName()
        {
            IntPtr hWnd = GetForegroundWindow();
            if (hWnd == IntPtr.Zero) return null;

            GetWindowThreadProcessId(hWnd, out int processId);
            Process? process = Process.GetProcessById(processId);
            return process?.ProcessName.ToLower() + ".exe";
        }
    }
}
