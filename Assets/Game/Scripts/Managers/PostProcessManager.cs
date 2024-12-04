using System;
using Managers;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PostProcessManager : Singleton<PostProcessManager>
{
    [SerializeField] private Volume _globalVolume;
    [SerializeField] private float _temperatureChangeSpeed = 25f;
    [SerializeField, Min(1)] private float _oscillationSpeed = 2f;
    [SerializeField, Range(1, 8)] private float _oscillationSize = 2f;
    [SerializeField] private float _minTemperature;
    [SerializeField] private int _maxTemperature;
    [SerializeField] private TemperatureEffectData _minTemperatureData;
    [SerializeField] private TemperatureEffectData _maxTemperatureData;

    private VolumeProfile _volumeProfile;
    private TemperatureEffect _temperatureEffect;
    private readonly TemperatureEffectData _lerpEffectData = new();

    private float _lerpValue;
    private float _targetTemperature;

    private void Awake()
    {
        _volumeProfile = _globalVolume.profile;
        _volumeProfile.TryGet(out Bloom bloom);
        _volumeProfile.TryGet(out Vignette vignette);
        _volumeProfile.TryGet(out FilmGrain filmGrain);
        _temperatureEffect = new TemperatureEffect(bloom, vignette, filmGrain);
        TemperatureManager.Instance.TemperatureChanged += OnTemperatureChanged;
    }

    private void Update()
    {
        if (_targetTemperature - _lerpValue > .01f)
        {
            _lerpValue = Mathf.Lerp(_lerpValue, _targetTemperature, Time.deltaTime * _temperatureChangeSpeed);
            LerpEffectData(_lerpValue);
        }
        else
        {
            LerpEffectData(_lerpValue * (_oscillationSize + Mathf.Sin(Time.time * _oscillationSpeed) / 10));
        }

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
        private readonly FilmGrain _filmGrain;

        public TemperatureEffect(Bloom bloom, Vignette vignette, FilmGrain filmGrain)
        {
            _bloom = bloom;
            _vignette = vignette;
            _filmGrain = filmGrain;
        }

        public void ChangeTemperature(TemperatureEffectData temperatureEffectData)
        {
            _bloom.intensity.value = temperatureEffectData.BloomIntensity;
            _vignette.intensity.value = temperatureEffectData.VignetteIntensity;
            _filmGrain.intensity.value = temperatureEffectData.FilmGrainIntensity;
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