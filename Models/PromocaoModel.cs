using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AppPromocoesGamer.API.Models
{
    public class Promocao
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Usuario")]
        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; }

        [ForeignKey("Empresa")]
        public int? EmpresaId { get; set; } // Pode ser null
        public Empresa? Empresa { get; set; }

        public string UrlPromocao { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public decimal Preco { get; set; }
        public string? Descricao { get; set; }
        public string SiteVendedor { get; set; } = string.Empty;
        public TimeSpan TempoPostado { get; set; }
        public string? Cupom { get; set; }
        public string ImagemUrl { get; set; } = string.Empty;
        public bool Aprovado { get; set; } = false;

    }
}
