using Newtonsoft.Json;

public abstract class BuffBase
{
    protected BuffBase(string name, float cooldown, float duration)
    {
        Name = name;
        CooldownMax = cooldown;
        DurationMax = duration;
    }

    [JsonIgnore]
    public bool Active
    {
        get
        {
            return !OnCoolDown && RemainingDuration > 0;
        }
    }

    public float CooldownMax { get; set; }

    public float DurationMax { get; set; }

    [JsonIgnore]
    public Limb Limb { get; set; }

    public string Name { get; set; }
    public bool OnCoolDown { get; set; }

    [JsonIgnore]
    public Creature Owner
    {
        get
        {
            return Limb.Owner;
        }
    }

    [JsonIgnore]
    public float RemainingCooldown { get; set; }

    [JsonIgnore]
    public float RemainingDuration { get; set; }

    public void Activate()
    {
        RemainingDuration = DurationMax;
        Limb.Busy = true;
        StartBuff();
    }

    public abstract int EstimateBuffEffect();

    public void Update(float deltaTime)
    {
        if (OnCoolDown)
        {
            RemainingCooldown -= deltaTime;

            if (RemainingCooldown <= 0)
            {
                OnCoolDown = false;
            }
        }
        else if (Active)
        {
            Limb.Busy = true;
            RemainingDuration -= deltaTime;
            if (RemainingDuration <= 0)
            {
                RemainingCooldown = CooldownMax;
                OnCoolDown = true;
                Limb.Busy = false;
                EndBuff();
            }
        }
    }

    internal abstract void EndBuff();

    internal abstract void StartBuff();
}