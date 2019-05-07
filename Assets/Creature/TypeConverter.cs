using System;
using System.Collections.Generic;
using System.ComponentModel;

public static class TypeConverter
{
    public static T ChangeType<T>(this object value)
    {
        return (T)ChangeType(typeof(T), value);
    }

    public static object ChangeType(Type t, object value)
    {
        System.ComponentModel.TypeConverter tc = TypeDescriptor.GetConverter(t);
        return tc.ConvertFrom(value);
    }
    public static void RegisterTypeConverter<T, TC>() where TC : System.ComponentModel.TypeConverter
    {
        TypeDescriptor.AddAttributes(typeof(T), new TypeConverterAttribute(typeof(TC)));
    }

    public static object ConvertToType(Type type, string valueString)
    {
        object value = null;

        var converter = TypeDescriptor.GetConverter(type);
        bool canConvert = converter.CanConvertFrom(valueString.GetType());

        if (canConvert)
        {
            try { value = converter.ConvertFrom(valueString); }
            catch { }
        }

        return value;
    }
}