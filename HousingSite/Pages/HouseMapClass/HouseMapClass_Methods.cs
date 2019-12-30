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

namespace HousingSite.Pages
{
    public partial class HouseMapClass : ComponentBase
    {
        private void SetLastParams(string city, int priceMin, int priceMax, int area, int bedrooms)
        {
            lastSearchParams.city = city;
            lastSearchParams.priceMin = priceMin;
            lastSearchParams.priceMax = priceMax;
            lastSearchParams.area = area;
            lastSearchParams.bedrooms = bedrooms;
        }

        private bool CheckIfSameParams()
        {
            return lastSearchParams.city == city && lastSearchParams.priceMin == priceMin && lastSearchParams.priceMax == priceMax && lastSearchParams.area == area && lastSearchParams.bedrooms == bedrooms;
        }

        #region House updating
        protected async Task RefreshVisibleHouses()
        {
            //await JSRuntime.InvokeAsync<Task>("ClearHouses");
            //housesAdded = 0;

            await CheckHouseDistances();
            //await AddColorPinsToHousesInRadius();
            await ApplyHouseFilter();
        }

        protected async Task ApplyHouseFilter()
        {
            switch (selectedHouseFilter)
            {
                case HouseFilters.circle:
                    await ShowHousesInCircle();
                    Console.WriteLine("Showing houses inside circle");
                    break;
                case HouseFilters.isochrone:
                    await ShowHousesInIsochrone();
                    Console.WriteLine("Showing houses inside isochrone");
                    break;
                case HouseFilters.all:
                default:
                    await ShowAllHouses();
                    Console.WriteLine("Showing all houses");
                    break;
            }

            await JSRuntime.InvokeAsync<Task>("CheckHousePins");
        }

        protected async Task AddColorPinsToAllHouses()
        {
            progress3Max = houseData.Count;
            for (int h = 0; h < houseData.Count; h++)
            {
                string extendedDescription = houseData[h].street + ", " + houseData[h].postalCode + ", " + houseData[h].neighbourhood + "\n" +
                                houseData[h].price + " voor " + houseData[h].area;
                //progressValue3 = (h / houseData.Count) * 100;
                //progressValue1Max = HouseService.sitesChecked.Count;
                
                await JSRuntime.InvokeAsync<Task>("AddColorHousetoAddress", addressCoordinatesDictionary[houseData[h].postalCode.Replace(" ", "")], "House " + h, extendedDescription, GetHouseColor(houseData[h]), h);
                progress3Value = h+1;
                //progress3Fill = (progress3Max == 0) ? 0 : (progress3Value / progress3Max) * 100;
                progress3Fill = (progress3Max == 0) ? 0 : (progress3Value * 100 / progress3Max);
                this.StateHasChanged();
            }
        }

        protected async Task ShowAllHouses()
        {
            await JSRuntime.InvokeAsync<Task>("ShowAllHousePins");
        }
        protected async Task ShowHousesInCircle()
        {
            for (int h = 0; h < houseData.Count; h++)
            {
                await JSRuntime.InvokeAsync<Task>("ShowHouseInsideCircle", h, addressCoordinatesDictionary[houseData[h].postalCode.Replace(" ", "")]);
            }
        }
        protected async Task ShowHousesInIsochrone()
        {
            for (int h = 0; h < houseData.Count; h++)
            {
                await JSRuntime.InvokeAsync<Task>("ShowHouseInsideIsochrone", h, addressCoordinatesDictionary[houseData[h].postalCode.Replace(" ", "")]);
            }
        }

        protected async Task AddColorPinsToHousesInRadius()
        {
            //Console.WriteLine("Houses already added: " + housesAdded + "/" + houseData.Count);
            for (int h = 0; h < houseData.Count; h++)
            {
                //if (h < housesAdded) { continue; }
                if (housesInRadius.Count < h || !housesInRadius[h]) { continue; }

                string houseQuery = houseData[h].street + " " + houseData[h].postalCode;

                string extendedDescription = houseData[h].street + ", " + houseData[h].postalCode + ", " + houseData[h].neighbourhood + "\n" +
                                houseData[h].price + " voor " + houseData[h].area;

                //string houseColor = "white";
                //int priceRange = priceMax - priceMin;
                //double priceStep = priceRange / 3;
                //double housePrice = double.Parse(Regex.Match(houseData[h].price, @"\d+").Value);
                //Console.WriteLine("House " + h + " price: " + housePrice);

                //if (priceRange > 0)
                //{
                //    if (housePrice < (priceMin + priceStep))
                //    {
                //        Console.WriteLine(housePrice + " < " + (priceMin + priceStep));
                //        houseColor = "green";
                //    }
                //    else if (housePrice < (priceMin + (2 * priceStep)))
                //    {
                //        Console.WriteLine(housePrice + " < " + (priceMin + (2 * priceStep)));
                //        houseColor = "yellow";
                //    }
                //    else
                //    {
                //        Console.WriteLine(housePrice + " > " + (priceMin + (2 * priceStep)));
                //        houseColor = "red";
                //    }
                //}
                //Console.WriteLine("House " + h + " color: " + houseColor);
                string houseColor = GetHouseColor(houseData[h]);

                //await JSRuntime.InvokeAsync<Task>("AddColorHousetoAddress", houseQuery, "House " + h, extendedDescription, houseColor); 
                //await JSRuntime.InvokeAsync<Task>("AddColorHousetoAddress", MappingService.GetPDOKinfoFromAddress(houseQuery), "House " + h, extendedDescription, houseColor);
                await JSRuntime.InvokeAsync<Task>("AddColorHousetoAddress", addressCoordinatesDictionary[houseData[h].postalCode.Replace(" ", "")], "House " + h, extendedDescription, houseColor, h);
            }
        }
        
