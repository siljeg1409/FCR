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
        private static string _machinename;
        private static string _username;
        private static string _passwordFrom;
        private static string _emailTo;
        private static string _emailFrom;
        private static DateTime _lastservicecrash;
        private static DateTime _lastappcrash;
        private static DateTime _lastokstatus;
        private static string _reportspath;
        private static string _archivepath;
        #endregion

        [JsonProperty("fcr_emailfrom")]
        public static string EmailFrom
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
        public static string MachineName
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
        public static string UserName
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
        public static string EmailTo
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
        public static string Password
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
        public static int Counter
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
        public static DateTime LastServiceCrash
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

        [JsonProperty("fcr_lastappcrash")]
        public static DateTime LastAppCrash
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

        [JsonProperty("fcr_lastokstatus")]
        public static DateTime LastOKStatus
        {
            get { return _lastokstatus; }
            set
            {
                if (value != _lastokstatus)
                {
                    _lastokstatus = value;
                }
            }
        }

        [JsonProperty("fcr_reportspath")]
        public static string ReportsPath
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
        public static string ArchivePath
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

            MachineName = Environment.MachineName;
            UserName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            LastServiceCrash = new DateTime(1990, 9, 14);
            LastAppCrash = new DateTime(1990, 9, 14);
            LastOKStatus = new DateTime(1990, 9, 14);
            ReportsPath = reportspath;
            ArchivePath = archivepath;
            Counter = 1;
            EmailFrom = "";
            EmailTo = "";
            Password = "";

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
