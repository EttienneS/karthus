using UnityEngine;

internal class DelayedPropertyEffect : EffectBase
{
    public float Delay;
    public float PropertyFloatValue;
    public string PropertyName;
    public string PropertyStringValue;

    private float _elapsed;

    private int _range;

    public DelayedPropertyEffect(float delay, string propertyName, string value, int range)
    {
        Delay = delay;
        PropertyName = propertyName;
        PropertyStringValue = value;
        _range = range;
    }

    public DelayedPropertyEffect(float delay, string propertyName, float value, int range)
    {
        Delay = delay;
        PropertyName = propertyName;
        PropertyFloatValue = value;
        _range = range;
    }

    public override int Range
    {
        get
        {
            return _range;
        }
    }
    public override bool DoEffect()
    {
        if (string.IsNullOrWhiteSpace(PropertyStringValue))
        {
            AssignedEntity.Properties[PropertyName] = PropertyStringValue;
        }
        else
        {
            AssignedEntity.ValueProperties[PropertyName] = PropertyFloatValue;
        }

        return true;
    }

    public override bool Ready()
    {
        _elapsed += Time.deltaTime;

        if (_elapsed >= Delay)
        {
            return true;
        }

        return false;
    }
}