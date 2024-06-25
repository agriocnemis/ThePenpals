using System;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using BepInEx;
using UnityEngine;
using NCRApenpals;
using On;

namespace NCRApenpals
{
    public static class callingallpenpals
    {
        // dream cwt stuff!
        public class NCRAdream
        {
            public bool IsDream;
            public bool DreamActive;
            public bool InTheNightmare;
            public float Sanity;

            public NCRAdream(){
                this.IsDream = false;
                this.DreamActive = false;
                this.InTheNightmare = false;
                this.Sanity = 1f;
            }
        }

        private static readonly ConditionalWeakTable<Player, NCRAdream> CWTD = new();
        public static NCRAdream GetDreamCat(this Player player) => CWTD.GetValue(player, _ => new());


        // present cwt stuff!
        public class NCRAreal
        {
            public bool IsReal;
            public bool RealActive;
            public float Sanity;

            public NCRAreal()
            {
                this.IsReal = false;
                this.RealActive = false;
                this.Sanity = 1f;
            }
        }

        private static readonly ConditionalWeakTable<Player, NCRAreal> CWTR = new();
        public static NCRAreal GetRealCat(this Player player) => CWTR.GetValue(player, _ => new());
    }
}