using System;
using UnityEngine;

public static class VectorHelper
{
    public static Vector3 ToVector3(this string input)
    {
        var parts = input.Split(',');
        switch (parts.Length)
        {
            case 1:
                return Vector3.one * parts[0].ToFloat();

            case 2:
                return new Vector3(parts[0].ToFloat(), parts[1].ToFloat());

            case 3:
                return new Vector3(parts[0].ToFloat(), parts[1].ToFloat(), parts[2].ToFloat());

            default:
                throw new InvalidCastException("Input not in correct format: value,value,value");
        }
    }
}