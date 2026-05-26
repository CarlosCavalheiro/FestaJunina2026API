using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ApiFestaJulina.DTO;
using ApiFestaJulina.Models;
using ApiFestaJulina.Repository;
using ApiFestaJulina.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ApiFestaJulina.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PedidosController : ControllerBase
    {
        private readonly QRCodeServico _qrCodeServico;
        private readonly AppDbContext _context;

        private readonly IWebHostEnvironment _env;
        private readonly ILogger<PedidosController> _logger;

        public PedidosController(AppDbContext context, IWebHostEnvironment env, QRCodeServico qrCodeServico, ILogger<PedidosController> logger)
        {
            _context = context;
            _env = env;
            _qrCodeServico = qrCodeServico;
            _logger = logger;
        }

        
        [HttpGet("ListarPedidos")]
        public async Task<ActionResult<List<Pedidos>>> GetPedidos()
        {
            var ListaPedidos = await _context.Pedidos.ToListAsync();

            return Ok(ListaPedidos);
        }

        
        [HttpGet("ListarPedidoID")]
        public async Task<ActionResult<Pedidos>> GetUsuarioId(int idUsuario)
        {
            var pedido = await _context.Pedidos.FirstOrDefaultAsync(p => p.IdUsuario == idUsuario);
            if (pedido == null)
            {
                return NotFound($"Não foi encontrado nenhum pedido para o usuário com o ID {idUsuario} fornecido.");
            }

            return Ok(pedido);
        }

        [HttpGet("PedidoComStatusPendente")]
        public async Task<ActionResult<bool>> GetPedidoComStatusPendente(int idUsuario)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);
            if (usuario == null)
            {
                return NotFound("Usuário não encontrado");
            }
            
            var temPedidoPendente = await _context.Pedidos
                .AnyAsync(p => p.IdUsuario == idUsuario && p.IdStatus == 1 || p.IdStatus == 4);
            
            return Ok(temPedidoPendente);
        }

        [HttpGet("ValorTotalPendentePorUsuario")]
        public async Task<ActionResult<decimal>> GetValorTotalPendentePorUsuario(int idUsuario)
        {
            var valorTotal = await _context.Pedidos
                .Where(p => p.IdUsuario == idUsuario && p.IdStatus == 1)
                .SumAsync(p => p.Valor);

            return Ok(valorTotal);
        }

        [HttpPost("CriarPedido")]
        public async Task<ActionResult<Pedidos>> CriarPedido(PedidoDTO dto)
        {
            
            try
            {
                var pedido = new Pedidos
            {
                IdUsuario = dto.IdUsuario,
                Quantidade = dto.Quantidade,
                IdStatus = 1,
                DtaReserva = DateTime.Now,
                Valor = dto.Valor,
                TipoPagamento = dto.TipoPagamento,
                DtaFechamento = DateTime.Now.AddDays(1)
            };

            if (pedido.Quantidade > 4)
            {
                return BadRequest("Máximo de 4 ingressos por pedido");
            }

            _context.Pedidos.Add(pedido);
            await _context.SaveChangesAsync();

            var lotesCarregados = new Dictionary<int, Lotes>();

            foreach (var ingressoDto in dto.ListaIngressos)
            {
                int tipoLote = ingressoDto.IdTipo == 5 ? 2 : 1;

                var lote = await _context.Lotes
                    .Where(i =>
                        i.TipoLote == tipoLote &&
                        i.Ativo &&
                        i.Saldo > 0
                    )
                    .OrderBy(i => i.IdLote)
                    .ThenBy(i => i.DataCriacao)
                    .FirstOrDefaultAsync();

                if (lote == null)
                {
                    return BadRequest($"Lote {ingressoDto.IdLote} não encontrado");
                }
                if (lote.Saldo <= 0)
                {
                    return BadRequest($"Lote {lote.IdLote} sem saldo");
                }
                lote.Saldo--;

                var codigoQR = $"{Guid.NewGuid()}{dto.IdUsuario}{lote.IdLote}";
                var QRTextoCriptografado = _qrCodeServico.CriptografarQRcode(codigoQR);

                var ingresso = new Ingressos
                {
                    IdPedido = pedido.IdPedido,
                    IdLote = lote.IdLote,
                    Valor = ingressoDto.Valor,
                    QrCode = QRTextoCriptografado,
                    IdUsuario = dto.IdUsuario,
                    IdTipo = ingressoDto.IdTipo,
                    IdStatusValidacao = 1,
                    UsuarioQueLeu = 0
                };

                _context.Ingressos.Add(ingresso);
            }

                await _context.SaveChangesAsync();

                
                return Ok("Pedido criado com sucesso!");

            } catch (Exception ex)
            {
                return BadRequest(new
                {
                    Mensagem = ex.Message,
                    Inner = ex.InnerException?.Message,
                    Inner2 = ex.InnerException?.InnerException?.Message
                });
            }
            
            
        }
        
        [HttpPut("EditarPedido")]
        public async Task<ActionResult<Pedidos>> EditarPedido(int id, PedidoEditar pedido)
        {
            var pedidoExistent = await _context.Pedidos.FirstOrDefaultAsync(p => p.IdPedido == id);
            if (pedidoExistent == null)
            {
                return NotFound("Pedido não encontrado não encontrado!");
            }

            pedidoExistent.Quantidade = pedido.Quantidade;
            pedidoExistent.IdStatus = pedido.IdStatus;
            pedidoExistent.Valor = pedido.Valor;
            pedidoExistent.TipoPagamento = pedido.TipoPagamento;

            _context.Pedidos.Update(pedidoExistent);
            await _context.SaveChangesAsync();
            return Ok("Usuario Editado com Sucesso");
        }

        
        [HttpDelete("CancelarPedido")]
        public async Task<IActionResult> CancelarPedido(int id)
        {
            var pedido = await _context.Pedidos.FindAsync(id);
            if (pedido == null)
            {
                return NotFound();
            }

            _context.Pedidos.Remove(pedido);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [RequestSizeLimit(5 * 1024 * 1024)]
        [HttpPost("PublicarComprovante")]
        public async Task<ActionResult<Pedidos>> PublicarComprovante(IFormFile file, int id)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Nenhuma imagem foi enviada");
            }

            if (id <= 0)
            {
                return BadRequest("ID do pedido inválido");
            }

            const long limite = 5 * 1024 * 1024;
            if (file.Length > limite)
            {
                return BadRequest("A imagem deve ter no máximo 5MB");
            }

            var extensoesPermitidas = new [] { ".jpg", ".jpeg", ".png", ".pdf", ".webp"};
            var extensao = Path.GetExtension(file.FileName).ToLower();
            var pedido = await _context.Pedidos.FirstOrDefaultAsync(p => p.IdPedido == id);
            var pathSaveImage = Path.Combine("\\Sites\\uploads\\ComprovantesPixs");

            if (!Directory.Exists(pathSaveImage))
            {
                Directory.CreateDirectory(pathSaveImage);
            }

            if (!string.IsNullOrEmpty(pedido.FtComprovante))
            {
                var caminhoAntigo = Path.Combine(pathSaveImage, pedido.FtComprovante);

                if (System.IO.File.Exists(caminhoAntigo))
                {
                    System.IO.File.Delete(caminhoAntigo);
                }
            }

            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var fullPath = Path.Combine(pathSaveImage, fileName);

            if (!extensoesPermitidas.Contains(extensao))
            {
                return BadRequest("Formato inválido! Apenas JPG, PNG, JPEG, PDF e WEBP");
            }

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            pedido.FtComprovante = fileName;
            pedido.IdStatus = 4;

            await _context.SaveChangesAsync();

            return Ok(new { mensagem = "Foto atualizada!", arquivo = fileName});
        }
        
        
        [HttpPut("AlterarStatusdoPedidoEIngresso")]
        public async Task<IActionResult> AlterarStatus(int id_pedido, [FromBody] AlteraStatusPedidoDTO dto)
        {
            var pedido = await _context.Pedidos
                .FirstOrDefaultAsync(p => p.IdPedido == id_pedido);

            if (pedido == null)
            {
                return NotFound("Pedido não encontrado");
            }

            int novoStatus = dto.IdStatus;
            
            if (novoStatus != 1 || novoStatus != 2 || novoStatus != 3)
            {
                return BadRequest("Status inválido. Use: 1=Pendente, 2=Pago, 3=Cancelado");
            }

            pedido.IdStatus = novoStatus;
            pedido.DtaFechamento = DateTime.Now;

            int quantidadeIngressos = 0;

            if (novoStatus == 2)
            {
                var ingressos = await _context.Ingressos
                    .Where(i => i.IdPedido == id_pedido)
                    .ToListAsync();

                if (!ingressos.Any())
                {
                    return BadRequest("Nenhum ingresso encontrado para este pedido");
                }

                foreach (var ingresso in ingressos)
                {
                    ingresso.IdStatusValidacao = 2;
                }

                quantidadeIngressos = ingressos.Count;
            }

            if (novoStatus == 3)
            {
                var ingressos = await _context.Ingressos
                    .Where(i => i.IdPedido == id_pedido)
                    .ToListAsync();

                foreach (var ingresso in ingressos)
                {
                    ingresso.IdStatusValidacao = 4;

                    var loteAtual = await _context.Lotes
                        .FirstOrDefaultAsync(l => l.IdLote == ingresso.IdLote);

                    if (loteAtual != null)
                    {
                        // Se for infantil, devolve nele mesmo
                        if (loteAtual.TipoLote == 2)
                        {
                            loteAtual.Saldo++;
                        }
                        else
                        {
                            // Busca o próximo lote comum
                            var proximoLote = await _context.Lotes
                                .Where(l =>
                                    l.TipoLote == 1 &&
                                    l.IdLote > loteAtual.IdLote)
                                .OrderBy(l => l.IdLote)
                                .FirstOrDefaultAsync();

                            // Se existir próximo lote, soma nele
                            if (proximoLote != null)
                            {
                                proximoLote.Saldo++;
                            }
                            else
                            {
                                // Se não existir, devolve ao atual
                                loteAtual.Saldo++;
                            }
                        }
                    }
                }

                quantidadeIngressos = ingressos.Count;
            }

            await _context.SaveChangesAsync();

            var ingressosAtualizados = await _context.Ingressos
                .Where(i => i.IdPedido == id_pedido)
                .Select(i => new { i.IdIngresso, i.IdStatusValidacao })
                .ToListAsync();

            return Ok(new
            {
                mensagem = "Status do pedido alterado com sucesso",
                pedidoId = pedido.IdPedido,
                status = pedido.IdStatus,
                quantidadeIngressos,
                ingressos = ingressosAtualizados
            });
        }

        [HttpPut("AlterarStatusdoPedido")]
        public async Task<IActionResult> AlterarStatus(int id, int id_usuario, int novoStatus)
        {
            var pedido = await _context.Pedidos.FindAsync(id);

            if (pedido == null)
            {
                return NotFound("Pedido não encontrado");
            }

           if (novoStatus != 1 &&  novoStatus != 2 && novoStatus != 3)
            {
                return BadRequest("Status inválido");
            }

            pedido.IdStatus = novoStatus;
           
            pedido.UltimaAcaoPor = id_usuario;
           
            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem = "Status atualizado com sucesso!!!",
                pedido
            });
        }

    }
}