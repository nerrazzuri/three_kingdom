using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Career;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.World;

namespace ThreeKingdom.Application.Session
{
    /// <summary>
    /// 跨系统后果原子写回事务（ADR-0009 §R-6 / TR-session-002）。
    /// 收集变更（生涯 / 城池控制权经 GDD_004 / 势力创建经 GDD_015），**先全部校验、再全部提交**——
    /// 校验阶段任一失败即返回稳定错误码且<b>零应用</b>（无部分写入）；提交期异常则回滚到事务前态、状态哈希一致。
    /// <para>
    /// 装配层只<b>路由</b>后果到各权威系统：城池归属经 <see cref="ICityControlAuthority"/>（ADR-0008），
    /// 势力创建经 GDD_015（R-3）；不在本层直接写归属/势力存续公式（R-5）。
    /// </para>
    /// </summary>
    public sealed class ConsequenceTransaction
    {
        private readonly CampaignSession _session;
        private readonly CareerSnapshot _careerBefore;
        private readonly WorldState _worldBefore;
        private readonly List<Func<CampaignErrorCode>> _validate = new List<Func<CampaignErrorCode>>();
        private readonly List<Action> _apply = new List<Action>();

        internal ConsequenceTransaction(CampaignSession session)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _careerBefore = session.Career;   // 不可变快照，回滚锚点
            _worldBefore = session.World;
        }

        /// <summary>暂存生涯写回（新 CareerSnapshot 由 Domain 服务结算产出）。</summary>
        public ConsequenceTransaction StageCareer(CareerSnapshot newCareer)
        {
            _validate.Add(() => newCareer != null ? CampaignErrorCode.None : CampaignErrorCode.InvalidConfig);
            _apply.Add(() => _session.SetCareer(newCareer!));
            return this;
        }

        /// <summary>暂存城池控制权变更（经 GDD_004 唯一权威，ADR-0008）。城须已登记。</summary>
        public ConsequenceTransaction StageControlChange(CityId city, FactionId newOwner, Garrison garrison, ChangeCause cause)
        {
            _validate.Add(() => _session.Control.OwnerOf(city) != null
                ? CampaignErrorCode.None : CampaignErrorCode.InvalidConfig);
            _apply.Add(() => _session.Control.RequestControlChange(city, newOwner, garrison, cause));
            return this;
        }

        /// <summary>暂存势力创建（自立成立，经 GDD_015 唯一权威，R-3）。势力 id 须不重复。</summary>
        public ConsequenceTransaction StageFactionCreation(FactionRecord faction)
        {
            _validate.Add(() => faction != null && _session.World.FactionById(faction.Id) == null
                ? CampaignErrorCode.None : CampaignErrorCode.InvalidConfig);
            _apply.Add(() => _session.CreateFaction(faction!));
            return this;
        }

        /// <summary>
        /// 原子提交：先全部校验（任一失败→零应用、返回错误码）；再全部应用（异常→回滚到事务前态，哈希一致）。
        /// </summary>
        public CampaignCommandResult Commit()
        {
            // 1) 校验阶段——尚未应用任何变更，失败即无部分写入。
            foreach (Func<CampaignErrorCode> v in _validate)
            {
                CampaignErrorCode e = v();
                if (e != CampaignErrorCode.None)
                    return CampaignCommandResult.Failure(e, "后果校验失败，零应用（原子）。");
            }

            // 2) 应用阶段——异常则回滚到事务前态。
            try
            {
                foreach (Action a in _apply) a();
            }
            catch (Exception ex)
            {
                _session.SetCareer(_careerBefore);
                _session.RestoreWorld(_worldBefore);
                return CampaignCommandResult.Failure(CampaignErrorCode.InvalidConfig, "提交期异常已回滚：" + ex.Message);
            }
            return CampaignCommandResult.Success();
        }
    }
}
