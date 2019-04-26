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
    private List<string> _currentPlan;

    [JsonIgnore]
    public List<string> CurrentPlan
    {
        get
        {
            if (_currentPlan == null)
            {
                _currentPlan = Plan.ToList();
            }

            return _currentPlan;
        }
        set
        {
            _currentPlan = value;
        }
    }

    [JsonIgnore]
    public List<string> FlippedPlan
    {
        get
        {
            var flipped = CurrentPlan.ToList();
            flipped.Reverse();

            return flipped;
        }
    }

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
                _texture = GetTexture();
            }

            return _texture;
        }
    }

    internal Texture2D GetTexture()
    {
        var texture = new Texture2D(Width * MapConstants.PixelsPerCell, Height * MapConstants.PixelsPerCell);

        var y = 0;
        var x = 0;

        foreach (var line in FlippedPlan)
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
                        texture.SetPixel(startX + subTexX,
                                         startY + subTexY,
                                         pixel);
                    }
                }

                x++;
            }
            y++;
        }
        texture.Apply();

        return texture;
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
        foreach (var line in FlippedPlan)
        {
            foreach (var character in line)
            {
                var coorinate = new Coordinates(cellData.Coordinates.X + x, cellData.Coordinates.Y + y);
                var cell = Game.MapGrid.GetCellAtCoordinate(coorinate);

                if (cell.Pathable)
                {
                    if (cell.Structure != null && !cell.Structure.Name.Equals(GetStructure(character)))
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }

                x++;
            }
            x = 0;
            y++;
        }
        return true;
    }

    internal bool Place(CellData cellData)
    {
        var x = 0;
        var y = 0;
        foreach (var line in FlippedPlan)
        {
            foreach (var character in line)
            {
                var cell = Game.MapGrid.GetCellAtCoordinate(new Coordinates(cellData.Coordinates.X + x, cellData.Coordinates.Y + y));

                if (cell.Pathable && cell.Structure == null)
                {
                    var blueprint = Game.StructureController.GetStructureBluePrint(GetStructure(character));
                    cell.AddContent(blueprint.gameObject);
                }

                x++;
            }
            x = 0;
            y++;
        }
        return true;
    }

    public void RotateLeft()
    {
        RotateRight();
        RotateRight();
        RotateRight();
    }

    public void RotateRight()
    {
        var current = new char[Width, Height];
        var x = 0;
        var y = 0;

        foreach (var line in CurrentPlan)
        {
            foreach (var character in line)
            {
                current[x++, y] = character;
            }
            x = 0;
            y++;
        }

        var newArray = new char[Width, Height];

        for (int width = Width - 1; width >= 0; width--)
        {
            for (int height = 0; height < Height; height++)
            {
                newArray[height, Width - width - 1] = current[width, height];
            }
        }

        var newPlan = new List<string>();
        for (y = 0; y < Height; y++)
        {
            var line = string.Empty;
            for (x = 0; x < Width; x++)
            {
                line += newArray[x, y];
            }
            newPlan.Add(line);
        }

        CurrentPlan = newPlan;
    }
}