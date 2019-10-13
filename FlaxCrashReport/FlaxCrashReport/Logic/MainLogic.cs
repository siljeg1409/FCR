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
        public static void SendCrashData()
        {
            GenerateJSONs();
            foreach (var file in Directory.GetFiles(Data.SGeneral.Instance.Settings.ReportsPath, "*.json"))
            {
                if (!File.Exists(file)) continue;
                JObject o = JObject.Parse(File.ReadAllText(file));
                Data.JsonData s = JsonConvert.DeserializeObject<Data.JsonData>(o.ToString());
                SendEmail(s.Subject, s.Body + $"{ Environment.NewLine}{ Environment.NewLine}{ Environment.NewLine} FILE: {Path.GetFileName(file)}", s.Date);
                MoveToArchive(file);
            }
        }

        public static void SendEmail(string subject, string body = "", object dt = null)
        {
            string from = Data.SGeneral.Instance.Settings.EmailFrom.Trim();
            string to = Data.SGeneral.Instance.Settings.EmailTo.Trim();
            string password = Data.SGeneral.Instance.Settings.Password.Trim();
            if ( from == "" || to == "" || password == "")
            {
                return;
            }
            if (dt == null) dt = DateTime.Now;

            body = $"Crash time: {((DateTime)dt).ToString("dd.MM.yyyy HH:mm:ss")} {Environment.NewLine}" +
                $"Machine: {Data.SGeneral.Instance.Settings.MachineName} {Environment.NewLine}" +
                $"Username: {Data.SGeneral.Instance.Settings.UserName} {Environment.NewLine}" +
                $"------------------ LOG DATA ------------------ {Environment.NewLine}" +
                $"{body} {Environment.NewLine}" +
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
                Subject = subject,
                Body = body,
                Priority = MailPriority.High

            })
            {
                smtp.Send(message);
            }

        }

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

                var serializerSettings = new JsonSerializerSettings();
                serializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                var json = JsonConvert.SerializeObject(jd, serializerSettings);
                File.WriteAllText(filepath, json);
                UpdateSettingsJSON(true, DateTime.Now, DateTime.Now);


            }
            catch (Exception)
            {
                // To prevent infinite loops and email spamming
                // Information (crash) will be lost here
            }
           
        }

        private static void GenerateJSONs()
        {
            DateTime crashdateFlax = DateTime.Now;
            DateTime crashdateApp = DateTime.Now;
            List<EventLogEntry> elgs = GetLogData();
            if (elgs == null || elgs.Count() < 1) return;
            foreach ( EventLogEntry e in elgs)
            {
                if (e.Source == "FLAX") crashdateFlax = e.TimeWritten;
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

                var serializerSettings = new JsonSerializerSettings();
                serializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                var json = JsonConvert.SerializeObject(jd, serializerSettings);
                string newreport = Data.SGeneral.Instance.Settings.ReportsPath + @"\Report_" + e.TimeWritten.ToString("yyyyMMddHHmmss") + ".json";
                File.WriteAllText(newreport, json);
            }
            UpdateSettingsJSON(false, crashdateFlax, crashdateApp);
        }

        private static List<EventLogEntry> GetLogData()
        {
            EventLog el1 = new EventLog("FLAX");
            var FlaxLog =  (from EventLogEntry elog in el1.Entries
                    where elog.TimeWritten > Data.SGeneral.Instance.Settings.LastFlaxCrash
                    && elog.EntryType == EventLogEntryType.Error
                    orderby elog.TimeGenerated ascending
                    select elog).ToList();

            // The rest of logs that is created by the system
            EventLog el2 = new EventLog("Application");
            var AppLog = (from EventLogEntry elog in el2.Entries
                       where elog.TimeWritten > Data.SGeneral.Instance.Settings.LastAppCrash
                       && elog.Message.ToString().Contains("Application: Users.exe")
                       && elog.EntryType == EventLogEntryType.Error
                       orderby elog.TimeGenerated ascending
                       select elog).ToList();

            return FlaxLog.Union(AppLog).ToList();

        }

        private static void MoveToArchive(string file)
        {
            string tmpFile = Data.SGeneral.Instance.Settings.ArchivePath + @"\" + Path.GetFileName(file);
            if (File.Exists(tmpFile)) File.Delete(tmpFile);
            File.Move(file, tmpFile);
        }

        private static void UpdateSettingsJSON(bool fcrcrash, DateTime dflax, DateTime dapp)
        {
            Data.GlobalSettings gs = Data.SGeneral.Instance.Settings;
            if (fcrcrash)
                gs.LastServiceCrash = DateTime.Now;
            else
            {
                gs.Counter += 1;
                gs.LastAppCrash = dapp;
                gs.LastFlaxCrash = dflax;
            }
            var json = JsonConvert.SerializeObject(gs, Formatting.Indented);
            File.WriteAllText(@"C:\FLAX\Settings\GlobalSettings.json", json);

        }
      
    }
}
