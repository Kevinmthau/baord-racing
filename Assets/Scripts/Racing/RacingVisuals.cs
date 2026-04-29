using UnityEngine;

namespace BoardRacing
{
    public static class RacingVisuals
    {
        private static Sprite whiteSprite;
        private static Font defaultFont;
        private static PhysicsMaterial2D wallPhysicsMaterial;

        public static Sprite WhiteSprite
        {
            get
            {
                if (whiteSprite != null)
                {
                    return whiteSprite;
                }

                var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false)
                {
                    name = "Runtime White Pixel",
                    filterMode = FilterMode.Point
                };
                texture.SetPixel(0, 0, Color.white);
                texture.Apply();
                whiteSprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
                whiteSprite.name = "Runtime White Sprite";
                return whiteSprite;
            }
        }

        public static Font DefaultFont
        {
            get
            {
                if (defaultFont != null)
                {
                    return defaultFont;
                }

                defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                if (defaultFont == null)
                {
                    defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
                }

                return defaultFont;
            }
        }

        public static PhysicsMaterial2D WallPhysicsMaterial
        {
            get
            {
                if (wallPhysicsMaterial != null)
                {
                    return wallPhysicsMaterial;
                }

                wallPhysicsMaterial = new PhysicsMaterial2D("Low Friction Track Wall")
                {
                    friction = 0.02f,
                    bounciness = 0.08f
                };
                return wallPhysicsMaterial;
            }
        }

        public static Material CreateMaterial(Color color)
        {
            var shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null)
            {
                shader = Shader.Find("Sprites/Default");
            }

            var material = new Material(shader)
            {
                color = color
            };

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            return material;
        }
    }
}
