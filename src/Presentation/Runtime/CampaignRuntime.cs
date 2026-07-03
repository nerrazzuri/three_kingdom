using System;
using ThreeKingdom.Application.Scenarios;
using ThreeKingdom.Application.Session;
using ThreeKingdom.Domain.Persistence;
using ThreeKingdom.Domain.Time;
using ThreeKingdom.Presentation.Screens;

namespace ThreeKingdom.Presentation.Runtime
{
    /// <summary>
    /// 战役会话运行期核心（epic-028 story-001 / TR-ux-005）：Unity 壳与完整 <see cref="CampaignSession"/>
    /// 脊梁之间的<b>纯 C# 生命周期接缝</b>——新局 / 推进 / 状态投影 / 统一信封存读档。
    /// <para>
    /// 架构边界（ADR-0002 / ADR-0009）：只持 Application 会话句柄，一切变更经 <see cref="CampaignSessionService"/>；
    /// 投影为纯函数（同会话态渲染恒等，ADR-0004）；存档 I/O 经 <see cref="ISaveMedium"/> 端口注入（R-7），
    /// 原子写回沿 SaveRepository 同款「临时槽 + 原子改名」编排（失败保留上一份有效存档，ADR-0005）。
    /// 场景配置来自注入的 <see cref="PlayableCampaign"/>（与 console harness 单一同源，勿复制数值）。
    /// 本类无 UnityEngine 依赖，可 <c>dotnet test</c>（Unity 侧薄壳见 Assets/UI/SessionRuntime.cs）。
    /// </para>
    /// </summary>
    public sealed class CampaignRuntime
    {
        /// <summary>默认存档槽名（与旧竖切 "campaign" 槽区分，避免旧格式误读）。</summary>
        public const string DefaultSlot = "campaign-session";

        private readonly CampaignSessionService _service = new CampaignSessionService();
        private readonly ISaveMedium _medium;
        private readonly PlayableCampaign _scenario;
        private readonly string _slot;
        private CampaignSession? _session;
        private int _daysCrossedLastAdvance;

        /// <summary>构造运行期核心；存档介质必须注入（端口），场景缺省为「汜水关太守」共享场景源。</summary>
        public CampaignRuntime(ISaveMedium medium, PlayableCampaign? scenario = null, string slot = DefaultSlot)
        {
            _medium = medium ?? throw new ArgumentNullException(nameof(medium));
            _scenario = scenario ?? PlayableCampaign.Default();
            if (string.IsNullOrWhiteSpace(slot)) throw new ArgumentException("槽名不可为空。", nameof(slot));
            _slot = slot;
        }

        /// <summary>当前会话（首访自动开局，保证 HUD 单独打开也可玩）。仅供后续屏 story 经服务命令使用。</summary>
        public CampaignSession Session => _session ??= StartNew();

        /// <summary>共享场景源（不可变配置；供屏 story 取卫星配置/梯队等，勿复制数值）。</summary>
        public PlayableCampaign Scenario => _scenario;

        /// <summary>开新局（MainMenu「新游戏」）：以共享场景配置重开会话，返回初始世界状态视图。</summary>
        public WorldStatusView NewGame()
        {
            _session = StartNew();
            _daysCrossedLastAdvance = 0;
            return Status();
        }

        /// <summary>推进 <paramref name="segments"/> 个时段（HUD「推进时段」），返回推进后的世界状态视图（含跨日提示）。</summary>
        public WorldStatusView Advance(int segments = 1)
        {
            CampaignSession session = Session;
            int dayBefore = session.CurrentTime.Day;
            _service.Advance(session, segments);
            _daysCrossedLastAdvance = session.CurrentTime.Day - dayBefore;
            return Status();
        }

        /// <summary>取当前世界状态视图（不推进；纯函数——同会话态两次调用结果恒等）。</summary>
        public WorldStatusView Status()
        {
            WorldTime t = Session.CurrentTime;
            return new WorldStatusView(new WorldStatusProjection(t.Day, t.Segment, t.AbsoluteIndex, _daysCrossedLastAdvance));
        }

        /// <summary>默认槽是否有存档（主菜单「继续」可用性）。</summary>
        public bool HasSave() => _medium.Exists(_slot);

        /// <summary>
        /// 原子存档当前会话到槽（统一信封 <see cref="CampaignSessionService.CaptureSnapshot"/>）；
        /// 先写临时槽再原子改名——任一步失败返回 false 且正式槽保留上一份有效存档（ADR-0005 guardrail）。
        /// </summary>
        public bool Save()
        {
            string content = _service.CaptureSnapshot(Session);
            string tmp = _slot + ".tmp";
            try
            {
                _medium.Write(tmp, content);
            }
            catch (Exception)
            {
                TryDelete(tmp);
                return false;
            }

            try
            {
                _medium.Move(tmp, _slot);
            }
            catch (Exception)
            {
                TryDelete(tmp);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 读取槽恢复会话（统一信封 <see cref="CampaignSessionService.Restore"/>，卫星配置由场景源提供——数据驱动）。
        /// 成功切换当前会话返回 true；失败（无存档 / 版本、指纹、格式不符）返回 false 与原因，<b>当前会话不变</b>（不部分载入）。
        /// </summary>
        public bool Load(out string reason)
        {
            string? text = _medium.Read(_slot);
            if (text == null)
            {
                reason = "槽内无存档。";
                return false;
            }

            CampaignStartConfig config = _scenario.StartConfig;
            try
            {
                CampaignSession restored = _service.Restore(
                    text, config.Fingerprint,
                    settlementConfig: config.SettlementConfig,
                    governanceConfig: config.GovernanceConfig,
                    populationPressure: config.PopulationPressure,
                    intelConfig: config.IntelConfig,
                    councilSetup: config.CouncilSetup,
                    prepConfig: config.PreparationConfig,
                    reachableRegions: config.ReachableRegions,
                    authorizedOrders: config.AuthorizedOrders,
                    battleConfig: _scenario.BattleConfig,
                    tacticChains: _scenario.TacticChains);
                _session = restored;
                _daysCrossedLastAdvance = 0;
                reason = string.Empty;
                return true;
            }
            catch (SaveFormatException ex)
            {
                reason = ex.Message;
                return false;
            }
        }

        private CampaignSession StartNew()
        {
            CampaignStartResult result = _service.StartCampaign(_scenario.StartConfig);
            if (!result.Started)
                throw new InvalidOperationException("场景开局失败（配置源已验证，此处失败属编程错误）：" + result.Error + " " + result.Detail);
            return result.Session!;
        }

        private void TryDelete(string tmp)
        {
            try { _medium.Delete(tmp); } catch { /* 清理失败不掩盖原始错误 */ }
        }
    }
}
