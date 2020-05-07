using LPC.Spritesheet.Generator;
using LPC.Spritesheet.Generator.Enums;
using Needs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Animation = LPC.Spritesheet.Generator.Interfaces.Animation;
using Random = UnityEngine.Random;

public enum Mobility
{
    Walk, Fly
}

public class Creature : IEntity
{
    public const string SelfKey = "Self";

    public Animation Animation = Animation.Walk;

    public float AnimationDelta = 0f;

    [JsonIgnore]
    public CharacterSpriteSheet CharacterSpriteSheet;

    [JsonIgnore]
    public List<Creature> Combatants = new List<Creature>();

    public Direction Facing = Direction.S;

    public List<Feeling> Feelings = new List<Feeling>();
    public Animation? FixedAnimation = null;

    public int? FixedFrame;

    public Gender Gender;

    [JsonIgnore]
    public Behaviours.GetBehaviourTaskDelegate GetBehaviourTask;

    public List<OffensiveActionBase> IncomingAttacks = new List<OffensiveActionBase>();
    public Mobility Mobility;
    public bool Moving;

    [JsonIgnore]
    public List<Cell> Path = new List<Cell>();

    public Race Race;
    public List<Relationship> Relationships = new List<Relationship>();

    public (float x, float y) TargetCoordinate;

    public bool UnableToFindPath;

    internal int Frame;

    internal float InternalTick = float.MaxValue;

    [JsonIgnore]
    internal Cell LastPercievedCoordinate;

    private const int SelfTickCount = 10;

    [JsonIgnore]
    private List<Cell> _awareness;

    private Faction _faction;

    private int _selfTicks;


    public float Aggression { get; set; }

    [JsonIgnore]
    public List<Cell> Awareness
    {
        get
        {
            if (_awareness == null && Cell != null)
            {
                _awareness = Game.Instance.Map.GetCircle(Cell, Perception);
            }

            return _awareness;
        }
    }

    public string BehaviourName { get; set; }

    [JsonIgnore]
    public Cell Cell
    {
        get
        {
            return Game.Instance.Map.GetCellAtCoordinate(X, Y);
        }
        set
        {
            X = Cell.Vector.x;
            Y = Cell.Vector.y;

            CreatureRenderer?.UpdatePosition();
        }
    }

    [JsonIgnore]
    public CreatureRenderer CreatureRenderer { get; set; }

    public bool Dead { get; internal set; }

    public int Dexterity { get; set; }

    [JsonIgnore]
    public Faction Faction
    {
        get
        {
            if (_faction == null)
            {
                _faction = Game.Instance.FactionController.Factions[FactionName];
            }

            return _faction;
        }
    }

    public string FactionName { get; set; }

    [JsonIgnore]
    public Item HeldItem
    {
        get
        {
            if (string.IsNullOrEmpty(HeldItemId))
            {
                return null;
            }
            return HeldItemId.GetItem();
        }
        set
        {
            if (value != null)
            {
                HeldItemId = value.Id;
            }
            else
            {
                HeldItemId = null;
            }
        }
    }

    public string HeldItemId { get; set; }

    public string Id { get; set; }

    [JsonIgnore]
    public bool InCombat
    {
        get
        {
            return Combatants.Count > 0;
        }
    }

    public List<Limb> Limbs { get; set; }

    public List<VisualEffectData> LinkedVisualEffects { get; set; } = new List<VisualEffectData>();

    public List<string> LogHistory { get; set; }

    [JsonIgnore]
    public int Mood
    {
        get
        {
            return Feelings.Sum(f => f.MoodImpact);
        }
    }

    [JsonIgnore]
    public string MoodString
    {
        get
        {
            if (Mood > 80)
            {
                return "Ecstatic";
            }
            else if (Mood > 50)
            {
                return "Very Happy";
            }
            else if (Mood > 20)
            {
                return "Happy";
            }
            else if (Mood > -20)
            {
                return "Fine";
            }
            else if (Mood > -40)
            {
                return "Sad";
            }
            else if (Mood > -60)
            {
                return "Very Sad";
            }
            else if (Mood > -80)
            {
                return "Terrible";
            }

            return "Fine";
        }
    }

    public string Name { get; set; }

    public List<NeedBase> Needs { get; set; }

    public int Perception { get; set; }

    public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();

    public List<Skill> Skills { get; set; }

    public float Speed { get; set; }

    public string Sprite { get; set; }

    public int Strenght { get; set; }

    public CreatureTask Task { get; set; }

