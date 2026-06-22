using System;
using System.Collections.Generic;

namespace ThreeKingdom.Presentation.Screens
{
    /// <summary>一条通知（hud.md §8）。</summary>
    public sealed class Notification
    {
        /// <summary>类别（同类合并键）。</summary>
        public string Kind { get; }
        /// <summary>时间戳（毫秒，合并窗判定）。</summary>
        public long TimestampMs { get; }
        /// <summary>是否临界（断粮/期限/校验失败：绕队列立即显示）。</summary>
        public bool IsCritical { get; }

        public Notification(string kind, long timestampMs, bool isCritical)
        {
            if (string.IsNullOrWhiteSpace(kind)) throw new ArgumentException("类别不可为空。", nameof(kind));
            Kind = kind;
            TimestampMs = timestampMs;
            IsCritical = isCritical;
        }
    }

    /// <summary>活动 toast（可因同类合并而累计计数）。</summary>
    public sealed class ActiveToast
    {
        /// <summary>类别。</summary>
        public string Kind { get; }
        /// <summary>是否临界。</summary>
        public bool IsCritical { get; }
        /// <summary>合并计数（同类 500ms 内合并 → 计数累加）。</summary>
        public int Count { get; internal set; }
        /// <summary>最近一次时间戳。</summary>
        public long LastTimestampMs { get; internal set; }

        internal ActiveToast(string kind, bool isCritical, long ts)
        {
            Kind = kind; IsCritical = isCritical; Count = 1; LastTimestampMs = ts;
        }
    }

    /// <summary>
    /// HUD 通知聚合（hud.md §8 / §12 通知验收）。规则（可测）：
    /// <list type="bullet">
    ///   <item>同类 <see cref="MergeWindowMs"/>(500ms) 内合并为一条（计数累加），不刷屏。</item>
    ///   <item>临界通知（断粮/期限/校验失败）<b>绕队列立即显示</b>，不受并发上限约束。</item>
    ///   <item>非临界并发可见 ≤ <see cref="MaxConcurrent"/>(3)；超出入队，<see cref="Release"/> 后放出。</item>
    /// </list>
    /// 确定性：同输入序列 → 同可见集。
    /// </summary>
    public sealed class NotificationFeed
    {
        /// <summary>同类合并窗（毫秒）。</summary>
        public const long MergeWindowMs = 500;
        /// <summary>非临界并发可见上限。</summary>
        public const int MaxConcurrent = 3;

        private readonly List<ActiveToast> _active = new List<ActiveToast>();
        private readonly Queue<Notification> _pending = new Queue<Notification>();

        /// <summary>当前可见 toast（只读）。</summary>
        public IReadOnlyList<ActiveToast> ActiveToasts => _active;

        /// <summary>排队等待放出的非临界通知数。</summary>
        public int PendingCount => _pending.Count;

        /// <summary>推入一条通知，按规则合并/立即显示/入队。</summary>
        public void Push(Notification n)
        {
            if (n == null) throw new ArgumentNullException(nameof(n));

            // 同类 500ms 内合并。
            foreach (var t in _active)
            {
                if (t.Kind == n.Kind && n.TimestampMs - t.LastTimestampMs <= MergeWindowMs && n.TimestampMs >= t.LastTimestampMs)
                {
                    t.Count++;
                    t.LastTimestampMs = n.TimestampMs;
                    return;
                }
            }

            if (n.IsCritical)
            {
                _active.Add(new ActiveToast(n.Kind, true, n.TimestampMs)); // 绕队列 + 绕并发上限
                return;
            }

            if (NonCriticalActiveCount() < MaxConcurrent)
                _active.Add(new ActiveToast(n.Kind, false, n.TimestampMs));
            else
                _pending.Enqueue(n); // 缓和后经 Release 放出
        }

        /// <summary>放出一条排队的非临界通知（缓和时调用）；无可放出则无操作。</summary>
        public bool Release()
        {
            if (_pending.Count == 0 || NonCriticalActiveCount() >= MaxConcurrent) return false;
            var n = _pending.Dequeue();
            _active.Add(new ActiveToast(n.Kind, false, n.TimestampMs));
            return true;
        }

        /// <summary>消除一条可见 toast（如玩家关闭或超时）。</summary>
        public void Dismiss(ActiveToast toast) => _active.Remove(toast);

        private int NonCriticalActiveCount()
        {
            int c = 0;
            foreach (var t in _active) if (!t.IsCritical) c++;
            return c;
        }
    }
}
