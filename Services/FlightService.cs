using System.Linq.Expressions;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using FlightApi.Models;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using QuickGraph;
using QuickGraph.Algorithms;
using QuickGraph.Algorithms.ShortestPath;

namespace FlightApi.Services
{
    public class Station
    {
        public string Name { get; set; }
        public List<Flight> OutgoingFlights { get; set; }
        public double Distance { get; set; } // Updated to double
    }

    public class FlightService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMemoryCache _cache;

        public FlightService(IHttpClientFactory httpClientFactory, IMemoryCache cache)
        {
            _httpClientFactory = httpClientFactory;
            _cache = cache;
        }

        /// <summary>
        /// Gets all one-way and return flights
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<Flight>> GetFlights()
        {
            var httpClient = _httpClientFactory.CreateClient();

            // Check if flights data exists in cache
            if (!_cache.TryGetValue("FlightsData", out List<Flight> flights))
            {
                // Flights data not found in cache, fetch from API
                var apiUrl = "https://recruiting-api.newshore.es/api/flights/2";
                var response = await httpClient.GetAsync(apiUrl);

                if (!response.IsSuccessStatusCode)
                {
                    // Handle API error
                    return null;
                }

                var responseData = await response.Content.ReadAsStringAsync();
                flights = JsonConvert.DeserializeObject<List<Flight>>(responseData);

                // Store flights data in cache with a sliding expiration of 1 hour
                var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(
                    TimeSpan.FromHours(1)
                );
                _cache.Set("FlightsData", flights, cacheEntryOptions);
            }

            return flights;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        public async Task<List<Flight>> SearchFlights(string origin, string destination)
        {
            var flights = await GetFlights();
            List<Flight> route = new List<Flight>();

            var stations = new Dictionary<string, Station>();
            foreach (var flight in flights)
            {
                if (!stations.ContainsKey(flight.DepartureStation))
                    stations[flight.DepartureStation] = new Station
                    {
                        Name = flight.DepartureStation,
                        OutgoingFlights = new List<Flight>()
                    };

                stations[flight.DepartureStation].OutgoingFlights.Add(flight);
            }

            var shortestPath = FindShortestPath(origin, destination, stations);

            route.AddRange(shortestPath);

            return route;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="startStation"></param>
        /// <param name="endStation"></param>
        /// <param name="stations"></param>
        /// <returns></returns>
        private List<Flight> FindShortestPath(
            string startStation,
            string endStation,
            Dictionary<string, Station> stations
        )
        {
            var queue = new SortedSet<Station>(
                Comparer<Station>.Create((s1, s2) => s1.Distance.CompareTo(s2.Distance))
            );
            var distances = new Dictionary<string, double>();
            var previousStations = new Dictionary<string, Station>();

            foreach (var station in stations.Values)
            {
                distances[station.Name] = double.PositiveInfinity;
                station.Distance = double.PositiveInfinity;
            }

            distances[startStation] = 0;
            stations[startStation].Distance = 0;
            queue.Add(stations[startStation]);

            while (queue.Count > 0)
            {
                var currentStation = queue.First();
                queue.Remove(currentStation);

                if (currentStation.Name == endStation)
                    break;

                foreach (var flight in currentStation.OutgoingFlights)
                {
                    var neighborStation = stations[flight.ArrivalStation];
                    var altDistance = distances[currentStation.Name] + flight.Price;

                    if (altDistance < distances[neighborStation.Name])
                    {
                        distances[neighborStation.Name] = altDistance;
                        neighborStation.Distance = altDistance;
                        previousStations[neighborStation.Name] = currentStation;
                        queue.Add(neighborStation);
                    }
                }
            }

            var shortestPath = new List<Flight>();
            var stationName = endStation;

            while (previousStations.ContainsKey(stationName))
            {
                var previousStation = previousStations[stationName];
                var flight = previousStation.OutgoingFlights.FirstOrDefault(
                    f => f.ArrivalStation == stationName
                );
                shortestPath.Insert(0, flight);
                stationName = previousStation.Name;
            }

            return shortestPath;
        }
    }
}
