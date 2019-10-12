using System.ServiceProcess;
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
            OnElapsedTime(null, null);
            System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite);
        }

        protected override void OnStart(string[] args)
        {
            //timer.Elapsed += new ElapsedEventHandler(OnElapsedTime);
            //timer.Interval = 60000; //1 minute
            //timer.Enabled = true;
        }

       

        protected override void OnStop()
        {
            try
            {
                ml.SendEmail("FCR_SERVICE_STOPPED", "");
            }
            catch (System.Exception ex)
            {
                ml.createServiceCrashReport(ex);
            }
        }

        private void OnElapsedTime(object source, ElapsedEventArgs e)
        {
            try
            {
                ml.SendCrashData();
            }
            catch (System.Exception ex)
            {
                ml.createServiceCrashReport(ex);
            }
        }

    }
}
