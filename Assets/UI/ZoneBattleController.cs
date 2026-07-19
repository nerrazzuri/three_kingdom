using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using ThreeKingdom.Presentation.Screens;
using ThreeKingdom.Domain.ZoneBattle;

namespace ThreeKingdom.Unity.UI
{
    /// <summary>
    /// 区域战斗屏控制器（Presentation 薄壳 / ADR-0002，GDD_021 §12 / visual-battle-scene GDD）——<b>独立战斗场景</b>。
    /// 车道方格战场 + 队列旗帜兵力 + 将领名牌（P1）；排兵布阵（键盘可达兜底列表）+ 亲自打/AI 代打/结算。
    /// 逻辑在纯 C# ZoneBattleRuntime（dotnet 已单测）；本壳只按 name 渲染。P2 反全知雾+姿态、P3 涌现标记+地形分期接入。
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class ZoneBattleController : MonoBehaviour
    {
        // 兵力图元映射（visual-battle-scene GDD §4 F1）。
        private const float IconUnitFraction = 0.15f;
        private const int IconCap = 7;

        // ── M1 真地图化：区域锚点表（zone-id 去前缀 → 地图归一化坐标 0..1，x 左→右 / y 上→下）。
        // 敌方在上/远（关城正面、粮道居敌后、高地在上），我方在下（预备后方）。纯呈现层布局（float 合 ADR-0004）。
        // ★ 数据驱动：真战场地图一到，只调这张表的坐标即可把节点重贴到新地形上，无需动布局代码。
        private static readonly Dictionary<string, Vector2> ZoneAnchors = new()
        {
            ["cover"] = new Vector2(0.30f, 0.18f),   // 遮蔽高地——上方高地
            ["supply"] = new Vector2(0.82f, 0.24f),  // 敌粮道——敌后
            ["front"] = new Vector2(0.52f, 0.44f),   // 正面关城——中央目标区★
            ["flank"] = new Vector2(0.16f, 0.52f),   // 侧翼隘口——侧翼
            ["reserve"] = new Vector2(0.50f, 0.82f), // 预备后方——我方后方
        };

        // 邻接连线（对齐 Domain StandardAdjacency 的 6 条路；连线画在 zb-edges 层，节点之下）。
        private static readonly (string A, string B)[] Edges =
        {
            ("reserve", "front"), ("front", "flank"), ("front", "cover"),
            ("reserve", "cover"), ("cover", "supply"), ("flank", "supply"),
        };

        // M2/M3：持久的兵马图元（detachmentId→图元；跨帧复用→换区时 left/top 变化触发 transition"行军"）。
        private readonly Dictionary<string, VisualElement> _troops = new();
        // 当前合法拖放落点（"detId|zoneId"）——每帧由 view.MoveOptions 刷新，拖放松手时校验。
        private readonly HashSet<string> _moveTargets = new();

        private void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            // 战场底图（坚城占位）铺在 zb-map（ZoneBattle.uss .zb-map）；区域节点绝对定位其上。
            SetLabel(root, "zb-adjacency",
                "邻接（可调动路径）：预备—正面 · 正面—侧翼 · 正面—遮蔽 · 预备—遮蔽 · 遮蔽—粮道 · 侧翼—粮道");

            // 邻接连线层：Painter2D 描夯土路（节点之下）。地图尺寸就绪/变化时重绘。
            var edges = root.Q<VisualElement>("zb-edges");
            if (edges != null)
            {
                edges.generateVisualContent += ctx => DrawEdges(ctx, edges);
                edges.RegisterCallback<GeometryChangedEvent>(_ => edges.MarkDirtyRepaint());
            }

            Render(root);

            Wire(root, "zb-resolve", () => { if (!ZoneBattleSession.IsOver) ZoneBattleSession.ResolveRound(); Render(root); });
            Wire(root, "zb-auto", () => { if (!ZoneBattleSession.IsOver) ZoneBattleSession.AutoResolve(); Render(root); });
            Wire(root, "zb-conclude", () =>
            {
                if (!ZoneBattleSession.IsOver) return;
                ZoneBattleSession.Conclude();                        // 出征→占城/退兵（权威）；守城→守土成败
                SceneManager.LoadScene(ZoneBattleSession.ReturnScene); // 返回来源场景（HUD）
            });
            Wire(root, "zb-to-menu", () => SceneManager.LoadScene("MainMenu"));
        }

