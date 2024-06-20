using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

namespace HighlightsVault.Models
{
    public class Highlight
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        public int ID { get; set; }
        public string SteamID { get; set; }

        [MaxLength(100)]
        public string HighlightPersonName { get; set; }
        [Required(ErrorMessage = "User description is required")]
        public string UserDescription { get; set; }
        public string ProfileUrl { get; set; }
        public string ProfilePictureUrl { get; set; }
        public byte[] ProfilePicture { get; set; } // property to store local image path
        [Required(ErrorMessage = "Highlight Date is required")]
        public DateTime HighlightDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? GroupId { get; set; }        
        public HighlightsVaultGroup Group { get; set; }
        public byte[]? Clip { get; set; } // property to store local video path
    }
}
