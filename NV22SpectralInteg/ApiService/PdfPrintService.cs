using PdfiumViewer;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Windows.Forms;

namespace NV22SpectralInteg.PdfPrintService
{
    public class PdfPrintService
    {
        public async Task PreviewAndPrintPdfFromUrl(string pdfUrl)
        {
            try
            {
                // 1. Download the PDF file from the URL
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(pdfUrl);
                    response.EnsureSuccessStatusCode();

                    using (Stream pdfStream = await response.Content.ReadAsStreamAsync())
                    {
                        // 2. Load the PDF Document
                        using (var pdfDocument = PdfDocument.Load(pdfStream))
                        {
                            // 3. Create a Printable Document
                            using (var printDocument = pdfDocument.CreatePrintDocument())
                            {
                                // 4. Show Print Preview Dialog

                                PrintPreviewDialog previewDialog = new PrintPreviewDialog();
                                previewDialog.Document = printDocument;
                                previewDialog.PrintPreviewControl.Zoom = 3.0;

                                // This dialog handles the printing internally if the user clicks 'Print'.
                                DialogResult result = previewDialog.ShowDialog();

                                printDocument.Print();
                            }
                        }
                    }
                }
            }
            catch (HttpRequestException httpEx)
            {
                // Handles network errors (e.g., URL not found, server error)
                MessageBox.Show($"Could not download the file.\nCheck the URL or internet connection.\nError: {httpEx.Message}", "Network Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                // Handles errors during PDF loading or printing
                MessageBox.Show($"An error occurred during printing: {ex.Message}", "Processing Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}