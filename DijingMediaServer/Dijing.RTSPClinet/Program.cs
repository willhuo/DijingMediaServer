using Dijing.ComContext.Core;
using Dijing.Common.Core.Utility;
using Dijing.CommunicationHelper;
using Dijing.RTSP;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Dijing.RTSPClinet
{
    class Program
    {
        static void Main(string[] args)
        {
            //初始化
            Init();

            string rtspUrl = "rtsp://184.72.239.149/vod/mp4://BigBuckBunny_175k.mov";
            IRtspClient rtspClient = new RtspClient(RTSP.Enums.RTPTransport.TCP);
            rtspClient.Connect(rtspUrl);

            //等待循环结束
            Cycling();
        }


        /*private method*/
        private static void Init()
        {
            InitUI();
            InitLog();
        }
        private static void InitUI()
        {
            var curAssembly = System.Reflection.Assembly.GetExecutingAssembly();
            var attributes = curAssembly.GetCustomAttributes(typeof(System.Reflection.AssemblyFileVersionAttribute), false);
            var fileVersionAttribute = (System.Reflection.AssemblyFileVersionAttribute)attributes.First();
            var version = fileVersionAttribute.Version;
            Console.Title = $"Dijing.Client[{version}]";

            RemoveCloseButton();
        }
        private static void RemoveCloseButton()
        {
            EnvironmentHelper.Default.RemoveCurrentConsoleWindowMenu();
        }
        private static void InitLog()
        {
            LogHelper.Default.LogConfig(true, 1, true);
            LogHelper.Default.ReceivingLogEvent += Default_ReceivingLogEvent;
            LogHelper.Default.LogPrint($"日志设置成功,printLog={true},logType={1},supportEvent={true}", 2);

        }
        private static void Cycling()
        {
            while (true)
            {
                Task.Delay(5000).Wait();
            }
        }



        /*event*/
        private static void Default_ReceivingLogEvent(object sender, LogContent e)
        {
            ConsoleColor printColor;
            switch (e.Type)
            {
                case 1:
                    printColor = ConsoleColor.White;
                    break;
                case 2:
                    printColor = ConsoleColor.Green;
                    break;
                case 3:
                    printColor = ConsoleColor.Yellow;
                    break;
                case 4:
                    printColor = ConsoleColor.Red;
                    break;
                case 5:
                    printColor = ConsoleColor.Magenta;
                    break;
                default:
                    printColor = ConsoleColor.White;
                    break;
            }
            Console.ForegroundColor = printColor;
            Console.WriteLine(e.Msg);
        }
    }
}
