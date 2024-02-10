using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using LethalLib.Modules;
using GameNetcodeStuff;
using System.IO;
using Unity.Netcode;
using LC_StockMarketIndex.Patches;
using UnityEngine.SceneManagement;
using SAM;
using System.Reflection;
using TerminalApi;
using TerminalApi.Classes;
using static TerminalApi.Events.Events;
using static TerminalApi.TerminalApi;
using System.Runtime.InteropServices;


namespace LC_StockMarketIndex
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class StockMarketIndexMod : BaseUnityPlugin
    {
        private const string modGUID = "Mellowdy.StockMarketIndex";
        private const string modName = "StockMarketIndex";
        private const string modVersion = "0.0.1";

        private const string assetName = "stock.device";
        private const string gameObjectName = "StockIndex.prefab";

        private readonly Harmony harmony = new Harmony(modGUID);

        public static ManualLogSource mls;
        private static StockMarketIndexMod instance;



        static string Name = "Stock Index";
        static string Description = "Allows you to buy and sell speculative assets.";

        static ConfigEntry<int> price;
        public static ConfigEntry<bool> voice;

        public static GameObject networkObject;

        void Awake()
        {
            if (instance == null) instance = this;

            mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);

            harmony.PatchAll();

            #region Stock market index device

            //Assign Config Settings

            price = Config.Bind<int>("Price", "DevicePrice", 35, "Credits needed to buy the hand-held device");
            voice = Config.Bind<bool>("Voice", "DeviceVoiceOn", true, "If the device talkt to you or not.");

            //Asset Bundle

            string currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string path = Path.Combine(currentDirectory, assetName).Replace("\\", "/");

            mls.LogMessage(path);

            AssetBundle assets = AssetBundle.LoadFromFile(path);
            Item device = assets.LoadAsset<Item>("Stock.asset");

            GameObject deviceObject = assets.LoadAsset<GameObject>(gameObjectName);

            //GameObject
            #region

            //Assign Components

            StockMarketIndex smi = deviceObject.AddComponent<StockMarketIndex>();
            
            smi.originalScale = deviceObject.transform.localScale;
            smi.grabbable = true;
            smi.itemProperties = device;
            smi.floorYRot = -1;

            device.spawnPrefab = deviceObject;
            SceneManager.sceneLoaded += StockMarketIndex.FindTerminal;
            #endregion

            #endregion

            //Add to shop
            Items.RegisterItem(device);
            Items.RegisterShopItem(device, null, null, CreateInfoNode(Name, Description), price.Value);

            #region Terminal Entries

            AddTerminalCommand("Blockbuster LLC",
"An ancient company that produces and sells strange arcane artifacts. The sole patent owner for the “quantum superposition box”, sold as the “perfect gift box” that is advertised to become whatever the receiver wants it to be once opened. The company owns major stock in several biotech companies, some of which produce biological weapons. \\nSome rumors say it once was a video rental service, but it's often scoffed at as being highly unfactual. <i>“Video rental as a service and core business model is stupid and doomed to fail. The people that truly believe that one of the top NASDAQ companies could start from such a business model, I can only describe as mentally inefficient.”</i> According to one seasoned economist who was asked about the topic.");
            
            AddTerminalCommand("Kremmer's crematorium LLC",
                "A fixture in the cremation industry, founded by Jonathan Kremmer, inventor of the <i>body recombobulator</i>. The innovation that single handedly crashed and reinvented the personal safety market in the year of 1987 paving the way for new, previously impossibly dangerous frontiers such as [REDACTED], the scrap collection company. The company specializes in body disposal and resurrection services.<");

            AddTerminalCommand("Halden electronics INC",
                "A venerable establishment in the realm of electronic innovation, Halden Electronics Inc. stands tall among the top echelons of electronic manufacturers, boasting an extensive inventory surpassing even the formidable holdings of their rival, Handy Tools n' Hardware. Renowned for their mastery over a myriad of products, including hazard suits, terminals, radar scanners, flashlights, and their illustrious flagship creation – the Infinite Lifetime Lamp – Halden Electronics commands a monopoly over these essential wares.\nWith a stranglehold on various sectors, including military, food, transportation, and medical technology, Halden Electronics wields considerable influence, holding significant shares in numerous enterprises.");

            AddTerminalCommand("Midas Scrap LLC",
                "<b>[REDACTED]</b>\n\n" +
                "It seems this file is redacted! " +
                "Cause: limitations placed by administrator. " +
                "If you believe this redaction was placed by accident contact company " +
                "management at [121-768-7395] or contact our support line at Haldan Electronics.");

            AddTerminalCommand("Farmer's Union INC",
                "An ancient company that produces and sells strange arcane artifacts. " +
                "The sole patent owner for the “quantum superposition box”, " +
                "sold as the “perfect gift box” that is advertised to become " +
                "whatever the receiver wants it to be once opened. The company " +
                "owns major stock in several biotech companies; some of which " +
                "produce biological weapons. \nSome rumors say it once was a video " +
                "rental service but it's often scoffed at as being highly unfactual. " +
                "<i>“Video rental as a service and core business model is stupid and" +
                " doomed to fail. The people that truly believe that one of the top NASDAQ" +
                " companies could start from such a business model, I can only describe as" +
                " mentally inefficient.”</i> according to one seasoned economist who was " +
                "asked about the topic.");

            AddTerminalCommand("Handy tool's n' Hardware",
                "company info");

            #endregion

            mls.LogInfo($"{modName} is active");
        }
        private TerminalNode CreateInfoNode(string name, string description)
        {
            TerminalNode val = ScriptableObject.CreateInstance<TerminalNode>();
            val.clearPreviousText = true;
            val.name = name + "InfoNode";
            val.displayText = description + "\n\n";
            return val;
        }

        private void AddTerminalCommand(string name = "Name", string body = "Text")
        {
            string starter = $"<b>{name}</b>\n\n";
            string ender = "\n";
            string command = "get";
            AddCommand(name, starter + body + ender, command, true);

        }
    }
}
