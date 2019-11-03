using Microsoft.Win32.SafeHandles;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Mail;
using System.Runtime.InteropServices;

namespace FlaxCrashReport.Logic
{
    public class EmailProcess : IDisposable
    {
        #region Private members
        private bool _disposed = false;
        private SafeHandle handle = new SafeFileHandle(IntPtr.Zero, true);
        private List<Data.Email> _emails;
        #endregion

        internal void ProcessAndSendEmails()
        {
            PrepareEmails();
            SendEmails();
        }

        private void PrepareEmails()
        {
            foreach (var file in Directory.GetFiles(Data.Settings.ReportsPath, "*.json"))
            {
                if (File.Exists(file))
                {
                    _emails.Add(GenerateEmail(file));
                }
            }
        }

        private void SendEmails()
        {
            new System.Threading.Thread(() =>
            {
                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(new MailAddress(Data.Settings.EmailFrom, "FCR Service").Address, Data.Settings.Password)

                };

                foreach (Data.Email email in _emails)
                {
                    using (var message = new MailMessage(new MailAddress(Data.Settings.EmailFrom, "FCR Service"), new MailAddress(Data.Settings.EmailTo, "FCR Report Center"))
                    {
                        Subject = email.EmailSubject,
                        Body = email.EmailBody,
                        Priority = MailPriority.High
                    })
                    {
                        smtp.Send(message);
                    }
                }
          
            }).Start();
        }

        private Data.Email GenerateEmail(string filePath)
        {
            JObject o = JObject.Parse(File.ReadAllText(filePath));
            Data.Crash s = JsonConvert.DeserializeObject<Data.Crash>(o.ToString());

            return new Data.Email
            {
                EmailSubject = $"{EmailSubjectStatus.CRASH_REPORT.ToString()} : {s.CrashCounter}",
                EmailBody = $"Crash time: {s.TimeWritten}{Environment.NewLine}" +
                            $"Machine: {s.MachineName}{Environment.NewLine}" +
                            $"User: {s.UserName}{Environment.NewLine}" +
                            $"Detailed information in attachment.",
                EmailAttachment = new Attachment(ZipJSOINFile(filePath)),
                JSONPath = filePath
            };


        }

        private string ZipJSOINFile(string sourceFileName)
        {
            using (ZipArchive archive = ZipFile.Open(Path.ChangeExtension(sourceFileName, ".zip"), ZipArchiveMode.Create))
            {
                archive.CreateEntryFromFile(sourceFileName, Path.GetFileName(sourceFileName));
            }
            return Path.ChangeExtension(sourceFileName, ".zip");
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

        private enum EmailSubjectStatus
        {
            CRASH_REPORT = 0, //will use just this enum for now, the rest is coming soon
            FCR_OK = 1,
            FCR_CRASH_REPORT = 2,
            FCR_STARTED = 3,
            FCR_STOPPED = 4
        }
    }
}
