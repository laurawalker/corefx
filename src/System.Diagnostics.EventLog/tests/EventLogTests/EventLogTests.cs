﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.IO;
using Xunit;

namespace System.Diagnostics.Tests
{
    public class EventLogTests
    {
        [ConditionalFact(typeof(PlatformDetection), nameof(PlatformDetection.IsNotWindowsNanoServer))]
        public void EventLogReinitializationException()
        {
            using (EventLog eventLog = new EventLog())
            {
                eventLog.BeginInit();
                Assert.Throws<InvalidOperationException>(() => eventLog.BeginInit());
                eventLog.EndInit();
            }
        }

        [ConditionalFact(typeof(Helpers), nameof(Helpers.IsElevatedAndNotWindowsNano))]
        public void ClearLog()
        {
            string log = "ClearTest";
            string source = "Source_" + nameof(ClearLog);

            try
            {
                EventLog.CreateEventSource(source, log);
                using (EventLog eventLog = new EventLog())
                {
                    eventLog.Source = source;
                    eventLog.Clear();
                    Assert.Equal(0, eventLog.Entries.Count);
                    eventLog.WriteEntry("Writing to event log.");
                    Assert.Equal(eventLog.Entries.Count, 1);
                }
            }
            finally
            {
                EventLog.DeleteEventSource(source);
                EventLog.Delete(log);
            }
        }

        [ConditionalFact(typeof(PlatformDetection), nameof(PlatformDetection.IsNotWindowsNanoServer))]
        public void ApplicationEventLog_Count()
        {
            using (EventLog eventLog = new EventLog("Application"))
            {
                Assert.InRange(eventLog.Entries.Count, 1, Int32.MaxValue);
            }
        }

        [ConditionalFact(typeof(Helpers), nameof(Helpers.IsElevatedAndNotWindowsNano))]
        public void DeleteLog()
        {
            string log = "DeleteTest";
            string source = "Source_" + nameof(DeleteLog);

            try
            {
                EventLog.CreateEventSource(source, log);
                Assert.True(EventLog.Exists(log));
            }
            finally
            {
                EventLog.DeleteEventSource(source);
                EventLog.Delete(log);
                Assert.False(EventLog.Exists(log));
            }
        }

        [ConditionalFact(typeof(PlatformDetection), nameof(PlatformDetection.IsNotWindowsNanoServer))]
        public void CheckLogName_Get()
        {
            using (EventLog eventLog = new EventLog("Application"))
            {
                Assert.Equal("Application", eventLog.LogDisplayName);
            }
        }

        [ConditionalFact(typeof(PlatformDetection), nameof(PlatformDetection.IsNotWindowsNanoServer))]
        public void CheckMachineName_Get()
        {
            using (EventLog eventLog = new EventLog("Application"))
            {
                Assert.Equal(".", eventLog.MachineName);
            }
        }

        [ConditionalFact(typeof(PlatformDetection), nameof(PlatformDetection.IsNotWindowsNanoServer))]
        public void GetLogDisplayName_NotSet_Throws()
        {
            using (EventLog eventLog = new EventLog())
            {
                eventLog.Log = Guid.NewGuid().ToString("N");
                Assert.Throws<InvalidOperationException>(() => eventLog.LogDisplayName);
            }
        }

        [ConditionalFact(typeof(PlatformDetection), nameof(PlatformDetection.IsNotWindowsNanoServer))]
        public void GetLogDisplayName_Set()
        {
            using (EventLog eventLog = new EventLog())
            {
                eventLog.Log = "Application";
                Assert.Equal("Application", eventLog.LogDisplayName);
            }
        }

        [ConditionalFact(typeof(PlatformDetection), nameof(PlatformDetection.IsNotWindowsNanoServer))]
        public void EventLogs_Get()
        {
            Assert.Throws<ArgumentException>(() => EventLog.GetEventLogs(""));
            EventLog[] eventLogCollection = EventLog.GetEventLogs();
            Assert.Contains(eventLogCollection, eventlog => eventlog.Log.Equals("Application"));
            Assert.Contains(eventLogCollection, eventlog => eventlog.Log.Equals("Security"));
            Assert.Contains(eventLogCollection, eventlog => eventlog.Log.Equals("System"));
        }

