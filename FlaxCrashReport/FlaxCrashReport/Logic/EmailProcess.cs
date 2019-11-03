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
        private List<Data.Email> _emails = new List<Data.Email>();
        private SmtpClient _smtp;
        #endregion

        public EmailProcess()
        {
            GenerateSMTP();
        }

        internal void ProcessAndSendEmails()
        {
            PrepareEmails();
            if (_emails == null || _emails.Count < 1) return;
            SendEmails();
        }

        private void PrepareEmails()
        {
            foreach (var file in Directory.GetFiles(Data.Settings.Instance.ReportsPath, "*.json"))
            {
                if (File.Exists(file))
                {
                    _emails.Add(GenerateEmail(file));
                }
            }
        }

        private void SendEmails()
        {
            //new System.Threading.Thread(() =>
            //{
            foreach (Data.Email email in _emails)
            {
                SendEmail(email);
                //_emails.Remove(email);
            }
            //}).Start();
        }

        private void GenerateSMTP()
        {
            _smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(new MailAddress(Data.Settings.Instance.EmailFrom, "FCR Service").Address, Data.Settings.Instance.Password)

            };
        }
        public void SendEmail(Data.Email email)
        {
            using (var message = new MailMessage(new MailAddress(Data.Settings.Instance.EmailFrom, "FCR Service"), new MailAddress(Data.Settings.Instance.EmailTo, "FCR Report Center"))
            {
                Subject = email.EmailSubject,
                Body = email.EmailBody,
                Priority = MailPriority.High
            })
            {
                if(email.EmailAttachment != null)
                {
                    message.Attachments.Add(email.EmailAttachment);
                }
                _smtp.Send(message);
            }
        }
        private Data.Email GenerateEmail(string filePath)
        {
            JObject o = JObject.Parse(File.ReadAllText(filePath));
            Data.Crash s = JsonConvert.DeserializeObject<Data.Crash>(o.ToString());

            return new Data.Email
            {
                EmailSubject = $"{Enumerations.EStatus.EmailSubjectStatus.CRASH_REPORT.ToString()} : {s.CrashCounter}",
                EmailBody = $"Crash time: {s.TimeWritten}{Environment.NewLine}" +
                            $"Machine: {s.MachineName}{Environment.NewLine}" +
                            $"User: {s.UserName}{Environment.NewLine}" +
                            $"Detailed information in attachment.",
                EmailAttachment = new Attachment(ZipJSOINFile(filePath))
            };
        }

        private string ZipJSOINFile(string sourceFileName)
        {
            string zipFile = Path.ChangeExtension(sourceFileName, ".zip");
            if (File.Exists(zipFile)) return zipFile;
            using (ZipArchive archive = ZipFile.Open(Path.ChangeExtension(sourceFileName, ".zip"), ZipArchiveMode.Create))
            {
                archive.CreateEntryFromFile(sourceFileName, Path.GetFileName(sourceFileName));
            }
            return zipFile;
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
