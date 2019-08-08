public interface IEntity
{
    Coordinates Coordinates { get; set; }

    string Id { get; set; }

    ManaPool ManaPool { get; set; }
}