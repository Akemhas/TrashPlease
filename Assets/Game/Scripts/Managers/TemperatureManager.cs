using System;
using UnityEngine;

public static class TemperatureManager
{
    private const float FailTemperature = 100;
    public static event Action<float> TemperatureChanged;

    public static float Temperature
    {
        get => PlayerPrefs.GetFloat(nameof(Temperature), 0f);
        private set => PlayerPrefs.SetFloat(nameof(Temperature), value);
    }

    private static void Start()
    {
        TemperatureChanged?.Invoke(Temperature);
    }

    public static void IncreaseTemperature(float temp)
    {
        var newTemperature = Temperature + temp;
        Temperature = Mathf.Clamp(newTemperature, -8, 100);
        TemperatureChanged?.Invoke(newTemperature);
        if (newTemperature > FailTemperature) GameManager.Instance.Fail();
    }
}