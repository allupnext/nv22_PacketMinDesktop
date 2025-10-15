using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NV22SpectralInteg.Model;

// Request: Payload for /machine/availability/log
public class AvailabilityRequest
{
    public string kioskId { get; set; }
    public string ipAddress { get; set; }
}

// Response: The 'data' object returned. Often empty or simple for a log endpoint.
// Use 'object' or an empty class if the data property is always null/empty {}
// If the API always returns an empty object, you can use:
public class AvailabilityResponseData { }
