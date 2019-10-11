using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace FlaxCrashReport.Logic
{
    public class MainLogic
    {

        public MainLogic()
        {
        }

        public bool SendEmail(string subject, string body)
        {
            try
            {
                var fromAddress = new MailAddress(Data.SGeneral.Instance.EmailFrom, "FCR Service");
                var toAddress = new MailAddress(Data.SGeneral.Instance.EmailTo, "FCR Report Center");
                string fromPassword = Data.SGeneral.Instance.Password;

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
                    Body = $"Date: { DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")} {Environment.NewLine}" +
                    $"Machine: {Data.SGeneral.Instance.MachineName} {Environment.NewLine}" +
                    $"Username: {Data.SGeneral.Instance.UserName} {Environment.NewLine}" + 
                    $"------------------ LOG DATA ------------------" +
                    $"{body}" +
                    $"------------------ LOG DATA ------------------" 

                })
                {
                    smtp.Send(message);
                }

            }
            catch (Exception e)
            {
                GenerateJSON("SERVICE_SENDEMAIL_ERROR", e.StackTrace);
                return false;
            }
           

            return true;
        }

        private void GenerateJSON(string subject = "", string msg = "", bool recusion = false)
        {
            try
            {
                EventLogEntry elg = GetLogData();
                DateTime crashdate = elg.TimeGenerated == null ? DateTime.Now : elg.TimeGenerated;

                Data.JsonData jd = new Data.JsonData
                {
                    MachineName = Data.SGeneral.Instance.MachineName,
                    UserName = Data.SGeneral.Instance.UserName,
                    Date = crashdate,
                    Subject = subject == "" ? "CRASH_REPORT: " + Data.SGeneral.Instance.Counter++ : subject,
                    Body = msg == "" ? elg.Message.ToString() : msg
                };

                var serializerSettings = new JsonSerializerSettings();
                serializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                var json = JsonConvert.SerializeObject(jd, serializerSettings);
                File.WriteAllText(@"C:\FLAX\FCR\Reports\Report_" + crashdate.ToString("yyyyMMddHHmmss") + ".json", json);
                UpdateJSON(crashdate);
            }
            catch (Exception e)
            {
                if (recusion) return;
                GenerateJSON("SERVICE_WRITE_JSON_REPORT_ERROR", e.StackTrace, true);
            }
           
        }

        private EventLogEntry GetLogData()
        {
            EventLog el = new EventLog("Application");
            try
            {
                return (from EventLogEntry elog in el.Entries
                              where (elog.Message.ToString().StartsWith("Application: WindowsFormsApp1.exe"))
                              && elog.TimeGenerated > Data.SGeneral.Instance.LastCrash
                              orderby elog.TimeGenerated descending
                              select elog).First();
            }
            catch (InvalidOperationException e)
            {
                GenerateJSON("SERVICE_READ_LOG_ENTRY_ERROR", e.StackTrace);
                return null;
            }
        }


        public bool CheckAppStatus()
        {
            Process[] pname = Process.GetProcessesByName("Users");
            if (pname.Length == 0) return false;
            return true;
        }

        public void SendCrashData()
        {
            GenerateJSON();

            foreach (var file in Directory.GetFiles(@"C:\FLAX\FCR\Reports\", "*.json"))
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
            if (!Directory.Exists(archiveFolder)) Directory.CreateDirectory(archiveFolder);
            File.Move(file, archiveFolder + Path.GetFileName(file));
        }

        private void UpdateJSON(DateTime d)
        {
            string filepath = @"C:\FLAX\Settings\GlobalSettings.json";
            if (!File.Exists(filepath))
            {
                var ml = new Logic.MainLogic();
                ml.SendEmail("NO_SETTINGS_FILE", "");
            }

            JObject o = JObject.Parse(File.ReadAllText(filepath));
            o["FCR_Counter"] = (int)o["FCR_Counter"] + 1;
            o["FCR_LastCrash"] = d;
            string output = Newtonsoft.Json.JsonConvert.SerializeObject(o, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(@"C:\FLAX\Settings\GlobalSettings.json", output);

        }
    }
}
