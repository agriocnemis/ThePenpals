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

        public static readonly PlayerFeature<float> SuperJump = PlayerFloat("slugcwt/super_jump");


        // Add hooks
        public void OnEnable()
        {
            On.RainWorld.OnModsInit += Extras.WrapInit(LoadResources);

            // Put your custom hooks here!
            On.Player.Jump += Player_Jump;
            On.Player.CanIPickThisUp += Playering_CanPickUp;  // Name of method (right side) can be any name as you please!
            On.Player.ctor += Abracadabra;
        }



        // Load any resources, such as sprites or sounds
        private void LoadResources(RainWorld rainWorld)
        {
        }

        private void Abracadabra(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
        {
            orig(self, abstractCreature, world);
            if (self.slugcatStats.name.value == "CWTCat"){
                self.GetCat().IsYourSlugcat = true;
            }
        }

        private bool Playering_CanPickUp(On.Player.orig_CanIPickThisUp orig, Player self, PhysicalObject obj)
        {
            bool canPickUp = orig(self, obj);

            // Simply add creature to the list
            if (self.GetCat().IsYourSlugcat && canPickUp && obj != null && obj is Creature){
                self.GetCat().CreaturesYouPickedUp.Add(obj as Creature);
            }

            return canPickUp;
        }

        // Implement SuperJump
        private void Player_Jump(On.Player.orig_Jump orig, Player self)
        {
            orig(self);

            if (self.GetCat().IsYourSlugcat && SuperJump.TryGet(self, out var power))
            {
                self.jumpBoost *= 1f + power;

                // Add 1 to the amount of times the player jumped with this character!
                self.GetCat().HowManyJumps++;
            }

        }
    }
}