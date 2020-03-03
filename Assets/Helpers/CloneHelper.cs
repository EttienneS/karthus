using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;

public static class JsonHelper
{
    public static T LoadJson<T>(this string json)
    {
        var traceWriter = new MemoryTraceWriter();

        T obj;
        try
        {
            obj = JsonConvert.DeserializeObject<T>(json, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                NullValueHandling = NullValueHandling.Ignore,
                TraceWriter = traceWriter
            });
        }
        catch (System.Exception)
        {
            Debug.LogError(traceWriter);
            throw;
        }

        return obj;
    }
}

public static class CloneHelper
{
    // from: https://stackoverflow.com/questions/78536/deep-cloning-objects

    /// <summary>
    /// Perform a deep Copy of the object, using Json as a serialisation method. NOTE: Private members are not cloned using this method.
    /// </summary>
    /// <typeparam name="T">The type of object being copied.</typeparam>
    /// <param name="source">The object instance to copy.</param>
    /// <returns>The copied object.</returns>
    public static T CloneJson<T>(this T source)
    {
        // Don't serialize a null object, simply return the default for that object
        if (ReferenceEquals(source, null))
        {
            return default;
        }

        // initialize inner objects individually
        // for example in default constructor some list property initialized with some values,
        // but in 'source' these items are cleaned -
        // without ObjectCreationHandling.Replace default constructor values will be added to result
        var serializeSettings = new JsonSerializerSettings
        {
            ObjectCreationHandling = ObjectCreationHandling.Replace,
            TypeNameHandling = TypeNameHandling.Auto,
            NullValueHandling = NullValueHandling.Ignore,
        };

        var serialized = JsonConvert.SerializeObject(source, serializeSettings);

        return JsonConvert.DeserializeObject<T>(serialized, serializeSettings);
    }
}