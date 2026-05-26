using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiFestaJulina.DTO;
using ApiFestaJulina.Models;
using ApiFestaJulina.Repository;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiFestaJulina.Controllers
{   
    
    [ApiController]
    [Route("api/[controller]")]

    public class PesquisaController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PesquisaController(AppDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<ActionResult<List<Perguntas>>> ListarPerguntas()
        {
            var listarperguntas = await _context.Perguntas.ToListAsync();
            return Ok(listarperguntas);
        }
        [HttpPost("registro-pergunta")]
        public async Task<ActionResult<Perguntas>> RegistroPerguntas(NewPerguntas newPergunta)
        {
            Perguntas perguntas = new Perguntas
            {
                IdTpUser = newPergunta.IdTpUser,
                DescricaoPergunta = newPergunta.DescricaoPergunta,
                TipoPergunta = newPergunta.TipoPergunta
            };

            await _context.Perguntas.AddAsync(perguntas);
            await _context.SaveChangesAsync();
            return Ok(perguntas);
        }
        [HttpPost("registro-resposta")]
        public async Task<ActionResult<Resposta>> RegistroResposta(NewResposta newResposta)
        {
            Resposta resposta = new Resposta
            {
                IdPergunta = newResposta.IdPergunta,
                resposta = newResposta.resposta
            };

            await _context.Respostas.AddAsync(resposta);
            await _context.SaveChangesAsync();
            return Ok(resposta);
        }

        [HttpDelete("delete-pergunta")]
        public async Task<ActionResult> DeletarPerguntas(int id)
        {
            var perguntaExistente = await _context.Perguntas.FindAsync(id);

            if (perguntaExistente == null)
            {
                return NotFound("Pergunta não encontrada");
            }

            _context.Perguntas.Remove(perguntaExistente);
            await _context.SaveChangesAsync();

            return Ok("Pergunta removida com sucesso");
        }

        [HttpGet("resposta-por-idpergunta")]//Puxa as respostas pelo idPergunta
        public async Task<ActionResult<List<Resposta>>> GetRespostaPorPerguntaId([FromQuery] int idPergunta)
        {
            var perguntaExiste = await _context.Perguntas
                .AnyAsync(p => p.IdPergunta == idPergunta);

            if (!perguntaExiste)
            {
                return NotFound($"A pergunta com id {idPergunta} não foi encontrada!");
            }

            var listaRespostas = await _context.Respostas
                .Where(r => r.IdPergunta == idPergunta)
                .ToListAsync();

            return Ok(listaRespostas);
        }


    }
}