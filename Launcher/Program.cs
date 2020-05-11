using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Launcher
{
    class Program
    {
        private static string IncomingQueueName { get; } = "launcher";
        private static RabbitMQCommunicationService Communicator { get; set; }

        private static AutoResetEvent AppClose = new AutoResetEvent(false);
        
        static void Initialize()
        {
            Communicator = RabbitMQCommunicationService.Instance;
            Communicator.Initialize();
            Communicator.DeclareIncomingQueue(IncomingQueueName);
            Communicator.MessageReceived += MessageReceiver;
        }

        static void Main(string[] args)
        {
            Initialize();

            Process pythonControllerProcess = new Process();
            pythonControllerProcess.StartInfo.FileName = "pythonw";

            // Here paste the absolute path to your PythonScripts/controller.pyw file.
            // In the final release, the Launcher.exe file will be placed in a folder
            // whose relative position to PythonScripts/controller.pyw will be always the same.
            pythonControllerProcess.StartInfo.Arguments = "D:/Dane/MichalKuzemczak/Projects/ProjektZespołowy/PythonScripts/controller.pyw";
            pythonControllerProcess.StartInfo.UseShellExecute = false;
            pythonControllerProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            pythonControllerProcess.Start();

            // Give the python controller some time to set up
            Thread.Sleep(500);

            Process piceonProcess = new Process();
            piceonProcess.StartInfo.FileName = "Piceon.exe";
            piceonProcess.StartInfo.Arguments = "";
            piceonProcess.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;

            // Comment this line, if you want to launch piceon yourself
            // from Visual studio
            // piceonProcess.Start();

            AppClose.WaitOne();

            pythonControllerProcess.Kill();
        }

        private static void MessageReceiver(object sender, MessageReceivedEventArgs e)
        {
            if (e.QueueName != IncomingQueueName)
                return;

            if (e.Message == "closing")
            {
                AppClose.Set();
            }

            return;
        }
    }
}
