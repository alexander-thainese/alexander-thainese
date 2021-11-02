using System.ComponentModel.DataAnnotations;

namespace CMT.Models
{
    public class TokenRequest
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
        public string Scope { get; set; }
        public string Role { get; set; }
    }
}
