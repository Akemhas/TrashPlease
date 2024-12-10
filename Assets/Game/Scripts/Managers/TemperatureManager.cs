using System;
using UnityEngine;

namespace Managers
{
    public class TemperatureManager : Singleton<TemperatureManager>
    {
        [SerializeField] private float _failTemperature = 100;
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
            var newTemperature = Temperature + temp;
            Temperature = Mathf.Clamp(newTemperature, -8, 100);
            TemperatureChanged?.Invoke(newTemperature);
            if(newTemperature > _failTemperature) GameManager.Instance.Fail();
        }
    }
}