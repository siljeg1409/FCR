using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace FlaxCrashReport.Data
{

    /// <summary>
    /// Singleton class with global settings
    /// Loads data from JSON file in (C:\FLAX\Settings\GlobalSettings.json)
    /// </summary>
    public sealed class Settings
    {

        #region Private members
        private static int _counter;
        private static int _intevalMinutes;
        private static string _machinename;
        private static string _username;
        private static string _passwordFrom;
        private static string _emailTo;
        private static string _emailFrom;
        private static DateTime _lastappcrash;
        private static string _reportspath;
        private static string _archivepath;
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

        [JsonProperty("fcr_interval")]
        public int Interval
        {
            get { return _intevalMinutes; }
            set
            {
                if (value != _intevalMinutes)
                {
                    _intevalMinutes = value;
                }
            }
        }

        [JsonProperty("fcr_lastappcrash")]
        public DateTime LastAppCrash
        {
            get { return _lastappcrash; }
            set
            {
                if (value != _lastappcrash)
                {
                    _lastappcrash = value;
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


        Settings()
        {
        }

        private static readonly object padlock = new object();
        private static Settings instance = null;
        public static Settings Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (padlock)
                    {
                        if (instance == null)
                        {
                            instance = new Settings();
                            GetSettings();
                        }
                    }
                }
                return instance;
            }
        }

        /// <summary>
        /// Reads GlobalSettigns.json file
        /// </summary>
        /// <returns></returns>
        private static void GetSettings()
        {
            JObject jo = JObject.Parse(File.ReadAllText(CheckGlobalSettingsFile()));
            instance = JsonConvert.DeserializeObject<Settings>(jo.ToString());
        }

        /// <summary>
        /// Checks all necessary folder paths
        /// Initiates GlobalSettings.json file with default values if needed
        /// </summary>
        /// <returns></returns>
        private static string CheckGlobalSettingsFile()
        {
            string settingspath = CheckFolder(@"C:\FLAX\Settings\");
            string reportspath = CheckFolder(@"C:\FLAX\Services\FCR\Reports\");
            string archivepath = CheckFolder(@"C:\FLAX\Services\FCR\Archive\");
            string settingsfilepath = @"C:\FLAX\Settings\GlobalSettings.json";
            if (File.Exists(settingsfilepath)) return settingsfilepath;

            Instance.MachineName = Environment.MachineName;
            Instance.UserName = Environment.UserName; // System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            Instance.LastAppCrash = DateTime.Now.AddDays(-1); // i don't want to send all logs from the beginning of the time
            Instance.ReportsPath = reportspath;
            Instance.ArchivePath = archivepath;
            Instance.Counter = 1;
            Instance.Interval = 10;
            Instance.EmailFrom = "";
            Instance.EmailTo = "";
            Instance.Password = "";

            var json = JsonConvert.SerializeObject(Instance, Formatting.Indented);
            File.WriteAllText(settingspath + @"\GlobalSettings.json", json);
            return settingsfilepath;
        }

        /// <summary>
        /// Checks if folder path exists, if not than it will be created
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static string CheckFolder(string path)
        {
            path = Path.GetDirectoryName(path);
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            return path;
        }

    }
}
