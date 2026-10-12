using WindowMaximizer;

WindowManager.EnableDpiAwareness();

// =========================================================
// --help
// =========================================================

static void ShowHelp()
{
    Console.WriteLine(
        "Window Maximizer 0.1");

    Console.WriteLine();

    Console.WriteLine(
        "Usage:");

    Console.WriteLine(
        "  WindowMaximizer.exe --list");

    Console.WriteLine(
        "  WindowMaximizer.exe --info <index>");

    Console.WriteLine(
        "  WindowMaximizer.exe --set <index> <x> <y> <width> <height>");

    Console.WriteLine(
        "  WindowMaximizer.exe --maximize <index>");

    Console.WriteLine();

    Console.WriteLine(
        "Commands:");

    Console.WriteLine(
        "  --list");
    Console.WriteLine(
        "      Lists open windows.");

    Console.WriteLine(
        "  --info <index>");
    Console.WriteLine(
        "      Displays detailed information about a window.");

    Console.WriteLine(
        "  --set <index> <x> <y> <width> <height>");
    Console.WriteLine(
        "      Sets the window position and size manually.");

    Console.WriteLine(
        "  --maximize <index>");
    Console.WriteLine(
        "      Fills the monitor work area while preserving " +
        "the window's appearance.");
}

if (args.Length == 0)
{
    ShowHelp();
    return;
}

if (args[0].Equals(
        "--help",
        StringComparison.OrdinalIgnoreCase) ||
    args[0].Equals(
        "-h",
        StringComparison.OrdinalIgnoreCase) ||
    args[0].Equals(
        "/?",
        StringComparison.OrdinalIgnoreCase))
{
    ShowHelp();
    return;
}

// =========================================================
// --list
// =========================================================

if (args[0].Equals(
        "--list",
        StringComparison.OrdinalIgnoreCase))
{
    var windows =
        WindowManager.GetOpenWindows();

    Console.WriteLine(
        "Window Maximizer 0.1");

    Console.WriteLine();

    Console.WriteLine(
        "Windows found:");

    Console.WriteLine();

    for (int i = 0; i < windows.Count; i++)
    {
        var window =
            windows[i];

        Console.WriteLine(
            $"[{i + 1}] {window.Title}");

        Console.WriteLine(
            $"    Executable: {window.ExecutableName}");

        Console.WriteLine(
            $"    PID: {window.ProcessId}");

        Console.WriteLine(
            $"    Position: {window.X}, {window.Y}");

        Console.WriteLine(
            $"    Size: {window.Width}x{window.Height}");

        Console.WriteLine();
    }

    return;
}

// =========================================================
// --info
// =========================================================

if (args.Length >= 2 &&
    args[0].Equals(
        "--info",
        StringComparison.OrdinalIgnoreCase))
{
    if (!int.TryParse(
            args[1],
            out int index))
    {
        Console.WriteLine(
            "Invalid index.");

        return;
    }

    var windows =
        WindowManager.GetOpenWindows();

    if (index < 1 ||
        index > windows.Count)
    {
        Console.WriteLine(
            "Window not found.");

        return;
    }

    var window =
        windows[index - 1];

    var monitor =
        WindowManager.GetMonitorInfo(
            window);

    Console.WriteLine(
        $"Title:          {window.Title}");

    Console.WriteLine(
        $"Process:        {window.ProcessName}");

    Console.WriteLine(
        $"Executable:     {window.ExecutableName}");

    Console.WriteLine(
        $"Path:           {window.ExecutablePath}");

    Console.WriteLine(
        $"PID:            {window.ProcessId}");

    Console.WriteLine(
        $"HWND:           0x{window.Handle.ToInt64():X}");

    Console.WriteLine();

    Console.WriteLine(
        $"Position:       {window.X}, {window.Y}");

    Console.WriteLine(
        $"Size:           {window.Width}x{window.Height}");

    Console.WriteLine();

    Console.WriteLine(
        $"Resizable:      {(window.IsResizable ? "Yes" : "No")}");

    Console.WriteLine(
        $"Minimizable:    {(window.IsMinimizable ? "Yes" : "No")}");

    Console.WriteLine(
        $"Maximizable:    {(window.IsMaximizable ? "Yes" : "No")}");

    Console.WriteLine(
        $"Minimized:      {(window.IsMinimized ? "Yes" : "No")}");

    Console.WriteLine(
        $"Maximized:      {(window.IsMaximized ? "Yes" : "No")}");

    Console.WriteLine();

    Console.WriteLine(
        $"Style:          0x{window.WindowStyle:X}");

    Console.WriteLine(
        $"Extended style: 0x{window.ExtendedWindowStyle:X}");

    if (monitor != null)
    {
        Console.WriteLine();

        Console.WriteLine(
            "Monitor:");

        Console.WriteLine(
            $"    {monitor.X}, {monitor.Y} " +
            $"{monitor.Width}x{monitor.Height}");

        Console.WriteLine(
            "Work area:");

        Console.WriteLine(
            $"    {monitor.WorkX}, {monitor.WorkY} " +
            $"{monitor.WorkWidth}x{monitor.WorkHeight}");
    }

    if (WindowManager.GetVisibleWindowBounds(
            window,
            out int visibleX,
            out int visibleY,
            out int visibleWidth,
            out int visibleHeight))
    {
        Console.WriteLine();

        Console.WriteLine(
            "Visible bounds:");

        Console.WriteLine(
            $"    {visibleX}, {visibleY} " +
            $"{visibleWidth}x{visibleHeight}");
    }

    return;
}

