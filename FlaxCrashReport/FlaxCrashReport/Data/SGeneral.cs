using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
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
            string filepath = @"C:\FLAX\Settings\GlobalSettings.json";
            checkGlobalSettings(filepath);
            JObject o1 = JObject.Parse(File.ReadAllText(filepath));
            GlobalSettings gs = new GlobalSettings();
            return JsonConvert.DeserializeObject<GlobalSettings>(o1.ToString());
        }

        private static void checkGlobalSettings(string filepath)
        {
            if (File.Exists(filepath)) return;
            if (!Directory.Exists(@"C:\FLAX\Settings\")) Directory.CreateDirectory(@"C:\FLAX\Settings\"); 
            GlobalSettings gs = new GlobalSettings
            {
                MachineName = Environment.MachineName,
                UserName = System.Security.Principal.WindowsIdentity.GetCurrent().Name,
                LastCrash = new DateTime(1990, 9, 14),
                Counter = 1,
            };
            var json = JsonConvert.SerializeObject(gs, Formatting.Indented);
            File.WriteAllText(@"C:\FLAX\Settings\GlobalSettings.json", json);
        }
      
    }
}
