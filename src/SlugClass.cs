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
        // dream cwt stuff!
        public class NCRAdream
        {
            public bool IsDream;

            public NCRAdream(){
                this.IsDream = false;
            }
        }

        private static readonly ConditionalWeakTable<Player, NCRAdream> CWTD = new();
        public static NCRAdream GetDreamCat(this Player player) => CWTD.GetValue(player, _ => new());


        // present cwt stuff!
        public class NCRAreal
        {
            public bool IsYourSlugcat;

            public NCRAreal()
            {
                this.IsYourSlugcat = false;
            }
        }

        private static readonly ConditionalWeakTable<Player, NCRAreal> CWTR = new();
        public static NCRAreal GetPresentCat(this Player player) => CWTR.GetValue(player, _ => new());
    }
}