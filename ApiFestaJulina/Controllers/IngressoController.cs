
using Microsoft.AspNetCore.Mvc;
using ApiFestaJulina.Repository;
using ApiFestaJulina.Models;
using Microsoft.EntityFrameworkCore;
using ApiFestaJulina.Services;
using ApiFestaJulina.DTO;
using System.Security.Cryptography;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using QRCoder.Exceptions;

namespace ApiFestaJulina.Controllers
{
    
    [ApiController]
    [Route("api/[controller]")]
    public class IngressoController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly QRCodeServico _qrCodeServico;
        private HashAlgorithm _algoritmo;

        private const int Limite_Ingresso = 4;

        public IngressoController(AppDbContext context, QRCodeServico qRCodeServico, HashAlgorithm algoritmo)
        {
            _context = context;
            _qrCodeServico = qRCodeServico;
            _algoritmo = algoritmo;
        }

        [HttpGet("ListarIngressos")]
        public async Task<ActionResult<IEnumerable<Ingressos>>> GetIngressos()
        {
            var ingressos = await _context.Ingressos.ToListAsync();
            return Ok(ingressos);
        }

        [HttpGet ("ListarIngressoID")]
        public async Task<ActionResult<Ingressos>> GetIngresso(int id)
        {
            var ingresso = await _context.Ingressos.FindAsync(id);
            if (ingresso == null)
            {
                return NotFound();
            }

            return ingresso;
        }

        // [HttpPost("CriarIngresso")]
        // public async Task<ActionResult<Ingressos>> CriarIngresso([FromBody] IngressoDTO dto, int id_usuario, int id_pedido, int id_lote)
        // {
        //     var ingressos = await _context.Ingressos.ToListAsync();
        //     var numero_ingressos = ingressos.Count();
        //     var lote = await _context.Lotes.FirstOrDefaultAsync(l => l.IdLote == id_lote);
        //     if (lote == null)
        //     {
        //         return BadRequest("Lote não encontrado");
        //     }
        [HttpGet("BuscarIngressoPorUsuario")]
        public async Task<ActionResult<IEnumerable<Ingressos>>> BuscarIngressoPorUsuario(int idUsuario)
        {
            var ingressosComUsuario = await _context.Ingressos
                .Where(i => i.IdUsuario == idUsuario)
                .Join(_context.Usuarios, 
                    i => i.IdUsuario, 
                    u => u.IdUsuario, 
                    (i, u) => new IngressoComUsuarioDTO
                    {
                        IdIngresso = i.IdIngresso,
                        IdPedido = i.IdPedido,
                        IdLote = i.IdLote,
                        IdTipo = i.IdTipo,
                        NomeTipo = _context.Tipo
                            .Where(t => t.IdTipo == i.IdTipo)
                            .Select(t => t.Descricao)
                            .FirstOrDefault(),
                        Valor = i.Valor,
                        QrCode = i.QrCode,
                        IdUsuario = u.IdUsuario,
                        Nome = u.Nome,
                        Email = u.Email,
                        Telefone = u.Telefone,
                        PedidoIdStatus = _context.Pedidos
                            .Where(p => p.IdPedido == i.IdPedido)
                            .Select(p => p.IdStatus)
                            .FirstOrDefault(),
                        PedidoFtComprovante = _context.Pedidos
                            .Where(p => p.IdPedido == i.IdPedido)
                            .Select(p => p.FtComprovante)
                            .FirstOrDefault(),
                        PedidoTipoPagamento = _context.Pedidos
                            .Where(p => p.IdPedido == i.IdPedido)
                            .Select(p => p.TipoPagamento)
                            .FirstOrDefault(),
                    })
                .ToListAsync()
                .ConfigureAwait(false);
            
            if (ingressosComUsuario == null || !ingressosComUsuario.Any())
            {
                return NotFound("Nenhum ingresso encontrado para este usuário.");
            }
            
            return Ok(ingressosComUsuario);
        }

