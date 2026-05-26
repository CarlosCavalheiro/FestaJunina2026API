using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiFestaJulina.DTO;
using ApiFestaJulina.Models;
using ApiFestaJulina.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiFestaJulina.Controllers
{
    
    [ApiController]
    [Route("api/[controller]")]
    public class EventosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EventosController(AppDbContext context)
        {
            _context = context;
        }

        //lista dos eventos criados
        [HttpGet]
        public async Task<ActionResult<List<Eventos>>> Get()
        {
            var listaEventos = await _context.Eventos.ToListAsync();
            return Ok(listaEventos);
        }

        //criar evento
        [HttpPost]
        public async Task<ActionResult<Eventos>> CriarEvento(EventosDTO dto)
        {
            Eventos evento = new Eventos
            {
                NomeEvento = dto.NomeEvento,
                Data = dto.Data,
                Ativo = dto.Ativo,
                Local = dto.Local,
                Descricao = dto.Descricao,
                Qtde_Ingressos = dto.Qtde_Ingressos,
                Qtde_Lotes = dto.Qtde_Lotes
            };

            await _context.Eventos.AddAsync(evento);
            await _context.SaveChangesAsync();

            return Ok(evento);
        }

        //editar evento
        [HttpPut("EditarEvento")]
        public async Task<ActionResult<Eventos>> EditarEvento(int id, EventosDTO eventoEditar)
        {
            var evento = await _context.Eventos.FirstOrDefaultAsync(e => e.IdEvento == id);

            if (evento == null)
            {
                return NotFound($"Não foi encontrado um evento com o id {id}.");
            }

            evento.NomeEvento = eventoEditar.NomeEvento;
            evento.Data = eventoEditar.Data;
            evento.Ativo = eventoEditar.Ativo;
            evento.Local = eventoEditar.Local;
            evento.Descricao = eventoEditar.Descricao;
            evento.Qtde_Ingressos = eventoEditar.Qtde_Ingressos;
            evento.Qtde_Lotes = eventoEditar.Qtde_Lotes;

            _context.Eventos.Update(evento);
            await _context.SaveChangesAsync();

            return Ok($"Evento com o id {id} foi editado com sucesso");
        }

        //excluir evento 
        // [HttpDelete("ExcluirEvento")]
        // public async Task<ActionResult> ExcluirEvento( int id)
        // {
        //     var evento = await _context.Eventos.FirstOrDefaultAsync(e => e.IdEvento == id);

        //     if (evento == null)
        //     {
        //         return NotFound($"Não foi encontrado um evento com o id {id}.");
        //     }

        //     _context.Eventos.Remove(evento);
        //     await _context.SaveChangesAsync();

        //     return Ok($"O evento com o id {id} foi excluído com sucesso.");
        // }

        //desativar evento
        [HttpPut("DesativarEvento")]
        public async Task<IActionResult> DesativarEvento(int id, AlterarStatusEventoDTO dto)
        {
            var evento = await _context.Eventos.FirstOrDefaultAsync(e => e.IdEvento == id);
            if (evento == null)
            {
                return BadRequest("Esse evento não existe");
            }

            evento.Ativo = dto.Ativo;

            await _context.SaveChangesAsync();

            return Ok(evento.Ativo);
        }
    }

}