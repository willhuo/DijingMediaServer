using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Dijing.Appsettings;
using Dijing.Common.Core.Utility;
using Dijing.SerilogExt;
using Xabe.FFmpeg;

namespace PushStreamCore
{
    class Program
    {
        private static int _delaySecs = 10;
        private static bool _isWorking = false;
        private static string _basePushUrl = "-rtsp_transport tcp -i {0} -vcodec copy -acodec copy -f flv {1}";


        static void Main(string[] args)
        {
            InitLog.SetLog(RunModeEnum.Debug);

            ProcessHelper.KillProcess("ffmpeg");
            Serilog.Log.Information("IPC摄像头发现者启动完成");

            var urlFrom = AppSettingsHelper.Configuration["Push:From"];
            var urlTo = AppSettingsHelper.Configuration["Push:To"];
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


            while (true)
            {
                try
                {
                    if (_isWorking)
                    {
                        continue;
                    }

                    ProcessHelper.KillProcess("ffmpeg");
                    Task.Run(() => Push(urlFrom, urlTo));
                }
                catch (Exception ex)
                {
                    Serilog.Log.Error(ex, "推流异常");
                }
                finally
                {
                    Thread.Sleep(TimeSpan.FromSeconds(_delaySecs));
                }
            }
        }


        private static void Push(string urlFrom, string urlTo)
        {
            try
            {
                _isWorking = true;
                Serilog.Log.Information("开始推流");
                var ffmpegPath = AppSettingsHelper.Configuration["Push:FfmpegPath"];
                if (!Directory.Exists(ffmpegPath))
                {
                    Serilog.Log.Warning("ffmpeg路径不存在");
                    return;
                }
                FFmpeg.SetExecutablesPath(ffmpegPath);


                var args = string.Format(_basePushUrl, urlFrom, urlTo);
                var conversion = FFmpeg.Conversions.New();
                conversion.OnProgress += Conversion_OnProgress;
                conversion.OnDataReceived += Conversion_OnDataReceived;
                var conversionResult = conversion.Start(args).Result;
                Serilog.Log.Information("主动推流耗时：{0}", conversionResult.Duration);

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
        private static void Conversion_OnProgress(object sender, Xabe.FFmpeg.Events.ConversionProgressEventArgs args)
        {
            //Serilog.Log.Information("Duration={0},TotalLength={1},Percent={2},ProcessId={3}", args.Duration,args.TotalLength,args.Percent,args.ProcessId);
            Serilog.Log.Information("Duration={0}", args.Duration);
        }
        private static void Conversion_OnDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            Serilog.Log.Information("recvdata={0}", e.Data);
        }
    }
}
