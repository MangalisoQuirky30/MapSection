using MapAppVid.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace MapAppVid.Services
{
    public class RouteService
    {
        private readonly string baseRouteUrl = "https://maps.googleapis.com/maps/api/directions/json?";
        
        private HttpClient _httpClient;

        public RouteService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<DirectionsResponse> GetDirectionsResponseAsync(string origin, string destination)
        {
            try
            {
                var originLocations = await Geocoding.GetLocationsAsync(origin);
                var originLocation = originLocations?.FirstOrDefault();
                var destinationLocations = await Geocoding.GetLocationsAsync(destination);
                var destinationLocation = destinationLocations?.FirstOrDefault();

                if(destinationLocation == null || originLocation == null)
                {
                    return null;
                }
                if (destinationLocation != null && originLocation != null)
                {
                    string url = string.Format(baseRouteUrl) + $"{originLocation.Longitude},{originLocation.Latitude}&" +
                        $"{destinationLocation.Longitude},{destinationLocation.Latitude}&key=AIzaSyC0Vykx3OeOVmhLXv5xxALSePYn86cRRyI";
                    var response = await _httpClient.GetAsync(url);
                    var json = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        var result = JsonConvert.DeserializeObject<DirectionsResponse>(json);

                        if(result.GeocodedWaypoints[0].GeocoderStatus == "OK")
                        {
                            return result;
                        }
                        return null;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch
            {

            }
            return null;
        }
    }
}
