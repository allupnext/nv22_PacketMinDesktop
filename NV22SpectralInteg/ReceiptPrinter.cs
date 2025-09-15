using NV22SpectralInteg;
using QRCoder;
using System.Drawing; 
using System.Drawing.Printing;
using System.IO; 

namespace BCSKioskServerCrypto
{
    
    public class LocalRequestBean
    {
        public string operation { get; set; }
        public string customerName { get; set; }
        public List<AmountDetail> amountDetails { get; set; }
        public int kioskTotalAmount { get; set; }

        // public string address { get; set; }
        // public string city { get; set; }
        // public string zipCode { get; set; }
        // public string transactionId { get; set; }
        // public string amount { get; set; }
        // public string gameName { get; set; }
        // public string idnNo { get; set; }
        // public string pin { get; set; }
        // public string gameURL { get; set; }
        // public string gamePlayerId { get; set; }
        // public string gamePassword { get; set; }
        // public string mode { get; set; }
        // public string bcsFee { get; set; }
        // public int gameTipFee { get; set; }
        // public string redeemableAmount { get; set; }
        // public string paymentQRCode { get; set; }
        // public string currEntDate { get; set; }
        // public string currTransactionId { get; set; }
        // public string currAmount { get; set; }
        // public string lastTransactionId { get; set; }
        // public string lastEntDate { get; set; }
        // public string lastAmount { get; set; }
        // public string currCashin { get; set; }
        // public string currCashout { get; set; }
        // public string currSettledAmount { get; set; }
        // public string lastCashin { get; set; }
        // public string lastCashout { get; set; }
        // public string lastSettledAmount { get; set; }
    }

    public class AmountDetail
    {
        public int denomination { get; set; }
        public int count { get; set; }
        public int total { get; set; }
    }

    public class ReceiptPrinter
    {
        private PrintDocument printDocument;
        private PrintPreviewDialog printPreviewDialog;
        private LocalRequestBean localRequestBean;
        private Image headerImage;
        private Image gameQR;
        private Image claimBTCQR;

        public ReceiptPrinter(LocalRequestBean requestBean)
        {
            Logger.Log($"👉 We Received operation: {requestBean.operation}, customerName: {requestBean.customerName}, kioskTotalAmount: {requestBean.kioskTotalAmount}");
            Logger.Log("📋 Logging individual AmountDetail items...");

            foreach (var ad in requestBean.amountDetails)
            {
                Logger.Log($"🧾 AmountDetail - Denomination: {ad.denomination}, Count: {ad.count}, Total: {ad.total}");
            }


            //headerImage = Image.FromFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", SessionManager.vendor + ".png"));
            this.localRequestBean = requestBean;

            printDocument = new PrintDocument();
            printPreviewDialog = new PrintPreviewDialog
            {
                Document = printDocument,
                Width = 800,
                Height = 600
            };
            printDocument.PrintPage += new PrintPageEventHandler(PrintDocument_PrintPage);
        }

        private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            Logger.Log("🚀 [PrintDocument_PrintPage Call] Starting...");

            StringFormat sf = new StringFormat();
            sf.LineAlignment = StringAlignment.Center;
            sf.Alignment = StringAlignment.Center;

            Font font = new Font("Arial", 11);
            Font boldFont = new Font("Arial", 11, FontStyle.Bold);
            Font largeFont = new Font("Arial", 14, FontStyle.Bold);

            float lineHeight = font.GetHeight(e.Graphics) + 1;
            float x = 10;
            float y = 20;

            if (headerImage != null)
            {
                e.Graphics.DrawImage(headerImage, x, y, headerImage.Width - 200, headerImage.Height - 50);
                y += headerImage.Height + lineHeight;
            }
            x += 5;
            y += 5;

