// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using Microsoft.AspNetCore.Mvc;
// using ApiFestaJulina.DTO;
// using ApiFestaJulina.Models;
// using ApiFestaJulina.Repository;
// using Microsoft.EntityFrameworkCore;

// namespace ApiFestaJulina.Controllers
// {
    
//     [ApiController]
//     [Route("api/[controller]")]
//     public class PagamentoController : ControllerBase
//     {
//         private readonly AppDbContext _context;

//         public PagamentoController(AppDbContext context)
//         {
//             _context = context;
//         }

//         [HttpPost("ProcessarPagamento")]
//         public async Task<IActionResult> ProcessarPagamento([FromBody] PagamentoDTO dto)
//         {
//             var pedido = await _context.Pedidos.FindAsync(dto.IdPedido);
//             if (pedido == null)
//             {
//                 return NotFound("Pedido não encontrado");
//             }

//             if (pedido.IdStatus == 2)
//             {
//                 return BadRequest("Pedido já foi pago");
//             }

//             pedido.IdStatus = 2;
//             pedido.TipoPagamento = dto.TipoPagamento;
//             pedido.DtaFechamento = DateTime.Now;

//             // Busca todos os ingressos do pedido
//             var ingressos = await _context.Ingressos
//             .Where(i => i.IdPedido == dto.IdPedido)
//             .ToListAsync();

//             // Atualiza status_validacao de cada ingresso
//             foreach (var ingresso in ingressos)
//             {
//                 ingresso.Status_validacao = true;
//             }

//             _context.Pedidos.Update(pedido);

//             await _context.SaveChangesAsync();

//             return Ok(new
//             {
//                 mensagem = "Pagamento processado com sucesso",
//                 pedido,
//                 quantidadeIngressos = ingressos.Count
//             });
//         }

//         // [HttpPost("ValidarIngressosPedido")]
//         // public async Task<ActionResult> ValidarIngressos(Pedidos pedido,Ingressos ingresso)
//         // {
//         //     // Presisa colocar uma referencia para o Id

//         //     var idPedido = await _context.Ingressos
//         //         .AnyAsync(ingresso => ingresso.IdPedido == idpedido);

//         //     var pedidos = await _context.Pedidos
//         //     .FirstOrDefaultAsync(p => p.IdPedido == pedido.IdPedido);

//         //     if (pedidos == null)
//         //     {
//         //         return BadRequest("Pedido não encontrado");
//         //     }

//         //     // Busca TODOS os ingressos do pedido
//         //     var ingressos = await _context.Ingressos
//         //     .Where(i => i.IdPedido == pedido.IdPedido)
//         //     .ToListAsync();

//         //     if (ingressos.Count == 0)
//         //     {
//         //     return BadRequest("Ingressos não encontrados");
//         //     }

//         //     // Verifica status do pedido
//         //     if (pedidos.IdStatus == 1)
//         //     {
//         //         return Ok("Pedido não pago!");
//         //     }

//         //     if (pedidos.IdStatus == 3)
//         //     {
//         //         return Ok("Pedido cancelado");
//         //     }

//         //     // Marca todos os ingressos como válidos
//         //     foreach (var ingresso in ingressos)
//         //     {
//         //         ingresso.Status_validacao = true;
//         //     }

//         //     await _context.SaveChangesAsync();

//         //     return Ok(new
//         //     {
//         //         mensagem = "Ingressos validados com sucesso",
//         //         quantidade = ingressos.Count
//         //     });
//         // }
//     }
// }

// //(i.IdStatus ==);