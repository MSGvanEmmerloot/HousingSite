using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HousingSite.Data.HouseSites
{
    public interface IHousingSiteHandler
    {
        List<HouseData> houseData { get; set; }
        List<int> priceOptions { get; set; }
        List<int> roomOptions { get; set; }
        List<int> areaOptions { get; set; }

        string city { get; set; }
        int priceMin { get; set; }
        int priceMax { get; set; }
        int area { get; set; }
        int bedrooms { get; set; }

        string pagePrefix { get; set; }
        string pageLink { get; set; }

        // Sets the search parameters
        void SetParams(string searchCity, int minimumPrice = 0, int maximumPrice = 0, int minimumArea = 0, int minimumNumBedrooms = 0);

        // Parses the webpage with results to find house entries
        void ParsePage();

        // Parses a single house on the result page
        void ParseHouse(HtmlNode house);

        // Parses the webpage of the house
        void ParseHousePage(string houseLink, HouseData house = null);
    }
}
