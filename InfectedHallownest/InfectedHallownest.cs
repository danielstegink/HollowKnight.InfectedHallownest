using System.Collections.Generic;
using GlobalEnums;
using Modding;
using Modding.Utils;
using UnityEngine;

namespace InfectedHallownest
{
    public class InfectedHallownest : Mod
    {
        internal static InfectedHallownest Instance;

        public override string GetVersion() => "1.0.0.0";

        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            Log("Initializing");

            Instance = this;

            On.HealthManager.Start += SpreadInfection;

            Log("Initialized");
        }

        /// <summary>
        /// Once the player has beaten THK, the Infection enters stage 2 and starts to empower enemies throughout the map like it does in Crossroads
        /// </summary>
        /// <param name="orig"></param>
        /// <param name="self"></param>
        private void SpreadInfection(On.HealthManager.orig_Start orig, HealthManager self)
        {
            orig(self);

            // Only apply Infection if THK has been defeated at least once
            if (!PlayerData.instance.killedHollowKnight)
            {
                return;
            }

            // Make sure the given object is an enemy. HP Bar is screwy on this front
            if (self.gameObject.layer != (int)PhysLayers.ENEMIES &&
                !self.gameObject.tag.Equals("Boss"))
            {
                return;
            }

            string sceneName = GameManager.instance.sceneName;
            string objectName = self.gameObject.name;

            // The Infection has already advanced in Crossroads, so it can't spread to enemies there
            List<string> specialCrossroadsRooms = new List<string>()
            {
                "Room_temple",
                "Room_Final_Boss_Atrium",
                "Room_Final_Boss_Core",
                "Room_Tram_RG",
                "Room_Charm_Shop",
                "Room_ruinhouse",
                "Room_Mender_House",
            };
            if (sceneName.Contains("Crossroads") ||
                specialCrossroadsRooms.Contains(sceneName))
            {
                return;
            }
            
            // The Infection doesn't affect the Mantis Tribe (aside from Traitors)
            if (objectName.Contains("Mantis") &&
                !objectName.Contains("Mantis Heavy Flyer") && // Flying Traitor
                !objectName.Contains("Mantis Heavy") && // Mantis Traitor
                !objectName.Contains("Mantis Traitor Lord")) // Traitor Lord
            {
                return;
            }

            // The Infection doesn't affect the Grimm Troupe
            if (objectName.Contains("Grimm"))
            {
                return;
            }

            // The Infection doesn't affect those few capable of resisting it
            List<string> specialBossNames = new List<string>()
            {
                "Hornet Boss 1",
                "Hornet Boss 2",
                "Dung Defender"
            };
            if (specialBossNames.Contains(objectName) ||
                objectName.Contains("Zote Boss"))
            {
                return;
            }

            // The Infection doesn't affect Void
            if (objectName.Contains("Jar Collector") ||
                objectName.Contains("Sibling") ||
                objectName.Contains("Hollow Shade"))
            {
                return;
            }

            // The Infection doesn't affect Dreams
            List<string> dreamBossScenes = new List<string>()
            {
                "Dream_03_Infected_Knight", // Lost Kin
                "Dream_02_Mage_Lord", // Soul Tyrant
                "Dream_Mighty_Zote", // GPZ
                "Dream_Final_Boss", // Radiance
                "Dream_01_False_Knight", // Failed Champion
                "Dream_04_White_Defender", // White Defender
                "gg dryya", // PC Dryya
                "dryya overworld",
                "gg zemer", // PC Ze'mer
                "zemer overworld arena",
                "gg hegemol", // PC Hegemol
                "hegemol overworld arena",
                "gg isma", // PC Isma
                "isma overworld",
            };
            List<string> dreamBossNames = new List<string>()
            {
                "Ghost Warrior Galien",
                "Ghost Warrior Hu",
                "Ghost Warrior No Eyes",
                "Ghost Warrior Slug",
                "Ghost Warrior Markoth",
                "Ghost Warrior Marmu",
                "Ghost Warrior Xero",
            };
            if (dreamBossScenes.Contains(sceneName) ||
                dreamBossNames.Contains(objectName) ||
                sceneName.StartsWith("White_Palace") ||
                sceneName.StartsWith("GG_"))
            {
                return;
            }

            // Want a little less than half of eligible enemies to be infected
            //Log($"{objectName} is eligible for Infection");
            int randomChance = UnityEngine.Random.Range(1, 101);
            if (randomChance <= 40)
            {
                // Infected enemies have 2x health and flash orange periodically
                int oldHp = self.hp;
                self.hp *= 2;
                self.gameObject.GetOrAddComponent<Infection>();
                //Log($"{objectName} has been infected: {oldHp} -> {self.hp}");
            }
        }
    }
}