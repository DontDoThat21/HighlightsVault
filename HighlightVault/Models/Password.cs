using System.ComponentModel.DataAnnotations;

namespace HighlightsVault.Models
{
    public class Password
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string PasswordValue { get; set; }
    }
}
