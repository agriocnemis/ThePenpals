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
using System.Xml.Schema;
using On;
using IL;
using System.Collections.Generic;

namespace NCRApenpals
{
    [BepInPlugin(MOD_ID, "NCRApenpals", "0.0.0")]
    class Plugin : BaseUnityPlugin
    {
        private const string MOD_ID = "neoncityrain-agriocnemis.penpals";
        public delegate Color orig_OverseerMainColor(global::OverseerGraphics self);

        public void OnEnable()
        {
            On.RainWorld.OnModsInit += NCRAExtras.WrapInit(LoadResources);
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
            On.CentipedeGraphics.ApplyPalette += CentipedeGraphics_ApplyPalette;
            On.CentipedeGraphics.ctor += CentipedeGraphics_ctor;
            On.SpiderGraphics.ApplyPalette += SpiderGraphics_ApplyPalette;
            On.BigSpiderGraphics.ApplyPalette += BigSpiderGraphics_ApplyPalette;
            // making dream colourful
            //testing atm if can be simplified -Y
            // it cant :') - A
            Hook fancyoverseers = new Hook(typeof(global::OverseerGraphics).GetProperty("MainColor", BindingFlags.Instance |
                BindingFlags.Public).GetGetMethod(), new Func<orig_OverseerMainColor,
                OverseerGraphics, Color>(this.OverseerGraphics_MainColor_get));
            On.CoralBrain.Mycelium.UpdateColor += Mycelium_UpdateColor;
            On.OverseerGraphics.DrawSprites += OverseerGraphics_DrawSprites;
            On.OverseerGraphics.DrawSprites -= Overseer_DrawspritesRemove;
            On.OverseerGraphics.InitiateSprites += OverseerGraphics_InitiateSprites;
            On.OverseerGraphics.InitiateSprites -= OverseerGraphics_RemoveSprites;
            On.OverseerGraphics.ColorOfSegment += OverseerGraphics_ColorOfSegment;
            // random overseers

            On.DaddyGraphics.DrawSprites += DaddyGraphics_DrawSprites;
            On.DaddyGraphics.Eye.GetColor += Eye_GetColor;
            On.DaddyGraphics.DaddyTubeGraphic.ApplyPalette += DaddyTubeGraphic_ApplyPalette;
            On.DaddyGraphics.DaddyDeadLeg.ApplyPalette += DaddyDeadLeg_ApplyPalette;
            On.DaddyGraphics.DaddyDangleTube.ApplyPalette += DaddyDangleTube_ApplyPalette;
            // daddy stuff. winks

            On.DaddyGraphics.HunterDummy.ApplyPalette += HunterDummy_ApplyPalette;
            On.DaddyGraphics.HunterDummy.ctor += HunterDummy_ctor;
            // insomniac long legs

            //------------------------------------ REAL THINGS
            // omg so real

            On.GlobalRain.DeathRain.NextDeathRainMode += DeathRain_NextDeathRainMode;
            // rain does not instakill... or well. it SHOULDNT. it still does tho
        }

        private void DaddyDangleTube_ApplyPalette(On.DaddyGraphics.DaddyDangleTube.orig_ApplyPalette orig, DaddyGraphics.DaddyDangleTube self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            if ()
            {
                Color color = palette.blackColor;
                if (this.owner.daddy.HDmode)
                {
                    color = Color.Lerp(PlayerGraphics.DefaultSlugcatColor(SlugcatStats.Name.Red), Color.gray, 0.4f);
                }
                for (int i = 0; i < (sLeaser.sprites[this.firstSprite] as TriangleMesh).vertices.Length; i++)
                {
                    float floatPos = Mathf.InverseLerp(0.3f, 1f, (float)i / (float)((sLeaser.sprites[this.firstSprite] as TriangleMesh).vertices.Length - 1));
                    (sLeaser.sprites[this.firstSprite] as TriangleMesh).verticeColors[i] = Color.Lerp(color, this.owner.EffectColor, this.OnTubeEffectColorFac(floatPos));
                }
                sLeaser.sprites[this.firstSprite].color = color;
                for (int j = 0; j < this.bumps.Length; j++)
                {
                    sLeaser.sprites[this.firstSprite + 1 + j].color = Color.Lerp(color, this.owner.EffectColor, this.OnTubeEffectColorFac(this.bumps[j].pos.y));
                }
            }
            else
            {
                orig(self, sLeaser, rCam, palette);
            }
        }

