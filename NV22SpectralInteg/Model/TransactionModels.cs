using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NV22SpectralInteg.Model;

// 1. The inner detail object for each note denomination
public class DenominationDetail
{
    public int denomination { get; set; }
    public int count { get; set; }
    public decimal total { get; set; }
}

// 2. The main request body for /user/transaction/persist
public class TransactionPersistRequest
{
    public string kioskId { get; set; }
    public string kioskRegId { get; set; }
    public string customerRegId { get; set; }
    public decimal kioskTotalAmount { get; set; }
    public List<DenominationDetail> amountDetails { get; set; } 
}


public class TransactionPersistData
{
    public decimal userBalance { get; set; } 
    public decimal storeBalance { get; set; } 
    public decimal cryptoConversionFee { get; set; }
}