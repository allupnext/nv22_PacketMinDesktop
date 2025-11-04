using System.Drawing.Printing;

namespace NV22SpectralInteg.PdfPrintService
{
    public class PdfPrintService
    {
        private readonly HttpClient _httpClient;

        public PdfPrintService(HttpClient? httpClient = null)
        {
            // Use dependency injection-friendly approach
            _httpClient = httpClient ?? new HttpClient();
        }

        public async Task PreviewAndPrintPdfFromUrl(string pdfUrl)
        {
            if (string.IsNullOrWhiteSpace(pdfUrl))
                throw new ArgumentException("PDF URL cannot be null or empty.", nameof(pdfUrl));

            try
            {
                string filePath = await DownloadLatestPdfAsync(pdfUrl);
                Logger.Log($"PDF successfully downloaded to: {filePath}");

                var pdf = IronPdf.PdfDocument.FromFile(filePath); 
                var printDoc = pdf.GetPrintDocument();

                //printDoc.PrinterSettings.PrinterNane = printerNane;
                printDoc.PrinterSettings.Copies = 1;

                var widthInHundredths = (int)(68 / 25.4 * 105); // 80 mm ~ 315
                var heightInHundredths = (int)(210 / 25.4 * 105); // 210 mm ~ 827

                printDoc.DefaultPageSettings.PaperSize = new PaperSize("80x210mm", widthInHundredths, heightInHundredths);
                printDoc.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);

                // No auto orientation detection — just set fixed or decide yourself
                printDoc.DefaultPageSettings.Landscape = false; // or true if you want landscape always

                //using (PrintPreviewDialog previewDialog = new PrintPreviewDialog())
                //{
                //    previewDialog.Document = printDoc;
                //    previewDialog.Width = 800;
                //    previewDialog.Height = 600;
                //    previewDialog.ShowDialog();
                //}

                printDoc.Print();
                Logger.Log("Kiosk report Printed successfully");
            }
            catch (HttpRequestException ex)
            {
                Logger.Log($"Failed to download the PDF. Network error: {ex.Message}");
            }
            catch (IOException ex)
            {
                Logger.Log($"File I/O error while saving the PDF: {ex.Message}");
            }
            catch (Exception ex)
            {
                Logger.Log($"Unexpected error: {ex.Message}");
            }
        }

        private async Task<string> DownloadLatestPdfAsync(string pdfUrl)
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string reportsFolder = Path.Combine(desktopPath, "KioskReports");

            Directory.CreateDirectory(reportsFolder);

            // STEP 1: Delete all old PDFs before saving new one ---
            try
            {
                foreach (var oldFile in Directory.GetFiles(reportsFolder, "*.pdf"))
                {
                    File.Delete(oldFile);
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Warning: Failed to delete old PDFs: {ex.Message}");
            }

            // STEP 2: Build the filename from the URL ---
            string fileName = Path.GetFileName(new Uri(pdfUrl).AbsolutePath);
            if (string.IsNullOrWhiteSpace(fileName) || !fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                fileName = $"Report_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            }

            string filePath = Path.Combine(reportsFolder, fileName);

            // --- STEP 3: Download the PDF ---
            using var response = await _httpClient.GetAsync(pdfUrl, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            await using var pdfStream = await response.Content.ReadAsStreamAsync();
            await using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);

            await pdfStream.CopyToAsync(fileStream);

            return filePath;
        }
    }
}