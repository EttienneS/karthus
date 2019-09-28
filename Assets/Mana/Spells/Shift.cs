public class PipeConstants
{
    public const string Nothing = "Nothing";
    public const string Content = "Content";
    public const string Pressure = "Pressure";
    public const string Suckable = "Suckable";
}

public class Shift : SpellBase
{
    public Shift()
    {
    }

    public override bool DoSpell()
    {
        var linkedPipes = AssignedEntity.Cell.LinkedPipes;

        var pressure = AssignedEntity.ValueProperties[PipeConstants.Pressure];
        var content = AssignedEntity.Properties[PipeConstants.Content];

        if (pressure <= 0 || content == PipeConstants.Nothing)
        {
            AssignedEntity.Properties[PipeConstants.Content] = PipeConstants.Nothing;
            return false;
        }

        foreach (var linkedpipe in linkedPipes)
        {
            var targetContent = linkedpipe.Properties[PipeConstants.Content];
            if ((targetContent == content || targetContent == PipeConstants.Nothing) 
                && linkedpipe.ValueProperties[PipeConstants.Pressure] < pressure)
            {
                linkedpipe.Properties[PipeConstants.Content] = content;

                AssignedEntity.ValueProperties[PipeConstants.Pressure]--;
                linkedpipe.ValueProperties[PipeConstants.Pressure]++;

                linkedpipe.Cell.UpdateTile();
                break;
            }
        }

        return true;
    }
}