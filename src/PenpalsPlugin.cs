using BepInEx;
using UnityEngine;
using MoreSlugcats;
using RWCustom;
using HUD;
using MonoMod.RuntimeDetour;
using System.Reflection;
using System;
using CoralBrain;
using System.Collections.Generic;
using System.IO;


namespace NCRApenpals
{
    [BepInPlugin(MOD_ID, "NCRApenpals", "0.0.0")]
    class Plugin : BaseUnityPlugin
    {
        private const string MOD_ID = "neoncityrain-agriocnemis.penpals";
        public delegate Color orig_OverseerMainColor(global::OverseerGraphics self);

        public static void LoadShaders(RainWorld rainWorld)
        {
            UnityEngine.Debug.Log("Loading Insomniac Shader...");

            string path = AssetManager.ResolveFilePath("shaders/GrayscaleGrab");
            if (!File.Exists(path))
            {
                UnityEngine.Debug.Log("Can't find shader path: " + path);
            }

            AssetBundle bundle = AssetBundle.LoadFromFile(path);
            if (bundle == null)
            {
                UnityEngine.Debug.Log("File is not found in path: " + path);
            }

            Shader shader = bundle.LoadAsset<Shader>("GrayscaleGrab");
            if (shader == null)
            {
                UnityEngine.Debug.Log("Shader not found in path: " + path + ". Cancelling.");
            }
            else
            {
                rainWorld.Shaders["ncrgray"] = FShader.CreateShader("GrayscaleGrab", shader);
                // this is called via rainWorld.Shaders["ncrgray"]

                if (rainWorld.Shaders["ncrgray"] == null)
                {
                    UnityEngine.Debug.Log("Createshader failed. Dying");
                }
                else
                {

                    UnityEngine.Debug.Log("Insomniac Shader loaded properly for once!");
                }
            }
        }

        public void OnEnable()
        {
            // ------------------------------------ ALL THINGS ------------------------------------

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

            On.SSOracleBehavior.SSOracleMeetWhite.Update += SSOracleMeetWhite_Update;
            // fixing oracle issues

            On.RainWorld.OnModsInit += RainWorld_OnModsInit;
            

            



            // ----------------------------------- DREAM THINGS ------------------------------------
            On.SSOracleSwarmer.Update += SSOracleSwarmer_Update;
            // zero-gravity oracle swarmers always

            On.FlyGraphics.ApplyPalette += FlyGraphics_ApplyPalette;
            On.PoleMimicGraphics.ApplyPalette += PoleMimicGraphics_ApplyPalette;
            On.SpiderGraphics.ApplyPalette += SpiderGraphics_ApplyPalette;
            On.WormGrass.Worm.ApplyPalette += Worm_ApplyPalette;
            On.SeedCob.ApplyPalette += SeedCob_ApplyPalette;
            On.GreenSparks.GreenSpark.ApplyPalette += GreenSpark_ApplyPalette;
            On.Lantern.TerrainImpact += Lantern_TerrainImpact;
            On.FlyLure.ApplyPalette += FlyLure_ApplyPalette;
            On.Lantern.ApplyPalette += Lantern_ApplyPalette;
            On.Lantern.Update += Lantern_Update;
            On.DangleFruit.ApplyPalette += DangleFruit_ApplyPalette;
            On.TentaclePlantGraphics.ApplyPalette += TentaclePlantGraphics_ApplyPalette;
            On.PoleMimicGraphics.DrawSprites += PoleMimicGraphics_DrawSprites;
            On.CentipedeGraphics.ApplyPalette += CentipedeGraphics_ApplyPalette;
            On.CentipedeGraphics.ctor += CentipedeGraphics_ctor;
            On.BigSpiderGraphics.ApplyPalette += BigSpiderGraphics_ApplyPalette;
            // making dream colourful and real grayscale
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

            // nightmare-exclusive code below!

            On.DaddyGraphics.DrawSprites += DaddyGraphics_DrawSprites;
            On.DaddyGraphics.Eye.GetColor += Eye_GetColor;
            On.DaddyGraphics.DaddyTubeGraphic.ApplyPalette += DaddyTubeGraphic_ApplyPalette;
            On.DaddyGraphics.DaddyDeadLeg.ApplyPalette += DaddyDeadLeg_ApplyPalette;
            On.DaddyGraphics.DaddyDangleTube.ApplyPalette += DaddyDangleTube_ApplyPalette;
            On.DaddyBubble.DrawSprites += DaddyBubble_DrawSprites;
            On.DaddyGraphics.ApplyPalette += DaddyGraphics_ApplyPalette;
            On.DaddyGraphics.RenderSlits += DaddyGraphics_RenderSlits;
            On.DaddyRipple.DrawSprites += DaddyRipple_DrawSprites;
            // longlegs code

            On.DaddyGraphics.HunterDummy.ApplyPalette += HunterDummy_ApplyPalette;
            On.DaddyGraphics.HunterDummy.ctor += HunterDummy_ctor;
            On.DaddyGraphics.HunterDummy.DrawSprites += HunterDummy_DrawSprites;
            // insomniac long legs

            On.DaddyCorruption.Bulb.ApplyPalette += Bulb_ApplyPalette;
            On.DaddyCorruption.Update += DaddyCorruption_Update;
            // wall-corruption

            On.RoomCamera.MoveCamera_Room_int += RoomCamera_MoveCamera_Room_int;

            //------------------------------------ REAL THINGS ------------------------------------
            // omg so real

            On.GlobalRain.DeathRain.NextDeathRainMode += DeathRain_NextDeathRainMode;
            On.GlobalRain.DeathRain.DeathRainUpdate += DeathRain_DeathRainUpdate;
            // rain does not instakill and instead cycles back around to the precycle.

            On.RainCycle.ctor += RainCycle_ctor;
            // every cycle has a precycle

            On.RoomCamera.UpdateDayNightPalette += RoomCamera_UpdateDayNightPalette;
            On.RoomCamera.Update += RoomCamera_Update;
            // night cycles

            On.RoomCamera.ApplyPalette += RoomCamera_ApplyPalette;
        }

        private void RoomCamera_ApplyPalette(On.RoomCamera.orig_ApplyPalette orig, RoomCamera self)
        {
            if (self.room != null &&
                self.room.game.session.characterStats.name.value == "NCRAreal")
            {
                FSprite grayshader = new FSprite("Futile_White", true);

                grayshader.shader = self.game.rainWorld.Shaders["ncrgray"];
                grayshader.scaleX = self.game.rainWorld.options.ScreenSize.x + 5f;
                grayshader.scaleY = self.game.rainWorld.options.ScreenSize.y + 5f;
                grayshader.anchorX = 0f;
                grayshader.anchorY = 0f;
                grayshader.alpha = 1f;

                self.ReturnFContainer("Bloom").AddChild(grayshader);
            }
            orig(self);
        }

        private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            orig(self);
            LoadShaders(self);
        }

