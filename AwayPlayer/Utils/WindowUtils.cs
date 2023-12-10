using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class WindowUtils
{
    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    public static Rect GetGameWindowRect()
    {
        IntPtr hWnd = GetForegroundWindow();
        if (GetWindowRect(hWnd, out RECT windowRect))
        {
            int width = windowRect.Right - windowRect.Left;
            int height = windowRect.Bottom - windowRect.Top;
            return new Rect(0, 0, width, height);
        }

        // Default to Screen.width and Screen.height if window size retrieval fails
        return new Rect(0, 0, Screen.width, Screen.height);
    }
}