        /// <summary>渲染当前战斗投影（纯读；同态渲染恒等）。</summary>
        private void Render(VisualElement root)
        {
            ZoneBattleView view = ZoneBattleSession.View();

            SetLabel(root, "zb-round", $"第 {view.Round} / {view.MaxRounds} 回合");
            SetLabel(root, "zb-title", ZoneBattleSession.Current switch
            {
                ZoneBattleSession.Mode.Offensive => "出征 · 攻城战 — 战场区域",
                ZoneBattleSession.Mode.Defense => "守城迎敌 — 战场区域",
                _ => "演武 — 战场区域",
            });
            SetLabel(root, "zb-outcome", view.IsOver ? view.OutcomeLabel : string.Empty);
            SetLabel(root, "zb-result", view.IsOver ? "点「结算战果并返回」收兵。" : string.Empty);

            SetEnabled(root, "zb-resolve", !view.IsOver);
            SetEnabled(root, "zb-auto", !view.IsOver);
            SetEnabled(root, "zb-conclude", view.IsOver);

            // 各区态势 → 地图分层：地点标记（固定 zb-regions）+ 兵马图元（可移动/可拖 zb-troops）。
            var regions = root.Q<VisualElement>("zb-regions");
            if (regions != null)
            {
                regions.Clear(); // 地点标记纯静态内容，逐帧重建（不含兵马，故不打断行军补间）。
                foreach (ZoneLineView z in view.Zones)
                {
                    if (!ZoneAnchors.TryGetValue(StripZonePrefix(z.ZoneId), out Vector2 anchor)) continue;
                    var marker = new VisualElement();
                    marker.AddToClassList("zb-node");
                    marker.style.left = Length.Percent(anchor.x * 100f);
                    marker.style.top = Length.Percent(anchor.y * 100f);
                    BuildRegionMarker(marker, z);
                    regions.Add(marker);
                }
            }
            ReconcileTroops(root, view);                       // 兵马持久复用 + 换区补间行军
            root.Q<VisualElement>("zb-edges")?.MarkDirtyRepaint();

            // 涌现横幅（醒目一次性）+ 侧栏涌现记录。
            var banner = root.Q<VisualElement>("zb-emergence-banner");
            if (banner != null)
            {
                banner.Clear();
                foreach (string e in view.Emergences)
                {
                    var b = new Label("⚡ " + e);
                    b.style.unityFontStyleAndWeight = FontStyle.Bold;
                    b.style.color = new Color(0.78f, 0.30f, 0.12f);
                    b.style.whiteSpace = WhiteSpace.Normal;
                    banner.Add(b);
                }
            }

            var emerg = root.Q<VisualElement>("zb-emergences");
            if (emerg != null)
            {
                emerg.Clear();
                if (view.Emergences.Count == 0) emerg.Add(new Label("　尚无涌现——兵法条件按区按回合成型。"));
                foreach (string e in view.Emergences) emerg.Add(new Label("　⚡ " + e));
            }

            // 排兵布阵（键盘可达兜底列表，R6 双轨之一）。
            var moves = root.Q<VisualElement>("zb-moves");
            if (moves != null)
            {
                moves.Clear();
                if (view.IsOver)
                    moves.Add(new Label("　战斗已终局。"));
                else if (view.MoveOptions.Count == 0)
                    moves.Add(new Label("　暂无可调动支队（在途/溃散或无相邻空档）。"));
                else
                    foreach (ZoneMoveOption m in view.MoveOptions)
                    {
                        var captured = m;
                        moves.Add(new Button(() => { ZoneBattleSession.Move(captured.DetachmentId, captured.TargetZoneId); Render(root); })
                        {
                            text = $"{captured.DetachmentLabel} → {captured.TargetZoneLabel}",
                        });
                    }
            }
        }

