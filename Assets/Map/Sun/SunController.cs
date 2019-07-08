using System;
using System.Collections.Generic;
using UnityEngine;

public class SunController : MonoBehaviour
{
    internal const float BaseIntensity = 0.5f;
    internal const float MaxIntensity = 1f;

    internal const int Sunrise = 4;
    internal const int Sunset = 17;
    internal const float DayLightHours = Sunset - Sunrise;

    internal float IntensityPerHour = MaxIntensity / (DayLightHours / 2);
    internal Light Sun;
    internal int MidDay = Sunrise + Mathf.FloorToInt(DayLightHours / 2);
    internal DayState State = DayState.Night;

    public enum DayState
    {
        SunRise, Morning, Noon, AfterNoon, Night
    }

    public float GetCurrentIntensity(int hour, float minutePercentage)
    {
        return ((hour - 1) * IntensityPerHour) + (IntensityPerHour * minutePercentage);
    }

    internal void UpdateSun(int hour, int minutes)
    {
        var minutePercentage = minutes / 60f;
        if (hour >= Sunrise && hour < Sunset)
        {
            if (hour < MidDay)
            {
                Sun.intensity = GetCurrentIntensity(hour - Sunrise, minutePercentage) + BaseIntensity;
                Sun.intensity = Mathf.Clamp(Sun.intensity, BaseIntensity, MaxIntensity);

                State = hour <= Sunrise + 1 ? DayState.SunRise : DayState.Morning;
            }
            else
            {
                Sun.intensity = Math.Max(MaxIntensity - GetCurrentIntensity(hour - MidDay, minutePercentage) + BaseIntensity, BaseIntensity * 1.5f);
                Sun.intensity = Mathf.Clamp(Sun.intensity, BaseIntensity * 2f, MaxIntensity);

                State = hour <= MidDay + 1 ? DayState.Noon : DayState.AfterNoon;
            }
        }
        else
        {
            State = DayState.Night;
            Sun.intensity = Math.Max(Sun.intensity - (IntensityPerHour * minutePercentage), BaseIntensity);
        }

    }

    private void Start()
    {
        transform.position = new Vector3(0, 0);
        Sun = GetComponent<Light>();
    }
}