        // [HttpPost("CriarIngresso")]
        // public async Task<ActionResult<Ingressos>> CriarIngresso([FromBody] IngressoDTO dto, int id_usuario, int id_pedido, int id_lote)
        // {
        //     var ingressos = await _context.Ingressos.ToListAsync();
        //     var numero_ingressos = ingressos.Count();
        //     var lote = await _context.Lotes.FirstOrDefaultAsync(l => l.IdLote == id_lote);
        //     if (lote == null)
        //     {
        //         return BadRequest("Lote não encontrado");
        //     }

        //     var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.IdUsuario == id_usuario);
        //     if (usuario == null)
        //     {
        //         return BadRequest("Usuário não encontrado");
        //     }

        //     var pedido = await _context.Pedidos.FirstOrDefaultAsync(p => p.IdPedido == id_pedido);
        //     if (pedido == null)
        //     {
        //         return BadRequest("Pedido não encontrado");
        //     }
        //     var valor_pedido =  pedido.IdPedido;

        //     var codigoQR = $"{usuario.IdUsuario}{pedido.IdPedido}{lote.IdLote}{numero_ingressos}";
        //     //var QRTextoCriptografado = _algoritmo.CriptografarQRcode(codigoQR);

        //     // Ingressos ingresso = new Ingressos
        //     // {
        //     //     IdPedido = pedido.IdPedido,
        //     //     IdUsuario = usuario.IdUsuario,
        //     //     IdLote = lote.IdLote,
        //     //     IdTipo = dto.IdTipo,
        //     //     Valor = dto.Valor,
        //     //     QrCode = QRTextoCriptografado
        //     // };

        //     _context.Ingressos.Add(ingresso);
        //     await _context.SaveChangesAsync();
        //     _context.Ingressos.Update(ingresso);
        //     await _context.SaveChangesAsync();

        //     // Gerar QR Code após salvar para obter o ID
        //     var dadosDoIngresso = QRTextoCriptografado;
        //     byte[] imagemQRCode = _qrCodeServico.GerarQRCode(dadosDoIngresso, ingresso.IdIngresso.ToString());
        //     string imagemBase64 = Convert.ToBase64String(imagemQRCode);

        //     return Ok(new 
        //     {
        //         Ingresso = ingresso,
        //         QRCodeImagem = $"data:image/png;base64,{imagemBase64}"
        //     });
        // }

        // [HttpPut ("EditarIngresso")]
        // public async Task<IActionResult> EditarIngresso(int id, Ingressos ingresso)
        // {
        //     if (id != ingresso.IdIngresso)
        //     {
        //         return BadRequest();
        //     }

        //     _context.Entry(ingresso).State = EntityState.Modified;
        //     await _context.SaveChangesAsync();

        //     return NoContent();
        // }

        [HttpDelete ("CancelarIngresso")]
        public async Task<IActionResult> CancelarIngresso(int id)
        {
            var ingresso = await _context.Ingressos.FindAsync(id);
            if (ingresso == null)
            {
                return NotFound();
            }

            _context.Ingressos.Remove(ingresso);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // [HttpPost]
        // public async Task<IActionResult<Ingressos>> LeituraQR(int id, LeituraQrDTO codigo)
        // {
        //     var IngressoExistente = await _context.Ingressos.FirstOrDefaultAsync(i => i.IdIngresso == id);

        //     if (IngressoExistente == null)
        //     {
        //         return NotFound("Ingresso não encontrado!");
        //     }

        //     var QRcriptografado = _algoritmo.ComputeHash(Encoding.UTF8.GetBytes(codigo.QrCode));
        //     var sb = new StringBuilder();

        //     foreach (var caractere in QRcriptografado)
        //     {
        //         sb.Append(caractere.ToString("X2"));
        //     }

        //     if((IngressoExistente.QrCode ?? string.Empty) != sb.ToString())
        //     {
        //         return BadRequest("QrCode Inválido ou incorreto");
        //     }


        // }


        

    }
}