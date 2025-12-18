using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using Motor3D_Educativo_P2.Geometry;

namespace Motor3D_Educativo_P2.Core
{
    public static class ProceduralTextures
    {
        // Public settings to tweak generators at runtime (can be modified from UI later)
        public static ProceduralTextureSettings Settings { get; } = new ProceduralTextureSettings();

        public static Bitmap Generate(string name, int size = 512)
        {
            switch ((name ?? "").ToLowerInvariant())
            {
                case "madera":
                case "wood":
                    return GenerateWood(size);
                case "metal":
                case "metallic":
                    return GenerateMetal(size);
                case "piedra":
                case "stone":
                    return GenerateStone(size);
                case "concreto":
                case "concrete":
                    return GenerateConcrete(size);
                default:
                    return null;
            }
        }

        // Settings class and per-material subsettings
        public class ProceduralTextureSettings
        {
            public WoodSettings Wood { get; } = new WoodSettings();
            public MetalSettings Metal { get; } = new MetalSettings();
            public StoneSettings Stone { get; } = new StoneSettings();
            public ConcreteSettings Concrete { get; } = new ConcreteSettings();

            public class WoodSettings
            {
                public float RingsScale = 20f;
                public float GrainScale = 150f;
                public Color Light = Color.FromArgb(255, 210, 160);
                public Color Dark = Color.FromArgb(120, 70, 30);
                public float Contrast = 1.0f;
            }

            public class MetalSettings
            {
                public float StripeFreq = 40f;
                public float NoiseScale = 80f;
                public float Brightness = 0.9f;
            }

            public class StoneSettings
            {
                public float Scale = 6f;
                public float Contrast = 1.0f;
            }

            public class ConcreteSettings
            {
                public float NoiseScale = 100f;
                public float SpeckIntensity = 60f;
            }
        }

        // Simple hash-based pseudo random (deterministic) for noise
        private static float HashNoise(float x, float y)
        {
            int xi = (int)Math.Floor(x * 374761393);
            int yi = (int)Math.Floor(y * 668265263);
            // Use constants that fit in int to avoid implicit long conversions
            int n = xi * 1836311903 ^ yi * 297121507;
            n = (n << 13) ^ n;
            int nn = (n * (n * n * 15731 + 789221) + 1376312589) & 0x7fffffff;
            return 1.0f - ((float)nn / 1073741824.0f);
        }

        private static float SmoothNoise(float x, float y)
        {
            float corners = (HashNoise(x - 1, y - 1) + HashNoise(x + 1, y - 1) + HashNoise(x - 1, y + 1) + HashNoise(x + 1, y + 1)) / 16f;
            float sides = (HashNoise(x - 1, y) + HashNoise(x + 1, y) + HashNoise(x, y - 1) + HashNoise(x, y + 1)) / 8f;
            float center = HashNoise(x, y) / 4f;
            return corners + sides + center;
        }

        private static float InterpolatedNoise(float x, float y)
        {
            int integer_X = (int)Math.Floor(x);
            float fractional_X = x - integer_X;

            int integer_Y = (int)Math.Floor(y);
            float fractional_Y = y - integer_Y;

            float v1 = SmoothNoise(integer_X, integer_Y);
            float v2 = SmoothNoise(integer_X + 1, integer_Y);
            float v3 = SmoothNoise(integer_X, integer_Y + 1);
            float v4 = SmoothNoise(integer_X + 1, integer_Y + 1);

            float i1 = CosineInterpolate(v1, v2, fractional_X);
            float i2 = CosineInterpolate(v3, v4, fractional_X);

            return CosineInterpolate(i1, i2, fractional_Y);
        }

        private static float PerlinNoise(float x, float y)
        {
            float total = 0f;
            float p = 0.5f;
            int n = 4;
            float frequency = 1f;
            float amplitude = 1f;
            for (int i = 0; i < n; i++)
            {
                total += InterpolatedNoise(x * frequency, y * frequency) * amplitude;
                amplitude *= p;
                frequency *= 2f;
            }
            return total;
        }

        private static float CosineInterpolate(float a, float b, float x)
        {
            float ft = x * (float)Math.PI;
            float f = (1f - (float)Math.Cos(ft)) * 0.5f;
            return a * (1f - f) + b * f;
        }

        private static Color Lerp(Color a, Color b, float t)
        {
            t = Math.Min(1f, Math.Max(0f, t));
            return Color.FromArgb(
                (int)(a.A + (b.A - a.A) * t),
                (int)(a.R + (b.R - a.R) * t),
                (int)(a.G + (b.G - a.G) * t),
                (int)(a.B + (b.B - a.B) * t));
        }

