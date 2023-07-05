using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FlightApi.Models
{
    public class Station
    {
        public string Name { get; set; }
        public List<Flight> OutgoingFlights { get; set; }
    }
}
