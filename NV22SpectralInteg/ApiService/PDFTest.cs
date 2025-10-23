using System;
using System.Diagnostics;
using System.Drawing.Printing;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms; 

namespace NV22SpectralInteg.PdfService;

public class PdfPrintService
{
    private static readonly string PdfFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "KioskReportPDF");
    private PrintDocument printDocument;


    private string GetFileNameFromUrl(string url)
    {
        try
        {
            Uri uri = new Uri(url);
            string fileName = Path.GetFileName(uri.LocalPath);
            if (string.IsNullOrEmpty(fileName))
                fileName = "downloaded_report.pdf";
            if (!fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                fileName += ".pdf";
            return fileName;
        }
        catch
        {
            return "downloaded_report.pdf";
        }
    }

    public async Task DownloadAndPrintPdf(string pdfUrl)
    {
        string localPath = null;
        try
        {
            // Ensure directory exists
            Directory.CreateDirectory(PdfFolderPath);

            string fileName = GetFileNameFromUrl(pdfUrl);
            localPath = Path.Combine(PdfFolderPath, fileName);

            // Download the PDF
            using (var client = new HttpClient())
            {
                try
                {
                    var bytes = await client.GetByteArrayAsync(pdfUrl);
                    await File.WriteAllBytesAsync(localPath, bytes);
                }
                catch (Exception ex)
                {
                    Logger.Log($"Failed to download PDF: {ex.Message}");
                    return;
                }
            }

            // --- Printing/Preview Logic ---
            try
            {
                // Open the PDF file for preview (user-facing)
                Process.Start(new ProcessStartInfo
                {
                    FileName = localPath, // Open the file itself
                    UseShellExecute = true // Let the OS determine the handler (e.g., Edge, Adobe Reader)
                });

                Logger.Log($"Opened PDF for preview: {localPath}");

                // Load the PDF (assuming PdfDocument.FromFile is a valid async-compatible method)
                using (var pdf = PdfDocument.FromFile(localPath))
                {
                    // Perform a silent print job
                    await pdf.Print();
                    Logger.Log($"Printed PDF silently: {localPath}");
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error during PDF preview/printing: {ex.Message}");
            }
        }
        catch (HttpRequestException httpEx)
        {
            MessageBox.Show($"Error downloading PDF. Check URL and connectivity.\n{httpEx.Message}",
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch (Exception ex)
        {
            // Catches file-not-found for Edge if used, or other I/O errors.
            MessageBox.Show($"Error downloading or printing PDF:\n{ex.Message}\nLocal file path: {localPath}",
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}