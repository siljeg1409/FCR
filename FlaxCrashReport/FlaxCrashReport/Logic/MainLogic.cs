using System;
using System.Net;
using System.Net.Mail;

namespace FlaxCrashReport.Logic
{
    public class MainLogic
    {

        public MainLogic()
        {
        }

        public bool SendEmail()
        {
            // foreach json in C:\FLAX\FCR\Reports\ ....JSON(s)
            try
            {
                var fromAddress = new MailAddress(Data.SGlobal.Instance.Settings.EmailFrom, "Flax Crash Report Service");
                var toAddress = new MailAddress(Data.SGlobal.Instance.Settings.EmailTo, "FCR Report Center");
                string fromPassword = Data.SGlobal.Instance.Settings.Password;
                string subject = "Crash report from " + Data.SGlobal.Instance.UserName;
                string body = GetCrashData();

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

                //delete current JSON -> continue
            }
            catch (Exception)
            {

                //IDK what to do here :/
            }
           

            return true;
        }

        private string GetCrashData()
        {
            string ret = "Default information!";

            //need to make email body from windows log data about current app

            return ret;
        }

        public bool CheckAppStatus()
        {


            return true;
        }

    }
}