        private void DaddyDeadLeg_ApplyPalette(On.DaddyGraphics.DaddyDeadLeg.orig_ApplyPalette orig, DaddyGraphics.DaddyDeadLeg self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            if (self.owner.owner.room != null && self.owner != null && self != null && self.owner.owner != null &&
                self.owner.owner.room.game.session.characterStats.name.value == "NCRAdream")
            {
                Color color = new Color(0f, 0f, 0f);
                Color effect = new Color(1f, 0f, 0f);
                if (self.owner.daddy.HDmode)
                {
                    color = Color.Lerp(new Color(0.39f, 0.25f, 0.25f), Color.gray, 0.4f);
                }
                for (int i = 0; i < (sLeaser.sprites[self.firstSprite] as TriangleMesh).vertices.Length; i++)
                {
                    float floatPos = Mathf.InverseLerp(0.3f, 1f, (float)i / (float)((sLeaser.sprites[self.firstSprite] as
                        TriangleMesh).vertices.Length - 1));
                    (sLeaser.sprites[self.firstSprite] as TriangleMesh).verticeColors[i] = Color.Lerp(color, effect,
                        self.OnTubeEffectColorFac(floatPos));
                }
                int num = 0;
                for (int j = 0; j < self.bumps.Length; j++)
                {
                    sLeaser.sprites[self.firstSprite + 1 + j].color = Color.Lerp(color, effect, self.OnTubeEffectColorFac(self.bumps[j].pos.y));
                    if (self.bumps[j].eyeSize > 0f)
                    {
                        sLeaser.sprites[self.firstSprite + 1 + self.bumps.Length + num].color = (self.owner.colorClass ?
                            (effect * Mathf.Lerp(0.5f, 0.2f, self.deadness)) : color);
                        num++;
                    }
                }
            }
            else
            {
                orig(self, sLeaser, rCam, palette);
            }
        }

        private void DaddyTubeGraphic_ApplyPalette(On.DaddyGraphics.DaddyTubeGraphic.orig_ApplyPalette orig, DaddyGraphics.DaddyTubeGraphic self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            if (self.owner.owner.room != null && self.owner != null && self != null && self.owner.owner != null &&
                self.owner.owner.room.game.session.characterStats.name.value == "NCRAdream")
            {
                Color color = new Color(0f, 0f, 0f);
                Color effect = new Color(1f, 0f, 0f);
                if (self.owner.daddy.HDmode)
                {
                    color = Color.Lerp(new Color(0.39f, 0.25f, 0.25f), Color.gray, 0.4f);
                }
                for (int i = 0; i < (sLeaser.sprites[self.firstSprite] as TriangleMesh).vertices.Length; i++)
                {
                    float floatPos = Mathf.InverseLerp(0.3f, 1f, (float)i / (float)((sLeaser.sprites[self.firstSprite] as
                        TriangleMesh).vertices.Length - 1));
                    (sLeaser.sprites[self.firstSprite] as TriangleMesh).verticeColors[i] = Color.Lerp(color,
                        effect, self.OnTubeEffectColorFac(floatPos));
                }
                int num = 0;
                for (int j = 0; j < self.bumps.Length; j++)
                {
                    sLeaser.sprites[self.firstSprite + 1 + j].color = Color.Lerp(color, effect,
                        self.OnTubeEffectColorFac(self.bumps[j].pos.y));
                    if (self.bumps[j].eyeSize > 0f)
                    {
                        sLeaser.sprites[self.firstSprite + 1 + self.bumps.Length + num].color =
                            (self.owner.colorClass ? effect : color);
                        num++;
                    }
                }
            }
            else
            {
                orig(self, sLeaser, rCam, palette);
            }
        }

        private void HunterDummy_ctor(On.DaddyGraphics.HunterDummy.orig_ctor orig, DaddyGraphics.HunterDummy self, DaddyGraphics dg, int startSprite)
        {
            orig(self, dg, startSprite);
            if (self.owner.owner.room != null && self.owner != null && self != null && self.owner.owner != null &&
                self.owner.owner.room.game.session.characterStats.name.value == "NCRAdream")
            {
                self.tail[0] = new TailSegment(self.owner, 5.5f, 4f, null, 0.85f, 1f, 1f, true);
                self.tail[1] = new TailSegment(self.owner, 4f, 7f, self.tail[0], 0.8f, 1f, 0.6f, true);
                self.tail[2] = new TailSegment(self.owner, 2.5f, 4.5f, self.tail[1], 0.82f, 1f, 0.5f, true);
                self.tail[3] = new TailSegment(self.owner, 1f, 1f, self.tail[2], 1f, 1f, 0.5f, true);
            }
        }