    public Dictionary<string, float> ValueProperties { get; set; } = new Dictionary<string, float>();

    [JsonIgnore]
    public Vector3 Vector
    {
        get
        {
            return new Vector3(X, Y, Cell.RenderHeight);
        }
    }

    public int Vitality { get; set; }

    public float X { get; set; }

    public float Y { get; set; }

    public void AddLimb(Limb limb)
    {
        limb.Owner = this;
        Limbs.Add(limb);
    }

    public void ClearFixedAnimation()
    {
        FixedAnimation = null;
        FixedFrame = null;
    }

    public Item DropItem(Cell cell)
    {
        if (HeldItem == null)
        {
            return null;
        }
        var item = HeldItem;

        HeldItem.Free();
        HeldItem.Coords = (cell.Vector.x, cell.Vector.y);
        HeldItem = null;
        return item;
    }

    public void Face(Cell cell)
    {
        if (cell.Y < Cell.Y)
        {
            Facing = Direction.S;
        }
        else if (cell.Y > Cell.Y)
        {
            Facing = Direction.N;
        }
        else if (cell.X < Cell.X)
        {
            Facing = Direction.W;
        }
        else if (cell.X > Cell.X)
        {
            Facing = Direction.E;
        }
        else
        {
            // default
            Facing = Direction.S;
        }
    }

    public void GainSkill(string skillName, float amount)
    {
        var skill = GetSkill(skillName);

        if (skill == null)
        {
            skill = new Skill(skillName);
            Skills.Add(skill);
        }

        skill.Level += amount;
    }

    public List<OffensiveActionBase> GetAvailableOffensiveOptions()
    {
        return Limbs
                    .Where(l => !l.Disabled && !l.Busy)
                    .SelectMany(l => l.OffensiveActions)
                    .ToList();
    }

    public float GetCurrentNeed<T>() where T : NeedBase
    {
        return GetNeed<T>().Current;
    }

    public List<OffensiveActionBase> GetDefendableIncomingAttacks()
    {
        return IncomingAttacks.Where(a => GetPossibleDefensiveActions(a).Count > 0).ToList();
    }

    public List<DefensiveActionBase> GetDefensiveOptions()
    {
        return Limbs
                .Where(l => !l.Disabled && !l.Busy)
                .SelectMany(l => l.DefensiveActions)
                .ToList();
    }

    public List<OffensiveActionBase> GetInRangeOffensiveOptions(Creature target)
    {
        var offensiveActions = GetAvailableOffensiveOptions();
        var distance = Cell.DistanceTo(target.Cell);
        return offensiveActions.Where(o => o.Range >= distance).ToList();
    }

    public int GetMinRange()
    {
        var options = GetAvailableOffensiveOptions();
        if (options.Count == 0)
        {
            return 1;
        }
        return options.Max(o => o.Range);
    }

    public NeedBase GetNeed<T>() where T : NeedBase
    {
        return Needs.OfType<T>().FirstOrDefault();
    }

    public float GetNeedMax<T>() where T : NeedBase
    {
        return GetNeed<T>().Max;
    }

    public Orientation GetOrientation()
    {
        switch (Facing)
        {
            case Direction.N:
            case Direction.SE:
            case Direction.NE:
                return Orientation.Back;

            case Direction.E:
                return Orientation.Right;

            case Direction.SW:
            case Direction.NW:
            case Direction.S:
                return Orientation.Front;

            case Direction.W:
                return Orientation.Left;

            default:
                return Orientation.Front;
        }
    }

    public List<DefensiveActionBase> GetPossibleDefensiveActions(OffensiveActionBase action)
    {
        return GetDefensiveOptions().Where(d => d.ActivationTIme <= action.TimeToComplete()).ToList();
    }

    public Skill GetSkill(string skillName)
    {
        return Skills.Find(s => s.Name == skillName);
    }

    public float GetSkillLevel(string skillName)
    {
        var skill = GetSkill(skillName);

        if (skill == null)
        {
            return 5; // untyped
        }

        if (skill.Enabled != true)
        {
            return float.MinValue;
        }

        return skill.Level;
    }

    public Wound GetWorstWound()
    {
        return Limbs.SelectMany(l => l.Wounds)
                    .Where(w => !w.Treated)
                    .OrderByDescending(w => w.Danger)
                    .FirstOrDefault();
    }

    public bool HasSkill(string skillName)
    {
        if (string.IsNullOrEmpty(skillName))
        {
            return true; // unskilled
        }
        var skill = GetSkill(skillName);
        return skill?.Enabled == true;
    }