            if (localRequestBean.operation == "bankadd")
            {
                // Define a new, wider starting x-coordinate for better alignment
                float receiptX = 20;

                // CUSTOMER line
                e.Graphics.DrawString($"CUSTOMER: {localRequestBean.customerName}", boldFont, Brushes.Black, receiptX, y);
                y += lineHeight + 20;

                // --- TABLE HEADERS ---
                e.Graphics.DrawString("Note", boldFont, Brushes.Black, receiptX, y);
                e.Graphics.DrawString("Count", boldFont, Brushes.Black, receiptX + 100, y);
                e.Graphics.DrawString("Total", boldFont, Brushes.Black, receiptX + 200, y);
                y += lineHeight + 5;

                // Draw a separator line for the header
                e.Graphics.DrawLine(Pens.Black, receiptX, y, e.PageBounds.Width - 20, y);
                y += 5;

                // --- TABLE ROWS (loop) ---
                var amountDetails = localRequestBean.amountDetails;
                if (amountDetails != null)
                {
                    foreach (var detail in amountDetails)
                    {
                        // Print a new row with three columns
                        e.Graphics.DrawString($"${detail.denomination}", font, Brushes.Black, receiptX, y);
                        e.Graphics.DrawString($"{detail.count}", font, Brushes.Black, receiptX + 100, y);
                        e.Graphics.DrawString($"${detail.total}", font, Brushes.Black, receiptX + 200, y);

                        y += lineHeight + 5;
                    }
                }
                y += lineHeight + 10;

                // --- GRAND TOTAL ---
                e.Graphics.DrawString("GRAND TOTAL:", largeFont, Brushes.Black, receiptX, y);
                e.Graphics.DrawString($"${localRequestBean.kioskTotalAmount}", largeFont, Brushes.Black, receiptX + 200, y);
                y += lineHeight + 50;

                // --- THANK YOU MESSAGE ---
                float pageCenter = e.PageBounds.Width / 2f;
                float thankYouY = y; // Use current y position
                e.Graphics.DrawString("Thank you!", largeFont, Brushes.Black, pageCenter, thankYouY, sf);

                Logger.Log("✅ [PrintDocument_PrintPage Call] Ending...");
            }
            //else
            //{
            //    // Existing logic for other operations ("buy", "claim", "storesettlement")
            //    if (localRequestBean.operation == "buy")
            //    {
            //        e.Graphics.DrawString("***** BUY CLOUD SPACE *****", new Font("Arial", 12, FontStyle.Bold), Brushes.Black, x, y);
            //        y += lineHeight + 10;
            //        e.Graphics.DrawString("***** &     *****", new Font("Arial", 12, FontStyle.Bold), Brushes.Black, x, y);
            //        y += lineHeight + 10;
            //        e.Graphics.DrawString("***** GET FREE PLAY *****", new Font("Arial", 12, FontStyle.Bold), Brushes.Black, x, y);
            //    }
            //    else if (localRequestBean.operation == "claim")
            //    {
            //        e.Graphics.DrawString("***** CLAIM THE PRIZE *****", new Font("Arial", 12, FontStyle.Bold), Brushes.Black, x, y);
            //    }

            //    // Rest of the existing logic...
            //    y += lineHeight + 20;
            //    e.Graphics.DrawString("LOCATION:" + localRequestBean.address, font, Brushes.Black, x, y);
            //    y += lineHeight + 10;
            //    e.Graphics.DrawString(localRequestBean.city + " " + localRequestBean.zipCode, font, Brushes.Black, x, y);
            //    y += lineHeight + 20;
            //    e.Graphics.DrawString("DATE & TIME:" + DateTime.Now.ToString("yyyy/MM/dd hh:mm tt"), font, Brushes.Black, x, y);
            //    y += lineHeight + 10;
            //    e.Graphics.DrawString("RECIEPT ID: " + localRequestBean.transactionId, font, Brushes.Black, x, y);
            //    y += lineHeight + 10;
            //    e.Graphics.DrawString((localRequestBean.operation == "buy" ? "AMOUNT: " : "PRIZE CLAIMED: ") + localRequestBean.amount, font, Brushes.Black, x, y);
            //    y += lineHeight + 10;
            //    if (localRequestBean.operation == "buy")
            //    {
            //        e.Graphics.DrawString("CLOUD SPACE: " + localRequestBean.amount + " MB ", font, Brushes.Black, x, y);
            //        y += lineHeight + 10;
            //    }
            //    else if (localRequestBean.operation == "claim" && SessionManager.vendor == "ATIBCS")
            //    {
            //        e.Graphics.DrawString("BCS TRANSACTION FEES: -" + localRequestBean.bcsFee, font, Brushes.Black, x, y);
            //        y += lineHeight + 10;
            //        if (localRequestBean.gameTipFee > 0)
            //        {
            //            e.Graphics.DrawString("GAME TIP: -" + localRequestBean.gameTipFee, font, Brushes.Black, x, y);
            //            y += lineHeight + 10;
            //        }
            //        e.Graphics.DrawString("TOTAL PRIZE: " + localRequestBean.redeemableAmount, font, Brushes.Black, x, y);
            //        y += lineHeight + 10;

