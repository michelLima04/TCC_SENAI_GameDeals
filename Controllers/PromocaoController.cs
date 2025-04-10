using Microsoft.AspNetCore.Mvc;
using AppPromocoesGamer.API.Data;
using AppPromocoesGamer.API.Models;
using AppPromocoesGamer.API.DTOs;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

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

        private async Task<(string titulo, string imagemUrl, decimal preco, string siteVendedor, List<string> falhas)> ExtrairDadosDaUrl(string url)
        {
            var falhas = new List<string>();
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36");

            var html = await httpClient.GetStringAsync(url);
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            // Extração do título
            var tituloNode = doc.DocumentNode.SelectSingleNode("//title");
            string titulo = tituloNode?.InnerText.Trim();
            if (string.IsNullOrEmpty(titulo))
            {
                titulo = "Sem título";
                falhas.Add("Título não encontrado na página.");
            }

            // Extração da URL da imagem
            var imagemNode = doc.DocumentNode.SelectSingleNode("//meta[@property='og:image']");
            string imagemUrl = imagemNode?.GetAttributeValue("content", "");
            if (string.IsNullOrEmpty(imagemUrl))
            {
                falhas.Add("URL da imagem (og:image) não encontrada na página.");
            }

            // Extração do preço
            var precoNode = doc.DocumentNode.SelectSingleNode("//meta[@property='product:price:amount']");
            string precoStr = precoNode?.GetAttributeValue("content", "");
            if (!decimal.TryParse(precoStr.Replace(",", "."), out decimal preco))
            {
                preco = 0;
                falhas.Add("Preço (product:price:amount) não encontrado ou inválido na página.");
            }

            // Extração do site vendedor
            Uri uri = new Uri(url);
            string siteVendedor = uri.Host.Replace("www.", "");
            if (string.IsNullOrEmpty(siteVendedor))
            {
                falhas.Add("Não foi possível determinar o site vendedor a partir da URL.");
            }

            return (titulo, imagemUrl, preco, siteVendedor, falhas);
        }

        [HttpPost("Cadastrar promoção")]
        [Authorize] // Requer autenticação para obter o usuário logado
        public async Task<IActionResult> PostPromocao([FromBody] PromocaoCreateDTO dto)
        {
            try
            {
                // Validação inicial da URL
                if (string.IsNullOrWhiteSpace(dto.UrlPromocao))
                {
                    return BadRequest("A URL da promoção não pode estar vazia.");
                }

                if (!Uri.TryCreate(dto.UrlPromocao, UriKind.Absolute, out Uri uriResult) ||
                    (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
                {
                    return BadRequest("A URL fornecida não é válida. Use uma URL no formato http:// ou https://.");
                }

                // Obter o ID do usuário autenticado
                var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(usuarioIdClaim) || !int.TryParse(usuarioIdClaim, out int usuarioId))
                {
                    return Unauthorized("Usuário não autenticado ou ID inválido.");
                }

                // Extrair dados e verificar falhas
                var (titulo, imagemUrl, preco, siteVendedor, falhas) = await ExtrairDadosDaUrl(dto.UrlPromocao);

                var promocao = new Promocao
                {
                    UsuarioId = usuarioId,
                    EmpresaId = null, // Pode ser ajustado se houver lógica para inferir
                    UrlPromocao = dto.UrlPromocao,
                    Titulo = titulo,
                    Preco = preco,
                    Descricao = null, // Pode ser preenchido depois, se necessário
                    SiteVendedor = siteVendedor,
                    TempoPostado = DateTime.Now.TimeOfDay,
                    Cupom = null, // Pode ser preenchido depois, se necessário
                    ImagemUrl = imagemUrl,
                    Aprovado = false // Só admin pode aprovar
                };

                _context.Promocoes.Add(promocao);
                await _context.SaveChangesAsync();

                // Se houver falhas, retornar um objeto com a promoção e as falhas
                if (falhas.Any())
                {
                    return Ok(new
                    {
                        Promocao = promocao,
                        Mensagem = "Promoção cadastrada, mas alguns dados não foram extraídos.",
                        Falhas = falhas
                    });
                }

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
                .Where(p => p.Aprovado == true) // Apenas promoções aprovadas
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