using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using BingMapsRESTToolkit;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.IO;
using System.Text;
using System.Net.Http;
using System.Diagnostics;
using HousingSite.Data.Helper;

namespace HousingSite.Data
{
    public class MapService
    {
        public string sessionKey = "";
        //public void TestREST(string query)
        //{
        //    Uri geocodeRequest = new Uri(string.Format("http://dev.virtualearth.net/REST/v1/Locations?q={0}&key={1}", query, sessionKey));
        //    GetResponse(geocodeRequest, (x) =>
        //    {
        //        Console.WriteLine(x.ResourceSets[0].Resources.Length + " result(s) found.");
        //        for(int r=0; r<x.ResourceSets[0].Resources.Length; r++)
        //        {
        //            BingMapsRESTToolkit.Location location = (BingMapsRESTToolkit.Location)x.ResourceSets[0].Resources[r];
        //            Console.WriteLine("Result " + r + " = " + location.Name);
        //            Console.WriteLine("Result " + r + " = " + location.Point.Coordinates[0]);
        //            Console.WriteLine("Result " + r + " = " + location.Point.Coordinates[1]);
        //        }
        //    });
        //}

        private void GetResponse(Uri uri, Action<Response> callback)
        {
            WebClient wc = new WebClient();
            wc.OpenReadCompleted += (o, a) =>
            {
                if (callback != null)
                {
                    DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(Response));
                    callback(ser.ReadObject(a.Result) as Response);
                }
            };
            wc.OpenReadAsync(uri);
        }

