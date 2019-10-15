using System.ServiceProcess;
using System.Timers;

namespace FlaxCrashReport
{
    public partial class Main : ServiceBase
    {
        Timer timer = new Timer();

        /// <summary>
        /// Main function, initialize components
        /// Code below //Debug used for VS debugging the service
        /// </summary>
        public Main()
        {
            InitializeComponent();
            ////DEBUG => uncomment code below!
            //OnElapsedTime(null, null);
            //System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite);
        }

        /// <summary>
        /// Called od service starting
        /// Sends email that service has started
        /// Calls function OnElapsedTime each minute
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args)
        {
            Logic.MainLogic.SendEmail(System.Tuple.Create("FCR_STARTED", "", System.DateTime.Now, ""));
            timer.Elapsed += new ElapsedEventHandler(OnElapsedTime);
            timer.Interval = 60000; //1 minute
            timer.Enabled = true;
        }

        /// <summary>
        /// Called on service stopping
        /// Sends email that service has stopped
        /// </summary>
        protected override void OnStop()
        {
            try
            {
                Logic.MainLogic.SendEmail(System.Tuple.Create("FCR_STOPPED", "", System.DateTime.Now, ""));
            }
            catch (System.Exception ex)
            {
                Logic.MainLogic.CreateServiceCrashReport(ex);
            }
        }

        /// <summary>
        /// This will fire every minute
        /// Calls function to check error in EventViewer and send emails
        /// Also sends OK Status email every 24h
        /// Global try-catch here to create FCR_CRASH.json if Service fails somewhere
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void OnElapsedTime(object source, ElapsedEventArgs e)
        {
            try
            {
                Logic.MainLogic.SendCrashData();
                if (System.DateTime.Now.Hour >= 12 
                    && System.DateTime.Now.Minute >= 0
                    && System.DateTime.Now >= Data.SGeneral.Instance.Settings.LastOKStatus.AddDays(1))
                {
                    Logic.MainLogic.SendEmail(System.Tuple.Create("FCR_OK", "", System.DateTime.Now, ""));
                }
            }
            catch (System.Exception ex)
            {
                Logic.MainLogic.CreateServiceCrashReport(ex);
            }
        }

    }
}
