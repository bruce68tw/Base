using Base.Services;
using System;
using System.Runtime.InteropServices;

/// <summary>
/// 透過 Windows mpr.dll 管理需帳密的網路共用連線。
/// </summary>
public static class _Net
{
    [StructLayout(LayoutKind.Sequential)]
    private class NetResourceDto
    {
        public int dwScope = 0;
        public int dwType = 1; // RESOURCETYPE_DISK
        public int dwDisplayType = 0;
        public int dwUsage = 0;
        public string lpLocalName = null!;
        public string lpRemoteName = null!;
        public string lpComment = null!;
        public string lpProvider = null!;
    }

    //名稱固定不可改
    [DllImport("mpr.dll")]
    private static extern int WNetAddConnection2(
        NetResourceDto lpNetResource,
        string lpPassword,
        string lpUsername,
        int dwFlags);

    //名稱固定不可改
    [DllImport("mpr.dll")]
    private static extern int WNetCancelConnection2(string lpName, int dwFlags, bool bForce);

    /// <summary>
    /// 先取消同一路徑連線再以帳密連接；原生回傳碼為 0 時回傳 true。
    /// </summary>
    public static bool Connect(string netPath, string userId, string pwd)
    {
        //連線前先取消連線，避免重複連線, 造成error
        Disconnect(netPath);

        //連線
        var netResource = new NetResourceDto
        {
            lpRemoteName = netPath
        };

        var result = WNetAddConnection2(netResource, pwd, userId, 0);
        var status = (result == 0);
        if (!status)
        {
            _Log.Error($"_Net.cs Connect fail, result={result}, path={netPath}");
        }
        return status;
    }

    /// <summary>
    /// 強制取消指定路徑的共用連線，忽略原生回傳碼。
    /// </summary>
    public static void Disconnect(string netPath)
    {
        WNetCancelConnection2(netPath, 0, true);
    }
}
