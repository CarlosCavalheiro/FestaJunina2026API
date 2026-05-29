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
        private readonly ILogger<PedidosController> _logger;
        private readonly AzureBlobStorageService _blobStorageService;

        public PedidosController(AppDbContext context, QRCodeServico qrCodeServico, ILogger<PedidosController> logger, AzureBlobStorageService blobStorageService)
        {
            _context = context;
            _qrCodeServico = qrCodeServico;
            _logger = logger;
            _blobStorageService = blobStorageService;
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
                .AnyAsync(p => p.IdUsuario == idUsuario && (p.IdStatus == 1 || p.IdStatus == 4));
            
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
                if (dto == null)
                {
                    return BadRequest("Dados do pedido são obrigatórios");
                }

                if (dto.ListaIngressos == null || dto.ListaIngressos.Count == 0)
                {
                    return BadRequest("Nenhum ingresso foi informado");
                }

                if (dto.Quantidade <= 0)
                {
                    return BadRequest("Quantidade inválida");
                }

                if (dto.Quantidade > 4)
                {
                    return BadRequest("Máximo de 4 ingressos por pedido");
                }

                if (dto.Quantidade != dto.ListaIngressos.Count)
                {
                    return BadRequest("Quantidade informada não confere com a quantidade de ingressos enviados");
                }

                var ingressosTipo1 = dto.ListaIngressos.Count(i => i.IdTipo != 5);
                var ingressosTipo2 = dto.ListaIngressos.Count(i => i.IdTipo == 5);

                var saldoTipo1 = await _context.Lotes
                    .Where(l => l.TipoLote == 1 && l.Ativo)
                    .SumAsync(l => l.Saldo);

                var saldoTipo2 = await _context.Lotes
                    .Where(l => l.TipoLote == 2 && l.Ativo)
                    .SumAsync(l => l.Saldo);

                if (saldoTipo1 < ingressosTipo1)
                {
                    return BadRequest("Não há saldo suficiente para os ingressos do lote padrão");
                }

                if (saldoTipo2 < ingressosTipo2)
                {
                    return BadRequest("Não há saldo suficiente para os ingressos do lote promocional");
                }

                await using var transaction = await _context.Database.BeginTransactionAsync();

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

                _context.Pedidos.Add(pedido);
                await _context.SaveChangesAsync();

                var ingressosCriados = new List<Ingressos>();

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
                        await transaction.RollbackAsync();
                        return BadRequest($"Lote {ingressoDto.IdLote} não encontrado");
                    }

                    if (lote.Saldo <= 0)
                    {
                        await transaction.RollbackAsync();
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
                    ingressosCriados.Add(ingresso);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                foreach (var ingresso in ingressosCriados)
                {
                    if (string.IsNullOrWhiteSpace(ingresso.QrCode))
                    {
                        continue;
                    }

                    _qrCodeServico.GerarQRCode(ingresso.QrCode, ingresso.IdIngresso.ToString());
                }

                return Ok("Pedido criado com sucesso!");
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Mensagem = ex.Message,
                    Inner = ex.InnerException?.Message,
                    Inner2 = ex.InnerException?.InnerException?.Message
                });
            }
        }

            [HttpPost("ReprocessarIngressosPedido")]
            public async Task<IActionResult> ReprocessarIngressosPedido([FromQuery] int id_pedido, [FromQuery] int id_tipo)
            {
                if (id_pedido <= 0)
                {
                    return BadRequest("ID do pedido inválido");
                }

                if (id_tipo != 1 && id_tipo != 5)
                {
                    return BadRequest("ID do tipo inválido. Use 1 ou 5");
                }

                var pedido = await _context.Pedidos
                    .FirstOrDefaultAsync(p => p.IdPedido == id_pedido);

                if (pedido == null)
                {
                    return NotFound("Pedido não encontrado");
                }

                if (pedido.Quantidade <= 0)
                {
                    return BadRequest("Pedido sem quantidade de ingressos");
                }

                var ingressosExistentes = await _context.Ingressos
                    .CountAsync(i => i.IdPedido == id_pedido);

                if (ingressosExistentes > pedido.Quantidade)
                {
                    return Conflict("O pedido já possui mais ingressos do que a quantidade informada");
                }

                if (ingressosExistentes == pedido.Quantidade)
                {
                    return Ok(new
                    {
                        mensagem = "Pedido já possui a quantidade correta de ingressos",
                        pedidoId = pedido.IdPedido,
                        ingressosExistentes,
                        ingressosCriados = 0
                    });
                }

                var faltantes = pedido.Quantidade - ingressosExistentes;
                var tipoLote = id_tipo == 5 ? 2 : 1;

                var saldoDisponivel = await _context.Lotes
                    .Where(l => l.TipoLote == tipoLote && l.Ativo)
                    .SumAsync(l => l.Saldo);

                if (saldoDisponivel < faltantes)
                {
                    return BadRequest("Não há saldo suficiente no lote para recriar os ingressos faltantes");
                }

                await using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    var ingressosCriados = new List<Ingressos>();

                    for (int indice = 0; indice < faltantes; indice++)
                    {
                        var lote = await _context.Lotes
                            .Where(l =>
                                l.TipoLote == tipoLote &&
                                l.Ativo &&
                                l.Saldo > 0)
                            .OrderBy(l => l.IdLote)
                            .ThenBy(l => l.DataCriacao)
                            .FirstOrDefaultAsync();

                        if (lote == null)
                        {
                            await transaction.RollbackAsync();
                            return BadRequest("Não foi possível localizar um lote com saldo disponível");
                        }

                        if (lote.Saldo <= 0)
                        {
                            await transaction.RollbackAsync();
                            return BadRequest($"Lote {lote.IdLote} sem saldo");
                        }

                        lote.Saldo--;

                        var codigoQR = $"{Guid.NewGuid()}{pedido.IdUsuario}{lote.IdLote}";
                        var QRTextoCriptografado = _qrCodeServico.CriptografarQRcode(codigoQR);

                        var ingresso = new Ingressos
                        {
                            IdPedido = pedido.IdPedido,
                            IdLote = lote.IdLote,
                            Valor = pedido.Valor / pedido.Quantidade,
                            QrCode = QRTextoCriptografado,
                            IdUsuario = pedido.IdUsuario,
                            IdTipo = id_tipo,
                            IdStatusValidacao = 1,
                            UsuarioQueLeu = 0
                        };

                        _context.Ingressos.Add(ingresso);
                        ingressosCriados.Add(ingresso);
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    foreach (var ingresso in ingressosCriados)
                    {
                        if (string.IsNullOrWhiteSpace(ingresso.QrCode))
                        {
                            continue;
                        }

                        _qrCodeServico.GerarQRCode(ingresso.QrCode, ingresso.IdIngresso.ToString());
                    }

                    return Ok(new
                    {
                        mensagem = "Ingressos recriados com sucesso",
                        pedidoId = pedido.IdPedido,
                        ingressosExistentes,
                        ingressosCriados = ingressosCriados.Count,
                        faltantes
                    });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

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

            var (quantidadeIngressos, erro) = await AtualizarStatusPedidoEIngressosAsync(pedido, 3);
            if (!string.IsNullOrEmpty(erro))
            {
                return BadRequest(erro);
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem = "Pedido cancelado com sucesso",
                pedidoId = pedido.IdPedido,
                status = pedido.IdStatus,
                quantidadeIngressos
            });
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
            const string pastaBlob = "comprovantes";

            if (pedido == null)
            {
                return NotFound("Pedido não encontrado");
            }

            if (!string.IsNullOrEmpty(pedido.FtComprovante))
            {
                await _blobStorageService.DeleteIfExistsAsync(pastaBlob, pedido.FtComprovante);
            }

            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);

            if (!extensoesPermitidas.Contains(extensao))
            {
                return BadRequest("Formato inválido! Apenas JPG, PNG, JPEG, PDF e WEBP");
            }

            using (var stream = file.OpenReadStream())
            {
                await _blobStorageService.UploadFileAsync(stream, pastaBlob, fileName, file.ContentType);
            }

            pedido.FtComprovante = fileName;
            pedido.IdStatus = 4;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem = "Foto atualizada!",
                arquivo = fileName,
                url = _blobStorageService.GetBlobUrl(pastaBlob, fileName)
            });
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
            
            if (novoStatus != 1 && novoStatus != 2 && novoStatus != 3)
            {
                return BadRequest("Status inválido. Use: 1=Pendente, 2=Pago, 3=Cancelado");
            }

            var (quantidadeIngressos, erro) = await AtualizarStatusPedidoEIngressosAsync(pedido, novoStatus);
            if (!string.IsNullOrEmpty(erro))
            {
                return BadRequest(erro);
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

            var (quantidadeIngressos, erro) = await AtualizarStatusPedidoEIngressosAsync(pedido, novoStatus);
            if (!string.IsNullOrEmpty(erro))
            {
                return BadRequest(erro);
            }

            pedido.UltimaAcaoPor = id_usuario;
           
            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem = "Status atualizado com sucesso!!!",
                pedido,
                quantidadeIngressos
            });
        }

        private async Task<(int QuantidadeIngressos, string? Erro)> AtualizarStatusPedidoEIngressosAsync(Pedidos pedido, int novoStatus)
        {
            pedido.IdStatus = novoStatus;
            pedido.DtaFechamento = DateTime.Now;

            int quantidadeIngressos = 0;

            if (novoStatus == 2)
            {
                var ingressos = await _context.Ingressos
                    .Where(i => i.IdPedido == pedido.IdPedido)
                    .ToListAsync();

                if (!ingressos.Any())
                {
                    return (0, "Nenhum ingresso encontrado para este pedido");
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
                    .Where(i => i.IdPedido == pedido.IdPedido)
                    .ToListAsync();

                foreach (var ingresso in ingressos)
                {
                    ingresso.IdStatusValidacao = 4;

                    var loteAtual = await _context.Lotes
                        .FirstOrDefaultAsync(l => l.IdLote == ingresso.IdLote);

                    if (loteAtual == null)
                    {
                        continue;
                    }

                    if (loteAtual.TipoLote == 2)
                    {
                        loteAtual.Saldo++;
                    }
                    else
                    {
                        var proximoLote = await _context.Lotes
                            .Where(l => l.TipoLote == 1 && l.IdLote > loteAtual.IdLote)
                            .OrderBy(l => l.IdLote)
                            .FirstOrDefaultAsync();

                        if (proximoLote != null)
                        {
                            proximoLote.Saldo++;
                        }
                        else
                        {
                            loteAtual.Saldo++;
                        }
                    }
                }

                quantidadeIngressos = ingressos.Count;
            }

            return (quantidadeIngressos, null);
        }

    }
}