        /// <summary>地点标记（固定于锚点）：标题(★) + 敌方兵力/将领(反全知) + 已成条件徽章。我方兵马已拆为独立可移动图元。</summary>
        private void BuildRegionMarker(VisualElement marker, ZoneLineView z)
        {
            marker.Clear();
            marker.EnableInClassList("zb-node-objective", z.IsObjective);

            var title = new Label((z.IsObjective ? "★ " : "") + z.ZoneLabel);
            title.AddToClassList("zb-zone-title");
            marker.Add(title);

            // 敌方兵力（反全知：已侦察=精确数；未侦察=区间估计"约 X–Y"+旗帜半透+问号，F2）。
            marker.Add(BuildForceRow($"敌 {z.EnemyStrengthLabel}", z.EnemyStrength, z.ZoneCapacity, enemy: true, fogged: !z.EnemyRevealed));
            foreach (string cmd in z.EnemyCommanders)
                marker.Add(BuildNameplate(cmd, cmd, enemy: true));

            // 已成条件徽章（涌现兵法·形状+色，推测语气；不剧透未成型）。
            foreach (string c in z.FormedConditions)
                marker.Add(BuildConditionBadge(c));
        }

        /// <summary>
        /// 调和兵马图元（M2/M3）：按 detachmentId 持久复用元素；把每支队摆到其所在区锚点（下方错开），
        /// 换区时 left/top 变化触发 USS transition → 兵马<b>沿路补间"行军"</b>；离场支队图元移除。同时刷新合法拖放落点集。
        /// </summary>
        private void ReconcileTroops(VisualElement root, ZoneBattleView view)
        {
            var troops = root.Q<VisualElement>("zb-troops");
            if (troops == null) return;

            _moveTargets.Clear();
            foreach (ZoneMoveOption m in view.MoveOptions)
                _moveTargets.Add(m.DetachmentId + "|" + m.TargetZoneId);

            var seen = new HashSet<string>();
            foreach (ZoneLineView z in view.Zones)
            {
                if (!ZoneAnchors.TryGetValue(StripZonePrefix(z.ZoneId), out Vector2 anchor)) continue;
                int i = 0;
                foreach (OwnUnitView u in z.OwnUnits)
                {
                    seen.Add(u.DetachmentId);
                    // 目标锚点：区锚点下方按序错开，避免同区多队重叠。
                    float tx = anchor.x;
                    float ty = Mathf.Min(0.97f, anchor.y + 0.06f + i * 0.075f);
                    i++;

                    if (!_troops.TryGetValue(u.DetachmentId, out VisualElement chip))
                    {
                        chip = new VisualElement();
                        chip.AddToClassList("zb-troop");
                        chip.AddToClassList("zb-troop-new"); // 首帧不从(0,0)滑入
                        troops.Add(chip);
                        _troops[u.DetachmentId] = chip;
                        AttachDrag(chip, u.DetachmentId, root);
                        chip.schedule.Execute(() => chip.RemoveFromClassList("zb-troop-new")).ExecuteLater(60);
                    }
                    // 设目标位置（新建=无补间就位；已存在且换区=transition 行军过去）。
                    chip.style.left = Length.Percent(tx * 100f);
                    chip.style.top = Length.Percent(ty * 100f);
                    BuildTroopContent(chip, u, z.ZoneCapacity, root);
                }
            }

            // 移除已离场（阵亡/并入）的支队图元。
            var stale = new List<string>();
            foreach (KeyValuePair<string, VisualElement> kv in _troops)
                if (!seen.Contains(kv.Key)) stale.Add(kv.Key);
            foreach (string id in stale) { _troops[id].RemoveFromHierarchy(); _troops.Remove(id); }
        }

        /// <summary>兵马图元内容（逐帧重建；拖拽/姿态交互挂在图元根上，内容重建不影响）：抓手 + 立绘名牌 + 兵力旗帜 + 姿态三态。</summary>
        private void BuildTroopContent(VisualElement chip, OwnUnitView u, int zoneCapacity, VisualElement root)
        {
            chip.Clear();

            var grip = new Label("⠿ 拖动布防");
            grip.AddToClassList("zb-troop-grip");
            chip.Add(grip);

            string label = (u.LeaderName ?? u.DetachmentId) + $"（{u.Strength}{(u.InTransit ? "·在途" : "")}{(u.IsBroken ? "·溃" : "")}）";
            chip.Add(BuildNameplate(label, u.LeaderName, enemy: false));
            chip.Add(BuildForceRow(string.Empty, u.Strength, zoneCapacity, enemy: false));

            var postureRow = new VisualElement();
            postureRow.AddToClassList("zb-force-row");
            var tag = new Label("姿态");
            tag.AddToClassList("zb-force-label");
            postureRow.Add(tag);
            bool locked = u.InTransit || u.IsBroken;
            foreach (Posture p in new[] { Posture.Assault, Posture.Feint, Posture.Hold })
            {
                var captured = p;
                var btn = new Button(() => { ZoneBattleSession.SetPosture(u.DetachmentId, captured); Render(root); })
                {
                    text = ZoneBattleText.Posture(p),
                };
                btn.AddToClassList("zb-posture-btn");
                if (p == u.Posture) btn.AddToClassList("zb-posture-on"); // 当前姿态高亮
                btn.SetEnabled(!locked && p != u.Posture);
                postureRow.Add(btn);
            }
            chip.Add(postureRow);
        }

