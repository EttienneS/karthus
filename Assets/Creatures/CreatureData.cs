using Assets.Creature.Behaviour;
using Needs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public enum Mobility
{
    Walk, Fly
}

namespace Assets.Creature
{
    public class CreatureData : IEntity
    {
        [JsonIgnore]
        public List<CreatureData> Combatants = new List<CreatureData>();

        public Direction Facing = Direction.S;

        public List<Feeling> Feelings = new List<Feeling>();

        [JsonIgnore]
        public IBehaviour Behaviour;

        public List<OffensiveActionBase> IncomingAttacks = new List<OffensiveActionBase>();

        public Mobility Mobility;

        public string Model;

        [JsonIgnore]
        public List<Cell> Path = new List<Cell>();

        public List<Relationship> Relationships = new List<Relationship>();

        public (float x, float z) TargetCoordinate;

        public bool UnableToFindPath;

        internal int Frame;

        internal float InternalTick = float.MaxValue;

        [JsonIgnore]
        internal Cell LastPercievedCoordinate;

        private const int SelfTickCount = 10;

        [JsonIgnore]
        private List<Cell> _awareness;

        private bool _combatMoving;

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
                return Game.Instance.Map.GetCellAtCoordinate(X, Z);
            }
            set
            {
                X = Cell.Vector.x;
                Z = Cell.Vector.y;

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
                    if (string.IsNullOrEmpty(FactionName))
                    {
                        Debug.LogError($"Null faction: {Name}:{Id}");
                    }

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
                return new Vector3(X, Cell.Y + 0.5f, Z);
            }
        }

        public int Vitality { get; set; }

        public float X { get; set; }

        public float Z { get; set; }

        public void AddLimb(Limb limb)
        {
            limb.Owner = this;
            Limbs.Add(limb);
        }

        public Item DropItem(Cell cell)
        {
            if (HeldItem == null)
            {
                return null;
            }
            var item = HeldItem;

            HeldItem.Free();
            HeldItem.Cell = cell;
            HeldItem = null;
            return item;
        }

        public void Face(Cell cell)
        {
            if (cell.Z < Cell.Z)
            {
                Facing = Direction.S;
            }
            else if (cell.Z > Cell.Z)
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
            CreatureRenderer.UpdateRotation();
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

        public List<OffensiveActionBase> GetInRangeOffensiveOptions(CreatureData target)
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

        public void Say(string message, float duration = 10f)
        {
            if (FactionName == FactionConstants.Player)
            {
                CreatureRenderer.ShowText(message, duration);
            }
        }

        public void SetAnimation(AnimationType animation)
        {
            CreatureRenderer?.SetAnimation(animation);
        }

        public void SetTargetCoordinate(float targetX, float targetZ)
        {
            UnableToFindPath = false;

            // only change the path and reset if the coords do not match
            if (TargetCoordinate.x != targetX || TargetCoordinate.z != targetZ)
            {
                TargetCoordinate = (targetX, targetZ);
                Path = null;
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

        internal void AbandonTask()
        {
            Faction.AvailableTasks.Add(Task);
            DropItem(Cell);

            Debug.Log($"{Task.GetType().Name}");
            Log($"Abandoned task: {Task.Message}");

            Task = null;
        }

        internal void AddRelationshipEvent(CreatureData creature, string name, float value)
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

            TargetCoordinate = (Cell.X, Cell.Z);
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

        internal bool Update(float timeDelta)
        {
            if (Game.Instance.TimeManager.Paused)
                return false;

            if (Dead)
            {
                return true;
            }

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

                Move();

                InternalTick = 0;

                if (InCombat)
                {
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
                    HeldItem.Coords = (X, Z);
                }

                return true;
            }

            return false;
        }

        private OffensiveActionBase GetBestAttack(CreatureData target)
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

        private (int buffEffect, StatusActionBase bestBuff) GetBestBuff()
        {
            var most = int.MinValue;
            StatusActionBase bestBuff = null;

            foreach (var buff in GetBuffOptions())
            {
                var effect = buff.EstimateBuffEffect();
                if (effect == int.MinValue)
                {
                    continue;
                }

                if (effect > most || effect == most && Random.value > 0.5f)
                {
                    most = effect;
                    bestBuff = buff;
                }
            }

            return (most, bestBuff);
        }

        private List<StatusActionBase> GetBuffOptions()
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
                if (dmg > most || dmg == most && Random.value > 0.5f)
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
                    if (dmg < least || dmg == least && Random.value > 0.5f)
                    {
                        least = dmg;
                        mostEffectiveDefense = def;
                    }
                }

                return mostEffectiveDefense;
            }
            return null;
        }

        private (float outgoingDamage, OffensiveActionBase bestAttack) GetOffense(CreatureData target)
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
            if (X.AlmostEquals(TargetCoordinate.x) && Z.AlmostEquals(TargetCoordinate.z))
            {
                StopMoving();
                return;
            }

            var src = Game.Instance.Map.GetCellAtCoordinate(X, Z);
            var tgt = Game.Instance.Map.GetCellAtCoordinate(TargetCoordinate.x, TargetCoordinate.z);
            if (Path == null || Path.Count == 0)
            {
                Path = Pathfinder.FindPath(src, tgt, Mobility);
                Path?.Reverse();
            }

            if (Path == null || Path.Count == 0)
            {
                UnableToFindPath = true;
                StopMoving();

                Debug.LogWarning($"Unable to find path! ({X}:{Z}) >> ({TargetCoordinate.x}:{TargetCoordinate.z})");
                return;
            }

            var nextCell = Path[0];
            var targetX = nextCell.Vector.x;
            var targetZ = nextCell.Vector.z;

            if (!nextCell.Pathable(Mobility))
            {
                UnableToFindPath = true;
                StopMoving();

                Say("...");
                return;
            }

            if (X.AlmostEquals(targetX) && Z.AlmostEquals(targetZ))
            {
                // reached the cell
                Path.RemoveAt(0);

                if (Path.Count > 0)
                {
                    nextCell = Path[0];
                    targetX = nextCell.Vector.x;
                    targetZ = nextCell.Vector.z;
                }
                else
                {
                    // reached end of path
                    return;
                }
            }

            var maxX = Mathf.Max(targetX, X);
            var minX = Mathf.Min(targetX, X);

            var maxZ = Mathf.Max(targetZ, Z);
            var minZ = Mathf.Min(targetZ, Z);

            var yspeed = Mathf.Min(Speed + Random.Range(0f, 0.01f), maxZ - minZ);
            var xspeed = Mathf.Min(Speed + Random.Range(0f, 0.01f), maxX - minX);

            if (targetZ > Z)
            {
                Facing = Direction.N;
                Z += yspeed;
            }
            else if (targetZ < Z)
            {
                Facing = Direction.S;
                Z -= yspeed;
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

            if (X > 0 || Z > 0)
            {
                SetAnimation(AnimationType.Running);
            }

            CreatureRenderer.UpdatePosition();

            if (X.AlmostEquals(TargetCoordinate.x) && Z.AlmostEquals(TargetCoordinate.z))
            {
                StopMoving();
            }
        }

        private void ProcessCombat()
        {
            try
            {
                if (_combatMoving)
                {
                    return;
                }

                var target = Combatants[0];
                var incomingAttack = GetMostDangerousUnblockedIncomingAttack();

                var (defendedDamage, bestDefense) = GetDefense(incomingAttack);
                var (outgoingDamage, bestAttack) = GetOffense(target);
                var (boostEffect, bestBuff) = GetBestBuff();

                //if (target.Cell == Cell)
                //{
                //    var closestCell = target.Cell.NonNullNeighbors.Where(c => c != Cell).OrderBy(c => c.DistanceTo(Cell)).First();
                //    SetTargetCoordinate(closestCell.Vector.x, closestCell.Vector.z);
                //    _combatMoving = true;
                //    return;
                //}

                if (bestAttack == null && bestDefense == null && bestBuff == null)
                {
                    var minRange = GetMinRange();

                    if (Cell.DistanceTo(target.Cell) > minRange)
                    {
                        var closestCell = target.Cell.NonNullNeighbors.OrderBy(c => c.DistanceTo(Cell)).First();
                        SetTargetCoordinate(closestCell.Vector.x, closestCell.Vector.z);
                        _combatMoving = true;
                    }

                    return;
                }

                Face(target.Cell);
                if (boostEffect > outgoingDamage && boostEffect > defendedDamage)
                {
                    SetAnimation(AnimationType.Interact);
                    bestBuff.Activate();
                }
                else if (outgoingDamage > defendedDamage)
                {
                    // aggro
                    Log($"{Name} launches a {bestAttack.Name} at {target.Name}'s {bestAttack.TargetLimb.Name}");
                    SetAnimation(AnimationType.Attack);

                    bestAttack.Reset();

                    if (!target.Combatants.Contains(this))
                        target.Combatants.Add(this);

                    target.IncomingAttacks.Add(bestAttack);
                    bestAttack.Limb.Busy = true;
                }
                else
                {
                    SetAnimation(AnimationType.Interact);

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
                if (Faction == null)
                {
                    Debug.LogError($"Null  faction: {Name}:{Id}");
                }
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
                        Z = c.Z;
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

        private void StopMoving()
        {
            Path = null;
            TargetCoordinate = (X, Z);

            _combatMoving = false;
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
}