using System;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using BepInEx;
using UnityEngine;
using NCRApenpals;
using On;

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
            public bool IsReal;
            public bool IsRealSession;

            public NCRAreal()
            {
                this.IsReal = false;
                this.IsRealSession = false;
            }
        }

        private static readonly ConditionalWeakTable<Player, NCRAreal> CWTR = new();
        public static NCRAreal GetRealCat(this Player player) => CWTR.GetValue(player, _ => new());
    }
}