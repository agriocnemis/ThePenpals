using System;
using BepInEx;
using UnityEngine;
using SlugBase.Features;
using static SlugBase.Features.FeatureTypes;
using callingallpenpals;

namespace NCRApenpals
{
    [BepInPlugin(MOD_ID, "NCRApenpals", "0.0.0")]
    class Plugin : BaseUnityPlugin
    {
        private const string MOD_ID = "neoncityrain-agriocnemis.penpals";

        public void OnEnable()
        {
            // initializing
            On.RainWorld.OnModsInit += Extras.WrapInit(LoadResources);

            // locking and unlocking
            On.SlugcatStats.SlugcatUnlocked += SlugcatStats_SlugcatUnlocked;

            // ----------------------------------- DREAM THINGS
            // zero-gravity oracles always
            On.SSOracleSwarmer.Update += SSOracleSwarmer_Update;

            //------------------------------------ REAL THINGS
            //

        }

        private void SSOracleSwarmer_Update(On.SSOracleSwarmer.orig_Update orig, SSOracleSwarmer self, bool eu)
        {
            orig(self, eu);
            if (self.room.game.session.characterStats.name.value == "NCRAdream" && self.room.readyForAI)
            {
                self.affectedByGravity = 0f;
            }
        }

        private bool SlugcatStats_SlugcatUnlocked(On.SlugcatStats.orig_SlugcatUnlocked orig, SlugcatStats.Name i, RainWorld rainWorld)
        {
            if (i.value == "NCRAdream" || i.value == "NCRAreal")
            {
                return true;
            }
            else return orig(i, rainWorld);
        }

        // the below is a weight-bearing piece of code lol
        private void LoadResources(RainWorld rainWorld)
        {
        }
    }
}