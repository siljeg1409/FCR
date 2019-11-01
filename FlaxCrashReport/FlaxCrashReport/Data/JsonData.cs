using System;

namespace FlaxCrashReport.Data
{
    /// <summary>
    /// Main Class for JSON file serialization
    /// TODO: Gather more data here and send thsi file with email as attachment
    /// </summary>
    public class JsonData
    {

        #region Private members
        private string category;
        private string entityType;
        private string machineName;
        private string message;
        private string source;
        private DateTime timeGenerated;
        private DateTime timeWritten;
        private string userName;
        #endregion

        public string Category
        {
            get { return category; }
            set
            {
                if (value != category)
                {
                    category = value;
                }
            }
        }

        public string EntityType
        {
            get { return entityType; }
            set
            {
                if (value != entityType)
                {
                    entityType = value;
                }
            }
        }

        public string MachineName
        {
            get { return machineName; }
            set
            {
                if (value != machineName)
                {
                    machineName = value;
                }
            }
        }

        public string Message
        {
            get { return message; }
            set
            {
                if (value != message)
                {
                    message = value;
                }
            }
        }

        public string Source
        {
            get { return source; }
            set
            {
                if (value != source)
                {
                    source = value;
                }
            }
        }

        public DateTime TimeGenerated
        {
            get { return timeGenerated; }
            set
            {
                if (value != timeGenerated)
                {
                    timeGenerated = value;
                }
            }
        }

        public DateTime TimeWritten
        {
            get { return timeWritten; }
            set
            {
                if (value != timeWritten)
                {
                    timeWritten = value;
                }
            }
        }

        public string UserName
        {
            get { return userName; }
            set
            {
                if (value != userName)
                {
                    userName = value;
                }
            }
        }

    }
}
