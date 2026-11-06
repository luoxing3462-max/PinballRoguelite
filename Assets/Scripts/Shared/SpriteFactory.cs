using UnityEngine;

namespace PinballRoguelite
{
    public static class SpriteFactory
    {
        private static Sprite square;
        private static Sprite circle;

        public static Sprite Square
        {
            get
            {
                if (square != null) return square;
                var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false)
                {
                    name = "RuntimeSquare",
                    filterMode = FilterMode.Bilinear,
                    wrapMode = TextureWrapMode.Clamp,
                    hideFlags = HideFlags.DontSave
                };
                texture.SetPixels(new[] { Color.white, Color.white, Color.white, Color.white });
                texture.Apply();
                square = Sprite.Create(texture, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f), 2f);
                square.name = "RuntimeSquareSprite";
                return square;
            }
        }

        public static Sprite Circle
        {
            get
            {
                if (circle != null) return circle;
                const int size = 64;
                var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
                {
                    name = "RuntimeCircle",
                    filterMode = FilterMode.Bilinear,
                    wrapMode = TextureWrapMode.Clamp,
                    hideFlags = HideFlags.DontSave
                };
                var pixels = new Color[size * size];
                var center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
                var radius = size * 0.48f;
                for (var y = 0; y < size; y++)
                for (var x = 0; x < size; x++)
                {
                    var distance = Vector2.Distance(new Vector2(x, y), center);
                    var alpha = Mathf.Clamp01(radius - distance + 0.9f);
                    pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
                texture.SetPixels(pixels);
                texture.Apply();
                circle = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
                circle.name = "RuntimeCircleSprite";
                return circle;
            }
        }

        public static SpriteRenderer AddVisual(Transform parent, Sprite sprite, Color color, Vector2 size, int order)
        {
            var visual = new GameObject("Visual");
            visual.transform.SetParent(parent, false);
            visual.transform.localScale = new Vector3(size.x, size.y, 1f);
            var renderer = visual.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = color;
            renderer.sortingOrder = order;
            return renderer;
        }
    }
}
