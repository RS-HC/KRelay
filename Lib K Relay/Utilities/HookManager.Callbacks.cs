using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Lib_K_Relay.Utilities
{
    public static partial class HookManager
    {
        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr LoadLibrary(string fileName);

        private delegate int HookProc(int nCode, int wParam, IntPtr lParam);

        #region Mouse hook processing

        private static HookProc s_MouseDelegate;

        private static int s_MouseHookHandle;

        private static int m_OldX;
        private static int m_OldY;

        private static int MouseHookProc(int nCode, int wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                MouseLLHookStruct mouseHookStruct = (MouseLLHookStruct)Marshal.PtrToStructure(lParam, typeof(MouseLLHookStruct));

                MouseButtons button = MouseButtons.None;
                short mouseDelta = 0;
                int clickCount = 0;
                bool mouseDown = false;
                bool mouseUp = false;

                switch (wParam)
                {
                    case WM_LBUTTONDOWN:
                        mouseDown = true;
                        button = MouseButtons.Left;
                        clickCount = 1;
                        break;

                    case WM_LBUTTONUP:
                        mouseUp = true;
                        button = MouseButtons.Left;
                        clickCount = 1;
                        break;

                    case WM_LBUTTONDBLCLK:
                        button = MouseButtons.Left;
                        clickCount = 2;
                        break;

                    case WM_RBUTTONDOWN:
                        mouseDown = true;
                        button = MouseButtons.Right;
                        clickCount = 1;
                        break;

                    case WM_RBUTTONUP:
                        mouseUp = true;
                        button = MouseButtons.Right;
                        clickCount = 1;
                        break;

                    case WM_RBUTTONDBLCLK:
                        button = MouseButtons.Right;
                        clickCount = 2;
                        break;

                    case WM_MOUSEWHEEL:
                        mouseDelta = (short)((mouseHookStruct.MouseData >> 16) & 0xffff);

                        break;
                }

                MouseEventExtArgs e = new MouseEventExtArgs(
                                                   button,
                                                   clickCount,
                                                   mouseHookStruct.Point.X,
                                                   mouseHookStruct.Point.Y,
                                                   mouseDelta);

                if (s_MouseUp != null && mouseUp)
                {
                    s_MouseUp.Invoke(null, e);
                }

                if (s_MouseDown != null && mouseDown)
                {
                    s_MouseDown.Invoke(null, e);
                }

                if (s_MouseClick != null && clickCount > 0)
                {
                    s_MouseClick.Invoke(null, e);
                }

                if (s_MouseClickExt != null && clickCount > 0)
                {
                    s_MouseClickExt.Invoke(null, e);
                }

                if (s_MouseDoubleClick != null && clickCount == 2)
                {
                    s_MouseDoubleClick.Invoke(null, e);
                }

                if (s_MouseWheel != null && mouseDelta != 0)
                {
                    s_MouseWheel.Invoke(null, e);
                }

                if ((s_MouseMove != null || s_MouseMoveExt != null) && (m_OldX != mouseHookStruct.Point.X || m_OldY != mouseHookStruct.Point.Y))
                {
                    m_OldX = mouseHookStruct.Point.X;
                    m_OldY = mouseHookStruct.Point.Y;
                    if (s_MouseMove != null)
                    {
                        s_MouseMove.Invoke(null, e);
                    }

                    if (s_MouseMoveExt != null)
                    {
                        s_MouseMoveExt.Invoke(null, e);
                    }
                }

                if (e.Handled)
                {
                    return -1;
                }
            }

            return CallNextHookEx(s_MouseHookHandle, nCode, wParam, lParam);
        }

        private static void EnsureSubscribedToGlobalMouseEvents()
        {
            if (s_MouseHookHandle == 0)
            {
                s_MouseDelegate = MouseHookProc;
                s_MouseHookHandle = SetWindowsHookEx(
                    WH_MOUSE_LL,
                    s_MouseDelegate,
                    LoadLibrary("User32")
                                ,
                    0);
                if (s_MouseHookHandle == 0)
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    throw new Win32Exception(errorCode);
                }
            }
        }

        private static void TryUnsubscribeFromGlobalMouseEvents()
        {
            if (s_MouseClick == null &&
                s_MouseDown == null &&
                s_MouseMove == null &&
                s_MouseUp == null &&
                s_MouseClickExt == null &&
                s_MouseMoveExt == null &&
                s_MouseWheel == null)
            {
                ForceUnsunscribeFromGlobalMouseEvents();
            }
        }

        private static void ForceUnsunscribeFromGlobalMouseEvents()
        {
            if (s_MouseHookHandle != 0)
            {
                int result = UnhookWindowsHookEx(s_MouseHookHandle);
                s_MouseHookHandle = 0;
                s_MouseDelegate = null;
                if (result == 0)
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    throw new Win32Exception(errorCode);
                }
            }
        }

        #endregion Mouse hook processing

        #region Keyboard hook processing

        private static HookProc s_KeyboardDelegate;

        private static int s_KeyboardHookHandle;

        private static int KeyboardHookProc(int nCode, Int32 wParam, IntPtr lParam)
        {
            bool handled = false;

            if (nCode >= 0)
            {
                KeyboardHookStruct MyKeyboardHookStruct = (KeyboardHookStruct)Marshal.PtrToStructure(lParam, typeof(KeyboardHookStruct));
                if (s_KeyDown != null && (wParam == WM_KEYDOWN || wParam == WM_SYSKEYDOWN))
                {
                    Keys keyData = (Keys)MyKeyboardHookStruct.VirtualKeyCode;
                    KeyEventArgs e = new KeyEventArgs(keyData);
                    s_KeyDown.Invoke(null, e);
                    handled = e.Handled;
                }

                if (s_KeyPress != null && wParam == WM_KEYDOWN)
                {
                    bool isDownShift = ((GetKeyState(VK_SHIFT) & 0x80) == 0x80 ? true : false);
                    bool isDownCapslock = (GetKeyState(VK_CAPITAL) != 0 ? true : false);

                    byte[] keyState = new byte[256];
                    GetKeyboardState(keyState);
                    byte[] inBuffer = new byte[2];
                    if (ToAscii(MyKeyboardHookStruct.VirtualKeyCode,
                              MyKeyboardHookStruct.ScanCode,
                              keyState,
                              inBuffer,
                              MyKeyboardHookStruct.Flags) == 1)
                    {
                        char key = (char)inBuffer[0];
                        if ((isDownCapslock ^ isDownShift) && Char.IsLetter(key)) key = Char.ToUpper(key);
                        KeyPressEventArgs e = new KeyPressEventArgs(key);
                        s_KeyPress.Invoke(null, e);
                        handled = handled || e.Handled;
                    }
                }

                if (s_KeyUp != null && (wParam == WM_KEYUP || wParam == WM_SYSKEYUP))
                {
                    Keys keyData = (Keys)MyKeyboardHookStruct.VirtualKeyCode;
                    KeyEventArgs e = new KeyEventArgs(keyData);
                    s_KeyUp.Invoke(null, e);
                    handled = handled || e.Handled;
                }
            }

            if (handled)
                return -1;

            return CallNextHookEx(s_KeyboardHookHandle, nCode, wParam, lParam);
        }

        private static void EnsureSubscribedToGlobalKeyboardEvents()
        {
            if (s_KeyboardHookHandle == 0)
            {
                s_KeyboardDelegate = KeyboardHookProc;
                s_KeyboardHookHandle = SetWindowsHookEx(
                    WH_KEYBOARD_LL,
                    s_KeyboardDelegate,
                    LoadLibrary("User32")
                                ,
                    0);
                if (s_KeyboardHookHandle == 0)
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    throw new Win32Exception(errorCode);
                }
            }
        }

        private static void TryUnsubscribeFromGlobalKeyboardEvents()
        {
            if (s_KeyDown == null &&
                s_KeyUp == null &&
                s_KeyPress == null)
            {
                ForceUnsunscribeFromGlobalKeyboardEvents();
            }
        }

        private static void ForceUnsunscribeFromGlobalKeyboardEvents()
        {
            if (s_KeyboardHookHandle != 0)
            {
                int result = UnhookWindowsHookEx(s_KeyboardHookHandle);
                s_KeyboardHookHandle = 0;
                s_KeyboardDelegate = null;
                if (result == 0)
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    throw new Win32Exception(errorCode);
                }
            }
        }

        #endregion Keyboard hook processing
    }
}
