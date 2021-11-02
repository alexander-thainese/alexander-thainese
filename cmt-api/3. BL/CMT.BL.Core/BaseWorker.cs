using System;
using System.Collections.Generic;
using System.Threading;

namespace CMT.BL.Core
{
    public enum WorkerState
    {
        NotInitialized = 0,
        Initialized = 1,
        ThreadStarted = 2,
        WaitingForWakeup = 3,
        Processing = 4,
        FinishedProcessing = 5,
    }

    public interface ISystemLogger
    {
        void LogAction(string message, Type caller);


        void LogError(Type caller, Exception e);
    }

    public abstract class BaseWorker
    {
        private static List<BaseWorker> workers = new List<BaseWorker>();

        public static List<BaseWorker> Workers
        {
            get { return workers; }
        }

        public static bool ShowFullInfo { get; set; }

        private Thread worker;
        private ISystemLogger logger;

        public BaseWorker()
        {
            WorkerName = string.Format("{0} ({1}, {2})", GetType().Name, Environment.MachineName, Guid.NewGuid());
            WorkerState = WorkerState.NotInitialized;
        }

        public BaseWorker(ISystemLogger logger)
            : this()
        {
            this.logger = logger;
        }

        public void Start()
        {
            if (worker != null)
            {
                throw new InvalidOperationException("Worker already started");
            }

            worker = new Thread(ProcessMainLoop);
            worker.Priority = ThreadPriority.BelowNormal;
            worker.IsBackground = true;
            if (!workers.Contains(this))
            {
                workers.Add(this);
            }

            WorkerState = WorkerState.Initialized;
            worker.Start();
        }

        private void ProcessMainLoop()
        {
            WorkerState = WorkerState.ThreadStarted;
            if (logger != null)
            {
                logger.LogAction(string.Format("{0} started", WorkerName), GetType());
            }

            TimeSpan actualDelay = ThreatStartAtExactTime ? DateTime.Now.TimeOfDay : TimeSpan.Zero;
            WorkerLoopDelay = GetThreadLoopDelay();
            int errorCount = 0;
            while (true)
            {
                if (errorCount > 3)
                {
                    Stop();
                    return;
                }

                if (ShowFullInfo)
                {
                    if (logger != null)
                    {
                        logger.LogAction(string.Format("{0} loop started", WorkerName), GetType());
                    }
                }

                try
                {

                    TimeSpan threadLoopDelay = WorkerLoopDelay - actualDelay;
                    if (threadLoopDelay < TimeSpan.Zero)
                    {
                        threadLoopDelay = threadLoopDelay.Add(new TimeSpan(1, 0, 0, 0));
                    }

                    actualDelay = ThreatStartAtExactTime ? DateTime.Now.TimeOfDay : TimeSpan.Zero;
                    WorkerState = WorkerState.WaitingForWakeup;
                    WorkerNextWakeup = DateTime.Now.Add(threadLoopDelay);
                    Thread.Sleep(threadLoopDelay);
                    WorkerState = WorkerState.Processing;

                    ProcessWork();
                    errorCount = 0;
                }
                catch (ThreadAbortException)
                {
                    if (logger != null)
                    {
                        logger.LogAction(string.Format("{0} thread aborted", WorkerName), GetType());
                    }

                    WorkerState = WorkerState.FinishedProcessing;
                }
                catch (Exception ex)
                {
                    int delay = 10;
                    errorCount++;
                    if (logger != null)
                    {
                        logger.LogError(GetType(), ex);
                    }

                    if (errorCount < 3)
                    {
                        logger.LogAction(string.Format("{0} worker will restart in {1} minutes", WorkerName, delay), GetType());
                        actualDelay = WorkerLoopDelay.Add(new TimeSpan(0, -delay, 0));
                    }
                    else
                    {
                        logger.LogAction(string.Format("{0} worker maximum errors exceeded. Worker stopped.", WorkerName, delay), GetType());
                    }
                }
            }
        }

        protected abstract void ProcessWork();

        protected virtual TimeSpan GetTimeDelayToStartWork()
        {
            return TimeSpan.Zero;
        }

        protected abstract TimeSpan GetThreadLoopDelay();

        protected bool ThreatStartAtExactTime { get; set; }

        public void Stop()
        {
            if (worker != null)
            {
                if (logger != null)
                {
                    logger.LogAction(string.Format("{0} worker is being stopped manually", WorkerName), GetType());
                }

                worker.Abort();
                worker = null;
            }
        }

        public static void RestartActiveWorkers()
        {
            foreach (BaseWorker worker in workers)
            {
                if (worker.WorkerState != WorkerState.FinishedProcessing)
                {
                    worker.Stop();
                    worker.Start();
                }
            }
        }

        public static void RestartAllWorkers()
        {
            foreach (BaseWorker worker in workers)
            {
                worker.Stop();
                worker.Start();
            }
        }

        public ThreadState ThreadState
        {
            get
            {
                if (worker == null)
                {
                    return ThreadState.Unstarted;
                }
                else
                {
                    return worker.ThreadState;
                }
            }
        }

        public WorkerState WorkerState { get; private set; }

        public string WorkerName { get; private set; }

        public TimeSpan WorkerLoopDelay { get; private set; }

        public DateTime WorkerNextWakeup { get; private set; }
    }
}