        private static Bitmap GenerateWood(int size)
        {
            Bitmap bmp = new Bitmap(size, size);
            var s = Settings.Wood;
            Color light = s.Light;
            Color dark = s.Dark;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float u = (float)x / size;
                    float v = (float)y / size;
                    float nx = u * (s.RingsScale);
                    float ny = v * (s.RingsScale);
                    float n = PerlinNoise(nx, ny) * 0.5f;
                    float rings = (float)Math.Abs(Math.Sin((u * s.RingsScale + n * 0.5f)));
                    float t = 0.5f + 0.5f * rings * s.Contrast;
                    Color c = Lerp(light, dark, t);
                    float grain = PerlinNoise(u * s.GrainScale, v * 1f);
                    int rr = Math.Min(255, Math.Max(0, (int)(c.R * (0.8f + grain * 0.4f))));
                    int gg = Math.Min(255, Math.Max(0, (int)(c.G * (0.8f + grain * 0.4f))));
                    int bb = Math.Min(255, Math.Max(0, (int)(c.B * (0.8f + grain * 0.4f))));
                    bmp.SetPixel(x, y, Color.FromArgb(255, rr, gg, bb));
                }
            }
            return bmp;
        }

        private static Bitmap GenerateMetal(int size)
        {
            Bitmap bmp = new Bitmap(size, size);
            var s = Settings.Metal;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float u = (float)x / size;
                    float v = (float)y / size;
                    float n = PerlinNoise(u * s.NoiseScale, v * 1f) * 0.5f + 0.5f;
                    float stripe = (float)Math.Abs(Math.Sin(u * s.StripeFreq + n * 5f));
                    float val = s.Brightness * (0.6f + 0.4f * stripe + 0.15f * (PerlinNoise(u * 200f, v * 200f)));
                    int c = Math.Min(255, Math.Max(0, (int)(val * 220)));
                    bmp.SetPixel(x, y, Color.FromArgb(255, c, c + 5 > 255 ? 255 : c + 5, c + 20 > 255 ? 255 : c + 20));
                }
            }
            return bmp;
        }

        private static Bitmap GenerateStone(int size)
        {
            Bitmap bmp = new Bitmap(size, size);
            var s = Settings.Stone;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float u = (float)x / size * s.Scale;
                    float v = (float)y / size * s.Scale;
                    int ix = (int)Math.Floor(u);
                    int iy = (int)Math.Floor(v);
                    float best = 1e6f;
                    float val = 0f;
                    for (int oy = -1; oy <= 1; oy++)
                    for (int ox = -1; ox <= 1; ox++)
                    {
                        float cx = ix + ox + HashNoise(ix + ox, iy + oy) * 0.8f;
                        float cy = iy + oy + HashNoise(iy + oy, ix + ox) * 0.8f;
                        float dx = u - cx;
                        float dy = v - cy;
                        float d = dx * dx + dy * dy;
                        if (d < best) { best = d; val = HashNoise(cx, cy); }
                    }
                    float shade = 0.5f + 0.5f * val;
                    int c = Math.Min(255, Math.Max(0, (int)((shade * 200 + PerlinNoise(u, v) * 30) * s.Contrast)));
                    bmp.SetPixel(x, y, Color.FromArgb(255, c, c - 10 < 0 ? 0 : c - 10, c - 30 < 0 ? 0 : c - 30));
                }
            }
            return bmp;
        }

        private static Bitmap GenerateConcrete(int size)
        {
            Bitmap bmp = new Bitmap(size, size);
            var s = Settings.Concrete;
            Color basec = Color.FromArgb(200, 200, 200);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float u = (float)x / size;
                    float v = (float)y / size;
                    float n = PerlinNoise(u * s.NoiseScale, v * s.NoiseScale);
                    int speck = (int)(Math.Abs(HashNoise(u * 300f, v * 300f)) * s.SpeckIntensity);
                    int rr = Math.Min(255, Math.Max(0, basec.R + (int)(n * 30) - speck));
                    int gg = Math.Min(255, Math.Max(0, basec.G + (int)(n * 30) - speck));
                    int bb = Math.Min(255, Math.Max(0, basec.B + (int)(n * 30) - speck));
                    bmp.SetPixel(x, y, Color.FromArgb(255, rr, gg, bb));
                }
            }
            return bmp;
        }
    }
}
