using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NV22SpectralInteg.Model;

// Request: Payload for /send/user/mobileno/otp
public class OtpSendRequest
{
    public string mobileNo { get; set; }
    public string kioskId { get; set; }
}

// Response: The 'data' object containing smsId from /send/user/mobileno/otp
public class OtpSendResponseData
{
    public string smsId { get; set; }
}

// Request: Payload for /validate/user/mobileno/otp
public class OtpVerifyRequest
{
    public string mobileNo { get; set; }
    public string kioskId { get; set; }
    public string otp { get; set; }
    public string smsId { get; set; }
}

// Response: The 'data' object returned from /validate/user/mobileno/otp
public class OtpVerifyData
{
    public string REGID { get; set; }
    public string NAME { get; set; }
    public decimal BALANCE { get; set; }
}
