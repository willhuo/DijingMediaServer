using Dijing.ComContext.Core;
using Dijing.Common.Core.Utility;
using Dijing.CommunicationHelper;
using Dijing.RTSP.Enums;
using Dijing.RTSP.Utility;
using DotNetty.Transport.Channels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dijing.RTSP
{
    public class RtspClient: IRtspClient
    {
        /// <summary>
        /// constructor
        /// </summary>
        public RtspClient(RTPTransport rtpTransport)
        {
            this.RtpTransport = rtpTransport;
            Init();
        }


        /*attr*/
        public RTPTransport RtpTransport { get; private set; }
        public string RtspUrl { get; private set; }
        public string Host { get; private set; }
        public int Port { get; private set; }
        public string Username { get; private set; }
        public string Password { get; private set; }
        public IChannel Channnel { get; private set; }


        /*variable*/


        /*private method*/
        private void Init()
        {
            UriParserHelper.RegisterRtspUri();
            InitEvent();
        }
        private void InitEvent()
        {
            DataHandler.OnGetTCPDataEvent += DataHandler_OnGetTCPDataEvent;
            DataHandler.OnClientOnlineEvent += DataHandler_OnClientOnlineEvent;
            DataHandler.OnClientOfflineEvent += DataHandler_OnClientOfflineEvent;
        }
        private void TerminalOnLine(string sessionKey)
        {
            var view = ContextMng.Default.GetContextView(sessionKey);
            LogHelper.Default.LogPrint($"客户端与服务器建立TCP连接成功", 2);
        }
        private void TerminalOffLine(string sessionKey)
        {
            LogHelper.Default.LogPrint("客户端与服务器离线完成", 3);
        }



        /*public method*/
        public bool Connect(string rtspUrl)
        {
            try
            {
                Uri rtspUri = new Uri(rtspUrl);
                this.Host = rtspUri.Host;
                this.Port = rtspUri.Port;
                if (rtspUri.UserInfo.Length > 0)
                {
                    string[] userInfoArray = rtspUri.UserInfo.Split(new char[':'], StringSplitOptions.RemoveEmptyEntries);
                    this.Username = userInfoArray[0];
                    this.Password = userInfoArray[1];
                    this.RtspUrl = rtspUri.GetComponents((UriComponents.AbsoluteUri & ~UriComponents.UserInfo), UriFormat.UriEscaped);
                }

                this.Channnel = TCPHelper.Default.ConnectToServer(Host, Port);

                return true;
            }
            catch(Exception ex)
            {
                LogHelper.Default.LogDay($"rtsp connect error,{ex}");
                LogHelper.Default.LogPrint($"rtsp connect error,{ex.Message}", 4);
                return false;
            }
        }


        /*event*/
        private void DataHandler_OnGetTCPDataEvent(object sender, Context e)
        {
            //if (!e.IsParsing_Protocol)
            //{
            //    e.IsParsing_Protocol = true;
            //    Task.Run(() => ProtocolParser.Default.ProtocolParse(e));
            //    LogHelper.Default.LogPrint($"Start new protocol parsing task for {e.SessionKey}", 2);
            //}
            //else
            //{
            //    LogHelper.Default.LogPrint($"Data cache at protocol parsing {e.SessionKey}", 3);
            //}
        }
        private void DataHandler_OnClientOnlineEvent(object sender, string e)
        {
            TerminalOnLine(e);
        }
        private void DataHandler_OnClientOfflineEvent(object sender, string e)
        {
            TerminalOffLine(e);
        }
    }
}
