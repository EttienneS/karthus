using LPC.Spritesheet.Generator.Enums;
using System.Collections.Generic;

namespace LPC.Spritesheet.Generator.Interfaces
{
    public interface ICharacterSpriteDefinition
    {
        Race Race { get; set; }
        Gender Gender { get; set; }

        List<ISpriteSheet> Layers { get; set; }

        void AddLayer(ISpriteSheet spriteSheet);
    }
}