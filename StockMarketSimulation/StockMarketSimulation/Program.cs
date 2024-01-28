using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Console = Colorful.Console;
using System.Drawing;
using Colorful;

internal class Program
{
    static void Main()
    {
        int atk = 10;
        int def = 10;
        int level = -4;
        Console.WriteLine(((2 * level / 5) + 2) * 50 * atk / def / 50 + 2);

        Stock[] stockMarket = {
        new Stock("Control LLC",840,0.5f,1.1f),
        new Stock("Hal Inc",120,.05f,1.05f),
        new Stock("Scrap Union Nonprofit", 30)
        };
        for(int day = 0; day < 10; day++)
        {
            Console.WriteLine($"\nBegining of Day {day} - Stockmarket now open");
            Stock.currentDay = day;
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine($"\tCurrent time is {8 + i}:30");
                foreach (Stock stock in stockMarket)
                {
                    Color c = Math.Sign(stock.GetDailyGrowth()) >= 0 ? Color.Green : Color.Red; //Green : Red
                    Console.WriteLineFormatted("\t\t" + stock.GetStock(), Color.White, new Formatter (stock.WriteDailyGrowth(), c));
                    stock.UpdatePrice(i);
                }
            }
            Console.WriteLine($"End of Day {day} - Stockmarket now closed");
            foreach (Stock stock in stockMarket)
            {
                stock.NextDay(0.1f);
            }

        }
    }
}

public class Stock
{
    string name;
    float value;
    float volatility;
    float growthMultiplier;

    float previousValue;

    static float universalGrowth = 1.01f;
    static int lowGrowthDays = 4;
    public static int currentDay = 0;

    public Stock(string stockName, float initialValue = 100, float initialVolatility = 0.05f, float usualGrowth = 1.2f)
    {
        name = stockName;
        previousValue = initialValue;
        value = initialValue; // * (1 + volatility * new Random().Next(-1, 1));
        volatility = initialVolatility;
        growthMultiplier = usualGrowth;
    }

    
    //Value
    public void NextDay(float growth)
    {
        float earlyMultiplier = 1;
        if(currentDay < lowGrowthDays) earlyMultiplier = currentDay / lowGrowthDays;

        float scaledGrowth = Math.Max(-1f, Math.Min(1f, growth)) * growthMultiplier;
        NewValue(value * (universalGrowth + scaledGrowth * earlyMultiplier) * (1 + volatility * new Random().Next(-1, 1) * earlyMultiplier));
    }
    public void UpdatePrice(float time)
    {
        float noise = PerlinNoise(time);
        NewValue(value + noise * volatility * value * 0.1f, false);
    }
    public void NewValue(float newValue, bool updateOldValue = true)
    {
        if(updateOldValue) previousValue = value;
        value = newValue;
    }
    private float PerlinNoise(float time)
    {
        float perlinValue = (float)(Math.Sin(2 * time) + Math.Sin(Math.PI * time));

        return perlinValue;
    }


    //Writing
    public string GetStock()
    {
        string dailyGrowth = Math.Sign(GetDailyGrowth()) >= 0 ? "<color=green>+" : "<color=red>"; //Green : Red
        string plusOrMinus = Math.Sign(GetDailyGrowth()) >= 0 ? "+" : ""; //Green : Red
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
        return (int)Math.Ceiling(value);
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
