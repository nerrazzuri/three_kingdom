using System;
using System.Collections.Generic;
using UnityEngine;
using ThreeKingdom.Presentation.Screens;

namespace ThreeKingdom.Unity.UI
{
    /// <summary>
    /// 新手引导偏好的进程内/持久壳（epic-028 story-005）：记录「已见提示」与「是否关闭引导」。
    /// <b>表现层偏好，经 PlayerPrefs 持久，绝不进权威会话存档体</b>（引导不改 Domain 状态哈希，见 OnboardingViewModelTests）。
    /// 纯 Unity 侧薄壳；引导展示逻辑在纯 C# <see cref="OnboardingHints"/>（dotnet 已单测）。
    /// </summary>
    public static class OnboardingRuntime
    {
        private const string SeenKey = "tk.onboarding.seen";
        private const string DisabledKey = "tk.onboarding.disabled";

        /// <summary>是否已关闭引导（关闭后一律不打扰）。</summary>
        public static bool Disabled
        {
            get => PlayerPrefs.GetInt(DisabledKey, 0) == 1;
            set { PlayerPrefs.SetInt(DisabledKey, value ? 1 : 0); PlayerPrefs.Save(); }
        }

        /// <summary>已见提示集（从 PlayerPrefs 反序列化枚举名）。</summary>
        public static IReadOnlyCollection<OnboardingCue> Seen()
        {
            var set = new HashSet<OnboardingCue>();
            string csv = PlayerPrefs.GetString(SeenKey, string.Empty);
            foreach (string part in csv.Split(','))
                if (!string.IsNullOrEmpty(part) && Enum.TryParse(part, out OnboardingCue cue))
                    set.Add(cue);
            return set;
        }

        /// <summary>是否已见某提示。</summary>
        public static bool HasSeen(OnboardingCue cue) => ((HashSet<OnboardingCue>)Seen()).Contains(cue);

        /// <summary>登记某提示为已见（显示后调用，避免重复打扰）。</summary>
        public static void MarkSeen(OnboardingCue cue)
        {
            var set = new HashSet<OnboardingCue>(Seen()) { cue };
            PlayerPrefs.SetString(SeenKey, string.Join(",", set));
            PlayerPrefs.Save();
        }

        /// <summary>重置引导偏好（供「重新体验新手引导」或测试）。</summary>
        public static void Reset()
        {
            PlayerPrefs.DeleteKey(SeenKey);
            PlayerPrefs.DeleteKey(DisabledKey);
            PlayerPrefs.Save();
        }
    }
}
