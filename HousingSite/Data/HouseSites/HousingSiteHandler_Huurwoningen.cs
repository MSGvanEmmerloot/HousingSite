using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HousingSite.Data.Helper;
using System.Text;

namespace HousingSite.Data.HouseSites
{
    public class HousingSiteHandler_Huurwoningen : IHousingSiteHandler
    {
        public List<HouseData> houseData { get; set; }

        public List<int> priceOptions { get; set; }
        public List<int> roomOptions  { get; set; }
        public List<int> areaOptions { get; set; }

        public string city { get; set; }
        public int priceMin { get; set; }
        public int priceMax { get; set; }
        public int area { get; set; }
        public int bedrooms { get; set; }

        public string pagePrefix { get; set; }
        public string pageLink { get; set; }

        public HousingSiteHandler_Huurwoningen(string searchCity, int minimumPrice = 0, int maximumPrice = 0, int minimumArea = 0, int minimumNumBedrooms = 0)
        {
            pagePrefix = "https://huurwoningen.nl";
            priceOptions = new List<int> { 0, 100, 200, 300, 400, 500, 600, 700, 800, 900, 1000, 1250, 1500, 1750, 2000, 2500, 3000, 3500 };
            roomOptions = new List<int> { 0, 1, 2, 3, 4, 5 };
            areaOptions = new List<int> { 0, 10, 50, 75, 100, 125, 150, 200 };

            SetParams(searchCity, minimumPrice, maximumPrice, minimumArea, minimumNumBedrooms);
        }

        public void SetParams(string searchCity, int minimumPrice = 0, int maximumPrice = 0, int minimumArea = 0, int minimumNumBedrooms = 0)
        {
            Console.WriteLine("Setting parameters of Huurwoningen");
            city = searchCity;
            priceMin = priceOptions.FindSmallestAboveMinimum(minimumPrice);            
            priceMax = priceOptions.FindBiggestBelowMaximum(maximumPrice);            
            area = areaOptions.FindSmallestAboveMinimum(minimumArea);            
            bedrooms = roomOptions.FindSmallestAboveMinimum(minimumNumBedrooms);

            StringBuilder sb = new StringBuilder();
            sb.Append("https://huurwoningen.nl/in/").Append(city.ToLower()).Append("/?");
            if(priceMin != 0) { sb.Append("min_price=" + priceMin).Append("&"); }
            if (priceMax != 0) { sb.Append("max_price=" + priceMax).Append("&"); }
            if (area != 0) { sb.Append("size=" + area).Append("&"); }
            if (bedrooms != 0) { sb.Append("number_of_bedrooms=" + bedrooms).Append("5&"); }
            pageLink = sb.ToString().TrimEnd('&').TrimEnd('?');

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

            HtmlNodeCollection houseCollection = doc.DocumentNode.SelectSingleNode("//div[@id='listings']").
                                                     SelectNodes(".//section[@class='listing__content']");
            if (houseCollection == null) { return; }
            
            List<HtmlNode> houses = houseCollection.ToList();

            if (houses.Count < 1)
            {
                //Console.WriteLine("No houses found");
                return;
            }

            if (houseData == null)
            {
                houseData = new List<HouseData>();
            }
            else houseData.Clear();

            HtmlNode pagination = doc.DocumentNode.SelectSingleNode("//div[@id='pagination']");

            if (pagination.SelectSingleNode(".//ul") != null)
            {
                List<HtmlNode> pages = doc.DocumentNode.SelectSingleNode("//ul[@class='pagination pagination--search']").
                                                        SelectNodes("./li").ToList();

                int pageCount = Int32.Parse(pages[pages.Count - 2].InnerText.Trim());
                //Console.WriteLine(pageCount);
                for (int p=0; p < pageCount; p++)
                {
                    if (p > 0)
                    {
                        HtmlDocument doc2 = web.Load(pageLink + "&page=" + (p+1));
                        if (doc2 == null) { continue; }

                        HtmlNodeCollection houses2collection = doc2.DocumentNode.SelectSingleNode("//div[@id='listings']").
                                                                  SelectNodes(".//section[@class='listing__content']");
                        if(houses2collection == null) { continue; }
                        //List<HtmlNode> houses2 = doc2.DocumentNode.SelectSingleNode("//div[@id='listings']").
                        //                                          SelectNodes(".//section[@class='listing__content']").ToList();
                        List<HtmlNode> houses2 = houses2collection.ToList();
                        for (int h=0; h < houses2.Count; h++)
                        {
                            houses.Add(houses2[h]);
                        }
                        Console.WriteLine("houses now contains " + houses.Count);
                    }                    
                }
            }

            //ParseHouse(houses[0]);

            foreach (HtmlNode house in houses)
            {
                ParseHouse(house);
                //Console.WriteLine("houseData list now contains " + houseData.Count + " houses.");
            }

            //for (int i = 0; i < 5; i++)
            //{
            //    ParseHouse(houses[i]);
            //    Console.WriteLine("houseData list now contains " + houseData.Count + " houses.");
            //}
        }

