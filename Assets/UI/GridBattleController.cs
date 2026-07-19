using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using ThreeKingdom.Domain.GridBattle;
using ThreeKingdom.Presentation.Screens;

namespace ThreeKingdom.Unity.UI
{
    /// <summary>
    /// 格子战斗屏控制器（Presentation 薄壳 / ADR-0002，GDD-028 / ADR-0018）——独立战斗场景。
    /// 只读投影 <see cref="GridBattleRuntime.View"/> 渲染地形网格 + 部队图元；点选我军→点格子设目的地；进行/重开。
    /// 逻辑在纯 C# GridBattleEngine/GridBattleSession（dotnet 已单测）。★视觉需编辑器验证；2.5D 等距后续分期。
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class GridBattleController : MonoBehaviour
    {
        private const int Cell = 34;
        private const int TokenSize = 28;
        private string _selected;

        private void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            Wire(root, "gb-advance", () => { GridBattleRuntime.Advance(); _selected = null; Render(root); });
            Wire(root, "gb-reset", () => { GridBattleRuntime.Reset(); _selected = null; Render(root); });
            Wire(root, "gb-menu", () => SceneManager.LoadScene("MainMenu"));

            var map = root.Q<VisualElement>("gb-map");
            if (map != null) map.RegisterCallback<PointerDownEvent>(e => OnMapClick(root, e));

            Render(root);
        }

        /// <summary>点地图：己方单位所在格→选中；已选且点空格→设目的地（命令契约，仅相邻由引擎后续裁决）。</summary>
        private void OnMapClick(VisualElement root, PointerDownEvent e)
        {
            GridBattleView view = GridBattleRuntime.View();
            if (view.IsOver) return;
            int x = (int)(e.localPosition.x / Cell);
            int y = (int)(e.localPosition.y / Cell);
            if (x < 0 || y < 0 || x >= view.Width || y >= view.Height) return;

            foreach (GridUnitView u in view.Units)
                if (u.IsPlayer && u.X == x && u.Y == y) { _selected = u.Id; Render(root); return; }

            if (_selected != null) { GridBattleRuntime.SetDestination(_selected, x, y); Render(root); }
        }

        private void Render(VisualElement root)
        {
            GridBattleView view = GridBattleRuntime.View();

            SetLabel(root, "gb-clock", view.ClockLabel);
            SetLabel(root, "gb-outcome", view.IsOver ? view.OutcomeLabel : string.Empty);
            SetLabel(root, "gb-status", view.IsOver
                ? "点「重开」再来一局。"
                : _selected != null ? "已选一支我军——点一个格子设为它的目的地。"
                : "点一支我军(赤) → 再点格子设目的地 → 点「进行」。");
            SetEnabled(root, "gb-advance", !view.IsOver);

            var map = root.Q<VisualElement>("gb-map");
            if (map != null)
            {
                map.Clear();
                map.style.width = (float)(view.Width * Cell);
                map.style.height = (float)(view.Height * Cell);

                foreach (GridCellView c in view.Cells)
                {
                    var tile = new VisualElement();
                    tile.AddToClassList("gb-cell");
                    tile.style.left = (float)(c.X * Cell);
                    tile.style.top = (float)(c.Y * Cell);
                    tile.style.width = (float)Cell;
                    tile.style.height = (float)Cell;
                    tile.style.backgroundColor = TerrainColor(c.Terrain);
                    tile.pickingMode = PickingMode.Ignore;
                    if (c.IsPlayerGranary) tile.AddToClassList("gb-granary-player");
                    if (c.IsEnemyGranary) tile.AddToClassList("gb-granary-enemy");
                    if (!string.IsNullOrEmpty(c.Glyph))
                    {
                        var g = new Label(c.Glyph);
                        g.AddToClassList("gb-cell-glyph");
                        tile.Add(g);
                    }
                    map.Add(tile);
                }

                int off = (Cell - TokenSize) / 2;
                foreach (GridUnitView u in view.Units)
                {
                    var tok = new VisualElement();
                    tok.AddToClassList("gb-token");
                    tok.AddToClassList(u.IsPlayer ? "gb-token-player" : "gb-token-enemy");
                    if (u.InCover) tok.AddToClassList("gb-token-cover");
                    if (u.Id == _selected) tok.AddToClassList("gb-token-selected");
                    tok.style.left = (float)(u.X * Cell + off);
                    tok.style.top = (float)(u.Y * Cell + off);
                    tok.style.width = TokenSize;
                    tok.style.height = TokenSize;
                    tok.pickingMode = PickingMode.Ignore; // 点击落到地图格定位
                    var glyph = new Label(u.KindGlyph);
                    glyph.AddToClassList("gb-token-glyph");
                    tok.Add(glyph);
                    var nm = new Label($"{u.Name} {u.Strength}");
                    nm.AddToClassList("gb-token-name");
                    tok.Add(nm);
                    map.Add(tok);
                }
            }

            var supply = root.Q<VisualElement>("gb-supply");
            if (supply != null)
            {
                supply.Clear();
                supply.Add(SupplyRow("我军", view.PlayerSupply));
                supply.Add(SupplyRow("敌军", view.EnemySupply));
            }

            var log = root.Q<VisualElement>("gb-log");
            if (log != null)
            {
                log.Clear();
                var enc = GridBattleRuntime.LastEncounters;
                if (!view.IsOver && enc.Count > 0)
                {
                    var head = new Label("半路遭遇！临机抉择：");
                    head.style.unityFontStyleAndWeight = FontStyle.Bold;
                    log.Add(head);
                    foreach (string id in enc)
                    {
                        var row = new VisualElement();
                        row.style.flexDirection = FlexDirection.Row;
                        row.style.flexWrap = Wrap.Wrap;
                        row.Add(new Label(id));
                        row.Add(EncounterButton(root, id, "继续", EncounterChoice.Continue));
                        row.Add(EncounterButton(root, id, "据守", EncounterChoice.Hold));
                        row.Add(EncounterButton(root, id, "后撤", EncounterChoice.Retreat));
                        log.Add(row);
                    }
                }
                else
                {
                    log.Add(new Label(view.IsOver ? view.OutcomeLabel : "推进「进行」，相邻格敌我交战；断敌粮、入隘伏。"));
                }
            }
        }

