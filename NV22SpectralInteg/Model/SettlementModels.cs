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
    // Note: The service will populate these as formatted strings
    public string startTime { get; set; }
    public string endTime { get; set; }
    public decimal totalDenominationDepository { get; set; }
    public decimal totalSettlementAmount { get; set; }
    public string kioskIpAddress { get; set; }
}

// 2. Data object within the successful API response
public class SettlementReportData
{
    // Ensure this matches the expected JSON key case (often all caps)
    public string RECEIPTURL { get; set; }
}

// 3. (Optional, but recommended) A general class to represent the aggregated data
// from TransactionRepository.GetAggregatedSettlementData
public class AggregatedSettlementData
{
    public decimal totalDenominationDepository { get; set; }
    public decimal totalSettlementAmount { get; set; }
}
