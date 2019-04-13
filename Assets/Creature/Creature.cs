using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;


public class Creature : MonoBehaviour
{
    internal Sprite[] BackSprites;
    internal CreatureData Data = new CreatureData();
    internal Sprite[] FrontSprites;
    internal SpriteRenderer Highlight;
    internal float RemainingTextDuration;
    internal Sprite[] SideSprites;
    internal SpriteRenderer SpriteRenderer;
    internal TextMeshPro Text;

    private float deltaTime = 0;

    private int frame;

    private float frameSeconds = 0.3f;

    public void Awake()
    {
        Text = transform.Find("Text").GetComponent<TextMeshPro>();
        Highlight = transform.Find("Highlight").GetComponent<SpriteRenderer>();

        Highlight.gameObject.SetActive(false);
    }

    public void FaceRandomDirection()
    {
        var values = Enum.GetValues(typeof(Direction));
        Data.MoveDirection = (Direction)values.GetValue(Random.Range(0, values.Length));
    }

    public void ShowText(string text, float duration)
    {
        Text.text = text;
        Text.color = Color.white;

        RemainingTextDuration = duration + 1f;
    }

    public void Update()
    {
        if (Game.TimeManager.Paused) return;

        Work();

        UpdateSelf();
    }

    internal void DisableHightlight()
    {
        Highlight.gameObject.SetActive(false);
    }

    internal void EnableHighlight(Color color)
    {
        Highlight.color = color;
        Highlight.gameObject.SetActive(true);
    }

    internal void GetSprite()
    {
        SpriteRenderer = GetComponent<SpriteRenderer>();

        var sprites = Game.SpriteStore.CreatureSprite[Data.SpriteId];
        BackSprites = sprites.Where(s => s.name.StartsWith("all_back", StringComparison.InvariantCultureIgnoreCase)).ToArray();
        FrontSprites = sprites.Where(s => s.name.StartsWith("all_front", StringComparison.InvariantCultureIgnoreCase)).ToArray();
        SideSprites = sprites.Where(s => s.name.StartsWith("all_side", StringComparison.InvariantCultureIgnoreCase)).ToArray();

        Animate(true);
    }

    private void Animate(bool force = false)
    {
        if (!Data.Animate && !force)
        {
            return;
        }

        Sprite[] sprites;
        switch (Data.MoveDirection)
        {
            case Direction.N:
                sprites = BackSprites;
                break;

            case Direction.SE:
            case Direction.NE:
            case Direction.E:
                sprites = SideSprites;
                SpriteRenderer.flipX = true;
                break;

            case Direction.S:
                sprites = FrontSprites;
                break;
            //case Direction.NW:
            //case Direction.SW:
            //case Direction.W:
            default:
                sprites = SideSprites;
                SpriteRenderer.flipX = false;
                break;
        }

        deltaTime += Time.deltaTime;

        if (deltaTime > frameSeconds || force)
        {
            deltaTime = 0;
            frame++;
            if (frame >= sprites.Length)
            {
                frame = 0;
            }
            SpriteRenderer.sprite = sprites[frame];
        }
    }

    private void CarryItem()
    {
        if (Data.CarriedItemId > 0)
        {
            var item = Game.ItemController.ItemDataLookup[Data.CarriedItem];

            item.transform.position = transform.position;
            item.SpriteRenderer.sortingLayerName = LayerConstants.CarriedItem;
        }
    }

    private void UpdateFloatingText()
    {
        if (RemainingTextDuration > 0)
        {
            RemainingTextDuration -= Time.deltaTime;

            if (RemainingTextDuration < 1f)
            {
                Text.color = new Color(Text.color.r, Text.color.g, Text.color.b, RemainingTextDuration);
            }
        }
        else
        {
            Text.text = "";
        }
    }

    private void UpdateSelf()
    {
        Data.InternalTick += Time.deltaTime;

        var thoughts = new List<string>();
        if (Data.InternalTick >= Game.TimeManager.TickInterval)
        {
            Data.InternalTick = 0;

            if (Random.value > 0.65)
            {
                Data.Task?.ShowBusyEmote();
            }

            Data.Hunger += Random.value;
            Data.Thirst += Random.value;
            Data.Energy -= Random.Range(0.1f,0.25f);
        }

        if (thoughts.Count > 0 && Random.value > 0.9)
        {
            ShowText(thoughts[Random.Range(0, thoughts.Count - 1)], 2f);
        }

        CarryItem();
        UpdateFloatingText();

        Animate();
    }