    public bool IsIdle()
    {
        return Task == null || Task is Idle;
    }

    public void Log(string message)
    {
        Debug.Log(Name + ":" + message);
        LogHistory.Add(message);

        // if verbose
        // Say(message);
    }

    public void PickUpItem(Item item, int amount)
    {
        if (HeldItem != null)
        {
            if (HeldItem.Name != item.Name)
            {
                DropItem(Cell);
            }
            else
            {
                if (amount < item.Amount)
                {
                    HeldItem.Amount += amount;
                    item.Amount -= amount;
                }
                else
                {
                    HeldItem.Amount += item.Amount;
                    Game.Instance.ItemController.DestroyItem(item);
                }
            }
        }
        else
        {
            if (amount < item.Amount)
            {
                HeldItem = item.Split(amount);
            }
            else
            {
                HeldItem = item;
            }
        }

        HeldItem.Reserve(this);
    }

    public void Say(string message, float duration = 1f)
    {
        if (FactionName == FactionConstants.Player)
        {
            CreatureRenderer.ShowText(message, duration);
        }
    }

    public void SetAnimation(Animation animation, float duration)
    {
        ClearFixedAnimation();
        Animation = animation;
        AnimationDelta = duration;
    }

    public void SetFixedAnimation(Animation animation, int? fixedFrame = null)
    {
        FixedAnimation = animation;
        FixedFrame = fixedFrame;
    }

    public void SetTargetCoordinate(float targetX, float targetY)
    {
        // only change the path and reset if the coords do not match
        if (TargetCoordinate.x != targetX || TargetCoordinate.y != targetY)
        {
            TargetCoordinate = (targetX, targetY);
            Path = null;
            UnableToFindPath = false;
        }
    }

    public override string ToString()
    {
        var text = $"{Name}\n";

        foreach (var limb in Limbs)
        {
            text += $"\t{limb}\n";
        }
        text += "\n";
        foreach (var need in Needs)
        {
            text += $"\t{need}\n";
        }

        return text;
    }

    public void UpdateSprite()
    {
        if (Sprite == "Composite")
        {
            if (FixedAnimation != null)
            {
                if (FixedFrame.HasValue)
                {
                    Frame = FixedFrame.Value;
                }
                SetSprite(FixedAnimation.Value);
            }
            else
            {
                if (Animation == Animation.Walk && !Moving)
                {
                    // standing still, stay on frame 0
                    Frame = 0;
                }
                SetSprite(Animation);
            }
        }
        else
        {
            CreatureRenderer.ClothesRenderer.gameObject.SetActive(false);
            CreatureRenderer.WeaponRenderer.gameObject.SetActive(false);

            CreatureRenderer.MainRenderer.flipX = Facing == Direction.W || Facing == Direction.NE || Facing == Direction.SW;
            if (!Sprite.Contains("_"))
            {
                CreatureRenderer.MainRenderer.sprite = Game.Instance.SpriteStore.GetCreatureSprite(Sprite, ref Frame);
            }
            else
            {
                CreatureRenderer.MainRenderer.sprite = Game.Instance.SpriteStore.GetSprite(Sprite);
            }
        }
    }

    internal void AbandonTask()
    {
        Faction.AvailableTasks.Add(Task);
        DropItem(Cell);

        Debug.Log($"{Task.GetType().Name}");
        Log($"Abandoned task: {Task.Message}");

        Task = null;
    }

    internal void AddRelationshipEvent(Creature creature, string name, float value)
    {
        var relation = Relationships.Find(r => r.Entity == creature);
        if (relation == null)
        {
            relation = new Relationship(creature);
            Relationships.Add(relation);
        }
        relation.AddEffect(name, value);
    }

    internal void CancelTask()
    {
        if (Task != null)
        {
            if (Task is Build build)
            {
                Game.Instance.StructureController.DestroyStructure(build.TargetStructure);
            }

            DropItem(Cell);
            Log($"Canceled {Task} task");
            
            Faction.RemoveTask(Task);
            Task.Complete();
            Task.Destroy();
            Task = null;
        }
    }

    internal void SuspendTask(bool autoRetry = true)
    {
        if (Task != null)
        {
            DropItem(Cell);
            Log($"Suspended {Task} task");

            Task.ToggleSuspended(autoRetry);
            Faction.AddTask(Task);

            Task = null;
        }
    }

