namespace AppPromocoesGamer.API.DTOs
{
    public class ComentarioCreateDTO
    {
        public int UsuarioId { get; set; }
        public int PromocaoId { get; set; }
        public string ComentarioTexto { get; set; } = string.Empty;
    }
}
