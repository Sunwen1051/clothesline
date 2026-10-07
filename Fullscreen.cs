using System.Runtime.InteropServices;

namespace Clothesline;

internal static class Fullscreen
{
    private enum QUNS : int
    {
        NOT_PRESENT = 1,
        BUSY = 2,
        RUNNING_D3D_FULL_SCREEN = 3,
        PRESENTATION_MODE = 4,
        ACCEPTS_NOTIFICATIONS = 5,
        QUIET_TIME = 6,
        APP = 7,
    }

    [DllImport("shell32.dll")]
    private static extern int SHQueryUserNotificationState(out QUNS state);

    /// <summary>全屏游戏 / 演示模式 / 系统安静时间下不打扰（照片仍会挂上，热键可唤出）。</summary>
    public static bool Suppressed()
    {
        try
        {
            SHQueryUserNotificationState(out var s);
            return s is QUNS.BUSY or QUNS.RUNNING_D3D_FULL_SCREEN
                or QUNS.PRESENTATION_MODE or QUNS.QUIET_TIME;
        }
        catch
        {
            return false;
        }
    }
}
