using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace WindowMaximizer;

public class WindowInfo
{
    public nint Handle { get; init; }
    public string Title { get; init; } = "";
    public uint ProcessId { get; init; }

    public string ProcessName { get; init; } = "";
    public string ExecutableName { get; init; } = "";
    public string ExecutablePath { get; init; } = "";

    public int X { get; init; }
    public int Y { get; init; }
    public int Width { get; init; }
    public int Height { get; init; }

    public long WindowStyle { get; init; }
    public long ExtendedWindowStyle { get; init; }

    public bool IsResizable { get; init; }
    public bool IsMinimizable { get; init; }
    public bool IsMaximizable { get; init; }
    public bool IsMinimized { get; init; }
    public bool IsMaximized { get; init; }
}

public static class WindowManager
{
    // =========================================================
    // DPI
    // =========================================================

    private static readonly nint DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2 = new(-4);

    [DllImport("user32.dll")]
    private static extern bool SetProcessDpiAwarenessContext(
        nint dpiContext);

    public static bool EnableDpiAwareness()
    {
        return SetProcessDpiAwarenessContext(
            DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2);
    }

    // =========================================================
    // Window enumeration
    // =========================================================

    private delegate bool EnumWindowsProc(
        nint hWnd,
        nint lParam);

    [DllImport("user32.dll")]
    private static extern bool EnumWindows(
        EnumWindowsProc lpEnumFunc,
        nint lParam);

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(
        nint hWnd);

    // =========================================================
    // Window title
    // =========================================================

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowText(
        nint hWnd,
        StringBuilder lpString,
        int nMaxCount);

    // =========================================================
    // Process
    // =========================================================

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(
        nint hWnd,
        out uint processId);

    // =========================================================
    // RECT
    // =========================================================

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    // =========================================================
    // Window position and size
    // =========================================================

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(
        nint hWnd,
        out RECT lpRect);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowPos(
        nint hWnd,
        nint hWndInsertAfter,
        int X,
        int Y,
        int cx,
        int cy,
        uint uFlags);

    private const uint SWP_NOZORDER = 0x0004;
    private const uint SWP_NOACTIVATE = 0x0010;

    // =========================================================
    // Window state
    // =========================================================

    [DllImport("user32.dll")]
    private static extern bool IsIconic(
        nint hWnd);

    [DllImport("user32.dll")]
    private static extern bool IsZoomed(
        nint hWnd);

    // =========================================================
    // Window style
    // =========================================================

    private const int GWL_STYLE = -16;
    private const int GWL_EXSTYLE = -20;

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
    private static extern nint GetWindowLongPtr(
        nint hWnd,
        int nIndex);

    // Window styles
    private const long WS_THICKFRAME = 0x00040000L;
    private const long WS_MINIMIZEBOX = 0x00020000L;
    private const long WS_MAXIMIZEBOX = 0x00010000L;

    // =========================================================
    // DWM - visible bounds
    // =========================================================

    private const int DWMWA_EXTENDED_FRAME_BOUNDS = 9;

    [DllImport("dwmapi.dll")]
    private static extern int DwmGetWindowAttribute(
        nint hWnd,
        int dwAttribute,
        out RECT pvAttribute,
        int cbAttribute);

    // =========================================================
    // Monitor
    // =========================================================

    [StructLayout(LayoutKind.Sequential)]
    private struct MONITORINFO
    {
        public int CbSize;
        public RECT RcMonitor;
        public RECT RcWork;
        public uint DwFlags;
    }

    [DllImport("user32.dll")]
    private static extern nint MonitorFromWindow(
        nint hWnd,
        uint dwFlags);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool GetMonitorInfo(
        nint hMonitor,
        ref MONITORINFO lpmi);

    private const uint MONITOR_DEFAULTTONEAREST = 2;

    public class MonitorInfo
    {
        public int X { get; init; }
        public int Y { get; init; }
        public int Width { get; init; }
        public int Height { get; init; }

        public int WorkX { get; init; }
        public int WorkY { get; init; }
        public int WorkWidth { get; init; }
        public int WorkHeight { get; init; }
    }

