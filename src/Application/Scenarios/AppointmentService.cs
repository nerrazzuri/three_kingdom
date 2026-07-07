using ThreeKingdom.Domain.Appointment;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Map;

namespace ThreeKingdom.Application.Scenarios
{
    /// <summary>任用合法性门（GDD_027 P3）：在容器 <see cref="AppointmentBook"/> 之上加"这个人现在能不能任用"的规则校验。</summary>
    public enum AppointGate
    {
        Ok = 0,
        /// <summary>非玩家麾下（既非事奉玩家势力，也未招揽入伙）。</summary>
        NotYours = 1,
        /// <summary>在押（被俘）。</summary>
        Captive = 2,
        /// <summary>重创不可受命。</summary>
        Incapacitated = 3,
        /// <summary>未及冠或已故（不在世间）。</summary>
        Absent = 4,
        /// <summary>城册已满。</summary>
        CityFull = 5,
        /// <summary>已在该城。</summary>
        AlreadyThere = 6,
        /// <summary>非法（空 id 等）。</summary>
        Invalid = 7,
    }

    /// <summary>
    /// 太守任用应用服务（GDD_027 P3 / ADR-0016）：在 <see cref="AppointmentBook.Assign"/> 前施加合法性硬约束——
    /// 须在世、非被俘、非重创、且属玩家麾下（事奉玩家势力 或 已招揽入伙）。防止表现层乱传 id 污染权威任用态。纯函数。
    /// </summary>
    public static class AppointmentService
    {
        /// <summary>校验并调拨。返回 (门结果, 新簿)；门未过则簿不变。</summary>
        public static (AppointGate Gate, AppointmentBook Book) Assign(
            AppointmentBook book, CityId city, CharacterId general,
            int anchorYear, FactionId playerFaction, GeneralLedger ledger, TalentKnowledgeBook talents)
        {
            if (book == null || city.Value == null || general.Value == null) return (AppointGate.Invalid, book!);

            // 在世门（生卒 / 纪元）。
            if (!GeneralDossiers.AvailableAt(general, anchorYear)) return (AppointGate.Absent, book);

            // 人生态门（被俘 / 重创）。
            GeneralState? life = ledger?.Get(general);
            if (life != null)
            {
                if (life.CaptiveOf.HasValue) return (AppointGate.Captive, book);
                if (life.Health == GeneralHealth.Grave) return (AppointGate.Incapacitated, book);
            }

            // 归属门：事奉玩家势力，或已招揽入伙。
            Affiliation aff = GeneralAffiliations.AffiliationOf(general, anchorYear);
            bool inPlayerService = aff.Status == AffiliationStatus.InService && aff.Faction.Equals(playerFaction);
            bool recruited = talents != null && talents.OutcomeOf(general) == RecruitOutcome.Joined;
            if (!inPlayerService && !recruited) return (AppointGate.NotYours, book);

            // 委托容器（满员/已在/一将一城）。
            (AppointResult r, AppointmentBook nb) = book.Assign(city, general);
            return (MapResult(r), r == AppointResult.Ok ? nb : book);
        }

        private static AppointGate MapResult(AppointResult r) => r switch
        {
            AppointResult.Ok => AppointGate.Ok,
            AppointResult.CityFull => AppointGate.CityFull,
            AppointResult.AlreadyThere => AppointGate.AlreadyThere,
            _ => AppointGate.Invalid,
        };
    }
}
