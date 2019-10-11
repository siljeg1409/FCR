using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace FlaxCrashReport
{
    public partial class Main : ServiceBase
    {
        Timer timer = new Timer();
        Logic.MainLogic ml = new Logic.MainLogic();

        public Main()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            timer.Elapsed += new ElapsedEventHandler(OnElapsedTime);
            timer.Interval = 5000; 
            timer.Enabled = true;
        }

       

        protected override void OnStop()
        {
            SendStopEmail();
        }

        private void SendStopEmail()
        {
            //Sends Email that service has stopped
        }

        private void OnElapsedTime(object source, ElapsedEventArgs e)
        {
            if (ml.CheckAppStatus()) return;
            ml.SendCrashData();
        }

    }
}
