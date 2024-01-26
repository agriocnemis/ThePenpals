using BepInEx;
using UnityEngine;
using SlugBase.Features;
using static SlugBase.Features.FeatureTypes;
using callingallpenpals;
using MoreSlugcats;
using CustomRegions;
using RWCustom;
using On;
using On.MoreSlugcats;
using System;
using IL;
using System.Drawing.Text;

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
            On.Player.ctor += Player_ctor;

            // locking and unlocking
            On.SlugcatStats.SlugcatUnlocked += SlugcatStats_SlugcatUnlocked;

            // water bouyancy, dream gravity
            On.Player.UpdateMSC += Player_UpdateMSC;


            // general colour things- making dream colourful, making real shades of grey
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

            // actual palette hell! twirls hair and cries<3
            // water is actually evil I think rn
            On.Water.ApplyPalette += Water_ApplyPalette;
            On.WaterFall.ApplyPalette += WaterFall_ApplyPalette;
            On.WaterDrip.ApplyPalette += WaterDrip_ApplyPalette;

            //Stowaway being awake, hopefully
            //I dont want to talk about it
            On.MoreSlugcats.StowawayBugState.AwakeThisCycle += StowAwake;

            // lizards be gray!
            On.LizardGraphics.DrawSprites += LizardGraphics_DrawSprites;

            // ----------------------------------- DREAM THINGS
            // zero-gravity oracles always
            On.SSOracleSwarmer.Update += SSOracleSwarmer_Update;

            //------------------------------------ REAL THINGS
            // omg so real
            // desaturate all rooms
            On.RoomCamera.Update += RoomCamera_Update;

        }

        private bool StowAwake(On.MoreSlugcats.StowawayBugState.orig_AwakeThisCycle orig, MoreSlugcats.StowawayBugState self, int cycle)
        {
            return orig(self, cycle);
           // return true;

        }

        private void WaterDrip_ApplyPalette(On.WaterDrip.orig_ApplyPalette orig, WaterDrip self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            if (self.room.game.session.characterStats.name.value == "NCRAreal")
            {
                palette = new RoomPalette(palette.texture, palette.fogAmount, palette.darkness, Custom.Desaturate(palette.blackColor, 1f), Custom.Desaturate(palette.waterColor1, 1f),
                    Custom.Desaturate(palette.waterColor2, 1f), Custom.Desaturate(palette.waterSurfaceColor1, 1f), Custom.Desaturate(palette.waterSurfaceColor2, 1f),
                    Custom.Desaturate(palette.waterShineColor, 1f), Custom.Desaturate(palette.fogColor, 1f), Custom.Desaturate(palette.skyColor, 1f),
                    palette.shortcutColors[0], palette.shortcutColors[1], palette.shortcutColors[2], palette.shortCutSymbol);
                if (self.waterColor)
                {
                    self.colors = new Color[]
                    {
                        Color.Lerp(palette.waterColor2, palette.waterColor1, 0.5f),
                        palette.waterColor1,
                        new Color(1f, 1f, 1f)
                    };
                    return;
                }
                self.colors = new Color[]
                {
                    palette.blackColor,
                    Color.Lerp(palette.blackColor, new Color(1f, 1f, 1f), 0.5f),
                    new Color(1f, 1f, 1f)
                };
            }
            else
            {
                orig(self, sLeaser, rCam, palette);
            }
        }

        private void WaterFall_ApplyPalette(On.WaterFall.orig_ApplyPalette orig, WaterFall self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            if (self.room.game.session.characterStats.name.value == "NCRAreal")
            {
                palette = new RoomPalette(palette.texture, palette.fogAmount, palette.darkness, Custom.Desaturate(palette.blackColor, 1f), Custom.Desaturate(palette.waterColor1, 1f),
                    Custom.Desaturate(palette.waterColor2, 1f), Custom.Desaturate(palette.waterSurfaceColor1, 1f), Custom.Desaturate(palette.waterSurfaceColor2, 1f),
                    Custom.Desaturate(palette.waterShineColor, 1f), Custom.Desaturate(palette.fogColor, 1f), Custom.Desaturate(palette.skyColor, 1f),
                    palette.shortcutColors[0], palette.shortcutColors[1], palette.shortcutColors[2], palette.shortCutSymbol);
                for (int i = 0; i < self.bubbles.GetLength(0); i++)
                {
                    sLeaser.sprites[1 + i].color = Color.Lerp(palette.waterColor1, palette.waterColor2, 0.3f);
                }
            }
            else
            {
                orig(self, sLeaser, rCam, palette);
            }
        }

        private void Water_ApplyPalette(On.Water.orig_ApplyPalette orig, Water self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            if (self.room.game.session.characterStats.name.value == "NCRAreal")
            {
                self.palette = new RoomPalette(palette.texture, palette.fogAmount, palette.darkness, Custom.Desaturate(palette.blackColor, 1f), Custom.Desaturate(palette.waterColor1, 1f),
                    Custom.Desaturate(palette.waterColor2, 1f), Custom.Desaturate(palette.waterSurfaceColor1, 1f), Custom.Desaturate(palette.waterSurfaceColor2, 1f),
                    Custom.Desaturate(palette.waterShineColor, 1f), Custom.Desaturate(palette.fogColor, 1f), Custom.Desaturate(palette.skyColor, 1f),
                    palette.shortcutColors[0], palette.shortcutColors[1], palette.shortcutColors[2], palette.shortCutSymbol);

            }
            else
            {
                orig(self, sLeaser, rCam, palette);
            }
        }

        private void LizardGraphics_DrawSprites(On.LizardGraphics.orig_DrawSprites orig, LizardGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            if (self.owner.room.game.session.characterStats.name.value == "NCRAreal")
            {
                self.lizard.effectColor = self.lizard.lizardParams.standardColor;
                if (self.lizard.Template.type == CreatureTemplate.Type.PinkLizard)
                {
                    self.lizard.effectColor = Custom.HSL2RGB(0.87f, 0f, Custom.ClampedRandomVariation(0.5f, 0.15f, 0.1f));
                }
                else if (self.lizard.Template.type == CreatureTemplate.Type.GreenLizard)
                {
                    self.lizard.effectColor = Custom.HSL2RGB(0.32f, 0f, Custom.ClampedRandomVariation(0.5f, 0.15f, 0.1f));
                }
                else if (self.lizard.Template.type == CreatureTemplate.Type.BlueLizard)
                {
                    self.lizard.effectColor = Custom.HSL2RGB(0.57f, 0f, Custom.ClampedRandomVariation(0.5f, 0.15f, 0.1f));
                }
                else if (self.lizard.Template.type == CreatureTemplate.Type.YellowLizard)
                {
                    self.lizard.effectColor = Custom.HSL2RGB(0.1f, 0f, Custom.ClampedRandomVariation(0.5f, 0.15f, 0.1f));
                }
                else if (self.lizard.Template.type == CreatureTemplate.Type.Salamander)
                {
                    self.lizard.effectColor = Custom.HSL2RGB(0.9f, 0f, Custom.ClampedRandomVariation(0.4f, 0.15f, 0.2f));
                }
                else if (self.lizard.Template.type == CreatureTemplate.Type.RedLizard)
                {
                    self.lizard.effectColor = Custom.HSL2RGB(0.0025f, 0f, Custom.ClampedRandomVariation(0.5f, 0.15f, 0.1f));
                }
                else if (self.lizard.Template.type == CreatureTemplate.Type.CyanLizard)
                {
                    self.lizard.effectColor = Custom.HSL2RGB(0.49f, 0f, Custom.ClampedRandomVariation(0.5f, 0.15f, 0.1f));
                }
                else if (self.lizard.Template.type == MoreSlugcats.MoreSlugcatsEnums.CreatureTemplateType.SpitLizard)
                {
                    self.lizard.effectColor = Custom.HSL2RGB(0.1f, 0f, Custom.ClampedRandomVariation(0.55f, 0.36f, 0.2f));
                    float num8 = UnityEngine.Random.Range(0.7f, 1f);
                    if (num8 >= 0.8f)
                    {
                        self.ivarBodyColor = new HSLColor(UnityEngine.Random.Range(0.075f, 0.125f), 0f, num8).rgb;
                        self.lizard.effectColor = Custom.HSL2RGB(Custom.WrappedRandomVariation(0.1f, 0.03f, 0.2f), 0.55f, Custom.ClampedRandomVariation(0.55f, 0.05f, 0.2f));
                    }
                    else
                    {
                        self.ivarBodyColor = new HSLColor(UnityEngine.Random.Range(0.075f, 0.125f), 0f, num8).rgb;
                    }
                }
            }
            orig(self, sLeaser, rCam, timeStacker, camPos);
        }

        private void DangleFruit_ApplyPalette(On.DangleFruit.orig_ApplyPalette orig, DangleFruit self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            if (self.room.game.session.characterStats.name.value == "NCRAdream")
            {
                UnityEngine.Random.State state = UnityEngine.Random.state;
                UnityEngine.Random.InitState(self.abstractPhysicalObject.ID.RandomSeed);
                sLeaser.sprites[0].color = palette.blackColor;
                self.color = Color.Lerp(new Color(0.8f, 0.8f, 0.8f), palette.blackColor, self.darkness);
                UnityEngine.Random.state = state;
            }
            else if (self.room.game.session.characterStats.name.value == "NCRAreal")
            {
                sLeaser.sprites[0].color = Color.black;
                self.color = Color.Lerp(new Color(0.8f, 0.8f, 0.8f), Color.black, self.darkness);
            }
            else orig(self, sLeaser, rCam, palette);
        }

        private void Lantern_Update(On.Lantern.orig_Update orig, Lantern self, bool eu)
        {
            if ((self.room.game.session.characterStats.name.value == "NCRAreal") && self.lightSource == null)
            {
                self.lightSource = new LightSource(self.firstChunk.pos, false, new UnityEngine.Color(0.9f, 0.9f, 0.9f), self);
                self.room.AddObject(self.lightSource);
            }
            else
            {
                orig.Invoke(self, eu);
            }
        }

        private void Lantern_ApplyPalette(On.Lantern.orig_ApplyPalette orig, Lantern self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            if (self.room.game.session.characterStats.name.value == "NCRAreal")
            {
                sLeaser.sprites[0].color = new UnityEngine.Color(0.7f, 0.7f, 0.7f);
                sLeaser.sprites[1].color = new UnityEngine.Color(1f, 1f, 1f);
                sLeaser.sprites[2].color = UnityEngine.Color.Lerp(new UnityEngine.Color(0.8f, 0.8f, 0.8f), new UnityEngine.Color(1f, 1f, 1f), 0.3f);
                sLeaser.sprites[3].color = new UnityEngine.Color(0.9f, 0.9f, 0.9f);
                if (self.stick != null)
                {
                    sLeaser.sprites[4].color = Color.black;
                }
            }
            else
            {
                orig.Invoke(self, sLeaser, rCam, palette);
            }
        }

        private void RoomCamera_Update(On.RoomCamera.orig_Update orig, RoomCamera self)
        {
            orig(self);
            if (self.room.game.session.characterStats.name.value == "NCRAreal" && self.effect_desaturation != 1f)
            {
                self.effect_desaturation = 1f;
            }
        }

        private void FlyGraphics_ApplyPalette(On.FlyGraphics.orig_ApplyPalette orig, FlyGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            if (self.owner.room.game.session.characterStats.name.value == "NCRAdream")
            {
                UnityEngine.Random.State state = UnityEngine.Random.state;
                UnityEngine.Random.InitState(self.fly.abstractCreature.ID.RandomSeed);
                for (int i = 0; i < 3; i++)
                {
                    sLeaser.sprites[i].color = new UnityEngine.Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
                }
                UnityEngine.Random.state = state;
            }
            else orig(self, sLeaser, rCam, palette);
        }

        private void PoleMimicGraphics_ApplyPalette(On.PoleMimicGraphics.orig_ApplyPalette orig, PoleMimicGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            if (self.owner.room.game.session.characterStats.name.value == "NCRAreal")
            {
                self.mimicColor = Color.Lerp(palette.texture.GetPixel(4, 3), palette.fogColor, palette.fogAmount * 0.13333334f);
                self.blackColor = Color.black;
            }
            else if (self.owner.room.game.session.characterStats.name.value == "NCRAdream")
            {
                UnityEngine.Random.State state = UnityEngine.Random.state;
                UnityEngine.Random.InitState(self.pole.abstractCreature.ID.RandomSeed);
                self.mimicColor = new UnityEngine.Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
                self.blackColor = new UnityEngine.Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
                UnityEngine.Random.state = state;
            }
            else
            {
                orig(self, sLeaser, rCam, palette);
            }
        }

        private void FlyLure_ApplyPalette(On.FlyLure.orig_ApplyPalette orig, FlyLure self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            if (self.room.game.session.characterStats.name.value == "NCRAdream")
            {
                self.color = UnityEngine.Color.Lerp(new UnityEngine.Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value), palette.fogColor, UnityEngine.Random.value);
                self.UpdateColor(sLeaser, false);
            }
            else if (self.room.game.session.characterStats.name.value == "NCRAreal")
            {
                UnityEngine.Random.State state = UnityEngine.Random.state;
                UnityEngine.Random.InitState(self.abstractPhysicalObject.ID.RandomSeed);
                self.color = UnityEngine.Color.Lerp(Color.white, palette.fogColor, 0.3f);
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
            if (speed > 5f && firstContact && (self.room.game.session.characterStats.name.value == "NCRAdream"))
            {
                Vector2 pos = self.bodyChunks[chunk].pos + direction.ToVector2() * self.bodyChunks[chunk].rad * 0.9f;
                int num = 0;
                while ((float)num < Mathf.Round(Custom.LerpMap(speed, 5f, 15f, 2f, 8f)))
                {
                    self.room.AddObject(new Spark(pos, direction.ToVector2() * Custom.LerpMap(speed, 5f, 15f, -2f, -8f) + Custom.RNV() * UnityEngine.Random.value * Custom.LerpMap(speed, 5f, 15f, 2f, 4f), UnityEngine.Color.Lerp(new UnityEngine.Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value), new UnityEngine.Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value), UnityEngine.Random.value * 0.5f), null, 19, 47));
                    num++;
                }
            }
            else if (speed > 5f && firstContact && (self.room.game.session.characterStats.name.value == "NCRAreal"))
            {
                Vector2 pos = self.bodyChunks[chunk].pos + direction.ToVector2() * self.bodyChunks[chunk].rad * 0.9f;
                int num = 0;
                while ((float)num < Mathf.Round(Custom.LerpMap(speed, 5f, 15f, 2f, 8f)))
                {
                    self.room.AddObject(new Spark(pos, direction.ToVector2() * Custom.LerpMap(speed, 5f, 15f, -2f, -8f) + Custom.RNV() * UnityEngine.Random.value * Custom.LerpMap(speed, 5f, 15f, 2f, 4f), UnityEngine.Color.Lerp(new UnityEngine.Color(0.8f, 0.8f, 0.8f), new UnityEngine.Color(1f, 1f, 1f), UnityEngine.Random.value * 0.5f), null, 19, 47));
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
            if (self.room.game.session.characterStats.name.value == "NCRAdream")
            {
                UnityEngine.Random.State state = UnityEngine.Random.state;
                UnityEngine.Random.InitState(self.abstractPhysicalObject.ID.RandomSeed);
                sLeaser.sprites[self.StalkSprite(0)].color = new UnityEngine.Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
                self.StoredBlackColor = new UnityEngine.Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
                UnityEngine.Color pixel = palette.texture.GetPixel(0, 5);
                self.StoredPlantColor = pixel;
                for (int i = 0; i < (sLeaser.sprites[self.StalkSprite(1)] as TriangleMesh).verticeColors.Length; i++)
                {
                    float num = (float)i / (float)((sLeaser.sprites[self.StalkSprite(1)] as TriangleMesh).verticeColors.Length - 1);
                    (sLeaser.sprites[self.StalkSprite(1)] as TriangleMesh).verticeColors[i] = UnityEngine.Color.Lerp(palette.blackColor, pixel, 0.4f + Mathf.Pow(1f - num, 0.5f) * 0.4f);
                }
                self.yellowColor = UnityEngine.Color.Lerp(new UnityEngine.Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value), palette.blackColor, self.AbstractCob.dead 
                    ? (0.95f + 0.5f * rCam.PaletteDarkness()) : (0.18f + 0.7f * rCam.PaletteDarkness()));
                for (int j = 0; j < 2; j++)
                {
                    for (int k = 0; k < (sLeaser.sprites[self.ShellSprite(j)] as TriangleMesh).verticeColors.Length; k++)
                    {
                        float num2 = 1f - (float)k / (float)((sLeaser.sprites[self.ShellSprite(j)] as TriangleMesh).verticeColors.Length - 1);
                        (sLeaser.sprites[self.ShellSprite(j)] as TriangleMesh).verticeColors[k] = UnityEngine.Color.Lerp(palette.blackColor, new UnityEngine.Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value), Mathf.Pow(num2, 2.5f) * 0.4f);
                    }
                }
                sLeaser.sprites[self.CobSprite].color = self.yellowColor;
                UnityEngine.Color color = self.yellowColor + new UnityEngine.Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value) * Mathf.Lerp(1f, 0.15f, rCam.PaletteDarkness());
                if (self.AbstractCob.dead)
                {
                    color = UnityEngine.Color.Lerp(self.yellowColor, pixel, 0.75f);
                }
                for (int l = 0; l < self.seedPositions.Length; l++)
                {
                    sLeaser.sprites[self.SeedSprite(l, 0)].color = self.yellowColor;
                    sLeaser.sprites[self.SeedSprite(l, 1)].color = color;
                    sLeaser.sprites[self.SeedSprite(l, 2)].color = UnityEngine.Color.Lerp(new UnityEngine.Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value), palette.blackColor, self.AbstractCob.dead ? 0.6f : 0.3f);
                }
                for (int m = 0; m < self.leaves.GetLength(0); m++)
                {
                    sLeaser.sprites[self.LeafSprite(m)].color = palette.blackColor;
                }
            UnityEngine.Random.state = state;
            }
            else if (self.room.game.session.characterStats.name.value == "NCRAreal")
            {
                sLeaser.sprites[self.StalkSprite(0)].color = Color.black;
                self.StoredBlackColor = Color.black;
                UnityEngine.Color pixel = Color.gray;
                self.StoredPlantColor = pixel;
                for (int i = 0; i < (sLeaser.sprites[self.StalkSprite(1)] as TriangleMesh).verticeColors.Length; i++)
                {
                    float num = (float)i / (float)((sLeaser.sprites[self.StalkSprite(1)] as TriangleMesh).verticeColors.Length - 1);
                    (sLeaser.sprites[self.StalkSprite(1)] as TriangleMesh).verticeColors[i] = UnityEngine.Color.Lerp(Color.black, pixel, 0.4f + Mathf.Pow(1f - num, 0.5f) * 0.4f);
                }
                self.yellowColor = UnityEngine.Color.Lerp(new UnityEngine.Color(0.8f, 0.8f, 0.8f), Color.black, self.AbstractCob.dead ? (0.95f + 0.5f * rCam.PaletteDarkness()) : (0.18f + 0.7f * rCam.PaletteDarkness()));
                for (int j = 0; j < 2; j++)
                {
                    for (int k = 0; k < (sLeaser.sprites[self.ShellSprite(j)] as TriangleMesh).verticeColors.Length; k++)
                    {
                        float num2 = 1f - (float)k / (float)((sLeaser.sprites[self.ShellSprite(j)] as TriangleMesh).verticeColors.Length - 1);
                        (sLeaser.sprites[self.ShellSprite(j)] as TriangleMesh).verticeColors[k] = UnityEngine.Color.Lerp(Color.black, new UnityEngine.Color(0.5f, 0.5f, 0.5f), Mathf.Pow(num2, 2.5f) * 0.4f);
                    }
                }
                sLeaser.sprites[self.CobSprite].color = self.yellowColor;
                UnityEngine.Color color = self.yellowColor + new UnityEngine.Color(1f, 1f, 1f) * Mathf.Lerp(1f, 0.15f, rCam.PaletteDarkness());
                if (self.AbstractCob.dead)
                {
                    color = UnityEngine.Color.Lerp(self.yellowColor, pixel, 0.75f);
                }
                for (int l = 0; l < self.seedPositions.Length; l++)
                {
                    sLeaser.sprites[self.SeedSprite(l, 0)].color = self.yellowColor;
                    sLeaser.sprites[self.SeedSprite(l, 1)].color = color;
                    sLeaser.sprites[self.SeedSprite(l, 2)].color = UnityEngine.Color.Lerp(new UnityEngine.Color(0.2f, 0.2f, 0.2f), Color.black, self.AbstractCob.dead ? 0.6f : 0.3f);
                }
                for (int m = 0; m < self.leaves.GetLength(0); m++)
                {
                    sLeaser.sprites[self.LeafSprite(m)].color = Color.black;
                }
            }
            else
            {
                orig.Invoke(self, sLeaser, rCam, palette);
            }
        }

        private void Player_UpdateMSC(On.Player.orig_UpdateMSC orig, Player self)
        {
            orig(self);
            if (self.GetDreamCat().IsDream)
            {
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

                // this is here for intro purposes :]
                //if (self.room.game.session is StoryGameSession && self.room.game.session.characterStats.name.value == "NCRreal")
                //{
                    //string name = self.room.abstractRoom.name;
                    //if (name == "SB_L01")
                    //{
                        //self.room.AddObject(new EntropyIntro(self.room));
                    //}
                //}
            }
            // ---------------------------------------------------- REAL ----------------------------------------------------
            if (self.slugcatStats.name.value == "NCRAreal")
            {
                self.GetRealCat().IsReal = true;

                // this is here for intro purposes :]
                //if (self.room.game.session is StoryGameSession && self.room.game.session.characterStats.name.value == "NCRreal")
                //{
                //string name = self.room.abstractRoom.name;
                //if (name == "SB_L01")
                //{
                //self.room.AddObject(new EntropyIntro(self.room));
                //}
                //}
            }
        }

        private void GreenSpark_Update(On.GreenSparks.GreenSpark.orig_Update orig, GreenSparks.GreenSpark self, bool eu)
        {
            if (self.room.game.session.characterStats.name.value == "NCRAdream")
            {
                self.col = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
                orig(self, eu);
            }
            if (self.room.game.session.characterStats.name.value == "NCRAreal")
            {
                self.col = Color.white;
                orig(self, eu);
            }
            else { orig(self, eu); }
        }

        private void GreenSpark_ApplyPalette(On.GreenSparks.GreenSpark.orig_ApplyPalette orig, GreenSparks.GreenSpark self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            orig(self, sLeaser, rCam, palette);
            if (self.room.game.session.characterStats.name.value == "NCRAdream")
            {
                sLeaser.sprites[0].color = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
            }
            else if (self.room.game.session.characterStats.name.value == "NCRAreal")
            {
                sLeaser.sprites[0].color = Color.white;
            }
        }

        private void FireFly_ctor(On.FireFly.orig_ctor orig, FireFly self, Room room, Vector2 pos)
        {
            orig(self, room, pos);
            if (room.game.session.characterStats.name.value == "NCRAdream")
            {
                self.col = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
            }
            else if (room.game.session.characterStats.name.value == "NCRAreal")
            {
                self.col = Color.white;
            }
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
        //emotional support code omg...
        private void LoadResources(RainWorld rainWorld)
        {
        }
    }
}