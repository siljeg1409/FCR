using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.IO;
using System.Net;
using System.Net.Mail;

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
                GenerateJSON(e.StackTrace);
                return false;
            }
           

            return true;
        }

        private void GenerateJSON(string msg = "")
        {
            Data.JsonData jd = new Data.JsonData
            {
                MachineName = Data.SGeneral.Instance.MachineName,
                UserName = Data.SGeneral.Instance.UserName,
                Date = DateTime.Now,
                Subject = "CRASH_REPORT: " + Data.SGeneral.Instance.Counter++,
                Body = msg == "" ? GetBodyData() : msg
        };

        var serializerSettings = new JsonSerializerSettings();
        serializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        var json = JsonConvert.SerializeObject(jd, serializerSettings);
        File.WriteAllText(@"C:\FLAX\FCR\Reports\Report_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".json", json);
        }

        private string GetBodyData()
        {
            throw new NotImplementedException();
        }


        public bool CheckAppStatus()
        {


            return true;
        }

    }
}
