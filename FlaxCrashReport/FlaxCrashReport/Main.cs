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
        }
   
    
        protected override void OnStart(string[] args)
        {
            timer.Elapsed += new ElapsedEventHandler(OnElapsedTime);
            timer.Interval = 60000; //1 minute
            timer.Enabled = true;
        }

       

        protected override void OnStop()
        {
            ml.SendEmail("FCR_SERVICE_STOPPED");
        }

        private void OnElapsedTime(object source, ElapsedEventArgs e)
        {
            try
            {
                ml.SendCrashData();
            }
            catch (System.Exception)
            {
                ml.SendEmail("FCR_SERVICE_CRASH");
            }
        }

    }
}
