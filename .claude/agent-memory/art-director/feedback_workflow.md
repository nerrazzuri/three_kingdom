---
name: feedback-workflow-approval
description: 用户在特定上下文中会授权跳过逐节确认，直接产出完整成稿
metadata:
  type: feedback
---

当用户明确说"无需再问我，直接给完整成稿"时，跳过逐节审批流程，一次性产出全部内容写入文件。

**Why:** 用户在承接 AD-PHASE-GATE 缺口分析后的 art bible 撰写任务时明确指示，此类任务已在对话中完成充分的范围对齐，不需要再走 Question→Options→Decision→Draft→Approval 全流程。

**How to apply:** 仅在用户已明确批准范围（如"Section 1-4 + Section 8"）且明确授权跳过审批时生效。若范围未锁定，仍须走完整确认流程。
