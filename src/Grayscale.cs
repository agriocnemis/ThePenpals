using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RWCustom;
using AssetBundles;

namespace NCRApenpals.GrayscaleEffect
{
    public static class GrayscaleEffect
    {
        private static ConditionalWeakTable<RoomCamera, FSprite> _cwt = new();

        public static void Apply()
        {
            var bundle = AssetBundle.LoadFromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("NCRApenpals.Assets.grayscalegrab"));
            Custom.rainWorld.Shaders["VigaroGrayscaleGrab"] = FShader.CreateShader("VigaroGrayscaleGrab", bundle.LoadAsset<Shader>("Assets/GrayscaleGrab.shader"));
            bundle.Unload(false);

            On.RoomCamera.ctor += RoomCamera_ctor;
            On.RoomCamera.ClearAllSprites += RoomCamera_ClearAllSprites;
            On.RoomCamera.Update += RoomCamera_Update;
        }

        private static void RoomCamera_Update(On.RoomCamera.orig_Update orig, RoomCamera self)
        {
            orig(self);

            if (_cwt.TryGetValue(self, out var sprite))
            {
                sprite.alpha = 1f;
            }
        }

        private static void RoomCamera_ctor(On.RoomCamera.orig_ctor orig, RoomCamera self, RainWorldGame game, int cameraNumber)
        {
            orig(self, game, cameraNumber);

            if (!_cwt.TryGetValue(self, out var sprite))
            {
                sprite = new FSprite("pixel")
                {
                    shader = Custom.rainWorld.Shaders["VigaroGrayscaleGrab"],
                    scaleX = 1366,
                    scaleY = 768,
                    anchorX = 0,
                    anchorY = 0,
                    alpha = 0
                };

                _cwt.Add(self, sprite);
            }

            Futile.stage.AddChild(sprite);
            sprite.MoveBehindOtherNode(self.ReturnFContainer("HUD"));
        }

        private static void RoomCamera_ClearAllSprites(On.RoomCamera.orig_ClearAllSprites orig, RoomCamera self)
        {
            orig(self);

            if (_cwt.TryGetValue(self, out var sprite))
            {
                sprite.RemoveFromContainer();
            }
        }
    }
}
