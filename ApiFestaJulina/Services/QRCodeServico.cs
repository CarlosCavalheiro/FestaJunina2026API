using QRCoder;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.Security.Cryptography;

namespace ApiFestaJulina.Services
{
    public class QRCodeServico
    {

        private HashAlgorithm _algoritmo;

        public QRCodeServico(HashAlgorithm algoritmo)
        {
            _algoritmo = algoritmo;
        }

        public byte[] GerarQRCode(string textoCodificar, string nomeArquivo)
        {
            // Pega o caminho físico da wwwroot
            
            string caminhoArquivo = System.Environment.CurrentDirectory + "\\..\\uploads\\QrCodes";
            //var caminhoArquivo = Path.Combine("\\Sites\\uploads\\QrCodes");

            if (!Directory.Exists(caminhoArquivo))
            {
                Directory.CreateDirectory(caminhoArquivo);
            }

            Console.WriteLine(caminhoArquivo);
            
            // Exemplo: Criar um caminho para uma pasta/arquivo específico dentro da wwwroot
            string pastaUploads = Path.Combine(caminhoArquivo, nomeArquivo + ".png");

            using (QRCodeGenerator gerador = new QRCodeGenerator())
            {
                using (QRCodeData dadosQR = gerador.CreateQrCode(textoCodificar, QRCodeGenerator.ECCLevel.Q))
                {
                    using (PngByteQRCode qrCodeEmBytes = new PngByteQRCode(dadosQR))
                    {
                        byte[] imagemQRCodeArray = qrCodeEmBytes.GetGraphic(2);
                        
                        
                        File.WriteAllBytes(pastaUploads, imagemQRCodeArray);

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