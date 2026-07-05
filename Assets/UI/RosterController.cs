using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using ThreeKingdom.Presentation.Screens;

namespace ThreeKingdom.Unity.UI
{
    /// <summary>
    /// 武将录屏控制器（GDD_026 #2 / GDD_025 R1，Presentation 薄壳）：列全体武将目录卡——<b>只呈中文名 + 气质性情</b>，
    /// 绝不投影数值/隐藏档/隐秘心（反全知）。数据经 <see cref="SessionRuntime.Roster"/>（dotnet 已单测）。★需 Unity 编辑器验证。
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class RosterController : MonoBehaviour
    {
        [SerializeField] private string _backScene = "Hud";

        private void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            var list = root.Q<ScrollView>("roster-list");
            list.Clear();

            foreach (GeneralCardView card in SessionRuntime.Roster().Cards)
            {
                var sb = new StringBuilder(card.Name);
                if (card.Traits.Count > 0)
                {
                    sb.Append("　〔");
                    for (int i = 0; i < card.Traits.Count; i++)
                    {
                        if (i > 0) sb.Append('·');
                        sb.Append(card.Traits[i]);
                    }
                    sb.Append("〕");
                }
                else sb.Append("　〔泛泛之辈〕");

                var row = new Label(sb.ToString());
                row.AddToClassList("roster-row");
                list.Add(row);
            }

            var back = root.Q<Button>("roster-back");
            if (back != null) back.clicked += () => SceneManager.LoadScene(_backScene);
        }
    }
}