// =========================================================
// --set
// =========================================================

if (args.Length >= 6 &&
    args[0].Equals(
        "--set",
        StringComparison.OrdinalIgnoreCase))
{
    if (!int.TryParse(args[1], out int index) ||
        !int.TryParse(args[2], out int x) ||
        !int.TryParse(args[3], out int y) ||
        !int.TryParse(args[4], out int width) ||
        !int.TryParse(args[5], out int height))
    {
        Console.WriteLine(
            "Invalid parameters.");

        return;
    }

    if (width <= 0 ||
        height <= 0)
    {
        Console.WriteLine(
            "Width and height must be greater than zero.");

        return;
    }

    var windows =
        WindowManager.GetOpenWindows();

    if (index < 1 ||
        index > windows.Count)
    {
        Console.WriteLine(
            "Window not found.");

        return;
    }

    var window =
        windows[index - 1];

    bool success =
        WindowManager.SetWindowBounds(
            window,
            x,
            y,
            width,
            height);

    Console.WriteLine(
        success
            ? "Window resized successfully."
            : "Could not resize the window.");

    return;
}

// =========================================================
// --maximize
// =========================================================

if (args.Length >= 2 &&
    args[0].Equals(
        "--maximize",
        StringComparison.OrdinalIgnoreCase))
{
    if (!int.TryParse(
            args[1],
            out int index))
    {
        Console.WriteLine(
            "Invalid index.");

        return;
    }

    var windows =
        WindowManager.GetOpenWindows();

    if (index < 1 ||
        index > windows.Count)
    {
        Console.WriteLine(
            "Window not found.");

        return;
    }

    var window =
        windows[index - 1];

    Console.WriteLine(
        "Maximizing window...");

    Console.WriteLine();

    bool success =
        WindowManager.MaximizeWindow(
            window,
            out string message);

    Console.WriteLine(
        message);

    Console.WriteLine();

    Console.WriteLine(
        success
            ? "Window maximized successfully."
            : "Could not maximize the window.");

    return;
}

// =========================================================
// Incomplete commands
// =========================================================

if (args[0].Equals(
        "--info",
        StringComparison.OrdinalIgnoreCase) ||
    args[0].Equals(
        "--maximize",
        StringComparison.OrdinalIgnoreCase))
{
    Console.WriteLine(
        $"Usage: WindowMaximizer.exe {args[0]} <index>");

    return;
}

if (args[0].Equals(
        "--set",
        StringComparison.OrdinalIgnoreCase))
{
    Console.WriteLine(
        "Usage: WindowMaximizer.exe " +
        "--set <index> <x> <y> <width> <height>");

    return;
}

// =========================================================
// Unknown command
// =========================================================

Console.WriteLine(
    $"Unknown command: {args[0]}");

Console.WriteLine();

ShowHelp();