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

        public int owned;


        float previousValue;

        static float universalGrowth = 1.01f;
        static int lowGrowthDays = 4;
        public static int currentDay = 0;

        public struct StockType
        {
            public float value;
            public float volatility;
            public float growthMultiplier;

            public StockType(float val, float vol, float growth)
            {
                value = val;
                volatility = vol;
                growthMultiplier = growth;
            }

            public enum Type
            {
                None,
                PennyStock,
                LowGrowth,
                CrackStock,
                Stable,
                HighGrowth,
                BlueChip,
                DividendStock
            }

            public StockType(Type newType)
            {
                switch (newType)
                {
                    case Type.PennyStock:
                        value = Random.Range(1, 10);
                        volatility = Random.Range(0.03f, 0.1f);
                        growthMultiplier = Random.Range(1.05f, 1.1f);
                        break;
                    case Type.LowGrowth:
                        value = Random.Range(10, 30);
                        volatility = Random.Range(0.03f, 0.15f);
                        growthMultiplier = Random.Range(1.02f, 1.05f);
                        break;
                    case Type.CrackStock:
                        value = Random.Range(5, 20);
                        volatility = Random.Range(0.2f, 0.5f);
                        growthMultiplier = Random.Range(0.5f, 0.8f);
                        break;
                    case Type.Stable:
                        value = Random.Range(40, 80);
                        volatility = Random.Range(0.01f, 0.05f);
                        growthMultiplier = Random.Range(0.95f, 1.05f);
                        break;
                    case Type.HighGrowth:
                        value = Random.Range(20, 60);
                        volatility = Random.Range(0.1f, 0.3f);
                        growthMultiplier = Random.Range(1.1f, 1.2f);
                        break;
                    case Type.BlueChip:
                        value = Random.Range(70, 120);
                        volatility = Random.Range(0.01f, 0.1f);
                        growthMultiplier = Random.Range(1.02f, 1.08f);
                        break;
                    case Type.DividendStock:
                        value = Random.Range(50, 100);
                        volatility = Random.Range(0.05f, 0.15f);
                        growthMultiplier = Random.Range(0.95f, 1.1f);
                        break;
                    default:
                        value = Random.Range(10, 50);
                        volatility = Random.Range(0.03f, 0.2f);
                        growthMultiplier = Random.Range(.9f, 1.1f);
                        break;
                }
            }

            public static Type RandomType()
            {
                return (Type)Random.Range(1, Type.GetValues(typeof(Type)).Length);
            }
        }

        public Stock(string stockName, float initialValue = 100, float initialVolatility = 0.05f, float usualGrowth = 1.2f)
        {
            name = stockName;
            previousValue = initialValue;
            value = initialValue * (1 + volatility *  Random.Range(-1,1));
            volatility = initialVolatility;
            growthMultiplier = usualGrowth;
        }
        public Stock(string stockName, StockType stockType)
        {
            name = stockName;
            previousValue = stockType.value;
            value = stockType.value * (1 + volatility * Random.Range(-1, 1));
            volatility = stockType.volatility;
            growthMultiplier = stockType.growthMultiplier;
        }


        //Value
        public void NextDay(float growth)
        {
            float earlyMultiplier = 1;
            if (currentDay < lowGrowthDays) earlyMultiplier = currentDay / lowGrowthDays;

            float scaledGrowth = Mathf.Max(-1f, Mathf.Min(1f, growth)) * growthMultiplier;
            NewValue(value * (universalGrowth + scaledGrowth * earlyMultiplier) * (1 + volatility * Random.Range(-1, 1) * earlyMultiplier));
        }
        public void UpdatePrice(float time)
        {
            float noise = PerlinNoise(time);
            NewValue(value + noise * volatility * value * 0.1f, false);
        }
        public void NewValue(float newValue, bool updateOldValue = true)
        {
            if (updateOldValue) previousValue = value;
            value = newValue;
        }
        private float PerlinNoise(float time)
        {
            float perlinValue = (float)(Mathf.Sin(2 * time) + Mathf.Sin(Mathf.PI * time));

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
            return $"{GetCurrentValue()}$";
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
}
