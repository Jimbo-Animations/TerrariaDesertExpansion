using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReLogic.Content;
using Terraria.WorldBuilding;

namespace TerrariaDesertExpansion.Systems.Utilities
{
    // Code taken from a variety of sources. Ebon provided the majority of it.

    static class BasicUtils
    {
        public static Vector2 findGroundUnder(this Vector2 position)
        {
            Vector2 returned = position;
            while (!WorldUtils.Find(returned.ToTileCoordinates(), Searches.Chain(new Searches.Down(1), new GenCondition[]
                {
                new Conditions.IsSolid()
                }), out _))
            {
                returned.Y++;
            }

            return returned;
        }

        public static Vector2 findSurfaceAbove(this Vector2 position)
        {
            Vector2 returned = position;
            while (WorldUtils.Find(returned.ToTileCoordinates(), Searches.Chain(new Searches.Up(1), new GenCondition[]
                {
                new Conditions.IsSolid()
                }), out _))
            {
                returned.Y--;
            }

            return returned;
        }

        public static Vector2 findCeilingAbove(this Vector2 position)
        {
            Vector2 returned = position;
            while (!WorldUtils.Find(returned.ToTileCoordinates(), Searches.Chain(new Searches.Up(1), new GenCondition[]
                {
        new Conditions.IsSolid()
                }), out _))
            {
                returned.Y--;
            }

            return returned;
        }

        // Casts a line at a set position, with a set length.

        public static Vector2 Cast(Vector2 start, Vector2 direction, float length, bool platformCheck = false)
        {
            direction.SafeNormalize(Vector2.UnitY);
            Vector2 output = start;

            for (int i = 0; i < length; i++)
            {
                if ((Collision.CanHitLine(output, 0, 0, output + direction, 0, 0) && (platformCheck ? !Collision.SolidTiles(output, 1, 1, platformCheck) && Main.tile[(int)output.X / 16, (int)output.Y / 16].TileType != TileID.Platforms : true)))
                {
                    output += direction;
                }
                else
                {
                    break;
                }
            }

            return output;
        }

        // Casts a line and returns the length.

        public static float CastLength(Vector2 start, Vector2 direction, float length, bool platformCheck = false)
        {
            Vector2 end = Cast(start, direction, length, platformCheck);
            return (end - start).Length();
        }

        // Checks if a value is within a set range of another.

        public static bool CloseTo(this float f, float target, float range = 1f)
        {
            return f > target - range && f < target + range;
        }

        public static float ClosestTo(this IEnumerable<float> collection, float target)
        {
            // NB Method will return int.MaxValue for a sequence containing no elements.
            // Apply any defensive coding here as necessary.
            var closest = float.MaxValue;
            var minDifference = float.MaxValue;
            foreach (var element in collection)
            {
                var difference = Math.Abs(element - target);
                if (minDifference > difference)
                {
                    minDifference = difference;
                    closest = element;
                }
            }

            return closest;
        }
        public static int IndexOfClosestTo(this IEnumerable<float> collection, float target)
        {
            // NB Method will return int.MaxValue for a sequence containing no elements.
            // Apply any defensive coding here as necessary.
            int closest = 0;
            var minDifference = float.MaxValue;
            foreach (float element in collection)
            {
                var difference = Math.Abs(element - target);
                if (minDifference > difference)
                {
                    minDifference = difference;
                    closest = Array.IndexOf(collection.ToArray(), element);
                }
            }

            return closest;
        }
        public static float Closer(float a, float b, float compareValue)
        {

            float calcA = Math.Abs(a - compareValue);
            float calcB = Math.Abs(b - compareValue);

            if (calcA == calcB)
            {
                return 0;
            }

            if (calcA < calcB)
            {
                return a;
            }

            return b;
        }

        public static Color ColorLerpCycle(float time, float cycleTime, params Color[] colors)
        {
            if (colors.Length == 0) return default(Color);

            int index = (int)(time / cycleTime * colors.Length) % colors.Length;
            float lerpAmount = time / cycleTime * colors.Length % 1;

            return Color.Lerp(colors[index], colors[(index + 1) % colors.Length], lerpAmount);
        }

        public static Color HunterPotionColor(this NPC npc)
        {
            return Color.Lerp(Color.OrangeRed * 0.5f, Color.Transparent, Math.Clamp(Utils.GetLerpValue(npc.Size.Length(), 0, Main.LocalPlayer.Distance(npc.Center)), 0, 1));
        }

        // Avoids division by zero.

        public static float Safe(this float f, float x = 1)
        {
            return f + (f == 0 ? x : 0);
        }

        // Finds the shortest path to a desired angle.

        public static float ShortestPathToAngle(float from, float to)
        {
            float difference = (to - from) % MathHelper.TwoPi;
            return (2 * difference % MathHelper.TwoPi) - difference;
        }

        // Lerps to the shortest path to a desired angle.

        public static float LerpAngle(float from, float to, float t)
        {
            return from + ShortestPathToAngle(from, to) * t;
        }

        public static Rectangle ToRectangle(this System.Drawing.RectangleF rect)
        {
            return new Rectangle((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
        }    
    }
}
