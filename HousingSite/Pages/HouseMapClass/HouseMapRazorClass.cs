//using HousingSite.Data;
//using Microsoft.AspNetCore.Components;
//using Microsoft.AspNetCore.Components.Web;
//using Microsoft.JSInterop;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Text.RegularExpressions;
//using System.Threading.Tasks;

//namespace HousingSite.Pages
//{
//    public class HouseMapRazorClass : ComponentBase
//    {
//        [Inject]
//        IJSRuntime JSRuntime { get; set; }
//        [Inject]
//        protected HouseTableService HouseService { get; set; }
//        [Inject]
//        protected MapService MappingService { get; set; }

//        private string sessionKey = "";
//        private string[] userPinCoords = null;
//        private string mapClicked = null;

//        private double clickX;
//        private double clickY;

//        protected List<HouseData> houseData = new List<HouseData>();
//        protected List<bool> housesInRadius = new List<bool>();

//        protected bool init = false;
//        protected int housesAdded = 0;
//        protected string statusText = "";

//        protected string city = "Nijmegen";
//        protected int priceMin = 200;
//        protected int priceMax = 800;
//        protected int area = 0;
//        protected int bedrooms = 0;

//        protected string place = "Nijmegen Platolaan";
//        protected double radius = 2.5;

//        protected int radiusTime = 10;

//        protected int housenum = 0;

//        protected bool viewSingleHouse = false;
//        protected int selectedHouse = -1;
//        protected bool routesCalculated = false;

//        protected string walkingRouteText = "Walking route";
//        protected string drivingRouteText = "Driving route";
//        protected string transitRouteText = "Transit route";

//        protected enum TravelModes { none, walking, driving, transit };
//        protected static string[] travelModeStrings = new string[4] { "", "Walking", "Driving", "Public transport" };
//        protected TravelModes selectedTravelMode = TravelModes.driving;

//        protected enum CalculationModes { none, time, distance };
//        protected static string[] calculationModeStrings = new string[3] { "", "Time", "Distance" };
//        protected CalculationModes selectedCalculationMode = CalculationModes.distance;

//        protected float travelTime = 10;
//        protected float travelDistance = 3;

//        protected bool showSearchOptions = true;

//        public struct SearchParams
//        {
//            public string city;
//            public int priceMin;
//            public int priceMax;
//            public int area;
//            public int bedrooms;
//        }
//        public SearchParams lastSearchParams;
        
//        protected override async Task OnAfterRenderAsync(bool firstRender)
//        {
//            if (firstRender)
//            {
//                await JSRuntime.InvokeAsync<string>("loadMapScenario");
//                SyncSessionKey();
//            }
//        }

//        protected async Task MouseClicked(MouseEventArgs e)
//        {
//            clickX = e.ClientX;
//            clickY = e.ClientY;
//            mapClicked = null;
//            SyncMapClicked();
//        }

//        protected async Task FindHouses()
//        {
//            if (CheckIfSameParams())
//            {
//                Console.WriteLine("Same params. Refreshing view.");
//                await RefreshVisibleHouses();
//            }
//            else
//            {
//                Console.WriteLine("Other params. Getting new data!");
//                houseData.Clear();
//                housesInRadius.Clear();
//                HouseService.houseData.Clear();
//                await JSRuntime.InvokeAsync<Task>("ClearHouses");
//                await JSRuntime.InvokeAsync<Task>("CalculateCircleAtAddressWithRadius", MappingService.GetPDOKinfoFromAddress(place), radius);
//                housesAdded = 0;
//                SetLastParams(city, priceMin, priceMax, area, bedrooms);
//                await HouseService.SetSearchParams(city, priceMin, priceMax, area, bedrooms);

//                for (int s = 0; s < HouseService.sitesChecked.Count; s++)
//                {
//                    string sn = HouseService.housingSites.Keys.ElementAt(s);
//                    if (HouseService.sitesChecked[s] == true)
//                    {
//                        Console.WriteLine("Site " + s + "(" + sn + ") is checked!");
//                        statusText = "Processing " + sn;
//                        this.StateHasChanged();
//                        await HouseService.RunAsync(s);
//                        houseData = await HouseService.GetHouseData();
//                        for(int h= housesInRadius.Count; h< houseData.Count; h++)
//                        {
//                            housesInRadius.Add(true);
//                        }
//                        Console.WriteLine(houseData.Count + " houses retrieved");
//                        statusText = "Adding houses from " + sn;
//                        this.StateHasChanged();

