using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace FlaxCrashReport.Data
{
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
                            instance = new SGeneral();
                            instance.Settings = GetSettings();
                        }
                    }
                }
                return instance;
            }
        }

        private static GlobalSettings GetSettings()
        {
            JObject jo = JObject.Parse(File.ReadAllText(CheckGlobalSettingsFile()));
            GlobalSettings gs = new GlobalSettings();
            return JsonConvert.DeserializeObject<GlobalSettings>(jo.ToString());
        }

        private static string CheckGlobalSettingsFile()
        {
            string settingsfilepath = @"C:\FLAX\Settings\GlobalSettings.json"; 
            if (File.Exists(settingsfilepath)) return settingsfilepath;

            string settingspath = @"C:\FLAX\Settings\";
            string reportspath = @"C:\FLAX\Services\FCR\Reports\";
            string archivepath = @"C:\FLAX\Services\FCR\Archive\";

            GlobalSettings gs = new GlobalSettings
            {
                MachineName = Environment.MachineName,
                UserName = System.Security.Principal.WindowsIdentity.GetCurrent().Name,
                LastCrash = new DateTime(1990, 9, 14),
                LastServiceCrash = new DateTime(1990, 9, 14),
                ReportsPath = CheckFolder(reportspath),
                ArchivePath = CheckFolder(archivepath),
                Counter = 1,
                EmailFrom = "",
                EmailTo = "",
                Password = ""
            };
            var json = JsonConvert.SerializeObject(gs, Formatting.Indented);
            File.WriteAllText(CheckFolder(settingspath) + @"\GlobalSettings.json", json);
            return settingsfilepath;
        }

        private static string CheckFolder(string path)
        {
            path = Path.GetDirectoryName(path);
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            return path;
        }

    }
}
