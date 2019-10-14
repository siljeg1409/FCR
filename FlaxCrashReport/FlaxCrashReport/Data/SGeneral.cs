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
    public sealed class SGeneral
    {

        #region Private members
        private static GlobalSettings _gs;
        #endregion

        public GlobalSettings Settings
        {
            get { return _gs; }
            set
            {
                if (value != _gs)
                {
                    _gs = value;
                }
            }
        }

        SGeneral()
        {
        }
        private static readonly object padlock = new object();
        private static SGeneral instance = null;
        public static SGeneral Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (padlock)
                    {
                        if (instance == null)
                        {
                            instance = new SGeneral
                            {
                                Settings = GetSettings()
                            };
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
        private static GlobalSettings GetSettings()
        {
            JObject jo = JObject.Parse(File.ReadAllText(CheckGlobalSettingsFile()));
            GlobalSettings gs = new GlobalSettings();
            return JsonConvert.DeserializeObject<GlobalSettings>(jo.ToString());
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

            GlobalSettings gs = new GlobalSettings
            {
                MachineName = Environment.MachineName,
                UserName = System.Security.Principal.WindowsIdentity.GetCurrent().Name,
                LastServiceCrash = new DateTime(1990, 9, 14),
                LastAppCrash = new DateTime(1990, 9, 14),
                LastFlaxCrash = new DateTime(1990, 9, 14),
                LastOKStatus = new DateTime(1990, 9, 14),
                ReportsPath = reportspath,
                ArchivePath = archivepath,
                Counter = 1,
                EmailFrom = "",
                EmailTo = "",
                Password = "",
            };
            var json = JsonConvert.SerializeObject(gs, Formatting.Indented);
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
