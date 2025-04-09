namespace AppPromocoesGamer.API.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string NomeSobrenome { get; set; }
        public string UsuarioNome { get; set; }
        public string Email { get; set; }
        public string Senha { get; set; }

        public string? TokenRecuperacaoSenha { get; set; }
        public DateTime? TokenExpiraEm { get; set; }

    }
}
