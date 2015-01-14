using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WeatherService.Data
{
    [Table("Setting")]
    public class Setting
    {
        [Key]
        [StringLength(500)]
        public string Name { get; set; }

        [Required]
        [StringLength(3500)]
        public string Value { get; set; }
    }
}
