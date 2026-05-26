using QRCoder;
using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;

namespace ApiFestaJulina.Services
{
    public class QRCodeServico
    {

        private HashAlgorithm _algoritmo;
        private readonly AzureBlobStorageService _blobStorageService;

        public QRCodeServico(HashAlgorithm algoritmo, AzureBlobStorageService blobStorageService)
        {
            _algoritmo = algoritmo;
            _blobStorageService = blobStorageService;
        }

        public byte[] GerarQRCode(string textoCodificar, string nomeArquivo)
        {
            const string pastaBlob = "qrcodes";
            var nomeFinalArquivo = nomeArquivo + ".png";

            using (QRCodeGenerator gerador = new QRCodeGenerator())
            {
                using (QRCodeData dadosQR = gerador.CreateQrCode(textoCodificar, QRCodeGenerator.ECCLevel.Q))
                {
                    using (PngByteQRCode qrCodeEmBytes = new PngByteQRCode(dadosQR))
                    {
                        byte[] imagemQRCodeArray = qrCodeEmBytes.GetGraphic(2);

                        _blobStorageService.UploadBytes(imagemQRCodeArray, pastaBlob, nomeFinalArquivo, "image/png");

                        return imagemQRCodeArray;
                    }
                }
            }
        }

        public string CriptografarQRcode(string qrCode)
        {
            var encodedValue = Encoding.UTF8.GetBytes(qrCode);
            var encryptedQrcode = _algoritmo.ComputeHash(encodedValue);

            var sb = new StringBuilder();
            foreach (var caracter in encryptedQrcode)
            {
                sb.Append(caracter.ToString("X2"));
            }

            return sb.ToString();
        }
    }
}