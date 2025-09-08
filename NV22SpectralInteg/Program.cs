using NV22SpectralInteg.Login;

namespace NV22SpectralInteg
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            // Show Welcome Form first
            using (var welcome = new WelcomeForm())
            {
                welcome.Show();
                welcome.Refresh();
                System.Threading.Thread.Sleep(3000);
            }

            // Launch the login form
            var loginForm = new LoginForm();
            Application.Run(loginForm);
        }
    }
}