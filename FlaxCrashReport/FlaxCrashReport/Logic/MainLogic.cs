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
            foreach( Data.Application app in Data.SGeneral.Instance.Settings.AppList)
            {
                GenerateJSONs(app);
            }
            foreach (var file in Directory.GetFiles(Data.SGeneral.Instance.Settings.ReportsPath, "*.json"))
            {
                if (!File.Exists(file)) continue;
                JObject o = JObject.Parse(File.ReadAllText(file));
                Data.JsonData s = JsonConvert.DeserializeObject<Data.JsonData>(o.ToString());
                SendEmail(s.Subject, s.Body, s.Date);
                MoveToArchive(file);
            }
        }

        public static void SendEmail(string subject, string body = "", Object dt = null)
        {
            string from = Data.SGeneral.Instance.Settings.EmailFrom.Trim();
            string to = Data.SGeneral.Instance.Settings.EmailTo.Trim();
            string password = Data.SGeneral.Instance.Settings.Password.Trim();
            if ( from == "" || to == "" || password == "")
            {
                //There is really no need to craete report here, it wont be sent anyway
                //Maybe i should make daily report from service that will send email, each day so i can monitor service that way
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
                if (Data.SGeneral.Instance.Settings.LastServiceCrash.AddDays(1) > DateTime.Now) return;
                Data.JsonData jd = new Data.JsonData();
                string filepath = Data.SGeneral.Instance.Settings.ReportsPath +  @"\FCR_CRASH.json";

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

        private static void GenerateJSONs(Data.Application app)
        {
            DateTime crashdate = DateTime.Now;
            List<EventLogEntry> elgs = GetLogData(app);
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
                string newreport = Data.SGeneral.Instance.Settings.ReportsPath + @"\" + app.AppName.ToUpper() + "_Report_" + crashdate.ToString("yyyyMMddHHmmss") + ".json";
                File.WriteAllText(newreport, json);
            }
            UpdateSettingsJSON(crashdate, false, app);
        }

        private static List<EventLogEntry> GetLogData(Data.Application app)
        {
            EventLog el = new EventLog("Application");
            return  (from EventLogEntry elog in el.Entries
                            where (elog.Message.ToString().StartsWith("Application: " + app.AppName + ".exe"))
                            && elog.TimeGenerated > app.AppCrashTime
                            & elog.EntryType == EventLogEntryType.Error
                            select elog).ToList();
        }

        private static void MoveToArchive(string file)
        {
            File.Move(file, Data.SGeneral.Instance.Settings.ArchivePath + Path.GetFileName(file));
        }

        private static void UpdateSettingsJSON(DateTime d, bool fcrcrash = false, Data.Application app = null)
        {
            string filepath = @"C:\FLAX\Settings\GlobalSettings.json";

            JObject jo = JObject.Parse(File.ReadAllText(filepath));
            Data.GlobalSettings gs = JsonConvert.DeserializeObject<Data.GlobalSettings>(jo.ToString());

            if(app != null)
            {
                Data.Application a = gs.AppList.FirstOrDefault(w => w.AppName == app.AppName);
                if(a != null) a.AppCrashTime = d;

            }
            
            if (fcrcrash)
                gs.LastServiceCrash = d;
            else
                gs.Counter += 1;

            var json = JsonConvert.SerializeObject(gs, Formatting.Indented);
            File.WriteAllText(@"C:\FLAX\Settings\GlobalSettings.json", json);

        }
      
    }
}
