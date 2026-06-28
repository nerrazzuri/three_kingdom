# Skill Test Spec: /vertical-slice

> **Category**: utility
> **Priority**: low
> **Spec written**: 2026-06-28

## Skill Summary

`/vertical-slice` manages the Pre-Production validation workflow for building a near-production-quality end-to-end slice. It reads concept, systems, architecture, control manifest, and key GDDs; defines a falsifiable validation question; confirms scope before implementation; creates a disposable prototype directory only after approval; collects playtest observations; produces a `REPORT.md`; and returns a PROCEED / PIVOT / KILL recommendation for the Pre-Production → Production decision.

---

## Static Assertions

These should pass before any behavioral testing:

- [ ] Frontmatter has all required fields (`name`, `description`, `argument-hint`, `user-invocable`, `allowed-tools`)
- [ ] 2+ phase headings found
- [ ] At least one verdict keyword present (`PROCEED`, `PIVOT`, `KILL`, `READY`)
- [ ] If `allowed-tools` includes Write/Edit: `"May I write"` language present
- [ ] Next-step handoff section present at end

---

## Director Gate Checks

`/vertical-slice` triggers only the creative playtest review gate after the report exists. It does not run phase gates itself; `/gate-check pre-production` consumes the resulting evidence.

- **Full mode**: spawns `creative-director` with gate `CD-PLAYTEST` after `REPORT.md` is generated.
- **Lean mode**: skips `CD-PLAYTEST` and notes `CD-PLAYTEST skipped — Lean mode.`
- **Solo mode**: skips `CD-PLAYTEST` and notes `CD-PLAYTEST skipped — Solo mode.`

---

## Test Cases

### Case 1: Happy Path — Validated slice produces PROCEED report

**Fixture**:
- `CLAUDE.md` exists and names a configured engine.
- `design/gdd/game-concept.md`, `design/gdd/systems-index.md`, `docs/architecture/architecture.md`, and `docs/architecture/control-manifest.md` exist.
- Key GDDs for the chosen slice scope exist.
- No existing `prototypes/three-kingdom-siege-vertical-slice/REPORT.md`.

**Input**: `/vertical-slice --review lean`

**Expected behavior**:
1. Skill resolves review mode to `lean`.
2. Skill reads concept, systems index, architecture, control manifest, and relevant GDDs before defining scope.
3. Skill defines a falsifiable validation question covering both player experience and build feasibility.
4. Skill presents the slice scope and asks the user to confirm before building.
5. Skill asks "May I create the vertical slice directory at `prototypes/[concept-name]-vertical-slice/` and begin implementation?"
6. After approval, all created slice files are isolated under `prototypes/[concept-name]-vertical-slice/`.
7. Skill collects playtest answers one at a time and writes `REPORT.md` only after asking "May I write this report to `prototypes/[concept-name]-vertical-slice/REPORT.md`?"
8. Report contains a PROCEED verdict and recommended next steps.

**Assertions**:
- [ ] Required design and architecture context is read before scope definition.
- [ ] Scope confirmation occurs before implementation.
- [ ] "May I create" is asked before prototype directory creation.
- [ ] "May I write this report" is asked before `REPORT.md` is written.
- [ ] Output verdict is PROCEED, PIVOT, or KILL.
- [ ] Lean mode skips `CD-PLAYTEST` with an explicit skip note.

**Case Verdict**: PASS

---

### Case 2: Blocked — Missing design or architecture context

**Fixture**:
- `design/gdd/game-concept.md` is missing, or `docs/architecture/control-manifest.md` is missing.
- `prototypes/` may or may not exist.

**Input**: `/vertical-slice`

**Expected behavior**:
1. Skill detects the missing required context during Phase 1.
2. Skill reports the missing file and explains that a vertical slice should run after GDDs, architecture, and UX specs are complete.
3. Skill does not create `production/session-state/active.md`.
4. Skill does not create a prototype directory.
5. Skill recommends the upstream command or artifact needed before retrying.

**Assertions**:
- [ ] Missing context is reported clearly.
- [ ] No prototype files are created.
- [ ] No report is written.
- [ ] Recommended upstream next step is provided.

