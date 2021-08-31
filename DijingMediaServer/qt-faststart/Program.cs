using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Dijing.SerilogExt;
using Xabe.FFmpeg;

namespace qt_faststart
{
    class Program
    {
        private static string _qtFastStartPath = "qt-faststart";
        private static string _ffmpegPath = "/usr/bin";

        static void Main(string[] args)
        {
            InitLog.SetLog(RunModeEnum.Debug);

            var oldMp4FilePath = "/home/smartrtmpd/vod/490ba5a4c008489fb10a232affc861c7.mp4";
            var newMp4FilePath = "/home/smartrtmpd/vod/490ba5a4c008489fb10a232affc861c7_mf.mp4";

            FfmpegMoveFlagsRun(oldMp4FilePath, newMp4FilePath);
        }


        /// <summary>
        /// ffmpeg转换mp4文件，实现边下边播
        /// </summary>
        /// <param name="oldMp4FilePath"></param>
        /// <param name="newMp4FilePath"></param>
        private static void FfmpegMoveFlagsRun(string oldMp4FilePath,string newMp4FilePath)
        {
            try
            {
                Serilog.Log.Information("开始用ffmpeg转换mp4文件");
                FFmpeg.SetExecutablesPath(_ffmpegPath);

                IMediaInfo mediaInfo = FFmpeg.GetMediaInfo(oldMp4FilePath).Result;
                IStream videoStream = mediaInfo.VideoStreams.FirstOrDefault()
                    ?.SetCodec(VideoCodec.libx264);
                IStream audioStream = mediaInfo.AudioStreams.FirstOrDefault()
                    ?.SetCodec(AudioCodec.aac);

                var conversion = FFmpeg.Conversions.New();
                conversion.OnProgress += Conversion_OnProgress;
                conversion.OnDataReceived += Conversion_OnDataReceived;

                var conversionResult = conversion
                    .AddStream(videoStream, audioStream)
                    .AddParameter("-strict -2 -movflags +faststart")
                    .SetOutput(newMp4FilePath)
                    .Start().Result;

                Serilog.Log.Information("文件转换完成，耗时：{0}", conversionResult.Duration);

            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "mp4转换完成 error");
            }
        }
        private static void Conversion_OnDataReceived(object sender, DataReceivedEventArgs e)
        {
            Serilog.Log.Information("recv data:{0}",e.Data);
        }
        private static void Conversion_OnProgress(object sender, Xabe.FFmpeg.Events.ConversionProgressEventArgs args)
        {
            Serilog.Log.Information("Duration:{0}", args.Duration);
        }






        private void QtFastStartRun(string oldMp4,string newMp4)
        {
            try
            {
                var currPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                using var process = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    CreateNoWindow = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    FileName = _qtFastStartPath,
                    WorkingDirectory = currPath
                };


                startInfo.Arguments = $" {oldMp4} {newMp4}";
                process.StartInfo = startInfo;
                process.Start();

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();


                process.OutputDataReceived += Process_OutputDataReceived;
                process.ErrorDataReceived += Process_ErrorDataReceived;

                Serilog.Log.Warning("qt-faststart 启动完成");
                process.WaitForExit();
                Serilog.Log.Warning("qt-faststart 进程退出");
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "error");
            }
        }
        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            Serilog.Log.Information("qf-error:" + e.Data);
        }
        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            Serilog.Log.Information("qf-output:" + e.Data);
        }
    }
}
