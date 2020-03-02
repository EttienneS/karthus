using LPC.Spritesheet.Generator.Enums;

namespace LPC.Spritesheet.Generator.Interfaces
{
    public interface ISpriteSheet
    {
        SpriteLayer SpriteLayer { get; set; }

        Gender Gender { get; set; }

        Race Race { get; set; }

        string DisplayName { get; set; }

        string FullName { get; set; }

        byte[] SpriteData { get; set; }

        string[] Tags { get; set; }
    }
}