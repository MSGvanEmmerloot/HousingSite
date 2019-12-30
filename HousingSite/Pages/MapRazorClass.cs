using HousingSite.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HousingSite.Pages
{
    public class MapRazorClass : ComponentBase
    {
        [Inject]
        IJSRuntime JSRuntime { get; set; }
        [Inject]
        protected MapService MappingService { get; set; }

        private string sessionKey = "";
        private string[] userPinCoords = null;
        private string mapClicked = null;

        private double clickX;
        private double clickY;

        protected bool init = false;
        protected int value = 10;

        protected string place = "Apeldoorn";
        protected int radius = 1;

        protected string baseLocation = "Hyacinthstraat 2 6658 XR Beneden-Leeuwen Netherlands";
        protected int isoDist = 5;

        protected int housenum = 1;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await JSRuntime.InvokeAsync<string>("loadMapScenario");
            }
        }

        protected async Task MouseClicked(MouseEventArgs e)
        {
            clickX = e.ClientX;
            clickY = e.ClientY;
            //Console.WriteLine("e.ClientX: " + e.ClientX);
            //Console.WriteLine("e.ClientY" + e.ClientY);
            mapClicked = null;
            SyncMapClicked();
        }

        protected async Task RedrawCircle()
        {
            await JSRuntime.InvokeAsync<Task>("RedrawCircle", place, radius);
            //Console.WriteLine("Commented out drawing call :P");
            //string sk = await JSRuntime.InvokeAsync<string>("GetSessionKey");
            //Console.WriteLine("SessionKey: " + sk);
            //SyncSessionKey();

            //List<string> stringList = new List<string>();
            //stringList.Add("Linie 544 7325 DZ Apeldoorn Netherlands");
            //stringList.Add("Hyacinthstraat 2 6658 XR Beneden-Leeuwen Netherlands");
            //stringList.Add("Platolaan Nijmegen Netherlands");
            //stringList.Add("Ruitenberglaan 31 Arnhem Netherlands");
            //stringList.Add("Oude Groenewoudseweg Nijmegen Netherlands");

            //List<double[]> res = await MappingService.GetLocationsWithRouteTask(stringList);

            //for (int r = 0; r < res.Count; r++)
            //{
            //    if (res[r] != null)
            //    {
            //        Console.WriteLine("Result " + r + ": (" + res[r][0].ToString() + " " + res[r][1].ToString() + ")");
            //    }
            //}
            
        }

        protected async Task AddPin()
        {
            await JSRuntime.InvokeAsync<Task>("AddColorHousetoAddress", place, housenum, "House " + housenum + ", " + place, "");
            housenum++;
            Console.WriteLine("Placed pin at " + place);
        }

        protected async Task ClearMap()
        {
            await JSRuntime.InvokeAsync<Task>("ClearMap");
            housenum = 1;
        }

        protected async Task DrawCircleAtAddress()
        {
            Console.WriteLine("DrawCircleAtAddress");
            await JSRuntime.InvokeAsync<Task>("DrawCircleAtAddressWithRadius", baseLocation, radius);
            Console.WriteLine("Circle drawn at " + place);            
        }

        //protected async Task PinAddress()
        //{
        //    Console.WriteLine("PinAddress");
        //    string address = "Linie 7325 DZ";//Hyacinthstraat 6658 XR
        //    await JSRuntime.InvokeAsync<Task>("AddAltenPin", address);
        //    Console.WriteLine("Alten pinned at " + address);
        //}

        //protected async Task RouteFind_TS()
        //{
        //    double[] pointA = await MappingService.GetCoordinatesAsync("Linie 544 7325 DZ Apeldoorn Netherlands");
        //    double[] pointB = await MappingService.GetCoordinatesAsync("Hyacinthstraat 2 6658 XR Beneden-Leeuwen Netherlands");

        //    await JSRuntime.InvokeAsync<Task>("CalculateRouteWithOption", pointA, pointB, "transit");
        //}

        protected async Task RouteFind_TS_WithMode(string transportationMode)
        {
            //await Task.Run(() => SyncSessionKey());

            //double[] pointA = await MappingService.GetCoordinatesAsync("Linie 544 7325 DZ Apeldoorn Netherlands");
            //double[] pointB = await MappingService.GetCoordinatesAsync("Hyacinthstraat 2 6658 XR Beneden-Leeuwen Netherlands");
            //Console.WriteLine(pointA);
            //Console.WriteLine(pointB);
            //await JSRuntime.InvokeAsync<Task>("CalculateRouteWithOption", pointA, pointB, transportationMode);
        }

        protected async Task RouteFind()
        {
            string addressA = "Linie 544 7325 DZ Apeldoorn Netherlands";
            string addressB = "Hyacinthstraat 2 6658 XR Beneden-Leeuwen Netherlands";

            Console.WriteLine("Going from \"" + addressA + "\" to \"" + addressB + "\"");
            await GetRouteWithMode(addressA, addressB, "Driving");
            await GetRouteWithMode(addressA, addressB, "Walking");
            await GetRouteWithMode(addressA, addressB, "Transit");
        }

        protected async Task ShowTraffic()
        {
            await JSRuntime.InvokeAsync<Task>("ShowTraffic");
        }

        protected async Task GetRouteWithMode(string addressA, string addressB, string mode)
        {
            SyncSessionKey();

            int[] res = await MappingService.GetRouteAsyncWithMode(addressA, addressB, mode);
            if (res != null && res.Length == 3)
            {
                Console.WriteLine(mode);
                Console.WriteLine("Duration without traffic: " + res[0] + " -> " + DurationFromSeconds(res[0]));
                if (res[1] != 0) 
                {
                    Console.WriteLine("Duration with traffic: " + res[1] + " -> " + DurationFromSeconds(res[1])); 
                }
                if (res[2] != 0)
                {
                    Console.WriteLine("Distance: " + res[2] + "km");
                }
                Console.WriteLine();
            }
        }

        private string DurationFromSeconds(int sec)
        {
            int durationMinutes = sec / 60;
            int durationHours = 0;

            if (durationMinutes >= 60)
            {
                durationHours = durationMinutes / 60;
                durationMinutes = durationMinutes - (durationHours * 60);
            }

            return durationHours + " hours and " + durationMinutes + " minutes.";
        }
                
        protected async Task PlotIsochrone()
        {
            Console.WriteLine("Plotting isochrone: distance = " + isoDist + " driving minutes.");
            await JSRuntime.InvokeAsync<Task>("ClearMap");
            await JSRuntime.InvokeAsync<Task>("GetIsochrone", baseLocation, isoDist);
        }

        // Polls sessionKey until it's set and then passes this variable to MappingService
        private async void SyncSessionKey()
        {
            if(sessionKey == "" || sessionKey == null)
            {
                sessionKey = await JSRuntime.InvokeAsync<string>("GetSessionKey");
                SyncSessionKey();
            }
            else
            {
                if (MappingService.sessionKey != sessionKey)
                {
                    MappingService.sessionKey = sessionKey;
                    Console.WriteLine("\nSet sessionKey of MappingService: " + sessionKey);
                }
            }
        }
        private async void SyncUserPin()
        {
            if (userPinCoords == null)
            {
                userPinCoords = await JSRuntime.InvokeAsync<string[]>("GetUserpinCoords");
                SyncUserPin();
            }
            else
            {
                string address = MappingService.GetPDOKinfoFromCoords(userPinCoords);
                if (address != null)
                {
                    Console.WriteLine("Address of (" + userPinCoords[0] + " " + userPinCoords[1] + "): " + address);
                    await JSRuntime.InvokeAsync<Task>("UpdateLocationInfoBox", address);
                }
            }
        }

        private async void SyncMapClicked()
        {
            if (mapClicked == null)
            {
                mapClicked = await JSRuntime.InvokeAsync<string>("GetMapClicked");
                SyncMapClicked();
            }
            else
            {
                if (mapClicked == "Yes")
                {
                    Console.WriteLine("Clicked on map");
                    await JSRuntime.InvokeAsync<Task>("LocationfromMouseCoords", clickX, clickY);
                    userPinCoords = null;
                    SyncUserPin();
                }
            }
        }

    }
}
