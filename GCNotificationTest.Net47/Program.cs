using System;
using System.Threading;
using ITCC.Logging.Core;
using ITCC.Logging.Windows.Loggers;

namespace GCNotificationTest.Net47
{
    internal class Program
    {
        private static void Main()
        {
            Logger.Level = LogLevel.Trace;
            Logger.RegisterReceiver(new ColouredConsoleLogger());

            GCNotificationTest.Common.GcTester.Start(16, true);

            Console.ReadLine();

            GCNotificationTest.Common.GcTester.Stop(-1);
        }
    }
}
