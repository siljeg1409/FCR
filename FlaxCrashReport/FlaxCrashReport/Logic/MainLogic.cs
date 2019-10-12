﻿using Newtonsoft.Json;
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
    public class MainLogic
    {

        public MainLogic()
        {
        }

        public void SendEmail(string subject, string body = "")
        {
            var fromAddress = new MailAddress(Data.SGeneral.Instance.Settings.EmailFrom, "FCR Service");
            var toAddress = new MailAddress(Data.SGeneral.Instance.Settings.EmailTo, "FCR Report Center");
            string fromPassword = Data.SGeneral.Instance.Settings.Password;
            body = $"Crash time: {Data.SGeneral.Instance.Settings.LastCrash.ToString("dd.MM.yyyy HH:mm:ss")} {Environment.NewLine}" +
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
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };
            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body

            })
            {
                smtp.Send(message);
            }

        }

        public void createServiceCrashReport(System.Exception ex)
        {
            try
            {
                if (Data.SGeneral.Instance.Settings.LastServiceCrash.AddDays(1) > DateTime.Now) return;
                Data.JsonData jd = new Data.JsonData();
                string filepath = @"C:\FLAX\FCR\Reports\FCR_CRASH.json";

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
                UpdateSettingsJSON(DateTime.Now, true);


            }
            catch (Exception)
            {
                // To prevent infinite loops and email spamming
                // Information (crash) will be lost here
            }
           
        }



        private void GenerateJSONs()
        {
            DateTime crashdate = DateTime.Now;
            List<EventLogEntry> elgs = GetLogData();
            if (elgs == null) return;
            foreach ( EventLogEntry e in elgs)
            {
                crashdate = e.TimeGenerated;
                Data.JsonData jd = new Data.JsonData
                {
                    MachineName = Data.SGeneral.Instance.Settings.MachineName,
                    UserName = Data.SGeneral.Instance.Settings.UserName,
                    Date = crashdate,
                    Subject = "CRASH_REPORT: " + Data.SGeneral.Instance.Settings.Counter++,
                    Body = e.Message.ToString()
                };

                var serializerSettings = new JsonSerializerSettings();
                serializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                var json = JsonConvert.SerializeObject(jd, serializerSettings);
                File.WriteAllText(@"C:\FLAX\FCR\Reports\Report_" + crashdate.ToString("yyyyMMddHHmmss") + ".json", json);
            }
            UpdateSettingsJSON(crashdate);
        }


        private List<EventLogEntry> GetLogData()
        {
            EventLog el = new EventLog("Application");
            return  (from EventLogEntry elog in el.Entries
                            where (elog.Message.ToString().StartsWith("Application: Users.exe"))
                            && elog.TimeGenerated > Data.SGeneral.Instance.Settings.LastCrash
                            & elog.EntryType == EventLogEntryType.Error
                            orderby elog.TimeGenerated descending
                            select elog).ToList();
        }

        public void SendCrashData()
        {
            GenerateJSONs();
            string reportsFolder = @"C:\FLAX\FCR\Reports\";
            CheckFolder(reportsFolder);
            foreach (var file in Directory.GetFiles(reportsFolder, "*.json"))
            {
                if (!File.Exists(file)) continue;
                JObject o = JObject.Parse(File.ReadAllText(file));
                Data.JsonData s = JsonConvert.DeserializeObject<Data.JsonData>(o.ToString());
                SendEmail(s.Subject, s.Body);
                MoveToArchive(file);
            }
        }

        private void MoveToArchive(string file)
        {
            string archiveFolder = @"C:\FLAX\FCR\Archive\";
            CheckFolder(archiveFolder);
            File.Move(file, archiveFolder + Path.GetFileName(file));
        }

        private void UpdateSettingsJSON(DateTime d, bool fcrcrash = false)
        {
            string filepath = @"C:\FLAX\Settings\GlobalSettings.json";
            CheckFolder(filepath);
            JObject o = JObject.Parse(File.ReadAllText(filepath));
            o["fcr_counter"] = (int)o["fcr_counter"] + 1;
            o["fcr_lastcrash"] = d;
            if(fcrcrash) o["fcr_lastservicecrash"] = DateTime.Now;
            string output = Newtonsoft.Json.JsonConvert.SerializeObject(o, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(@"C:\FLAX\Settings\GlobalSettings.json", output);
        }

        private void CheckFolder(string path)
        {
            path = Path.GetDirectoryName(path);
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        }
    }
}
