using UnityEngine;

namespace ThreeKingdom.Presentation.CampaignMap
{
    /// <summary>
    /// ScriptableObject: all tunable Presentation-layer config for the campaign map.
    /// Create via: Assets → Create → ThreeKingdom → CampaignMapConfig
    ///
    /// Follows your existing ScriptableObject-driven config pattern.
    /// </summary>
    [CreateAssetMenu(
        fileName = "CampaignMapConfig",
        menuName  = "ThreeKingdom/CampaignMapConfig",
        order     = 10)]
    public class CampaignMapConfig : ScriptableObject
    {
        [Header("Territory Colours")]
        public Color UnownedTerritoryColor  = new Color(0.7f, 0.65f, 0.55f, 1f);
        public Color SelectionOutlineColor  = Color.yellow;
        public float SelectionOutlineWidth  = 0.05f;

        [Header("Faction Colours")]
        // TODO: wire to your Faction domain enum/ids
        public Color WeiColor  = new Color(0.25f, 0.35f, 0.65f, 1f);   // 魏 — blue
        public Color ShuColor  = new Color(0.18f, 0.52f, 0.30f, 1f);   // 蜀 — green
        public Color WuColor   = new Color(0.72f, 0.20f, 0.18f, 1f);   // 吳 — red
        public Color OtherColor = new Color(0.55f, 0.50f, 0.45f, 1f);  // independent

        [Header("Camera")]
        public float DefaultZoom   = 12f;
        public float MinZoom       = 3f;
        public float MaxZoom       = 25f;
        public float PanSpeed      = 20f;

        [Header("Weather Overlay")]
        public float WeatherOverlayAlpha = 0.35f;
        [Tooltip("Seconds to cross-fade between weather states")]
        public float WeatherFadeDuration = 1.2f;

        [Header("Hero Tokens")]
        public float TokenMoveAnimDuration = 0.4f;
        public float TokenHoverScaleUp     = 1.15f;

        [Header("UI Timing")]
        public float TooltipDelay         = 0.3f;   // seconds before tooltip appears
        public float PanelFadeDuration    = 0.2f;
    }

    // ── Faction colour lookup helper ────────────────────────────────────────
    [CreateAssetMenu(
        fileName = "FactionColourMap",
        menuName  = "ThreeKingdom/FactionColourMap",
        order     = 11)]
    public class FactionColourMap : ScriptableObject
    {
        [System.Serializable]
        public struct FactionColourEntry
        {
            public string FactionId;
            public Color  PrimaryColor;
            public Color  SecondaryColor;
        }

        public FactionColourEntry[] Entries;

        public Color GetColor(string factionId)
        {
            if (string.IsNullOrEmpty(factionId)) return Color.grey;
            foreach (var entry in Entries)
                if (entry.FactionId == factionId) return entry.PrimaryColor;
            return Color.grey;
        }
    }
}