//                        await CheckHouseDistances();
//                        Console.WriteLine(housesInRadius.Count + " house distances checked");
//                        await AddColorPinsToHousesInRadius();                        

//                        housesAdded = houseData.Count;
//                        statusText = "Added houses from " + sn;
//                        this.StateHasChanged();
//                    }
//                }

//                //Debugging
//                //await HouseService.SetSearchParams(city, 100, 2000, area, bedrooms);
//                //await HouseService.RunAsync(0);
//                //houseData = await HouseService.GetHouseData();
//                //await AddPinsToHouses();
//            }
//        }

//        private void SetLastParams(string city, int priceMin, int priceMax, int area, int bedrooms)
//        {
//            lastSearchParams.city = city;
//            lastSearchParams.priceMin = priceMin;
//            lastSearchParams.priceMax = priceMax;
//            lastSearchParams.area = area;
//            lastSearchParams.bedrooms = bedrooms;
//        }

//        private bool CheckIfSameParams()
//        {
//            return lastSearchParams.city == city && lastSearchParams.priceMin == priceMin && lastSearchParams.priceMax == priceMax && lastSearchParams.area == area && lastSearchParams.bedrooms == bedrooms;
//        }

//        protected async Task RefreshVisibleHouses()
//        {
//            await JSRuntime.InvokeAsync<Task>("ClearHouses");
//            housesAdded = 0;

//            await CheckHouseDistances();
//            await AddColorPinsToHousesInRadius();
//        }

//        protected async Task AddColorPinsToHousesInRadius()
//        {
//            Console.WriteLine("Houses already added: " + housesAdded + "/" + houseData.Count);
//            for (int h = 0; h < houseData.Count; h++)
//            {
//                if (h < housesAdded) { continue; }
//                if (housesInRadius.Count < h || !housesInRadius[h]) { continue; }

//                string houseQuery = houseData[h].street + " " + houseData[h].postalCode;

//                string extendedDescription = houseData[h].street + ", " + houseData[h].postalCode + ", " + houseData[h].neighbourhood + "\n" +
//                                houseData[h].price + " voor " + houseData[h].area;

//                //string houseColor = "white";
//                //int priceRange = priceMax - priceMin;
//                //double priceStep = priceRange / 3;
//                //double housePrice = double.Parse(Regex.Match(houseData[h].price, @"\d+").Value);
//                //Console.WriteLine("House " + h + " price: " + housePrice);

//                //if (priceRange > 0)
//                //{
//                //    if (housePrice < (priceMin + priceStep))
//                //    {
//                //        Console.WriteLine(housePrice + " < " + (priceMin + priceStep));
//                //        houseColor = "green";
//                //    }
//                //    else if (housePrice < (priceMin + (2 * priceStep)))
//                //    {
//                //        Console.WriteLine(housePrice + " < " + (priceMin + (2 * priceStep)));
//                //        houseColor = "yellow";
//                //    }
//                //    else
//                //    {
//                //        Console.WriteLine(housePrice + " > " + (priceMin + (2 * priceStep)));
//                //        houseColor = "red";
//                //    }
//                //}
//                //Console.WriteLine("House " + h + " color: " + houseColor);
//                string houseColor = GetHouseColor(houseData[h]);

//                //await JSRuntime.InvokeAsync<Task>("AddColorHousetoAddress", houseQuery, "House " + h, extendedDescription, houseColor); 
//                await JSRuntime.InvokeAsync<Task>("AddColorHousetoAddress", MappingService.GetPDOKinfoFromAddress(houseQuery), "House " + h, extendedDescription, houseColor);
//            }
//        }

