using BCSKioskServerCrypto;
using Newtonsoft.Json;
using NV22SpectralInteg.Dashboard;
using NV22SpectralInteg.Data;
using NV22SpectralInteg.InactivityManager;
using NV22SpectralInteg.Login;
using NV22SpectralInteg.Services;

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
            KioskHeartbeatService.Initialize();


            string configPath = Path.Combine(Application.StartupPath, "config.json");
            // Read the entire file into a string
            string jsonContent = File.ReadAllText(configPath);
            // Deserialize the JSON string into the class field
            AppConfig config = JsonConvert.DeserializeObject<AppConfig>(jsonContent);
            ApiService.Initialize(config);

            SQLitePCL.Batteries_V2.Init();
            TransactionRepository.EnsureDatabaseAndTable();

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
            //    isSucceed = true,
            //    printmessage  = "You purchased $20.0 in crypto. Please check ypur PocketMint wallet linked to mobile ending 7717"
            //};

            //var printer = new ReceiptPrinter(dummyRequestBean);
            //printer.printReceipt();

            // Launch the login form
            mainLoginForm = new LoginForm();
            Application.Run(mainLoginForm);
        }


        public static void PerformLogout()
        {
            // Log that the logout process has started.
            Logger.Log("➡️ Performing global logout...");

            // This code looks for any open form that is NOT the login form and closes it.
            foreach (Form openForm in Application.OpenForms.Cast<Form>().ToList())
            {
                if (mainLoginForm != null && openForm.Name != mainLoginForm.Name)
                {
                    // Log the specific form being closed. This helps in debugging.
                    Logger.Log($"   Closing form: {openForm.Name} (Type: {openForm.GetType().Name})");
                    openForm.Close();
                }
            }

            // Reset and show the main login form
            if (mainLoginForm != null && !mainLoginForm.IsDisposed)
            {
                Logger.Log("   Resetting and showing the main login form.");
                mainLoginForm.ResetToLogin();
                mainLoginForm.Show();
                mainLoginForm.Activate();
            }
            else // Failsafe in case the login form was somehow closed
            {
                // Log a warning because this is unexpected behavior.
                Logger.Log("⚠️ Main login form was null or disposed. Recreating a new instance as a failsafe.");
                mainLoginForm = new LoginForm();
                mainLoginForm.Show();
            }

            Logger.Log("✅ Logout complete. Application returned to login screen.");
        }
    }
}