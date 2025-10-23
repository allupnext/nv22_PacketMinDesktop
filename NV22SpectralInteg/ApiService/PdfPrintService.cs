using PdfiumPdfDocument = PdfiumViewer.PdfDocument;
using System;
using System.Drawing.Printing;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NV22SpectralInteg.PdfPrintService
{
    public class PdfPrintService
    {
        private static readonly string PdfFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "KioskReportPDF");

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

        public async Task PreviewAndPrintPdfFromFile(string pdfUrl)
        {
            try
            {
                Directory.CreateDirectory(PdfFolderPath);

                string fileName = GetFileNameFromUrl(pdfUrl);
                string localPath = Path.Combine(PdfFolderPath, fileName);

                using (var client = new HttpClient())
                {
                    var bytes = await client.GetByteArrayAsync(pdfUrl);
                    await File.WriteAllBytesAsync(localPath, bytes);
                }

                using (var document = PdfiumPdfDocument.Load(localPath))
                {
                    using (var printDoc = document.CreatePrintDocument())
                    {
                        using (var previewDialog = new PrintPreviewDialog())
                        {
                            previewDialog.Document = printDoc;
                            previewDialog.ShowDialog();
                        }

                        // After the preview, you can directly print the same document
                        printDoc.Print();
                    }
                }
            }
            catch (HttpRequestException httpEx)
            {
                MessageBox.Show($"Failed to download PDF.\nError: {httpEx.Message}", "Download Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (IOException ioEx)
            {
                MessageBox.Show($"File access error.\nError: {ioEx.Message}", "File Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)  // Use generic exception because PdfException is not defined here
            {
                MessageBox.Show($"Unexpected error occurred.\nError: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
