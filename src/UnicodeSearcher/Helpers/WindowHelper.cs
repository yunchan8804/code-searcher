using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace UnicodeSearcher.Helpers;

/// <summary>
/// 창 위치 및 포커스 관련 헬퍼
/// </summary>
public static class WindowHelper
{
    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out POINT lpPoint);

    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromPoint(POINT pt, uint dwFlags);

    [DllImport("user32.dll")]
    private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

    private const uint MONITOR_DEFAULTTONEAREST = 2;

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MONITORINFO
    {
        public int cbSize;
        public RECT rcMonitor;
        public RECT rcWork;
        public uint dwFlags;
    }

    private static IntPtr _lastActiveWindow = IntPtr.Zero;

    /// <summary>
    /// 현재 활성 창 핸들 저장
    /// </summary>
    public static void SaveActiveWindow()
    {
        _lastActiveWindow = GetForegroundWindow();
    }

    /// <summary>
    /// 저장된 활성 창으로 포커스 복원
    /// </summary>
    public static void RestoreActiveWindow()
    {
        if (_lastActiveWindow != IntPtr.Zero)
        {
            SetForegroundWindow(_lastActiveWindow);
        }
    }

    /// <summary>
    /// 창을 마우스 커서 근처에 표시
    /// </summary>
    public static void PositionNearCursor(Window window)
    {
        if (!GetCursorPos(out POINT cursor))
        {
            // 커서 위치를 가져올 수 없으면 화면 중앙에 표시
            PositionAtCenter(window);
            return;
        }

        // 커서가 있는 모니터 정보 가져오기
        var monitor = MonitorFromPoint(cursor, MONITOR_DEFAULTTONEAREST);
        var monitorInfo = new MONITORINFO { cbSize = Marshal.SizeOf<MONITORINFO>() };

        if (!GetMonitorInfo(monitor, ref monitorInfo))
        {
            PositionAtCenter(window);
            return;
        }

        var workArea = monitorInfo.rcWork;

        // DPI 스케일 고려
        var source = PresentationSource.FromVisual(window);
        double dpiX = 1.0, dpiY = 1.0;
        if (source != null)
        {
            dpiX = source.CompositionTarget.TransformToDevice.M11;
            dpiY = source.CompositionTarget.TransformToDevice.M22;
        }

        double windowWidth = window.Width * dpiX;
        double windowHeight = window.Height * dpiY;

        // 창 위치 계산 (커서 약간 아래에)
        double left = cursor.X - windowWidth / 2;
        double top = cursor.Y + 20; // 커서 아래 20픽셀

        // 화면 경계 체크
        left = Math.Max(workArea.Left, Math.Min(left, workArea.Right - windowWidth));
        top = Math.Max(workArea.Top, Math.Min(top, workArea.Bottom - windowHeight));

        // DPI 스케일 역변환
        window.Left = left / dpiX;
        window.Top = top / dpiY;
    }

    /// <summary>
    /// 창을 화면 중앙에 표시
    /// </summary>
    public static void PositionAtCenter(Window window)
    {
        window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
    }

    /// <summary>
    /// 창 활성화 및 포커스
    /// </summary>
    public static void ActivateWindow(Window window)
    {
        if (window.WindowState == WindowState.Minimized)
        {
            window.WindowState = WindowState.Normal;
        }

        window.Show();
        window.Activate();

        var handle = new WindowInteropHelper(window).Handle;
        SetForegroundWindow(handle);
    }
}
