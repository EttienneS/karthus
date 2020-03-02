using LPC.Spritesheet.Generator.Enums;
using LPC.Spritesheet.Generator.Interfaces;
using LPC.Spritesheet.ResourceManager;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace LPC.Spritesheet.Generator
{
    public class CharacterSpriteGenerator
    {
        private List<ISpriteSheet> _spriteLibrary;

        public CharacterSpriteGenerator(IResourceManager resoureManager)
        {
            ResourceManager = resoureManager;
        }

        public IResourceManager ResourceManager { get; set; }

        public List<ISpriteSheet> SpriteLibrary
        {
            get
            {
                if (_spriteLibrary == null)
                {
                    _spriteLibrary = new List<ISpriteSheet>();

                    // todo: revisit this later to add a 'blank' option to each
                    //foreach (SpriteLayer layer in Enum.GetValues(typeof(SpriteLayer)))
                    //{
                    //    _spriteLibrary.Add(new SpriteSheet("None", "", Gender.Either, layer));
                    //}

                    _spriteLibrary.AddRange(GetSprites("body/female", SpriteLayer.Body, SearchOption.TopDirectoryOnly));
                    _spriteLibrary.AddRange(GetSprites("body/female/orcs", SpriteLayer.Body, SearchOption.TopDirectoryOnly));

                    _spriteLibrary.AddRange(GetSprites("body/female/nose", SpriteLayer.Nose));
                    _spriteLibrary.AddRange(GetSprites("body/female/eyes", SpriteLayer.Eyes));
                    _spriteLibrary.AddRange(GetSprites("body/female/ears", SpriteLayer.Ears));

                    _spriteLibrary.AddRange(GetSprites("body", SpriteLayer.Body, SearchOption.TopDirectoryOnly, "^((?!24.png|25.png).)*$"));

                    _spriteLibrary.AddRange(GetSprites("body/male", SpriteLayer.Body, SearchOption.TopDirectoryOnly));
                    _spriteLibrary.AddRange(GetSprites("body/male/orcs", SpriteLayer.Body, SearchOption.TopDirectoryOnly));

                    _spriteLibrary.AddRange(GetSprites("body/male/nose", SpriteLayer.Nose));
                    _spriteLibrary.AddRange(GetSprites("body/male/eyes", SpriteLayer.Eyes));
                    _spriteLibrary.AddRange(GetSprites("body/male/ears", SpriteLayer.Ears));

                    _spriteLibrary.AddRange(GetSprites("body", SpriteLayer.Wound, SearchOption.TopDirectoryOnly, ".+blood"));

                    _spriteLibrary.AddRange(GetSprites("facial", SpriteLayer.Facial));

                    _spriteLibrary.AddRange(GetSprites("feet", SpriteLayer.Shoes));

                    _spriteLibrary.AddRange(GetSprites("legs/pants", SpriteLayer.Legs));
                    _spriteLibrary.AddRange(GetSprites("legs/skirt", SpriteLayer.Legs));

                    _spriteLibrary.AddRange(GetSprites("feet", SpriteLayer.Boots));

                    _spriteLibrary.AddRange(GetSprites("torso", SpriteLayer.Clothes, SearchOption.AllDirectories, "^((?!plate|mail|back).)*$"));

                    _spriteLibrary.AddRange(GetSprites("torso/chain", SpriteLayer.Mail));
                    _spriteLibrary.AddRange(GetSprites("torso/chain/tabard", SpriteLayer.Jacket));

                    _spriteLibrary.AddRange(GetSprites("arms", SpriteLayer.Arms));

                    _spriteLibrary.AddRange(GetSprites("shoulders", SpriteLayer.Shoulders));

                    _spriteLibrary.AddRange(GetSprites("hands/bracers", SpriteLayer.Bracers));

                    _spriteLibrary.AddRange(GetSprites("legs/armor", SpriteLayer.Greaves));

                    _spriteLibrary.AddRange(GetSprites("hands/gloves", SpriteLayer.Gloves));

                    _spriteLibrary.AddRange(GetSprites("belt", SpriteLayer.Belts));
                    _spriteLibrary.AddRange(GetSprites("belt", SpriteLayer.Buckles, SearchOption.AllDirectories, "buckles.*"));

                    _spriteLibrary.AddRange(GetSprites("accessories", SpriteLayer.Necklaces));

                    _spriteLibrary.AddRange(GetSprites("hands/bracelets", SpriteLayer.Bracelet));

                    _spriteLibrary.AddRange(GetSprites("behind_body/cape", SpriteLayer.Cape));
                    _spriteLibrary.AddRange(GetSprites("torso/back", SpriteLayer.Cape));

                    _spriteLibrary.AddRange(GetSprites("accessories/ties", SpriteLayer.Neck));

                    _spriteLibrary.AddRange(GetSprites("weapons/left hand", SpriteLayer.Shield, SearchOption.AllDirectories, "^((?!oversize).)*$"));

                    _spriteLibrary.AddRange(GetSprites("behind_body/equipment", SpriteLayer.Quiver));

                    _spriteLibrary.AddRange(GetSprites("hair/female", SpriteLayer.Hair));
                    _spriteLibrary.AddRange(GetSprites("hair/male", SpriteLayer.Hair));

                    _spriteLibrary.AddRange(GetSprites("head", SpriteLayer.Hats));

                    _spriteLibrary.AddRange(GetSprites("weapons/right hand", SpriteLayer.Weapon, SearchOption.AllDirectories));
                    _spriteLibrary.AddRange(GetSprites("weapons/both hand", SpriteLayer.Weapon, SearchOption.AllDirectories));
                }
                return _spriteLibrary;
            }
        }

        public void AddClothes(ICharacterSpriteDefinition character)
        {
            AddClothesLayer(character, SpriteLayer.Legs);
            AddClothesLayer(character, SpriteLayer.Belts);
            AddClothesLayer(character, SpriteLayer.Buckles);
            AddClothesLayer(character, SpriteLayer.Clothes);

            if (RandomHelper.Random.Next(0,100) > 80)
            {
                AddClothesLayer(character, SpriteLayer.Hats);
            }
        }

        private void AddClothesLayer(ICharacterSpriteDefinition character, SpriteLayer layer)
        {
            var sprites = GetSprites(layer, Race.Any, character.Gender).ToList();
            sprites = sprites.Where(s => !s.Tags.Contains("chain") && !s.Tags.Contains("leather") && !s.Tags.Contains("plate")
                                         && !s.Tags.Contains("overskirt") && !s.Tags.Contains("overskirtDark")).ToList();

            if (sprites.Count > 0)
            {
                character.AddLayer(sprites[RandomHelper.Random.Next(0, sprites.Count)]);
            }
        }

        private void AddArmorLayer(ICharacterSpriteDefinition character, SpriteLayer layer, ArmorType armorType)
        {
            var sprites = GetSprites(layer, Race.Any, character.Gender).ToList();

            if (layer != SpriteLayer.Greaves && layer != SpriteLayer.Shoulders)
            {
                sprites = sprites.Where(s => s.Tags.Contains(armorType.ToString(), StringComparer.OrdinalIgnoreCase)).ToList();
            }

            if (sprites.Count > 0)
            {
                character.AddLayer(sprites[RandomHelper.Random.Next(0, sprites.Count)]);
            }
        }

        public void AddArmor(ICharacterSpriteDefinition character, ArmorType armorType)
        {
            AddArmorLayer(character, SpriteLayer.Greaves, armorType);
            AddArmorLayer(character, SpriteLayer.Shoulders, armorType);
            AddArmorLayer(character, SpriteLayer.Clothes, armorType);
            AddArmorLayer(character, SpriteLayer.Hats, armorType);
        }

        public void AddWeapons(ICharacterSpriteDefinition character)
        {
            var hand = Hand.Right;
            if (RandomHelper.Random.Next(0,100) > 80)
            {
                hand = Hand.Both;
            }

            var weapons = GetSprites(SpriteLayer.Weapon, Race.Any, character.Gender).ToList();
            weapons = weapons.Where(s => s.Tags.Contains(hand.ToString(), StringComparer.OrdinalIgnoreCase))
                             .ToList();

            if (weapons.Count > 0)
            {
                character.AddLayer(weapons[RandomHelper.Random.Next(0, weapons.Count)]);
            }

            if (hand == Hand.Right && RandomHelper.Random.Next(0, 100) > 50)
            {
                var shields = GetSprites(SpriteLayer.Shield, Race.Any, character.Gender).ToList();
                if (shields.Count > 0)
                {
                    character.AddLayer(shields[RandomHelper.Random.Next(0, shields.Count)]);
                }
            }
        }

        public ICharacterSpriteDefinition GetBaseCharacter(Gender gender, Race race)
        {
            var character = new CharacterSpriteDefinition(gender, race);

            var bodySprites = GetSprites(SpriteLayer.Body, race, gender).ToList();
            if (race == Race.Elf)
            {
                bodySprites = GetSprites(SpriteLayer.Body, Race.Human, gender).ToList();
            }
            if (race == Race.Skeleton)
            {
                bodySprites = GetSprites(SpriteLayer.Body, race, Gender.Male).ToList();
            }

            var tone = "";
            var body = bodySprites[RandomHelper.Random.Next(0, bodySprites.Count)];
            character.AddLayer(body);
            tone = body.Tags.FirstOrDefault(t => Settings.ToneConstants.Contains(t));

            if (race == Race.Elf || race == Race.DarkElf)
            {
                if (race == Race.DarkElf)
                {
                    tone = "darkelf";
                }
                else
                {
                    // we do not have elven ears for these tones, add the 'synonyms' to ensure they still get ears
                    tone = tone.Replace("white", "light").Replace("peach", "light")
                               .Replace("olive", "dark").Replace("black", "dark")
                               .Replace("brown", "dark");
                }

                var earSprites = GetBodyPartSprite(SpriteLayer.Ears, gender, race, tone);
                if (earSprites.Count > 0)
                {
                    character.AddLayer(earSprites[RandomHelper.Random.Next(0, earSprites.Count)]);
                }
            }

            var eyeSprites = GetBodyPartSprite(SpriteLayer.Eyes, gender, Race.Any, "");
            if (eyeSprites.Count > 0)
            {
                character.AddLayer(eyeSprites[RandomHelper.Random.Next(0, eyeSprites.Count)]);
            }

            var noseSprites = GetBodyPartSprite(SpriteLayer.Nose, gender, Race.Any, tone);
            if (noseSprites.Count > 0)
            {
                character.AddLayer(noseSprites[RandomHelper.Random.Next(0, noseSprites.Count)]);
            }

            var hairTone = "";
            var noHair = false;

            if (race == Race.Skeleton)
            {
                noHair = true;
            }
            else if (race == Race.Orc && RandomHelper.Random.Next(0, 100) > 20)
            {
                noHair = true;
            }
            else if (gender == Gender.Male && RandomHelper.Random.Next(0, 100) > 90)
            {
                noHair = true;
            }

            if (!noHair)
            {
                var hairSprites = GetBodyPartSprite(SpriteLayer.Hair, gender, Race.Any, "");
                if (hairSprites.Count > 0)
                {
                    var hair = hairSprites[RandomHelper.Random.Next(0, hairSprites.Count)];
                    character.AddLayer(hair);

                    hairTone = Regex.Replace(hair.Tags[hair.Tags.Length - 2], @"[\d-]", string.Empty);
                }
            }

            if (race == Race.Zombie)
            {
                var woundSprites = GetSprites(SpriteLayer.Wound, Race.Zombie, Gender.Male).ToList();
                if (woundSprites.Count > 0)
                {
                    character.AddLayer(woundSprites[RandomHelper.Random.Next(0, woundSprites.Count)]);
                    if (RandomHelper.Random.Next(0, 10) > 7)
                    {
                        character.AddLayer(woundSprites[RandomHelper.Random.Next(0, woundSprites.Count)]);
                    }
                }
            }

            if (gender == Gender.Male)
            {
                if (RandomHelper.Random.Next(0, 10) > 3)
                {
                    var facialHairSprites = GetSprites(SpriteLayer.Facial, Race.Any, Gender.Male)
                            .Where(t => t.Tags.Contains("male") && !t.Tags.Contains("Mask")).ToList();

                    if (!string.IsNullOrEmpty(hairTone))
                    {
                        facialHairSprites = facialHairSprites.Where(t => t.Tags.Contains(hairTone)).ToList();
                    }

                    if (facialHairSprites.Count > 0)
                    {
                        character.AddLayer(facialHairSprites[RandomHelper.Random.Next(0, facialHairSprites.Count)]);
                    }
                }
            }

            return character;
        }

        private List<ISpriteSheet> GetBodyPartSprite(SpriteLayer layer, Gender gender, Race race, string tone)
        {
            var sprites = GetSprites(layer, race, gender).ToList();
            if (!string.IsNullOrEmpty(tone))
            {
                sprites = sprites.Where(s => s.Tags.Contains(tone)).ToList();
            }

            return sprites;
        }

        public bool FilterValid(string file, string filter)
        {
            return Regex.IsMatch(file, filter);
        }

        public ICharacterSpriteDefinition GetRandomCharacterSprite(Race race)
        {
            var character = GetBaseCharacter(RandomHelper.Random.Next(10) > 5 ? Gender.Male : Gender.Female, race);

            AddClothes(character);

            var rand = RandomHelper.Random.Next(0, 100);
            if (rand > 66)
            {
                AddArmor(character, ArmorType.plate);
            }
            else if (rand > 33)
            {
                AddArmor(character, ArmorType.leather);
            }
            else
            {
                AddArmor(character, ArmorType.chain);
            }

            AddWeapons(character);

            return character;
        }

        public IEnumerable<ISpriteSheet> GetSprites(SpriteLayer layer, Race race, Gender gender)
        {
            var sprites = SpriteLibrary.Where(s => s.SpriteLayer == layer && (s.Gender == gender || s.Gender == Gender.Either));

            if (layer == SpriteLayer.Body)
            {
                return sprites.Where(s => s.Race == race);
            }
            else
            {
                return sprites.Where(s => s.Race == race || s.Race == Race.Any);
            }
        }

        private Gender GetGender(string fileName)
        {
            if (fileName.ToLower().Contains("female") || fileName.ToLower().Contains("woman"))
            {
                return Gender.Female;
            }
            if (fileName.ToLower().Contains("male") || fileName.ToLower().Contains("man"))
            {
                return Gender.Male;
            }

            return Gender.Either;
        }

        private Race GetRace(string fileName)
        {
            var human = new[] { "dark.png", "dark2.png", "woman-", "man-", "tanned.png", "light.png", "tanned2.png" };

            var darkElf = new[] { "darkelf.png", "darkelf2.png" };
            var orc = new[] { "orcs" };
            var elf = new[] { "elf", "elve" };
            var reptile = new[] { ".Drake_" };
            var skeleton = new[] { "24.png", "25.png" };
            var zombie = new[] { "zombie" };

            if (darkElf.Any(h => fileName.Contains(h)))
            {
                return Race.DarkElf;
            }
            else if (orc.Any(h => fileName.Contains(h)))
            {
                return Race.Orc;
            }
            else if (reptile.Any(h => fileName.Contains(h)))
            {
                return Race.Reptile;
            }
            else if (skeleton.Any(h => fileName.Contains(h)))
            {
                return Race.Skeleton;
            }
            else if (zombie.Any(h => fileName.Contains(h)))
            {
                return Race.Zombie;
            }
            else if (elf.Any(h => fileName.Contains(h)))
            {
                return Race.Elf;
            }
            else if (human.Any(h => fileName.Contains(h)))
            {
                return Race.Human;
            }
            return Race.Any;
        }

        private List<ISpriteSheet> GetSprites(string path, SpriteLayer layer, SearchOption option = SearchOption.AllDirectories, string filterRegex = ".*")
        {
            var sheets = new List<ISpriteSheet>();

            var files = ResourceManager.GetSprites(path, option);

            foreach (var file in files)
            {
                var name = Path.GetFileNameWithoutExtension(file);

                if (FilterValid(name, filterRegex))
                {
                    sheets.Add(new SpriteSheet(name, file, ResourceManager.GetImageStream(file), GetGender(file), GetRace(file), layer));
                }
            }

            return sheets;
        }
    }
}