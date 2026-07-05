using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ThreeKingdom.Presentation.CampaignMap
{
    /// <summary>
    /// Manages hero token GameObjects on the campaign map.
    /// Handles spawn, movement animation, and click delegation.
    ///
    /// Attach to: CampaignMapScene/Layers/HeroTokenLayer GameObject.
    /// </summary>
    public class HeroTokenLayerController : MonoBehaviour
    {
        public event Action<HeroTokenViewModel> OnHeroTokenClicked;

        [Header("Prefabs")]
        [SerializeField] private HeroTokenView _heroTokenPrefab;

        [Header("Movement")]
        [SerializeField] private float _moveAnimDuration = 0.4f;
        [SerializeField] private AnimationCurve _moveEase = AnimationCurve.EaseInOut(0, 0, 1, 1);

        // ── Internal ───────────────────────────────────────────────────────
        private readonly Dictionary<string, HeroTokenView> _tokens = new();

        // TODO: inject territory world-position lookup
        // private ITerritoryPositionLookup _positionLookup;

        // ── Public API ─────────────────────────────────────────────────────
        public void Initialise(IReadOnlyList<HeroPositionViewModel> heroPositions)
        {
            foreach (var hp in heroPositions)
                SpawnToken(hp);
        }

        public void MoveHeroToken(string heroId, string fromTerritoryId, string toTerritoryId)
        {
            if (!_tokens.TryGetValue(heroId, out var token)) return;

            // TODO: resolve world positions from territory IDs
            // var destination = _positionLookup.GetWorldPosition(toTerritoryId);
            var destination = Vector3.zero; // Replace with real lookup

            StartCoroutine(AnimateMoveToken(token, destination));
        }

        // ── Spawn ──────────────────────────────────────────────────────────
        private void SpawnToken(HeroPositionViewModel hp)
        {
            var token = Instantiate(_heroTokenPrefab, transform);
            token.name = $"Hero_{hp.HeroId}_{hp.HeroNameChinese}";

            // TODO: resolve initial world position from territory ID
            // token.transform.position = _positionLookup.GetWorldPosition(hp.TerritoryId);
            token.transform.position = hp.InitialWorldPosition;

            token.Initialise(hp);
            token.OnClicked += () => OnHeroTokenClicked?.Invoke(hp.ToViewModel());

            _tokens[hp.HeroId] = token;
        }

        // ── Animation ──────────────────────────────────────────────────────
        private IEnumerator AnimateMoveToken(HeroTokenView token, Vector3 destination)
        {
            token.SetMoving(true);
            var start   = token.transform.position;
            var elapsed = 0f;

            while (elapsed < _moveAnimDuration)
            {
                elapsed += Time.deltaTime;
                var t = _moveEase.Evaluate(elapsed / _moveAnimDuration);
                token.transform.position = Vector3.Lerp(start, destination, t);
                yield return null;
            }

            token.transform.position = destination;
            token.SetMoving(false);
        }
    }
}
