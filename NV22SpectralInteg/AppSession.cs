using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NV22SpectralInteg
{
    public static class AppSession
    {
        public static string KioskId { get; set; } = null;
        public static string KioskRegId { get; set; } = null;
        public static string CustomerRegId { get; set; } = null;
        public static string CustomerName { get; set; } = "None";
        public static string StoreName { get; set; } = null;
        public static string StoreAddress { get; set; } = null;
        public static string CustomerMobile { get; set; } = null;
        public static decimal? CustomerBALANCE { get; set; } = 0.00m;
        public static string smsId { get; set; } = null;

    }
}