        private void HunterDummy_ApplyPalette(On.DaddyGraphics.HunterDummy.orig_ApplyPalette orig, DaddyGraphics.HunterDummy self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            if (self.owner.owner.room != null && self.owner != null && self != null && self.owner.owner != null &&
                self.owner.owner.room.game.session.characterStats.name.value == "NCRAdream")
            {
                Color color = Color.Lerp(new Color(0.39f, 0.25f, 0.25f), Color.gray, 0.4f);
                // insomniac body colour. may add statements for if insomniac has custom colours?
                Color blackColor = new Color(0.96f, 0.83f, 0.76f);
                // insomniac eye colour.
                for (int i = 0; i < self.numberOfSprites - 1; i++)
                {
                    sLeaser.sprites[self.startSprite + i].color = color;
                }
                sLeaser.sprites[self.startSprite + 5].color = blackColor;
            }
            else
            {
                orig(self, sLeaser, rCam, palette);
            }
        }

        private Color Eye_GetColor(On.DaddyGraphics.Eye.orig_GetColor orig, DaddyGraphics.Eye self)
        {
            if (self.owner.owner.room != null && self.owner != null && self != null && self.owner.owner != null &&
                self.owner.owner.room.game.session.characterStats.name.value == "NCRAdream")
            {
                self.renderColor = new Color(1f, 0f, 0f);
                return new Color(1f, 0f, 0f);
            }
            else
            {
                return orig(self);
            }
        }

        private void DaddyGraphics_DrawSprites(On.DaddyGraphics.orig_DrawSprites orig, DaddyGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            if (self.owner.room != null && self.owner != null && self != null &&
                self.owner.room.game.session.characterStats.name.value == "NCRAdream")
            {
                self.blackColor = new Color(1f, 0f, 0f);
                // this alters the colour the eyes turn when "blinking". this does NOT change anything else.
            }
            orig(self, sLeaser, rCam, timeStacker, camPos);
        }

        private void BigSpiderGraphics_ApplyPalette(On.BigSpiderGraphics.orig_ApplyPalette orig, BigSpiderGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            if (self.owner.room != null && self.owner != null && self != null &&
                self.owner.room.game.session.characterStats.name.value == "NCRAdream")
            {
                UnityEngine.Random.State state = UnityEngine.Random.state;
                UnityEngine.Random.InitState(self.bug.abstractCreature.ID.RandomSeed);

                float num;
                self.blackColor = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
                self.yellowCol = Color.Lerp(new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value),
                    palette.fogColor, 0.2f);
                num = 1f - self.darkness;
                for (int i = 0; i < sLeaser.sprites.Length; i++)
                {
                    sLeaser.sprites[i].color = self.blackColor;
                }
                for (int j = 0; j < 2; j++)
                {
                    for (int k = 0; k < 4; k++)
                    {
                        (sLeaser.sprites[self.MandibleSprite(j, 1)] as CustomFSprite).verticeColors[k] =
                            new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
                    }
                }
                for (int l = 0; l < self.scales.Length; l++)
                {
                    for (int m = 0; m < self.scales[l].GetLength(0); m++)
                    {
                        float num2 = (Mathf.InverseLerp(0f, (float)(self.scales[l].GetLength(0) - 1),
                            (float)m) + Mathf.InverseLerp(0f, 5f, (float)m)) / 2f;
                        sLeaser.sprites[self.FirstScaleSprite + (int)self.scales[l][m, 3].x].color =
                            Color.Lerp(new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value),
                            new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value), num2 *
                            Mathf.Lerp(0.3f, 0.9f, self.scaleSpecs[l, 0].x) * num);
                    }
                }


