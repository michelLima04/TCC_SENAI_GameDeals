using System.ComponentModel.DataAnnotations;
using AppPromocoesGamer.API.Models;

namespace AppPromocoesGamer.API.DTOs
{
    public class UserLoginDTO
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Senha { get; set; }
    }
}
