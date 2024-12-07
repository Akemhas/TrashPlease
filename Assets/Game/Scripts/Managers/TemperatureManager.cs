using System;
using UnityEngine;

namespace Managers
{
    public class TemperatureManager : Singleton<TemperatureManager>
    {
        public event Action<float> TemperatureChanged;

        public float Temperature
        {
            get => PlayerPrefs.GetFloat(nameof(Temperature), 0f);
            private set => PlayerPrefs.SetFloat(nameof(Temperature), value);
        }

        private void Start()
        {
            TemperatureChanged?.Invoke(Temperature);
        }

#if UNITY_EDITOR
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.T)) IncreaseTemperature(5f);
            if (Input.GetKeyDown(KeyCode.Y)) IncreaseTemperature(-5f);
        }
#endif

        public void IncreaseTemperature(float temp)
        {
            Temperature = Mathf.Clamp(Temperature + temp, -8, 100);
            TemperatureChanged?.Invoke(Temperature);
        }
    }
}