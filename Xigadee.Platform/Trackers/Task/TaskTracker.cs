﻿#region using
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
#endregion
namespace Xigadee
{
    /// <summary>
    /// This is the tracker agent used to trace queued and executing jobs.
    /// </summary>
    [DebuggerDisplay("{Type}/{Name}={ProcessSlot}@{Priority}|{Id}")]
    public class TaskTracker
    {
        /// <summary>
        /// This is the priority value for a internal task.
        /// </summary>
        public const int PriorityInternal = -1;

        public TaskTracker(TaskTrackerType type, TimeSpan? ttl)
        {
            this.Id = Guid.NewGuid();
            this.UTCStart = DateTime.UtcNow;
            this.TTL = ttl??TimeSpan.FromSeconds(30);
            this.Cts = new CancellationTokenSource();
            this.TickCount = Environment.TickCount;
            this.Type = type;
        }

        public int? ExecuteTickCount { get; set; }

        public TaskTrackerType Type { get; set; }

        public TimeSpan? TimeProcessing
        {
            get
            {
                return UTCExecute.HasValue ? DateTime.UtcNow - UTCExecute.Value : default(TimeSpan?);
            }
        }

        public TimeSpan? TimeToExpiry
        {
            get
            {
                return ExpireTime.HasValue ? ExpireTime.Value - DateTime.UtcNow : default(TimeSpan?);
            }
        }

        public long? ProcessSlot { get; set; }

        public int? Priority { get; set; }

        /// <summary>
        /// This is the friendly name used during statistic debugging.
        /// </summary>
        public string Name { get; set; }

        public readonly int TickCount;

        public string Caller { get; set; }

        public Guid Id { get; set; }

        /// <summary>
        /// This boolean property identifies when a task is long running and is used to identify that fact to the Task Manager.
        /// </summary>
        public bool IsLongRunning { get; set; }

        /// <summary>
        /// This boolean property identifies whether the request has been generated by another task for immediate processing.
        /// This type of task will not count as a running task as it has been generated by a task that already has been assigned 
        /// a running slot.
        /// </summary>
        public bool IsInternal { get { return Priority.HasValue && Priority.Value == PriorityInternal; } }

        public bool IsCancelled { get; set; }

        public bool IsKilled { get; set; }

        /// <summary>
        /// This is the functional called which returns the task to be executed when the tracker is scheduled to execute.
        /// </summary>
        public Func<CancellationToken, Task> Execute { get; set; }
        /// <summary>
        /// This action is executed once the task has completed. It passed the original task, a boolean value 
        /// indicating whether the task failed, and any exception that was generated by the failure.
        /// </summary>
        public Action<TaskTracker, bool, Exception> ExecuteComplete { get; set; }

        public DateTime UTCStart { get; set; }

        public DateTime? UTCExecute { get; set; }

        public DateTime? CancelledTime { get; set; }

        public TimeSpan TTL { get; set; }

        public bool HasExpired
        {
            get
            {
                var time = ExpireTime;
                return time.HasValue && (DateTime.UtcNow > time.Value);
            }
        }

        public DateTime? ExpireTime
        {
            get
            {
                return (!UTCExecute.HasValue || IsLongRunning)?default(DateTime?):UTCExecute.Value.Add(TTL);
            }
        }

        public void Cancel()
        {
            if (IsCancelled)
                return;

            CancelledTime = DateTime.UtcNow;
            IsCancelled = true;
            Cts.Cancel();
        }

        public Task ExecuteTask { get; set; }

        /// <summary>
        /// This is the cancellation token used to signal tiemouts or shutdown.
        /// </summary>
        public CancellationTokenSource Cts { get; set; }

        public object Context { get; set; }

        #region Debug
        /// <summary>
        /// This is the debug message for the task.
        /// </summary>
        public string Debug
        {
            get
            {
                try
                {
                    var queueTime = StatsCounter.LargeTime((UTCExecute ?? DateTime.UtcNow) - UTCStart);
                    var executeTime = StatsCounter.LargeTime(TimeProcessing, "Never");
                    var expireTime = StatsCounter.LargeTime(TimeToExpiry, "Never");

                    string id = null;

                    switch (Type)
                    {
                        case TaskTrackerType.Notset:
                            return "Not set";
                        case TaskTrackerType.Payload:
                            var payload = Context as TransmissionPayload;
                            id = payload.Message.CorrelationKey;
                            break;
                        case TaskTrackerType.Schedule:
                            var schedule = Context as Schedule;
                            id = schedule.Id.ToString("N");
                            break;
                        case TaskTrackerType.ListenerPoll:
                            id = Id.ToString("N");
                            break;
                        case TaskTrackerType.Overload:
                            id = Id.ToString("N");
                            break;
                    }

                    return string.Format("{10} {0}[{1}] {2} [{3}] Runtime={5} Expires={6} ({7}){8}{9} QueueTime={4}"
                        , Type
                        , Priority
                        , id
                        , Name
                        , queueTime
                        , executeTime
                        , expireTime
                        , Caller
                        , IsLongRunning ? " Long running" : ""
                        , IsCancelled ? (IsKilled?"Killed":"Cancelled") : ""
                        , ProcessSlot
                        );
                }
                catch (Exception ex)
                {
                    return string.Format("Error {0} - {1}", Id, ex.Message);
                }
            }
        }
        #endregion

    }
}