        private void Worm_ApplyPalette(On.WormGrass.Worm.orig_ApplyPalette orig, WormGrass.Worm self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            if (self.room != null && self != null &&
                self.room.game.session.characterStats.name.value == "NCRAdream")
            {
                UnityEngine.Random.State state = UnityEngine.Random.state;

                Color color = new Color(0f, 0f, 0f, 1f);
                Color color2 = new Color(0f, 0f, 0f, 1f);


                
                    color = rCam.PixelColorAtCoordinate(self.belowGroundPos) + new Color(UnityEngine.Random.value,
                        UnityEngine.Random.value, UnityEngine.Random.value);
                    color2 = Color.Lerp(palette.texture.GetPixel(self.color, 3), new Color(UnityEngine.Random.value, UnityEngine.Random.value,
                        UnityEngine.Random.value), self.iFac * 0.5f);

                    sLeaser.sprites[1].color = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
                

                Room room = self.room;
                if (((room != null) ? room.world.region : null) != null)
                {
                    Room room2 = self.room;
                    if (((room2 != null) ? room2.world.region.name : null) == "OE")
                    {
                        float num = 1000f;
                        float num2 = (float)self.room.world.rainCycle.dayNightCounter / num;

                            color = Color.Lerp(color, Color.Lerp(new Color(UnityEngine.Random.value, UnityEngine.Random.value,
                                UnityEngine.Random.value), color2, 0.5f), num2 * 0.04f);
                            color2 = Color.Lerp(color2, new Color(UnityEngine.Random.value, UnityEngine.Random.value,
                                UnityEngine.Random.value), num2 * 0.4f);
                        
                    }
                }
                for (int i = 0; i < self.segments.Length; i++)
                {
                    (sLeaser.sprites[0] as TriangleMesh).verticeColors[i * 4] = Color.Lerp(color2, color, (float)i /
                        (float)(self.segments.Length - 1));
                    (sLeaser.sprites[0] as TriangleMesh).verticeColors[i * 4 + 1] = Color.Lerp(color2, color, (float)i /
                        (float)(self.segments.Length - 1));
                    (sLeaser.sprites[0] as TriangleMesh).verticeColors[i * 4 + 2] = Color.Lerp(color2, color, ((float)i + 0.5f) /
                        (float)(self.segments.Length - 1));
                    if (i < self.segments.Length - 1)
                    {
                        (sLeaser.sprites[0] as TriangleMesh).verticeColors[i * 4 + 3] = Color.Lerp(color2, color, ((float)i + 0.5f) /
                            (float)(self.segments.Length - 1));
                    }
                }

                UnityEngine.Random.state = state;
            }
            else
            {
                orig(self, sLeaser, rCam, palette);
            }
        }

        private void RoomCamera_Update(On.RoomCamera.orig_Update orig, RoomCamera self)
        {
            orig(self);
            if (self.room != null && self != null &&
                self.room.game.session.characterStats.name.value == "NCRAreal")
            {
                if (self.currentPalette.darkness < 0.2f)
                {
                    self.currentPalette.darkness = 0.2f;
                }
                if (self.effect_dayNight !=  1f)
                {
                    self.effect_dayNight = 1f;
                    self.UpdateDayNightPalette();
                }
            }
        }

        private void DeathRain_DeathRainUpdate(On.GlobalRain.DeathRain.orig_DeathRainUpdate orig, GlobalRain.DeathRain self)
        {
            if (self.globalRain.game.session.characterStats.name.value == "NCRAreal" &&
                self.globalRain.game.FirstAlivePlayer.realizedCreature != null)
            {
                self.progression += 1f / self.timeInThisMode * (self.globalRain.game.IsArenaSession ? 3.2f : 1f);
                bool progressrain = false;
                if (self.progression > 1f)
                {
                    self.progression = 1f;
                    progressrain = true;
                }

                if (self.deathRainMode == GlobalRain.DeathRain.DeathRainMode.CalmBeforeStorm)
                {
                    self.globalRain.RumbleSound = Mathf.Max(self.globalRain.RumbleSound - 0.025f, 0f);
                }
                else
                {
                    self.globalRain.RumbleSound = Mathf.Lerp(self.globalRain.RumbleSound, 1f - Mathf.InverseLerp(0f, 0.6f,
                        self.globalRain.game.world.rainCycle.RainApproaching), 0.2f);
                }

                if (self.deathRainMode == GlobalRain.DeathRain.DeathRainMode.CalmBeforeStorm)
                {
                    self.globalRain.Intensity = Mathf.Pow(Mathf.InverseLerp(0.15f, 0f, self.progression), 1.5f) * 0.24f;
                    self.globalRain.ShaderLight = -1f + 0.3f * Mathf.Sin(Mathf.InverseLerp(0.03f, 0.8f, self.progression)
                        * 3.1415927f) * self.calmBeforeStormSunlight;
                    self.globalRain.bulletRainDensity = Mathf.Pow(Mathf.InverseLerp(0.3f, 1f, self.progression), 8f);
                }
                else if (self.deathRainMode == GlobalRain.DeathRain.DeathRainMode.GradeABuildUp)
                {
                    self.globalRain.Intensity = self.progression * 0.6f;
                    self.globalRain.MicroScreenShake = self.progression * 1.5f;
                    self.globalRain.bulletRainDensity = 1f - self.progression;
                }
                else if (!(self.deathRainMode == GlobalRain.DeathRain.DeathRainMode.GradeAPlateu))
                {
                    if (self.deathRainMode == GlobalRain.DeathRain.DeathRainMode.GradeBBuildUp)
                    {
                        self.globalRain.Intensity = Mathf.Lerp(0.6f, 0.71f, self.progression);
                        self.globalRain.MicroScreenShake = Mathf.Lerp(1.5f, 2.1f, self.progression);
                    }
                    else if (!(self.deathRainMode == GlobalRain.DeathRain.DeathRainMode.GradeBPlateu))
                    {
                        if (self.deathRainMode == GlobalRain.DeathRain.DeathRainMode.FinalBuildUp)
                        {
                            self.globalRain.Intensity = Mathf.Lerp(0.71f, 1f, self.progression);
                            self.globalRain.MicroScreenShake = Mathf.Lerp(2.1f, 4f, Mathf.Pow(self.progression, 1.2f));
                        }
                        else if (self.deathRainMode == GlobalRain.DeathRain.DeathRainMode.AlternateBuildUp)
                        {
                            self.globalRain.Intensity = Mathf.Lerp(0.24f, 0.6f, self.progression);
                            self.globalRain.MicroScreenShake = 1f + self.progression * 0.5f;
                        }
                        else if (self.deathRainMode == MoreSlugcatsEnums.DeathRainMode.Pulses)
                        {
                            float num3;
                            if (self.progression <= 0.9f)
                            {
                                float num = (1f - self.progression) * 50f;
                                float num2 = 0.4f + Mathf.Sin(self.progression / (num / 3f));
                                num3 = self.progression * self.timeInThisMode / self.timeInThisMode + Mathf.Sin(self.progression *
                                    self.timeInThisMode / num) / (self.timeInThisMode / (self.progression * self.timeInThisMode));
                                if (self.progression > 0.6f && Mathf.Abs(num3 - num2) < 0.1f)
                                {
                                    num3 *= num2;
                                }
                                self.globalRain.bulletRainDensity = Mathf.Lerp(0.1f, 1f, num2 - 0.4f);
                                num3 = Mathf.Clamp(num3, self.progression * 0.6f, 1f);
                            }
                            else
                            {
                                self.globalRain.bulletRainDensity = 0f;
                                num3 = 1f;
                            }
                            float t = 0.25f * Mathf.InverseLerp(0f, 0.1f, self.progression);
                            self.globalRain.Intensity = Mathf.Lerp(self.globalRain.Intensity, Mathf.Lerp(0f, 0.75f, num3), t);
                            self.globalRain.MicroScreenShake = (1f + self.progression * 0.65f) * (num3 + 0.25f);
                        }
                    }
                }


                if (progressrain)
                {
                    self.NextDeathRainMode();
                }
            }
            else
            {
                orig(self);
            }
        }

