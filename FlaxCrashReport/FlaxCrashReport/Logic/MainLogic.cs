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


        internal static void CheckAndProcessLogs()
        {
            try
            {
                using (CrashProcess oCrashProcess = new CrashProcess())
                {
                    oCrashProcess.ProcessEventLogsIntoJSONFiles();
                };

                using (EmailProcess oEmailProcess = new EmailProcess())
                {
                    oEmailProcess.ProcessAndSendEmails();
                }

                //UPDATE JSON FILE FROM SETTINGS
                //MOVE ZIP FILES TO ARCHIVE
                //DELETE JSONs

            }
            catch (Exception ex)
            {
                //CreateServiceCrashReport(ex);
            }
        }




        internal static void CheckForServiceStatus()
        {
            if (DateTime.Now.Hour >= 12
                   && DateTime.Now.Minute >= 0
                   && DateTime.Now >= Data.Settings.LastOKStatus.AddDays(1))
            {
                DateTime okDate = DateTime.Now.Date.Add(new TimeSpan(12, 0, 0));
                SendEmail(Tuple.Create("FCR_OK", "", okDate, ""));
            }
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
                string filepath = Data.Settings.Instance.Settings.ReportsPath + @"\FCR_CRASH.json";
                if ((Data.Settings.Instance.Settings.LastServiceCrash.AddDays(1) > DateTime.Now) && File.Exists(filepath)) return;
                Data.Crash jd = new Data.Crash();

                if (File.Exists(filepath))
                {
                    JObject o1 = JObject.Parse(File.ReadAllText(filepath));
                    jd = JsonConvert.DeserializeObject<Data.Crash>(o1.ToString());
                }


                jd.MachineName = Data.Settings.Instance.Settings.MachineName;
                jd.UserName = Data.Settings.Instance.Settings.UserName;
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
        /// Moves file to archive (C:\FLAX\Services\FCR\Archive\)
        /// If file exists it will be overwriten
        /// </summary>
        /// <param name="file">Path of the file to be moved to archive</param>
        private static void MoveToArchive(string file)
        {
            string tmpFile = Data.Settings.Instance.Settings.ArchivePath + @"\" + Path.GetFileName(file);
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

            var json = JsonConvert.SerializeObject(Data.Settings, Formatting.Indented);
            File.WriteAllText(@"C:\FLAX\Settings\GlobalSettings.json", json);

        }

    }
        
}
