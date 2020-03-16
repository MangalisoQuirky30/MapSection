using MapAppVid.Model;
using MapAppVid.Services;
using MvvmHelpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Distance = MapAppVid.Model.Distance;
using Polyline = Xamarin.Forms.Maps.Polyline;

namespace MapAppVid.ViewModel
{
    public class MainPageViewModel : BaseViewModel
    {
        private string _origin;
        public string Origin
        {
            get { return _origin; }
            set { _origin = value; OnPropertyChanged(); }
        }
        private string _destination;
        public string Destination
        {
            get { return _destination; }
            set { _destination = value; OnPropertyChanged(); }
        }
        private Duration _routeDuration;
        public Duration RouteDuration
        {
            get { return _routeDuration; }
            set { _routeDuration = value; OnPropertyChanged(); }
        }
        private Distance _routeDistance;
        public Distance RouteDistance
        {
            get { return _routeDistance; }
            set { _routeDistance = value; OnPropertyChanged(); }
        }



        public static Map myMap;
        public Command ShowRouteCommand;
        private RouteService rService;
        private DirectionsResponse dr;



        public MainPageViewModel()
        {
            myMap = new Map();
            rService = new RouteService();
            dr = new DirectionsResponse();
           ShowRouteCommand = new Command(async () => await AddPolylineAsync());
        }

        public async Task DisplayAlert(string title, string message, string cancel)
        {
            await Application.Current.MainPage.DisplayAlert(title, message, cancel);
        }
        public async Task DisplayAlert(string title, string message, string accept, string cancel)
        {
            await Application.Current.MainPage.DisplayAlert(title, message, accept, cancel);
        }


        public async Task AddPolylineAsync()
        {
            if (!IsBusy)
            {
                try
                {
                    IsBusy = true;
                    RouteDuration = null;
                    RouteDistance = null;
                    var current = Xamarin.Essentials.Connectivity.NetworkAccess;

                    if(current != Xamarin.Essentials.NetworkAccess.Internet)
                    {
                        // connection is not available
                        await DisplayAlert("Error", "You must be connected to the internet", "OK");
                        return;
                    }
                    if (Origin == null || Destination == null)
                    {
                        // no start or end point
                        await DisplayAlert("Error", "Origin and Destination must not be empty", "OK");
                        return;
                    }

                    myMap.MapElements.Clear();
                    myMap.Pins.Clear();
                    List<Route> routes = new List<Route>();
                    List<Leg> legs = new List<Leg>();
                    List<Step> steps = new List<Step>();
                    //List<Intersection> steps = new List<Step>();
                    List<Position> locations = new List<Position>();


                    dr = await rService.GetDirectionsResponseAsync(Origin, Destination);

                    if(dr == null)
                    {
                        await DisplayAlert("Error", "Route could not be found", "OK");
                        return;
                    }

                    if (dr != null)
                    {
                        routes = dr.Routes;

                        RouteDuration = routes[2].legs[0].duration;
                        RouteDistance = routes[2].legs[0].distance;

                        //routes[0].legs[0].

                        foreach (var route in routes)
                        {
                            legs = route.legs;
                        }

                        foreach (var leg in legs)
                        {
                            steps = leg.steps;
                        }
                        string encodedPoly = routes[0].overview_polyline.points;

                        List<Position> lines = DecodePolyline(encodedPoly);
                        Polyline poli = new Polyline()
                        {
                            StrokeColor = Color.Blue,
                            StrokeWidth = 10
                        };

                        for(int x = 0; x < lines.Count - 1; x++)
                        {
                            poli.Geopath.Add(lines[x]);
                        }

                        myMap.MapElements.Add(poli);

                        /*   var polylineOptions = new PolylineOptions()
                                        .InvokeColor(Android.Graphics.Color.Blue)
                                        .InvokeWidth(4);   */

                        ArrayList routePosList = new ArrayList();
                        foreach (var item in lines)
                        {
                            routePosList.Add(item);
                        }

                        // i have the polyline encoded, i have decoded it.. but cant set the geopath. what now?
                        
                        

                    }
                    }
                catch
                {

                }
            }
        }






        private List<Position> DecodePolyline(string encodedPoints)
        {
            if (string.IsNullOrWhiteSpace(encodedPoints))
            {
                return null;
            }

            int index = 0;
            var polylineChars = encodedPoints.ToCharArray();
            List<Position> poly = new List<Position>();
            int currentLat = 0;
            int currentLng = 0;
            int next5Bits;

            while (index < polylineChars.Length)
            {
                // calculate next latitude
                int sum = 0;
                int shifter = 0;

                do
                {
                    next5Bits = polylineChars[index++] - 63;
                    sum |= (next5Bits & 31) << shifter;
                    shifter += 5;
                }
                while (next5Bits >= 32 && index < polylineChars.Length);

                if (index >= polylineChars.Length)
                {
                    break;
                }

                currentLat += (sum & 1) == 1 ? ~(sum >> 1) : (sum >> 1);

                // calculate next longitude
                sum = 0;
                shifter = 0;

                do
                {
                    next5Bits = polylineChars[index++] - 63;
                    sum |= (next5Bits & 31) << shifter;
                    shifter += 5;
                }
                while (next5Bits >= 32 && index < polylineChars.Length);

                if (index >= polylineChars.Length && next5Bits >= 32)
                {
                    break;
                }

                currentLng += (sum & 1) == 1 ? ~(sum >> 1) : (sum >> 1);

                var mLatLng = new Position(Convert.ToDouble(currentLat) / 100000.0, Convert.ToDouble(currentLng) / 100000.0);
                poly.Add(mLatLng);
            }

            return poly;
        }
    }
}