        private Button EncounterButton(VisualElement root, string unitId, string text, EncounterChoice choice)
        {
            var b = new Button(() =>
            {
                GridBattleRuntime.ApplyEncounter(unitId, choice);
                GridBattleRuntime.ClearEncounters();
                Render(root);
            })
            { text = text };
            return b;
        }

        private static VisualElement SupplyRow(string name, int val)
        {
            var row = new VisualElement();
            row.AddToClassList("gb-supply-row");
            var lab = new VisualElement();
            lab.AddToClassList("gb-supply-lab");
            lab.Add(new Label(name));
            lab.Add(new Label(val.ToString()));
            row.Add(lab);
            var bar = new VisualElement();
            bar.AddToClassList("gb-supply-bar");
            var fill = new VisualElement();
            fill.AddToClassList("gb-supply-fill");
            fill.style.width = Length.Percent(Mathf.Clamp(val, 0, 100));
            fill.style.backgroundColor = val >= 50 ? new Color(0.29f, 0.48f, 0.25f)
                : val < 25 ? new Color(0.70f, 0.23f, 0.18f) : new Color(0.78f, 0.54f, 0.18f);
            bar.Add(fill);
            row.Add(bar);
            return row;
        }

        private static Color TerrainColor(TerrainKind t)
        {
            switch (t)
            {
                case TerrainKind.Mountain: return new Color(0.66f, 0.60f, 0.48f);
                case TerrainKind.Pass: return new Color(0.925f, 0.816f, 0.659f);
                case TerrainKind.Forest: return new Color(0.725f, 0.776f, 0.60f);
                case TerrainKind.Granary: return new Color(0.941f, 0.863f, 0.627f);
                default: return new Color(0.905f, 0.847f, 0.71f);
            }
        }

        private static void Wire(VisualElement root, string name, Action handler)
        {
            var button = root.Q<Button>(name);
            if (button != null) button.clicked += handler;
        }

        private static void SetLabel(VisualElement root, string name, string text)
        {
            var label = root.Q<Label>(name);
            if (label != null) label.text = text;
        }

        private static void SetEnabled(VisualElement root, string name, bool enabled)
        {
            var button = root.Q<Button>(name);
            if (button != null) button.SetEnabled(enabled);
        }
    }
}
