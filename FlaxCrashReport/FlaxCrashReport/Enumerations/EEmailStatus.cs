using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlaxCrashReport.Enumerations
{
    public class EEmailStatus
    {
        public enum EmailSubjectStatus
        {
            OK = 0,
            APP_CRASH = 1,
            FCR_CRASH = 2,
            FCR_STARTED = 3,
            FCR_STOPPED = 4
        }

    }
}
