using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlaxCrashReport.Data
{
    public sealed class SGeneral
    {

        #region Private members
        private int _counter;
        private string _machinename;
        private string _username;
        private string _passwordFrom;
        private string _emailTo;
        private string _emailFrom;
        #endregion

        [JsonProperty("FCR_EmailFrom")]
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

        [JsonProperty("Machine")]
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

        [JsonProperty("User")]
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

        [JsonProperty("FCR_EmailTo")]
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

        [JsonProperty("FCR_Password")]
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

        [JsonProperty("FCR_Counter")]
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
                            instance = GetSettings();
                        }
                    }
                }
                return instance;
            }
        }

        private static SGeneral GetSettings()
        {
            string filepath = @"C:\FLAX\Settings\GlobalSettings.json";
            if(!File.Exists(filepath))
            {
                var o = new Logic.MainLogic();
                o.SendEmail("NO_SETTINGS_FILE", "");
            }
            JObject o1 = JObject.Parse(File.ReadAllText(filepath));
            SGeneral s = Newtonsoft.Json.JsonConvert.DeserializeObject<SGeneral>(o1.ToString());
            return s;
        }

      
    }
}
