using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NV22SpectralInteg.Model;

// Request: Payload for /get/kiosks/details
public class KioskSessionRequest
{
    [JsonProperty("kioskId")]
    public string KioskId { get; set; }
}

// Response: The 'data' object returned from /get/kiosks/details
public class KioskSessionData
{
    // Ensure property names match the JSON keys exactly (case-sensitive)
    public string KIOSKID { get; set; }
    public string REGID { get; set; }
    public string STORENAME { get; set; }
    public string KIOSKNAME { get; set; }
    public string ADDRESS { get; set; }
    public string CITY { get; set; }
    public string LOCATION { get; set; }
    public string ZIPCODE { get; set; }
}