        /// <summary>兵马图元拖放（M3）：拈起→跟随指针→松手落在相邻区则发 Move 命令（否则回弹）。姿态按钮上的按压不触发拖拽；键盘调动列表仍是无障碍兜底。</summary>
        private void AttachDrag(VisualElement chip, string detId, VisualElement root)
        {
            bool dragging = false;
            chip.RegisterCallback<PointerDownEvent>(e =>
            {
                var t = e.target as VisualElement;
                if (t != null && (t is Button || t.GetFirstAncestorOfType<Button>() != null)) return; // 姿态按钮正常工作
                dragging = true;
                chip.AddToClassList("zb-troop-drag");
                chip.CapturePointer(e.pointerId);
                e.StopPropagation();
            });
            chip.RegisterCallback<PointerMoveEvent>(e =>
            {
                if (!dragging) return;
                var map = root.Q<VisualElement>("zb-map");
                if (map == null) return;
                Vector2 local = map.WorldToLocal(e.position);
                chip.style.left = local.x; // 拖拽态 transition 关，实时跟随
                chip.style.top = local.y;
            });
            chip.RegisterCallback<PointerUpEvent>(e =>
            {
                if (!dragging) return;
                dragging = false;
                chip.ReleasePointer(e.pointerId);
                chip.RemoveFromClassList("zb-troop-drag");
                var map = root.Q<VisualElement>("zb-map");
                string target = map != null ? DropTargetZone(map, map.WorldToLocal(e.position), detId) : null;
                if (target != null) ZoneBattleSession.Move(detId, target); // 合法相邻落点 → 调动命令
                Render(root); // 有效→补间到新区；无效→补间回原区（松手回弹）
            });
        }

        /// <summary>拖放落点判定：松手点最近的区锚点（半径内），且属该支队的合法相邻调动，则返回其 zone-id；否则 null。</summary>
        private string DropTargetZone(VisualElement map, Vector2 local, string detId)
        {
            float w = map.contentRect.width, h = map.contentRect.height;
            if (w <= 0f || h <= 0f) return null;
            float nx = local.x / w, ny = local.y / h;
            const float radiusSq = 0.16f * 0.16f;
            string best = null; float bestD = radiusSq;
            foreach (KeyValuePair<string, Vector2> kv in ZoneAnchors)
            {
                float dx = kv.Value.x - nx, dy = kv.Value.y - ny;
                float d = dx * dx + dy * dy;
                if (d < bestD) { bestD = d; best = kv.Key; }
            }
            if (best == null) return null;
            string zoneId = "zone-" + best;
            return _moveTargets.Contains(detId + "|" + zoneId) ? zoneId : null;
        }

        /// <summary>涌现兵法条件徽章：形状符号 + 色（P3 色彩外编码，不只靠色）。</summary>
        private static VisualElement BuildConditionBadge(string condition)
        {
            // 按条件名归类给形状符号 + 色（火/水/伏/夜 + 通用兜底）。
            string glyph; Color color;
            if (condition.Contains("火")) { glyph = "▲"; color = new Color(0.78f, 0.30f, 0.12f); }
            else if (condition.Contains("水") || condition.Contains("淹")) { glyph = "●"; color = new Color(0.18f, 0.48f, 0.50f); }
            else if (condition.Contains("伏")) { glyph = "◆"; color = new Color(0.30f, 0.45f, 0.25f); }
            else if (condition.Contains("夜")) { glyph = "■"; color = new Color(0.28f, 0.28f, 0.40f); }
            else { glyph = "✦"; color = new Color(0.62f, 0.44f, 0.16f); }

            var badge = new Label($"{glyph} {condition}");
            badge.AddToClassList("zb-cond-badge");
            badge.style.color = color;
            badge.style.borderLeftColor = color;
            return badge;
        }