//        private string GetHouseColor(HouseData house)
//        {
//            string houseColor = "white";
//            int priceRange = priceMax - priceMin;
//            double priceStep = priceRange / 3;
//            double housePrice = double.Parse(Regex.Match(house.price, @"\d+").Value);
//            //Console.WriteLine("House " + h + " price: " + housePrice);

//            if (priceRange > 0)
//            {
//                if (housePrice < (priceMin + priceStep))
//                {
//                    Console.WriteLine(housePrice + " < " + (priceMin + priceStep));
//                    houseColor = "green";
//                }
//                else if (housePrice < (priceMin + (2 * priceStep)))
//                {
//                    Console.WriteLine(housePrice + " < " + (priceMin + (2 * priceStep)));
//                    houseColor = "yellow";
//                }
//                else
//                {
//                    Console.WriteLine(housePrice + " > " + (priceMin + (2 * priceStep)));
//                    houseColor = "red";
//                }
//            }
//            //Console.WriteLine("House " + h + " color: " + houseColor);
//            return houseColor;
//        }

//        protected async Task AddToSpecificLayer()
//        {
//            string houseColor = GetHouseColor(houseData[selectedHouse]);
//            string houseQuery = houseData[selectedHouse].street + " " + houseData[selectedHouse].postalCode;
//            Console.WriteLine("Adding to specific layer.");
//            await JSRuntime.InvokeAsync<Task>("AddToSpecificLayer", MappingService.GetPDOKinfoFromAddress(houseQuery), "House", MappingService.GetPDOKinfoFromAddress(place), "Base", houseColor);
//        }

//        protected async Task CheckHouseDistances()
//        {
//            for (int h = 0; h < houseData.Count; h++)
//            {
//                if (h < housesAdded) { continue; }

//                bool inRadius = await CheckHouseInRadius(h);
//                if (h < housesInRadius.Count) {
//                    housesInRadius[h] = inRadius;
//                }
//                else housesInRadius.Add(inRadius);
//            }
//        }

//        protected async Task<bool> CheckHouseInRadius(int h)
//        {
//            string houseQueryExtend = houseData[h].street + " " + houseData[h].postalCode + " " + houseData[h].city + " Netherlands";

//            //double[] d = await MappingService.GetCoordinatesAsync(houseQueryExtend); 
//            //double[] p = await MappingService.GetCoordinatesAsync(place);
//            double[] d = MappingService.GetPDOKinfoFromAddress(houseQueryExtend);
//            double[] p = MappingService.GetPDOKinfoFromAddress(place);
//            double dist = await MappingService.CalculateDistance(d, p);

//            if (dist < radius)
//            {
//                Console.WriteLine("House " + h + " is inside the radius! (" + dist + "km)");
//                return true;
//            }
//            else
//            {
//                Console.WriteLine("*House " + h + " is NOT inside the radius! (" + dist + "km)");
//                return false;
//            }
//        }

//        protected async Task ClearMap()
//        {
//            await JSRuntime.InvokeAsync<Task>("ClearMap");
//            housenum = 1;
//        }

//        protected async Task PlacePinAtAddress()
//        {
//            //await JSRuntime.InvokeAsync<Task>("DrawCircleAtAddressWithRadius", place, radius);
//            await JSRuntime.InvokeAsync<Task>("AddBaseLocationPintoAddress", MappingService.GetPDOKinfoFromAddress(place));
//        }

//        protected async Task DrawCircleAtAddress()
//        {
//            //await JSRuntime.InvokeAsync<Task>("DrawCircleAtAddressWithRadius", place, radius);
//            await JSRuntime.InvokeAsync<Task>("DrawCircleAtAddressWithRadius", MappingService.GetPDOKinfoFromAddress(place), radius);
//        }

//        protected async Task DrawIsochroneAtAddress()
//        {
//            //await JSRuntime.InvokeAsync<Task>("DrawCircleAtAddressWithRadius", place, radius);
//            //await JSRuntime.InvokeAsync<Task>("GetIsochroneByTime", MappingService.GetPDOKinfoFromAddress(place), "walking", radiusTime); //GetIsochroneByTime

