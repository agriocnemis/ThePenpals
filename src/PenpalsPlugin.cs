using BepInEx;
using UnityEngine;
using SlugBase.Features;
using MoreSlugcats;
using CustomRegions;
using RWCustom;
using NCRApenpals;
using DevInterface;
using HUD;
using MonoMod.RuntimeDetour;
using System.Reflection;
using System;
using CoralBrain;

namespace NCRApenpals
{
    [BepInPlugin(MOD_ID, "NCRApenpals", "0.0.0")]
    class Plugin : BaseUnityPlugin
    {
        private const string MOD_ID = "neoncityrain-agriocnemis.penpals";
        public delegate Color orig_OverseerMainColor(global::OverseerGraphics self);

        public void OnEnable()
        {
            On.RainWorld.OnModsInit += Extras.WrapInit(LoadResources);
            On.Player.ctor += Player_ctor;
            // initializing

            On.SlugcatStats.SlugcatUnlocked += SlugcatStats_SlugcatUnlocked;
            // locking and unlocking

            On.Player.UpdateMSC += Player_UpdateMSC;
            // water bouyancy, dream gravity

            On.Player.DeathByBiteMultiplier += Player_DeathByBiteMultiplier;
            // lizards kill real more and dream less

            On.MoreSlugcats.StowawayBugState.AwakeThisCycle += StowAwake;
            // forcing stowaway states

            On.PlayerGraphics.ctor += PlayerGraphics_ctor;
            // graphics editing

            On.RegionGate.customKarmaGateRequirements += RegionGate_customKarmaGateRequirements;
            // can change any gate requirement we want

            // ----------------------------------- DREAM THINGS
            On.SSOracleSwarmer.Update += SSOracleSwarmer_Update;
            // zero-gravity oracles always

            On.FireFly.ctor += FireFly_ctor;
            On.GreenSparks.GreenSpark.ApplyPalette += GreenSpark_ApplyPalette;
            On.GreenSparks.GreenSpark.Update += GreenSpark_Update;
            On.SeedCob.ApplyPalette += SeedCob_ApplyPalette;
            On.Lantern.TerrainImpact += Lantern_TerrainImpact;
            On.FlyLure.ApplyPalette += FlyLure_ApplyPalette;
            On.PoleMimicGraphics.ApplyPalette += PoleMimicGraphics_ApplyPalette;
            On.FlyGraphics.ApplyPalette += FlyGraphics_ApplyPalette;
            On.Lantern.ApplyPalette += Lantern_ApplyPalette;
            On.Lantern.Update += Lantern_Update;
            On.DangleFruit.ApplyPalette += DangleFruit_ApplyPalette;
            On.TentaclePlantGraphics.ApplyPalette += TentaclePlantGraphics_ApplyPalette;
            On.PoleMimicGraphics.DrawSprites += PoleMimicGraphics_DrawSprites;
            // making dream colourful

            Hook fancyoverseers = new Hook(typeof(global::OverseerGraphics).GetProperty("MainColor", BindingFlags.Instance |
                BindingFlags.Public).GetGetMethod(), new Func<orig_OverseerMainColor,
                OverseerGraphics, Color>(this.OverseerGraphics_MainColor_get));
            On.CoralBrain.Mycelium.UpdateColor += Mycelium_UpdateColor;
            On.OverseerGraphics.DrawSprites += OverseerGraphics_DrawSprites;
            On.OverseerGraphics.DrawSprites -= Overseer_DrawspritesRemove;
            On.OverseerGraphics.InitiateSprites += OverseerGraphics_InitiateSprites;
            On.OverseerGraphics.InitiateSprites -= OverseerGraphics_RemoveSprites;
            On.OverseerGraphics.ColorOfSegment += OverseerGraphics_ColorOfSegment;
            On.OverseerGraphics.ApplyPalette += OverseerGraphics_ApplyPalette;
            // random overseers

            //------------------------------------ REAL THINGS
            // omg so real

            On.GlobalRain.DeathRain.NextDeathRainMode += DeathRain_NextDeathRainMode;
            // rain does not instakill, but always has precycles
        }

