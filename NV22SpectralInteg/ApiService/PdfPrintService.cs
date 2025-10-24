using PdfiumViewer;
using System;
using System.Drawing.Printing;
using System.Net.Http;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NV22SpectralInteg.PdfPrintService
{
    public class PdfPrintService
    {
        public async Task PreviewAndPrintPdfFromUrl(string pdfUrl)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(pdfUrl);
                    response.EnsureSuccessStatusCode();

                    using (Stream pdfStream = await response.Content.ReadAsStreamAsync())
                    {
                        using (var pdfDocument = PdfDocument.Load(pdfStream))
                        {
                            using (var printDocument = pdfDocument.CreatePrintDocument())
                            {
                                // 1. Set HIGH-QUALITY settings (for the physical print job)
                                printDocument.DefaultPageSettings.PrinterResolution.Kind = PrinterResolutionKind.High;
                                printDocument.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);

                                // 2. DIRECTLY print the document without showing any dialog.
                                // The job is sent straight to the default printer's queue.
                                printDocument.Print();

                                // 2. Create the PrintDialog
                                //PrintDialog printDialog = new PrintDialog();
                                //printDialog.Document = printDocument;

                                //// 3. Show the dialog and check the result
                                //if (printDialog.ShowDialog() == DialogResult.OK)
                                //{
                                //    // 4. Print the document using the settings chosen by the user in the dialog
                                //    printDocument.Print();
                                //}
                            }
                        }
                    }
                }
            }
            catch (HttpRequestException httpEx)
            {
                MessageBox.Show($"Could not download the file.\nError: {httpEx.Message}", "Network Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during printing: {ex.Message}", "Processing Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}