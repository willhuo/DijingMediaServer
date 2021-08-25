using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Dijing.Common.Core.Utility;
using Dijing.SerilogExt;
using Xabe.FFmpeg;

namespace PushStream
{
    public partial class HomeForm : Form
    {
        private bool _isWorking { get; set; }
        private string _basePushUrl = "-rtsp_transport tcp -i {0} -vcodec copy -acodec copy -f flv {1}";


        public HomeForm()
        {
            InitializeComponent();
        }

        private void HomeForm_Load(object sender, EventArgs e)
        {
            InitLog.SetLog(RunModeEnum.Debug);
            InMemorySink.OnLogReceivedEvent += InMemorySink_OnLogReceivedEvent;

            ProcessHelper.KillProcess("ffmpeg");
            Serilog.Log.Information("IPC摄像头发现者启动完成");
        }

        private void btnPush_Click(object sender, EventArgs e)
        {
            var urlFrom = txtFrom.Text;
            var urlTo = txtTo.Text;
            if (urlFrom.IsNullorEmpty() || urlTo.IsNullorEmpty())
            {
                Serilog.Log.Information("地址部分为空");
                return;
            }

            if (_isWorking)
            {
                Serilog.Log.Information("正在推流中");
                return;
            }

            ProcessHelper.KillProcess("ffmpeg");
            _isWorking = true;
            Task.Run(() => Push(urlFrom, urlTo));
        }

        private void Push(string urlFrom,string urlTo)
        {
            try
            {
                Serilog.Log.Information("开始推流");
                FFmpeg.SetExecutablesPath("D:\\ffmpeg\\bin");


                var args = string.Format(_basePushUrl, urlFrom, urlTo);
                var conversion = FFmpeg.Conversions.New();
                conversion.OnProgress += Conversion_OnProgress;
                conversion.OnDataReceived += Conversion_OnDataReceived;
                var conversionResult = conversion.Start(args).Result;
                Serilog.Log.Information("主动推流耗时：{0}",  conversionResult.Duration);

                //var mediaInfo = FFmpeg.GetMediaInfo(urlFrom).Result;
                //IStream videoStream = mediaInfo.VideoStreams.FirstOrDefault()
                //    ?.SetCodec(VideoCodec.h264);
                //IStream audioStream = mediaInfo.AudioStreams.FirstOrDefault()
                //    ?.SetCodec(AudioCodec.aac);
                //var conversionResult = FFmpeg.Conversions.New()
                //    .AddStream(videoStream,audioStream)
                //    .SetOutput(urlTo)
                //    .Start().Result;
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Push error");
            }
            finally
            {
                _isWorking = false;
            }
        }

        private void Conversion_OnProgress(object sender, Xabe.FFmpeg.Events.ConversionProgressEventArgs args)
        {
            //Serilog.Log.Information("Duration={0},TotalLength={1},Percent={2},ProcessId={3}", args.Duration,args.TotalLength,args.Percent,args.ProcessId);
            Serilog.Log.Information("Duration={0}", args.Duration);
        }

        private void Conversion_OnDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            Serilog.Log.Information("recvdata={0}", e.Data);
        }

        private void InMemorySink_OnLogReceivedEvent(object sender, string e)
        {
            try
            {
                if (txtLog.InvokeRequired)
                {
                    txtLog.Invoke(new Action(() =>
                    {
                        txtLog.AppendText(e + Environment.NewLine);
                        txtLog.ScrollToCaret();
                        if (txtLog.TextLength > 200000)
                            txtLog.ResetText();
                    }));
                }
                else
                {
                    txtLog.AppendText(e + Environment.NewLine);
                    txtLog.ScrollToCaret();
                    if (txtLog.TextLength > 200000)
                        txtLog.ResetText();
                }

            }
            catch
            {
                // ignored
            }
        }
    }
}
