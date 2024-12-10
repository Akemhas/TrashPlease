using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PostProcessManager : MonoBehaviour
{
    [SerializeField] private Volume _globalVolume;
    [SerializeField] private float _temperatureChangeSpeed = 25f;

    [SerializeField] private float _minWaveEffect = 0.5f;
    [SerializeField] private float _maxWaveEffect = 1f;

    [SerializeField] private float _minTemperature;
    [SerializeField] private int _maxTemperature;

    [SerializeField] private TemperatureEffectData _minTemperatureData;
    [SerializeField] private TemperatureEffectData _maxTemperatureData;

    private VolumeProfile _volumeProfile;
    private TemperatureEffect _temperatureEffect;
    private readonly TemperatureEffectData _lerpEffectData = new();

    private const float LerpThreshold = 0.01f;

    private float _oscillationSpeed;
    private float _lerpValue;
    private float _targetTemperature;

    private void Awake()
    {
        _volumeProfile = _globalVolume.profile;
        _volumeProfile.TryGet(out Bloom bloom);
        _volumeProfile.TryGet(out Vignette vignette);
        _temperatureEffect = new TemperatureEffect(bloom, vignette);
    }

    private void OnEnable()
    {
        TemperatureManager.TemperatureChanged += OnTemperatureChanged;
    }

    private void OnDisable()
    {
        TemperatureManager.TemperatureChanged -= OnTemperatureChanged;
    }

    private void Update()
    {
        if (Mathf.Abs(_targetTemperature - _lerpValue) > LerpThreshold)
        {
            _lerpValue = Mathf.Lerp(_lerpValue, _targetTemperature, Time.deltaTime * _temperatureChangeSpeed);
        }

        var sinWave = Mathf.Abs(Mathf.Sin(Time.time * _oscillationSpeed));
        var waveEffect = Mathf.Lerp(_minWaveEffect, _maxWaveEffect, sinWave);
        var oscillationValue = _lerpValue * waveEffect;
        LerpEffectData(oscillationValue);

        _temperatureEffect.ChangeTemperature(_lerpEffectData);
    }

    private void OnTemperatureChanged(float temperature)
    {
        _targetTemperature = (temperature - _minTemperature) / (_maxTemperature - _minTemperature);
    }

    private void LerpEffectData(float lerpValue)
    {
        _lerpEffectData.BloomIntensity = Mathf.Lerp(_minTemperatureData.BloomIntensity, _maxTemperatureData.BloomIntensity, lerpValue);
        _lerpEffectData.VignetteIntensity = Mathf.Lerp(_minTemperatureData.VignetteIntensity, _maxTemperatureData.VignetteIntensity, lerpValue);
        _lerpEffectData.FilmGrainIntensity = Mathf.Lerp(_minTemperatureData.FilmGrainIntensity, _maxTemperatureData.FilmGrainIntensity, lerpValue);
    }

    [Serializable]
    private class TemperatureEffect
    {
        private readonly Bloom _bloom;
        private readonly Vignette _vignette;

        public TemperatureEffect(Bloom bloom, Vignette vignette)
        {
            _bloom = bloom;
            _vignette = vignette;
        }

        public void ChangeTemperature(TemperatureEffectData temperatureEffectData)
        {
            _bloom.intensity.value = temperatureEffectData.BloomIntensity;
            _vignette.intensity.value = temperatureEffectData.VignetteIntensity;
        }
    }

    [Serializable]
    private class TemperatureEffectData
    {
        public float BloomIntensity;
        public float VignetteIntensity;
        public float FilmGrainIntensity;
    }
}