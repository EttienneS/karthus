public enum FloatOperation
{
    Add, Subtract, Divide, Mulitply, Set
}

public class ChangeEntityProperty : EffectBase
{
    public string PropertyName { get; set; }
    public string PropertyValueString { get; set; }
    public float PropertyValueFloat { get; set; }

    public FloatOperation FloatOperation { get; set; }

    public override bool DoEffect()
    {
        if (!string.IsNullOrWhiteSpace(PropertyValueString))
        {
            AssignedEntity.Properties[PropertyName] = PropertyValueString;
        }
        else
        {
            switch (FloatOperation)
            {
                case FloatOperation.Add:
                    AssignedEntity.ValueProperties[PropertyName] += PropertyValueFloat;
                    break;

                case FloatOperation.Subtract:
                    AssignedEntity.ValueProperties[PropertyName] -= PropertyValueFloat;
                    break;

                case FloatOperation.Divide:
                    AssignedEntity.ValueProperties[PropertyName] *= PropertyValueFloat;
                    break;

                case FloatOperation.Mulitply:
                    AssignedEntity.ValueProperties[PropertyName] /= PropertyValueFloat;
                    break;

                case FloatOperation.Set:
                    AssignedEntity.ValueProperties[PropertyName] = PropertyValueFloat;
                    break;
            }
        }

        return true;
    }
}