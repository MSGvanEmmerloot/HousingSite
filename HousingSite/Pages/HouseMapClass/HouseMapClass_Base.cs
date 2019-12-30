using HousingSite.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HousingSite.Pages.HouseMapComponents;

namespace HousingSite.Pages
{
    public partial class HouseMapClass : ComponentBase
    {
        [Inject]
        IJSRuntime JSRuntime { get; set; }
        [Inject]
        protected HouseTableService HouseService { get; set; }
        [Inject]
        protected MapService MappingService { get; set; }

        #region Variables
        private string sessionKey = "";
        private string[] userPinCoords = null;
        private string mapClicked = null;

        private double clickX;
        private double clickY;

        protected List<HouseData> houseData = new List<HouseData>();
        protected List<bool> housesInRadius = new List<bool>();

        protected bool init = false;
        protected int housesAdded = 0;
        protected string statusText = "";

        protected string city = "Nijmegen";
        protected int priceMin = 200;
        protected int priceMax = 800;
        protected int area = 0;
        protected int bedrooms = 0;

        protected string place = "Nijmegen Platolaan";
        protected double radius = 2.5;

        protected int radiusTime = 10;

        protected int housenum = 0;

        protected bool viewSingleHouse = false;
        protected int selectedHouse = -1;
        protected bool routesCalculated = false;

        protected string walkingRouteText = "Walking route";
        protected string drivingRouteText = "Driving route";
        protected string transitRouteText = "Transit route";

        public enum HouseFilters { all, circle, isochrone };
        protected static string[] houseFilterStrings = new string[3] { "All", "Only inside circle", "Only inside isochrone" };
        protected HouseFilters selectedHouseFilter = HouseFilters.circle;

        public enum TravelModes { none, walking, driving, transit };
        protected static string[] travelModeStrings = new string[4] { "", "Walking", "Driving", "Public transport" };
        protected TravelModes selectedTravelMode = TravelModes.driving;

        public enum CalculationModes { none, time, distance };
        protected static string[] calculationModeStrings = new string[3] { "", "Time", "Distance" };
        protected CalculationModes selectedCalculationMode = CalculationModes.distance;

        protected float travelTime = 10;
        protected float travelDistance = 3;

        protected bool showSearchOptions = true;

        protected Dictionary<string, double[]> addressCoordinatesDictionary = new Dictionary<string, double[]>();
        protected Dictionary<string, string> addressQueue = new Dictionary<string, string>();
        protected KeyValuePair<string, double[]> baseAddress = new KeyValuePair<string, double[]>();
        protected int transactions = 0;

        protected bool AddUserLocationOnClick = false;
        protected bool HouseCalculationsFinished = true;

        protected int progress1Value = 0;
        protected int progress2Value = 0;
        protected int progress3Value = 0;
        protected int progress1Max = 0;
        protected int progress2Max = 0;
        protected int progress3Max = 0;
        protected int progress1Fill = 0;
        protected int progress2Fill = 0;
        protected int progress3Fill = 0;

        public struct SearchParams
        {
            public string city;
            public int priceMin;
            public int priceMax;
            public int area;
            public int bedrooms;
        }
        public SearchParams lastSearchParams;
        #endregion

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await JSRuntime.InvokeAsync<string>("loadMapScenario");
                SyncSessionKey();
                //Console.WriteLine("Hi");
            }

