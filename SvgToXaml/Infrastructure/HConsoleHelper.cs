using System;
using System.Runtime.InteropServices;

namespace SvgToXaml.Infrastructure;

/// <summary>
/// This class provides functionality to run from a regular WinForms or WPF application
/// optionally make a console application as well.
/// In principle, only InitConsoleHandles and later ReleaseConsoleHandles need to be called.
/// If the prog is started from the command line, it attaches to the existing Console,
/// otherwise (start via Windows) a temporary Console is created.
/// Here is an example for WPF (the BuildAction of App.xaml must also be set to 'Page')
/// (The CommandLineParser is also used here)
/// </summary>
/// <code>
/// private static int Main(string[] args)
/// {
///     int exitCode = 0;
///     bool commandLine = false;
///     CommandLineParser clp = new CommandLineParser
///     {
///         SkipCommandsWhenHelpRequested = true
///     };
///     if (args.Length > 0)
///     {
///         clp.Target = new CmdLineHandler( );
///         clp.Header = "HCNCInstHelper - Tool to register HRemoteCNCSLServer\r\n";
///         clp.LogErrorsToConsole = true;
///         clp.ParseArgs(args, false);
///         commandLine = clp.ArgsParsed;
///     }
///     if (commandLine)
///     {
///         HToolBox.HConsoleHelper.InitConsoleHandles( );
///         exitCode = clp.ExecCommands(true);
///         HToolBox.HConsoleHelper.ReleaseConsoleHandles( );
///     }
///     else
///     {
///         // normal WPF application logic
///         HCNCInstHelper.App app = new HCNCInstHelper.App( );
///         app.InitializeComponent( );
///         app.Run( );
///     }
///     return exitCode;
/// }
/// </code>
internal static class HConsoleHelper
{
    #region Functions Import
    [DllImport("kernel32.dll")]
    internal static extern bool AllocConsole( );

    [DllImport("kernel32.dll")]
    internal static extern bool AttachConsole(uint pid);

    [DllImport("kernel32.dll")]
    internal static extern bool FreeConsole( );

    [DllImport("kernel32", SetLastError = true)]
    internal static extern bool WriteConsoleInput(
        IntPtr hConsoleInput,
        KeyEventStruct[] lpBuffer,
        int nLength,
        ref int lpNumberOfEventsWritten);

    [DllImport("kernel32", SetLastError = true)]
    internal static extern IntPtr GetStdHandle(StandardHandle nStdHandle);
    #endregion

    #region Structs Import
    [StructLayout(LayoutKind.Explicit)]
    internal struct CHARUNION
    {
        [FieldOffset(0)]
        public byte AsciiChar;
        [FieldOffset(0)]
        public short UnicodeChar;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct KeyEventStruct
    {
        public InputEventType EventType;
        [MarshalAs(UnmanagedType.Bool)]
        //wenn hier ein anderer Typ wie KeyEvent verwendet werden soll, so muss der struct entsprechend angepasst werden
        public bool bKeyDown;
        public short wRepeatCount;
        public short wVirtualKeyCode;
        public short wVirtualScanCode;
        public CHARUNION uChar;
        public ControlKeyState dwControlKeyState;
    }

    [Flags]
    internal enum InputEventType
    {
        KeyEvent = 0x0001,
        MouseEvent = 0x0002,
        WindowBufferSizeEvent = 0x004,
        MenuEvent = 0x0008,
        FocusEvent = 0x0010,
    }

    [Flags]
    internal enum ControlKeyState
    {
        None = 0x0000,
        RightAltPressed = 0x0001,
        LeftAltPressed = 0x0002,
        RightCtrlPressed = 0x0004,
        LeftCtrlPressed = 0x0008,
        ShiftPressed = 0x0010,
        NumLockOn = 0x0020,
        ScrollLockOn = 0x0040,
        CapsLockOn = 0x0080,
        EnhancedKey = 0x0100,
    }

    internal enum StandardHandle
    {
        Input = -10,
        Output = -11,
        Error = -12,
    }
    #endregion

    private const uint ATTACH_PARENT_PROCESS = 0xFFFFFFFF;

    //true if attached - used to free it later
    public static bool ConsoleIsAttached { get; private set; }

    public static void InitConsoleHandles( )
    {
        // Attach to console window – this may modify the standard handles
        if (AttachConsole(ATTACH_PARENT_PROCESS))
            ConsoleIsAttached = true;
        else AllocConsole( );
    }

    public static void ReleaseConsoleHandles( )
    {
        if (ConsoleIsAttached)
        {
            ///<summary>
            /// Unfortunately, in the case of AttachConsole (to an existing console), an Enter is expected at the end
            /// this is a known problem on the net, you just have to simulate an enter
            /// there are various methods for this:
            /// <code>
            /// System.Windows.Forms.SendKeys.SendWait("{ENTER}"); // No WinForms
            /// Process.GetCurrentProcess().Kill(); // doesn't work
            /// HToolBox.HSendKeys.Send("\r"); // doesn't work too
            /// </code>
            ///</summary>
            ///
            SendEnterToConsoleInput( ); // Cleanest solution
        }
        else
        {
            FreeConsole( ); // Same as AllocConsole
        }
    }

    /// <summary>
    /// Puts an "enter" into the std input
    /// </summary>
    public static void SendEnterToConsoleInput( )
    {
        IntPtr stdIn = GetStdHandle(StandardHandle.Input);
        int eventsWritten = 0;
        KeyEventStruct[] data = new KeyEventStruct[]
        {
            new KeyEventStruct
            {
                EventType = InputEventType.KeyEvent,
                bKeyDown = true,
                uChar = new CHARUNION { AsciiChar = 13 },
                dwControlKeyState = 0,
                wRepeatCount = 1,
                wVirtualKeyCode = 0,
                wVirtualScanCode = 0
            }
        };
        WriteConsoleInput(stdIn, data, 1, ref eventsWritten);
    }
}
