using Microsoft.Win32.SafeHandles;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

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
                        //if (t.Item1 == "FCR_OK") UpdateSettingsJSON(Tuple.Create(0, (DateTime?)null, (DateTime?)null, (DateTime?)null, (DateTime?)DateTime.Now));
                    }
                    //if (File.Exists(t.Item4)) MoveToArchive(t.Item4);
                }


                //string from = Data.SGeneral.Instance.Settings.EmailFrom.Trim();
                //string to = Data.SGeneral.Instance.Settings.EmailTo.Trim();
                //string password = Data.SGeneral.Instance.Settings.Password.Trim();
                //if (from == "" || to == "" || password == "") return;

                //string body = $"Crash time: {t.Item3.ToString("dd.MM.yyyy HH:mm:ss")} {Environment.NewLine}" +
                //                $"Machine: {Data.SGeneral.Instance.Settings.MachineName} {Environment.NewLine}" +
                //                $"Username: {Data.SGeneral.Instance.Settings.UserName} {Environment.NewLine}" +
                //                $"------------------ LOG DATA ------------------ {Environment.NewLine}" +
                //                $"{t.Item2} {Environment.NewLine}" +
                //                $"------------------ LOG DATA ------------------";

               


          
            }).Start();
        }

        private Data.Email GenerateEmail(string filePath)
        {
            JObject o = JObject.Parse(File.ReadAllText(filePath));
            Data.Crash s = JsonConvert.DeserializeObject<Data.Crash>(o.ToString());

            return new Data.Email
            {
                EmailSubject = "",
                EmailBody = "",
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


    }
}
