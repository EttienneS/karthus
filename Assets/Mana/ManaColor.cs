using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

[JsonConverter(typeof(StringEnumConverter))]
public enum ManaColor
{
    Red, Green, Blue, White, Black
}