            //        byte[] imageBytes = Convert.FromBase64String(localRequestBean.paymentQRCode);
            //        using (MemoryStream ms = new MemoryStream(imageBytes))
            //        {
            //            claimBTCQR = Image.FromStream(ms);
            //            if (claimBTCQR != null)
            //            {
            //                e.Graphics.DrawString("***** SCAN BELOW QR CODE *****", new Font("Arial", 12, FontStyle.Bold), Brushes.Black, x, y);
            //                y += lineHeight + 10;
            //                e.Graphics.DrawString("***** TO RECEIVE PRIZE  *****", new Font("Arial", 12, FontStyle.Bold), Brushes.Black, x, y);
            //                y += lineHeight + 10;
            //                e.Graphics.DrawImage(claimBTCQR, x, y, claimBTCQR.Width - 250, claimBTCQR.Height - 250);
            //                y += 260;
            //                e.Graphics.DrawString("ABOVE QR CODE IS VALID FOR 6 HOURS FROM NOW", new Font("Arial", 11, FontStyle.Bold), Brushes.Black, x, y);
            //                y += lineHeight + 10;
            //                e.Graphics.DrawString("***************", new Font("Arial", 12, FontStyle.Bold), Brushes.Black, x, y);
            //                y += lineHeight + 10;
            //            }
            //        }
            //    }
            //    e.Graphics.DrawString("PROMOTIONAL GAME: " + localRequestBean.gameName, font, Brushes.Black, x, y);
            //    y += lineHeight + 20;
            //    if (localRequestBean.operation == "buy")
            //    {
            //        e.Graphics.DrawString("***** CLOUD SPACE ACCESS *****", new Font("Arial", 12, FontStyle.Bold), Brushes.Black, x, y);
            //        y += lineHeight + 20;
            //        e.Graphics.DrawString("https://bitcloudsweeps.com", font, Brushes.Black, x, y);
            //        y += lineHeight + 10;
            //        e.Graphics.DrawString("IDN NUMBER: " + localRequestBean.idnNo, font, Brushes.Black, x, y);
            //        if (localRequestBean.mode == "new")
            //        {
            //            y += lineHeight + 10;
            //            e.Graphics.DrawString("PIN: " + localRequestBean.pin, font, Brushes.Black, x, y);
            //        }
            //        y += lineHeight + 20;
            //        if (localRequestBean.gameURL != null && localRequestBean.gameURL != "")
            //        {
            //            e.Graphics.DrawString("***** PROMOTIONAL *****", new Font("Arial", 12, FontStyle.Bold), Brushes.Black, x, y);
            //            y += lineHeight + 10;
            //            e.Graphics.DrawString("***** GAME ACCESS *****", new Font("Arial", 12, FontStyle.Bold), Brushes.Black, x, y);
            //            y += lineHeight + 10;
            //            gameQR = generateQRCode(localRequestBean.gameURL);
            //            e.Graphics.DrawImage(gameQR, x, y, gameQR.Width - 80, gameQR.Height - 70);
            //            y += gameQR.Height - 40;

            //            e.Graphics.DrawString("ACCOUNT: " + localRequestBean.gamePlayerId, font, Brushes.Black, x, y);
            //            y += lineHeight + 10;
            //            if (localRequestBean.mode == "new")
            //            {
            //                e.Graphics.DrawString("PASSWORD: " + localRequestBean.gamePassword, font, Brushes.Black, x, y);
            //                y += lineHeight + 20;
            //            }
            //        }
            //    }
            //    e.Graphics.DrawString("Terms and conditions; you have already-", new Font("Arial", 10, FontStyle.Bold), Brushes.Black, x, y);
            //    y += lineHeight + 10;
            //    e.Graphics.DrawString("accepted prior of this purchase is–", new Font("Arial", 10, FontStyle.Bold), Brushes.Black, x, y);
            //    y += lineHeight + 10;
            //    e.Graphics.DrawString("available for your review on below link.", new Font("Arial", 10, FontStyle.Bold), Brushes.Black, x, y);
            //    y += lineHeight + 10;
            //    e.Graphics.DrawString("https://grews.s3.amazonaws.com/bcs/privacypolicy.pdf", new Font("Arial", 6, FontStyle.Bold), Brushes.Black, x, y);
            //}

            e.HasMorePages = false;
        }
        public bool printReceipt()
        {
            try
            {
                Logger.Log("🚀 [printReceipt Call] Starting...");
                printDocument.Print();
                //printPreviewDialog.ShowDialog();
                // SessionManager.status = true; // Not defined in the provided code
                Logger.Log("✅ [printReceipt Call] Completed...");
                return true;
            }
            catch (Exception ex)
            {
                // SessionManager.message = "An error occurred while printing the document: " + ex.Message; // Not defined
                // SessionManager.status = false; // Not defined
                Logger.Log($"❌ [printReceipt Call] {ex.Message}");
                return false;
            }
        }

        private Bitmap generateQRCode(string url)
        {
            // This method is not used in the "bankadd" receipt.
            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            {
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
                using (QRCode qrCode = new QRCode(qrCodeData))
                {
                    Bitmap qrCodeImage = qrCode.GetGraphic(7);
                    return qrCodeImage;
                }
            }
        }
    }
}
