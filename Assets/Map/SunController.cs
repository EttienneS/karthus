using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunController : MonoBehaviour
{
    internal DayState State = DayState.Night;

    internal const int SunHeight = -100;
    internal const int Sunrise = 5;
    internal const int Sunset = 19;

    internal const float BaseIntensity = 0.4f;
    internal const float MaxIntensity = 1.3f;

    internal const float DayLightHours = Sunset - Sunrise;

    internal int MidDay = Sunrise + Mathf.FloorToInt(DayLightHours / 2);

    internal float IntensityPerHour = MaxIntensity / (DayLightHours / 2);

    internal Light Light;
    private static SunController _instance;

    public enum DayState
    {
        SunRise, Morning, Noon, AfterNoon, Night
    }

    public static SunController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.Find("SunController").GetComponent<SunController>();
            }

            return _instance;
        }
    }

    internal float JumpDistance
    {
        get
        {
            return MapGrid.Instance.Map.GetLength(0) / DayLightHours;
        }
    }
    internal void UpdatePosition(int hour, int minutes)
    {
        var minutePercentage = minutes / 60f;
        if (hour >= Sunrise && hour < Sunset)
        {
            transform.position = new Vector3((hour - Sunrise) * JumpDistance,
                                             (hour - Sunrise) * JumpDistance, SunHeight);

            if (hour < MidDay)
            {
                Light.intensity = GetCurrentIntensity(hour - Sunrise, minutePercentage) + BaseIntensity;
                Light.intensity = Mathf.Clamp(Light.intensity, BaseIntensity, MaxIntensity);

                State = hour <= Sunrise + 1 ? DayState.SunRise : DayState.Morning;
            }
            else
            {
                Light.intensity = Math.Max(MaxIntensity - GetCurrentIntensity(hour - MidDay, minutePercentage) + BaseIntensity, BaseIntensity * 1.5f);
                Light.intensity = Mathf.Clamp(Light.intensity, BaseIntensity * 2f, MaxIntensity);

                State = hour <= MidDay + 1 ? DayState.Noon : DayState.AfterNoon;
            }

        }
        else
        {
            State = DayState.Night;
            transform.position = new Vector3((DayLightHours / 2) * JumpDistance, (DayLightHours / 2) * JumpDistance, SunHeight);
            Light.intensity = Math.Max(Light.intensity - (IntensityPerHour * minutePercentage), BaseIntensity);
        }
    }

    public float GetCurrentIntensity(int hour, float minutePercentage)
    {
        return ((hour - 1) * IntensityPerHour) + (IntensityPerHour * minutePercentage);
    }

    // Update is called once per frame
    void Start()
    {
        Light = GetComponent<Light>();
    }
}
