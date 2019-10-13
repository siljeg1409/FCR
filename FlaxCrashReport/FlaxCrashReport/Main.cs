using System.ServiceProcess;
using System.Timers;

namespace FlaxCrashReport
{
    public partial class Main : ServiceBase
    {
        Timer timer = new Timer();

        public Main()
        {
            InitializeComponent();
            Logic.MainLogic.SendEmail("FCR_SERVICE_STARTED", "");
            //DEBUG => uncomment code below!
            OnElapsedTime(null, null);
            System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite);
        }

        protected override void OnStart(string[] args)
        {
            timer.Elapsed += new ElapsedEventHandler(OnElapsedTime);
            timer.Interval = 60000; //1 minute
            timer.Enabled = true;
        }

        protected override void OnStop()
        {
            try
            {
                Logic.MainLogic.SendEmail("FCR_SERVICE_STOPPED", "");
            }
            catch (System.Exception ex)
            {
                Logic.MainLogic.CreateServiceCrashReport(ex);
            }
        }

        private void OnElapsedTime(object source, ElapsedEventArgs e)
        {
            try
            {
                Logic.MainLogic.SendCrashData();
                if (System.DateTime.Now.Hour == 12 && System.DateTime.Now.Minute == 0)
                {
                    Logic.MainLogic.SendEmail("FCR_OK", "");
                }
            }
            catch (System.Exception ex)
            {
                Logic.MainLogic.CreateServiceCrashReport(ex);
            }
        }

    }
}
