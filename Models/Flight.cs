using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FlightApi.Models
{
    public class Flight
    {
        [JsonPropertyName("departureStation")]
        public string DepartureStation { get; set; }

        [JsonPropertyName("arrivalStation")]
        public string ArrivalStation { get; set; }

        [JsonPropertyName("flightCarrier")]
        public string FlightCarrier { get; set; }

        [JsonPropertyName("flightNumber")]
        public string FlightNumber { get; set; }

        [JsonPropertyName("price")]
        public double Price { get; set; }
    }
}
