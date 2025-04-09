public class PromocaoResponseDTO
{
    public int Id { get; set; }
    public string Titulo { get; set; }
    public decimal Preco { get; set; }
    public string Descricao { get; set; }
    public string SiteVendedor { get; set; }
    public string? Cupom { get; set; }
    public string ImagemUrl { get; set; }
    public string UrlPromocao { get; set; }
}