//            if (selectedCalculationMode == CalculationModes.time)
//            {
//                Console.WriteLine("Calculating with a " + selectedTravelMode + " time of " + travelTime + " minutes.");
//                await JSRuntime.InvokeAsync<Task>("GetIsochroneByTime", MappingService.GetPDOKinfoFromAddress(place), selectedTravelMode.ToString(), travelTime);
//            }
//            else if (selectedCalculationMode == CalculationModes.distance)
//            {
//                Console.WriteLine("Calculating with a " + selectedTravelMode + " distance of " + travelDistance + " kilometres.");
//                await JSRuntime.InvokeAsync<Task>("GetIsochroneByDistance", MappingService.GetPDOKinfoFromAddress(place), selectedTravelMode.ToString(), travelDistance);
//            }
//            else Console.WriteLine("No calculation mode selected.");
//        }

//        protected string imageColor = "green";
//        protected async Task TestPin()
//        {
//            await JSRuntime.InvokeAsync<Task>("ClearMap");
//            //await JSRuntime.InvokeAsync<Task>("AddColorHousetoAddress", "Linie 544 Apeldoorn", "Title", "Description", imageColor);
//            await JSRuntime.InvokeAsync<Task>("AddColorHousetoAddress", MappingService.GetPDOKinfoFromAddress("Linie 544 Apeldoorn"), "Title", "Description", imageColor);
//            switch (imageColor)
//            {
//                case "green":
//                    imageColor = "yellow";
//                    break;
//                case "yellow":
//                    imageColor = "red";
//                    break;
//                case "red":
//                    imageColor = "green";
//                    break;
//            }
//        }

//        protected async Task SwitchLayer()
//        {
//            viewSingleHouse = !viewSingleHouse;
//            this.StateHasChanged();
//            await JSRuntime.InvokeAsync<Task>("SwitchLayer");
//            await AddToSpecificLayer();
//        }

//        //protected async Task SetChosenHouse()
//        //{
//        //    if (houseData == null || houseData.Count == 0) { return; }
//        //    selectedHouse = 0;

//        //    routesCalculated = false;

//        //    string houseQueryExtend = houseData[selectedHouse].street + " " + houseData[selectedHouse].postalCode + " " + houseData[selectedHouse].city + " Netherlands";
//        //    //await JSRuntime.InvokeAsync<Task>("SetChosenHouse", houseQueryExtend);
//        //    await JSRuntime.InvokeAsync<Task>("SetChosenHouse", MappingService.GetPDOKinfoFromAddress(houseQueryExtend)); 

//        //    await GetRouteWithMode(houseQueryExtend, place, "Driving");
//        //    await GetRouteWithMode(houseQueryExtend, place, "Walking");
//        //    await GetRouteWithMode(houseQueryExtend, place, "Transit");

//        //    routesCalculated = true;
//        //}

//        protected async Task SetChosenHouse(int housenum)
//        {
//            selectedHouse = housenum;

//            Console.WriteLine(selectedHouse);

//            routesCalculated = false;

//            string houseQueryExtend = houseData[selectedHouse].street + " " + houseData[selectedHouse].postalCode + " " + houseData[selectedHouse].city + " Netherlands";
//            //await JSRuntime.InvokeAsync<Task>("SetChosenHouse", houseQueryExtend);
//            double[] coords = MappingService.GetPDOKinfoFromAddress(houseQueryExtend);
//            //await JSRuntime.InvokeAsync<Task>("SetChosenHouse", coords);
//            await JSRuntime.InvokeAsync<Task>("CheckAddressInsideIsochrone", coords);

//            //await GetRouteWithMode(houseQueryExtend, place, "Driving");
//            //await GetRouteWithMode(houseQueryExtend, place, "Walking");
//            //await GetRouteWithMode(houseQueryExtend, place, "Transit");

//            //routesCalculated = true;

//            await SwitchLayer();
//        }

//        protected async Task PlotRoute(string mode)
//        {
//            //await Task.Run(() => Console.WriteLine(mode));
//            await JSRuntime.InvokeAsync<Task>("PlotRoute", mode.ToLower());
//        }

