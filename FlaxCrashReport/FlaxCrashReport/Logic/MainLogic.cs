using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace FlaxCrashReport.Logic
{
    public static class MainLogic
    {
        /// <summary>
        /// Creates JSON(s) from EventViewer
        /// Sends all JSON(s) from Reports folder to FCR email
        /// Moves sent JSONs to Archive folder
        /// </summary>
        public static void SendCrashData()
        {
            GenerateJSONs();
            foreach (var file in Directory.GetFiles(Data.SGeneral.Instance.Settings.ReportsPath, "*.json"))
            {
                if (!File.Exists(file)) continue;
                JObject o = JObject.Parse(File.ReadAllText(file));
                Data.JsonData s = JsonConvert.DeserializeObject<Data.JsonData>(o.ToString());
                SendEmail(Tuple.Create(s.Subject, s.Body, s.Date, file));
            }
        }

        /// <summary>
        /// Send email to main FCR email, each function call is in new thread to speed the process
        /// Tupe parameters:
        /// Item1 => Subject string
        /// Item2 => Body string
        /// Item3 => Date DateTime
        /// Item4 => Path string (filepath for attachment)
        /// </summary>
        /// <param name="t"></param>
        public static void SendEmail(Tuple<string, string, DateTime, string> t)
        {
            new System.Threading.Thread(() =>
            {
                string from = Data.SGeneral.Instance.Settings.EmailFrom.Trim();
                string to = Data.SGeneral.Instance.Settings.EmailTo.Trim();
                string password = Data.SGeneral.Instance.Settings.Password.Trim();
                if (from == "" || to == "" || password == "") return;

                string body = $"Crash time: {t.Item3.ToString("dd.MM.yyyy HH:mm:ss")} {Environment.NewLine}" +
                                $"Machine: {Data.SGeneral.Instance.Settings.MachineName} {Environment.NewLine}" +
                                $"Username: {Data.SGeneral.Instance.Settings.UserName} {Environment.NewLine}" +
                                $"------------------ LOG DATA ------------------ {Environment.NewLine}" +
                                $"{t.Item2} {Environment.NewLine}" +
                                $"------------------ LOG DATA ------------------";

                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(new MailAddress(from, "FCR Service").Address, password)

                };


                using (var message = new MailMessage(new MailAddress(from, "FCR Service"), new MailAddress(to, "FCR Report Center"))
                {
                    Subject = t.Item1,
                    Body = body,
                    Priority = MailPriority.High
                })
                {
                    if (File.Exists(t.Item4)) message.Attachments.Add(new Attachment(t.Item4));
                    smtp.Send(message);
                    if (t.Item1 == "FCR_OK") UpdateSettingsJSON(Tuple.Create(0, (DateTime?)null, (DateTime?)null, (DateTime?)null, (DateTime?)DateTime.Now));
                }
                if (File.Exists(t.Item4)) MoveToArchive(t.Item4);
            }).Start();

        }

        /// <summary>
        /// Makes FCR_CRASH.json if not exist or older than 24Hours to prevent email spamming 
        /// Updates global Settings JSON about last FCR_CRASH email sent
        /// </summary>
        /// <param name="ex">Global exception thrown by the service</param>
        public static void CreateServiceCrashReport(Exception ex)
        {
            try
            {
                string filepath = Data.SGeneral.Instance.Settings.ReportsPath + @"\FCR_CRASH.json";
                if ((Data.SGeneral.Instance.Settings.LastServiceCrash.AddDays(1) > DateTime.Now) && File.Exists(filepath)) return;
                Data.JsonData jd = new Data.JsonData();

                if (File.Exists(filepath))
                {
                    JObject o1 = JObject.Parse(File.ReadAllText(filepath));
                    jd = JsonConvert.DeserializeObject<Data.JsonData>(o1.ToString());
                }
               

                jd.MachineName = Data.SGeneral.Instance.Settings.MachineName;
                jd.UserName = Data.SGeneral.Instance.Settings.UserName;
                jd.Date = DateTime.Now;
                jd.Subject = "FCR_CRASH_REPORT";
                jd.Body = ex.StackTrace;

                var serializerSettings = new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                };
                var json = JsonConvert.SerializeObject(jd, serializerSettings);
                File.WriteAllText(filepath, json);
                UpdateSettingsJSON(Tuple.Create(0, (DateTime?)DateTime.Now, (DateTime?)null, (DateTime?)null, (DateTime?)null));


            }
            catch (Exception)
            {
                // To prevent infinite loops and email spamming
                // Information (crash) will be lost here
            }
           
        }

        /// <summary>
        /// Collects data from EventViewer and makes JSON files in Reports folder to be send by email
        /// This functions 
        /// </summary>
        private static void GenerateJSONs()
        {
            DateTime? crashdateFlax = null;
            DateTime? crashdateApp = null;

            List<EventLogEntry> elgs = GetLogData();
            if (elgs == null || elgs.Count() < 1) return;
            foreach ( EventLogEntry e in elgs)
            {
                if (e.Source == "FLAX")
                    crashdateFlax = e.TimeWritten;
                else
                    crashdateApp = e.TimeWritten;

                Data.JsonData jd = new Data.JsonData
                {
                    MachineName = Data.SGeneral.Instance.Settings.MachineName,
                    UserName = Data.SGeneral.Instance.Settings.UserName,
                    Date = e.TimeWritten,
                    Subject = "CRASH_REPORT: " + Data.SGeneral.Instance.Settings.Counter++,
                    Body = e.Message.ToString()
                };

                var serializerSettings = new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                };
                var json = JsonConvert.SerializeObject(jd, serializerSettings);
                string newreport = Data.SGeneral.Instance.Settings.ReportsPath + @"\Report_" + e.TimeWritten.ToString("yyyyMMddHHmmss") + ".json";
                File.WriteAllText(newreport, json);
            }
            UpdateSettingsJSON(Tuple.Create(1, (DateTime?)null, crashdateApp, crashdateFlax, (DateTime?)null));
        }

        /// <summary>
        /// Get all logs from System EventViewer made by Flax or System itself
        /// </summary>
        /// <returns>List of EventLogEntrys for specific app</returns>
        private static List<EventLogEntry> GetLogData()
        {
            // This is the log that is created by a Flax software
            EventLog el1 = new EventLog("FLAX");
            var FlaxLog =  (from EventLogEntry elog in el1.Entries
                    where elog.TimeWritten > Data.SGeneral.Instance.Settings.LastFlaxCrash
                    && elog.EntryType == EventLogEntryType.Error
                    orderby elog.TimeGenerated ascending
                    select elog).ToList();

            // The rest of logs that is created by the System
            EventLog el2 = new EventLog("Application");
            var AppLog = (from EventLogEntry elog in el2.Entries
                       where elog.TimeWritten > Data.SGeneral.Instance.Settings.LastAppCrash
                       && elog.Message.ToString().Contains("Application: Users.exe")
                       && elog.EntryType == EventLogEntryType.Error
                       orderby elog.TimeGenerated ascending
                       select elog).ToList();

            return FlaxLog.Union(AppLog).ToList();

        }

        /// <summary>
        /// Moves file to archive (C:\FLAX\Services\FCR\Archive\)
        /// If file exists it will be overwriten
        /// </summary>
        /// <param name="file">Path of the file to be moved to archive</param>
        private static void MoveToArchive(string file)
        {
            string tmpFile = Data.SGeneral.Instance.Settings.ArchivePath + @"\" + Path.GetFileName(file);
            if (File.Exists(tmpFile)) File.Delete(tmpFile);
            File.Move(file, tmpFile);
        }

        /// <summary>
        /// Update global json and singleton class (SGeneral) with new data
        /// Tuple parameters:
        /// Item1 => Counter (0 or 1) 1 if app crash report, 0 if FCR report
        /// Item2 => LastServiceCrash DateTime?
        /// Item3 => LastAppCrash DateTime?
        /// Item4 => LastFlaxCrash DateTime?
        /// Item5 => LastOKStatus DateTime?
        /// </summary>
        /// <param name="t"></param>
        private static void UpdateSettingsJSON(Tuple<int, DateTime?, DateTime?, DateTime?, DateTime?> t)
        {
            Data.GlobalSettings gs = Data.SGeneral.Instance.Settings;

            gs.Counter += t.Item1;
            gs.LastServiceCrash = t.Item2 ?? gs.LastServiceCrash;
            gs.LastAppCrash = t.Item3 ?? gs.LastAppCrash;
            gs.LastFlaxCrash = t.Item4 ?? gs.LastFlaxCrash;
            gs.LastOKStatus = t.Item5 ?? gs.LastOKStatus;

            var json = JsonConvert.SerializeObject(gs, Formatting.Indented);
            File.WriteAllText(@"C:\FLAX\Settings\GlobalSettings.json", json);

        }
      
    }
}
