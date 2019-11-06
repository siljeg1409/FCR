using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlaxCrashReport.Enumerations
{
    public class EStatus
    {
        public enum EmailSubjectStatus
        {
            CRASH_REPORT = 0,
            FCR_OK = 1,
            FCR_CRASH_REPORT = 2,
            FCR_STARTED = 3,
            FCR_STOPPED = 4
        }

    }
}