        private string GetHouseColor(HouseData house)
        {
            string houseColor = "white";
            int priceRange = priceMax - priceMin;
            double priceStep = priceRange / 3;
            double housePrice = double.Parse(Regex.Match(house.price, @"\d+").Value);
            //Console.WriteLine("House " + h + " price: " + housePrice);

            if (priceRange > 0)
            {
                if (housePrice < (priceMin + priceStep))
                {
                    Console.WriteLine(housePrice + " < " + (priceMin + priceStep));
                    houseColor = "green";
                }
                else if (housePrice < (priceMin + (2 * priceStep)))
                {
                    Console.WriteLine(housePrice + " < " + (priceMin + (2 * priceStep)));
                    houseColor = "yellow";
                }
                else
                {
                    Console.WriteLine(housePrice + " > " + (priceMin + (2 * priceStep)));
                    houseColor = "red";
                }
            }
            //Console.WriteLine("House " + h + " color: " + houseColor);
            return houseColor;
        }
                
        protected async Task AddHousesToQueue()
        {
            progress2Max = houseData.Count;
            for (int h = 0; h < houseData.Count; h++)
            {
                if (h < housesAdded) { continue; }
                //progressValue2 = (h / houseData.Count) * 100;
                
                Console.WriteLine("Postal code: " + houseData[h].postalCode);
                string key = houseData[h].postalCode.Replace(" ", "");
                Console.WriteLine("Key: " + key);
                //string value = houseData[h].street + " " + houseData[h].postalCode + " " + houseData[h].city + " Netherlands";
                //Console.WriteLine("Value: " + value);

                if (!addressCoordinatesDictionary.ContainsKey(key))
                {
                    if (!addressQueue.ContainsKey(key))
                    {
                        string value = houseData[h].street + " " + houseData[h].postalCode + " " + houseData[h].city + " Netherlands";
                        Console.WriteLine("Value: " + value);
                        addressQueue.Add(key, value);
                    }
                }

                if(addressQueue.Count == 25)
                {
                    await UpdateDictionary();
                }
                progress2Value = h+1;
                progress2Fill = (progress2Max == 0) ? 0 : (progress2Value * 100 / progress2Max);
                //progress2Fill = (progress2Max == 0) ? 0 : (progress2Value / progress2Max) * 100;
                this.StateHasChanged();
            }
        }
        
        protected async Task UpdateDictionary()
        {
            if(addressQueue.Count == 0) { return; }
            Console.WriteLine(addressQueue.Count + " houses to be retrieved");
            //string[] s = new string[25];
            //addressQueue.Values.CopyTo(s, 0);
            List<string> addressList = new List<string>();// s.ToList();
            for(int a=0; a<addressQueue.Count; a++)
            {
                addressList.Add(addressQueue.Values.ElementAt(a));
            }

            for(int i=0; i< addressList.Count; i++)
            {
                Console.WriteLine(i + ") " + addressList[i]);
            }

            List<double[]> coordinates = await MappingService.GetLocationsWithRouteTask(addressList);
            transactions++;

            if (coordinates == null) { return; }

            for (int i = 0; i < coordinates.Count; i++)
            {
                if (addressQueue.Count < i) { return; }
                if (coordinates[i] == null || addressQueue.Keys.ElementAt(i) == null) { return; }

                addressCoordinatesDictionary.Add(addressQueue.Keys.ElementAt(i), coordinates[i]);
                Console.WriteLine("Added " + addressQueue.Keys.ElementAt(i) + " [" + coordinates[i][0] + " " + coordinates[i][1] + "] to dictionary");
            }
            addressQueue.Clear();
        }

