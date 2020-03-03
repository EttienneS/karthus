using System.Collections.Generic;

public static class NameHelper
{
    private static List<(string[] front, string[] mid, string[] end)> _generators;

    public static List<(string[] front, string[] mid, string[] end)> Generators
    {
        get
        {
            if (_generators == null)
            {
                _generators = new List<(string[] front, string[] mid, string[] end)>
                {
                    (new[] { "T", "K", "V", "R", "E", "L", "S" },
                     new[] { "a", "e", "u", "o", "i" },
                     new[] { "k", "l", "r", "ck", "t" }),

                    (new[] { "B", "K", "Ch", "F", "Y" },
                     new[] { "a", "e", "u", "o", "i" },
                     new[] { "ng" }),

                    (new[] { "Sh", "F", "S", "H", "G" },
                     new[] { "an", "atim", "osn", "owan", "in", "ahad" },
                     new[] { "a", "" }),

                    (new[] { "J", "D", "R", "B", "V" },
                     new[] { "era", "eva", "avo", "amo", "ee", "ero" },
                     new[] { "s", "k", "", "l" }),

                    (new[] { "J", "S", "B", "P", "K" },
                     new[] { "o", "a", "e", "ie", "ie", "u", "oe" },
                     new[] { "lly", "so", "boe", "ks", "ko", "nny", "cky", "rie" }),

                    (new[] { "U", "I", "De","O","A"},
                     new[] { "m", "d", "l"},
                     new[] { "an", "so" }),

                    (new[] { "C", "L", "S", "D", "F" },
                     new[] { "ar", "an", "iel", "er", "en" },
                     new[] { "a", "e", "y" , "ie"})
                };
            }

            return _generators;
        }
    }

    public static string GetRandomName()
    {
        var generator = Generators.GetRandomItem();
        return generator.front.GetRandomItem() + generator.mid.GetRandomItem() + generator.end.GetRandomItem();
    }
}