        private void RainCycle_ctor(On.RainCycle.orig_ctor orig, RainCycle self, World world, float minutes)
        {
            orig(self, world, minutes);
            if (world.game.session is StoryGameSession && world.game.session.characterStats.name.value == "NCRAreal" &&
                !self.challengeForcedPrecycle)
            {
                self.maxPreTimer = (int)UnityEngine.Random.Range(4800f, 12000f);
                self.preTimer = self.maxPreTimer;
                self.preCycleRainPulse_WaveA = 0f;
                self.preCycleRainPulse_WaveB = 0f;
                self.preCycleRainPulse_WaveC = 1.5707964f;
                world.game.globalRain.preCycleRainPulse_Scale = 1f;
                self.challengeForcedPrecycle = true;
            }
        }

        private void DeathRain_NextDeathRainMode(On.GlobalRain.DeathRain.orig_NextDeathRainMode orig, GlobalRain.DeathRain self)
        {
            if (self.globalRain.game.session.characterStats.name.value == "NCRAreal" && self.globalRain.game.FirstAlivePlayer.realizedCreature != null)
            {
                if (self.deathRainMode == GlobalRain.DeathRain.DeathRainMode.Mayhem)
                {
                    return;
                    // this is the auto-kill rain mode, if i am correct
                }


                if (self.deathRainMode == GlobalRain.DeathRain.DeathRainMode.None && UnityEngine.Random.value < 0.7f &&
                    !(self.globalRain.game.FirstAlivePlayer.realizedCreature as Player).GetRealCat().swapRainDir)
                {
                    (self.globalRain.game.FirstAlivePlayer.realizedCreature as Player).GetRealCat().InsomniaHalfCycles++;
                    // adds one to insomniacycles

                    if (UnityEngine.Random.value < 0.7f)
                    {
                        self.deathRainMode = GlobalRain.DeathRain.DeathRainMode.AlternateBuildUp;
                    }
                    else
                    {
                        self.deathRainMode = MoreSlugcatsEnums.DeathRainMode.Pulses;
                    }
                }
                else if (self.deathRainMode == GlobalRain.DeathRain.DeathRainMode.GradeBBuildUp &&
                    (self.globalRain.game.FirstAlivePlayer.realizedCreature as Player).GetRealCat().swapRainDir)
                {
                    self.deathRainMode = GlobalRain.DeathRain.DeathRainMode.CalmBeforeStorm;
                }
                else if (self.deathRainMode == GlobalRain.DeathRain.DeathRainMode.GradeABuildUp &&
                    (self.globalRain.game.FirstAlivePlayer.realizedCreature as Player).GetRealCat().swapRainDir)
                {
                    self.deathRainMode = GlobalRain.DeathRain.DeathRainMode.CalmBeforeStorm;
                }
                else if (self.deathRainMode == GlobalRain.DeathRain.DeathRainMode.AlternateBuildUp &&
                    !(self.globalRain.game.FirstAlivePlayer.realizedCreature as Player).GetRealCat().swapRainDir)
                {
                    self.deathRainMode = GlobalRain.DeathRain.DeathRainMode.GradeAPlateu;
                }

                else if (self.deathRainMode == GlobalRain.DeathRain.DeathRainMode.GradeAPlateu &&
                    !(self.globalRain.game.FirstAlivePlayer.realizedCreature as Player).GetRealCat().swapRainDir)
                {
                    self.deathRainMode = MoreSlugcatsEnums.DeathRainMode.Pulses;
                }
                else if (self.deathRainMode == GlobalRain.DeathRain.DeathRainMode.GradeBPlateu &&
                    !(self.globalRain.game.FirstAlivePlayer.realizedCreature as Player).GetRealCat().swapRainDir)
                {
                    self.deathRainMode = MoreSlugcatsEnums.DeathRainMode.Pulses;
                }


                else if (self.deathRainMode == MoreSlugcatsEnums.DeathRainMode.Pulses)
                {
                    if (UnityEngine.Random.value < 0.5f)
                    {
                        self.deathRainMode = GlobalRain.DeathRain.DeathRainMode.GradeBBuildUp;
                    }
                    else
                    {
                        self.deathRainMode = GlobalRain.DeathRain.DeathRainMode.GradeAPlateu;
                    }


                    UnityEngine.Debug.Log("Insomnia cycle triggered! Swapping rain buildup");
                    UnityEngine.Debug.Log("Insomnia cycle count:" + (self.globalRain.game.FirstAlivePlayer.realizedCreature as Player).GetRealCat().InsomniaHalfCycles);
                    UnityEngine.Debug.Log("Insomnia cycle math:" + ((self.globalRain.game.FirstRealizedPlayer.GetRealCat().InsomniaHalfCycles / 2) % 1));
                    (self.globalRain.game.FirstAlivePlayer.realizedCreature as Player).GetRealCat().swapRainDir = true;
                }


                else
                {
                    string entry = ExtEnum<GlobalRain.DeathRain.DeathRainMode>.values.GetEntry(self.deathRainMode.Index);
                    if ((self.globalRain.game.FirstAlivePlayer.realizedCreature as Player).GetRealCat().swapRainDir)
                    {
                        entry = ExtEnum<GlobalRain.DeathRain.DeathRainMode>.values.GetEntry(self.deathRainMode.Index - 1);
                        // if the direction of the rain is swapped, then go backwards.
                    }
                    else
                    {
                        entry = ExtEnum<GlobalRain.DeathRain.DeathRainMode>.values.GetEntry(self.deathRainMode.Index + 1);
                    }


                    if (entry == "None")
                    {
                        self.globalRain.ResetRain();
                    }
                    else if (entry == null)
                    {
                        self.globalRain.ResetRain();
                    }
                    else
                    {
                        self.deathRainMode = new GlobalRain.DeathRain.DeathRainMode(entry, false);
                    }
                }

                self.progression = 0f;




                if (self.deathRainMode == GlobalRain.DeathRain.DeathRainMode.CalmBeforeStorm)
                {
                    self.timeInThisMode = Mathf.Lerp(400f, 800f, UnityEngine.Random.value);
                    self.calmBeforeStormSunlight = ((UnityEngine.Random.value < 0.5f) ? 0f : UnityEngine.Random.value);
                    return;
                }
                if (self.deathRainMode == GlobalRain.DeathRain.DeathRainMode.GradeABuildUp)
                {
                    self.timeInThisMode = 6f;
                    self.globalRain.ShaderLight = -1f;
                    return;
                }
                if (self.deathRainMode == GlobalRain.DeathRain.DeathRainMode.GradeAPlateu)
                {
                    self.timeInThisMode = Mathf.Lerp(400f, 600f, UnityEngine.Random.value);
                    return;
                }
                if (self.deathRainMode == GlobalRain.DeathRain.DeathRainMode.GradeBBuildUp)
                {
                    self.timeInThisMode = ((UnityEngine.Random.value < 0.5f) ? 100f : Mathf.Lerp(50f, 300f, UnityEngine.Random.value));
                    return;
                }
                if (self.deathRainMode == GlobalRain.DeathRain.DeathRainMode.GradeBPlateu)
                {
                    self.timeInThisMode = ((UnityEngine.Random.value < 0.5f) ? 100f : Mathf.Lerp(50f, 300f, UnityEngine.Random.value));
                    return;
                }
                if (self.deathRainMode == GlobalRain.DeathRain.DeathRainMode.FinalBuildUp)
                {
                    self.timeInThisMode = ((UnityEngine.Random.value < 0.5f) ? Mathf.Lerp(300f, 500f, UnityEngine.Random.value) :
                        Mathf.Lerp(100f, 800f, UnityEngine.Random.value));
                    return;
                }
                if (self.deathRainMode == GlobalRain.DeathRain.DeathRainMode.AlternateBuildUp)
                {
                    self.timeInThisMode = Mathf.Lerp(400f, 1200f, UnityEngine.Random.value);
                    return;
                }
                if (self.deathRainMode == MoreSlugcatsEnums.DeathRainMode.Pulses)
                {
                    self.timeInThisMode = Mathf.Lerp(800f, 1200f, UnityEngine.Random.value);
                    return;
                }
            }
            else
            {
                orig(self);
            }
        }


