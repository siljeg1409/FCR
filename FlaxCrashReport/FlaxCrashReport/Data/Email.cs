using System.Net.Mail;

namespace FlaxCrashReport.Data
{
    public class Email
    {
        public string EmailSubject { get; internal set; }
        public string EmailBody { get; internal set; }
        public Attachment EmailAttachment { get; internal set; }
        public string JSONPath { get; internal set; }

    }
}
