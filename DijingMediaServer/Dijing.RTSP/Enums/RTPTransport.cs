using System;
using System.Collections.Generic;
using System.Text;

namespace Dijing.RTSP.Enums
{
    /// <summary>
    /// RTP数据包传输方式
    /// </summary>
    public enum RTPTransport : int
    {
        UNKNOWN = 0,
        UDP = 1,
        TCP = 2,
        MULTICAST = 3
    }
}
