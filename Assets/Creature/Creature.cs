using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public interface ICreatureSprite
{
    void Update();

    Sprite GetIcon();
}

public class ModularSprite : ICreatureSprite
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

    public enum SortOrder
    {
        Botton = 1,
        Mid = 5,
        Top = 10
    }

    public ModularSprite(Creature creature)
    {
        Head = CreateBodypart("Head", creature, 0, 0);
        Hair = CreateBodypart("Hair", creature, 0, 0.35f);
        Face = CreateBodypart("Face", creature, 0, 0, SortOrder.Mid);
        Neck = CreateBodypart("Neck", creature, 0, -1f);

        LeftArm = CreateBodypart("LeftArm", creature, 1, -1.55f);
        RightArm = CreateBodypart("RightArm", creature, -1, -1.55f);
        LeftSleeve = CreateBodypart("LeftSleeve", creature, 1, -1.55f, SortOrder.Mid);
        RightSleeve = CreateBodypart("RightSleeve", creature, -1, -1.55f, SortOrder.Mid);

        LeftHand = CreateBodypart("LeftHand", creature, 1.75f, -2.3f);
        RightHand = CreateBodypart("RightHand", creature, -1.75f, -2.3f);

        Torso = CreateBodypart("Torso", creature, 0, -1.75f, SortOrder.Top);
        Pelvis = CreateBodypart("Pelvis", creature, 0, -2.75f, SortOrder.Top);

        LeftLeg = CreateBodypart("LeftLeg", creature, 0.55f, -3.55f);
        RightLeg = CreateBodypart("RightLeg", creature, -0.55f, -3.55f);
        LeftPant = CreateBodypart("LeftPant", creature, 0.55f, -3.55f, SortOrder.Mid);
        RightPant = CreateBodypart("RightPant", creature, -0.55f, -3.55f, SortOrder.Mid);

        LeftFoot = CreateBodypart("LeftFoot", creature, 0.8f, -4.45f);
        RightFoot = CreateBodypart("RightFoot", creature, -0.8f, -4.45f);

        RightArm.flipX = true;
        RightSleeve.flipX = true;
        RightHand.flipX = true;
        RightLeg.flipX = true;
        RightPant.flipX = true;

        Head.sprite = Game.SpriteStore.HeadSprites.GetRandomItem();
        Face.sprite = Game.SpriteStore.FaceSprites.GetRandomItem();
        Hair.sprite = Game.SpriteStore.HairSprites.GetRandomItem();
        Neck.sprite = Game.SpriteStore.NeckSprites.GetRandomItem();

        LeftArm.sprite = Game.SpriteStore.ArmSprites.GetRandomItem();
        RightArm.sprite = LeftArm.sprite;
        LeftHand.sprite = Game.SpriteStore.HandSprites.GetRandomItem();
        RightHand.sprite = LeftHand.sprite;
        LeftLeg.sprite = Game.SpriteStore.LegSprites.GetRandomItem();
        RightLeg.sprite = LeftLeg.sprite;

        Torso.sprite = Game.SpriteStore.TorsoSprites.GetRandomItem();
        LeftSleeve.sprite = Game.SpriteStore.SleeveSprites.GetRandomItem();
        RightSleeve.sprite = LeftSleeve.sprite;

        Pelvis.sprite = Game.SpriteStore.PelvisSprites.GetRandomItem();
        LeftPant.sprite = Game.SpriteStore.PantSprites.GetRandomItem();
        RightPant.sprite = LeftPant.sprite;

        LeftFoot.sprite = Game.SpriteStore.FootSprites.GetRandomItem();
        RightFoot.sprite = LeftFoot.sprite;

        // tint clothes groups with random colors to match them more evently
        RightSleeve.color = LeftSleeve.color = Torso.color = ColorExtensions.GetRandomColor();
        RightPant.color = LeftPant.color = Pelvis.color = ColorExtensions.GetRandomColor();
        RightFoot.color = LeftFoot.color = ColorExtensions.GetRandomColor();
    }

    private SpriteRenderer CreateBodypart(string name, Creature creature, float x, float y, SortOrder sortorder = SortOrder.Botton)
    {
        var sr = GameObject.Instantiate(creature.BodyPartPrefab, creature.Body.transform);
        sr.name = name;

        sr.transform.localPosition = new Vector2(x, y);
        sr.sortingOrder = (int)sortorder;

        return sr;
    }

    public void Update()
    {
        //throw new System.NotImplementedException();
    }

    internal Sprite IconSprite;

    public Sprite GetIcon()
    {
        if (IconSprite == null)
        {
            var headHeight = (int)Head.sprite.textureRect.width;
            var headWidth = (int)Head.sprite.textureRect.height;

            var iconTex = TextureHelpers.GetSolidTexture(headHeight, headWidth, new Color(0, 0, 0, 0))
                                        .Combine(Head.sprite)
                                        .Combine(Hair.sprite, new Vector2(headWidth * 0.1f, headHeight * 0.40f))
                                        .Combine(Face.sprite, new Vector2(headWidth * 0.25f, headHeight * 0.25f));

            IconSprite = Sprite.Create(iconTex, new Rect(0, 0, headHeight, headWidth), new Vector2());
        }

        return IconSprite;
    }
}

public class Creature : MonoBehaviour
{
    internal CreatureData Data = new CreatureData();
    internal SpriteRenderer Highlight;
    internal float RemainingTextDuration;
    internal TextMeshPro Text;

    private float deltaTime = 0;

    private float frameSeconds = 0.3f;

    internal ICreatureSprite CreatureSprite;

    internal GameObject Body;

    public SpriteRenderer BodyPartPrefab;

    public void Awake()
    {
        Text = transform.Find("Text").GetComponent<TextMeshPro>();
        Highlight = transform.Find("Highlight").GetComponent<SpriteRenderer>();

        Highlight.gameObject.SetActive(false);
        Body = transform.Find("Body").gameObject;

        CreatureSprite = new ModularSprite(this);
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

    private void Animate(bool force = false)
    {
        CreatureSprite.Update();

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