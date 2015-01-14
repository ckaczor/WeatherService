using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WeatherService.Data
{
    [Table("Device")]
    public class Device
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Address { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        public int ReadInterval { get; set; }
    }
}
