using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;

namespace Bewirtung.Webserver.Data
{
    /// <summary>
    /// Wandelt Word-Dokumente (.docx) über den im Docker laufenden
    /// Stirling-PDF-Service in PDF um.
    /// </summary>
    public class StirlingPdfService
    {
        private readonly HttpClient _httpClient;
        private readonly string? _apiKey;

        public StirlingPdfService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["StirlingPdf:ApiKey"];            
        }

        /// <summary>
        /// Sendet das übergebene .docx an Stirling-PDF und liefert das erzeugte PDF als Bytes.
        /// </summary>
        /// <param name="docxBytes">Inhalt der Word-Datei.</param>
        /// <param name="dateiname">Dateiname der Word-Datei (für den Upload).</param>
        public async Task<byte[]> KonvertiereDocxZuPdfAsync(byte[] docxBytes, string dateiname)
        {
            using var content = new MultipartFormDataContent();

            var fileContent = new ByteArrayContent(docxBytes);
            fileContent.Headers.ContentType =
                new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.wordprocessingml.document");

            var uploadName = string.IsNullOrWhiteSpace(dateiname) ? "preisliste.docx" : dateiname;
            content.Add(fileContent, "fileInput", uploadName);

            // Stirling-PDF Endpunkt zum Umwandeln beliebiger Dateien in PDF.
            using var request = new HttpRequestMessage(HttpMethod.Post, "api/v1/convert/file/pdf")
            {
                Content = content
            };

            // Falls Stirling-PDF mit API-Key-Schutz läuft, den Schlüssel mitschicken.
            if (!string.IsNullOrWhiteSpace(_apiKey))
            {
                request.Headers.Add("X-API-KEY", _apiKey);
            }

            using var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsByteArrayAsync();
        }
    }
}
