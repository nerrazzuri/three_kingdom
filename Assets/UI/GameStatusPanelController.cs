using UnityEngine;
using UnityEngine.UIElements;
using ThreeKingdom.Presentation.Screens;

namespace ThreeKingdom.Unity.UI
{
    /// <summary>
    /// 顶栏聚合状态屏控制器（把此前无 Unity 屏的系统——君命/生涯/行动容量/人才——一次 surfaced）。
    /// 绑定 <see cref="SessionRuntime.HudSummary"/>（GameHudView 单一投影，dotnet 已单测）+ 人才录（反全知无数值）。
    /// 纯薄壳，逻辑在已测的 CampaignRuntime。★这是 Unity MonoBehaviour，需在编辑器验证绑定/样式。
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class GameStatusPanelController : MonoBehaviour
    {
        private void OnEnable() => Render();

        /// <summary>外部（推进/命令后）调用以刷新。</summary>
        public void Render()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            GameHudView h = SessionRuntime.HudSummary();

            SetText(root, "status-era", $"公元{h.Year}·{h.SeasonLabel}");
            SetText(root, "status-life", $"{h.Age}岁·{h.LifePhaseLabel}{(h.IsLifeOver ? "（寿终·待传承）" : "")}");
            string aff = h.IsUnaffiliated ? "（在野）" : "";
            string reb = h.HasRebelled ? "【自立·无退路】" : "";
            SetText(root, "status-career", $"{h.RankTitle}{aff}{reb}（功{h.Merit}/名望{h.Renown}）");
            SetText(root, "status-agents", $"手令 {h.ActionsInFlight}/{h.ActionCapacity}");
            SetText(root, "status-contention", $"据{h.PlayerCities}城·群雄{h.AliveRivals}家{(h.IsEliminated ? "·【覆灭·待发落】" : "")}");
            SetText(root, "status-mission", h.MissionOrder);

            var list = root.Q<ScrollView>("talent-list");
            if (list != null)
            {
                list.Clear();
                foreach (TalentRecruitLine t in SessionRuntime.TalentView().Talents)
                    list.Add(new Label($"{t.Name}〔{t.SpecialtyLabel}·{t.DifficultyLabel}〕"));
            }
        }

        private static void SetText(VisualElement root, string name, string text)
        {
            var l = root.Q<Label>(name);
            if (l != null) l.text = text;
        }
    }
}