    internal bool CanDo(CreatureTask t)
    {
        if (string.IsNullOrEmpty(t.RequiredSkill))
        {
            return true;
        }
        if (HasSkill(t.RequiredSkill) && GetSkillLevel(t.RequiredSkill) >= t.RequiredSkillLevel)
        {
            foreach (var subTask in t.SubTasks)
            {
                if (!CanDo(subTask))
                {
                    Debug.LogError($"{Name} cant do: {subTask.GetType().Name}");
                    return false;
                }
            }
        }
        else
        {
            return false;
        }

        return true;
    }

    internal int CurrentItemCount()
    {
        if (HeldItem == null)
        {
            return 0;
        }
        return HeldItem.Amount;
    }

    internal int GetPriority(CreatureTask t)
    {
        if (string.IsNullOrEmpty(t.RequiredSkill))
        {
            return 5; // untyped return baseline
        }

        if (HasSkill(t.RequiredSkill))
        {
            return GetSkill(t.RequiredSkill).Priority;
        }

        return 0;
    }

    internal void Perceive()
    {
        if (LastPercievedCoordinate != Cell)
        {
            _awareness = null;
            LastPercievedCoordinate = Cell;
        }
    }

    internal void ProcessSelf(float delta)
    {
        UpdateLimbs(delta);

        foreach (var need in Needs)
        {
            need.ApplyChange(delta);
            need.Update();
        }

        foreach (var feeling in Feelings.ToList())
        {
            if (feeling.DurationLeft <= -1f)
            {
                continue;
            }
            feeling.DurationLeft -= delta;

            if (feeling.DurationLeft <= 0)
            {
                Feelings.Remove(feeling);
            }
        }

        if (Random.value > 0.9 && !string.IsNullOrEmpty(Task?.BusyEmote))
        {
            Task.ShowBusyEmote(this);
        }
    }

    internal void Start()
    {
        foreach (var limb in Limbs)
        {
            limb.Link(this);
        }

        foreach (var need in Needs)
        {
            need.Creature = this;
        }

        LogHistory = new List<string>();

        TargetCoordinate = (Cell.X, Cell.Y);
    }

    internal bool Update(float timeDelta)
    {

        if (Game.Instance.TimeManager.Paused)
            return false;

        InternalTick += timeDelta;
        if (InternalTick >= Game.Instance.TimeManager.CreatureTick)
        {
            _selfTicks++;
            if (_selfTicks > SelfTickCount)
            {
                _selfTicks = 0;
                Perceive();
                ProcessSelf(Game.Instance.TimeManager.CreatureTick * SelfTickCount);
            }

            UpdateSprite();
            Move();

            InternalTick = 0;

            if (InCombat)
            {
                ClearFixedAnimation();
                ProcessCombat();
            }
            else
            {
                ProcessTask();
            }

            ResolveIncomingAttacks(timeDelta);
            Combatants.RemoveAll(c => c.Dead);

            if (HeldItem != null)
            {
                HeldItem.Renderer.SpriteRenderer.sortingLayerName = LayerConstants.CarriedItem;
                HeldItem.Coords = (X, Y);
            }

            return true;
        }

        if (AnimationDelta > 0)
        {
            AnimationDelta -= Time.deltaTime;
        }
        else
        {
            Animation = Animation.Walk;
        }

        return false;
    }

    private OffensiveActionBase GetBestAttack(Creature target)
    {
        var most = int.MinValue;
        OffensiveActionBase bestAttack = null;
        Limb bestTarget = null;

        foreach (var offensiveAction in GetInRangeOffensiveOptions(target))
        {
            foreach (var limb in target.Limbs)
            {
                if (limb.Disabled)
                    continue;

                offensiveAction.TargetLimb = limb;
                var prediction = offensiveAction.PredictDamage(target);

                if (limb.Vital)
                {
                    prediction = (int)(prediction * 2f * Aggression);
                }
                else
                {
                    prediction = (int)(prediction / Aggression);
                }

                if (prediction > most)
                {
                    most = prediction;
                    bestAttack = offensiveAction;
                    bestTarget = limb;
                }
            }
        }

        if (bestAttack != null)
        {
            bestAttack.TargetLimb = bestTarget;
        }

        return bestAttack;
    }

    private (int buffEffect, BuffBase bestBuff) GetBestBuff()
    {
        var most = int.MinValue;
        BuffBase bestBuff = null;

        foreach (var buff in GetBuffOptions())
        {
            var effect = buff.EstimateBuffEffect();
            if (effect == int.MinValue)
            {
                continue;
            }

            if (effect > most || (effect == most && Random.value > 0.5f))
            {
                most = effect;
                bestBuff = buff;
            }
        }

        return (most, bestBuff);
    }

