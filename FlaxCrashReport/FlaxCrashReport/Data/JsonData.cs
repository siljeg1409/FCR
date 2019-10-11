using System;

namespace FlaxCrashReport.Data
{
    public class JsonData
    {

        #region Private members
        private DateTime _Date;
        private string _MachineName;
        private string _UserName;
        private string _Subject;
        private string _Body;
        #endregion

        public string Subject
        {
            get { return _Subject; }
            set
            {
                if (value != _Subject)
                {
                    _Subject = value;
                }
            }
        }

        public string Body
        {
            get { return _Body; }
            set
            {
                if (value != _Body)
                {
                    _Body = value;
                }
            }
        }

        public DateTime Date
        {
            get { return _Date; }
            set
            {
                if (value != _Date)
                {
                    _Date = value;
                }
            }
        }

        public string MachineName
        {
            get { return _MachineName; }
            set
            {
                if (value != _MachineName)
                {
                    _MachineName = value;
                }
            }
        }

        public string UserName
        {
            get { return _UserName; }
            set
            {
                if (value != _UserName)
                {
                    _UserName = value;
                }
            }
        }


    }
}
