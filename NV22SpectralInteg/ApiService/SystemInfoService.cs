using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NV22SpectralInteg.Services
{
    public static class SystemInfo
    {
        public static string GetActiveLocalIpAddress()
        {
            try
            {
                // Create a dummy socket to find the preferred outbound IP
                using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
                {
                    // This doesn't send data but establishes a route to a public server
                    socket.Connect("8.8.8.8", 65530);

                    // Get the local endpoint that the OS chose for this connection
                    var endPoint = socket.LocalEndPoint as IPEndPoint;

                    return endPoint?.Address.ToString() ?? "Not Found";
                }
            }
            catch (Exception) { return "Error"; }
        }
    }
}
