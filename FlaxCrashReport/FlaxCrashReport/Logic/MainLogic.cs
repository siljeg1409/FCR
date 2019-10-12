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
    public class MainLogic
    {

        public MainLogic()
        {
        }

        public void SendEmail(string subject, string body = "")
        {
            try
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
            catch (Exception e)
            {
                GenerateFCRJSON("FCR_SERVICE_SEND_EMAIL_ERROR","ORIGINAL BODY: " + Environment.NewLine + body + Environment.NewLine + "SEND EMAIL EXCEPTION: " + Environment.NewLine + e.StackTrace);
            }

        }

        private void GenerateFCRJSON(string v1, string v2)
        {
            try
            {
                Data.JsonData jd = new Data.JsonData
                {
                    MachineName = Data.SGeneral.Instance.Settings.MachineName,
                    UserName = Data.SGeneral.Instance.Settings.UserName,
                    Date = DateTime.Now,
                    Subject = v1,
                    Body = v2
                };

                var serializerSettings = new JsonSerializerSettings();
                serializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                var json = JsonConvert.SerializeObject(jd, serializerSettings);
                string filepath = @"C:\FLAX\FCR\Reports\FCR_CRASH.json";
                File.WriteAllText(filepath, json);
            }
            catch (Exception)
            {
               // if this fails, i will loose error data
               //I cant allow this to go on global exception because it will eventaully call this function again -> infinite loop -> bad :(
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
            UpdateJSON(crashdate);
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
            checkFolder(reportsFolder);
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
            checkFolder(archiveFolder);
            File.Move(file, archiveFolder + Path.GetFileName(file));
        }

        private void UpdateJSON(DateTime d)
        {
            string filepath = @"C:\FLAX\Settings\GlobalSettings.json";
            checkFolder(filepath);
            JObject o = JObject.Parse(File.ReadAllText(filepath));
            o["fcr_counter"] = (int)o["fcr_counter"] + 1;
            o["fcr_lastcrash"] = d;
            string output = Newtonsoft.Json.JsonConvert.SerializeObject(o, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(@"C:\FLAX\Settings\GlobalSettings.json", output);
        }

        private void checkFolder(string path)
        {
            path = Path.GetDirectoryName(path);
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        }
    }
}