            //while (true)
            //{
            //Console.WriteLine(showSearchOptions);
            //System.Threading.Thread.Sleep(1000);
            //}
        }

        //public HouseMapComponent_SearchOptions SearchOptions;
        //public HouseMapComponent_MapOptions MapOptions;

        //protected async Task GetChildComponentVariables()
        //{
        //    if (showSearchOptions)
        //    {
        //        await SearchOptions.UpdateParentVariables();
        //    }
        //    else await MapOptions.UpdateParentVariables();
        //    await Test();
        //}

        //protected async Task Test()
        //{
        //    Console.WriteLine("Testy");

        //    Console.WriteLine("showSearchOptions = " + showSearchOptions);
        //    Console.WriteLine("city = " + city);
        //    Console.WriteLine("priceMin = " + priceMin);
        //    Console.WriteLine("priceMax = "+ priceMax);
        //    Console.WriteLine("area = " + area);
        //    Console.WriteLine("bedrooms = " + bedrooms);

        //    Console.WriteLine("selectedHouseFilter = " + selectedHouseFilter);
        //    Console.WriteLine("selectedTravelMode = " + selectedTravelMode);
        //    Console.WriteLine("selectedCalculationMode = " + selectedCalculationMode);
        //    Console.WriteLine("travelDistance = " + travelDistance);
        //    Console.WriteLine("travelTime = " + travelTime);
        //    Console.WriteLine("place = " + place);
        //    Console.WriteLine("radius = " + radius);
        //    Console.WriteLine("AddUserLocationOnClick = " + AddUserLocationOnClick);
        //}

        protected async Task MouseClicked(MouseEventArgs e)
        {
            clickX = e.ClientX;
            clickY = e.ClientY;
            mapClicked = null;
            SyncMapClicked();
        }

        protected async Task ToggleAddUserLocationOnClick()
        {
            // Pass !AddUserLocationOnClick since it's not updated immediately
            await JSRuntime.InvokeAsync<Task>("AddUserLocationOnClick", !AddUserLocationOnClick);
        }

        private async void CheckBaseKey()
        {
            if (baseAddress.Key == null || baseAddress.Value == null || baseAddress.Key != place)
            {
                //baseAddress = new KeyValuePair<string, double[]>(place, MappingService.GetPDOKinfoFromAddress(place));
                double[] baseCoordinates = await MappingService.GetSingleLocationTask(place);
                baseAddress = new KeyValuePair<string, double[]>(place, baseCoordinates);
                transactions++;
                Console.WriteLine("New base address: " + baseAddress.Key + " (" + baseAddress.Value[0] + " " + baseAddress.Value[1] + ")");
            }
        }

        protected async Task FindHouses()
        {
            CheckBaseKey();

            if (CheckIfSameParams())
            {
                Console.WriteLine("Same params. Refreshing view.");
                await RefreshVisibleHouses();
            }
            else
            {
                HouseCalculationsFinished = false;
                Console.WriteLine("Other params. Getting new data!");
                houseData.Clear();
                housesInRadius.Clear();
                HouseService.houseData.Clear();
                await JSRuntime.InvokeAsync<Task>("ClearHouses");
                await JSRuntime.InvokeAsync<Task>("ClearHousePins");
                await JSRuntime.InvokeAsync<Task>("CalculateCircleAtAddressWithRadius", baseAddress.Value, radius);
                housesAdded = 0;
                SetLastParams(city, priceMin, priceMax, area, bedrooms);
                await HouseService.SetSearchParams(city, priceMin, priceMax, area, bedrooms);

                progress1Max = HouseService.sitesChecked.Count;
                for (int s = 0; s < HouseService.sitesChecked.Count; s++)
                {   
                    //progressValue1 = (s / HouseService.sitesChecked.Count) * 100;
                    string sn = HouseService.housingSites.Keys.ElementAt(s);
                    if (HouseService.sitesChecked[s] == true)
                    {
                        Console.WriteLine("Site " + s + "(" + sn + ") is checked!");
                        statusText = "Processing " + sn;
                        this.StateHasChanged();
                        await HouseService.RunAsync(s);
                        houseData = await HouseService.GetHouseData();
                        for(int h= housesInRadius.Count; h< houseData.Count; h++)
                        {
                            housesInRadius.Add(true);
                        }
                        Console.WriteLine(houseData.Count + " houses retrieved");
                        statusText = "Adding houses from " + sn;
                        this.StateHasChanged();

                        //await CheckHouseDistances();
                        //Console.WriteLine(housesInRadius.Count + " house distances checked");
                        //await AddColorPinsToHousesInRadius();
                        await AddHousesToQueue();

                        housesAdded = houseData.Count;
                        statusText = "Added houses from " + sn;
                        this.StateHasChanged();
                    }
                    progress1Value = s+1;
                    progress1Fill = (progress1Max == 0) ? 0 : (progress1Value * 100 / progress1Max);
                    //progress1Fill = (progress1Max == 0) ? 0 : (progress1Value / progress1Max) * 100;
                    this.StateHasChanged();
                }
                await AddHousesToQueue();
                await UpdateDictionary();
                await CheckHouseDistances();
                //Console.WriteLine(housesInRadius.Count + " house distances checked");
                //await AddColorPinsToHousesInRadius();
                await AddColorPinsToAllHouses();
                Console.WriteLine(transactions + " transactions done.");
                await ApplyHouseFilter();
                HouseCalculationsFinished = true;
                //Debugging
                //await HouseService.SetSearchParams(city, 100, 2000, area, bedrooms);
                //await HouseService.RunAsync(0);
                //houseData = await HouseService.GetHouseData();
                //await AddPinsToHouses();
            }
        }

        #region Map manipulation        
        protected async Task ClearMap()
        {
            await JSRuntime.InvokeAsync<Task>("ClearMap");
            housenum = 1;
        }

        protected async Task PlacePinAtAddress()
        {
            CheckBaseKey();
            //await JSRuntime.InvokeAsync<Task>("DrawCircleAtAddressWithRadius", place, radius);
            await JSRuntime.InvokeAsync<Task>("AddBaseLocationPintoAddress", baseAddress.Value);
        }

        protected async Task DrawCircleAtAddress()
        {
            CheckBaseKey();
            //await JSRuntime.InvokeAsync<Task>("DrawCircleAtAddressWithRadius", place, radius);
            await JSRuntime.InvokeAsync<Task>("DrawCircleAtAddressWithRadius", baseAddress.Value, radius);
        }

        protected async Task DrawIsochroneAtAddress()
        {
            //await JSRuntime.InvokeAsync<Task>("DrawCircleAtAddressWithRadius", place, radius);
            //await JSRuntime.InvokeAsync<Task>("GetIsochroneByTime", MappingService.GetPDOKinfoFromAddress(place), "walking", radiusTime); //GetIsochroneByTime
            CheckBaseKey();

            if (selectedCalculationMode == CalculationModes.time)
            {
                Console.WriteLine("Calculating with a " + selectedTravelMode + " time of " + travelTime + " minutes.");
                await JSRuntime.InvokeAsync<Task>("GetIsochroneByTime", baseAddress.Value, selectedTravelMode.ToString(), travelTime);
            }
            else if (selectedCalculationMode == CalculationModes.distance)
            {
                Console.WriteLine("Calculating with a " + selectedTravelMode + " distance of " + travelDistance + " kilometres.");
                await JSRuntime.InvokeAsync<Task>("GetIsochroneByDistance", baseAddress.Value, selectedTravelMode.ToString(), travelDistance);
            }
            else Console.WriteLine("No calculation mode selected.");
        }
        #endregion

        #region Testing
        protected string imageColor = "green";
        protected async Task TestPin()
        {
            await JSRuntime.InvokeAsync<Task>("ClearMap");
            //await JSRuntime.InvokeAsync<Task>("AddColorHousetoAddress", "Linie 544 Apeldoorn", "Title", "Description", imageColor);
            await JSRuntime.InvokeAsync<Task>("AddColorHousetoAddress", MappingService.GetPDOKinfoFromAddress("Linie 544 Apeldoorn"), "Title", "Description", imageColor);
            switch (imageColor)
            {
                case "green":
                    imageColor = "yellow";
                    break;
                case "yellow":
                    imageColor = "red";
                    break;
                case "red":
                    imageColor = "green";
                    break;
            }
        }
        #endregion

        protected async Task SwitchLayer()
        {
            viewSingleHouse = !viewSingleHouse;
            this.StateHasChanged();
            await JSRuntime.InvokeAsync<Task>("SwitchLayer");
            await AddToSpecificLayer();
        }

        //protected async Task SetChosenHouse()
        //{
        //    if (houseData == null || houseData.Count == 0) { return; }
        //    selectedHouse = 0;

        //    routesCalculated = false;

        //    string houseQueryExtend = houseData[selectedHouse].street + " " + houseData[selectedHouse].postalCode + " " + houseData[selectedHouse].city + " Netherlands";
        //    //await JSRuntime.InvokeAsync<Task>("SetChosenHouse", houseQueryExtend);
        //    await JSRuntime.InvokeAsync<Task>("SetChosenHouse", MappingService.GetPDOKinfoFromAddress(houseQueryExtend)); 

        //    await GetRouteWithMode(houseQueryExtend, place, "Driving");
        //    await GetRouteWithMode(houseQueryExtend, place, "Walking");
        //    await GetRouteWithMode(houseQueryExtend, place, "Transit");

        //    routesCalculated = true;
        //}

        protected async Task SetChosenHouse(int housenum)
        {
            selectedHouse = housenum;

            Console.WriteLine(selectedHouse);

            routesCalculated = false;

            double[] coords = addressCoordinatesDictionary[houseData[selectedHouse].postalCode.Replace(" ", "")];
            if (coords != null)
            {
                Console.WriteLine("Coords: " + coords[0] + " " + coords[1]);
                await JSRuntime.InvokeAsync<Task>("CheckAddressInsideBounds", coords);
            }

            //string houseQueryExtend = houseData[selectedHouse].street + " " + houseData[selectedHouse].postalCode + " " + houseData[selectedHouse].city + " Netherlands";
            ////await JSRuntime.InvokeAsync<Task>("SetChosenHouse", houseQueryExtend);
            //double[] coords = MappingService.GetPDOKinfoFromAddress(houseQueryExtend);
            ////await JSRuntime.InvokeAsync<Task>("SetChosenHouse", coords);
            //await JSRuntime.InvokeAsync<Task>("CheckAddressInsideIsochrone", coords);

            //await GetRouteWithMode(houseQueryExtend, place, "Driving");
            //await GetRouteWithMode(houseQueryExtend, place, "Walking");
            //await GetRouteWithMode(houseQueryExtend, place, "Transit");

            //routesCalculated = true;

            await SwitchLayer();
        }

        protected async Task PlotRoute(string mode)
        {
            //await Task.Run(() => Console.WriteLine(mode));
            await JSRuntime.InvokeAsync<Task>("PlotRoute", mode.ToLower());
        }

        protected async Task GetRoutes()
        {
            string houseQueryExtend = houseData[selectedHouse].street + " " + houseData[selectedHouse].postalCode + " " + houseData[selectedHouse].city + " Netherlands";
            //await JSRuntime.InvokeAsync<Task>("SetChosenHouse", houseQueryExtend);
            //await JSRuntime.InvokeAsync<Task>("SetChosenHouse", MappingService.GetPDOKinfoFromAddress(houseQueryExtend));
            await JSRuntime.InvokeAsync<Task>("SetChosenHouse", addressCoordinatesDictionary[houseData[selectedHouse].postalCode.Replace(" ", "")]);

            await GetRouteWithMode(houseQueryExtend, place, "Driving");
            await GetRouteWithMode(houseQueryExtend, place, "Walking");
            await GetRouteWithMode(houseQueryExtend, place, "Transit");

            routesCalculated = true;
        }        
    }
}
