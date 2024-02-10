using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LC_StockMarketIndex.Patches
{
    public class Stock
    {
        public string name;
        public float value;
        float volatility;
        float growthMultiplier;
        public float moneyToReachBaseGrowth;

        public int owned;

        public string[] products; 
        float previousValue;

        static float universalGrowth = 0.2f;
        static int lowGrowthDays = 4;
        public static int currentDay = 0;

        public float tOffset;
        public float tMult;

        public static Stock[] GetStocks()
        {
            List<Stock> list = new List<Stock>();
            foreach(Company comp in (Company[])System.Enum.GetValues(typeof(Company)))
            {
                list.Add(new Stock(new StockType(comp)));
            }
            return list.ToArray();
        }
        public struct StockType
        {
            public string name;
            public float value;
            public float volatility;
            public float growthMultiplier;
            public int moneyToReachBaseGrowth;
            public string[] products;
            public StockType(Company company)
            {
                switch (company)
                {
                    case Company.HaldanElectronics:
                        name = "Haldan Electronics INC";
                        value = Random.Range(10, 30);
                        volatility = Random.Range(0.03f, 0.1f);
                        growthMultiplier = Random.Range(1.05f, 1.1f);
                        moneyToReachBaseGrowth = 350;
                        products = new string[] { "" };
                        break;
                    case Company.KremmersCrematorium:
                        name = "Kremmer's crematorium LLC";
                        value = Random.Range(5, 10);
                        volatility = 0;
                        growthMultiplier = Random.Range(1.5f, 2f);
                        moneyToReachBaseGrowth = 30;
                        products = new string[] { "" };
                        break;
                    case Company.FarmersUnion:
                        name = "Farmer's Union INC";
                        value = Random.Range(25, 60);
                        volatility = Random.Range(0.05f, 0.15f);
                        growthMultiplier = Random.Range(1.1f, 1.2f);
                        moneyToReachBaseGrowth = 150;
                        products = new string[] { "" };
                        break;
                    case Company.MidasScrap:
                        name = "Midas Scrap LLC";
                        value = Random.Range(10, 30);
                        volatility = Random.Range(0.1f, 0.3f);
                        growthMultiplier = Random.Range(0.8f, 3f);
                        moneyToReachBaseGrowth = 100;
                        products = new string[] { "" };
                        break;
                    case Company.Blockbuster:
                        name = "Blockbuster LLC";
                        value = Random.Range(80, 120);
                        volatility = Random.Range(0.05f, 0.1f);
                        growthMultiplier = Random.Range(1.2f, 1.4f);
                        moneyToReachBaseGrowth = 300;
                        products = new string[] { "" };
                        break;
                    case Company.HandyToolsNHardware:
                        name = "Handy tools n' Hardware INC";
                        value = Random.Range(5, 60);
                        volatility = Random.Range(0.0f, 0.1f);
                        growthMultiplier = Random.Range(1.1f, 1.3f);
                        moneyToReachBaseGrowth = 150;
                        products = new string[] { "" };
                        break;
                    default:
                        name = "";
                        value = 1;
                        volatility = 1;
                        growthMultiplier = 1;
                        moneyToReachBaseGrowth = 1;
                        products = new string[] { "" };
                        break;

                }
            }
        }

        public Stock(StockType stockType)
        {
            name = stockType.name;
            previousValue = stockType.value;
            value = stockType.value * (1 + volatility * Random.Range(-1, 1));
            volatility = stockType.volatility;
            growthMultiplier = stockType.growthMultiplier;
            products = stockType.products;
            moneyToReachBaseGrowth = stockType.moneyToReachBaseGrowth;
        }


        //Value
        public void NextDay(float growth)
        {
            float scaledGrowth;
            if (growth > 0) 
            {
                scaledGrowth = 1 + growth * growthMultiplier * universalGrowth;
            }
            else
            {
                var val = 1f / (growthMultiplier * universalGrowth);
                scaledGrowth = val / (val -growth);
            }

            if (currentDay < lowGrowthDays) Mathf.Clamp(scaledGrowth, (float)currentDay / (float)lowGrowthDays,1f / ((float)currentDay / (float)lowGrowthDays));

            NewValue(value * (scaledGrowth) * (1 + volatility * Random.Range(-1, 1)));
        }
        public void UpdatePrice(float time)
        {
            float overDayGrowth = 0.2f * universalGrowth * (volatility);
            float preNoise = PerlinNoise(time);
            float noise = Mathf.Pow(preNoise,2f) * overDayGrowth;
            
            float priceChange = 1;

            if(preNoise >= 0)
            {
                priceChange = 1f + noise;
            }
            else
            {
                priceChange = 1f / (Mathf.Abs(noise)+1f);
            }
            NewValue(value * priceChange, false);
        }
        public void NewValue(float newValue, bool updateOldValue = true)
        {
            if (updateOldValue) previousValue = value;
            value = newValue;
        }
        private float PerlinNoise(float time)
        {
            float perlinValue = Random.Range(-2f, 2f);

            return perlinValue;
        }


        //Writing
        public string GetStock()
        {
            string dailyGrowth = Mathf.Sign(GetDailyGrowth()) >= 0 ? "<color=green>+" : "<color=red>"; //Green : Red
            string plusOrMinus = Mathf.Sign(GetDailyGrowth()) >= 0 ? "+" : ""; //Green : Red
            string colorEnd = "</color>";

            //return $"{name}  -  {WriteValue()} {WriteDailyGrowth()}";
            return $"{name}  -  {WriteValue()} {plusOrMinus}" + "{0}";
        }
        public string WriteValue()
        {
            return $"{(int)Mathf.Ceil(value * owned)}$";
        }
        public int GetCurrentValue()
        {
            return (int)Mathf.Ceil(value);
        }

        public string WriteDailyGrowth()
        {
            return $"{(GetDailyGrowth() * 100f).ToString("N1")}%";
        }
        public float GetDailyGrowth()
        {
            return (float)(value / previousValue - 1);
        }
    }
    public enum Company
    {
        HaldanElectronics,
        KremmersCrematorium,
        Blockbuster,
        MidasScrap,
        FarmersUnion,
        HandyToolsNHardware
    }
}