        /// <summary>兵力行：标签 + 队列旗帜图元（数量按 F1 分档，封顶转 +N 溢出角标）。</summary>
        private static VisualElement BuildForceRow(string label, int strength, int capacity, bool enemy, bool fogged = false)
        {
            var row = new VisualElement();
            row.AddToClassList("zb-force-row");

            var lab = new Label(label);
            lab.AddToClassList("zb-force-label");
            row.Add(lab);

            var banners = new VisualElement();
            banners.AddToClassList("zb-banners");
            int count = BannerCount(strength, capacity, out int overflow);
            for (int i = 0; i < count; i++)
            {
                var flag = new VisualElement();
                flag.AddToClassList("zb-banner");
                flag.AddToClassList(enemy ? "zb-banner-enemy" : "zb-banner-own");
                if (fogged) flag.style.opacity = 0.5f; // 反全知：未侦察兵力半透（推测态）
                banners.Add(flag);
            }
            if (overflow > 0)
            {
                var plus = new Label("+" + overflow);
                plus.AddToClassList("zb-overflow");
                banners.Add(plus);
            }
            if (fogged && count > 0)
            {
                var q = new Label("?"); // 未侦察问号徽标（推测语气）
                q.AddToClassList("zb-overflow");
                banners.Add(q);
            }
            row.Add(banners);
            return row;
        }

        /// <summary>将领名牌：立绘缩略（有图显立绘、无图剪影占位）+ 阵营/敌我色边框 + 名（含支队摘要）。</summary>
        private static VisualElement BuildNameplate(string labelText, string leaderName, bool enemy)
        {
            var plate = new VisualElement();
            plate.AddToClassList("zb-nameplate");

            var portrait = new Image { scaleMode = ScaleMode.ScaleToFit };
            portrait.AddToClassList("zb-portrait");
            if (enemy) portrait.AddToClassList("zb-portrait-enemy");
            var tex = LoadPortrait(leaderName);
            if (tex != null) portrait.image = tex;
            plate.Add(portrait);

            var name = new Label(labelText);
            name.AddToClassList("zb-name");
            name.style.whiteSpace = WhiteSpace.Normal;
            plate.Add(name);
            return plate;
        }

        /// <summary>F1：图元数量 = ceil(兵力/容量 / 0.15)，非零≥1，封顶 IconCap，超出记 overflow。</summary>
        private static int BannerCount(int strength, int capacity, out int overflow)
        {
            overflow = 0;
            if (strength <= 0) return 0;
            if (capacity <= 0) capacity = 1;
            float ratio = (float)strength / capacity;
            int raw = Mathf.CeilToInt(ratio / IconUnitFraction);
            overflow = Mathf.Max(0, raw - IconCap);
            return Mathf.Max(1, Mathf.Min(raw, IconCap));
        }

        private static Texture2D LoadPortrait(string name)
        {
            if (string.IsNullOrEmpty(name) || name == "未探明之将") return null;
            return Resources.Load<Texture2D>($"Portraits/{name}");
        }

        /// <summary>邻接连线：在 zb-edges 层用 Painter2D 沿锚点描夯土路（节点之下，读作"可调动路径"）。</summary>
        private static void DrawEdges(MeshGenerationContext ctx, VisualElement edges)
        {
            float w = edges.contentRect.width, h = edges.contentRect.height;
            if (w <= 0f || h <= 0f) return;

            var p = ctx.painter2D;
            p.lineWidth = 3f;
            p.lineCap = LineCap.Round;
            p.strokeColor = new Color(0.42f, 0.34f, 0.22f, 0.55f); // 夯土路·半透（不抢节点）
            foreach ((string a, string b) in Edges)
            {
                if (!ZoneAnchors.TryGetValue(a, out Vector2 pa) || !ZoneAnchors.TryGetValue(b, out Vector2 pb)) continue;
                p.BeginPath();
                p.MoveTo(new Vector2(pa.x * w, pa.y * h));
                p.LineTo(new Vector2(pb.x * w, pb.y * h));
                p.Stroke();
            }
        }

        private static string StripZonePrefix(string zoneId)
            => zoneId != null && zoneId.StartsWith("zone-") ? zoneId.Substring(5) : zoneId;

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