        private void RoomCamera_UpdateDayNightPalette(On.RoomCamera.orig_UpdateDayNightPalette orig, RoomCamera self)
        {
            if (self.room != null && self.game.session.characterStats.name.value == "NCRAreal" && self.game.FirstRealizedPlayer != null &&
                self.game.FirstRealizedPlayer.GetRealCat().InsomniaHalfCycles != 0 &&
                (self.room.world.rainCycle.timer >= self.room.world.rainCycle.cycleLength ||
                (self.game.FirstRealizedPlayer.GetRealCat().InsomniaHalfCycles / 2) % 1 != 0))
            {
                // checks to make sure insomniahalfcycles divided by 2 is a whole number
                // aka: every even cycle is daytime, every uneven cycle is nighttime

                float num = 1320f;
                float num2 = 1.47f;
                float num3 = 1.92f;
                if ((float)self.room.world.rainCycle.dayNightCounter < num)
                {
                    if (self.room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.AboveCloudsView) > 0f &&
                        self.room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.SkyAndLightBloom) > 0f)
                    {
                        self.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.SkyAndLightBloom).amount = 0f;
                    }
                    float a = self.paletteBlend;
                    self.paletteBlend = Mathf.Lerp(a, 1f, (float)self.room.world.rainCycle.dayNightCounter / num);
                    self.ApplyFade();
                    self.paletteBlend = a;
                }
                else if ((float)self.room.world.rainCycle.dayNightCounter == num)
                {
                    self.ChangeBothPalettes(self.paletteB, self.room.world.rainCycle.duskPalette, 0f);
                }
                else if ((float)self.room.world.rainCycle.dayNightCounter < num * num2)
                {
                    if (self.paletteBlend == 1f || self.paletteB != self.room.world.rainCycle.duskPalette || self.dayNightNeedsRefresh)
                    {
                        self.ChangeBothPalettes(self.paletteB, self.room.world.rainCycle.duskPalette, 0f);
                    }
                    self.paletteBlend = Mathf.InverseLerp(num, num * num2, (float)self.room.world.rainCycle.dayNightCounter);
                    self.ApplyFade();
                }
                else if ((float)self.room.world.rainCycle.dayNightCounter == num * num2)
                {
                    self.ChangeBothPalettes(self.room.world.rainCycle.duskPalette, self.room.world.rainCycle.nightPalette, 0f);
                }
                else if ((float)self.room.world.rainCycle.dayNightCounter < num * num3)
                {
                    if (self.paletteBlend == 1f || self.paletteB != self.room.world.rainCycle.nightPalette ||
                        self.paletteA != self.room.world.rainCycle.duskPalette || self.dayNightNeedsRefresh)
                    {
                        self.ChangeBothPalettes(self.room.world.rainCycle.duskPalette, self.room.world.rainCycle.nightPalette, 0f);
                    }
                    self.paletteBlend = Mathf.InverseLerp(num * num2, num * num3, (float)self.room.world.rainCycle.dayNightCounter) * 0.99f;
                    self.ApplyFade();
                }
                else if ((float)self.room.world.rainCycle.dayNightCounter == num * num3)
                {
                    self.ChangeBothPalettes(self.room.world.rainCycle.duskPalette, self.room.world.rainCycle.nightPalette,
                        self.effect_dayNight * 0.99f);
                }
                else if ((float)self.room.world.rainCycle.dayNightCounter > num * num3)
                {
                    if (self.paletteBlend == 1f || self.paletteB != self.room.world.rainCycle.nightPalette ||
                        self.paletteA != self.room.world.rainCycle.duskPalette || self.dayNightNeedsRefresh)
                    {
                        self.ChangeBothPalettes(self.room.world.rainCycle.duskPalette, self.room.world.rainCycle.nightPalette,
                            self.effect_dayNight);
                    }
                    self.paletteBlend = self.effect_dayNight * 0.99f;
                    self.ApplyFade();
                }
                self.dayNightNeedsRefresh = false;
            }
            else
            {
                orig(self);
            }
        }

        private void RoomCamera_MoveCamera_Room_int(On.RoomCamera.orig_MoveCamera_Room_int orig, RoomCamera self, Room newRoom, int camPos)
        {
            orig(self, newRoom, camPos);
            if (newRoom.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.HeatWave) == 0f && newRoom != null &&
                self.game.session.characterStats.name.value == "NCRAdream")
            {
                self.levelGraphic.shader = self.game.rainWorld.Shaders["LevelHeat"];
                self.levelGraphic.alpha = 0.5f;
                // adds heatwave effect to every room that does not naturally have a heat wave
            }
        }

        private void SSOracleMeetWhite_Update(On.SSOracleBehavior.SSOracleMeetWhite.orig_Update orig, SSOracleBehavior.SSOracleMeetWhite self)
        {
            if ((self.player.GetDreamCat().DreamActive || self.player.GetRealCat().RealActive) && self.player != null &&
                self != null)
            {
                self.owner.LockShortcuts();
                if (ModManager.MSC && (self.action == MoreSlugcatsEnums.SSOracleBehaviorAction.MeetWhite_ThirdCurious ||
                    self.action == MoreSlugcatsEnums.SSOracleBehaviorAction.MeetWhite_SecondImages))
                {
                    Vector2 vector = self.oracle.room.MiddleOfTile(24, 14) - self.player.mainBodyChunk.pos;
                    float num = Custom.Dist(self.oracle.room.MiddleOfTile(24, 14), self.player.mainBodyChunk.pos);
                    self.player.mainBodyChunk.vel += Vector2.ClampMagnitude(vector, 40f) / 40f * Mathf.Clamp(16f - num / 100f * 16f, 4f, 16f);
                    if (self.player.mainBodyChunk.vel.magnitude < 1f || num < 8f)
                    {
                        self.player.mainBodyChunk.vel = Vector2.zero;
                        self.player.mainBodyChunk.HardSetPosition(self.oracle.room.MiddleOfTile(24, 14));
                    }
                }
                if (self.action == SSOracleBehavior.Action.MeetWhite_Shocked)
                {
                    self.owner.movementBehavior = SSOracleBehavior.MovementBehavior.KeepDistance;
                    if (self.owner.oracle.room.game.manager.rainWorld.progression.miscProgressionData.redHasVisitedPebbles ||
                        self.owner.oracle.room.game.manager.rainWorld.options.validation)
                    {
                        if (self.inActionCounter > 40)
                        {
                            self.owner.NewAction(SSOracleBehavior.Action.General_GiveMark);
                            self.owner.afterGiveMarkAction = SSOracleBehavior.Action.General_MarkTalk;
                            return;
                        }
                    }
                    else if (self.owner.oracle.room.game.IsStorySession &&
                        self.owner.oracle.room.game.GetStorySession.saveState.deathPersistentSaveData.theMark)
                    {
                        if (self.inActionCounter > 40)
                        {
                            self.owner.NewAction(SSOracleBehavior.Action.General_MarkTalk);
                            return;
                        }
                    }
                    else if (self.inActionCounter > 120)
                    {
                        self.owner.NewAction(SSOracleBehavior.Action.MeetWhite_Curious);
                        return;
                    }
                }
                else if (self.action == SSOracleBehavior.Action.MeetWhite_Curious)
                {
                    self.owner.movementBehavior = SSOracleBehavior.MovementBehavior.Investigate;
                    if (self.inActionCounter > 360)
                    {
                        self.owner.NewAction(SSOracleBehavior.Action.MeetWhite_Talking);
                        return;
                    }
                }
                else if (self.action == SSOracleBehavior.Action.MeetWhite_Talking)
                {
                    self.owner.movementBehavior = SSOracleBehavior.MovementBehavior.Talk;
                    if (!self.CurrentlyCommunicating && self.communicationPause > 0)
                    {
                        self.communicationPause--;
                    }
                    if (!self.CurrentlyCommunicating && self.communicationPause < 1)
                    {
                        if (self.communicationIndex >= 4)
                        {
                            self.owner.NewAction(SSOracleBehavior.Action.MeetWhite_Texting);
                        }
                        else if (self.owner.allStillCounter > 20)
                        {
                            self.NextCommunication();
                        }
                    }
                    if (!self.CurrentlyCommunicating)
                    {
                        self.owner.nextPos += Custom.RNV();
                        return;
                    }
                }
                else
                {
                    if (self.action == SSOracleBehavior.Action.MeetWhite_Texting)
                    {
                        self.movementBehavior = SSOracleBehavior.MovementBehavior.ShowMedia;
                        if (self.oracle.graphicsModule != null)
                        {
                            (self.oracle.graphicsModule as OracleGraphics).halo.connectionsFireChance = 0f;
                        }
                        if (!self.CurrentlyCommunicating && self.communicationPause > 0)
                        {
                            self.communicationPause--;
                        }
                        if (!self.CurrentlyCommunicating && self.communicationPause < 1)
                        {
                            if (self.communicationIndex >= 6 || (ModManager.MSC && self.owner.oracle.ID == MoreSlugcatsEnums.OracleID.DM &&
                                self.communicationIndex >= 4))
                            {
                                self.owner.NewAction(SSOracleBehavior.Action.MeetWhite_Images);
                            }
                            else if (self.owner.allStillCounter > 20)
                            {
                                self.NextCommunication();
                            }
                        }
                        self.chatLabel.setPos = new Vector2?(self.showMediaPos);
                        return;
                    }
                    if (self.action == SSOracleBehavior.Action.MeetWhite_Images ||
                        (ModManager.MSC && self.action == MoreSlugcatsEnums.SSOracleBehaviorAction.MeetWhite_SecondImages))
                    {
                        self.movementBehavior = SSOracleBehavior.MovementBehavior.ShowMedia;
                        if (self.communicationPause > 0)
                        {
                            self.communicationPause--;
                        }
                        if (ModManager.MSC && self.action == MoreSlugcatsEnums.SSOracleBehaviorAction.MeetWhite_SecondImages)
                        {
                            self.myProjectionCircle.pos = new Vector2(self.player.mainBodyChunk.pos.x - 10f,
                                self.player.mainBodyChunk.pos.y);
                        }
                        if (self.inActionCounter > 150 && self.communicationPause < 1)
                        {
                            if (self.action == SSOracleBehavior.Action.MeetWhite_Images && (self.communicationIndex >= 3 ||
                                (ModManager.MSC && self.owner.oracle.ID == MoreSlugcatsEnums.OracleID.DM && self.communicationIndex >= 1)))
                            {
                                self.owner.NewAction(SSOracleBehavior.Action.MeetWhite_SecondCurious);
                            }
                            else if (ModManager.MSC && self.action == MoreSlugcatsEnums.SSOracleBehaviorAction.MeetWhite_SecondImages &&
                                self.communicationIndex >= 2)
                            {
                                self.owner.NewAction(MoreSlugcatsEnums.SSOracleBehaviorAction.MeetWhite_StartDialog);
                            }
                            else
                            {
                                self.NextCommunication();
                            }
                        }
                        if (self.showImage != null)
                        {
                            self.showImage.setPos = new Vector2?(self.showMediaPos);
                        }
                        if (UnityEngine.Random.value < 0.033333335f)
                        {
                            self.idealShowMediaPos += Custom.RNV() * UnityEngine.Random.value * 30f;
                            self.showMediaPos += Custom.RNV() * UnityEngine.Random.value * 30f;
                            return;
                        }
                    }
                    else if (self.action == SSOracleBehavior.Action.MeetWhite_SecondCurious)
                    {
                        self.movementBehavior = SSOracleBehavior.MovementBehavior.Investigate;
                        if (self.inActionCounter == 80)
                        {
                            if (ModManager.MSC && self.owner.oracle.ID == MoreSlugcatsEnums.OracleID.DM)
                            {
                                self.voice = self.oracle.room.PlaySound(SoundID.SL_AI_Talk_5, self.oracle.firstChunk);
                            }
                            else
                            {
                                self.voice = self.oracle.room.PlaySound(SoundID.SS_AI_Talk_5, self.oracle.firstChunk);
                            }
                            self.voice.requireActiveUpkeep = true;
                        }
                        if (self.inActionCounter > 240)
                        {
                            self.owner.NewAction(SSOracleBehavior.Action.General_GiveMark);
                            self.owner.afterGiveMarkAction = SSOracleBehavior.Action.General_MarkTalk;
                            return;
                        }
                    }
                    else if (self.action == SSOracleBehavior.Action.General_MarkTalk)
                    {
                        self.movementBehavior = SSOracleBehavior.MovementBehavior.Talk;
                        if (self.owner.conversation != null && self.owner.conversation.id == self.convoID &&
                            self.owner.conversation.slatedForDeletion)
                        {
                            self.owner.conversation = null;
                            self.owner.NewAction(SSOracleBehavior.Action.ThrowOut_ThrowOut);
                            return;
                        }
                    }
                    else if (ModManager.MSC && self.action == MoreSlugcatsEnums.SSOracleBehaviorAction.MeetWhite_ThirdCurious)
                    {
                        self.owner.movementBehavior = SSOracleBehavior.MovementBehavior.Investigate;
                        if (self.inActionCounter % 180 == 1)
                        {
                            self.owner.investigateAngle = UnityEngine.Random.value * 360f;
                        }
                        if (self.inActionCounter == 180)
                        {
                            self.dialogBox.NewMessage(self.Translate("Hello there."), 0);
                            self.dialogBox.NewMessage(self.Translate("Are my words reaching you?"), 0);
                        }
                        if (self.inActionCounter == 460)
                        {
                            self.myProjectionCircle = new ProjectionCircle(self.player.mainBodyChunk.pos, 0f, 3f);
                            self.oracle.room.AddObject(self.myProjectionCircle);
                        }
                        if (self.inActionCounter > 460)
                        {
                            float num2 = Mathf.Lerp(0f, 1f, ((float)self.inActionCounter - 460f) / 150f);
                            self.myProjectionCircle.radius = 18f * Mathf.Clamp(num2 * 2f, 0f, 1f);
                            self.myProjectionCircle.pos = new Vector2(self.player.mainBodyChunk.pos.x - 10f,
                                self.player.mainBodyChunk.pos.y);
                        }
                        if (self.inActionCounter > 770)
                        {
                            self.owner.NewAction(MoreSlugcatsEnums.SSOracleBehaviorAction.MeetWhite_SecondImages);
                            return;
                        }
                    }
                    else if (ModManager.MSC && self.action == MoreSlugcatsEnums.SSOracleBehaviorAction.MeetWhite_StartDialog)
                    {
                        if (self.inActionCounter < 48)
                        {
                            self.player.mainBodyChunk.vel += Vector2.ClampMagnitude(self.oracle.room.MiddleOfTile(24, 14) -
                                self.player.mainBodyChunk.pos, 40f) / 40f * (6f - (float)self.inActionCounter / 8f);
                            float num3 = 1f - (float)self.inActionCounter / 48f;
                            self.myProjectionCircle.radius = 18f * Mathf.Clamp(num3 * 2f, 0f, 3f);
                            self.myProjectionCircle.pos = new Vector2(self.player.mainBodyChunk.pos.x - 10f,
                                self.player.mainBodyChunk.pos.y);
                        }
                        if (self.inActionCounter == 48)
                        {
                            self.myProjectionCircle.Destroy();
                        }
                        if (self.inActionCounter == 180)
                        {
                            SLOrcacleState sloracleState = self.oracle.room.game.GetStorySession.saveState.miscWorldSaveData.SLOracleState;
                            int playerEncounters = sloracleState.playerEncounters;
                            sloracleState.playerEncounters = playerEncounters + 1;
                            self.owner.NewAction(MoreSlugcatsEnums.SSOracleBehaviorAction.Moon_AfterGiveMark);
                        }
                    }
                }
            }
            else
            {
                orig(self);
            }
        }

        private void DaddyRipple_DrawSprites(On.DaddyRipple.orig_DrawSprites orig, DaddyRipple self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);
            if (self.room != null && self != null && self.owner != null &&
                self.room.game.session.characterStats.name.value == "NCRAdream")
            {
                float num = Mathf.Lerp(self.lastLife, self.life, timeStacker);
                float num2 = Mathf.InverseLerp(0f, 0.75f, num);
                sLeaser.sprites[0].color = Color.Lerp((num2 > 0.5f) ? new Color(1f, 0f, 0f) : new Color(0f, 0f, 0f),
                    Color.Lerp(new Color(0f, 0f, 0f), new Color(1f, 0f, 0f), 0.5f + 0.5f * self.intensity), Mathf.Sin(num2 * 3.1415927f));
            }
        }

        private void DaddyGraphics_RenderSlits(On.DaddyGraphics.orig_RenderSlits orig, DaddyGraphics self, int chunk, Vector2 pos, Vector2 middleOfBody, float rotation, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            if (self.daddy.room != null && self != null && self.owner != null && self.daddy != null &&
                self.daddy.room.game.session.characterStats.name.value == "NCRAdream")
            {
                if (self.daddy.HDmode && chunk < 2)
                {
                    if (self.daddy.room.game.FirstAlivePlayer.realizedCreature != null && 
                        (self.daddy.room.game.FirstAlivePlayer.realizedCreature as Player).GetDreamCat().Sanity < 0.5f)
                    {
                        // if sanity is too low, it DOES have eye sprites.
                        sLeaser.sprites[self.EyeSprite(chunk, 0)].isVisible = true;
                        sLeaser.sprites[self.EyeSprite(chunk, 1)].isVisible = true;
                        sLeaser.sprites[self.EyeSprite(chunk, 2)].isVisible = true;
                    }
                    else
                    {
                        sLeaser.sprites[self.EyeSprite(chunk, 0)].isVisible = false;
                        sLeaser.sprites[self.EyeSprite(chunk, 1)].isVisible = false;
                        sLeaser.sprites[self.EyeSprite(chunk, 2)].isVisible = false;
                        return;
                    }
                }
                float rad = self.daddy.bodyChunks[chunk].rad;
                float num = Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(self.eyes[chunk].lastClosed, self.eyes[chunk].closed,
                    timeStacker)), 0.6f);
                float num2 = (self.SizeClass ? 1f : 0.8f) * (1f - num);
                Vector2 b = Vector2.Lerp(self.eyes[chunk].lastDir, self.eyes[chunk].dir, timeStacker);
                float num3 = Mathf.Lerp(self.eyes[chunk].lastFocus, self.eyes[chunk].focus, timeStacker) *
                    Mathf.Pow(Mathf.InverseLerp(-1f, 1f, Vector2.Dot(Custom.DirVec(middleOfBody, pos), b.normalized)), 0.7f);
                num3 = Mathf.Max(num3, num);
                float num4 = Mathf.InverseLerp(0f, Mathf.Lerp(30f, 50f, self.chunksRotats[chunk, 1]),
                    Vector2.Distance(middleOfBody, pos + Custom.DirVec(middleOfBody, pos) * rad)) * 0.9f;
                num4 = Mathf.Lerp(num4, 1f, 0.5f * num3);
                Vector2 vector = Vector2.Lerp(Custom.DirVec(middleOfBody, pos) * num4, b, b.magnitude * 0.5f);
                self.eyes[chunk].centerRenderPos = pos + vector * rad;
                self.eyes[chunk].renderColor = Color.Lerp(new Color(1f, 0f, 0f), new Color(0f, 0f, 0f),
                    Mathf.Lerp(UnityEngine.Random.value * self.eyes[chunk].light, 1f, num));
                if (num > 0f)
                {
                    self.eyes[chunk].renderColor = Color.Lerp(self.eyes[chunk].renderColor, new Color(0f, 0f, 0f), num);
                }
                self.eyes[chunk].renderColor = Color.Lerp(self.eyes[chunk].renderColor, new Color(0f, 0f, 0f), self.eyes[chunk].flash);
                sLeaser.sprites[self.EyeSprite(chunk, 0)].color = self.eyes[chunk].renderColor;
                sLeaser.sprites[self.EyeSprite(chunk, 1)].color = self.eyes[chunk].renderColor;
                for (int i = 0; i < 2; i++)
                {
                    Vector2 vector2 = Custom.DegToVec(rotation + 90f * (float)i);
                    Vector2 a = Custom.PerpendicularVector(vector2);
                    (sLeaser.sprites[self.EyeSprite(chunk, i)] as TriangleMesh).MoveVertice(0, pos + self.BulgeVertex(vector2 *
                        rad * 0.9f * Mathf.Lerp(1f, 0.6f, num3), vector, rad) - camPos);
                    (sLeaser.sprites[self.EyeSprite(chunk, i)] as TriangleMesh).MoveVertice(9, pos + self.BulgeVertex(vector2 *
                        -rad * 0.9f * Mathf.Lerp(1f, 0.6f, num3), vector, rad) - camPos);
                    for (int j = 1; j < 5; j++)
                    {
                        for (int k = 0; k < 2; k++)
                        {
                            float d = rad * ((j < 3) ? 0.7f : 0.25f) * ((k == 0) ? 1f : -1f) * Mathf.Lerp(1f, 0.6f, num3);
                            int num5 = (k == 0) ? j : (9 - j);
                            float d2 = num2 * ((j < 3) ? 0.5f : 1f) * ((num5 % 2 == 0) ? 1f : -1f) * Mathf.Lerp(1f, 2.5f, num3);
                            (sLeaser.sprites[self.EyeSprite(chunk, i)] as TriangleMesh).MoveVertice(num5, pos +
                                self.BulgeVertex(vector2 * d + a * d2, vector, rad) - camPos);
                        }
                    }
                }
            }
            else
            {
                orig(self, chunk, pos, middleOfBody, rotation, sLeaser, rCam, timeStacker, camPos);
            }
        }

        private void HunterDummy_DrawSprites(On.DaddyGraphics.HunterDummy.orig_DrawSprites orig, DaddyGraphics.HunterDummy self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            if (self.owner.daddy.room != null && self != null && self.owner != null && self.owner.daddy != null &&
                self.owner.daddy.room.game.session.characterStats.name.value == "NCRAdream")
            {
                float num = 0.5f + 0.5f * Mathf.Sin(Mathf.Lerp(self.lastBreath, self.breath, timeStacker) * 3.1415927f * 2f);
                Vector2 vector = Vector2.Lerp(self.drawPositions[0, 1], self.drawPositions[0, 0], timeStacker);
                Vector2 vector2 = Vector2.Lerp(self.drawPositions[1, 1], self.drawPositions[1, 0], timeStacker);
                Vector2 vector3 = Vector2.Lerp(self.head.lastPos, self.head.pos, timeStacker);
                float num2 = Mathf.InverseLerp(0.3f, 0.5f, Mathf.Abs(Custom.DirVec(vector2, vector).y));
                sLeaser.sprites[self.startSprite].x = vector.x - camPos.x;
                sLeaser.sprites[self.startSprite].y = vector.y - camPos.y + 0.5f * num * (1f - num2);
                sLeaser.sprites[self.startSprite].rotation = Custom.AimFromOneVectorToAnother(vector2, vector);
                sLeaser.sprites[self.startSprite].scaleX = 1f + Mathf.Lerp(-0.15f, 0.05f, num) * num2;
                sLeaser.sprites[self.startSprite + 1].x = (vector2.x * 2f + vector.x) / 3f - camPos.x;
                sLeaser.sprites[self.startSprite + 1].y = (vector2.y * 2f + vector.y) / 3f - camPos.y;
                sLeaser.sprites[self.startSprite + 1].rotation = Custom.AimFromOneVectorToAnother(vector, Vector2.Lerp(self.tail[0].lastPos,
                    self.tail[0].pos, timeStacker));
                sLeaser.sprites[self.startSprite + 1].scaleY = 1f;
                sLeaser.sprites[self.startSprite + 1].scaleX = 1f + 0.05f * num - 0.05f;
                Vector2 vector4 = (vector2 * 3f + vector) / 4f;
                float d = 0.8f;
                float d2 = 6f;
                for (int i = 0; i < 4; i++)
                {
                    Vector2 vector5 = Vector2.Lerp(self.tail[i].lastPos, self.tail[i].pos, timeStacker);
                    Vector2 normalized = (vector5 - vector4).normalized;
                    Vector2 a = Custom.PerpendicularVector(normalized);
                    float d3 = Vector2.Distance(vector5, vector4) / 5f;
                    if (i == 0)
                    {
                        d3 = 0f;
                    }
                    (sLeaser.sprites[self.startSprite + 2] as TriangleMesh).MoveVertice(i * 4, vector4 - a * d2 * d + normalized * d3 - camPos);
                    (sLeaser.sprites[self.startSprite + 2] as TriangleMesh).MoveVertice(i * 4 + 1, vector4 + a * d2 * d + normalized * d3 - camPos);
                    if (i < 3)
                    {
                        (sLeaser.sprites[self.startSprite + 2] as TriangleMesh).MoveVertice(i * 4 + 2, vector5 - a *
                            self.tail[i].StretchedRad * d - normalized * d3 - camPos);
                        (sLeaser.sprites[self.startSprite + 2] as TriangleMesh).MoveVertice(i * 4 + 3, vector5 + a *
                            self.tail[i].StretchedRad * d - normalized * d3 - camPos);
                    }
                    else
                    {
                        (sLeaser.sprites[self.startSprite + 2] as TriangleMesh).MoveVertice(i * 4 + 2, vector5 - camPos);
                    }
                    d2 = self.tail[i].StretchedRad;
                    vector4 = vector5;
                }
                float num3 = Custom.AimFromOneVectorToAnother(Vector2.Lerp(vector2, vector, 0.5f), vector3);
                int num4 = Mathf.RoundToInt(Mathf.Abs(num3 / 360f * 34f));
                Vector2 vector6 = Vector2.zero;
                vector6 *= 0f;
                num4 = 0;
                sLeaser.sprites[self.startSprite + 5].rotation = num3;
                sLeaser.sprites[self.startSprite + 3].x = vector3.x - camPos.x;
                sLeaser.sprites[self.startSprite + 3].y = vector3.y - camPos.y;
                sLeaser.sprites[self.startSprite + 3].rotation = num3;
                sLeaser.sprites[self.startSprite + 3].scaleX = ((num3 >= 0f) ? 1f : -1f);
                sLeaser.sprites[self.startSprite + 3].element = Futile.atlasManager.GetElementWithName("HeadC" + num4.ToString());
                // the head sprite uses the slugpup sprite rather than the slugCAT sprite.
                sLeaser.sprites[self.startSprite + 5].x = vector3.x + vector6.x - camPos.x;
                sLeaser.sprites[self.startSprite + 5].y = vector3.y + vector6.y - 2f - camPos.y;
                Vector2 vector7 = Vector2.Lerp(self.legs.lastPos, self.legs.pos, timeStacker);
                sLeaser.sprites[self.startSprite + 4].x = vector7.x - camPos.x;
                sLeaser.sprites[self.startSprite + 4].y = vector7.y - camPos.y;
                sLeaser.sprites[self.startSprite + 4].rotation = Custom.AimFromOneVectorToAnother(self.legsDirection, new Vector2(0f, 0f));
                sLeaser.sprites[self.startSprite + 4].isVisible = true;
                string elementName = "LegsAAir0";
                sLeaser.sprites[self.startSprite + 4].element = Futile.atlasManager.GetElementWithName(elementName);
                if (self.darkenFactor > 0f)
                {
                    for (int j = 0; j < self.numberOfSprites; j++)
                    {
                        Color color = Color.Lerp(new Color(0.39f, 0.25f, 0.25f), Color.gray, 0.4f);
                        sLeaser.sprites[self.startSprite + j].color = new Color(Mathf.Min(1f, color.r *
                            (1f - self.darkenFactor) + 0.01f), Mathf.Min(1f, color.g * (1f - self.darkenFactor) + 0.01f),
                            Mathf.Min(1f, color.b * (1f - self.darkenFactor) + 0.01f));
                    }
                }
            }
            else
            {
                orig(self, sLeaser, rCam, timeStacker, camPos);
            }
        }

        private void DaddyGraphics_ApplyPalette(On.DaddyGraphics.orig_ApplyPalette orig, DaddyGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            orig(self, sLeaser, rCam, palette);
            if (self.daddy.room != null && self != null && self.daddy != null &&
                self.daddy.room.game.session.characterStats.name.value == "NCRAdream")
            {
                for (int i = 0; i < self.daddy.bodyChunks.Length; i++)
                {
                    if (self.daddy.HDmode)
                    {
                        sLeaser.sprites[self.BodySprite(i)].color = Color.Lerp(new Color(0.39f, 0.25f, 0.25f),
                            Color.gray, 0.4f);
                    }
                    else
                    {
                        sLeaser.sprites[self.BodySprite(i)].color = new Color(0f, 0f, 0f);
                    }
                }
                // this does not actually work, as it is redundant- instead, the sprites become invisible. fucked up
            }
        }

        private void DaddyBubble_DrawSprites(On.DaddyBubble.orig_DrawSprites orig, DaddyBubble self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);
            if (self.room != null && self != null && !self.slatedForDeletetion &&
                self.room.game.session.characterStats.name.value == "NCRAdream")
            {
                sLeaser.sprites[0].color = Color.Lerp(new Color(0f, 0f, 0f), new Color(1f, 0f, 0f), Mathf.InverseLerp(2f, 7f,
                    (float)self.freeCounter + timeStacker));
            }
        }

        private void DaddyCorruption_Update(On.DaddyCorruption.orig_Update orig, DaddyCorruption self, bool eu)
        {
            orig(self, eu);
            if (self.room != null && self != null &&
                self.room.game.session.characterStats.name.value == "NCRAdream")
            {
                self.effectColor = new Color(1f, 0f, 0f);
                self.eyeColor = new Color(1f, 0f, 0f);
                self.GWmode = false;
            }
        }

        private void Bulb_ApplyPalette(On.DaddyCorruption.Bulb.orig_ApplyPalette orig, DaddyCorruption.Bulb self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            if (self.owner.room != null && self.owner != null && self != null &&
                self.owner.room.game.session.characterStats.name.value == "NCRAdream")
            {
                sLeaser.sprites[self.firstSprite].color = new Color(0f, 0f, 0f);
                if (self.leg != null)
                {
                    self.leg.graphic.ApplyPalette(sLeaser, rCam, palette);
                }
            }
            else
            {
                orig(self, sLeaser, rCam, palette);
            }
        }

        private void DaddyDangleTube_ApplyPalette(On.DaddyGraphics.DaddyDangleTube.orig_ApplyPalette orig, DaddyGraphics.DaddyDangleTube self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            if (self.owner.daddy.room != null && self.owner != null && self != null && self.owner.daddy != null &&
                self.owner.daddy.room.game.session.characterStats.name.value == "NCRAdream")
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
                    if (self.owner.daddy.HDmode)
                    {
                        (sLeaser.sprites[self.firstSprite] as TriangleMesh).verticeColors[i] = Color.Lerp(color,
                        new Color(0.39f, 0.25f, 0.25f), self.OnTubeEffectColorFac(floatPos));
                    }
                    else
                    {
                        (sLeaser.sprites[self.firstSprite] as TriangleMesh).verticeColors[i] = Color.Lerp(color,
                        effect, self.OnTubeEffectColorFac(floatPos));
                    }
                }
                sLeaser.sprites[self.firstSprite].color = color;
                for (int j = 0; j < self.bumps.Length; j++)
                {
                    sLeaser.sprites[self.firstSprite + 1 + j].color = Color.Lerp(color, effect,
                        self.OnTubeEffectColorFac(self.bumps[j].pos.y));
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

                    if (self.owner.daddy.HDmode)
                    {
                        (sLeaser.sprites[self.firstSprite] as TriangleMesh).verticeColors[i] = Color.Lerp(color,
                        new Color(0.39f, 0.25f, 0.25f), self.OnTubeEffectColorFac(floatPos));
                    }
                    else
                    {
                        (sLeaser.sprites[self.firstSprite] as TriangleMesh).verticeColors[i] = Color.Lerp(color,
                        effect, self.OnTubeEffectColorFac(floatPos));
                    }
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

                    if (self.owner.daddy.HDmode)
                    {
                        (sLeaser.sprites[self.firstSprite] as TriangleMesh).verticeColors[i] = Color.Lerp(color,
                        new Color(0.39f, 0.25f, 0.25f), self.OnTubeEffectColorFac(floatPos));
                    }
                    else
                    {
                        (sLeaser.sprites[self.firstSprite] as TriangleMesh).verticeColors[i] = Color.Lerp(color,
                        effect, self.OnTubeEffectColorFac(floatPos));
                    }
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
                // gives insomniac hunter its signature tail
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
                        1f), new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value, 1f), 0.75f);
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
                self.mimicColor = new Color(0f, 0f, 0f);
                UnityEngine.Random.state = state;
            }
            else if (self != null && self.owner != null && !self.owner.slatedForDeletetion &&
                self.owner.room.game.session.characterStats.name.value == "NCRAreal")
            {
                // this should be kept, as it turns mimics to pure black.
                self.blackColor = new Color(0.01f, 0.01f, 0.01f);
                self.mimicColor = new Color(0f, 0f, 0f);
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
                    self.StoredPlantColor = palette.texture.GetPixel(0, 5);

                    for (int i = 0; i < (sLeaser.sprites[self.StalkSprite(1)] as TriangleMesh).verticeColors.Length; i++)
                    {
                        float num = (float)i / (float)((sLeaser.sprites[self.StalkSprite(1)] as TriangleMesh).verticeColors.Length - 1);
                        (sLeaser.sprites[self.StalkSprite(1)] as TriangleMesh).verticeColors[i] = Color.Lerp(palette.blackColor,
                            new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value), 0.4f +
                            Mathf.Pow(1f - num, 0.5f) * 0.4f);
                    }

                    self.yellowColor = Color.Lerp(new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value),
                        palette.blackColor, self.AbstractCob.dead ? (0.95f + 0.5f * rCam.PaletteDarkness()) :
                        (0.18f + 0.7f * rCam.PaletteDarkness()));

                    for (int j = 0; j < 2; j++)
                    {
                        for (int k = 0; k < (sLeaser.sprites[self.ShellSprite(j)] as TriangleMesh).verticeColors.Length; k++)
                        {
                            float num2 = 1f - (float)k / (float)((sLeaser.sprites[self.ShellSprite(j)] as TriangleMesh).verticeColors.Length - 1);
                            (sLeaser.sprites[self.ShellSprite(j)] as TriangleMesh).verticeColors[k] = Color.Lerp(palette.blackColor,
                                new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value),
                                Mathf.Pow(num2, 2.5f) * 0.4f);
                        }
                    }

                    sLeaser.sprites[self.CobSprite].color = self.yellowColor;

                    for (int l = 0; l < self.seedPositions.Length; l++)
                    {
                        sLeaser.sprites[self.SeedSprite(l, 0)].color = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
                        sLeaser.sprites[self.SeedSprite(l, 1)].color = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
                        sLeaser.sprites[self.SeedSprite(l, 2)].color = Color.Lerp(new Color(UnityEngine.Random.value, UnityEngine.Random.value,
                            UnityEngine.Random.value), palette.blackColor, self.AbstractCob.dead ? 0.3f : 0.2f);
                    }
                    for (int m = 0; m < self.leaves.GetLength(0); m++)
                    {
                        sLeaser.sprites[self.LeafSprite(m)].color = palette.blackColor;
                    }

                    UnityEngine.Random.state = state;
                
            }
            else
            {
                orig(self, sLeaser, rCam, palette);
            }
        }

        private void Player_UpdateMSC(On.Player.orig_UpdateMSC orig, Player self)
        {
            orig(self);
            if ((self.GetDreamCat().IsDream || self.GetDreamCat().DreamActive) && self.room.gravity >= 0.55f && !self.submerged)
            {
                // !submerged is necessary, as without that, dream is completely incapable of swimming.
                // this as a whole enables dream to have the dream-like bouncing, fall slowly, jump higher, ect.
                self.buoyancy = 0.96f;
                self.customPlayerGravity = 0.35f;
            }
            else if ((self.GetDreamCat().IsDream || self.GetDreamCat().DreamActive) &&
                self.room.gravity == 0f && !self.submerged)
            {
                self.customPlayerGravity = 1f;
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
            if (self.room.game.session.characterStats.name.value == "NCRAreal")
            {
                self.GetRealCat().RealActive = true;
            }
        }

        private void GreenSpark_ApplyPalette(On.GreenSparks.GreenSpark.orig_ApplyPalette orig, GreenSparks.GreenSpark self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            if (self != null && self.room != null && !self.slatedForDeletetion &&
                self.room.game.session.characterStats.name.value == "NCRAdream")
            {
                
                    UnityEngine.Random.State state = UnityEngine.Random.state;

                    self.col = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
                    if (self.depth <= 0f)
                    {
                        sLeaser.sprites[0].color = self.col;
                        return;
                    }
                    sLeaser.sprites[0].color = Color.Lerp(palette.skyColor, new Color(UnityEngine.Random.value,
                        UnityEngine.Random.value, UnityEngine.Random.value), Mathf.InverseLerp(0f, 5f, self.depth));

                    UnityEngine.Random.state = state;
                
            }
            else orig(self, sLeaser, rCam, palette);
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