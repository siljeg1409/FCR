using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlaxCrashReport.Data
{
    public sealed class SGlobal
    {

        #region Private members
        private static string _MachineName;
        private static string _UserName;
        private static Settings _Settings;
        #endregion

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


        public Settings Settings
        {
            get { return _Settings; }
            set
            {
                if (value != _Settings)
                {
                    _Settings = value;
                }
            }
        }

        SGlobal()
        {
        }
        private static readonly object padlock = new object();
        private static SGlobal instance = null;
        public static SGlobal Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (padlock)
                    {
                        if (instance == null)
                        {
                            instance = new SGlobal();
                            instance.MachineName = Environment.MachineName;
                            instance.UserName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                            instance.Settings = GetSettings();
                        }
                    }
                }
                return instance;
            }
        }

        private static Settings GetSettings()
        {
            Settings s = new Settings
            {
                EmailFrom = "",
                EmailTo = "",
                Password = ""
            };

            return s;
        }
    }
}