    private void Work()
    {
        if (Data.Task == null)
        {
            var task = Game.Taskmaster.GetTask(this);
            var context = $"{Data.GetGameId()} - {task} - {Game.TimeManager.Now}";

            Data.Know(context);
            task.Context = context;

            Taskmaster.AssignTask(Data, task);
            Data.Task = task;
        }
        else
        {
            try
            {
                Taskmaster.AssignTask(Data, Data.Task);

                if (!Data.Task.Done())
                {
                    Data.Task.Update();
                }
                else
                {
                    Data.Task.ShowDoneEmote();
                    Data.FreeResources(Data.Task.Context);
                    Data.Forget(Data.Task.Context);

                    Game.Taskmaster.TaskComplete(Data.Task);
                    Data.Task = null;
                }
            }
            catch (TaskFailedException ex)
            {
                Debug.LogWarning($"Task failed: {ex}");
                Game.Taskmaster.TaskFailed(Data.Task, ex.Message);
            }
        }
    }
}

public class CreatureData
{
    public const string SelfKey = "Self";
    public int CarriedItemId;
    public Coordinates Coordinates;
    public float Energy;
    public float Hunger;
    public int Id;
    public Dictionary<string, Memory> Mind = new Dictionary<string, Memory>();

    public Direction MoveDirection = Direction.S;

    public string Name;

    public float Speed = 10f;

    public int SpriteId;

    public float Thirst;

    internal float InternalTick;

    public bool Animate = true;

    [JsonIgnore]
    public ItemData CarriedItem
    {
        get
        {
            if (Game.ItemController.ItemIdLookup.ContainsKey(CarriedItemId))
            {
                return Game.ItemController.ItemIdLookup[CarriedItemId].Data;
            }
            return null;
        }
    }

    [JsonIgnore]
    public CellData CurrentCell
    {
        get
        {
            return Game.MapGrid.GetCellAtCoordinate(Coordinates);
        }
    }

    [JsonIgnore]
    public Creature LinkedGameObject
    {
        get
        {
            return Game.CreatureController.GetCreatureForCreatureData(this);
        }
    }

    [JsonIgnore]
    public Memory Self
    {
        get
        {
            if (!Mind.ContainsKey(SelfKey))
            {
                Mind.Add(SelfKey, new Memory());
            }

            return Mind[SelfKey];
        }
    }
    [JsonIgnore]
    public TaskBase Task { get; set; }

    internal ItemData DropItem(Coordinates coordinates = null)
    {
        if (CarriedItemId > 0)
        {
            var item = CarriedItem;
            item.Reserved = false;
            item.LinkedGameObject.SpriteRenderer.sortingLayerName = LayerConstants.Item;

            if (coordinates != null)
            {
                var cell = Game.MapGrid.GetCellAtCoordinate(coordinates);
                if (CurrentCell.Neighbors.Contains(cell))
                {
                    cell.AddContent(item.LinkedGameObject.gameObject);
                }
                else
                {
                    CurrentCell.AddContent(item.LinkedGameObject.gameObject);
                }
            }
            else
            {
                CurrentCell.AddContent(item.LinkedGameObject.gameObject);
            }

            CarriedItemId = 0;
            return item;
        }

        return null;
    }

    internal void Forget(string context)
    {
        // Debug.Log($"Forget context: {context}");
        // if !LongTerm?
        Mind.Remove(context);
    }

    internal void FreeResources(string context)
    {
        if (!Mind.ContainsKey(context))
        {
            // already forgot about this context, do nothing
            return;
        }

        // see if character remembers any structures used in this current task context
        // if any exist and they were reserved by this creature, free them
        if (Mind[context].ContainsKey(MemoryType.Structure))
        {
            foreach (var structureId in Mind[context][MemoryType.Structure])
            {
                var structure = IdService.GetStructureFromId(structureId);
                if (structure.InUseBy == this.GetGameId())
                {
                    structure.Free();
                }
            }
        }
    }

    internal void Know(string context)
    {
        // Debug.Log($"Add context: {context}");
        Mind.Add(context, new Memory());
    }

    internal void UpdateMemory(string context, MemoryType memoryType, string info)
    {
        Debug.Log($"Remember: {context}, {memoryType}: '{info}'");
        Mind[context].AddInfo(memoryType, info);
    }

    internal void UpdateSelfMemory( MemoryType memoryType, string info)
    {
        UpdateMemory(SelfKey, memoryType, info);
    }
}
