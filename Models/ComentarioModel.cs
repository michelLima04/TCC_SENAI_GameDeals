using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppPromocoesGamer.API.Models
{
    public class Comentario
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Usuario")]
        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; }

        [ForeignKey("Promocao")]
        public int PromocaoId { get; set; }
        public Promocao Promocao { get; set; }

        public string ComentarioTexto { get; set; } = string.Empty;
        public int Likes { get; set; } = 0;
        public int Deslikes { get; set; } = 0;
    }
}