    private List<BuffBase> GetBuffOptions()
    {
        return Limbs
             .Where(l => !l.Disabled && !l.Busy)
             .SelectMany(l => l.BuffActions)
             .ToList();
    }

    private (float defendedDamage, DefensiveActionBase bestDefense) GetDefense(OffensiveActionBase incomingAttack)
    {
        var defendedDamage = float.MinValue;
        DefensiveActionBase bestDefense = null;
        if (incomingAttack != null)
        {
            var incomingDamage = incomingAttack.PredictDamage(this);
            bestDefense = GetMostEffectiveDefensiveAction(incomingAttack);

            if (bestDefense != null)
            {
                defendedDamage = (incomingDamage - bestDefense.PredictDamageAfterDefense(incomingAttack)) / Aggression;
            }
        }

        return (defendedDamage, bestDefense);
    }

    private OffensiveActionBase GetMostDangerousUnblockedIncomingAttack()
    {
        var most = int.MinValue;
        OffensiveActionBase mostPowerfulAttack = null;
        foreach (var attack in IncomingAttacks.Where(a => a.DefensiveActions.Count == 0))
        {
            var dmg = attack.PredictDamage(this);
            if (dmg > most || (dmg == most && Random.value > 0.5f))
            {
                most = dmg;
                mostPowerfulAttack = attack;
            }
        }

        return mostPowerfulAttack;
    }

    private DefensiveActionBase GetMostEffectiveDefensiveAction(OffensiveActionBase attack)
    {
        if (attack != null)
        {
            var least = int.MaxValue;
            DefensiveActionBase mostEffectiveDefense = null;
            foreach (var def in GetPossibleDefensiveActions(attack))
            {
                var dmg = def.PredictDamageAfterDefense(attack);
                if (dmg < least || (dmg == least && Random.value > 0.5f))
                {
                    least = dmg;
                    mostEffectiveDefense = def;
                }
            }

            return mostEffectiveDefense;
        }
        return null;
    }

    private (float outgoingDamage, OffensiveActionBase bestAttack) GetOffense(Creature target)
    {
        var outgoingDamage = float.MinValue;
        var bestAttack = GetBestAttack(target);
        if (bestAttack != null)
        {
            outgoingDamage = bestAttack.PredictDamage(target) * Aggression;
        }

        return (outgoingDamage, bestAttack);
    }

    private void Move()
    {
        if (X == TargetCoordinate.x && Y == TargetCoordinate.y)
        {
            // no need to move
            Moving = false;
            return;
        }

        if (Path == null || Path.Count == 0)
        {
            Path = Pathfinder.FindPath(Game.Instance.Map.GetCellAtCoordinate(X, Y),
                                       Game.Instance.Map.GetCellAtCoordinate(TargetCoordinate.x, TargetCoordinate.y),
                                       Mobility);
            Path?.Reverse();
        }

        if (Path == null || Path.Count == 0)
        {
            UnableToFindPath = true;
            Debug.LogWarning("Unable to find path!");
            return;
        }

        var nextCell = Path[0];
        var targetX = nextCell.Vector.x;
        var targetY = nextCell.Vector.y;

        if (!nextCell.Pathable(Mobility))
        {
            Path = null;
            Say("...", 5);
            return;
        }

        if (X == targetX && Y == targetY)
        {
            // reached the cell
            Path.RemoveAt(0);
            return;
        }

        Moving = true;

        var maxX = Mathf.Max(targetX, X);
        var minX = Mathf.Min(targetX, X);

        var maxY = Mathf.Max(targetY, Y);
        var minY = Mathf.Min(targetY, Y);

        var yspeed = Mathf.Min(Speed + Random.Range(0f, 0.01f), maxY - minY);
        var xspeed = Mathf.Min(Speed + Random.Range(0f, 0.01f), maxX - minX);

        if (targetY > Y)
        {
            Facing = Direction.N;
            Y += yspeed;
        }
        else if (targetY < Y)
        {
            Facing = Direction.S;
            Y -= yspeed;
        }

        if (targetX > X)
        {
            X += xspeed;

            if (Facing == Direction.N)
            {
                Facing = Direction.NE;
            }
            else if (Facing == Direction.S)
            {
                Facing = Direction.SE;
            }
            else
            {
                Facing = Direction.E;
            }
        }
        else if (targetX < X)
        {
            X -= xspeed;
            if (Facing == Direction.N)
            {
                Facing = Direction.NW;
            }
            else if (Facing == Direction.S)
            {
                Facing = Direction.SW;
            }
            else
            {
                Facing = Direction.W;
            }
        }

        CreatureRenderer.UpdatePosition();
    }