        private void OverseerGraphics_ApplyPalette(On.OverseerGraphics.orig_ApplyPalette orig, OverseerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            if (self.owner.room != null && self.overseer != null &&
                self.owner.room.game.session.characterStats.name.value == "NCRAdream")
            {
                UnityEngine.Random.State state = UnityEngine.Random.state;
                UnityEngine.Random.InitState(self.owner.abstractPhysicalObject.ID.RandomSeed);

                self.ApplyPalette(sLeaser, rCam, palette);
                self.earthColor = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);

                UnityEngine.Random.state = state;
            }
            else
            {
                orig(self, sLeaser, rCam, palette);
            }
        }

        private void OverseerGraphics_DrawSprites(On.OverseerGraphics.orig_DrawSprites orig, OverseerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);
            if (self.owner.room != null && self.overseer != null &&
                self.owner.room.game.session.characterStats.name.value == "NCRAdream")
            {
                UnityEngine.Random.State state = UnityEngine.Random.state;
                UnityEngine.Random.InitState(self.owner.abstractPhysicalObject.ID.RandomSeed);

                sLeaser.sprites[self.WhiteSprite].color = Color.Lerp(self.ColorOfSegment(0.75f, timeStacker),
                    new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value), 0.5f);
                sLeaser.sprites[self.InnerGlowSprite].color =
                    new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value, 0.5f);

                UnityEngine.Random.state = state;
            }
            else if (self.owner.room != null && self.overseer != null)
            {
                sLeaser.sprites[self.WhiteSprite].color = Color.Lerp(self.ColorOfSegment(0.75f, timeStacker), new Color(0f, 0f, 1f), 0.5f);
            }
        }

        private void Overseer_DrawspritesRemove(On.OverseerGraphics.orig_DrawSprites orig, OverseerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            sLeaser.sprites[self.WhiteSprite].color = Color.Lerp(self.ColorOfSegment(0.75f, timeStacker), new Color(0f, 0f, 1f), 0.5f);
        }

        private Color OverseerGraphics_ColorOfSegment(On.OverseerGraphics.orig_ColorOfSegment orig, OverseerGraphics self, float f, float timeStacker)
        {
            if (self.owner != null && self != null && self.overseer != null && self.overseer.room != null &&
                self.overseer.room.world.game.session.characterStats.name.value == "NCRAdream")
            {
                UnityEngine.Random.State state = UnityEngine.Random.state;
                UnityEngine.Random.InitState(self.owner.abstractPhysicalObject.ID.RandomSeed);

                Color randcolour = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);

                UnityEngine.Random.state = state;
                return Color.Lerp(Color.Lerp(Custom.RGB2RGBA((self.MainColor +
                    randcolour + self.earthColor * 8f) / 10f, 0.5f), Color.Lerp(self.MainColor, Color.Lerp(self.NeutralColor,
                    self.earthColor, Mathf.Pow(f, 2f)), 0.5f),
                    self.ExtensionOfSegment(f, timeStacker)), Custom.RGB2RGBA(self.MainColor, 0f),
                    Mathf.Lerp(self.overseer.lastDying, self.overseer.dying, timeStacker));
            }
            else
            {
                return orig(self, f, timeStacker);
            }
        }

        private void OverseerGraphics_InitiateSprites(On.OverseerGraphics.orig_InitiateSprites orig, OverseerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            orig(self, sLeaser, rCam);
            if (self.owner != null && self.overseer.room != null && self.overseer != null &&
                // making sure no values are null
                self.overseer.room.world.game.session.characterStats.name.value == "NCRAdream")
            {
                UnityEngine.Random.State state = UnityEngine.Random.state;
                UnityEngine.Random.InitState(self.owner.abstractPhysicalObject.ID.RandomSeed);

                sLeaser.sprites[self.PupilSprite].color = new Color(UnityEngine.Random.value, UnityEngine.Random.value,
                    UnityEngine.Random.value, 0.5f);

                UnityEngine.Random.state = state;
            }
            else
            {
                sLeaser.sprites[self.PupilSprite].color = new Color(0f, 0f, 0f, 0.5f);
            }
        }

        private void OverseerGraphics_RemoveSprites(On.OverseerGraphics.orig_InitiateSprites orig, OverseerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites[self.PupilSprite].color = new Color(0f, 0f, 0f, 0.5f);
        }

        private void Mycelium_UpdateColor(On.CoralBrain.Mycelium.orig_UpdateColor orig, Mycelium self, Color newColor, float gradientStart, int spr, RoomCamera.SpriteLeaser sLeaser)
        {
            if (self.owner != null && self.owner.OwnerRoom != null &&
                self.owner.OwnerRoom.game.session.characterStats.name.value == "NCRAdream")
            {
                self.color = newColor;
                UnityEngine.Random.State state = UnityEngine.Random.state;
                if (self.owner is Creature)
                {
                    UnityEngine.Random.InitState((self.owner as Creature).abstractPhysicalObject.ID.RandomSeed);
                    // if the owner of the mycelium is a creature, it calls upon the ID of it to determine what seed to use
                }
                else if (self.owner is GraphicsModule && (self.owner as GraphicsModule).owner is Creature)
                {
                    UnityEngine.Random.InitState((self.owner as GraphicsModule).owner.abstractPhysicalObject.ID.RandomSeed);
                    // if the owner of the mycelium is a graphics module BELONGING to a creature, it calls upon the OWNER of the module
                    // to determine the seed
                }

                for (int i = 0; i < (sLeaser.sprites[spr] as TriangleMesh).verticeColors.Length; i++)
                {
                     float value = (float)i / (float)((sLeaser.sprites[spr] as TriangleMesh).verticeColors.Length - 1);
                     (sLeaser.sprites[spr] as TriangleMesh).verticeColors[i] = Color.Lerp(self.color,
                      Custom.HSL2RGB(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value),
                      Mathf.InverseLerp(gradientStart, 1f, value));
                }
                for (int j = 1; j < 3; j++)
                {
                     (sLeaser.sprites[spr] as TriangleMesh).verticeColors[(sLeaser.sprites[spr] as TriangleMesh).verticeColors.Length - j] =
                        new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
                }

                UnityEngine.Random.state = state;
            }
            else
            {
                orig(self, newColor, gradientStart, spr, sLeaser);
            }
        }

        public Color OverseerGraphics_MainColor_get(Plugin.orig_OverseerMainColor orig, global::OverseerGraphics self)
        {
            if (self.owner != null && self.owner.room != null &&
                self.overseer.room.world.game.session.characterStats.name.value == "NCRAdream")
            {
                UnityEngine.Random.State state = UnityEngine.Random.state;
                UnityEngine.Random.InitState(self.owner.abstractPhysicalObject.ID.RandomSeed);

                Color col = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);

                UnityEngine.Random.state = state;
                return col;
            }
            else
            {
                return orig(self);
            }
        }

        private void RegionGate_customKarmaGateRequirements(On.RegionGate.orig_customKarmaGateRequirements orig, RegionGate self)
        {
            if (self.room.game.session.characterStats.name.value == "NCRAreal")
            {
                if (self.room.abstractRoom.name == "GATE_UW_SL")
                {
                    int num;
                    if (int.TryParse(self.karmaRequirements[0].value, out num))
                    {
                        self.karmaRequirements[0] = RegionGate.GateRequirement.OneKarma;
                    }
                    int num2;
                    if (int.TryParse(self.karmaRequirements[1].value, out num2))
                    {
                        self.karmaRequirements[1] = RegionGate.GateRequirement.TwoKarma;
                    }
                }
            }
            else
            {
                orig(self);
            }
        }

        private void DeathRain_NextDeathRainMode(On.GlobalRain.DeathRain.orig_NextDeathRainMode orig, GlobalRain.DeathRain self)
        {
            if (self.globalRain.game.session.characterStats.name.value == "NCRAreal"
                && (self.deathRainMode == GlobalRain.DeathRain.DeathRainMode.AlternateBuildUp ||
                self.deathRainMode == MoreSlugcatsEnums.DeathRainMode.Pulses))
            {
                self.deathRainMode = GlobalRain.DeathRain.DeathRainMode.GradeABuildUp;
                // if the game is a NCRAreal game, the rain never actually kills. instead, it loops back to the beginning
            }
            else
            {
                orig(self);
            }
        }

        private float Player_DeathByBiteMultiplier(On.Player.orig_DeathByBiteMultiplier orig, Player self)
        {
            if (self.room != null && self != null &&
                self.room.game.IsStorySession && self.room.game.session.characterStats.name.value == "NCRAdream")
            {
                if (self.GetDreamCat().InTheNightmare)
                {
                    return 0.9f + self.room.game.GetStorySession.difficulty / 5f;
                    // bites usually kill when in the nightmare state
                }
                else
                {
                    return 0f;
                    // bites never kill (smth smth "cant die in dreams" rumor)
                }
            }
            else if (self.room != null && self != null &&
                self.room.game.IsStorySession && self.room.game.session.characterStats.name.value == "NCRAreal")
            {
                // bites kill just a little bit more than usual
                return 0.75f + self.room.game.GetStorySession.difficulty / 5f;
            }
            else
            {
                return orig(self);
            }
        }

        private void PoleMimicGraphics_DrawSprites(On.PoleMimicGraphics.orig_DrawSprites orig, PoleMimicGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);
            if (self != null && self.owner != null && !self.owner.slatedForDeletetion &&
                // makes sure base values arent null
                self.owner.room.game.session.characterStats.name.value == "NCRAdream")
            {
                UnityEngine.Random.State state = UnityEngine.Random.state;
                UnityEngine.Random.InitState(self.owner.abstractPhysicalObject.ID.RandomSeed);
                for (int i = 0; i < self.leafPairs; i++)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        
                        if (i < self.decoratedLeafPairs)
                        {
                            sLeaser.sprites[self.LeafDecorationSprite(i, j)].color = Color.Lerp(new Color(UnityEngine.Random.value, UnityEngine.Random.value,
                                UnityEngine.Random.value), new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value),
                                Mathf.Pow(Mathf.InverseLerp((float)(self.decoratedLeafPairs / 2), (float)self.decoratedLeafPairs,
                                (float)i), 0.6f));
                        }
                    }
                }
                UnityEngine.Random.state = state;
            }
        }

        private void TentaclePlantGraphics_ApplyPalette(On.TentaclePlantGraphics.orig_ApplyPalette orig, TentaclePlantGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            if (self != null && self.owner != null && !self.owner.slatedForDeletetion &&
                // makes sure base values arent null
                self.owner.room.game.session.characterStats.name.value == "NCRAdream")
            {
                UnityEngine.Random.State state = UnityEngine.Random.state;
                UnityEngine.Random.InitState(self.owner.abstractPhysicalObject.ID.RandomSeed);
                sLeaser.sprites[0].color = palette.blackColor;
                for (int i = 0; i < self.danglers.Length; i++)
                {
                    sLeaser.sprites[i + 1].color = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
                }
                UnityEngine.Random.state = state;
            }
            else
            {
                orig(self, sLeaser, rCam, palette);
            }
        }

        private void PlayerGraphics_ctor(On.PlayerGraphics.orig_ctor orig, PlayerGraphics self, PhysicalObject ow)
        {
            orig(self, ow);
            if ((self.owner as Player).GetDreamCat().IsDream)
            {
                self.tail[0] = new TailSegment(self, 6f, 4.5f, null, 0.85f, 1f, 1f, true);
                self.tail[1] = new TailSegment(self, 4f, 7.5f, self.tail[0], 0.85f, 0.45f, 0.5f, true);
                self.tail[2] = new TailSegment(self, 3f, 7f, self.tail[1], 0.85f, 0.4f, 0.5f, true);
                self.tail[3] = new TailSegment(self, 9f, 7f, self.tail[2], 0.80f, 0.4f, 0.5f, true);
                // this gives dream its signature tail curl. in all honesty im not sure WHY it does, but hey
            }
            else if ((self.owner as Player).GetRealCat().IsReal)
            {
                self.tail[0] = new TailSegment(self, 5.5f, 4f, null, 0.85f, 1f, 1f, true);
                self.tail[1] = new TailSegment(self, 4f, 7f, self.tail[0], 0.90f, 1f, 0.6f, true);
                self.tail[2] = new TailSegment(self, 2.5f, 4.5f, self.tail[1], 0.85f, 1f, 0.5f, true);
                self.tail[3] = new TailSegment(self, 1f, 3.5f, self.tail[2], 0.85f, 1f, 0.5f, true);
                // self.tail[value] = tailsegment(owner, radius?, connectionradius?, connectedtailsegment[value],
                // surfacefriction, airfriction, affect on previous segment, pullinpreviousposition)
                // this makes reals tail appear slightly stumpier (despite it actually being the same length)
            }
        }

        private bool StowAwake(On.MoreSlugcats.StowawayBugState.orig_AwakeThisCycle orig, StowawayBugState self, int cycle)
        {
            if (self.creature.world.game.session.characterStats.name.value == "NCRAreal")
            {
                Debug.Log("Stowaway forced awake by NCRAreal");
                return true;
            }
            else if (self.creature.world.game.session.characterStats.name.value == "NCRAdream")
            {
                Debug.Log("NCRAdream game! Stowaway asleep");
                return false;
            }
            else return orig(self, cycle);

        }

        private void DangleFruit_ApplyPalette(On.DangleFruit.orig_ApplyPalette orig, DangleFruit self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            if (self.room.game.session.characterStats.name.value == "NCRAdream" && !self.slatedForDeletetion)
            {
                UnityEngine.Random.State state = UnityEngine.Random.state;
                UnityEngine.Random.InitState(self.abstractPhysicalObject.ID.RandomSeed);
                sLeaser.sprites[0].color = palette.blackColor;
                self.color = Color.Lerp(new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value), palette.blackColor, self.darkness);
                UnityEngine.Random.state = state;
            }
            else orig(self, sLeaser, rCam, palette);
        }

        private void Lantern_Update(On.Lantern.orig_Update orig, Lantern self, bool eu)
        {
            if (self != null && self.room != null && !self.slatedForDeletetion &&
                // makes sure base values arent null
                (self.room.game.session.characterStats.name.value == "NCRAdream") && self.lightSource == null)
            {
                UnityEngine.Random.State state = UnityEngine.Random.state;
                UnityEngine.Random.InitState(self.abstractPhysicalObject.ID.RandomSeed);
                self.lightSource = new LightSource(self.firstChunk.pos, false, new UnityEngine.Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value), self);
                self.room.AddObject(self.lightSource);
                UnityEngine.Random.state = state;
            }
            else
            {
                orig.Invoke(self, eu);
            }
        }

        private void Lantern_ApplyPalette(On.Lantern.orig_ApplyPalette orig, Lantern self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            if (self != null && self.room != null && !self.slatedForDeletetion &&
                // makes sure base values arent null
                self.room.game.session.characterStats.name.value == "NCRAdream")
            {
                UnityEngine.Random.State state = UnityEngine.Random.state;
                UnityEngine.Random.InitState(self.abstractPhysicalObject.ID.RandomSeed);

                sLeaser.sprites[0].color = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
                sLeaser.sprites[1].color = new Color(1f, 1f, 1f);
                sLeaser.sprites[2].color = Color.Lerp(new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value),
                    new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value), 0.3f);
                sLeaser.sprites[3].color = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
                if (self.stick != null)
                {
                    sLeaser.sprites[4].color = palette.blackColor;
                }

                UnityEngine.Random.state = state;
            }
            else
            {
                orig(self, sLeaser, rCam, palette);
            }
        }

        private void FlyGraphics_ApplyPalette(On.FlyGraphics.orig_ApplyPalette orig, FlyGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            if (self != null && self.owner != null && !self.owner.slatedForDeletetion &&
                // makes sure base values arent null
                self.owner.room.game.session.characterStats.name.value == "NCRAdream")
            {
                UnityEngine.Random.State state = UnityEngine.Random.state;
                UnityEngine.Random.InitState(self.fly.abstractCreature.ID.RandomSeed);
                for (int i = 0; i < 3; i++)
                {
                    sLeaser.sprites[i].color = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
                }
                UnityEngine.Random.state = state;
            }
            else orig(self, sLeaser, rCam, palette);
        }

        private void PoleMimicGraphics_ApplyPalette(On.PoleMimicGraphics.orig_ApplyPalette orig, PoleMimicGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            if (self != null && self.owner != null && !self.owner.slatedForDeletetion &&
                // makes sure base values arent null
                self.owner.room.game.session.characterStats.name.value == "NCRAdream")
            {
                UnityEngine.Random.State state = UnityEngine.Random.state;
                UnityEngine.Random.InitState(self.pole.abstractCreature.ID.RandomSeed);
                self.blackColor = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
                self.mimicColor = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
                UnityEngine.Random.state = state;
            }
            else if (self != null && self.owner != null && !self.owner.slatedForDeletetion &&
                self.owner.room.game.session.characterStats.name.value == "NCRAdream")
            {
                // this does not ordinarily trigger yet. this is a placeholder for when the nightmare state is operational.
                UnityEngine.Random.State state = UnityEngine.Random.state;
                UnityEngine.Random.InitState(self.pole.abstractCreature.ID.RandomSeed);
                self.blackColor = palette.blackColor;
                self.mimicColor = new Color(0.01f, 0.01f, 0.01f);
                UnityEngine.Random.state = state;
            }
            else
            {
                orig(self, sLeaser, rCam, palette);
            }
        }

        private void FlyLure_ApplyPalette(On.FlyLure.orig_ApplyPalette orig, FlyLure self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            if (self != null && self.room != null && !self.slatedForDeletetion &&
                // makes sure base values arent null
                self.room.game.session.characterStats.name.value == "NCRAdream")
            {
                self.color = Color.Lerp(new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value), palette.fogColor, UnityEngine.Random.value);
                self.UpdateColor(sLeaser, false);
            }
            else
            {
                orig(self, sLeaser, rCam, palette);
            }
        }


        private void Lantern_TerrainImpact(On.Lantern.orig_TerrainImpact orig, Lantern self, int chunk, RWCustom.IntVector2 direction, float speed, bool firstContact)
        {
            if (self != null && self.room != null && !self.slatedForDeletetion &&
                speed > 5f && firstContact && (self.room.game.session.characterStats.name.value == "NCRAdream"))
            {
                Vector2 pos = self.bodyChunks[chunk].pos + direction.ToVector2() * self.bodyChunks[chunk].rad * 0.9f;
                int num = 0;
                while ((float)num < Mathf.Round(Custom.LerpMap(speed, 5f, 15f, 2f, 8f)))
                {
                    self.room.AddObject(new Spark(pos, direction.ToVector2() * Custom.LerpMap(speed, 5f, 15f, -2f, -8f) + Custom.RNV() *
                        UnityEngine.Random.value * Custom.LerpMap(speed, 5f, 15f, 2f, 4f), Color.Lerp(new Color(UnityEngine.Random.value,
                        UnityEngine.Random.value, UnityEngine.Random.value), new Color(UnityEngine.Random.value, UnityEngine.Random.value,
                        UnityEngine.Random.value), UnityEngine.Random.value * 0.5f), null, 19, 47));
                    // each spark has its own unique and random colour. This colour may change when using camerascroll, so prepare to need
                    // to fix that.
                    num++;
                }
            }
            else
            {
                orig(self, chunk, direction, speed, firstContact);
            }
        }

        private void SeedCob_ApplyPalette(On.SeedCob.orig_ApplyPalette orig, SeedCob self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            if (self != null && self.room != null && !self.slatedForDeletetion &&
                self.room.game.session.characterStats.name.value == "NCRAdream")
            {
                UnityEngine.Random.State state = UnityEngine.Random.state;
                UnityEngine.Random.InitState(self.abstractPhysicalObject.ID.RandomSeed);
                sLeaser.sprites[self.StalkSprite(0)].color = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
                self.StoredBlackColor = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
                Color pixel = palette.texture.GetPixel(0, 5);
                self.StoredPlantColor = pixel;
                for (int i = 0; i < (sLeaser.sprites[self.StalkSprite(1)] as TriangleMesh).verticeColors.Length; i++)
                {
                    float num = (float)i / (float)((sLeaser.sprites[self.StalkSprite(1)] as TriangleMesh).verticeColors.Length - 1);
                    (sLeaser.sprites[self.StalkSprite(1)] as TriangleMesh).verticeColors[i] = Color.Lerp(palette.blackColor, pixel, 0.4f +
                        Mathf.Pow(1f - num, 0.5f) * 0.4f);
                }
                self.yellowColor = Color.Lerp(new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value), palette.blackColor, self.AbstractCob.dead 
                    ? (0.95f + 0.5f * rCam.PaletteDarkness()) : (0.18f + 0.7f * rCam.PaletteDarkness()));
                for (int j = 0; j < 2; j++)
                {
                    for (int k = 0; k < (sLeaser.sprites[self.ShellSprite(j)] as TriangleMesh).verticeColors.Length; k++)
                    {
                        float num2 = 1f - (float)k / (float)((sLeaser.sprites[self.ShellSprite(j)] as TriangleMesh).verticeColors.Length - 1);
                        (sLeaser.sprites[self.ShellSprite(j)] as TriangleMesh).verticeColors[k] = Color.Lerp(palette.blackColor,
                            new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value), Mathf.Pow(num2, 2.5f) * 0.4f);
                    }
                }
                sLeaser.sprites[self.CobSprite].color = self.yellowColor;
                Color color = self.yellowColor + new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value) *
                    Mathf.Lerp(1f, 0.15f, rCam.PaletteDarkness());
                if (self.AbstractCob.dead)
                {
                    color = Color.Lerp(self.yellowColor, pixel, 0.75f);
                }
                for (int l = 0; l < self.seedPositions.Length; l++)
                {
                    sLeaser.sprites[self.SeedSprite(l, 0)].color = self.yellowColor;
                    sLeaser.sprites[self.SeedSprite(l, 1)].color = color;
                    sLeaser.sprites[self.SeedSprite(l, 2)].color = Color.Lerp(new Color(UnityEngine.Random.value, UnityEngine.Random.value,
                        UnityEngine.Random.value), palette.blackColor, self.AbstractCob.dead ? 0.6f : 0.3f);
                }
                for (int m = 0; m < self.leaves.GetLength(0); m++)
                {
                    sLeaser.sprites[self.LeafSprite(m)].color = palette.blackColor;
                }
            UnityEngine.Random.state = state;
            }
            else
            {
                orig.Invoke(self, sLeaser, rCam, palette);
            }
        }

        private void Player_UpdateMSC(On.Player.orig_UpdateMSC orig, Player self)
        {
            orig(self);
            if (self.GetDreamCat().IsDream && self.room.gravity >= 0.55f && !self.submerged)
            {
                // !submerged is necessary, as without that, dream is completely incapable of swimming.
                // this as a whole enables dream to have the dream-like bouncing, fall slowly, jump higher, ect.
                self.buoyancy = 0.96f;
                self.customPlayerGravity = 0.35f;
            }
            else if (self.GetRealCat().IsReal)
            {
                self.buoyancy = 0.92f;
            }
        }

        private void Player_ctor(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
        {
            orig(self, abstractCreature, world);
            // ---------------------------------------------------- DREAM ----------------------------------------------------
            if (self.slugcatStats.name.value == "NCRAdream")
            {
                self.GetDreamCat().IsDream = true;

                
            }
            // ---------------------------------------------------- DREAM STORY ----------------------------------------------------
            if (self.room.game.session.characterStats.name.value == "NCRAdream")
            {
                self.GetDreamCat().DreamActive = true;
            }



            // ---------------------------------------------------- REAL ----------------------------------------------------
            if (self.slugcatStats.name.value == "NCRAreal")
            {
                self.GetRealCat().IsReal = true;
            }
            // ---------------------------------------------------- REAL STORY ----------------------------------------------------
            if (self.room.game.session.characterStats.name.value == "NCRreal")
            {
                self.GetRealCat().RealActive = true;
            }
        }

        private void GreenSpark_Update(On.GreenSparks.GreenSpark.orig_Update orig, GreenSparks.GreenSpark self, bool eu)
        {
            if (self != null && self.room != null && !self.slatedForDeletetion &&
                // makes sure base values arent null. this effect especially needs it.
                self.room.game.session.characterStats.name.value == "NCRAdream")
            {
                UnityEngine.Random.State state = UnityEngine.Random.state;
                self.col = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
                orig(self, eu);
                UnityEngine.Random.state = state;
                // this should make it so the effect DOES NOT change upon changing screens. hopefully.
                // otherwise, SBCameraScroll will have an issue where it swaps rapidly between random values,
                // causing flashes...
            }
            else orig(self, eu);
        }

        private void GreenSpark_ApplyPalette(On.GreenSparks.GreenSpark.orig_ApplyPalette orig, GreenSparks.GreenSpark self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            if (self != null && self.room != null && !self.slatedForDeletetion &&
                self.room.game.session.characterStats.name.value == "NCRAdream")
            {
                UnityEngine.Random.State state = UnityEngine.Random.state;

                if (self.depth <= 0f)
                {
                    sLeaser.sprites[0].color = self.col;
                    return;
                }
                sLeaser.sprites[0].color = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);

                UnityEngine.Random.state = state;
            }
            else orig(self, sLeaser, rCam, palette);
        }

        private void FireFly_ctor(On.FireFly.orig_ctor orig, FireFly self, Room room, Vector2 pos)
        {
            orig(self, room, pos);
            if (self != null && self.room != null && !self.slatedForDeletetion &&
                room.game.session.characterStats.name.value == "NCRAdream")
            {
                self.col = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
            }
        }

        private void SSOracleSwarmer_Update(On.SSOracleSwarmer.orig_Update orig, SSOracleSwarmer self, bool eu)
        {
            orig(self, eu);
            if (self.room.readyForAI && self != null && self.room != null &&
                self.room.game.session.characterStats.name.value == "NCRAdream")
            {
                self.affectedByGravity = 0f;
                // makes it so oracleswarmers are never affected by gravity, regardless of the actual room gravity
            }
        }

        private bool SlugcatStats_SlugcatUnlocked(On.SlugcatStats.orig_SlugcatUnlocked orig, SlugcatStats.Name i, RainWorld rainWorld)
        {
            if (i.value == "NCRAdream" || i.value == "NCRAreal")
            {
                return true;
                // this is here primarily to be altered later on.
            }
            else return orig(i, rainWorld);
        }

        private void LoadResources(RainWorld rainWorld)
        {

        }
    }
}