        protected async Task CheckHouseDistances()
        {
            for (int h = 0; h < houseData.Count; h++)
            {
                //if (h < housesAdded) { continue; }

                bool inRadius = await CheckHouseInRadius(h);
                if (h < housesInRadius.Count)
                {
                    housesInRadius[h] = inRadius;
                }
                else housesInRadius.Add(inRadius);
            }
        }

        protected async Task<bool> CheckHouseInRadius(int h)
        {
            string houseQueryExtend = houseData[h].street + " " + houseData[h].postalCode + " " + houseData[h].city + " Netherlands";

            //double[] d = await MappingService.GetCoordinatesAsync(houseQueryExtend); 
            //double[] p = await MappingService.GetCoordinatesAsync(place);

            //double[] d = MappingService.GetPDOKinfoFromAddress(houseQueryExtend);
            double[] d = addressCoordinatesDictionary[houseData[h].postalCode.Replace(" ", "")];
            //double[] p = MappingService.GetPDOKinfoFromAddress(place);
            //double[] p = addressCoordinatesDictionary[place];

            CheckBaseKey();

            //if (placeCoordinates == null)
            //{
            //    if (addressCoordinatesDictionary.ContainsKey("BaseAddress") && baseAddress.Key == place)
            //    {
            //        placeCoordinates = addressCoordinatesDictionary["BaseAddress"];
            //    }
            //    else
            //    {
            //        placeCoordinates = MappingService.GetPDOKinfoFromAddress(place);
            //        addressCoordinatesDictionary.Add("BaseAddress", placeCoordinates);
            //        baseAddress = new KeyValuePair<string, double[]>(place, placeCoordinates);
            //    }
            //}
            double dist = await MappingService.CalculateDistance(d, baseAddress.Value);

            if (dist < radius)
            {
                Console.WriteLine("House " + h + " is inside the radius! (" + dist + "km)");
                return true;
            }
            else
            {
                Console.WriteLine("*House " + h + " is NOT inside the radius! (" + dist + "km)");
                return false;
            }
        }
        #endregion

        protected async Task AddToSpecificLayer()
        {
            string houseColor = GetHouseColor(houseData[selectedHouse]);
            string houseQuery = houseData[selectedHouse].street + " " + houseData[selectedHouse].postalCode;
            Console.WriteLine("Adding to specific layer.");
            await JSRuntime.InvokeAsync<Task>("AddToSpecificLayer", addressCoordinatesDictionary[houseData[selectedHouse].postalCode.Replace(" ", "")], "House", baseAddress.Value, "Base", houseColor);

            //Console.WriteLine("Testy testy");
            //double[] coords = addressCoordinatesDictionary[houseData[selectedHouse].postalCode.Replace(" ", "")];
            //if (coords != null)
            //{
            //    Console.WriteLine("Coords: " + coords[0] + " " + coords[1]);
            //    await JSRuntime.InvokeAsync<Task>("CheckAddressInsideIsochrone", addressCoordinatesDictionary[houseData[selectedHouse].postalCode.Replace(" ", "")]);
            //}
        }

        protected async Task GetRouteWithMode(string addressA, string addressB, string mode)
        {
            int[] res = await MappingService.GetRouteAsyncWithMode(addressA, addressB, mode);

            StringBuilder sb = new StringBuilder();
            if (res != null && res.Length == 3)
            {
                Console.WriteLine(mode);
                Console.WriteLine("Duration without traffic: " + res[0] + " -> " + DurationFromSeconds(res[0]));

                if (mode != "Driving")
                {
                    sb.Append("Travel time: +/- " + DurationFromSeconds(res[0]));
                }
                else sb.Append("Travel time (without traffic): +/- " + DurationFromSeconds(res[0]));

                if (res[2] != 0)
                {
                    Console.WriteLine("Distance: " + res[2] + "km");
                    sb.Append("\nTravel Distance: +/- " + res[2] + " km");
                }
                Console.WriteLine();
            }

            string resultText = sb.ToString();

            switch (mode)
            {
                case "Driving":
                    drivingRouteText = resultText;
                    break;
                case "Transit":
                    transitRouteText = resultText;
                    break;
                case "Walking":
                default:
                    walkingRouteText = resultText;
                    break;
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

            return durationHours + " h " + durationMinutes + " min.";
        }

        #region Synchronization functions (polling from maps.ts)
        // Polls sessionKey until it's set and then passes this variable to MappingService
        private async void SyncSessionKey()
        {
            if (sessionKey == "" || sessionKey == null)
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

        public void SetSessionKey(string key)
        {
            sessionKey = key;
            if (MappingService.sessionKey != sessionKey)
            {
                MappingService.sessionKey = sessionKey;
                Console.WriteLine("\nSet sessionKey of MappingService: " + sessionKey);
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
        #endregion

    }
}
