using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlaxCrashReport.Data
{
    public class GlobalSettings
    {
        #region Private members
        private int _counter;
        private string _machinename;
        private string _username;
        private string _passwordFrom;
        private string _emailTo;
        private string _emailFrom;
        private DateTime _lastcrash;
        #endregion

        [JsonProperty("fcr_emailfrom")]
        public string EmailFrom
        {
            get { return _emailFrom; }
            set
            {
                if (value != _emailFrom)
                {
                    _emailFrom = value;
                }
            }
        }

        [JsonProperty("machinename")]
        public string MachineName
        {
            get { return _machinename; }
            set
            {
                if (value != _machinename)
                {
                    _machinename = value;
                }
            }
        }

        [JsonProperty("username")]
        public string UserName
        {
            get { return _username; }
            set
            {
                if (value != _username)
                {
                    _username = value;
                }
            }
        }

        [JsonProperty("fcr_emailto")]
        public string EmailTo
        {
            get { return _emailTo; }
            set
            {
                if (value != _emailTo)
                {
                    _emailTo = value;
                }
            }
        }

        [JsonProperty("fcr_password")]
        public string Password
        {
            get { return _passwordFrom; }
            set
            {
                if (value != _passwordFrom)
                {
                    _passwordFrom = value;
                }
            }
        }

        [JsonProperty("fcr_counter")]
        public int Counter
        {
            get { return _counter; }
            set
            {
                if (value != _counter)
                {
                    _counter = value;
                }
            }
        }

        [JsonProperty("fcr_lastcrash")]
        public DateTime LastCrash
        {
            get { return _lastcrash; }
            set
            {
                if (value != _lastcrash)
                {
                    _lastcrash = value;
                }
            }
        }


    }
}
