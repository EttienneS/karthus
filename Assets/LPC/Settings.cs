using LPC.Spritesheet.Generator.Enums;
using LPC.Spritesheet.Generator.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace LPC.Spritesheet.Generator
{
    public static class Settings
    {
        public static Dictionary<(Animation animation, Orientation orientation), (int row, int frames)> SpriteSheetAnimationDefinition = new Dictionary<(Animation, Orientation), (int row, int frames)>()
        {
            { (Animation.Spellcast, Orientation.Back), (0,7) },
            { (Animation.Spellcast, Orientation.Left), (1,7) },
            { (Animation.Spellcast, Orientation.Front), (2,7) },
            { (Animation.Spellcast, Orientation.Right), (3,7) },
            { (Animation.Thrust, Orientation.Back), (4,8) },
            { (Animation.Thrust, Orientation.Left), (5,8) },
            { (Animation.Thrust, Orientation.Front), (6,8) },
            { (Animation.Thrust, Orientation.Right), (7,8) },
            { (Animation.Walk, Orientation.Back), (8,9) },
            { (Animation.Walk, Orientation.Left), (9,9) },
            { (Animation.Walk, Orientation.Front), (10,9) },
            { (Animation.Walk, Orientation.Right), (11,9) },
            { (Animation.Slash, Orientation.Back), (12,6) },
            { (Animation.Slash, Orientation.Left), (13,6) },
            { (Animation.Slash, Orientation.Front), (14,6) },
            { (Animation.Slash, Orientation.Right), (15,6) },
            { (Animation.Shoot, Orientation.Back), (16,13) },
            { (Animation.Shoot, Orientation.Left), (17,13) },
            { (Animation.Shoot, Orientation.Front), (18,13) },
            { (Animation.Shoot, Orientation.Right), (19,13) },
            { (Animation.Die, Orientation.Back), (20,6) },
            { (Animation.Die, Orientation.Left), (20,6) },
            { (Animation.Die, Orientation.Front), (20,6) },
            { (Animation.Die, Orientation.Right), (20,6) },
        };

        public static int SheetHeight { get; set; } = 1344;

        public static int SheetWidth { get; set; } = 832;

        public static int SpriteHeight { get; set; } = 64;

        public static int SpriteWidth { get; set; } = 64;

        public static int PixelsPerUnit { get; set; } = 64;

        public static List<ISpriteSheet> GetOrderedLayers(List<ISpriteSheet> layers)
        {
            return layers.OrderBy(l => (int)l.SpriteLayer).ToList();
        }

        public static List<ISpriteSheet> GetOrderedLayersDescending(List<ISpriteSheet> layers)
        {
            return layers.OrderByDescending(l => (int)l.SpriteLayer).ToList();
        }

        public static string[] ToneConstants = new[]
        {
            "dark", "dark2", "light", "black", "brown", "olive", "peach", "white", "tanned", "tanned2"
        };
    }
}