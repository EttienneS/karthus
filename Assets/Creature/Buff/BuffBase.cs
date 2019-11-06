public abstract class BuffBase
{
    protected BuffBase(string name, float cooldown, float duration)
    {
        Name = name;
        CooldownMax = cooldown;
        RemainingCooldown = cooldown;
        DurationMax = duration;
        RemainingDuration = duration;
    }

    public Limb Limb { get; set; }

    public string Name { get; set; }
    public bool OnCoolDown { get; set; }
    public float RemainingCooldown { get; set; }
    public float CooldownMax { get; set; }

    public float RemainingDuration { get; set; }
    public float DurationMax { get; set; }

    public Creature Owner
    {
        get
        {
            return Limb.Owner;
        }
    }

    public bool Active
    {
        get
        {
            return !OnCoolDown && RemainingDuration > 0;
        }
    }

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

    public void Activate()
    {
        RemainingDuration = DurationMax;
        Limb.Busy = true;
        StartBuff();
    }

    internal abstract void StartBuff();

    internal abstract void EndBuff();

    public abstract int EstimateBuffEffect();
}