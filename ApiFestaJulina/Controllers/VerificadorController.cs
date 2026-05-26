using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiFestaJulina.DTO;
using ApiFestaJulina.Models;
using ApiFestaJulina.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

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
            var ingresso = await _context.Ingressos
            .FirstOrDefaultAsync(q => q.QrCode == qrCode);
            if (ingresso == null)
            {
                return BadRequest("Ingresso não encontrado");
            }
            if (ingresso.IdStatusValidacao == 1)
            {
                return BadRequest("Ingresso não foi pago!");
            }
            if (ingresso.IdStatusValidacao == 3 )//&& ingresso.DtEntrada != null)
            {
                return BadRequest("Ingresso já foi usado!");
            }
            if (ingresso.IdStatusValidacao == 4)
            {
                return BadRequest("O Pedido desse ingresso foi cancelado!");
            }

            ingresso.IdStatusValidacao = 3;
            ingresso.DtEntrada = DateTime.Now;
            ingresso.UsuarioQueLeu = dto.UsuarioQueLeu;

            await _context.SaveChangesAsync();

            return Ok("Bem vindo à Festa Julina!!");
        }
    }
}