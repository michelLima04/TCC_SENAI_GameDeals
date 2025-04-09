namespace AppPromocoesGamer.API.DTOs
{
    public class ComentarioUpdateDTO
    {
        public int UsuarioId { get; set; }
        public bool IsAdmin { get; set; }
        public string ComentarioTexto { get; set; } = string.Empty;
    }
}
