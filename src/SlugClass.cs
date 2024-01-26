using System;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using BepInEx;
using UnityEngine;
using callingallpenpals;


namespace callingallpenpals
{
    public static class callingallpenpals
    {
        public class YourSlugcat
        {
            // Define your variables to store here!
            public int HowManyJumps;
            public bool IsYourSlugcat;
            public List<Creature> CreaturesYouPickedUp;

            public YourSlugcat(){
                // Initialize your variables here! (Anything not added here will be null or false or 0 (default values))
                this.HowManyJumps = 0;
                this.IsYourSlugcat = false;
                this.CreaturesYouPickedUp = new List<Creature>();
            }
        }

        // This part lets you access the stored stuff by simply doing "self.GetCat()" in Plugin.cs or everywhere else!
        private static readonly ConditionalWeakTable<Player, YourSlugcat> CWT = new();
        public static YourSlugcat GetCat(this Player player) => CWT.GetValue(player, _ => new());
    }
}