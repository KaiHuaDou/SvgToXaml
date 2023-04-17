using System;
using System.Runtime.InteropServices;

namespace SvgToXaml;
internal sealed class NativeMethods
{
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
        // if another type such as KeyEvent is to be used here, the struct must be adjusted accordingly
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
}
