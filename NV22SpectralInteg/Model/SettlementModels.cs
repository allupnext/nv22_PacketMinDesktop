using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NV22SpectralInteg.Model;

// 1. Request Body for /validate/settlement
public class SettlementReportRequest
{
    public string storeRegId { get; set; }
    public string kioskId { get; set; }
    public string settlementCode { get; set; }
    public string startTime { get; set; }
    public string endTime { get; set; }
    public List<DenominationSettlement> totalDenominationDepository { get; set; }
    public decimal totalSettlementAmount { get; set; }
    public string kioskIpAddress { get; set; }
}

// 2. Data object within the successful API response
public class SettlementReportData
{
    // Ensure this matches the expected JSON key case (often all caps)
    public string RECEIPTURL { get; set; }
}

public class DenominationSettlement
{
    public long denomination { get; set; }
    public long count { get; set; }
    public decimal total { get; set; }
}

// Define the main return structure
public class AggregatedSettlementData
{
    // Must match the property names returned by the query, but with proper types
    public List<DenominationSettlement> totalDenominationDepository { get; set; }
    public decimal totalSettlementAmount { get; set; }
}