        public void ParseHouse(HtmlNode house)
        {
            HtmlNode details = house.SelectSingleNode(".//a[@class='listing__link']");
            HtmlNode head = details.SelectSingleNode(".//h2");

            string href = details.Attributes["href"].Value;
            string houseTitle = head.InnerText.Trim().Replace("  ", "").Replace("\n", " ");
            string houseUrl = pagePrefix + href;

            HouseData houseData = new HouseData();
            string houseTitleText = "";
            string[] titleParts = houseTitle.Split(' ');
            for (int t = 0; t < titleParts.Length; t++)
            {
                if(titleParts[t].ToLower() == city.ToLower()) { continue; }
                houseTitleText += titleParts[t].Trim() + " ";
            }
            houseData.title = houseTitleText.Trim();
            houseData.siteName = "Huurwoningen.nl";
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

            string neighbourhoodText = "";
            string postalcodeText = "";
            string streetText = "";
            string areaText = "";
            string priceText = "?";
            string availableText = "";
            string bedroomsText = "";
            string placedText = "";
            string extraText = "";

            if (house == null)
            {
                house = new HouseData();
            }
            house.city = city;            

            HtmlNode header = doc.DocumentNode.SelectSingleNode("//div[@class='listing-view-header']");
            string fullTitle = header.SelectSingleNode(".//h1[@class='title__listing']").InnerText;
            string[] titleParts = fullTitle.Split(' ');
            for(int t=0; t < titleParts.Length; t++)
            {
                if (t == titleParts.Length - 3)
                {
                    streetText += titleParts[t];
                }
                else if(t>0 && t< titleParts.Length - 2)
                {
                    streetText += titleParts[t] + " ";
                }
            }
            house.street = streetText;

            string fullAddress = header.SelectSingleNode(".//p[@class='title__sub']").InnerText;
            string[] addressParts = fullAddress.Split(' ');
            postalcodeText = addressParts[0] + " " + addressParts[1];
            house.postalCode = postalcodeText;

            priceText = header.SelectSingleNode(".//span[@class='price__value']").InnerText;
            house.price = priceText.Replace("€", "").Replace(".", "");

            HtmlNode properties = doc.DocumentNode.SelectSingleNode("//div[@class='listing-view-properties']").
                                                   SelectSingleNode(".//dl");
            //areaText = properties.SelectSingleNode(".//dd[@class='properties__property properties__property--size']").
            //                      SelectNodes(".//span")[1].InnerText;
            //house.area = areaText.Replace("&sup2;", "2");
            HtmlNode sizeNode = properties.SelectSingleNode(".//dd[@class='properties__property properties__property--size']");
            if (sizeNode != null)
            {
                areaText = sizeNode.SelectNodes(".//span")[1].InnerText;
                house.area = areaText.Replace("&sup2;", "2");
            }

            HtmlNode bedroomsNode = properties.SelectSingleNode(".//dd[@class='properties__property properties__property--number-of-rooms']");
            if (bedroomsNode != null)
            {
                bedroomsText = bedroomsNode.SelectNodes(".//span")[1].InnerText;
                house.bedrooms = bedroomsText.Split(' ')[0];
            }
            //if (properties.SelectSingleNode(".//dd[@class='properties__property properties__property--number-of-rooms']") != null)
            //{
            //    bedroomsText = properties.SelectSingleNode(".//dd[@class='properties__property properties__property--number-of-rooms']").
            //                              SelectNodes(".//span")[1].InnerText;
            //    house.bedrooms = bedroomsText.Split(' ')[0];
            //}

            HtmlNodeCollection detailsContainer = doc.DocumentNode.SelectSingleNode(".//div[@class='listing-view__details']").
                                                      SelectSingleNode(".//div[@class='listing-view__main']").
                                                      SelectNodes(".//section[@class='widget widget--details']");
            if(detailsContainer == null) { return; }
            List<HtmlNode> details = detailsContainer.ToList();

            for(int d=0; d< details.Count; d++)
            {
                string headerText = details[d].SelectSingleNode(".//section[@class='widget__header']").
                                               SelectSingleNode(".//h3[@class='widget__heading']").InnerText;
                List<HtmlNode> props;
                switch (headerText)
                {
                    case "Extra informatie":
                        HtmlNode propsContainer = details[d].SelectSingleNode(".//section[@class='widget__body']").
                                           SelectSingleNode(".//dl[@class='properties properties--checkmark']");
                        HtmlNodeCollection propertyContainer = propsContainer.SelectNodes(".//dd[@class='properties__property']");
                        if (propertyContainer != null)
                        {
                            props = propertyContainer.ToList();                        

                            for (int p = 0; p < props.Count; p++)
                            {
                                extraText += "[" + props[p].InnerText.Trim() + "]";
                            }
                            house.extra = extraText;
                        }
                        //if (propsContainer.SelectNodes(".//dd[@class='properties__property']") != null)
                        //{
                        //    props = propsContainer.SelectNodes(".//dd[@class='properties__property']").ToList();

                        //    for (int p = 0; p < props.Count; p++)
                        //    {
                        //        extraText += "[" + props[p].InnerText.Trim() + "]";
                        //    }
                        //    house.extra = extraText;
                        //}
                        break;
                    case "Verhuurinformatie":
                        propsContainer = details[d].SelectSingleNode(".//section[@class='widget__body']").
                                                           SelectSingleNode(".//dl[@class='properties properties--list']");
                        HtmlNodeCollection labelContainer = propsContainer.SelectNodes(".//dt[@class='properties__label']");
                        //List<HtmlNode> labels = details[d].SelectSingleNode(".//section[@class='widget__body']").
                        //                                   SelectSingleNode(".//dl[@class='properties properties--list']").
                        //                                   SelectNodes(".//dt[@class='properties__label']").ToList();
                        //props = details[d].SelectSingleNode(".//section[@class='widget__body']").
                        //                   SelectSingleNode(".//dl[@class='properties properties--list']").
                        //                   SelectNodes(".//dd[@class='properties__property']").ToList();
                        if (labelContainer != null)
                        {
                            List<HtmlNode> labels = labelContainer.ToList();
                            props = propsContainer.SelectNodes(".//dd[@class='properties__property']").ToList();                        

                            for (int l = 0; l < labels.Count; l++)
                            {
                                if(labels[l].InnerText == "Beschikbaar per")
                                {
                                    availableText = props[l].InnerText;
                                    house.available = availableText;
                                }
                            }
                        }
                        break;
                }
            }
            
            houseData.Add(house);

            Console.WriteLine(streetText + ", " + postalcodeText + ", " + neighbourhoodText + "\n" +
                                priceText + " voor " + areaText + " met " + bedroomsText + " slaapkamer(s)\n" +
                                "Beschikbaar vanaf " + availableText + ", aangeboden sinds " + placedText + "\n" +
                                "Extra info: " + extraText);
        }
    }
}
