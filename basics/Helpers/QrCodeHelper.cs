using QRCoder;

namespace basics.Helpers
{
    public static class QrCodeHelper
    {
        /// <summary>
        /// GUID değerini QR kod olarak Base64 string formatında PNG resme çevirir.
        /// </summary>
        /// <param name="biletKodu">Bilet GUID değeri</param>
        /// <returns>Base64 formatında PNG resim string'i</returns>
        public static string GenerateQrCodeBase64(Guid biletKodu)
        {
            using var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(biletKodu.ToString(), QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);
            var qrCodeBytes = qrCode.GetGraphic(10);
            return Convert.ToBase64String(qrCodeBytes);
        }

        /// <summary>
        /// QR kod resmini HTML img src için kullanılabilir data URI formatında döndürür.
        /// </summary>
        public static string GenerateQrCodeDataUri(Guid biletKodu)
        {
            var base64 = GenerateQrCodeBase64(biletKodu);
            return $"data:image/png;base64,{base64}";
        }
    }
}
