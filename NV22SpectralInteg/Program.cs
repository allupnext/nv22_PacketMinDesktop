using BCSKioskServerCrypto;
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

            //var amountDetails = new List<AmountDetail>
            //{
            //    new AmountDetail { denomination = 10, count = 1, total = 10 },
            //    new AmountDetail { denomination = 20, count = 2, total = 40 },
            //    new AmountDetail { denomination = 5, count = 5, total = 25 }
            //};

            //int grandTotal = amountDetails.Sum(detail => detail.total);

            //var dummyRequestBean = new LocalRequestBean
            //{
            //    operation = "bankadd",
            //    customerName = "John Doe",
            //    amountDetails = amountDetails,
            //    kioskTotalAmount = grandTotal
            //};

            //var printer = new ReceiptPrinter(dummyRequestBean);
            //printer.printReceipt();

            // Launch the login form
            var loginForm = new LoginForm();
            Application.Run(loginForm);
        }
    }
}