        [ConditionalFact(typeof(Helpers), nameof(Helpers.IsElevatedAndNotWindowsNano))]
        public void GetMaxKilobytes_Set()
        {
            string source = "Source_" + nameof(GetMaxKilobytes_Set);
            string log = "maxKilobytesLog";

            try
            {
                EventLog.CreateEventSource(source, log);
                using (EventLog eventLog = new EventLog())
                {
                    eventLog.Source = source;
                    eventLog.MaximumKilobytes = 0x400;
                    Assert.Equal(0x400, eventLog.MaximumKilobytes);
                }
            }
            finally
            {
                EventLog.DeleteEventSource(source);
                EventLog.Delete(log);
            }

        }

        [ConditionalFact(typeof(PlatformDetection), nameof(PlatformDetection.IsNotWindowsNanoServer))]
        public void MaxKilobytesOutOfRangeException()
        {
            using (EventLog eventLog = new EventLog())
            {
                eventLog.Log = "Application";
                Assert.Throws<ArgumentOutOfRangeException>(() => eventLog.MaximumKilobytes = 2);
                Assert.Throws<ArgumentOutOfRangeException>(() => eventLog.MaximumKilobytes = 0x3FFFC1);
            }
        }

        [ConditionalFact(typeof(Helpers), nameof(Helpers.IsElevatedAndNotWindowsNano))]
        public void OverflowAndRetention_Set()
        {
            string source = "Source_" + nameof(OverflowAndRetention_Set);
            string log = "Overflow_Set";

            try
            {
                EventLog.CreateEventSource(source, log);
                using (EventLog eventLog = new EventLog())
                {
                    eventLog.Source = source;

                    //The second argument is only used when the overflow policy is set to OverWrite Older
                    eventLog.ModifyOverflowPolicy(OverflowAction.DoNotOverwrite, 1);
                    Assert.Equal(OverflowAction.DoNotOverwrite, eventLog.OverflowAction);

                    // -1 means overflow action is donot overrite
                    Assert.Equal(-1, eventLog.MinimumRetentionDays);
                }
            }
            finally
            {
                EventLog.DeleteEventSource(source);
                EventLog.Delete(log);
            }
        }

        [ConditionalFact(typeof(Helpers), nameof(Helpers.IsElevatedAndNotWindowsNano))]
        public void Overflow_OverWriteOlderAndRetention_Set()
        {
            string source = "Source_" + nameof(OverflowAndRetention_Set);
            string log = "Overflow_Set";
            int retentionDays = 30; //A number between 0 and 365 should work

            try
            {
                EventLog.CreateEventSource(source, log);
                using (EventLog eventLog = new EventLog())
                {
                    eventLog.Source = source;

                    //The second argument is only used when the overflow policy is set to OverWrite Older
                    eventLog.ModifyOverflowPolicy(OverflowAction.OverwriteOlder, retentionDays);
                    Assert.Equal(OverflowAction.OverwriteOlder, eventLog.OverflowAction);
                    Assert.Equal(retentionDays, eventLog.MinimumRetentionDays);
                }
            }
            finally
            {
                EventLog.DeleteEventSource(source);
                EventLog.Delete(log);
            }
        }

        [ConditionalFact(typeof(Helpers), nameof(Helpers.IsElevatedAndNotWindowsNano))]
        public void OverflowAndRetentionDaysOutOfRange()
        {
            using (EventLog eventLog = new EventLog())
            {
                eventLog.Log = "Application";
                Assert.Throws<ArgumentOutOfRangeException>(() => eventLog.ModifyOverflowPolicy(OverflowAction.OverwriteOlder, 400));
            }
        }

