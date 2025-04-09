using Microsoft.AspNetCore.Mvc;
using AppPromocoesGamer.API.Data;
using AppPromocoesGamer.API.Models;
using AppPromocoesGamer.API.DTOs;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;

namespace AppPromocoesGamer.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PromocaoController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PromocaoController(AppDbContext context)
        {
            _context = context;
        }

        private async Task<(string titulo, string imagemUrl, decimal preco, string siteVendedor)> ExtrairDadosDaUrl(string url)
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36");

            var html = await httpClient.GetStringAsync(url);
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var titulo = doc.DocumentNode.SelectSingleNode("//title")?.InnerText.Trim() ?? "Sem título";
            var imagemUrl = doc.DocumentNode.SelectSingleNode("//meta[@property='og:image']")?.GetAttributeValue("content", "") ?? "";
            var precoStr = doc.DocumentNode.SelectSingleNode("//meta[@property='product:price:amount']")?.GetAttributeValue("content", "") ?? "0";

            decimal.TryParse(precoStr.Replace(",", "."), out decimal preco);

            Uri uri = new Uri(url);
            string siteVendedor = uri.Host.Replace("www.", "");

            return (titulo, imagemUrl, preco, siteVendedor);
        }

        [HttpPost("Cadastrar promoção")]
        public async Task<IActionResult> PostPromocao([FromBody] PromocaoCreateDTO dto)
        {
            try
            {
                var (titulo, imagemUrl, preco, siteVendedor) = await ExtrairDadosDaUrl(dto.UrlPromocao);

                var promocao = new Promocao
                {
                    UsuarioId = dto.UsuarioId,
                    EmpresaId = dto.EmpresaId,
                    UrlPromocao = dto.UrlPromocao,
                    Titulo = titulo,
                    Preco = preco,
                    Descricao = dto.Descricao,
                    SiteVendedor = siteVendedor,
                    TempoPostado = DateTime.Now.TimeOfDay,
                    Cupom = dto.Cupom,
                    ImagemUrl = imagemUrl,
                    Aprovado = false // Só admin pode aprovar
                };

                _context.Promocoes.Add(promocao);
                await _context.SaveChangesAsync();

                return Ok(promocao);
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao cadastrar promoção: {ex.Message}");
            }
        }

        [HttpPut("Atualizar promoção/{id}")]
        public async Task<IActionResult> AtualizarPromocao(int id, [FromBody] PromocaoUpdateDTO dto)
        {
            var promocao = await _context.Promocoes.FindAsync(id);
            if (promocao == null)
                return NotFound("Promoção não encontrada.");

            if (dto.UsuarioId != promocao.UsuarioId)
                return Forbid("Você não tem permissão para editar esta promoção.");

            promocao.Titulo = dto.Titulo;
            promocao.Preco = dto.Preco;
            promocao.Descricao = dto.Descricao;
            promocao.Cupom = dto.Cupom;

            await _context.SaveChangesAsync();
            return Ok(promocao);
        }

        [HttpDelete("Deletar promoção/{id}")]
        public async Task<IActionResult> ExcluirPromocao(int id, [FromBody] PromocaoDeleteDTO dto)
        {
            var promocao = await _context.Promocoes.FindAsync(id);
            if (promocao == null)
                return NotFound("Promoção não encontrada.");

            if (!dto.IsAdmin && dto.UsuarioId != promocao.UsuarioId)
                return Forbid("Você não tem permissão para excluir esta promoção.");

            _context.Promocoes.Remove(promocao);
            await _context.SaveChangesAsync();

            return Ok("Promoção excluída com sucesso.");
        }

        [HttpGet]
        public IActionResult GetPromocoes()
        {
            var promocoes = _context.Promocoes
                .Include(p => p.Usuario)
                .Include(p => p.Empresa)
                .Where(p => p.Aprovado == true) // apenas promoções aprovadas
                .ToList();

            return Ok(promocoes);
        }

        [HttpGet("Listar promoções/{id}")]
        public async Task<IActionResult> GetPromocao(int id)
        {
            var promocao = await _context.Promocoes
                .Include(p => p.Usuario)
                .Include(p => p.Empresa)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (promocao == null)
                return NotFound();

            return Ok(promocao);
        }

        [HttpPut("Aprovar promoção/{id}")]
        public async Task<IActionResult> AprovarPromocao(int id, [FromBody] AprovarPromocaoDTO dto)
        {
            var promocao = await _context.Promocoes.FindAsync(id);
            if (promocao == null)
                return NotFound("Promoção não encontrada.");

            if (!dto.IsAdmin)
                return Forbid("Apenas administradores podem aprovar promoções.");

            promocao.Aprovado = true;
            await _context.SaveChangesAsync();

            return Ok("Promoção aprovada com sucesso.");
        }

        [HttpGet("Promoções pendentes")]
        public async Task<IActionResult> GetPromocoesPendentes([FromQuery] bool isAdmin)
        {
            if (!isAdmin)
                return Forbid("Apenas administradores podem visualizar promoções pendentes.");

            var pendentes = await _context.Promocoes
                .Include(p => p.Usuario)
                .Include(p => p.Empresa)
                .Where(p => p.Aprovado == false)
                .ToListAsync();

            return Ok(pendentes);
        }
    }
}
