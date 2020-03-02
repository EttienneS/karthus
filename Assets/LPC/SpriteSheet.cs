using LPC.Spritesheet.Generator.Enums;
using LPC.Spritesheet.Generator.Interfaces;
using System.IO;

namespace LPC.Spritesheet.Generator
{
    public class SpriteSheet : ISpriteSheet
    {
        public SpriteSheet(string displayName, string fileName, Stream stream, Gender gender, Race race, SpriteLayer layer)
        {
            DisplayName = displayName;
            Gender = gender;
            SpriteLayer = layer;
            SpriteData = ReadStream(stream);
            Race = race;
            FullName = fileName;
            Tags = fileName.Split(new[] { '\\', '-', '_', '.', ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
        }

        public string DisplayName { get; set; }
        public Gender Gender { get; set; }
        public Race Race { get; set; }
        public byte[] SpriteData { get; set; }
        public string[] Tags { get; set; }
        public SpriteLayer SpriteLayer { get; set; }
        public string FullName { get; set; }

        public static byte[] ReadStream(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (var ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        public override string ToString()
        {
            return $"{DisplayName} - {SpriteLayer} - {Race} - {Gender}";
        }
    }
}