        [ConditionalFact(typeof(Helpers), nameof(Helpers.IsElevatedAndNotWindowsNano))]
        public void MachineName_Set()
        {
            string source = "Source_" + nameof(MachineName_Set);
            using (EventLog eventLog = new EventLog())
            {
                eventLog.Log = "Application";
                eventLog.MachineName = Environment.MachineName;
                try
                {
                    EventLog.CreateEventSource(source, eventLog.LogDisplayName);
                    Assert.True(EventLog.SourceExists(source, Environment.MachineName));
                }
                finally
                {
                    EventLog.DeleteEventSource(source);
                }
            }
        }

        [ConditionalFact(typeof(Helpers), nameof(Helpers.IsElevatedAndNotWindowsNano))]
        public void RegisterDisplayLogNameNull()
        {
            string log = "DisplayName";
            string source = "Source_" + nameof(RegisterDisplayLogNameNull);
            string messageFile = "File_" + nameof(RegisterDisplayLogNameNull);
            long DisplayNameMsgId = 45;

            EventSourceCreationData mySourceData = new EventSourceCreationData(source, log);
            mySourceData.MessageResourceFile = messageFile;
            mySourceData.CategoryResourceFile = messageFile;
            mySourceData.ParameterResourceFile = messageFile;
            mySourceData.CategoryCount = 2;

            using (FileStream fs = File.Create(messageFile))
            {
                try
                {
                    EventLog.CreateEventSource(mySourceData);
                    log = EventLog.LogNameFromSourceName(source, ".");
                    using (EventLog myEventLog = new EventLog(log, ".", source))
                    {
                        if (messageFile.Length > 0)
                        {
                            myEventLog.RegisterDisplayName(messageFile, DisplayNameMsgId);
                        }
                        Assert.Equal(log, myEventLog.LogDisplayName);
                    }
                }
                finally
                {
                    EventLog.DeleteEventSource(source);
                    EventLog.Delete(log);
                }
            }

            File.Delete(messageFile);
        }

        [ConditionalFact(typeof(PlatformDetection), nameof(PlatformDetection.IsNotWindowsNanoServer))]
        public void InvalidFormatOrNullLogName()
        {
            Assert.Throws<ArgumentNullException>(() => new EventLog(null));
            Assert.Throws<ArgumentException>(() => new EventLog("?"));
        }

        [ConditionalFact(typeof(PlatformDetection), nameof(PlatformDetection.IsNotWindowsNanoServer))]
        public void EventLog_EnableRaisingEvents_DefaultFalse()
        {
            Assert.False(new EventLog("log").EnableRaisingEvents);
        }

        [ConditionalFact(typeof(Helpers), nameof(Helpers.IsElevatedAndNotWindowsNano))]
        public void InvalidFormatOrNullDeleteLogName()
        {
            Assert.Throws<ArgumentException>(() => EventLog.Delete(null));
            Assert.Throws<InvalidOperationException>(() => EventLog.Delete("?"));
        }

        [ConditionalFact(typeof(Helpers), nameof(Helpers.IsElevatedAndNotWindowsNano))]
        public void InvalidLogExistsLogName()
        {
            Assert.False(EventLog.Exists(null));
        }

        [ConditionalFact(typeof(Helpers), nameof(Helpers.IsElevatedAndNotWindowsNano))]
        public void InvalidMachineName()
        {
            Assert.Throws<ArgumentException>(() => EventLog.Exists("Application", ""));
            Assert.Throws<ArgumentException>(() => EventLog.Delete("", ""));
            Assert.Throws<ArgumentException>(() => EventLog.DeleteEventSource("", ""));
        }

        [ConditionalFact(typeof(Helpers), nameof(Helpers.IsElevatedAndNotWindowsNano))]
        public void LogDisplayNameDefault()
        {
            string source = "Source_" + nameof(LogDisplayNameDefault);
            string log = "MyLogDisplay";
            try
            {
                EventLog.CreateEventSource(source, log);
                using (EventLog eventlog = new EventLog())
                {
                    eventlog.Source = source;
                    Assert.Equal(log, eventlog.LogDisplayName);
                }
            }
            finally
            {
                EventLog.DeleteEventSource(source);
                EventLog.Delete(log);
            }
        }
    }
}