    // =========================================================
    // List open windows
    // =========================================================

    public static List<WindowInfo> GetOpenWindows()
    {
        var windows = new List<WindowInfo>();

        EnumWindows((hWnd, _) =>
        {
            if (!IsWindowVisible(hWnd))
                return true;

            var title = GetWindowTitle(hWnd);

            if (string.IsNullOrWhiteSpace(title))
                return true;

            GetWindowThreadProcessId(
                hWnd,
                out uint processId);

            GetWindowRect(
                hWnd,
                out var rect);

            long windowStyle =
                GetWindowLongPtr(
                    hWnd,
                    GWL_STYLE).ToInt64();

            long extendedWindowStyle =
                GetWindowLongPtr(
                    hWnd,
                    GWL_EXSTYLE).ToInt64();

            string processName = "";
            string executableName = "";
            string executablePath = "";

            try
            {
                using var process =
                    Process.GetProcessById((int)processId);

                processName = process.ProcessName;

                try
                {
                    executablePath =
                        process.MainModule?.FileName ?? "";

                    if (!string.IsNullOrWhiteSpace(executablePath))
                    {
                        executableName =
                            Path.GetFileName(executablePath);
                    }
                }
                catch
                {
                    // Some protected processes may not allow
                    // access to MainModule.
                }
            }
            catch
            {
                // The process may have exited during
                // window enumeration.
            }

            windows.Add(new WindowInfo
            {
                Handle = hWnd,
                Title = title,
                ProcessId = processId,

                ProcessName = processName,
                ExecutableName = executableName,
                ExecutablePath = executablePath,

                X = rect.Left,
                Y = rect.Top,
                Width = rect.Right - rect.Left,
                Height = rect.Bottom - rect.Top,

                WindowStyle = windowStyle,
                ExtendedWindowStyle = extendedWindowStyle,

                IsResizable =
                    (windowStyle & WS_THICKFRAME) != 0,

                IsMinimizable =
                    (windowStyle & WS_MINIMIZEBOX) != 0,

                IsMaximizable =
                    (windowStyle & WS_MAXIMIZEBOX) != 0,

                IsMinimized = IsIconic(hWnd),
                IsMaximized = IsZoomed(hWnd)
            });

            return true;

        }, nint.Zero);

        return windows;
    }

    private static string GetWindowTitle(
        nint hWnd)
    {
        var builder =
            new StringBuilder(512);

        GetWindowText(
            hWnd,
            builder,
            builder.Capacity);

        return builder.ToString();
    }

    // =========================================================
    // Resize window
    // =========================================================

    public static bool SetWindowBounds(
        WindowInfo window,
        int x,
        int y,
        int width,
        int height)
    {
        return SetWindowPos(
            window.Handle,
            nint.Zero,
            x,
            y,
            width,
            height,
            SWP_NOZORDER | SWP_NOACTIVATE);
    }

    // =========================================================
    // Visible window bounds
    // =========================================================

    public static bool GetVisibleWindowBounds(
        WindowInfo window,
        out int x,
        out int y,
        out int width,
        out int height)
    {
        x = 0;
        y = 0;
        width = 0;
        height = 0;

        int result =
            DwmGetWindowAttribute(
                window.Handle,
                DWMWA_EXTENDED_FRAME_BOUNDS,
                out var rect,
                Marshal.SizeOf<RECT>());

        if (result != 0)
            return false;

        x = rect.Left;
        y = rect.Top;
        width = rect.Right - rect.Left;
        height = rect.Bottom - rect.Top;

        return true;
    }

    // =========================================================
    // Window monitor
    // =========================================================

    public static MonitorInfo? GetMonitorInfo(
        WindowInfo window)
    {
        var hMonitor =
            MonitorFromWindow(
                window.Handle,
                MONITOR_DEFAULTTONEAREST);

        if (hMonitor == nint.Zero)
            return null;

        var monitorInfo =
            new MONITORINFO
            {
                CbSize =
                    Marshal.SizeOf<MONITORINFO>()
            };

        if (!GetMonitorInfo(
                hMonitor,
                ref monitorInfo))
        {
            return null;
        }

        var monitor =
            monitorInfo.RcMonitor;

        var work =
            monitorInfo.RcWork;

        return new MonitorInfo
        {
            X = monitor.Left,
            Y = monitor.Top,
            Width = monitor.Right - monitor.Left,
            Height = monitor.Bottom - monitor.Top,

            WorkX = work.Left,
            WorkY = work.Top,
            WorkWidth =
                work.Right - work.Left,
            WorkHeight =
                work.Bottom - work.Top
        };
    }