    private void ProcessCombat()
    {
        try
        {
            var target = Combatants[0];
            var incomingAttack = GetMostDangerousUnblockedIncomingAttack();

            var (defendedDamage, bestDefense) = GetDefense(incomingAttack);
            var (outgoingDamage, bestAttack) = GetOffense(target);
            var (boostEffect, bestBuff) = GetBestBuff();

            if (bestAttack == null && bestDefense == null && bestBuff == null)
            {
                var minRange = GetMinRange();

                foreach (var combatant in Combatants)
                {
                    if (Cell.DistanceTo(combatant.Cell) > minRange)
                    {
                        SetTargetCoordinate(combatant.X, combatant.Y);
                        break;
                    }
                }

                return;
            }

            if (boostEffect > outgoingDamage && boostEffect > defendedDamage)
            {
                bestBuff.Activate();
            }
            else if (outgoingDamage > defendedDamage)
            {
                // aggro
                Log($"{Name} launches a {bestAttack.Name} at {target.Name}'s {bestAttack.TargetLimb.Name}");

                bestAttack.Reset();

                if (!target.Combatants.Contains(this))
                    target.Combatants.Add(this);

                target.IncomingAttacks.Add(bestAttack);
                bestAttack.Limb.Busy = true;
            }
            else
            {
                // defend
                Log($"{Name} defends with a {bestDefense.Name} against {incomingAttack.Owner.Name}'s {incomingAttack.Name}");

                incomingAttack.DefensiveActions.Add(bestDefense);
                bestDefense.Limb.Busy = true;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
    }

    private void ProcessTask()
    {
        if (Task == null)
        {
            var task = Faction.TakeTask(this);

            if (task != null)
            {
                Task = task;
            }
        }
        else
        {
            try
            {
                if (!CanDo(Task))
                {
                    throw new Exception("Unable to do assigned task!");
                }

                if (Task.Done(this))
                {
                    Task.ShowDoneEmote(this);
                    Task.Complete();

                    Faction.RemoveTask(Task);
                    Task = null;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Task failed: {ex}");

                if (!Cell.Pathable(Mobility))
                {
                    // unstuck
                    Debug.LogError("Unstuck!");
                    var c = Game.Instance.Map.GetNearestPathableCell(Cell, Mobility, 10);

                    X = c.X;
                    Y = c.Y;
                    CreatureRenderer.UpdatePosition();
                }
                else
                {
                    CancelTask();
                }
            }
        }
    }

    private void ResolveIncomingAttacks(float timeDelta)
    {
        foreach (var attack in IncomingAttacks.ToList())
        {
            if (attack.Owner.Dead)
            {
                attack.Reset();
                IncomingAttacks.Remove(attack);
            }
            else if (attack.Done(timeDelta))
            {
                attack.Resolve();
                attack.Reset();
                IncomingAttacks.Remove(attack);
            }
        }
    }

    private void SetSprite(Animation animation)
    {
        var frame = CharacterSpriteSheet.GetFrame(animation, GetOrientation(), ref Frame);
        CreatureRenderer.MainRenderer.sprite = frame.body;
        CreatureRenderer.ClothesRenderer.sprite = frame.clothes;
    }

    private void UpdateLimbs(float timeDelta)
    {
        foreach (var boost in Limbs.ToList().SelectMany(l => l.BuffActions))
        {
            boost.Update(timeDelta);
        }

        foreach (var limb in Limbs)
        {
            limb.Update(timeDelta);
        }
    }
}

public class Relationship
{
    public List<(string name, float value)> Effectors = new List<(string name, float value)>();

    public Relationship()
    {
    }

    public Relationship(IEntity entity) : this()
    {
        Entity = entity;
    }

    [JsonIgnore]
    public IEntity Entity
    {
        get
        {
            return EntityId.GetEntity();
        }
        set
        {
            EntityId = value.Id;
        }
    }

    public string EntityId { get; set; }

    [JsonIgnore]
    public float Value
    {
        get
        {
            var total = 0f;

            foreach (var (name, value) in Effectors)
            {
                total += value;
            }

            return total;
        }
    }

    internal void AddEffect(string name, float value)
    {
        Effectors.Add((name, value));
    }
}