using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public enum Mobility
{
    Walk, AbyssWalk, Fly
}

public class Creature : IEntity
{
    public const string SelfKey = "Self";
    [JsonIgnore] public Color BottomColor;
    public List<Creature> Combatants = new List<Creature>();

    public Direction Facing = Direction.S;
    [JsonIgnore] public Behaviours.GetBehaviourTaskDelegate GetBehaviourTask;
    [JsonIgnore] public Color HairColor;
    public int HairStyle;
    public List<OffensiveActionBase> IncomingAttacks = new List<OffensiveActionBase>();
    public int Index = 1;
    public Dictionary<string, Memory> Mind = new Dictionary<string, Memory>();
    public Mobility Mobility;
    [JsonIgnore] public Color SkinColor;
    [JsonIgnore] public Color TopColor;
    internal float InternalTick = float.MaxValue;
    [JsonIgnore] internal Cell LastPercievedCoordinate;
    internal float WorkTick = float.MaxValue;
    private List<Cell> _awareness;
    private Cell _cell;
    private Faction _faction;
    private bool _firstRun = true;
    private CreatureTask _task;
    public float Aggression { get; set; }
    [JsonIgnore]
    public List<Cell> Awareness
    {
        get
        {
            if (_awareness == null && Cell != null)
            {
                _awareness = Game.Map.GetCircle(Cell, Perception);
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
            return Game.Map.GetCellAtCoordinate(X, Y);
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
    public int DEX
    {
        get
        {
            return (Dexterity - 10) / 2;
        }
    }

    public int Dexterity { get; set; }
    [JsonIgnore]
    public Faction Faction
    {
        get
        {
            if (_faction == null)
            {
                _faction = Game.FactionController.Factions[FactionName];
            }

            return _faction;
        }
    }

    public string FactionName { get; set; }
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
    public ManaPool ManaPool { get; set; }
    public string Name { get; set; }
    public int Perception { get; set; }
    public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();
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

    internal void Start()
    {
        foreach (var limb in Limbs)
        {
            limb.Link(this);
        }
    }

    public List<Skill> Skills { get; set; }
    public float Speed { get; set; }
    public string Sprite { get; set; }

    public int STR
    {
        get
        {
            return (Strenght - 10) / 2;
        }
    }

    public int Strenght { get; set; }

    public CreatureTask Task
    {
        get
        {
            return _task;
        }
        set
        {
            if (_task != null)
            {
                _task.Destroy();
            }
            _task = value;
        }
    }

    public Dictionary<string, float> ValueProperties { get; set; } = new Dictionary<string, float>();

    [JsonIgnore]
    public Vector2 Vector
    {
        get
        {
            return new Vector2(X, Y);
        }
    }

    public float X { get; set; }

    public float Y { get; set; }

    public void AddLimb(Limb limb)
    {
        limb.Owner = this;
        Limbs.Add(limb);
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

        if (skill != null)
        {
            skill = new Skill(skillName);
            Skills.Add(skill);
        }

        skill.Level += amount;
    }

    public List<OffensiveActionBase> GetAvailableOffensiveOptions()
    {
        return Limbs
                    .Where(l => l.Enabled && !l.Busy)
                    .SelectMany(l => l.OffensiveActions)
                    .ToList();
    }

    public List<OffensiveActionBase> GetDefendableIncomingAttacks()
    {
        return IncomingAttacks.Where(a => GetPossibleDefensiveActions(a).Count > 0).ToList();
    }

    public List<DefensiveActionBase> GetDefensiveOptions()
    {
        return Limbs
                .Where(l => l.Enabled && !l.Busy)
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

    public bool HasSkill(string skillName)
    {
        if (string.IsNullOrEmpty(skillName))
        {
            return true; // unskilled
        }
        var skill = GetSkill(skillName);
        return skill?.Enabled == true;
    }

    public void Log(string message = "")
    {
        Debug.Log(Name + ":" + message);
        CreatureRenderer.ShowText(message, 0.5f);
    }

    public void MoveTo(Creature creature)
    {
        MoveTo(creature.X, creature.Y);
    }

    public void MoveTo(IEntity entity)
    {
        MoveTo(entity.Cell);
    }

    public void MoveTo(Cell cell)
    {
        MoveTo(cell.X, cell.Y);
    }

    public void MoveTo(float targetX, float targetY)
    {
        var maxX = Mathf.Max(targetX, X);
        var minX = Mathf.Min(targetX, X);

        var maxY = Mathf.Max(targetY, Y);
        var minY = Mathf.Min(targetY, Y);


        var yspeed = Mathf.Min(Speed + Random.Range(0f, 0.01f), maxY - minY);
        var xspeed = Mathf.Min(Speed + Random.Range(0f, 0.01f), maxX - minX);

        if (targetY > Y)
        {
            Y += yspeed;
        }
        else if (targetY < Y)
        {
            Y -= yspeed;
        }

        if (targetX > X)
        {
            X += xspeed;
        }
        else if (targetX < X)
        {
            X -= xspeed;
        }

        CreatureRenderer.UpdatePosition();
    }

    public void MoveTo(Vector2 vector)
    {
        MoveTo(vector.x, vector.y);
    }

    public int RollDex()
    {
        return Random.Range(1, 20) + DEX;
    }

    public int RollStr()
    {
        return Random.Range(1, 20) + STR;
    }

    public void SetColors()
    {
        if (_firstRun)
        {
            SkinColor = ColorExtensions.GetRandomSkinColor();
            HairColor = ColorExtensions.GetRandomHairColor();
            TopColor = ColorExtensions.GetRandomColor();
            BottomColor = ColorExtensions.GetRandomColor();
            HairStyle = Random.Range(1, 3);
            _firstRun = false;
        }

        CreatureRenderer.BodyRenderer.color = SkinColor;
        CreatureRenderer.TopRenderer.color = TopColor;
        CreatureRenderer.BottomRenderer.color = BottomColor;
        CreatureRenderer.HairRenderer.color = HairColor;
    }

    public override string ToString()
    {
        var text = $"{Name}\n\n";

        foreach (var limb in Limbs)
        {
            text += $"\t{limb.Name} [{limb.HP}/{limb.Max}]\n";
        }

        return text;
    }

    internal void CancelTask()
    {
        if (Task != null)
        {
            Faction.RemoveTask(Task);
            Task.Destroy();
            Task = null;
        }
    }

    internal bool CanDo(CreatureTask t)
    {
        if (HasSkill(t.RequiredSkill))
        {
            return GetSkillLevel(t.RequiredSkill) >= t.RequiredSkillLevel;
        }

        return false;
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
                var structure = IdService.GetStructure(structureId);
                if (structure != null && structure.InUseBy == this)
                {
                    structure.Free();
                }
            }
        }
    }

    internal IEntity GetClosestBattery()
    {
        return Faction.GetClosestBattery(this);
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

    internal void Know(string context)
    {
        // Debug.Log($"Add context: {context}");
        Mind.Add(context, new Memory());
    }

    internal void Live()
    {
        ValueProperties[Prop.Hunger] += Random.value;
        ValueProperties[Prop.Energy] -= Random.Range(0.1f, 0.25f);
    }
    internal void Perceive()
    {
        if (LastPercievedCoordinate != Cell)
        {
            _awareness = null;
            LastPercievedCoordinate = Cell;
        }
    }
    internal bool Update(float timeDelta)
    {
        if (Game.TimeManager.Paused)
            return false;

        InternalTick += timeDelta;
        WorkTick += timeDelta;

        if (WorkTick >= Game.TimeManager.WorkInterval)
        {
            WorkTick = 0;
            if (InCombat)
            {
                ProcessCombat();
            }
            else
            {
                ProcessTask();
            }

            ResolveIncomingAttacks(timeDelta);
            UpdateLimbs(timeDelta);
            Combatants.RemoveAll(c => c.Dead);
        }


        if (InternalTick >= Game.TimeManager.TickInterval)
        {
            InternalTick = 0;
            Perceive();
            Live();
            UpdateSprite();

            return true;
        }

        return false;
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
    private OffensiveActionBase GetBestAttack(Creature target)
    {
        var most = int.MinValue;
        OffensiveActionBase bestAttack = null;
        Limb bestTarget = null;

        foreach (var offensiveAction in GetInRangeOffensiveOptions(target))
        {
            foreach (var limb in target.Limbs)
            {
                if (!limb.Enabled)
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
             .Where(l => l.Enabled && !l.Busy)
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
                        MoveTo(combatant);
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
                var context = $"{Id} - {task} - {Game.TimeManager.Now}";

                Know(context);
                task.Context = context;

                Task = task;
            }
        }
        else
        {
            try
            {
                if (Task.Done(this))
                {
                    Task.ShowDoneEmote(this);
                    FreeResources(Task.Context);
                    Forget(Task.Context);

                    Faction.RemoveTask(Task);
                    Task = null;
                }
                else
                {
                    if (Random.value > 0.8)
                    {
                        Task.ShowBusyEmote(this);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Task failed: {ex}");
                CancelTask();
            }
        }
    }

    private void ResolveIncomingAttacks(float timeDelta)
    {
        foreach (var attack in IncomingAttacks.ToList())
        {
            if (attack.Done(timeDelta))
            {
                attack.Resolve();
                attack.Reset();
                IncomingAttacks.Remove(attack);
            }
        }
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

    private void UpdateSprite()
    {
        bool flip = Facing == Direction.W || Facing == Direction.NW || Facing == Direction.SW;
        if (!Sprite.Contains("_"))
        {
            CreatureRenderer.MainRenderer.flipX = flip;
            CreatureRenderer.MainRenderer.sprite = Game.SpriteStore.GetCreatureSprite(Sprite, ref Index);
        }
        else
        {
            var facingKey = Game.SpriteStore.FacingUp(Facing) ? "b_" : "f_";

            CreatureRenderer.FaceRenderer.flipX = flip;
            CreatureRenderer.BodyRenderer.flipX = flip;
            CreatureRenderer.TopRenderer.flipX = flip;
            CreatureRenderer.BottomRenderer.flipX = flip;
            CreatureRenderer.HairRenderer.flipX = flip;

            SetColors();

            if (facingKey == "f_")
            {
                CreatureRenderer.FaceRenderer.sprite = Game.SpriteStore.GetBodySprite(Sprite + "face");
            }
            else
            {
                CreatureRenderer.FaceRenderer.sprite = null;
            }
            CreatureRenderer.BodyRenderer.sprite = Game.SpriteStore.GetBodySprite(Sprite + facingKey + "body");
            CreatureRenderer.TopRenderer.sprite = Game.SpriteStore.GetBodySprite(Sprite + facingKey + "top");
            CreatureRenderer.BottomRenderer.sprite = Game.SpriteStore.GetBodySprite(Sprite + facingKey + "bottom");
            CreatureRenderer.HairRenderer.sprite = Game.SpriteStore.GetBodySprite(Sprite + facingKey + "hair_" + HairStyle);
        }
    }
}