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
        private DateTime _lastservicecrash;
        private string _reportspath;
        private string _archivepath;
        private List<Application> _applist;
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

        [JsonProperty("fcr_lastservicecrash")]
        public DateTime LastServiceCrash
        {
            get { return _lastservicecrash; }
            set
            {
                if (value != _lastservicecrash)
                {
                    _lastservicecrash = value;
                }
            }
        }

        [JsonProperty("fcr_reportspath")]
        public string ReportsPath
        {
            get { return _reportspath; }
            set
            {
                if (value != _reportspath)
                {
                    _reportspath = value;
                }
            }
        }

        [JsonProperty("fcr_archivepath")]
        public string ArchivePath
        {
            get { return _archivepath; }
            set
            {
                if (value != _archivepath)
                {
                    _archivepath = value;
                }
            }
        }

        [JsonProperty("fcr_applist")]
        public List<Application> AppList
        {
            get { return _applist; }
            set
            {
                if (value != _applist)
                {
                    _applist = value;
                }
            }
        }

    }
}