**Case Verdict**: PASS

---

### Case 3: Mode Variant — Full review runs CD-PLAYTEST

**Fixture**:
- Full vertical slice report exists at `prototypes/[concept-name]-vertical-slice/REPORT.md`.
- Review mode is `full`.
- Concept pillars and core fantasy are available in `design/gdd/game-concept.md`.

**Input**: `/vertical-slice --review full`

**Expected behavior**:
1. Skill resolves review mode to `full`.
2. After report generation, skill spawns `creative-director` with `CD-PLAYTEST`.
3. The gate receives the full report content, validation question, game pillars, and core fantasy.
4. If the creative director changes the recommendation, skill asks before updating `REPORT.md`.

**Assertions**:
- [ ] `CD-PLAYTEST` is spawned in full mode only after the report exists.
- [ ] Gate prompt includes report content, validation question, pillars, and core fantasy.
- [ ] Any report update is gated by "May I write".
- [ ] Skill does not run `CD-PLAYTEST` before playtest observations exist.

**Case Verdict**: PASS

---

### Case 4: Edge Case — PIVOT creates carry-forward note

**Fixture**:
- A vertical slice report concludes PIVOT.
- User can identify which systems worked and what failed.

**Input**: `/vertical-slice` after PIVOT verdict is reached.

**Expected behavior**:
1. Skill asks what systems or mechanics worked and should be preserved.
2. Skill asks what specifically failed: core loop, architecture, pipeline, or fun.
3. Skill asks "May I write this to `prototypes/[concept-name]-vertical-slice/PIVOT-NOTE.md`?"
4. After approval, `PIVOT-NOTE.md` records what worked, what failed, affected systems or architecture, and what the next slice should prove differently.
5. Handoff recommends revising affected GDDs or ADRs before re-running `/vertical-slice`.

**Assertions**:
- [ ] Carry-forward questions are asked before writing a pivot note.
- [ ] `PIVOT-NOTE.md` is written only after approval.
- [ ] Next-step handoff references `/design-system`, `/architecture-decision`, or re-running `/vertical-slice`.
- [ ] PIVOT does not auto-create new GDD or ADR files.

**Case Verdict**: PASS

---

### Case 5: Kill Verdict — Graveyard write is gated

**Fixture**:
- A vertical slice reaches KILL.
- At least two kill-confirmation conditions apply.
- `prototypes/GRAVEYARD.md` may or may not exist.

**Input**: `/vertical-slice` after KILL verdict is reached.

**Expected behavior**:
1. Skill checks the KILL confirmation list.
2. Skill asks "May I append this to `prototypes/GRAVEYARD.md`?"
3. After approval, it appends one entry containing kill reason, what worked, what failed, and one next-time lesson.
4. Handoff recommends `/brainstorm` or `/prototype [new-concept]`.

**Assertions**:
- [ ] KILL is confirmed against explicit conditions.
- [ ] `GRAVEYARD.md` append is gated by "May I append".
- [ ] Existing prototype files are not deleted.
- [ ] Handoff points to `/brainstorm` or `/prototype`.

**Case Verdict**: PASS

---

## Protocol Compliance

- [ ] Uses `"May I create"` before creating the prototype directory.
- [ ] Uses `"May I write"` before writing `REPORT.md`, `PIVOT-NOTE.md`, or updating `prototypes/index.md`.
- [ ] Uses `"May I append"` before writing to `prototypes/GRAVEYARD.md`.
- [ ] Presents scope and validation question before implementation.
- [ ] Keeps vertical slice code isolated to `prototypes/`.
- [ ] States that vertical slice code must never be imported into production.
- [ ] Ends with a recommended next command based on PROCEED / PIVOT / KILL.

---

## Coverage Notes

- This spec validates workflow behavior, approval gates, review mode handling, and artifact boundaries. It does not validate whether a real slice is fun; that requires actual playtest evidence.
- Engine-specific implementation details are intentionally out of scope for this utility spec and are covered by engine specialists plus `/gate-check pre-production`.
