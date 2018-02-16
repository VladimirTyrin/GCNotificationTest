using System;
using System.Collections.Generic;
using System.Threading;
using ITCC.Logging.Core;

namespace GCNotificationTest.Common
{
    public class GcTester
    {
        private static volatile bool _shouldStop;
        private static readonly List<Thread> Threads = new List<Thread>();
        private static CountdownEvent _stopEvent;

        /// <summary>
        ///     Start GC notification test (N allocators, 1 monitor)
        /// </summary>
        /// <param name="allocatingThreadCount">Number of threads used to allocate objects and calculate their hashes</param>
        /// <param name="cpuIntensive">Make object hash calculation CPU-intensive (also involves a lot of RAM access)</param>
        public static void Start(int allocatingThreadCount, bool cpuIntensive)
        {
            _shouldStop = false;
            Threads.Clear();
            _stopEvent = new CountdownEvent(allocatingThreadCount + 1);

            for (var i = 0; i < allocatingThreadCount; ++i)
            {
                var allocatingThread = StartAllocatingThread(i + 1, cpuIntensive);
                Threads.Add(allocatingThread);
            }

            var monitoringThread = StartMonitoringThread();
            Threads.Add(monitoringThread);
        }

        /// <summary>
        ///     Signal all threads to stop and wait them to finalize. Class can be reused after this method returns with true
        /// </summary>
        /// <param name="millisecondsTimeout"></param>
        /// <returns>True in case all threads have stopped successfully</returns>
        public static bool Stop(int millisecondsTimeout)
        {
            _shouldStop = true;

            return _stopEvent.Wait(millisecondsTimeout);
        }

        private static Thread StartAllocatingThread(int index, bool cpuIntensive)
        {
            var logContext = $"ALLOCATOR {index}";
            var allocatingThread = new Thread(() =>
                {
                    var currentLevel = 0;
                    while (! _shouldStop)
                    {
                        currentLevel++;
                        MemoryEater.AllocateSomeObjects(currentLevel, cpuIntensive);

                        if (currentLevel % 10 == 0)
                        {
                            Logger.LogEntry(logContext, LogLevel.Debug, $"Reached level {currentLevel}");
                        }
                    }

                    _stopEvent.Signal();
                })
                { IsBackground = true };

            allocatingThread.Start();
            return allocatingThread;
        }

        private static Thread StartMonitoringThread()
        {
            var monitoringThread = new Thread(() =>
                {
                    while (! _shouldStop)
                    {
                        GC.RegisterForFullGCNotification(50, 50);

                        var s = GC.WaitForFullGCApproach();
                        Logger.LogEntry("NOTIFIER", LogLevel.Info, $"GC approach.  Status: {s}");

                        s = GC.WaitForFullGCComplete();
                        Logger.LogEntry("NOTIFIER", LogLevel.Info, $"GC completed. Status: {s}");
                        GC.CancelFullGCNotification();
                    }

                    _stopEvent.Signal();
                })
                { IsBackground = true };

            monitoringThread.Start();
            return monitoringThread;
        }
    }
}
