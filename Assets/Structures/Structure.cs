using System.Linq;
using UnityEngine;

public class Structure : MonoBehaviour
{
    internal StructureData Data = new StructureData();
    internal SpriteRenderer SpriteRenderer;
    internal SpriteRenderer StatusSprite;

    public void LoadSprite()
    {
        SpriteRenderer.sprite = Game.SpriteStore.GetSpriteByName(Data.SpriteName);
        SetTiledMode(SpriteRenderer, Data.Tiled);
    }

    public void SetTiledMode(SpriteRenderer spriteRenderer, bool tiled)
    {
        if (!tiled)
        {
            spriteRenderer.drawMode = SpriteDrawMode.Simple;
        }
        else
        {
            spriteRenderer.drawMode = SpriteDrawMode.Tiled;
            spriteRenderer.size = Vector2.one;
        }

        spriteRenderer.transform.localScale = new Vector3(Data.Width, Data.Height, 1);
    }

    internal void Load(string structureData)
    {
        Data = StructureData.GetFromJson(structureData);
        LoadSprite();
    }

    internal void Shift()
    {
        if (!string.IsNullOrEmpty(Data.ShiftX))
        {
            float shiftX = Helpers.GetValueFromFloatRange(Data.ShiftX);
            transform.position += new Vector3(shiftX, 0, 0);
        }

        if (!string.IsNullOrEmpty(Data.ShiftY))
        {
            float shiftY = Helpers.GetValueFromFloatRange(Data.ShiftY);
            transform.position += new Vector3(0, shiftY, 0);
        }

        if (Data.Width > 1 || Data.Height > 1)
        {
            transform.position += new Vector3((Data.Width / 2) - 0.5f, (Data.Height / 2) - 0.5f, 0);
        }
    }

    private void Awake()
    {
        SpriteRenderer = GetComponent<SpriteRenderer>();

        StatusSprite = transform.Find("Status").GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (Game.TimeManager.Paused) return;

        if (!Data.IsBluePrint && Data.Behaviour != null)
        {
            Data.Behaviour.Originator = Data.GetGameId();
            Data.Behaviour.Done();
        }

        if (Data.IsBluePrint && !Data.Faction.Tasks.OfType<Build>().Any(t => t.Structure == Data))
        {
            Data.Faction.AddTask(new Build(Data, Data.Coordinates), Data.GetGameId());
        }
    }
}
