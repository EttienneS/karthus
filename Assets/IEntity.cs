using System.Collections.Generic;

public interface IEntity
{
    Cell Cell { get; set; }

    string Name { get; set; }
    string Id { get; set; }

    ManaPool ManaPool { get; set; }

    string FactionName { get; set; }

    void Damage(IEntity attacker, TargetType target, float power, float accuracy);

    Dictionary<string, string> Properties { get; set; }
    Dictionary<string, float> ValueProperties { get; set; }
    List<VisualEffectData> LinkedVisualEffects { get; set; }
}

public enum TargetType
{
    Easy, Critical, Biggest
}