//        protected async Task GetRoutes()
//        {
//            string houseQueryExtend = houseData[selectedHouse].street + " " + houseData[selectedHouse].postalCode + " " + houseData[selectedHouse].city + " Netherlands";
//            //await JSRuntime.InvokeAsync<Task>("SetChosenHouse", houseQueryExtend);
//            await JSRuntime.InvokeAsync<Task>("SetChosenHouse", MappingService.GetPDOKinfoFromAddress(houseQueryExtend));

//            await GetRouteWithMode(houseQueryExtend, place, "Driving");
//            await GetRouteWithMode(houseQueryExtend, place, "Walking");
//            await GetRouteWithMode(houseQueryExtend, place, "Transit");

//            routesCalculated = true;
//        }

//        protected async Task GetRouteWithMode(string addressA, string addressB, string mode)
//        {
//            int[] res = await MappingService.GetRouteAsyncWithMode(addressA, addressB, mode);
            
//            StringBuilder sb = new StringBuilder();
//            if (res != null && res.Length == 3)
//            {
//                Console.WriteLine(mode);
//                Console.WriteLine("Duration without traffic: " + res[0] + " -> " + DurationFromSeconds(res[0]));

//                if(mode != "Driving")
//                {
//                    sb.Append("Travel time: +/- " + DurationFromSeconds(res[0]));
//                }
//                else sb.Append("Travel time (without traffic): +/- " + DurationFromSeconds(res[0]));

//                if (res[2] != 0)
//                {
//                    Console.WriteLine("Distance: " + res[2] + "km");
//                    sb.Append("\nTravel Distance: +/- " + res[2] + " km");
//                }
//                Console.WriteLine();
//            }

//            string resultText = sb.ToString();

//            switch (mode)
//            {
//                case "Driving":
//                    drivingRouteText = resultText;
//                    break;
//                case "Transit":
//                    transitRouteText = resultText;
//                    break;
//                case "Walking":                    
//                default:
//                    walkingRouteText = resultText;
//                    break;
//            }
//        }

//        private string DurationFromSeconds(int sec)
//        {
//            int durationMinutes = sec / 60;
//            int durationHours = 0;

//            if (durationMinutes >= 60)
//            {
//                durationHours = durationMinutes / 60;
//                durationMinutes = durationMinutes - (durationHours * 60);
//            }

//            return durationHours + " h " + durationMinutes + " min.";
//        }

//        // Polls sessionKey until it's set and then passes this variable to MappingService
//        private async void SyncSessionKey()
//        {
//            if (sessionKey == "" || sessionKey == null)
//            {
//                sessionKey = await JSRuntime.InvokeAsync<string>("GetSessionKey");
//                SyncSessionKey();
//            }
//            else
//            {
//                if (MappingService.sessionKey != sessionKey)
//                {
//                    MappingService.sessionKey = sessionKey;
//                    Console.WriteLine("\nSet sessionKey of MappingService: " + sessionKey);
//                }
//            }
//        }

//        private async void SyncUserPin()
//        {
//            if (userPinCoords == null)
//            {
//                userPinCoords = await JSRuntime.InvokeAsync<string[]>("GetUserpinCoords");
//                SyncUserPin();
//            }
//            else
//            {
//                string address = MappingService.GetPDOKinfoFromCoords(userPinCoords);
//                if (address != null)
//                {
//                    Console.WriteLine("Address of (" + userPinCoords[0] + " " + userPinCoords[1] + "): " + address);
//                    await JSRuntime.InvokeAsync<Task>("UpdateLocationInfoBox", address);
//                }
//            }
//        }

//        private async void SyncMapClicked()
//        {
//            if (mapClicked == null)
//            {
//                mapClicked = await JSRuntime.InvokeAsync<string>("GetMapClicked");
//                SyncMapClicked();
//            }
//            else
//            {
//                if (mapClicked == "Yes")
//                {
//                    Console.WriteLine("Clicked on map");
//                    await JSRuntime.InvokeAsync<Task>("LocationfromMouseCoords", clickX, clickY);
//                    userPinCoords = null;
//                    SyncUserPin();
//                }
//            }
//        }
//    }
//}
