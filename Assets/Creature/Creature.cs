using Newtonsoft.Json;
using System;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Creature : MonoBehaviour
{
    internal Sprite[] BackSprites;
    internal CreatureData Data = new CreatureData();
    internal Sprite[] FrontSprites;
    internal Sprite[] SideSprites;
    internal SpriteRenderer SpriteRenderer;

    private float deltaTime = 0;

    private int frame;

    private float frameSeconds = 0.3f;

    public void AssignTask(TaskBase task)
    {
        task.Creature = Data;

        if (task.SubTasks != null)
        {
            foreach (var subTask in task.SubTasks.ToList())
            {
                AssignTask(subTask);
            }
        }
    }

    public void FaceRandomDirection()
    {
        var values = Enum.GetValues(typeof(Direction));
        Data.MoveDirection = (Direction)values.GetValue(Random.Range(0, values.Length));
    }

    public void Start()
    {
        SpriteRenderer = GetComponent<SpriteRenderer>();

        Data.Hunger = Random.Range(0, 15);
        Data.Thirst = Random.Range(0, 15);
        Data.Energy = Random.Range(80, 100);

        GetSprite();
    }

    public void Update()
    {
        if (TimeManager.Instance.Paused) return;

        AnimateSprite();

        // something in the serialization makes the item object
        // revert when called, so we just clear it here to fix the issue
        if (Data.CarriedItem != null && string.IsNullOrEmpty(Data.CarriedItem.Name))
        {
            Data.CarriedItem = null;
        }

        Data.InternalTick += Time.deltaTime;

        if (Data.InternalTick >= TimeManager.Instance.TickInterval)
        {
            Data.InternalTick = 0;

            Data.Hunger += Random.value;
            Data.Thirst += Random.value;
            Data.Energy -= Random.value;
        }

        if (Data.Task == null)
        {
            var task = Taskmaster.Instance.GetTask(this);
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
                Taskmaster.Instance.TaskComplete(Data.Task);
                Data.Task = null;
            }
        }
        catch (CancelTaskException)
        {
            Taskmaster.Instance.TaskComplete(Data.Task);

            if (Data.CarriedItem != null)
            {
                Data.CarriedItem.Reserved = false;
                Data.CarriedItem = null;
            }

            Data.Task = Taskmaster.Instance.GetTask(this);
            AssignTask(Data.Task);
        }

        if (Data.CarriedItem != null)
        {
            var item = ItemController.Instance.ItemDataLookup[Data.CarriedItem];

            item.transform.position = transform.position;
            item.SpriteRenderer.sortingLayerName = "CarriedItem";
        }
    }

    private void AnimateSprite()
    {
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

        if (deltaTime > frameSeconds)
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

    private void GetSprite()
    {
        Data.SpriteId = Random.Range(0, SpriteStore.Instance.CreatureSprite.Keys.Count - 1);

        var sprites = SpriteStore.Instance.CreatureSprite[Data.SpriteId];
        BackSprites = sprites.Where(s => s.name.StartsWith("all_back", StringComparison.InvariantCultureIgnoreCase)).ToArray();
        FrontSprites = sprites.Where(s => s.name.StartsWith("all_front", StringComparison.InvariantCultureIgnoreCase)).ToArray();
        SideSprites = sprites.Where(s => s.name.StartsWith("all_side", StringComparison.InvariantCultureIgnoreCase)).ToArray();
    }
}

[Serializable]
public class CreatureData
{
    public ItemData CarriedItem;

    public Coordinates Coordinates;

    public float Energy;

    public float Hunger;

    public Direction MoveDirection = Direction.S;

    public string Name;

    public float Speed = 5f;

    public int SpriteId;

    public float Thirst;

    internal float InternalTick;

    [JsonIgnore]
    public CellData CurrentCell
    {
        get
        {
            return MapGrid.Instance.GetCellAtCoordinate(Coordinates).Data;
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
}