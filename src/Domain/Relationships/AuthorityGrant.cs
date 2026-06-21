using System;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.Time;

namespace ThreeKingdom.Domain.Relationships
{
    /// <summary>
    /// 授权凭证（GDD_006 §Data Model：AuthorityGrant / §Formula 5 / TR-relationship-002）。
    /// 授权来自<b>正式授予 + 期限 + 撤销规则</b>，<b>不</b>由关系凭空产生（关系高不能绕过有效性判定，AC-3/AC-4）。
    /// 不可变值；撤销产生新实例（<see cref="AsRevoked"/>）。
    /// </summary>
    public sealed class AuthorityGrant
    {
        /// <summary>授予者。</summary>
        public CharacterId Granter { get; }

        /// <summary>被授予者。</summary>
        public CharacterId Grantee { get; }

        /// <summary>权限范围（命令类型）。</summary>
        public CommandType Scope { get; }

        /// <summary>有效截止时间（含端点）。</summary>
        public WorldTime Expiry { get; }

        /// <summary>是否已撤销。</summary>
        public bool Revoked { get; }

        public AuthorityGrant(CharacterId granter, CharacterId grantee, CommandType scope, WorldTime expiry, bool revoked = false)
        {
            if (!Enum.IsDefined(typeof(CommandType), scope)) throw new ArgumentOutOfRangeException(nameof(scope));
            Granter = granter;
            Grantee = grantee;
            Scope = scope;
            Expiry = expiry;
            Revoked = revoked;
        }

        /// <summary>
        /// 授权是否有效（GDD §Formula 5）：未过期 ∧ 授予者仍具授予权限 ∧ 未撤销。
        /// 关系高<b>不能</b>绕过此判定（本方法不接受任何关系参数）。
        /// </summary>
        /// <param name="now">当前时间。</param>
        /// <param name="granterStillAuthorized">授予者当前是否仍具授予该范围的权限（由职责系统判定）。</param>
        public bool IsValid(WorldTime now, bool granterStillAuthorized)
            => !Revoked && granterStillAuthorized && now <= Expiry;

        /// <summary>返回已撤销的副本。</summary>
        public AuthorityGrant AsRevoked() => new AuthorityGrant(Granter, Grantee, Scope, Expiry, revoked: true);
    }
}
