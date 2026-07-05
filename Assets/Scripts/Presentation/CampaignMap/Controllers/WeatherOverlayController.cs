using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace ThreeKingdom.Presentation.CampaignMap
{
    /// <summary>
    /// Drives URP post-processing and particle systems to reflect weather state.
    ///
    /// Attach to: CampaignMapScene/Layers/WeatherOverlay
    ///
    /// URP Volume profile should have:
    ///   - Vignette
    ///   - Color Adjustments (saturation, tint)
    ///   - Film Grain (for fog/storm)
    /// </summary>
    public class WeatherOverlayController : MonoBehaviour
    {
        [Header("Particle Systems")]
        [SerializeField] private ParticleSystem _rainParticles;
        [SerializeField] private ParticleSystem _snowParticles;
        [SerializeField] private ParticleSystem _fogParticles;

        [Header("URP Volume")]
        [SerializeField] private Volume _weatherVolume;

        [Header("Config")]
        [SerializeField] private float _fadeDuration = 1.2f;

        // Cached URP override refs
        private Vignette         _vignette;
        private ColorAdjustments _colorAdjustments;
        private FilmGrain        _filmGrain;

        private WeatherType _currentWeather;
        private Coroutine   _fadeCoroutine;

        // ── Lifecycle ──────────────────────────────────────────────────────
        private void Awake()
        {
            if (_weatherVolume != null)
            {
                _weatherVolume.profile.TryGet(out _vignette);
                _weatherVolume.profile.TryGet(out _colorAdjustments);
                _weatherVolume.profile.TryGet(out _filmGrain);
            }
        }

        // ── Public API ─────────────────────────────────────────────────────
        public void Initialise(WeatherType initialWeather)
        {
            ApplyWeatherImmediate(initialWeather);
        }

        public void Refresh(WeatherType newWeather)
        {
            if (newWeather == _currentWeather) return;

            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(CrossfadeWeather(_currentWeather, newWeather));
            _currentWeather = newWeather;
        }

        // ── Implementation ─────────────────────────────────────────────────
        private void ApplyWeatherImmediate(WeatherType weather)
        {
            _currentWeather = weather;
            StopAllParticles();

            switch (weather)
            {
                case WeatherType.Clear:
                    SetVolumeWeight(0f);
                    break;
                case WeatherType.Rain:
                    _rainParticles?.Play();
                    SetVolumeWeight(0.4f);
                    if (_colorAdjustments != null)
                        _colorAdjustments.saturation.value = -15f;
                    break;
                case WeatherType.Snow:
                    _snowParticles?.Play();
                    SetVolumeWeight(0.35f);
                    if (_colorAdjustments != null)
                        _colorAdjustments.colorFilter.value = new Color(0.85f, 0.9f, 1f);
                    break;
                case WeatherType.Fog:
                    _fogParticles?.Play();
                    SetVolumeWeight(0.6f);
                    if (_filmGrain != null) _filmGrain.intensity.value = 0.2f;
                    break;
                case WeatherType.Storm:
                    _rainParticles?.Play();
                    SetVolumeWeight(0.8f);
                    if (_vignette != null)  _vignette.intensity.value   = 0.4f;
                    if (_filmGrain != null) _filmGrain.intensity.value  = 0.35f;
                    break;
            }
        }

        private IEnumerator CrossfadeWeather(WeatherType from, WeatherType to)
        {
            // Fade out current
            var startWeight = _weatherVolume.weight;
            var elapsed     = 0f;
            while (elapsed < _fadeDuration * 0.5f)
            {
                elapsed += Time.deltaTime;
                _weatherVolume.weight = Mathf.Lerp(startWeight, 0f, elapsed / (_fadeDuration * 0.5f));
                yield return null;
            }

            StopAllParticles();
            ApplyWeatherImmediate(to);

            // Fade in new
            elapsed = 0f;
            while (elapsed < _fadeDuration * 0.5f)
            {
                elapsed += Time.deltaTime;
                _weatherVolume.weight = Mathf.Lerp(0f, GetTargetWeight(to), elapsed / (_fadeDuration * 0.5f));
                yield return null;
            }
        }

        private void StopAllParticles()
        {
            _rainParticles?.Stop();
            _snowParticles?.Stop();
            _fogParticles?.Stop();
        }

        private void SetVolumeWeight(float weight) =>
            _weatherVolume.weight = weight;

        private float GetTargetWeight(WeatherType weather) => weather switch
        {
            WeatherType.Clear  => 0f,
            WeatherType.Rain   => 0.4f,
            WeatherType.Snow   => 0.35f,
            WeatherType.Fog    => 0.6f,
            WeatherType.Storm  => 0.8f,
            _                  => 0f
        };
    }
}
