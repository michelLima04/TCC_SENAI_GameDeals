namespace AppPromocoesGamer.API.DTOs
{
    public class PromocaoUpdateDTO
    {
        public int UsuarioId { get; set; }
        public bool IsAdmin { get; set; }

        public string Titulo { get; set; } = string.Empty;
        public decimal Preco { get; set; }
        public string? Descricao { get; set; }
        public string? Cupom { get; set; }
    }
}
