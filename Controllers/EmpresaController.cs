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
    public class EmpresaController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EmpresaController(AppDbContext context)
        {
            _context = context;
        }

        // Scraper: extrair nome e logo do site
        private async Task<(string nome, string logoUrl)> ExtrairDadosDoSite(string siteUrl)
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36");

            var html = await httpClient.GetStringAsync(siteUrl);
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            // Extrair logo
            var logoNode = doc.DocumentNode.SelectSingleNode("//link[@rel='icon']") ??
                           doc.DocumentNode.SelectSingleNode("//link[@rel='shortcut icon']");
            string logoUrl = logoNode?.GetAttributeValue("href", "") ?? "";

            if (!string.IsNullOrEmpty(logoUrl) && !logoUrl.StartsWith("http"))
            {
                Uri baseUri = new Uri(siteUrl);
                logoUrl = new Uri(baseUri, logoUrl).ToString();
            }

            // Extrair nome do domínio
            Uri uri = new Uri(siteUrl);
            string nomeBase = uri.Host.Replace("www.", "").Split('.')[0];
            string nome = char.ToUpper(nomeBase[0]) + nomeBase.Substring(1).ToLower();

            return (nome, logoUrl);
        }

        // POST: api/Empresa
        [HttpPost("Cadastrar empresa")]
        public async Task<IActionResult> PostEmpresa([FromBody] EmpresaCreateDTO dto)
        {
            if (!dto.IsAdmin)
                return Unauthorized("Apenas administradores podem cadastrar empresas.");

            if (_context.Empresas.Any(e => e.SiteUrl == dto.SiteUrl))
                return Conflict("Esta empresa já está cadastrada.");

            try
            {
                var (nome, logoUrl) = await ExtrairDadosDoSite(dto.SiteUrl);

                var empresa = new Empresa
                {
                    Nome = nome,
                    SiteUrl = dto.SiteUrl,
                    LogoUrl = logoUrl
                };

                _context.Empresas.Add(empresa);
                await _context.SaveChangesAsync();

                return Ok(empresa);
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao acessar ou processar o site: {ex.Message}");
            }
        }

        // GET: api/Empresa
        [HttpGet]
        public IActionResult GetEmpresas()
        {
            var empresas = _context.Empresas.ToList();
            return Ok(empresas);
        }

        // GET: api/Empresa/{id}
        [HttpGet("{id}")]
        public IActionResult GetEmpresa(int id)
        {
            var empresa = _context.Empresas.Find(id);
            if (empresa == null)
                return NotFound();

            return Ok(empresa);
        }

        // PUT: api/Empresa/{id}
        [HttpPut("Atualizar empresa/{id}")]
        public async Task<IActionResult> AtualizarEmpresa(int id, [FromBody] EmpresaUpdateDTO dto)
        {
            if (!dto.IsAdmin)
                return Unauthorized("Apenas administradores podem atualizar empresas.");

            var empresa = await _context.Empresas.FindAsync(id);
            if (empresa == null)
                return NotFound("Empresa não encontrada.");

            try
            {
                var (novoNome, novoLogoUrl) = await ExtrairDadosDoSite(dto.SiteUrl);

                empresa.SiteUrl = dto.SiteUrl;
                empresa.Nome = novoNome;
                empresa.LogoUrl = novoLogoUrl;

                await _context.SaveChangesAsync();

                return Ok(empresa);
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao atualizar empresa: {ex.Message}");
            }
        }

        // DELETE: api/Empresa
        [HttpDelete("Excluir empresa")]
        public async Task<IActionResult> DeleteEmpresa([FromBody] EmpresaDeleteDTO dto)
        {
            if (!dto.IsAdmin)
                return Unauthorized("Apenas administradores podem excluir empresas.");

            if (string.IsNullOrWhiteSpace(dto.Nome) && string.IsNullOrWhiteSpace(dto.SiteUrl))
                return BadRequest("Informe o nome ou a URL da empresa para excluir.");

            var empresa = await _context.Empresas.FirstOrDefaultAsync(e =>
                (!string.IsNullOrEmpty(dto.Nome) && e.Nome.ToLower() == dto.Nome.ToLower()) ||
                (!string.IsNullOrEmpty(dto.SiteUrl) && e.SiteUrl.ToLower() == dto.SiteUrl.ToLower())
            );

            if (empresa == null)
                return NotFound("Empresa não encontrada.");

            _context.Empresas.Remove(empresa);
            await _context.SaveChangesAsync();

            return Ok("Empresa excluída com sucesso.");
        }
    }
}
