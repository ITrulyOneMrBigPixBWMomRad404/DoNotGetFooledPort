using BepInEx;
using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.ObjectCreation;
using MTM101BaldAPI.OptionsAPI;
using MTM101BaldAPI.Registers;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace DontGetFooled
{
    /*
     * Hey, Big Thinker, I hope you'll see this comment. I know you are bad at coding, so you can use my port and code, than publish it!
     * Big Thinker, I heard you reject MTM101's API, isn't it because API was too complicate for you? If yes, read my comments to understand how methods and classes work!
     * 
     * First of all install assembly publicizer
     * Add all dependies including MTM101's API and Newtonsoft.Json
     */
    public class PluginInfo
    {
        // Use constants for defining mod info! This way is really good
        public const string GUID = "bigthinker.be.smarter";
        public const string NAME = "Don't Get Fooled";
        public const string VERSION = "1.0";
        // Your code didn't use C# standard, so read this
        /*
         * Classes, Interfaces, Structs, and Enums
            PascalCase is used for naming classes, structs, enums, and interfaces.
            Examples:
                Class: CustomerDetails
                Interface: IOrderService
                Struct: Point3D
                Enum: OrderStatus
           Methods
            PascalCase is used for method names.
            Examples:
                CalculateTotalPrice()
                ValidateUserInput()
           Variables
            camelCase is used for local variables, fields, and method parameters.
            Examples:
                Local variable: totalAmount
                Method parameter: orderId
           Constants
            PascalCase or UPPER_SNAKE_CASE can be used for constants.
            Examples:
                ModVersion
                MOD_VERSION
           Properties
            PascalCase is used for public properties.
            Examples:
                FirstName
                OrderId
         This list is really long, read at Google or ask ChatGPT about it!
         * */

    }
    [BepInPlugin(PluginInfo.GUID, PluginInfo.NAME, PluginInfo.VERSION)]
    [BepInDependency("mtm101.rulerp.bbplus.baldidevapi", BepInDependency.DependencyFlags.HardDependency)] // Add MTM101's API for making this mod working
    public class BasePlugin : BaseUnityPlugin
    {
        public static string ModPath { get; private set; }
        public static BasePlugin Instance { get; private set; }
        public static AssetManager Asset { get; private set; } // AssetManager really good class, I recommend use it for storaging any objects, thanks to MTM101 
        private IEnumerator Load()
        {
            yield return 3; // Count of steps
            yield return "Creating sound from file..."; // Text for step
            // Set lenght for audio to 0 for disabling subtitles
            Asset.Add("Kurth_Jumpscare", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(ModPath + "Audio/Kurth_Jumpscare.wav"), "", SoundType.Voice, new Color(0.6f, 0.55f, 0.55f), 0));
            yield return "Creating textures from files..."; // Text for step, again
            // Using loop is really better
            for (int i = 0; i < 9; i++)
            {
                // This shouldn't be possible, but I think this is better than throwing exception
                if (i > 8)
                    break;
                Asset.Add("KurthAttack_" + i.ToString(), AssetLoader.TextureFromFile(ModPath + "Textures/KurthAttack_" + i.ToString() + ".png").ConvertToSprite(0.5f, 256f));
            }
            //Read more about AssetLoader, this class really useful
            Asset.Add("KurthFakeDoor", AssetLoader.TextureFromFile(ModPath + "Textures/KurthFakeDoor.png").ConvertToSprite(0.5f, 256f));
            yield return "Creating kurth character..."; // I think you already know it, this is text for step!
            // Read or ask about NPCBuilder, this thing very conveniently
            Kurth kurth = new NPCBuilder<Kurth>(Info)
                .SetName("Kurth")
                .SetEnum("Kurth")
                .AddTrigger()
                .AddSpawnableRoomCategories(RoomCategory.Hall)
                .SetPoster(AssetLoader.TextureFromFile(ModPath + "Textures/pri_kurth.png"), "PST_Kurth_Name", "PST_Kurth_Desc")
                .Build();
            Asset.Add("Kurth", kurth);
            yield break;
        }
        // Awake method calls at then moment when you create object, it calls before Start()
        private void Awake()
        {
            Harmony harmony = new Harmony(PluginInfo.GUID);
            harmony.PatchAllConditionals(); // Use PatchAllConditionals method from MTM101's API for adding compatibility for other mods!
            if (Instance == null)
            {
                Instance = this;
            }
            Asset = new AssetManager();
            ModPath = AssetLoader.GetModPath(this) + "/"; // This method returns mod path
            LoadingEvents.RegisterOnAssetsLoaded(Info, Load(), false); // Register loading assets using MTM101's loading screen
            AssetLoader.LoadLocalizationFolder(Path.Combine(ModPath, "Language", "English"), Language.English); // Loading .json files
            GeneratorManagement.Register(this, GenerationModType.Addend, (x, y, z) =>
            {
                if (x != "F1")
                {
                    z.potentialNPCs.Add(new WeightedNPC()
                    {
                        selection = Asset.Get<NPC>("Kurth"),
                        weight = 250

                    }); 
                }
            });
        }
    }
}