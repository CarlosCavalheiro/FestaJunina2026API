using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace ApiFestaJulina.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task EnviarEmailAsync(string destinatario, string assunto, string corpo)
        {
            var remetente = _configuration["EmailSettings:Email"];
            var senhaApp = _configuration["EmailSettings:Senha"];

            var mensagem = new MimeMessage();

            mensagem.From.Add(new MailboxAddress("Festa Julina", remetente));
            mensagem.To.Add(MailboxAddress.Parse(destinatario));
            mensagem.Subject = assunto;

            mensagem.Body = new TextPart("html")
            {
                Text = corpo
            };

            using var smtp = new SmtpClient();

            await smtp.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(remetente, senhaApp);
            await smtp.SendAsync(mensagem);
            await smtp.DisconnectAsync(true);
        }
    }
}