using HousingSite.Data;
using HousingSite.Data.Helper;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace HousingSite.Pages
{
    public class TestingRazorClass : ComponentBase
    {        
        [Inject]
        IJSRuntime JSRuntime { get; set; }
        [Inject]
        protected MapService MappingService { get; set; }

        protected enum TravelModes { none, walking, driving, transit };
        protected static string[] travelModeStrings = new string[4] { "", "Walking", "Driving", "Public transport" };
        protected TravelModes selectedTravelMode = TravelModes.none;

        protected enum CalculationModes { none, time, distance };
        protected static string[] calculationModeStrings = new string[3] { "", "Time", "Distance" };
        protected CalculationModes selectedCalculationMode = CalculationModes.none;

        protected float travelTime = 10;
        protected float travelDistance = 2;

        protected string[] userPinCoords = null;

        protected int progress1 = 0;
        protected int progress2 = 0;

        protected string sign1 = "";
        protected string sign2 = "";
        protected string sign3 = "";
        protected int borderVal1 = 0;
        protected int borderVal2 = 0;

        public enum ColorOptions { green, yellow, red };
        protected static string[] colorStrings = new string[3] { "Green", "Yellow", "Red" };
        public enum ConditionOptions { below, above, between };
        protected static string[] conditionStrings = new string[3] { "Below", "Above", "Between" };
        public enum RuleParamOptions { price, area };
        protected static string[] ruleParamStrings = new string[2] { "Renting price", "Area" };
        public class RuleParams
        {
            public RuleParamOptions parameter;
            public bool apply;
            public string sign1;
            public string sign2;
            public string sign3;
            public double borderVal1;
            public double borderVal2;            
        }
        public List<RuleParams> ruleParams;


        public delegate void ruleParamDelegate(RuleParams2 r, int i);
        public class RuleParams2
        {
            //public int sequenceNumber
            //{
            //    get { return sequenceNumber; }
            //    set { sequenceNumber = value; if (del != null) { del(this, value); } }
            //}
            public int sequenceNumber;
            public bool apply;
            public ColorOptions color;
            public RuleParamOptions parameter;
            public ConditionOptions condition;

            public double borderVal1;
            public double borderVal2;

            public ruleParamDelegate del;

            public override string ToString()
            {
                if (condition == ConditionOptions.between) { return "#" + sequenceNumber + ": apply = " + apply + ",  color = " + color + ", " + parameter + " " + condition + " " + borderVal1 + " and " + borderVal2; }
                else return "#" + sequenceNumber + ": apply = " + apply + ",  color = " + color + ", " + parameter + " " + condition + " " + borderVal1;
            }
        }
        public List<RuleParams2> ruleParams2;

        protected double testPrice = 400;
        protected double testArea = 10;
        protected ColorOptions defaultColor = ColorOptions.green;

        protected JsInterop jsInterop;

        public void ShiftRuleParamList(RuleParams2 modifiedRule, int newNum)
        {
            int curPos = -1;
            int swapPos = -1;
            Console.WriteLine("Invoked delegate with " + newNum);
            if (ruleParams2.Contains(modifiedRule))
            {
                int curNum = modifiedRule.sequenceNumber;
                Console.WriteLine("Current num: " + curNum);
                modifiedRule.sequenceNumber = newNum;
                RuleParams2 tempRule = modifiedRule;
                Console.WriteLine("Yeet " + modifiedRule.sequenceNumber);
                curPos = ruleParams2.IndexOf(modifiedRule);
                Console.WriteLine("Index of clicked row: " + curPos);
                
                foreach(RuleParams2 rule in ruleParams2.FindAll(s => s.sequenceNumber == newNum))
                {
                    Console.WriteLine(rule);
                    if(rule != modifiedRule)
                    {
                        Console.WriteLine("Woop, found a match!");
                        swapPos = ruleParams2.IndexOf(rule);
                        rule.sequenceNumber = curNum;
                        Console.WriteLine("Index of row to swap: " + swapPos);

                        ruleParams2[curPos] = rule;
                        ruleParams2[swapPos] = tempRule;
                        this.StateHasChanged();
                    }
                }  
            }

            for(int i=0; i< ruleParams2.Count; i++)
            {
                Console.WriteLine("[rule " + i + "] " + ruleParams2[i].ToString());
            }
        }

        public int ShiftRuleParamList2(RuleParams2 modifiedRule, int newNum)
        {
            int curPos = -1;
            int swapPos = -1;
            Console.WriteLine("Invoked delegate with " + newNum);
            if (ruleParams2.Contains(modifiedRule))
            {
                int curNum = modifiedRule.sequenceNumber;
                Console.WriteLine("Current num: " + curNum);
                modifiedRule.sequenceNumber = newNum;
                RuleParams2 tempRule = modifiedRule;
                Console.WriteLine("Yeet " + modifiedRule.sequenceNumber);
                curPos = ruleParams2.IndexOf(modifiedRule);
                Console.WriteLine("Index of clicked row: " + curPos);

                foreach (RuleParams2 rule in ruleParams2.FindAll(s => s.sequenceNumber == newNum))
                {
                    Console.WriteLine(rule);
                    if (rule != modifiedRule)
                    {
                        Console.WriteLine("Woop, found a match!");
                        swapPos = ruleParams2.IndexOf(rule);
                        rule.sequenceNumber = curNum;
                        Console.WriteLine("Index of row to swap: " + swapPos);

                        ruleParams2[curPos] = rule;
                        ruleParams2[swapPos] = tempRule;
                        this.StateHasChanged();
                    }
                }
            }

            for (int i = 0; i < ruleParams2.Count; i++)
            {
                Console.WriteLine("[rule " + i + "] " + ruleParams2[i].ToString());
            }

            return newNum;
        }

        protected async Task Testing()
        {
            Console.WriteLine("Testing button clicked");
            await CallJS();
        }

        protected async Task CallJS()
        {
            string s = await jsInterop.CallFunction1();
            Console.WriteLine("s = " + s);

            s = await jsInterop.CallFunction2();
            Console.WriteLine("s = " + s);
        }

        protected override async Task OnInitializedAsync()
        {
            jsInterop = new JsInterop(JSRuntime);

            ruleParams2 = new List<RuleParams2>();
            ruleParams2.Add(new RuleParams2
            {
                
                apply = true,
                color = ColorOptions.green,
                parameter = RuleParamOptions.price,
                condition = ConditionOptions.below,
                borderVal1 = 400,
                del = ShiftRuleParamList,
                sequenceNumber = 1,
            });
            ruleParams2.Add(new RuleParams2
            {
                
                apply = true,
                color = ColorOptions.yellow,
                parameter = RuleParamOptions.area,
                condition = ConditionOptions.between,
                borderVal1 = 10,
                borderVal2 = 20,
                del = ShiftRuleParamList,
                sequenceNumber = 2,
            });
            ruleParams2.Add(new RuleParams2
            {
                apply = true,
                color = ColorOptions.red,
                parameter = RuleParamOptions.price,
                condition = ConditionOptions.above,
                borderVal1 = 700,
                del = ShiftRuleParamList,
                sequenceNumber = 3,
            });
            ruleParams2.Add(new RuleParams2
            {
                apply = true,
                color = ColorOptions.green,
                parameter = RuleParamOptions.area,
                condition = ConditionOptions.above,
                borderVal1 = 30,
                del = ShiftRuleParamList,
                sequenceNumber = 4,
            });
        }

        protected async Task CheckResultValue()
        {            
            double price = 20;
            double area = 5;

            //string res = CheckResult(price, area);
            string res = CheckResult(testPrice, testArea);

            Console.WriteLine("Result: " + res);
        }

        private string RetColorFromRule(int ruleIndex)
        {
            Console.WriteLine("Rule " + ruleIndex + " met: " + ruleParams2[ruleIndex]);
            return ruleParams2[ruleIndex].color.ToString();
        }

        private string CheckResult(double price, double area)
        {
            bool ruleMatch = false;

            for (int rule = 0; rule < ruleParams2.Count; rule++)
            {
                RuleParams2 r = ruleParams2[rule];
                if (!r.apply) { continue; }
                switch (r.condition)
                {
                    case ConditionOptions.above:
                        if (r.parameter == RuleParamOptions.price)
                        {
                            ruleMatch = price >= r.borderVal1;
                        }
                        else if (r.parameter == RuleParamOptions.area)
                        {
                            ruleMatch = area >= r.borderVal1;
                        }
                        break;
                    case ConditionOptions.below:
                        if (r.parameter == RuleParamOptions.price)
                        {
                            ruleMatch = price < r.borderVal1;
                        }
                        else if (r.parameter == RuleParamOptions.area)
                        {
                            ruleMatch = area < r.borderVal1;
                        }
                        break;
                    case ConditionOptions.between:                
                        if(r.parameter == RuleParamOptions.price)
                        {
                            ruleMatch = (price >= r.borderVal1 && price < r.borderVal2) || (price < r.borderVal1 && price >= r.borderVal2);
                        }
                        else if(r.parameter == RuleParamOptions.area)
                        {
                            ruleMatch = (area >= r.borderVal1 && area < r.borderVal2) || (area < r.borderVal1 && area >= r.borderVal2);
                        }
                        break;                        
                }

                if (ruleMatch)
                {
                    return RetColorFromRule(rule);
                }
            }
            
            return "default";
        }

        //protected override async Task OnInitializedAsync()
        //{
        //    jsInterop = new JsInterop(JSRuntime);

        //    ruleParams = new List<RuleParams>();
        //    ruleParams.Add(new RuleParams
        //    {
        //        parameter = RuleParamOptions.price,
        //        apply = true,
        //        sign1 = "below",
        //        sign2 = "between",
        //        sign3 = "above",
        //        borderVal1 = 400,
        //        borderVal2 = 600
        //    });
        //    ruleParams.Add(new RuleParams
        //    {
        //        parameter = RuleParamOptions.area,
        //        apply = false,
        //        sign1 = "",
        //        sign2 = "",
        //        sign3 = "",
        //        borderVal1 = 0,
        //        borderVal2 = 0
        //    });
        //}

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                //Test_GetPDOKinfo_AddressFromCoords();
                //Test_GetLocationsWithRouteTask();
                //await Test_TypescriptArray();
                //FillBars();
            }
        }

        protected async Task FillBars()
        {
            while (progress2 < 100)
            {
                progress1 = 0;
                this.StateHasChanged();
                while (progress1 < 100)
                {
                    progress1 += 10;
                    this.StateHasChanged();
                    System.Threading.Thread.Sleep(200);
                }
                progress2 += 10;
                this.StateHasChanged();
            }
        }

        protected async Task Validate(int index)
        {
            //sign1 = "";
            //sign2 = "";
            //sign3 = "";

            //if(borderVal1 < borderVal2)
            //{
            //    sign1 = "below";
            //    sign2 = "between";
            //    sign3 = "above";
            //}
            //else if (borderVal1 > borderVal2)
            //{
            //    sign1 = "above";
            //    sign2 = "between";
            //    sign3 = "below";
            //}
            for (int r = 0; r < ruleParams.Count; r++)
            {
                if (r == index)
                {
                    ruleParams[r].apply = true;
                }
                else ruleParams[r].apply = false;
            }
           
            ruleParams[index].sign1 = "";
            ruleParams[index].sign2 = "";
            ruleParams[index].sign3 = "";

            if (ruleParams[index].borderVal1 < ruleParams[index].borderVal2)
            {
                ruleParams[index].sign1 = "below";
                ruleParams[index].sign2 = "between";
                ruleParams[index].sign3 = "above";
            }
            else if (ruleParams[index].borderVal1 > ruleParams[index].borderVal2)
            {
                ruleParams[index].sign1 = "above";
                ruleParams[index].sign2 = "between";
                ruleParams[index].sign3 = "below";
            }
            this.StateHasChanged();
        }

        protected async Task SetDefaultValues(int index)
        {
            int priceMin = 200;
            int priceMax = 800;
            int priceRange = priceMax - priceMin;
            double priceStep = priceRange / 3;

            ruleParams[index].borderVal1 = priceMin + priceStep;
            ruleParams[index].borderVal2 = priceMin + (2 * priceStep);
            ruleParams[index].sign1 = "below";
            ruleParams[index].sign2 = "between";
            ruleParams[index].sign3 = "above";

            this.StateHasChanged();
        }

        protected async Task MouseClicked(MouseEventArgs e)
        {
            Console.WriteLine(e.ClientX);
            Console.WriteLine(e.ClientY);
            await JSRuntime.InvokeAsync<Task>("LocationfromMouseCoordsDummy", e.ClientX, e.ClientY);
            userPinCoords = null;
            SyncUserPin();
        }

        protected async Task DrawIsochrone()
        {
            if (selectedCalculationMode == CalculationModes.time)
            {
                Console.WriteLine("Calculating with a "+ selectedTravelMode + " time of " + travelTime + " minutes.");
                await JSRuntime.InvokeAsync<Task>("GetIsochroneByTime", new double[] { 50.2, 6.1 }, selectedTravelMode.ToString(), travelTime); 
            }
            else if (selectedCalculationMode == CalculationModes.distance)
            {
                Console.WriteLine("Calculating with a " + selectedTravelMode + " distance of " + travelDistance + " kilometres.");
                await JSRuntime.InvokeAsync<Task>("GetIsochroneByDistance", new double[] { 50.2, 6.1 }, selectedTravelMode.ToString(), travelTime);
            }
            else Console.WriteLine("No calculation mode selected.");
        }

        //protected async Task Test_TypescriptArray()
        //{
        //    await JSRuntime.InvokeAsync<Task>("InsertPinFunction", 5);
        //    await JSRuntime.InvokeAsync<Task>("InsertPinFunction", 7);
        //    await JSRuntime.InvokeAsync<Task>("CheckHousePins");

        //    await JSRuntime.InvokeAsync<Task>("ClearHousePins");

        //    await JSRuntime.InvokeAsync<Task>("InsertPinFunction", 3);
        //    await JSRuntime.InvokeAsync<Task>("InsertPinFunction", 1);
        //    await JSRuntime.InvokeAsync<Task>("CheckHousePins");
        //}

        protected async Task Test_GetLocationsWithRouteTask()
        {
            List<string> stringList = new List<string>();
            stringList.Add("Linie 544 7325 DZ Apeldoorn Netherlands");
            stringList.Add("Hyacinthstraat 2 6658 XR Beneden-Leeuwen Netherlands");
            stringList.Add("Platolaan Nijmegen Netherlands");
            stringList.Add("Ruitenberglaan 31 Arnhem Netherlands");
            stringList.Add("Oude Groenewoudseweg Nijmegen Netherlands");
            stringList.Add("Hurksestraat 54 Eindhoven Netherlands");
            stringList.Add("Portugalstraat 24 Haarlem Netherlands");
            stringList.Add("Kapittelweg 33 Nijmegen Netherlands");
            stringList.Add("O.C. Huismanstraat 64 Nijmegen Netherlands");
            stringList.Add("Oude Maasdijk 58 Dreumel Netherlands");
            stringList.Add("Linie 544 7325 DZ Apeldoorn Netherlands");
            stringList.Add("Hyacinthstraat 2 6658 XR Beneden-Leeuwen Netherlands");
            stringList.Add("Platolaan Nijmegen Netherlands");
            stringList.Add("Ruitenberglaan 31 Arnhem Netherlands");
            stringList.Add("Oude Groenewoudseweg Nijmegen Netherlands");
            stringList.Add("Hurksestraat 54 Eindhoven Netherlands");
            stringList.Add("Portugalstraat 24 Haarlem Netherlands");
            stringList.Add("Kapittelweg 33 Nijmegen Netherlands");
            stringList.Add("O.C. Huismanstraat 64 Nijmegen Netherlands");
            stringList.Add("Oude Maasdijk 58 Dreumel Netherlands");
            stringList.Add("Linie 544 7325 DZ Apeldoorn Netherlands");
            stringList.Add("Hyacinthstraat 2 6658 XR Beneden-Leeuwen Netherlands");
            stringList.Add("Platolaan Nijmegen Netherlands");
            stringList.Add("Ruitenberglaan 31 Arnhem Netherlands");
            stringList.Add("Oude Groenewoudseweg Nijmegen Netherlands");
            stringList.Add("Hurksestraat 54 Eindhoven Netherlands");
            stringList.Add("Portugalstraat 24 Haarlem Netherlands");
            stringList.Add("Kapittelweg 33 Nijmegen Netherlands");
            stringList.Add("O.C. Huismanstraat 64 Nijmegen Netherlands");
            stringList.Add("Oude Maasdijk 58 Dreumel Netherlands");

            List<double[]> res = await MappingService.GetLocationsWithRouteTask(stringList);
            if (res == null) { return; }
            for (int r = 0; r < res.Count; r++)
            {
                if (res[r] != null)
                {
                    Console.WriteLine("Result " + r + ": (" + res[r][0].ToString() + " " + res[r][1].ToString() + ")");
                }
            }
        }

        protected async void Test_GetPDOKinfo()
        {
            List<string> stringList = new List<string>();
            stringList.Add("Linie 7325 DZ Apeldoorn");
            //stringList.Add("Hyacinthstraat 6658 XR Beneden-Leeuwen");
            //stringList.Add("Oude Groenewoudseweg 6524 VP Nijmegen");

            Stopwatch sp = new Stopwatch();
            sp.Restart();
            for (int i=0; i< stringList.Count; i++)
            {
                string query = stringList[i];
                
                //double[] coords = await MappingService.GetPDOKinfoTask(query);
                double[] coords = MappingService.GetPDOKinfoFromAddress(stringList[i]);
                if (coords != null)
                {
                    Console.WriteLine("Coords of " + query + ": (" + coords[0] + " " + coords[1] + ")");
                }                
            }
            //MappingService.GetPDOKinfo(stringList[0]);
            sp.Stop();
            Console.WriteLine("Normal: " + sp.Elapsed);
            Console.WriteLine();
            //MappingService.GetPDOKinfoSmall(stringList[0]);      

            sp.Restart();
            for (int i = 0; i < stringList.Count; i++)
            {
                string query = stringList[i];

                //double[] coords = await MappingService.GetPDOKinfoTask(query);
                double[] coords = MappingService.GetPDOKinfoFromAddress(stringList[i]);
                if (coords != null)
                {
                    Console.WriteLine("Coords of " + query + ": (" + coords[0] + " " + coords[1] + ")");
                }
            }
            sp.Stop();
            //MappingService.GetPDOKinfo(stringList[0]);
            Console.WriteLine("Small: " + sp.Elapsed);
        }

        protected async void Test_GetPDOKinfo_CoordsFromAddress()
        {
            List<string> stringList = new List<string>();
            stringList.Add("Linie 7325 DZ Apeldoorn");
            //stringList.Add("Hyacinthstraat 6658 XR Beneden-Leeuwen");
            //stringList.Add("Oude Groenewoudseweg 6524 VP Nijmegen");

            for (int i = 0; i < stringList.Count; i++)
            {
                string query = stringList[i];

                double[] coords = MappingService.GetPDOKinfoFromAddress(query);
                if (coords != null)
                {
                    Console.WriteLine("Coords of " + query + ": (" + coords[0] + " " + coords[1] + ")");
                }
            }
        }

        protected async void Test_GetPDOKinfo_AddressFromCoords()
        {
            //double[] query = new double[] { double.Parse(userPinCoords[0].Replace(".",",")), double.Parse(userPinCoords[1].Replace(".", ",")) };
            //Console.WriteLine(query[0]);
            //Console.WriteLine(query[1]);
            Console.WriteLine("Going to retrieve address!");
            string address = MappingService.GetPDOKinfoFromCoords(userPinCoords);
            if (address != null)
            {
                Console.WriteLine("Address of (" + userPinCoords[0] + " " + userPinCoords[1] + "): " + address);
            }
        }

        private async void SyncUserPin()
        {
            if (userPinCoords == null)
            {
                Console.Write(".");
                userPinCoords = await JSRuntime.InvokeAsync<string[]>("GetUserpinCoords");

                if (userPinCoords != null)
                {
                    Console.Write("Got userPinCoords from JSRuntime: (" + userPinCoords[0] + " " + userPinCoords[1] + ")");
                }

                SyncUserPin();
            }
            else
            {
                Console.WriteLine("\nSet userPinCoords: (" + userPinCoords[0] + " " + userPinCoords[1] + ")");
                Test_GetPDOKinfo_AddressFromCoords();
            }
        }
    }
}
