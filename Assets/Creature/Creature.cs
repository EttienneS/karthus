using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public enum MemoryType
{
    Item, Craft, Location, Creature, Stockpile, Structure
}

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

    public void AssignTask(TaskBase task, string originator = "")
    {
        task.AssignedCreatureId = Data.Id;

        if (!string.IsNullOrEmpty(originator))
        {
            task.Originator = originator;
        }

        if (task.SubTasks != null)
        {
            foreach (var subTask in task.SubTasks.ToList())
            {
                subTask.Context = task.Context;
                AssignTask(subTask, task.Originator);
            }
        }
    }

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
        if (TimeManager.Instance.Paused) return;

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

        var sprites = SpriteStore.Instance.CreatureSprite[Data.SpriteId];
        BackSprites = sprites.Where(s => s.name.StartsWith("all_back", StringComparison.InvariantCultureIgnoreCase)).ToArray();
        FrontSprites = sprites.Where(s => s.name.StartsWith("all_front", StringComparison.InvariantCultureIgnoreCase)).ToArray();
        SideSprites = sprites.Where(s => s.name.StartsWith("all_side", StringComparison.InvariantCultureIgnoreCase)).ToArray();

        Animate(true);
    }

    private void Animate(bool force = false)
    {
        if (Data.Sleeping)
        {
            SpriteRenderer.sprite = FrontSprites[0];
            SpriteRenderer.flipY = true;
            return;
        }
        else
        {
            SpriteRenderer.flipY = false;
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
            var item = ItemController.Instance.ItemDataLookup[Data.CarriedItem];

            item.transform.position = transform.position;
            item.SpriteRenderer.sortingLayerName = "CarriedItem";
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
        if (Data.InternalTick >= TimeManager.Instance.TickInterval)
        {
            Data.InternalTick = 0;

            if (!Data.Sleeping)
            {
                Data.Hunger += Random.value;
                Data.Thirst += Random.value;
                Data.Energy -= Random.value;

                if (Data.Hunger > 40)
                {
                    thoughts.Add("Jaassss ek kan gaan vir 'n boerie!");
                }

                if (Data.Energy < 30)
                {
                    thoughts.Add("*Yawn..*");
                }
            }
            else
            {
                Data.Hunger += Random.value / 2f;
                Data.Thirst += Random.value / 2f;
                Data.Energy += Random.value * 1.2f;
            }
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
            var task = Taskmaster.Instance.GetTask(this);
            var context = $"{Data.GetGameId()} - {task} - {TimeManager.Instance.Now}";

            Data.Know(context);
            task.Context = context;

            AssignTask(task);
            Data.Task = task;
        }

        try
        {
            if (!Data.Task.Done())
            {
                Data.Task.Update();
            }
            else
            {
                Data.FreeResources(Data.Task.Context);

                Data.Forget(Data.Task.Context);

                Taskmaster.Instance.TaskComplete(Data.Task);
                Data.Task = null;
            }
        }
        catch (CancelTaskException)
        {
            Taskmaster.Instance.TaskComplete(Data.Task);

            if (Data.CarriedItemId > 0)
            {
                Data.CarriedItem.Reserved = false;
                Data.CarriedItemId = 0;
            }

            Data.Task = Taskmaster.Instance.GetTask(this);
            AssignTask(Data.Task, Data.Task.Context);
        }
    }
}

public class CreatureData
{
    public int CarriedItemId;
    public Coordinates Coordinates;
    public float Energy;
    public float Hunger;
    public int Id;

    public Dictionary<string, Memory> Mind = new Dictionary<string, Memory>();
    public Direction MoveDirection = Direction.S;
    public string Name;

    public bool Sleeping;
    public float Speed = 10f;

    public int SpriteId;

    public float Thirst;

    internal float InternalTick;

    [JsonIgnore]
    public ItemData CarriedItem
    {
        get
        {
            if (ItemController.Instance.ItemIdLookup.ContainsKey(CarriedItemId))
            {
                return ItemController.Instance.ItemIdLookup[CarriedItemId].Data;
            }
            return null;
        }
    }

    [JsonIgnore]
    public CellData CurrentCell
    {
        get
        {
            return MapGrid.Instance.GetCellAtCoordinate(Coordinates);
        }
    }

    [JsonIgnore]
    public Creature LinkedGameObject
    {
        get
        {
            return CreatureController.Instance.GetCreatureForCreatureData(this);
        }
    }

    [JsonIgnore]
    public TaskBase Task { get; set; }

    internal ItemData DropItem()
    {
        if (CarriedItemId > 0)
        {
            var item = CarriedItem;
            item.Reserved = false;
            item.LinkedGameObject.SpriteRenderer.sortingLayerName = "Item";
            CurrentCell.AddContent(item.LinkedGameObject.gameObject);

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

    internal void UpdateMemory(string context, MemoryType craft, string info)
    {
        Debug.Log($"Remember: {context}, {craft}: '{info}'");
        Mind[context].AddInfo(craft, info);
    }
}

public class Memory : Dictionary<MemoryType, List<string>>
{
    public string AddInfo(MemoryType type, string entry)
    {
        if (!ContainsKey(type))
        {
            Add(type, new List<string>());
        }

        this[type].Add(entry);

        return entry;
    }
}