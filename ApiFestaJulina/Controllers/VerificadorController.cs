using System;
using ApiFestaJulina.DTO;
using ApiFestaJulina.Models;
using ApiFestaJulina.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiFestaJulina.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VerificadorController : ControllerBase
    {
        private readonly AppDbContext _context;

        public VerificadorController(AppDbContext context)
        {
            _context = context;
        }
        
        [HttpPost("validar-qrCode")]
        public async Task<ActionResult<Ingressos>> GetQrCode([FromQuery] string qrCode, [FromBody] LeitorQrDTO dto )
        {
            if (string.IsNullOrWhiteSpace(qrCode))
            {
                return BadRequest("QrCode é obrigatório");
            }

            if (dto == null || dto.UsuarioQueLeu <= 0)
            {
                return BadRequest("Usuário da portaria inválido");
            }

            var qrCodeNormalizado = qrCode.Trim();

            var ingresso = await _context.Ingressos
            .FirstOrDefaultAsync(q => q.QrCode == qrCodeNormalizado);
            if (ingresso == null)
            {
                return BadRequest("Ingresso não encontrado");
            }

            if (ingresso.IdStatusValidacao == 1)
            {
                return BadRequest("Ingresso não foi pago!");
            }

            if (ingresso.IdStatusValidacao == 3)
            {
                return BadRequest("Ingresso já foi usado!");
            }

            if (ingresso.IdStatusValidacao == 4)
            {
                return BadRequest("O Pedido desse ingresso foi cancelado!");
            }

            if (ingresso.IdStatusValidacao != 2)
            {
                return BadRequest("Ingresso não está liberado para entrada");
            }

            ingresso.IdStatusValidacao = 3;
            ingresso.DtEntrada = DateTime.Now;
            ingresso.UsuarioQueLeu = dto.UsuarioQueLeu;

            await _context.SaveChangesAsync();

            return Ok("Bem vindo à Festa Julina!!");
        }
    }
}