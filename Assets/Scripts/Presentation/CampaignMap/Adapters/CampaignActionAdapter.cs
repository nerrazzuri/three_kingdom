using UnityEngine;
using ThreeKingdom.Unity.UI;   // SessionRuntime

namespace ThreeKingdom.Presentation.CampaignMap
{
    /// <summary>
    /// 战略地图动作适配器（scaffold ICampaignActionService 实现）：把地图动作接到我们已单测的 <see cref="SessionRuntime"/> 命令。
    /// MVP：EndTurn → 推进一周；Attack → 出征入口（授权+组装另走出征屏）；余动作预留。★需 Unity 编辑器验证。
    /// </summary>
    public sealed class CampaignActionAdapter : MonoBehaviour, ICampaignActionService
    {
        public bool CanExecute(ExecuteActionRequest request)
        {
            if (request == null) return false;
            return request.Action switch
            {
                CampaignAction.EndTurn => true,
                CampaignAction.Attack => !string.IsNullOrEmpty(request.TerritoryId),
                _ => false,   // 其余动作预留（Move/Recruit/BuildFort/Diplomacy/Supply）
            };
        }

        public void Execute(ExecuteActionRequest request)
        {
            if (!CanExecute(request)) return;
            switch (request.Action)
            {
                case CampaignAction.EndTurn:
                    SessionRuntime.AdvanceWeek();   // 世界地图一步=一周（GDD_026）
                    break;
                case CampaignAction.Attack:
                    // 出征走既有授权→组装→区域战斗流程（出征屏）；此处仅请授权占位。
                    SessionRuntime.RequestOffensiveAuthorization();
                    break;
            }
        }
    }
}
