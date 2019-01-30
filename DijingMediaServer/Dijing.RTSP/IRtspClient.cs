using Dijing.RTSP.Enums;
using DotNetty.Transport.Channels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dijing.RTSP
{
    public interface IRtspClient
    {
        RTPTransport RtpTransport { get; }
        string RtspUrl { get; }
        string Host { get; }
        int Port { get; }
        string Username { get; }
        string Password { get; }
        IChannel Channnel { get; }


        bool Connect(string rtspUrl);
    }
}