        private Response GetResponseSync(Uri uri)
        {
            WebClient wc = new WebClient();
            //Stream data = wc.OpenRead(uri);
            using (Stream data = wc.OpenRead(uri))
            {
                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(Response));
                return ser.ReadObject(data) as Response;
            }
        }

        //public async Task<double[]> GetCoordinatesAsync(string query)
        //{
        //   return await Task.FromResult(GetCoordinates(query));
        //}

        //public double[] GetCoordinates(string query)
        //{
        //    Console.WriteLine("Sessionkey: " + sessionKey);
        //    string url = string.Format("http://dev.virtualearth.net/REST/v1/Locations?q={0}&key={1}", query, sessionKey);
        //    Console.WriteLine("Url: " + url);
        //    Uri geocodeRequest = new Uri(url);

        //    Response x = GetResponseSync(geocodeRequest);
            
        //    //Console.WriteLine(x.ResourceSets[0].Resources.Length + " result(s) found.");
        //    for (int r = 0; r < x.ResourceSets[0].Resources.Length; r++)
        //    {
        //        BingMapsRESTToolkit.Location location = (BingMapsRESTToolkit.Location)x.ResourceSets[0].Resources[r];
        //        //Console.WriteLine("Result " + r + " = " + location.Name);
        //        //Console.WriteLine("Result " + r + " = " + location.Point.Coordinates[0]);
        //        //Console.WriteLine("Result " + r + " = " + location.Point.Coordinates[1]);
        //    }

        //    if (x.ResourceSets[0].Resources.Length > 0)
        //    {
        //        return ((BingMapsRESTToolkit.Location)x.ResourceSets[0].Resources[0]).Point.Coordinates;
        //    }
        //    else return null;
        //}

        public async Task<double> CalculateDistance(double[] p1, double[] p2)
        {
            DistanceMatrixRequest dmr = new DistanceMatrixRequest()
            {
                Origins = new List<SimpleWaypoint>() { new SimpleWaypoint { Coordinate = new Coordinate(p1[0], p1[1]) } },
                Destinations = new List<SimpleWaypoint>() { new SimpleWaypoint { Coordinate = new Coordinate(p2[0], p2[1]) } },
                DistanceUnits = DistanceUnitType.Kilometers
            };

            DistanceMatrix d = await dmr.GetEuclideanDistanceMatrix();
            return d.GetDistance(0, 0);
        }

        //public async Task<int[]> GetRouteAsync(string addr1, string addr2)
        //{
        //    return await Task.FromResult(GetRoute(addr1, addr2));
        //}

        //public int[] GetRoute(string addr1, string addr2)
        //{

        //    int[] resultArray = new int[3];

        //    addr1.Replace(" ", "%20");
        //    addr2.Replace(" ", "%20");
        //    //Uri geocodeRequest = new Uri(string.Format("http://dev.virtualearth.net/REST/v1/Routes/{travelMode}?wayPoint.1={wayPpoint1}&viaWaypoint.2={viaWaypoint2}&waypoint.3={waypoint3}&wayPoint.n={waypointN}&heading={heading}&optimize={optimize}&avoid={avoid}&distanceBeforeFirstTurn={distanceBeforeFirstTurn}&routeAttributes={routeAttributes}&timeType={timeType}&dateTime={dateTime}&maxSolutions={maxSolutions}&tolerances={tolerances}&distanceUnit={distanceUnit}&key={BingMapsKey}", query));
        //    Uri geocodeRequest = new Uri(string.Format("http://dev.virtualearth.net/REST/v1/Routes/{0}?wayPoint.1={1}&waypoint.2={2}&key=AtBNs13lJOHTAKXWNi87QaWxCYEctQcUwmyYhm1-2LFfMVq2EsO47Cqa0O6OD7cx", "Driving", addr1, addr2));

        //    Response x = GetResponseSync(geocodeRequest);

        //    //Console.WriteLine(x.ResourceSets[0].Resources.Length + " result(s) found.");
        //    for (int r = 0; r < x.ResourceSets[0].Resources.Length; r++)
        //    {
        //        BingMapsRESTToolkit.Route route = (BingMapsRESTToolkit.Route)x.ResourceSets[0].Resources[r];
        //        //Console.WriteLine("Result " + r + " = " + location.Name);
        //        //Console.WriteLine("Result " + r + " = " + location.Point.Coordinates[0]);
        //        //Console.WriteLine("Result " + r + " = " + location.Point.Coordinates[1]);
        //    }            

        //    if (x.ResourceSets[0].Resources.Length > 0)
        //    {
        //        Console.WriteLine("Driving duration: " + ((Route)x.ResourceSets[0].Resources[0]).TravelDuration);
        //        Console.WriteLine("Driving duration with traffic: " + ((Route)x.ResourceSets[0].Resources[0]).TravelDurationTraffic);
        //        Console.WriteLine("Time unit type: " + ((Route)x.ResourceSets[0].Resources[0]).TimeUnitType);

        //        Console.WriteLine("Driving distance: " + ((Route)x.ResourceSets[0].Resources[0]).TravelDistance);
        //        Console.WriteLine("Distance unit type: " + ((Route)x.ResourceSets[0].Resources[0]).DistanceUnitType);
        //        Console.WriteLine("Distance unit: " + ((Route)x.ResourceSets[0].Resources[0]).DistanceUnit);

        //        Console.WriteLine("Traffic congestion: " + ((Route)x.ResourceSets[0].Resources[0]).TrafficCongestion);

        //        resultArray[0] = (int)Math.Round(((Route)x.ResourceSets[0].Resources[0]).TravelDuration);
        //        resultArray[1] = (int)Math.Round(((Route)x.ResourceSets[0].Resources[0]).TravelDurationTraffic);
        //        resultArray[2] = (int)Math.Round(((Route)x.ResourceSets[0].Resources[0]).TravelDistance);

        //        return resultArray;// ((BingMapsRESTToolkit.Route)x.ResourceSets[0].Resources[0]).TravelDuration;
        //    }
        //    else return null;
        //}

        #region Get route between two points
        public async Task<int[]> GetRouteAsyncWithMode(string addr1, string addr2, string mode)
        {
            return await Task.FromResult(GetRouteWithMode(addr1, addr2, mode));
        }

        #region Handler
        public int[] GetRouteWithMode(string addr1, string addr2, string mode)
        {
            int[] resultArray = new int[3] { 0, 0, 0 };
            Console.WriteLine("addr1: \"" + addr1 + "\" -> \"" + Uri.EscapeDataString(addr1) + "\"");
            Console.WriteLine("addr2: \"" + addr2 + "\" -> \"" + Uri.EscapeDataString(addr2) + "\"");
            addr1.Replace(" ", "%20");
            addr2.Replace(" ", "%20");
            
            //Uri geocodeRequest = new Uri(string.Format("http://dev.virtualearth.net/REST/v1/Routes/{travelMode}?wayPoint.1={wayPpoint1}&viaWaypoint.2={viaWaypoint2}&waypoint.3={waypoint3}&wayPoint.n={waypointN}&heading={heading}&optimize={optimize}&avoid={avoid}&distanceBeforeFirstTurn={distanceBeforeFirstTurn}&routeAttributes={routeAttributes}&timeType={timeType}&dateTime={dateTime}&maxSolutions={maxSolutions}&tolerances={tolerances}&distanceUnit={distanceUnit}&key={BingMapsKey}", query));
            Uri geocodeRequest;// = new Uri(string.Format("http://dev.virtualearth.net/REST/v1/Routes/{0}?wayPoint.0={1}&waypoint.1={2}&key=AtBNs13lJOHTAKXWNi87QaWxCYEctQcUwmyYhm1-2LFfMVq2EsO47Cqa0O6OD7cx", mode, addr1, addr2));

            if(mode == "Transit")
            {
                string dateTime = "12:00:00";
                string timeType = "Arrival";
                geocodeRequest = new Uri(string.Format("http://dev.virtualearth.net/REST/v1/Routes/Transit?wayPoint.0={0}&waypoint.1={1}&timeType={2}&dateTime={3}&key={4}", addr1, addr2, timeType, dateTime, sessionKey));
            }
            else geocodeRequest = new Uri(string.Format("http://dev.virtualearth.net/REST/v1/Routes/{0}?wayPoint.0={1}&waypoint.1={2}&key={3}", mode, addr1, addr2, sessionKey));

            Response x = GetResponseSync(geocodeRequest);

            for (int r = 0; r < x.ResourceSets[0].Resources.Length; r++)
            {
                BingMapsRESTToolkit.Route route = (BingMapsRESTToolkit.Route)x.ResourceSets[0].Resources[r];
            }

            if (x.ResourceSets[0].Resources.Length > 0)
            {
                resultArray[0] = (int)Math.Round(((Route)x.ResourceSets[0].Resources[0]).TravelDuration);                

                if (mode != "Transit")
                {
                    if (mode != "Walking")
                    {
                        resultArray[1] = (int)Math.Round(((Route)x.ResourceSets[0].Resources[0]).TravelDurationTraffic);
                    }
                    resultArray[2] = (int)Math.Round(((Route)x.ResourceSets[0].Resources[0]).TravelDistance);
                }

                return resultArray;
            }
            else return null;
        }
        #endregion
        #endregion

        private string Compactify(string baseAddress)
        {
            string uri = Uri.EscapeDataString(baseAddress).Replace("%20", "+");
            Console.WriteLine(uri + ": " + uri.Length);
            return uri;
        }

        public async Task<double[]> GetSingleLocationTask(string address)
        {
            return await Task.FromResult(GetSingleLocation(address));
        }

        public double[] GetSingleLocation(string address)
        {
            double[] result;
            string url;
            
            StringBuilder sb = new StringBuilder();
            sb.Append("http://dev.virtualearth.net/REST/v1/Locations?");
            sb.Append(string.Format("q={0}", Compactify(address)));
            sb.Append(string.Format("&key={0}", sessionKey));
            url = sb.ToString();

            Console.WriteLine("\nUrl (" + url.Length + "): " + url);

            Uri geocodeRequest = new Uri(url);

            Response x = GetResponseSync(geocodeRequest);

            if (x.ResourceSets[0].Resources.Length > 0)
            {
                Location loc = (Location)x.ResourceSets[0].Resources[0];
                result = loc.Point.Coordinates;
                
                return result;
            }
            else return null;
        }

        #region Get a list of locations by requesting a route crossing multiple addresses
        public async Task<List<double[]>> GetLocationsWithRouteTask(List<string> addresses)
        {
            return await Task.FromResult(GetLocationsWithRoute(addresses));
        }

        public List<double[]> GetLocationsWithRoute(List<string> addresses)
        {
            List<double[]> resultList = new List<double[]>();
            List<string> urlList = new List<string>();
            //Console.WriteLine("addresses[0]: \"" + addresses[0] + "\" -> \"" + Uri.EscapeDataString(addresses[0]) + "\"");

            //for (int i = 0; i < addresses.Count; i++)
            //{
            //    string uri = Uri.EscapeDataString(addresses[i]);
            //    string compact = uri.Replace("%20", "+");
            //    Console.WriteLine("addresses["+i+"]: \"" + addresses[i] + "\" -> \"" + uri + "\" " + uri.Length);
            //    Console.WriteLine("\"" + uri + "\" -> \"" + compact + "\" " + compact.Length);
            //}

            //return null;

            //Uri geocodeRequest = new Uri(string.Format("http://dev.virtualearth.net/REST/v1/Routes/{travelMode}?wayPoint.1={wayPpoint1}&viaWaypoint.2={viaWaypoint2}&waypoint.3={waypoint3}&wayPoint.n={waypointN}&heading={heading}&optimize={optimize}&avoid={avoid}&distanceBeforeFirstTurn={distanceBeforeFirstTurn}&routeAttributes={routeAttributes}&timeType={timeType}&dateTime={dateTime}&maxSolutions={maxSolutions}&tolerances={tolerances}&distanceUnit={distanceUnit}&key={BingMapsKey}", query));
            //Uri geocodeRequest;// = new Uri(string.Format("http://dev.virtualearth.net/REST/v1/Routes/{0}?wayPoint.0={1}&waypoint.1={2}&key=AtBNs13lJOHTAKXWNi87QaWxCYEctQcUwmyYhm1-2LFfMVq2EsO47Cqa0O6OD7cx", mode, addr1, addr2));
            //Uri geocodeRequest = new Uri(string.Format("http://dev.virtualearth.net/REST/v1/Routes/Driving?wayPoint.0={0}&waypoint.1={1}&key={2}", addr1, addr2, sessionKey));

            //StringBuilder sessionKeyBuilder = new StringBuilder();
            //for (int i = 0; i < 64; i++) { sessionKeyBuilder.Append("#"); }
            //sessionKey = sessionKeyBuilder.ToString();

            StringBuilder sb = new StringBuilder();
            sb.Append("http://dev.virtualearth.net/REST/v1/Routes/Driving?");
            sb.Append(string.Format("wayPoint.0={0}", Compactify(addresses[0])));
            int wp = 1;
            for (int i = 1; i < addresses.Count; i++)
            {
                string compactAddress = Compactify(addresses[i]);
                if (sb.Length + compactAddress.Length < 2000 && wp < 25)
                {
                    sb.Append(string.Format("&wayPoint.{0}={1}", wp, compactAddress));
                    wp++;
                }
                else
                {
                    if (wp >= 25)
                    {
                        Console.WriteLine("Amount of waypoints= " + wp);
                    }
                    else Console.WriteLine(sb.Length + " + " + compactAddress.Length + " > 2000");

                    sb.Append(string.Format("&key={0}", sessionKey));
                    urlList.Add(sb.ToString());

                    sb.Clear();
                    sb.Append("http://dev.virtualearth.net/REST/v1/Routes/Driving?");
                    sb.Append(string.Format("wayPoint.0={0}", compactAddress));
                    wp = 1;
                    //break;
                }
            }
            sb.Append(string.Format("&key={0}", sessionKey));
            urlList.Add(sb.ToString());
            // sb.Append(string.Format("&key={0}", sessionKey));

            //sb.Append("&key=");
            //for(int i=0; i< 64; i++) { sb.Append("O"); }

            for (int u = 0; u < urlList.Count; u++)
            {
                Console.WriteLine("\nUrl " + u + " (" + urlList[u].Length + "): " + urlList[u]);
            }

            //string url = sb.ToString();
            //return null;

            //Uri geocodeRequest = new Uri(url);
            Uri geocodeRequest = new Uri(urlList[0]);

            Response x = GetResponseSync(geocodeRequest);

            if (x.ResourceSets[0].Resources.Length > 0)
            {
                Route route = (Route)x.ResourceSets[0].Resources[0];

                if (route.RouteLegs.Length > 0)
                {
                    // First waypoint: starting location of leg 0
                    resultList.Add(route.RouteLegs[0].StartLocation.Point.Coordinates);

                    // Other waypoints: ending location of every route leg
                    for (int r = 0; r < route.RouteLegs.Length; r++)
                    {
                        resultList.Add(route.RouteLegs[r].EndLocation.Point.Coordinates);
                    }
                    
                    for(int r=0; r< resultList.Count; r++)
                    {
                        Console.WriteLine("Result " + r + ": " + resultList[r][0] + " " + resultList[r][1]);
                    }

                    return resultList;
                }
                else return null;
            }
            else return null;
        }
