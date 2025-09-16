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
        public decimal kioskTotalAmount { get; set; }
        public decimal feeAmount { get; set; }
        public bool isSucceed { get; set; } = true;

        public string printmessage { get; set; }
    }

    
    public class ReceiptPrinter
    {
        private PrintDocument printDocument;
        private PrintPreviewDialog printPreviewDialog;
        private LocalRequestBean localRequestBean;
        private Image headerImage;
        private Image gameQR;
        private Image claimBTCQR;
        string imagePath = Path.Combine(Application.StartupPath, "Image", "-PocketMint.png");

        public ReceiptPrinter(LocalRequestBean requestBean)
        {
            Logger.Log($"👉 We Received operation: {requestBean.operation}, customerName: {AppSession.CustomerName}, kioskTotalAmount: {requestBean.kioskTotalAmount}");
            Logger.Log("📋 Logging individual AmountDetail items...");


            if (File.Exists(imagePath))
            {
                headerImage = Image.FromFile(imagePath);
            }
            else
            {
                Logger.Log("❌ Header image not found at: " + imagePath);
            }

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

            Font font = new Font("Arial", 10);
            Font boldFont = new Font("Arial", 10, FontStyle.Bold);
            Font largeFont = new Font("Arial", 14, FontStyle.Bold);

            float lineHeight = font.GetHeight(e.Graphics) + 1;
            float x = 10;
            float y = 10;

            if (localRequestBean.operation == "bankadd")
            {
                // Set up initial vertical position and left margin
                float receiptX = 10;
                float pageWidth = e.PageBounds.Width;
                float pageCenter = pageWidth / 2f;
                StringFormat centerFormat = new StringFormat() { Alignment = StringAlignment.Center };
                y = 20; // starting vertical position
                lineHeight = font.GetHeight(e.Graphics) + 4;

                // --- HEADER / LOGO ---
                if (headerImage != null)
                {
                    int drawWidth = 150;
                    int drawHeight = (int)(headerImage.Height * (drawWidth / (float)headerImage.Width));

                    pageWidth = e.PageBounds.Width;
                    float xCentered = (pageWidth - drawWidth) / 2f;

                    e.Graphics.DrawImage(headerImage, xCentered, y, drawWidth, drawHeight);
                    y += drawHeight + lineHeight;
                }


                // --- TIMESTAMP ---
                e.Graphics.DrawString("Timestamp: ", boldFont, Brushes.Black, receiptX, y);
                float labelWidth = e.Graphics.MeasureString("Timestamp: ", boldFont).Width;
                e.Graphics.DrawString($"{DateTime.Now:dd/MM/yyyy HH:mm:ss} EST", font, Brushes.Black, receiptX + labelWidth, y);
                y += lineHeight;

                // --- KIOSK ID ---
                e.Graphics.DrawString("KIOSK ID: ", boldFont, Brushes.Black, receiptX, y);
                labelWidth = e.Graphics.MeasureString("KIOSK ID: ", boldFont).Width;
                e.Graphics.DrawString(AppSession.KioskId, font, Brushes.Black, receiptX + labelWidth, y);
                y += lineHeight;

                // --- STORE NAME ---
                string storeNameLabel = "Store Name: ";
                e.Graphics.DrawString(storeNameLabel, boldFont, Brushes.Black, receiptX, y);
                labelWidth = e.Graphics.MeasureString(storeNameLabel, boldFont).Width;

                string storeNameValue = AppSession.StoreName;
                RectangleF storeNameRect = new RectangleF(receiptX + labelWidth, y, e.PageBounds.Width - (2 * receiptX) - labelWidth, 100);
                e.Graphics.DrawString(storeNameValue, font, Brushes.Black, storeNameRect);
                SizeF storeNameSize = e.Graphics.MeasureString(storeNameValue, font, (int)(e.PageBounds.Width - (2 * receiptX) - labelWidth));
                y += storeNameSize.Height;

                // --- STORE ADDRESS ---
                string addressLabel = "Store Address: ";
                e.Graphics.DrawString(addressLabel, boldFont, Brushes.Black, receiptX, y);
                labelWidth = e.Graphics.MeasureString(addressLabel, boldFont).Width;

                string addressValue = AppSession.StoreAddress;
                RectangleF addressRect = new RectangleF(receiptX + labelWidth, y, e.PageBounds.Width - (2 * receiptX) - labelWidth, 100);
                e.Graphics.DrawString(addressValue, font, Brushes.Black, addressRect);
                SizeF addressSize = e.Graphics.MeasureString(addressValue, font, (int)(e.PageBounds.Width - (2 * receiptX) - labelWidth));
                y += addressSize.Height;

                y += 18;

                // --- CASH ENTERED ---
                e.Graphics.DrawString("Cash Entered: ", boldFont, Brushes.Black, receiptX, y);
                labelWidth = e.Graphics.MeasureString("Cash Entered: ", boldFont).Width;
                e.Graphics.DrawString($"${localRequestBean.kioskTotalAmount}", font, Brushes.Black, receiptX + labelWidth, y);
                y += lineHeight;

                // --- FEES ---
                e.Graphics.DrawString("Crypto Conversion Fees: ", boldFont, Brushes.Black, receiptX, y);
                labelWidth = e.Graphics.MeasureString("Crypto Conversion Fees: ", boldFont).Width;
                e.Graphics.DrawString($"${localRequestBean.feeAmount}", font, Brushes.Black, receiptX + labelWidth, y);
                y += lineHeight;

                // --- STATUS / WALLET INFO ---
                string statusLabel = "Description: ";
                e.Graphics.DrawString(statusLabel, boldFont, Brushes.Black, receiptX, y);
                labelWidth = e.Graphics.MeasureString(statusLabel, boldFont).Width;

                string statusValue = localRequestBean.printmessage;
                RectangleF statusRect = new RectangleF(receiptX + labelWidth, y, e.PageBounds.Width - (2 * receiptX) - labelWidth, 100);
                e.Graphics.DrawString(statusValue, font, Brushes.Black, statusRect);
                SizeF statusSize = e.Graphics.MeasureString(statusValue, font, (int)(e.PageBounds.Width - (2 * receiptX) - labelWidth));
                y += statusSize.Height;


                y += 18;


                if (localRequestBean.isSucceed)
                {
                    // --- DISCLAIMER ---
                    e.Graphics.DrawString("Disclaimer", boldFont, Brushes.Black, receiptX, y);
                    y += 18;
                    string disclaimer = "This receipt confirms that your cash was successfully deposited and converted to USDT. " +
                                        "Please note that the credited balance will only appear in your PocketMint application if all eligibility requirements are met. " +
                                        "If the user is deemed ineligible, the purchase may be voided. In such cases, a refund can be requested at the point of purchase.";

                    RectangleF disclaimerRect = new RectangleF(receiptX, y, pageWidth - 40, 200);
                    e.Graphics.DrawString(disclaimer, font, Brushes.Black, disclaimerRect);
                }
                else
                {
                    // --- Notice ---
                    e.Graphics.DrawString("Refund Notice:", boldFont, Brushes.Black, receiptX, y);
                    y += 18;
                    string notice = "Your cash was not converted. Please request a refund at the point of purchase. For assistance, contact PocketMint.AI Support.";

                    RectangleF noticeRect = new RectangleF(receiptX, y, pageWidth - 40, 200);
                    e.Graphics.DrawString(notice, font, Brushes.Black, noticeRect);
                }


                y += 120;

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

                int receiptHeight = CalculateReceiptHeight();
                PaperSize paperSize = new PaperSize("CustomReceipt", 280, receiptHeight); // 2.8" wide
                printDocument.DefaultPageSettings.PaperSize = paperSize;
                
                //printPreviewDialog.ShowDialog();
                printDocument.Print();
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
        private int CalculateReceiptHeight()
        {
            int receiptWidth = 280; // Adjusted for 3-inch printer
            int totalHeight = 20;   // Top padding

            using (Bitmap dummyBitmap = new Bitmap(1, 1))
            using (Graphics g = Graphics.FromImage(dummyBitmap))
            {
                Font font = new Font("Arial", 10);
                Font boldFont = new Font("Arial", 11, FontStyle.Bold);
                int lineHeight = (int)(font.GetHeight(g) + 4);

                if (headerImage != null)
                {
                    int drawWidth = 150;
                    int drawHeight = (int)(headerImage.Height * (drawWidth / (float)headerImage.Width));
                    totalHeight += drawHeight + lineHeight;
                }

                // --- Fixed lines (timestamp, kiosk ID, cash, fees)
                totalHeight += 4 * lineHeight;

                // --- Store Name ---
                string storeName = AppSession.StoreName ?? "N/A";
                string storeNameLabel = "Store Name: ";
                float labelWidth = g.MeasureString(storeNameLabel, boldFont).Width;
                SizeF storeNameSize = g.MeasureString(storeName, font, (int)(receiptWidth - labelWidth - 20));
                totalHeight += (int)storeNameSize.Height;

                // --- Store Address ---
                string storeAddress = AppSession.StoreAddress ?? "N/A";
                string storeAddressLabel = "Store Address: ";
                labelWidth = g.MeasureString(storeAddressLabel, boldFont).Width;
                SizeF addressSize = g.MeasureString(storeAddress, font, (int)(receiptWidth - labelWidth - 20));
                totalHeight += (int)addressSize.Height;

                totalHeight += 18; // spacing after address

                // --- Status / Wallet info ---
                decimal netAmount = localRequestBean.kioskTotalAmount - localRequestBean.feeAmount;
                string status = $"✅ Successfully deposited {netAmount:F2} USDT into the wallet ending with 4707.";
                string statusLabel = "Status: ";
                labelWidth = g.MeasureString(statusLabel, boldFont).Width;
                SizeF statusSize = g.MeasureString(status, font, (int)(receiptWidth - labelWidth - 20));
                totalHeight += (int)statusSize.Height + 10;

                // --- Conditional section: disclaimer or refund notice ---
                totalHeight += lineHeight; // title line ("Disclaimer" or "Refund Notice")

                if (localRequestBean.isSucceed)
                {
                    string disclaimer = "This receipt confirms that your cash was successfully deposited and converted to USDT. " +
                                        "Please note that the credited balance will only appear in your PocketMint application if all eligibility requirements are met. " +
                                        "If the user is deemed ineligible, the purchase may be voided. In such cases, a refund can be requested at the point of purchase.";
                    SizeF disclaimerSize = g.MeasureString(disclaimer, font, receiptWidth - 20);
                    totalHeight += (int)disclaimerSize.Height + 10;
                }
                else
                {
                    string notice = "Your cash was not converted. Please request a refund at the point of purchase. For assistance, contact PocketMint.AI Support.";
                    SizeF noticeSize = g.MeasureString(notice, font, receiptWidth - 20);
                    totalHeight += (int)noticeSize.Height + 10;
                }

                // --- Bottom padding ---
                totalHeight += 3 * lineHeight;
            }

            return totalHeight;
        }



        private float DrawWrappedText(Graphics graphics, string text, Font font, float x, float y, float maxWidth, float lineSpacing = 5)
        {
            RectangleF layoutRect = new RectangleF(x, y, maxWidth, 1000); // Large height to allow wrapping
            graphics.DrawString(text, font, Brushes.Black, layoutRect);

            SizeF measuredSize = graphics.MeasureString(text, font, (int)maxWidth);
            return y + measuredSize.Height + lineSpacing;
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
