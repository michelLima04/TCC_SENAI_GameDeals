namespace AppPromocoesGamer.API.DTOs
{
    public class PromocaoCreateDTO
    {
        public int UsuarioId { get; set; }
        public int? EmpresaId { get; set; } // Pode ser null
        public string UrlPromocao { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public string? Cupom { get; set; }
    }
}
