using AppPromocoesGamer.API.Data;
using AppPromocoesGamer.API.DTOs;
using AppPromocoesGamer.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;


namespace AppPromocoesGamer.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("Registro usuário")]
        public async Task<ActionResult> Register(UserRegisterDTO dto)
        {
            if (await _context.Usuarios.AnyAsync(u => u.Email == dto.Email))
            {
                return BadRequest("Este e-mail já está em uso.");
            }

            var usuario = new Usuario
            {
                NomeSobrenome = dto.NomeSobrenome,
                UsuarioNome = dto.UsuarioNome,
                Email = dto.Email,
                Senha = HashSenha(dto.Senha)
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return Ok("Usuário registrado com sucesso.");
        }

        [HttpPost("Login usuário")]
        public async Task<ActionResult> Login(UserLoginDTO dto)
        {
            var senhaHash = HashSenha(dto.Senha);

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == dto.Email && u.Senha == senhaHash);

            if (usuario == null)
                return Unauthorized("E-mail ou senha inválidos.");

            bool ehAdmin = usuario.Email == "adm@gmail.com";

            return Ok(new
            {
                mensagem = ehAdmin ? "Bem-vindo, ADMIN." : "Login realizado com sucesso!",
                usuario = new
                {
                    usuario.Id,
                    usuario.NomeSobrenome,
                    usuario.UsuarioNome,
                    usuario.Email,
                    ehAdmin
                }
            });
        }

        [HttpPut("Editar usuário")]
        public async Task<IActionResult> EditarUsuario(UserUpdateDTO dto)
        {
            var senhaHash = HashSenha(dto.SenhaAtual);
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == dto.Email && u.Senha == senhaHash);

            if (usuario == null)
                return Unauthorized("E-mail ou senha incorretos.");

            if (!string.IsNullOrEmpty(dto.NovoNomeSobrenome))
                usuario.NomeSobrenome = dto.NovoNomeSobrenome;

            if (!string.IsNullOrEmpty(dto.NovoUsuarioNome))
                usuario.UsuarioNome = dto.NovoUsuarioNome;

            if (!string.IsNullOrEmpty(dto.NovaSenha))
                usuario.Senha = HashSenha(dto.NovaSenha);

            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync();

            return Ok("Usuário atualizado com sucesso.");
        }

        [HttpDelete("Excluir usuário")]
        public async Task<IActionResult> ExcluirUsuario(UserDeleteDTO dto)
        {
            var senhaHash = HashSenha(dto.Senha);
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == dto.Email && u.Senha == senhaHash);

            if (usuario == null)
                return Unauthorized("E-mail ou senha incorretos.");

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();

            return Ok("Usuário excluído com sucesso.");
        }

        [HttpPost("Recuperar senha")]
        public async Task<IActionResult> RecuperarSenha(RecuperarSenhaDTO dto)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (usuario == null)
                return NotFound("Usuário não encontrado.");

            usuario.Senha = HashSenha(dto.NovaSenha);
            await _context.SaveChangesAsync();

            return Ok("Senha atualizada com sucesso.");
        }
        private string HashSenha(string senha)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(senha);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
    }
}
