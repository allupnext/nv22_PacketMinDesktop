using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace NV22SpectralInteg.Network
{
    public static class NetworkHelper
    {
        public static async Task<(bool IsConnected, string Reason)> IsInternetAccessibleDetailedAsync(int timeout = 2000)
        {
            try
            {
                using (var ping = new Ping())
                {
                    var reply = await ping.SendPingAsync("8.8.8.8", timeout);
                    return (reply.Status == IPStatus.Success, reply.Status.ToString());
                }
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
    }
}
