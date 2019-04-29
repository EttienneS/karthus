using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class Creature : MonoBehaviour
{
    public SpriteRenderer Face;
    public SpriteRenderer Hair;
    public SpriteRenderer Head;
    public SpriteRenderer Neck;
    public SpriteRenderer Torso;
    public SpriteRenderer LeftArm;
    public SpriteRenderer RightArm;
    public SpriteRenderer LeftSleeve;
    public SpriteRenderer RightSleeve;
    public SpriteRenderer LeftHand;
    public SpriteRenderer RightHand;
    public SpriteRenderer Pelvis;
    public SpriteRenderer LeftLeg;
    public SpriteRenderer RightLeg;
    public SpriteRenderer LeftPant;
    public SpriteRenderer RightPant;
    public SpriteRenderer LeftFoot;
    public SpriteRenderer RightFoot;

    internal CreatureData Data = new CreatureData();
    internal SpriteRenderer Highlight;
    internal float RemainingTextDuration;
    internal TextMeshPro Text;

    private float deltaTime = 0;
    
    private float frameSeconds = 0.3f;

    public void Awake()
    {
        Text = transform.Find("Text").GetComponent<TextMeshPro>();
        Highlight = transform.Find("Highlight").GetComponent<SpriteRenderer>();

        Highlight.gameObject.SetActive(false);
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
        Head.sprite = Game.SpriteStore.HeadSprites.GetRandomItem();
        Face.sprite = Game.SpriteStore.FaceSprites.GetRandomItem();
        Hair.sprite = Game.SpriteStore.HairSprites.GetRandomItem();
        Neck.sprite = Game.SpriteStore.NeckSprites.GetRandomItem();

        LeftArm.sprite = Game.SpriteStore.ArmSprites.GetRandomItem();
        RightArm.sprite = Game.SpriteStore.ArmSprites.GetRandomItem();
        LeftHand.sprite = Game.SpriteStore.HandSprites.GetRandomItem();
        RightHand.sprite = Game.SpriteStore.HandSprites.GetRandomItem();
        LeftLeg.sprite = Game.SpriteStore.LegSprites.GetRandomItem();
        RightLeg.sprite = Game.SpriteStore.LegSprites.GetRandomItem();

        Torso.sprite = Game.SpriteStore.TorsoSprites.GetRandomItem();
        LeftSleeve.sprite = Game.SpriteStore.SleeveSprites.GetRandomItem();
        RightSleeve.sprite = Game.SpriteStore.SleeveSprites.GetRandomItem();

        Pelvis.sprite = Game.SpriteStore.PelvisSprites.GetRandomItem();
        LeftPant.sprite = Game.SpriteStore.PantSprites.GetRandomItem();
        RightPant.sprite = Game.SpriteStore.PantSprites.GetRandomItem();

        LeftFoot.sprite = Game.SpriteStore.FootSprites.GetRandomItem();
        RightFoot.sprite = Game.SpriteStore.FootSprites.GetRandomItem();

        //var tint = Color.blue;
        //Torso.color = tint;
        //LeftSleeve.color = tint;
        //RightSleeve.color = tint;

        //var tint2 = Color.red;
        //Pelvis.color = tint2;
        //LeftPant.color = tint2;
        //RightPant.color = tint2;

        //var tint3 = Color.green;
        //LeftFoot.color = tint3;
        //RightFoot.color = tint3;


        Animate(true);
    }

    private void Animate(bool force = false)
    {
        if (!Data.Animate && !force)
        {
            return;
        }

        deltaTime += Time.deltaTime;

        if (deltaTime > frameSeconds || force)
        {
            deltaTime = 0;
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
            Data.Energy -= Random.Range(0.1f, 0.25f);
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

    public string SpriteId;

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
        // Debug.Log($"Remember: {context}, {memoryType}: '{info}'");
        Mind[context].AddInfo(memoryType, info);
    }

    internal void UpdateSelfMemory(MemoryType memoryType, string info)
    {
        UpdateMemory(SelfKey, memoryType, info);
    }
}