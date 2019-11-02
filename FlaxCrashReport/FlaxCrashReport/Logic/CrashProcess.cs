using Microsoft.Win32.SafeHandles;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace FlaxCrashReport.Logic
{
    public class CrashProcess : IDisposable
    {
        #region Private members
        private bool _disposed = false;
        private SafeHandle handle = new SafeFileHandle(IntPtr.Zero, true);
        private List<EventLogEntry> _eventLogs;
        private DateTime _lastApplicationCrashTime;
        private string _machineName;
        private string _userName;
        #endregion

        public CrashProcess()
        {
            _machineName = Data.Settings.MachineName;
            _userName = Data.Settings.UserName;
            _lastApplicationCrashTime = Data.Settings.LastAppCrash;
        }

        /// <summary>
        /// Get all logs from System EventViewer made by System
        /// Generate JSON files on the global path
        /// </summary>
        public void ProcessEventLogsIntoJSONFiles()
        {
            _eventLogs = GetEventLogs();
            if (_eventLogs == null || _eventLogs.Count() < 1) return;
            SetLastApplicationCrashTime();
            GenerateJSONFiles();
        }

        private List<EventLogEntry> GetEventLogs()
        {
            EventLog el2 = new EventLog("Application");
            return (from EventLogEntry elog in el2.Entries
                         where elog.TimeWritten > _lastApplicationCrashTime
                         && elog.Message.ToString().Contains("Application: Users.exe") // <---- I know it's ugly
                         && elog.EntryType == EventLogEntryType.Error
                         orderby elog.TimeWritten ascending
                         select elog).ToList();
        }

        private void SetLastApplicationCrashTime()
        {
            _lastApplicationCrashTime = _eventLogs.Max(m => m.TimeWritten);
        }


        /// <summary>
        /// Collects data from EventViewer and makes JSON files in Reports folder to be send by email
        /// This functions 
        /// </summary>
        private void GenerateJSONFiles()
        {

            foreach (EventLogEntry e in _eventLogs)
            {
                Data.Crash oCrash = GenerateJSONLogData(e);

                var serializerSettings = new JsonSerializerSettings{ ContractResolver = new CamelCasePropertyNamesContractResolver() };
                var json = JsonConvert.SerializeObject(oCrash, serializerSettings);
                string newreport = $"{Data.Settings.ReportsPath}\\Report_{e.TimeWritten.ToString("yyyyMMddHHmmss")}.json";

                File.WriteAllText(newreport, json);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ele"></param>
        /// <returns></returns>
        private Data.Crash GenerateJSONLogData(EventLogEntry ele)
        {
            return new Data.Crash
            {
                Category = ele.Category.ToString(),
                EntityType = ele.EntryType.ToString(),
                MachineName = ele.MachineName,
                Message = ele.Message.ToString(),
                Source = ele.Source.ToString(),
                TimeGenerated = ele.TimeGenerated,
                TimeWritten = ele.TimeWritten,
                UserName = ele.UserName
            };
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                handle.Dispose();
            }
            _disposed = true;
        }

    }
}
