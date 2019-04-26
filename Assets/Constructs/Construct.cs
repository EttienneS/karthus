using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Construct
{
    public string Floor;
    public Dictionary<char, string> Key;
    public string Name;
    public List<string> Plan;

    [JsonIgnore]
    private Texture2D _texture;

    [JsonIgnore]
    public int Height
    {
        get
        {
            return Plan.Count;
        }
    }

    private Sprite _sprite;

    [JsonIgnore]
    public Sprite Sprite
    {
        get
        {
            if (_sprite == null)
            {
                _sprite = Sprite.Create(Texture,
                                        new Rect(0, 0, Texture.width, Texture.height),
                                        new Vector2(0.5f, 0.5f), MapConstants.PixelsPerCell);
            }
            return _sprite;
        }
    }

    [JsonIgnore]
    public int Width
    {
        get
        {
            return Plan[0].Length;
        }
    }

    [JsonIgnore]
    internal Texture2D Texture
    {
        get
        {
            if (_texture == null)
            {
                _texture = new Texture2D(Width * MapConstants.PixelsPerCell, Height * MapConstants.PixelsPerCell);

                var y = 0;
                var x = 0;

                // easier to just flip the plan order than to invert the drawing
                var flippedPlan = Plan.ToList();
                flippedPlan.Reverse();

                foreach (var line in flippedPlan)
                {
                    foreach (var character in line)
                    {
                        var startX = x * MapConstants.PixelsPerCell;
                        var startY = y * MapConstants.PixelsPerCell;

                        var sourceTexture = Game.SpriteStore.GetSpriteByName(GetStructure(character)).texture;
                        var constructTexture = sourceTexture.Clone();
                        constructTexture.ScaleToGridSize(1, 1);

                        for (var subTexX = 0; subTexX < MapConstants.PixelsPerCell; subTexX++)
                        {
                            for (var subTexY = 0; subTexY < MapConstants.PixelsPerCell; subTexY++)
                            {
                                var pixel = constructTexture.GetPixel(subTexX, subTexY);
                                _texture.SetPixel(startX + subTexX,
                                                 startY + subTexY,
                                                 pixel);
                            }
                        }

                        x++;
                    }
                    y++;
                }
                _texture.Apply();
            }

            return _texture;
        }
    }

    public string GetStructure(char character)
    {
        string structureName = Floor;
        if (character != ' ')
        {
            structureName = Key[character];
        }

        return structureName;
    }

    internal bool ValidateStartPos(CellData cellData)
    {
        var x = 0;
        var y = 0;
        foreach (var line in Plan)
        {
            foreach (var character in line)
            {
                var coorinate = new Coordinates(cellData.Coordinates.X + x, cellData.Coordinates.Y + y);

                if (!Game.MapGrid.GetCellAtCoordinate(coorinate).Buildable)
                {
                    Debug.Log($"not buildable {coorinate}");
                    return false;
                }

                x++;
            }
            y++;
        }
        return true;
    }

    internal bool Place(CellData cellData)
    {
        var x = 0;
        var y = 0;
        foreach (var line in Plan)
        {
            foreach (var character in line)
            {
                var coorinate = new Coordinates(cellData.Coordinates.X + x, cellData.Coordinates.Y + y);
                var blueprint = Game.StructureController.GetStructureBluePrint(GetStructure(character));

                Game.MapGrid
                    .GetCellAtCoordinate(coorinate)
                    .AddContent(blueprint.gameObject);

                //Game.Taskmaster.AddTask(new Build(blueprint.Data, coorinate), string.Empty);
                x++;
            }
            y++;
        }
        return true;
    }
}