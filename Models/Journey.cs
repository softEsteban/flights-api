using System.ComponentModel.DataAnnotations;

namespace FlightApi.Models
{
    public class Journey
    {
        [Required]
        public List<Flight> Flights { get; set; }

        [Required]
        [StringLength(3, MinimumLength = 3)]
        public string Origin { get; set; }

        [Required]
        [StringLength(3, MinimumLength = 3)]
        public string Destination { get; set; }

        [Required]
        public double Price { get; set; }
    }
}
