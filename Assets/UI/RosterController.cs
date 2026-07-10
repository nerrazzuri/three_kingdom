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

                // 横向卡片：立绘缩略图 + 名/性情（反全知——立绘不泄任何数值/隐藏档）。
                var row = new VisualElement();
                row.AddToClassList("roster-row");
                row.style.flexDirection = FlexDirection.Row;
                row.style.alignItems = Align.Center;
                row.style.marginTop = 4;
                row.style.marginBottom = 4;

                // 立绘位（美术圣经 §3.1：64×85px 缩略图，脸为焦点）。
                // 用 Texture2D + Image.image（PNG 默认导入即 Texture2D，无需 Sprite 模式）；
                // 有图者显示立绘，其余以中性剪影底占位（不泄信息）。
                var portrait = new Image { scaleMode = ScaleMode.ScaleToFit };
                portrait.style.width = 64;
                portrait.style.height = 85;
                portrait.style.flexShrink = 0;
                portrait.style.marginRight = 12;
                portrait.style.backgroundColor = new Color(0.16f, 0.15f, 0.13f, 1f); // 剪影占位底
                var tex = Resources.Load<Texture2D>($"Portraits/{card.Name}");
                if (tex != null) portrait.image = tex;
                row.Add(portrait);

                var label = new Label(sb.ToString());
                label.AddToClassList("roster-row-label");
                label.style.whiteSpace = WhiteSpace.Normal;
                label.style.flexGrow = 1;
                row.Add(label);

                list.Add(row);
            }

            var back = root.Q<Button>("roster-back");
            if (back != null) back.clicked += () => SceneManager.LoadScene(_backScene);
        }
    }
}
