using System;
using System.Collections.Generic;

namespace ThreeKingdom.Application.Session
{
    /// <summary>
    /// 战役场景目录（M01 / ADR-0003 数据驱动配置）。不可变。注册多个命名 <see cref="CampaignStartConfig"/>，
    /// 按 <c>ScenarioConfigId</c> 查询，供 CampaignSession 按 id 配置驱动开局——**不以硬编码场景为唯一源**。
    /// 构造时跨条目校验（非空 + 无重复 id），非法即抛、不部分加载（各 config 自身范围校验在其 ctor 完成）。
    /// </summary>
    public sealed class ScenarioCatalog
    {
        private readonly Dictionary<string, CampaignStartConfig> _byId;

        /// <summary>已注册场景 id（注册顺序）。</summary>
        public IReadOnlyList<string> Ids { get; }

        public ScenarioCatalog(IReadOnlyList<CampaignStartConfig> scenarios)
        {
            if (scenarios is null) throw new ArgumentNullException(nameof(scenarios));
            if (scenarios.Count == 0) throw new ArgumentException("场景目录不可为空。", nameof(scenarios));

            _byId = new Dictionary<string, CampaignStartConfig>();
            var ids = new List<string>(scenarios.Count);
            foreach (CampaignStartConfig c in scenarios)
            {
                if (c is null) throw new ArgumentException("场景配置不可含 null。", nameof(scenarios));
                if (_byId.ContainsKey(c.ScenarioConfigId))
                    throw new ArgumentException($"场景 id 重复：{c.ScenarioConfigId}。", nameof(scenarios));
                _byId[c.ScenarioConfigId] = c;
                ids.Add(c.ScenarioConfigId);
            }
            Ids = ids;
        }

        /// <summary>按 id 取场景配置；不存在返回 null。</summary>
        public CampaignStartConfig? Find(string scenarioId)
            => scenarioId != null && _byId.TryGetValue(scenarioId, out CampaignStartConfig? c) ? c : null;
    }
}
