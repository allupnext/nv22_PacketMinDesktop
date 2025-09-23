using BCSKioskServerCrypto;
using NV22SpectralInteg.InactivityManager;
using NV22SpectralInteg.Login;

namespace NV22SpectralInteg
{
    internal static class Program
    {
        public static LoginForm mainLoginForm;

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            // It creates the timers and prepares the manager for use.
            KioskIdleManager.Initialize(PerformLogout);

            // Show Welcome Form first
            using (var welcome = new WelcomeForm())
            {
                welcome.Show();
                welcome.Refresh();
                System.Threading.Thread.Sleep(3000);
            }



            //var dummyRequestBean = new LocalRequestBean
            //{
            //    operation = "bankadd",
            //    kioskTotalAmount = 100.00m,
            //    feeAmount = 8.00m,
            //    isSucceed = true
            //};

            //var printer = new ReceiptPrinter(dummyRequestBean);
            //printer.printReceipt();

            // Launch the login form
            mainLoginForm = new LoginForm();
            Application.Run(mainLoginForm);
        }


        public static void PerformLogout()
        {
            // This code looks for any open form that is NOT the login form and closes it.
            foreach (Form openForm in Application.OpenForms.Cast<Form>().ToList())
            {
                if (openForm.Name != mainLoginForm.Name)
                {
                    openForm.Close();
                }
            }

            // Reset and show the main login form
            if (mainLoginForm != null && !mainLoginForm.IsDisposed)
            {
                mainLoginForm.ResetToLogin();
                mainLoginForm.Show();
                mainLoginForm.Activate();
            }
            else // Failsafe in case the login form was somehow closed
            {
                mainLoginForm = new LoginForm();
                mainLoginForm.Show();
            }
        }
    }
}