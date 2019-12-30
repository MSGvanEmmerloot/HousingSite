using HousingSite.Data.Helper;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HousingSite.Data.HouseSites
{
    public class HousingSiteHandler_Pararius : IHousingSiteHandler
    {
        public List<HouseData> houseData { get; set; }

        public List<int> priceOptions { get; set; }
        public List<int> roomOptions { get; set; }
        public List<int> areaOptions { get; set; }

        public string city { get; set; }
        public int priceMin { get; set; }
        public int priceMax { get; set; }
        public int area { get; set; }
        public int bedrooms { get; set; }

        public string pagePrefix { get; set; }
        public string pageLink { get; set; }

        public HousingSiteHandler_Pararius(string searchCity, int minimumPrice = 0, int maximumPrice = 0, int minimumArea = 0, int minimumNumBedrooms = 0)
        {
            pagePrefix = "https://pararius.nl";
            priceOptions = new List<int> { 0, 200, 300, 400, 500, 600, 700, 800, 900, 1000, 1100, 1200, 1300, 1400, 1500, 1750, 2000, 2250, 2500, 3000, 4000, 6000 };
            roomOptions = new List<int> { 0, 1, 2, 3, 4, 5 };
            areaOptions = new List<int> { 0, 25, 50, 75, 100, 125, 150, 200 };

            SetParams(searchCity, minimumPrice, maximumPrice, minimumArea, minimumNumBedrooms);
        }

        public void SetParams(string searchCity, int minimumPrice = 0, int maximumPrice = 0, int minimumArea = 0, int minimumNumBedrooms = 0)
        {
            Console.WriteLine("Setting parameters of Pararius");
            city = searchCity;
            priceMin = priceOptions.FindSmallestAboveMinimum(minimumPrice);
            priceMax = priceOptions.FindBiggestBelowMaximum(maximumPrice);
            area = areaOptions.FindSmallestAboveMinimum(minimumArea);
            bedrooms = roomOptions.FindSmallestAboveMinimum(minimumNumBedrooms);

            StringBuilder sb = new StringBuilder();
            sb.Append(string.Format("https://pararius.nl/huurwoningen/{0}/{1}-{2}", city.ToLower(), priceMin, priceMax));
            if (area != 0) { sb.Append("/" + area + "m2"); }
            if (bedrooms != 0) { sb.Append("/"+bedrooms+"-slaapkamers"); }
            pageLink = sb.ToString();

            //Console.WriteLine("minimum price was " + minimumPrice + ", but is now " + priceMin);
            //Console.WriteLine("maximum price was " + maximumPrice + ", but is now " + priceMax);
            //Console.WriteLine("minimum area was " + minimumArea + ", but is now " + area);
            //Console.WriteLine("minimum amount of bedrooms was " + minimumNumBedrooms + ", but is now " + bedrooms);
            //Console.WriteLine("New pagelink: \"" + pageLink + "\"");
        }

        public void ParsePage()
        {
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(pageLink);
            if(doc == null) { return; }

            if (doc.DocumentNode.SelectSingleNode("//ul[@class='search-results-list']") == null)
            {
                //Console.WriteLine("No results!");
                return;
            }

            HtmlNodeCollection houseCollection = doc.DocumentNode.SelectSingleNode("//ul[@class='search-results-list']")
                                                    .SelectNodes(".//li[@class='property-list-item-container ']");
            if(houseCollection == null) { return; }

            List<HtmlNode> houses = houseCollection.ToList();

            if (houses.Count < 1)
            {
                //Console.WriteLine("No houses found");
                return;
            }

            if(houseData == null)
            {
                houseData = new List<HouseData>();
            }
            else houseData.Clear();

            HtmlNode pagination = doc.DocumentNode.SelectSingleNode("//ul[@class='pagination']");

            if (pagination != null)
            {
                HtmlNode lastPage = pagination.SelectSingleNode("./li[@class='last']").SelectSingleNode(".//a");
                string[] splitLink = lastPage.Attributes["href"].Value.Split('-');
                int pageCount = Int32.Parse(splitLink[splitLink.Length-1]);
                //Console.WriteLine(pageCount + " pages");
                for (int p = 0; p < pageCount; p++)
                {
                    if (p > 0)
                    {
                        HtmlDocument doc2 = web.Load(pageLink + "/page-" + (p + 1));
                        if (doc2 == null) { continue; }
                        //if (doc2.DocumentNode.SelectSingleNode("//ul[@class='search-results-list']") != null)
                        //{
                        HtmlNodeCollection houses2Collection = doc2.DocumentNode.SelectSingleNode("//ul[@class='search-results-list']")
                                                        .SelectNodes(".//li[@class='property-list-item-container ']");
                        if(houses2Collection == null) { continue; }
                        //List<HtmlNode> houses2 = doc2.DocumentNode.SelectSingleNode("//ul[@class='search-results-list']")
                                                    //.SelectNodes(".//li[@class='property-list-item-container ']").ToList();
                        List<HtmlNode> houses2 = houses2Collection.ToList();
                        for (int h = 0; h < houses2.Count; h++)
                        {
                            houses.Add(houses2[h]);
                        }
                        Console.WriteLine("houses now contains " + houses.Count);
                        //}
                    }
                }
            }

            //ParseHouse(siteName, houses[0]);
            foreach (HtmlNode house in houses)
            {
                ParseHouse(house);
                //Console.WriteLine("houseData list now contains " + houseData.Count + " houses.");
            }

            //for(int i=0; i<5; i++)
            //{
            //    ParseHouse(siteName, houses[i]);
            //    Console.WriteLine("houseData list now contains " + houseData.Count + " houses.");
            //}
        }

        public void ParseHouse(HtmlNode house)
        {
            HtmlNode details = house.SelectSingleNode(".//div[@class='details']");
            HtmlNode a = details.SelectSingleNode(".//h2/a");

            string href = a.Attributes["href"].Value;
            string houseTitle = a.InnerText.Trim().Replace("  ", "").Replace("\n", " ");
            string houseUrl = pagePrefix + href;

            HouseData houseData = new HouseData();
            houseData.title = houseTitle;
            houseData.siteName = "Pararius";
            houseData.url = houseUrl;

            Console.WriteLine(houseTitle + " (" + houseUrl + ")");
            ParseHousePage(houseUrl, houseData);
            Console.WriteLine();
        }

        public void ParseHousePage(string houseLink, HouseData house = null)
        {
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(houseLink);
            if (doc == null) { return; }

            HtmlNode content = doc.DocumentNode.SelectSingleNode("//div[@class='content-container']");
            if (content == null) { return; }

            HtmlNode dataList = content.SelectSingleNode(".//dl");
            HtmlNodeCollection labelCollection = dataList.SelectNodes("dt");
            HtmlNodeCollection valuesCollection = dataList.SelectNodes("dd");
            if (labelCollection == null || valuesCollection == null) { return; }
            List<HtmlNode> labels = dataList.SelectNodes("dt").ToList();
            List<HtmlNode> values = dataList.SelectNodes("dd").ToList();

            HtmlNodeCollection featuresCollection = content.SelectSingleNode(".//ul[@class='features']").SelectNodes("li");
            if (featuresCollection == null) { return; }
            List<HtmlNode> features = featuresCollection.ToList();

            string neighbourhoodText = "";
            string postalcodeText = "";
            string streetText = "";
            string areaText = "";
            string priceText = "";
            string availableText = "";
            string bedroomsText = "";
            string placedText = "";
            string extraText = "";

            if (house == null)
            {
                house = new HouseData();
            }
            house.city = city;

            for (int l = 0; l < labels.Count; l++)
            {
                string labelText = labels[l].InnerText.Trim();

                switch (labelText)
                {
                    case "Buurt":
                        neighbourhoodText = values[l].InnerText;
                        house.neighbourhood = neighbourhoodText;
                        break;
                    case "Postcode":
                        postalcodeText = values[l].InnerText;
                        house.postalCode = postalcodeText;
                        break;
                    case "Straat":
                        streetText = values[l].InnerText;
                        house.street = streetText;
                        break;
                    case "Oppervlakte (m²)":
                        areaText = values[l].InnerText.Replace("&sup2;", "2");//²
                        house.area = areaText;
                        break;
                    case "Huurprijs per maand":
                        priceText = values[l].InnerText.Replace("€", "").Replace(",-", " euro").Replace(".","").Trim();
                        house.price = priceText;
                        break;
                    case "Beschikbaar per":
                        availableText = values[l].InnerText;
                        house.available = availableText;
                        break;
                    case "Aantal slaapkamers":
                        bedroomsText = values[l].InnerText;
                        house.bedrooms = bedroomsText;
                        break;
                    case "Aangeboden sinds":
                        placedText = values[l].InnerText.Replace("&gt;", "meer dan"); //> 
                        house.placed = placedText;
                        break;
                }
            }

            for (int f = 0; f < features.Count; f++)
            {
                extraText += "[" + features[f].InnerText.Trim() + "]";
            }

            house.extra = extraText;
            houseData.Add(house);

            Console.WriteLine(streetText + ", " + postalcodeText + ", " + neighbourhoodText + "\n" +
                                priceText + " voor " + areaText + " met " + bedroomsText + " slaapkamer(s)\n" +
                                "Beschikbaar vanaf " + availableText + ", aangeboden sinds " + placedText + "\n" +
                                "Extra info: " + extraText);
        }
    }
}