    public static bool MaximizeWindow(
        WindowInfo window,
        out string message)
    {
        message = "";

        var monitor =
            GetMonitorInfo(window);

        if (monitor == null)
        {
            message =
                "Could not identify the window's monitor.";

            return false;
        }

        int targetX =
            monitor.WorkX;

        int targetY =
            monitor.WorkY;

        int targetWidth =
            monitor.WorkWidth;

        int targetHeight =
            monitor.WorkHeight;

        int targetRight =
            targetX + targetWidth;

        int targetBottom =
            targetY + targetHeight;

        bool success =
            SetWindowBounds(
                window,
                targetX,
                targetY,
                targetWidth,
                targetHeight);

        if (!success)
        {
            message =
                "Could not resize the window.";

            return false;
        }

        for (int attempt = 0;
            attempt < 3;
            attempt++)
        {
            if (!GetWindowRect(
                    window.Handle,
                    out var rect))
            {
                message =
                    "Could not get the window bounds.";

                return false;
            }

            int currentX =
                rect.Left;

            int currentY =
                rect.Top;

            int currentWidth =
                rect.Right - rect.Left;

            int currentHeight =
                rect.Bottom - rect.Top;

            if (!GetVisibleWindowBounds(
                    window,
                    out int visibleX,
                    out int visibleY,
                    out int visibleWidth,
                    out int visibleHeight))
            {
                message =
                    "Could not get the visible window bounds.";

                return false;
            }

            int visibleRight =
                visibleX + visibleWidth;

            int visibleBottom =
                visibleY + visibleHeight;

            int deltaLeft =
                targetX - visibleX;

            int deltaTop =
                targetY - visibleY;

            int deltaRight =
                targetRight - visibleRight;

            int deltaBottom =
                targetBottom - visibleBottom;

            bool exact =
                deltaLeft == 0 &&
                deltaTop == 0 &&
                deltaRight == 0 &&
                deltaBottom == 0;

            if (exact)
                break;

            int correctedX =
                currentX + deltaLeft;

            int correctedY =
                currentY + deltaTop;

            int correctedWidth =
                currentWidth +
                deltaRight -
                deltaLeft;

            int correctedHeight =
                currentHeight +
                deltaBottom -
                deltaTop;

            success =
                SetWindowBounds(
                    window,
                    correctedX,
                    correctedY,
                    correctedWidth,
                    correctedHeight);

            if (!success)
            {
                message =
                    "Could not apply the correction.";

                return false;
            }
        }

        if (!GetVisibleWindowBounds(
                window,
                out int finalX,
                out int finalY,
                out int finalWidth,
                out int finalHeight))
        {
            message =
                "The window was adjusted, but its visible bounds " +
                "could not be verified.";

            return false;
        }

        int finalRight =
            finalX + finalWidth;

        int finalBottom =
            finalY + finalHeight;

        bool finalExact =
            finalX == targetX &&
            finalY == targetY &&
            finalRight == targetRight &&
            finalBottom == targetBottom;

        if (!finalExact)
        {
            message =
                $"The window was resized, but there is still " +
                $"a difference in the visible bounds. " +
                $"Expected: {targetX}, {targetY} " +
                $"{targetWidth}x{targetHeight}. " +
                $"Actual: {finalX}, {finalY} " +
                $"{finalWidth}x{finalHeight}.";

            return false;
        }

        message =
            $"Work area: {targetX}, {targetY} " +
            $"{targetWidth}x{targetHeight}. " +
            $"Visible bounds: {finalX}, {finalY} " +
            $"{finalWidth}x{finalHeight}.";

        return true;
    }
}