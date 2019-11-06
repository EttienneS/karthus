using UnityEngine;

public static class CreatureHelper
{
    public static string GetRandomName()
    {
        // very basic name generator, just combines the three parts together
        // todo: Expand this to have some start letters be more or less common (for example X names are more rare)
        // todo: Add some kind of racial filter, perhaps a dragon has a longer name and elves have the classic el'ven style apostrophe names
        var front = new[]
        {
                "Ch", "K", "Sh", "R","E", "Lo", "Ve", "Ko", "Tu", "Hi", "J", "S", "St", "B", "T", "X","Br", "M",
                "P", "D", "Kr", "Can", "Ex", "J", "H", "Th", "Sch", "Ten", "W", "Wr", "V", "Ja", "Pi", "Fr", "Sw", "K", "Sp", "Sw"
        };
        var mid = new[]
        {
                "a", "e", "u", "olo", "i", "o", "oo", "ee", "ero", "ane", "ala", "are", "ou", "y", "ai",
                "or", "ava", "oe", "ozo"
        };
        var end = new[]
        {
                "ll", "xel", "lle", "p", "ck", "t", "ne", "lla", "le", "x", "lo", "lee", "bel", "tel", "xa",
                "ty", "te", "se", "tee", "ack", "f", "g", "u", "q", "ff", "ns", "es", "ko", "nna", "os","ner","ley","no","ba"
        };

        var name = string.Empty;

        name += front[Random.Range(0, front.Length)];
        name += mid[Random.Range(0, mid.Length)];
        name += end[Random.Range(0, end.Length)];

        return name;
    }
}