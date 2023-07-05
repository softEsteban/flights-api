using System.ComponentModel.DataAnnotations;

namespace FlightApi.Models
{
    public class Transport
    {
        [Required]
        public string FlightCarrier { get; set; }

        [Required]
        public string FlightNumber { get; set; }
    }
}