                UnityEngine.Random.state = state;
            }
            else
            {
                orig(self, sLeaser, rCam, palette);
            }
        }

        private void SpiderGraphics_ApplyPalette(On.SpiderGraphics.orig_ApplyPalette orig, SpiderGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            if (self.owner.room != null && self.owner != null && self != null &&
                self.owner.room.game.session.characterStats.name.value == "NCRAdream")
            {
                UnityEngine.Random.State state = UnityEngine.Random.state;
                UnityEngine.Random.InitState(self.spider.abstractCreature.ID.RandomSeed);

                Color color = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
                sLeaser.sprites[self.BodySprite].color = color;
                for (int i = 1; i < 17; i++)
                {
                    sLeaser.sprites[i].color = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
                }

                UnityEngine.Random.state = state;
            }
            else
            {
                orig(self, sLeaser, rCam, palette);
            }
        }

        private void CentipedeGraphics_ctor(On.CentipedeGraphics.orig_ctor orig, CentipedeGraphics self, PhysicalObject ow)
        {
            orig(self, ow);
            if (self.owner.room != null && self.owner != null && self != null &&
                self.owner.room.game.session.characterStats.name.value == "NCRAdream")
            {
                UnityEngine.Random.State state = UnityEngine.Random.state;
                UnityEngine.Random.InitState(self.centipede.abstractCreature.ID.RandomSeed);

                self.hue = UnityEngine.Random.value;
                self.saturation = UnityEngine.Random.value;

                UnityEngine.Random.state = state;
            }
        }

        private void CentipedeGraphics_ApplyPalette(On.CentipedeGraphics.orig_ApplyPalette orig, CentipedeGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            if (self.owner.room != null && self.owner != null && self != null &&
                self.owner.room.game.session.characterStats.name.value == "NCRAdream")
            {
                UnityEngine.Random.State state = UnityEngine.Random.state;
                UnityEngine.Random.InitState(self.centipede.abstractCreature.ID.RandomSeed);

                if (self.centipede.Glower != null)
                {
                    self.centipede.Glower.color = Color.Lerp(new Color(palette.waterColor1.r, palette.waterColor1.g, palette.waterColor1.b,
                        1f), new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value, 1f), 0.25f);
                }
                self.blackColor = palette.blackColor;
                for (int i = 0; i < sLeaser.sprites.Length; i++)
                {
                    sLeaser.sprites[i].color = self.blackColor;
                }
                for (int j = 0; j < self.totalSecondarySegments; j++)
                {
                    Mathf.Sin((float)j / (float)(self.totalSecondarySegments - 1) * 3.1415927f);
                    sLeaser.sprites[self.SecondarySegmentSprite(j)].color = Color.Lerp(Custom.HSL2RGB(UnityEngine.Random.value, 1f, 0.2f),
                        self.blackColor, Mathf.Lerp(0.4f, 1f, self.darkness));
                }
                for (int k = 0; k < self.owner.bodyChunks.Length; k++)
                {
                    for (int l = 0; l < 2; l++)
                    {
                        (sLeaser.sprites[self.LegSprite(k, l, 1)] as VertexColorSprite).verticeColors[0] = self.SecondaryShellColor + 
                            new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value, 0.5f);
                        (sLeaser.sprites[self.LegSprite(k, l, 1)] as VertexColorSprite).verticeColors[1] = self.SecondaryShellColor +
                            new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value, 0.5f);
                        (sLeaser.sprites[self.LegSprite(k, l, 1)] as VertexColorSprite).verticeColors[2] = self.blackColor;
                        (sLeaser.sprites[self.LegSprite(k, l, 1)] as VertexColorSprite).verticeColors[3] = self.blackColor;
                    }
                }

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
            if (self != null && self.owner != null && !self.owner.slatedForDeletetion && self.owner.room != null &&
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
                self.tail[0] = new TailSegment(self, 7f, 4.5f, null, 0.85f, 1f, 1f, true);
                self.tail[1] = new TailSegment(self, 5f, 7.5f, self.tail[0], 0.85f, 0.45f, 0.5f, true);
                self.tail[2] = new TailSegment(self, 4f, 7f, self.tail[1], 0.85f, 0.4f, 0.5f, true);
                self.tail[3] = new TailSegment(self, 9.5f, 8f, self.tail[2], 0.92f, 0.4f, 0.5f, true);
                // this gives dream its signature tail curl. in all honesty im not sure WHY it does, but hey
            }
            else if ((self.owner as Player).GetRealCat().IsReal)
            {
                self.tail[0] = new TailSegment(self, 5.5f, 4f, null, 0.85f, 1f, 1f, true);
                self.tail[1] = new TailSegment(self, 4f, 7f, self.tail[0], 0.8f, 1f, 0.6f, true);
                self.tail[2] = new TailSegment(self, 2.5f, 4.5f, self.tail[1], 0.82f, 1f, 0.5f, true);
                self.tail[3] = new TailSegment(self, 1f, 1f, self.tail[2], 1f, 1f, 0.5f, true);
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
                self.mimicColor = palette.blackColor;
                UnityEngine.Random.state = state;
            }
            else if (self != null && self.owner != null && !self.owner.slatedForDeletetion &&
                self.owner.room.game.session.characterStats.name.value == "NCRAreal")
            {
                UnityEngine.Random.State state = UnityEngine.Random.state;
                UnityEngine.Random.InitState(self.pole.abstractCreature.ID.RandomSeed);
                self.blackColor = new Color(0.01f, 0.01f, 0.01f);
                self.mimicColor = palette.blackColor;
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
                UnityEngine.Random.State state = UnityEngine.Random.state;
                UnityEngine.Random.InitState(self.abstractPhysicalObject.ID.RandomSeed);

                self.color = Color.Lerp(new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value), palette.fogColor, UnityEngine.Random.value);
                self.UpdateColor(sLeaser, false);

                UnityEngine.Random.state = state;
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
                UnityEngine.Random.State state = UnityEngine.Random.state;
                self.col = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
                UnityEngine.Random.state = state;
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