using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
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

    public static object LoadJson(this string json, Type type)
    {
        var traceWriter = new MemoryTraceWriter();

        object obj;
        try
        {
            obj = JsonConvert.DeserializeObject(json, type, new JsonSerializerSettings
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
