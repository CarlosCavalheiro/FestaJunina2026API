using Microsoft.AspNetCore.Mvc;
using ApiFestaJulina.Repository;
using ApiFestaJulina.Models;
using Microsoft.EntityFrameworkCore;
using ApiFestaJulina.DTO;

namespace ApiFestaJulina.Controllers
{
    
    [ApiController]
    [Route("api/[controller]")]
    public class LotesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LotesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("ListarLotes")]
        public async Task<ActionResult<IEnumerable<Lotes>>> GetLotes()
        {
            var lotes = await _context.Lotes.ToListAsync();
                //.OrderBy(l => l.IdLote)
            return Ok(lotes);
        }

        [HttpGet("ListarLoteID")]
        public async Task<ActionResult<Lotes>> GetLote(int id)
        {
            var lote = await _context.Lotes.FindAsync(id);
            if (lote == null)
            {
                return NotFound();
            }

            return lote;
        }

        [HttpPost("CriarLote")]
        public async Task<ActionResult<Lotes>> CriarLote(LotesDTO dto)
        {
            var lote = new Lotes
            {
                IdEvento = dto.IdEvento,
                QtdeIngressosLotes = dto.QtdeIngressosLotes,
                ValorIng = dto.ValorIng,
                DataCriacao = DateTime.Now,
                DataFechamento = dto.DataFechamento,
                Descricao = dto.Descricao,
                Saldo = dto.Saldo,
                TipoLote = dto.TipoLote
            };
            _context.Lotes.Add(lote);
            await _context.SaveChangesAsync();

            return Ok(lote);
        }

        [HttpPut("EditarLote")]
        public async Task<IActionResult> EditarLote(int id, LotesEditar editar)
        {
            var lote = await _context.Lotes.FirstOrDefaultAsync(l => l.IdLote == id);

            if (lote == null)
            {
                return NotFound($"Lote com o id {id} não foi encontrado");
            }
            
            lote.IdEvento = lote.IdEvento;
            
            lote.QtdeIngressosLotes = editar.QtdeIngressosLotes;
            lote.ValorIng = editar.ValorIng;
            lote.DataCriacao = DateTime.Now;
            lote.DataFechamento = editar.DataFechamento;
            lote.Descricao = editar.Descricao;
            lote.Saldo = editar.Saldo;
            lote.TipoLote = editar.TipoLote;

            _context.Lotes.Update(lote);
            await _context.SaveChangesAsync();

            return Ok("Lote editado com sucesso");
        }

        [HttpDelete ("CancelarLote")]
        public async Task<IActionResult> CancelarLote(int id)
        {
            var lote = await _context.Lotes.FindAsync(id);
            if (lote == null)
            {
                return NotFound();
            }

            _context.Lotes.Remove(lote);
            await _context.SaveChangesAsync();

            return Ok("Lote deletado com sucesso");
        }

        //Desativar lote
        [HttpPut("DesativarLote")]
        public async Task<IActionResult> DesativarLote(int id, AlterarStatusLoteDTO dto)
        {
            var lote = await _context.Lotes.FirstOrDefaultAsync(l => l.IdLote == id);
            if (lote == null)
            {
                return BadRequest("Esse lote não existe");
            }

            lote.Ativo = dto.Ativo;

            _context.Lotes.Update(lote);
            await _context.SaveChangesAsync();

            return Ok(lote.Ativo);
        }
    }
}
