using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlaxCrashReport.Data
{
    public class Settings
    {

        #region Private members
        private string _passwordFrom;
        private string _emailTo;
        private string _emailFrom;
        #endregion

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


    }
}
