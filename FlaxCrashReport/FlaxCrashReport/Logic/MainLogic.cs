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

                //THIS WILL ONLY EXECUTE IF ALL JSON FILES ARE SEND VIA EMAIL
                //UPDATE JSON FILE FROM SETTINGS
                //MOVE ZIP FILES TO ARCHIVE 
                //DELETE JSONs
                //SEND JSON OK STATUS

            }
            catch (Exception ex)
            {
                CreateAndSendFCREmail(ex.StackTrace, Enumerations.EStatus.EmailSubjectStatus.FCR_CRASH_REPORT);
            }
        }

        public static void CreateAndSendFCREmail(string message, Enumerations.EStatus.EmailSubjectStatus subjectStatus)
        {
            try
            {
                Data.Email email = new Data.Email
                {
                    EmailSubject = $"{subjectStatus.ToString()} : {++Data.Settings.Instance.Counter}",
                    EmailBody = $"Crash time: {DateTime.Now}{Environment.NewLine}" +
                          $"Machine: {Data.Settings.Instance.MachineName}{Environment.NewLine}" +
                          $"User: {Data.Settings.Instance.UserName}{Environment.NewLine}" +
                          $"Message: {Environment.NewLine} {message}"
                };

                using (EmailProcess oEmailProcess = new EmailProcess())
                {
                    oEmailProcess.SendEmail(email);
                }
            }
            catch (Exception ex)
            {
                CreateServiceCrashReportJSON($"Original message:{Environment.NewLine}{message}{Environment.NewLine}Send email failed:{Environment.NewLine}{ex.StackTrace}");
            }

        }


        /// <summary>
        /// Makes FCR_CRASH.json if not exist or older than 24Hours to prevent email spamming 
        /// Updates global Settings JSON about last FCR_CRASH email sent
        /// </summary>
        /// <param name="ex">Global exception thrown by the service</param>
        public static void CreateServiceCrashReportJSON(string message)
        {
            try
            {
                string serviceCrashJSONFile = $"{Data.Settings.Instance.ReportsPath}\\FCR_CRASH.json";

                Data.Crash oCrash = new Data.Crash
                {
                    Category = "FCR",
                    EntityType = "FCR",
                    MachineName = Data.Settings.Instance.MachineName,
                    Message = message,
                    Source = "FlaxCrashReport",
                    TimeGenerated = DateTime.Now,
                    TimeWritten = DateTime.Now,
                    UserName = Data.Settings.Instance.UserName,
                    CrashCounter = ++Data.Settings.Instance.Counter
                };

                var serializerSettings = new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                };
                var json = JsonConvert.SerializeObject(oCrash, serializerSettings);
                File.WriteAllText(serviceCrashJSONFile, json);

            }
            catch (Exception)
            {
                // To prevent infinite loops and email spamming
                // Information (crash) could be lost here
            }

        }





        /// <summary>
        /// Moves file to archive (C:\FLAX\Services\FCR\Archive\)
        /// If file exists it will be overwriten
        /// </summary>
        /// <param name="file">Path of the file to be moved to archive</param>
        private static void MoveToArchive(string file)
        {
            string tmpFile = Data.Settings.Instance.ArchivePath + @"\" + Path.GetFileName(file);
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
            var json = JsonConvert.SerializeObject(Data.Settings.Instance, Formatting.Indented);
            File.WriteAllText(@"C:\FLAX\Settings\GlobalSettings.json", json);
        }

    }
        
}
