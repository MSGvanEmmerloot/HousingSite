using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HousingSite.Data.HouseSites;
using HtmlAgilityPack;

namespace HousingSite.Data
{
    public class HouseTableService
    {
        public List<HouseData> houseData = new List<HouseData>();

        HousingSiteHandler_Pararius handlerPararius;
        HousingSiteHandler_Huurwoningen handlerHuurwoningen;

        //List<IHousingSiteHandler> hList = new List<IHousingSiteHandler>();
        public Dictionary<string, IHousingSiteHandler> housingSites = new Dictionary<string, IHousingSiteHandler>();
        public List<bool> sitesChecked = new List<bool>();

        public string city = "Nijmegen";
        public int priceMin = 200;
        public int priceMax = 800;
        public int area = 0;
        public int bedrooms = 0;

        //public int priceMin = 150;
        //public int priceMax = 1100;
        //public int area = 5;
        //public int bedrooms = 1;

        public HouseTableService()
        {
            handlerPararius = new HousingSiteHandler_Pararius(city, priceMin, priceMax, area, bedrooms);
            handlerHuurwoningen = new HousingSiteHandler_Huurwoningen(city, priceMin, priceMax, area, bedrooms);

            housingSites.Clear();
            housingSites.Add("Pararius", handlerPararius);
            housingSites.Add("Huurwoningen.nl", handlerHuurwoningen);

            sitesChecked.Clear();
            for(int h=0; h< housingSites.Count; h++)
            {
                sitesChecked.Add(true);
            }

            Console.WriteLine(housingSites.Count + " " + sitesChecked.Count);
        }

        public async Task SetSearchParams(string searchCity, int minimumPrice = 0, int maximumPrice = 0, int minimumArea = 0, int minimumNumBedrooms = 0)
        {
            await Task.Run(() => SetParams(searchCity, minimumPrice, maximumPrice, minimumArea, minimumNumBedrooms));
        }

        private void SetParams(string searchCity, int minimumPrice = 0, int maximumPrice = 0, int minimumArea = 0, int minimumNumBedrooms = 0)
        {
            city = searchCity;
            priceMin = minimumPrice;
            priceMax = maximumPrice;
            area = minimumArea;
            bedrooms = minimumNumBedrooms;
        }

        public async Task RunAsync(int site)
        {
            await Task.Run(() => Run(site));
        }

        private void Run(int site)
        {
            Console.WriteLine("Handling site " + site + "("+ housingSites.Keys.ElementAt(site) + ")");
            IHousingSiteHandler currentHousingSiteHandler = housingSites.Values.ElementAt(site);
            currentHousingSiteHandler.SetParams(city, priceMin, priceMax, area, bedrooms);
            currentHousingSiteHandler.ParsePage();
            ConcatHouseData(currentHousingSiteHandler);
        }

        private void ConcatHouseData(IHousingSiteHandler siteHandler)
        {
            foreach(HouseData house in siteHandler.houseData)
            {
                houseData.Add(house);
            }
        }

        public async Task<List<HouseData>> GetHouseData()
        {
            return await Task.FromResult(ReturnHouseData());
        }

        private List<HouseData> ReturnHouseData()
        {
            return houseData;
        }
    }
}
