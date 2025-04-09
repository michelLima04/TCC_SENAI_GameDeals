using Microsoft.AspNetCore.Mvc;
using AppPromocoesGamer.API.Data;
using AppPromocoesGamer.API.Models;
using AppPromocoesGamer.API.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AppPromocoesGamer.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ComentarioController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ComentarioController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("Criar comentário")]
        public async Task<IActionResult> PostComentario([FromBody] ComentarioCreateDTO dto)
        {
            var comentario = new Comentario
            {
                UsuarioId = dto.UsuarioId,
                PromocaoId = dto.PromocaoId,
                ComentarioTexto = dto.ComentarioTexto
            };

            _context.Comentarios.Add(comentario);
            await _context.SaveChangesAsync();

            return Ok(comentario);
        }

        [HttpPut("Atualizar comentário/{id}")]

        public async Task<IActionResult> AtualizarComentario(int id, [FromBody] ComentarioUpdateDTO dto)
        {
            var comentario = await _context.Comentarios.FindAsync(id);
            if (comentario == null)
                return NotFound("Comentário não encontrado.");

            if (dto.UsuarioId != comentario.UsuarioId)
                return Forbid("Você só pode editar seu próprio comentário.");

            comentario.ComentarioTexto = dto.ComentarioTexto;
            await _context.SaveChangesAsync();

            return Ok(comentario);
        }

        [HttpDelete("Excluir comentário/{id}")]
        public async Task<IActionResult> DeletarComentario(int id, [FromBody] ComentarioDeleteDTO dto)
        {
            var comentario = await _context.Comentarios.FindAsync(id);
            if (comentario == null)
                return NotFound("Comentário não encontrado.");

            if (!dto.IsAdmin && dto.UsuarioId != comentario.UsuarioId)
                return Forbid("Você não tem permissão para excluir este comentário.");

            _context.Comentarios.Remove(comentario);
            await _context.SaveChangesAsync();

            return Ok("Comentário excluído com sucesso.");
        }

        [HttpGet("Listar comentários")]
        public async Task<IActionResult> GetComentarios()
        {
            var comentarios = await _context.Comentarios
                .Include(c => c.Usuario)
                .Include(c => c.Promocao)
                .ToListAsync();

            return Ok(comentarios);
        }

        [HttpGet("Buscar comentário por ID/{id}")]
        public async Task<IActionResult> GetComentario(int id)
        {
            var comentario = await _context.Comentarios
                .Include(c => c.Usuario)
                .Include(c => c.Promocao)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (comentario == null)
                return NotFound();

            return Ok(comentario);
        }
    }
}