#endregion

        #region Data retrieval from the PDOK (Publieke Dienstverlening Op de Kaart) database
        public double[] GetPDOKinfoFromAddress(string query)
        {            
            string queryExtended = Compactify(query) + "+&fq=type:adres&rows=1&fl=geometrie_ll";
            string url = string.Format("https://geodata.nationaalgeoregister.nl/locatieserver/v3/free?q={0}", queryExtended);

            PDOKRootObjectCoords pro = GetPDOKResponseFromAddress(new Uri(url));
            if(pro.response.docs.Count < 1) { return null; }

            PDOKDocCoords firstResult = pro.response.docs[0];
            if(firstResult.geometrie_ll == null) { return null;  }

            string[] splittedLoc = firstResult.geometrie_ll[6..^1].Split(' ');

            double lat = double.Parse(splittedLoc[1].Replace(".", ","));
            double lon = double.Parse(splittedLoc[0].Replace(".", ","));

            return new double[]{lat,lon};
        }

        private PDOKRootObjectCoords GetPDOKResponseFromAddress(Uri uri)
        {
            using (WebClient wc = new WebClient())
            {
                using (Stream data = wc.OpenRead(uri))
                {
                    DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(PDOKRootObjectCoords));
                    return ser.ReadObject(data) as PDOKRootObjectCoords;
                }
            }
        }

        public string GetPDOKinfoFromCoords(string[] coords)
        {
            
            string queryExtended = "lat="+coords[0]+"&lon="+coords[1]+ "+&fq=type:adres&rows=1&fl=weergavenaam";
            string url = string.Format("https://geodata.nationaalgeoregister.nl/locatieserver/v3/free?{0}", queryExtended);
            Console.WriteLine(url);

            PDOKRootObjectAddress pro = GetPDOKResponseFromCoords(new Uri(url));
            Console.WriteLine(pro.response.docs.Count + " results");
            if (pro.response.docs.Count < 1) { return null; }

            PDOKDocAddress firstResult = pro.response.docs[0];
            Console.WriteLine(firstResult.weergavenaam);
            return firstResult.weergavenaam;
        }
        
        private PDOKRootObjectAddress GetPDOKResponseFromCoords(Uri uri)
        {
            using (WebClient wc = new WebClient())
            {
                using (Stream data = wc.OpenRead(uri))
                {
                    DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(PDOKRootObjectAddress));
                    return ser.ReadObject(data) as PDOKRootObjectAddress;
                }
            }
        }
        #endregion
    }
}
