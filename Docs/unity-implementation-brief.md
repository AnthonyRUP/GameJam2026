# Countdown — Unity Implementation Brief

Hackathon theme: **Countdown**. Game: a doctor/scientist must diagnose a dying patient, determine the cure, synthesize it, and administer it before the clock runs out.

## Core Loop

One reused verb across three phases: **compare evidence to a reference and narrow down an answer.**

1. **Triage** — symptoms reveal progressively over time; narrow disease candidates via a symptom book using whatever is currently visible.
2. **Blood Test** — two separate draws. Each draw, the player chooses which attribute to test: size or shape. The result is a visual glyph (not a text label) where only the tested attribute is accurate; the other is rendered as a neutral placeholder. Full blood certainty needs both draws.
3. **Synthesis** — pick reagents (color / size / shape) based on whatever's been confirmed or guessed. Always produces a compound, no feedback yet.
4. **Administer** — deliver the compound. Correctness is revealed here as a deterministic outcome (not a random roll) based on how many of the 3 attributes match — and even a wrong attempt gives real diagnostic information back.

Single patient, single continuous session. Health/vitals decay continuously in real time over the whole playthrough (not a separate flat clock) — the heart-rate monitor and vitals reflect this ongoing decline. Health hits zero at any point = flatline, death, game over. Administering the exact correct cure before health reaches zero = win.

## Why This Redesign (context for whoever picks this up)

Earlier versions of this game used 6 diseases with a structure engineered so that 2 symptoms + 2 blood attributes *always* resolved to exactly 1 candidate. That guaranteed correctness but made the game a fixed, memorizable procedure — solve it once, every future run is the same steps with different labels. This version deliberately expands to 14 diseases across three difficulty tiers with varying degrees of overlap, so which tier a given playthrough lands in is itself unpredictable: sometimes the case is quick and safe, sometimes it needs both systems combined, and sometimes it's genuinely irreducible through testing and requires a calculated guess. This is what makes the administer-risk mechanic matter on every playthrough rather than being a rare punishment for carelessness.

## Attribute Roles (important — these swapped from an earlier version)

Each disease has three defining attributes: **color, size, shape**.

- **Color** — diagnosis-only. Never revealed by the blood test. It's a static fact printed in the book next to each disease entry, and only becomes usable once the player has correctly identified which disease this is from symptoms.
- **Size** (small / medium / large) — blood-testable. Replaces "protein type" from an earlier iteration.
- **Shape** (triangle / square / circle / diamond) — blood-testable.

Size and shape are each obtainable via blood test — one draw per attribute, player's choice which to test first (see Blood Test below). Color is never obtainable from blood at all, regardless of how many draws are run.

## Phase Details

**Triage:** Each disease's 3 symptoms reveal progressively as health declines: symptom 1 at a high-health threshold, symptom 2 mid, symptom 3 late (close to the danger zone). Player cross-references whatever is currently visible against the Disease Book, which lists each disease's symptom checklist *and* its color (but never size or shape — those are blood-only). For Tier B's symptom-twin pairs, the two shared symptoms are ordered first and the distinguishing one last, so those pairs look identical on paper until dangerously late unless blood breaks the tie first. For Tier C, all 3 symptoms are identical between the pair — T3 gives zero extra information there, by design (see Tiers below).

**Blood Test:** Two separate draws, each costing real elapsed time (health keeps decaying during it). Each draw, the player chooses which attribute to test — size or shape — not both at once. The result renders as a visual glyph where only the tested attribute is accurate (e.g. testing shape shows the true shape at a neutral placeholder size; testing size shows the true size with a neutral placeholder shape) — still a genuine image to read, not a plain stat readout, but isolating exactly one variable per draw so the choice of what to test carries real weight. Getting both size and shape requires both draws. Color is never obtainable from blood, no matter how many draws are run — see Attribute Roles above.

**Synthesis:** Three reagent shelves — color / size / shape. Player picks one reagent from each into a mixer. Mixing never fails or gives feedback at this stage — it always produces *a* compound from whatever was selected, right or wrong.

**Administer:** Walk the mixed compound to bedside, inject. This is where correctness is revealed, and it's **deterministic, not random**:

| Attributes matching true disease | Outcome | Effect |
|---|---|---|
| 3/3 | Cure | Win |
| 2/3 | Improves | +15 health |
| 1/3 | No effect | No change |
| 0/3 | Worsens | -20 health |

After any non-cure outcome, filter the player's *current shortlist* (whatever's still plausible given symptoms + blood observed so far — not the full 14-disease codex) down to whichever diseases would produce that same outcome category against that exact compound, and reveal that filtered list. Scoping to the current shortlist matters: recomputing against the full codex can surface unrelated diseases that happen to share a raw match-count coincidentally, even though the player already ruled them out via symptoms. A wrong administer isn't fatal by itself (aside from the 0/3 penalty), so players can gather more information and try again — every attempt costs real time either way.

## The Three Tiers

**Tier A — Easy (4 diseases):** crimson_fever, marsh_ague, iron_cough, static_shock. Each has a symptom pair (T1+T2) and a (size,shape) combo that no other disease shares at all. Resolves quickly and safely, usually by T2 alone, blood test optional confirmation.

**Tier B — Moderate (6 diseases, 3 symptom-twin pairs):** nerve_static/glass_bone_syndrome, blue_lung_rot/choke_vine, sable_pox/weeping_root. Each pair shares T1+T2 exactly. Critically, each disease's (size,shape) blood-twin is a *different* disease with completely different early symptoms — not its own symptom-twin partner. That cross-cutting means 2 symptoms (safe, arrives by T2) plus both blood draws (size + shape) always narrows to exactly 1, without ever needing the risky T3. Verified programmatically:

- nerve_static ↔ blue_lung_rot share (size,shape) but differ instantly at T1 (tremor vs. cough)
- glass_bone_syndrome ↔ sable_pox share (size,shape) but differ instantly at T1 (tremor vs. nausea)
- choke_vine ↔ weeping_root share (size,shape) but differ instantly at T1 (cough vs. nausea)

**Tier C — Irreducible (4 diseases, 2 pairs):** ashen_wither/hollow_marrow, frost_bloom/iron_veil. Each pair shares ALL 3 symptoms *and* an identical (size,shape) — differing *only* in color. T3 gives zero extra information (all 3 symptoms are already shared) and blood gives zero extra information (size+shape identical). This pair is genuinely unresolvable through testing alone. Resolving it requires one administer attempt: guessing either candidate's full recipe gets a 2/3 match (color wrong, size+shape right), and because the shortlist is already down to just these 2 diseases, that "improves" outcome uniquely confirms which one it is — enabling a certain, cure-winning second attempt.

## Full Disease Table

| id | tier | symptoms (T1 → T2 → T3) | color (diagnosis-only) | size (blood) | shape (blood) |
|---|---|---|---|---|---|
| crimson_fever | A | fever → rash → rapid_pulse | crimson | small | triangle |
| marsh_ague | A | fever → chills → nausea | amber | large | diamond |
| iron_cough | A | cough → rapid_pulse → pale_skin | jade | medium | circle |
| static_shock | A | tremor → nausea → chills | violet | large | square |
| nerve_static | B | tremor → bloodshot_eyes → rapid_pulse | violet | small | circle |
| glass_bone_syndrome | B | tremor → bloodshot_eyes → swelling | azure | medium | diamond |
| blue_lung_rot | B | cough → rash → chills | jade | small | circle |
| choke_vine | B | cough → rash → swelling | amber | large | triangle |
| sable_pox | B | nausea → bloodshot_eyes → chills | crimson | medium | diamond |
| weeping_root | B | nausea → bloodshot_eyes → swelling | violet | large | triangle |
| ashen_wither | C | fever → tremor → pale_skin | crimson | small | square |
| hollow_marrow | C | fever → tremor → pale_skin | jade | small | square |
| frost_bloom | C | chills → swelling → pale_skin | amber | medium | triangle |
| iron_veil | C | chills → swelling → pale_skin | azure | medium | triangle |

All 14 full (color,size,shape) signatures are unique — verified programmatically, along with every tier's resolution property described above.

## Reagent Shelf

- Colors: crimson, amber, azure, violet, jade (used) + ash (decoy, never correct)
- Sizes: small, medium, large (used) + tiny (decoy)
- Shapes: triangle, square, circle, diamond (used) + star (decoy)

## Symptom Pool & Visual Technique (for hooking up whatever art arrives)

10 symptoms total (slurred_speech, discoloration, sweating, and clammy_skin were dropped from an earlier draft; every disease's symptom set below has been re-authored around the remaining 10, with all tier properties re-verified).

| symptom | technique | asset name to match |
|---|---|---|
| fever | instrument readout (temp monitor, high) | temp_gauge |
| chills | instrument readout (temp monitor, low) | temp_gauge |
| rapid_pulse | instrument readout (heart monitor spike) | heart_monitor |
| rash | sprite overlay | overlay_rash |
| swelling | sprite overlay | overlay_swelling |
| bloodshot_eyes | sprite overlay | overlay_bloodshot_eyes |
| pale_skin | sprite overlay | overlay_pale_skin |
| cough | discrete timed event (fog puff on glass tube) | event_cough_fog |
| tremor | sprite position jitter (no art asset — code-driven) | — |
| nausea | icon ticker above patient | icon_nausea |

Patient base sprite: `patient_base`. Reagent color vials: `reagent_color_<name>`. Size icons: `size_<name>`. Shape icons: `shape_<name>`. Blood test draw result: rendered at runtime from whichever icon set was tested (size or shape) plus a fixed neutral placeholder for the untested one — no separate static asset needed, just the two icon sets above composited per draw.

## Data-Driven Content

Every disease is a data row (see table above), not hardcoded logic — load from JSON/ScriptableObjects so new diseases can be added without touching gameplay code. A `countdown-codex.json` file with this exact data already exists; copy it into `Assets/StreamingAssets/` (or convert to ScriptableObjects) and parse at startup. It also includes the tier metadata and the deterministic administer outcome table.

## Open Decisions (defaults proposed — confirm or override before/while building)

- **Health decay model:** proposed start at 100, base decay of roughly 1-1.5/second.
- **Symptom reveal thresholds:** proposed T1/T2/T3 at roughly health 95 / 60 / 20.
- **Blood draw cost:** proposed ~9 seconds of elapsed time per draw; two draws (size + shape) needed for full blood certainty.
- **Disease selection:** proposed randomly chosen from the 14-disease pool at the start of each playthrough.
- **Administer outcome:** deterministic match-count table above (not random) — this is a firm design decision, not just a proposed default.
- **End screen:** proposed simple win/lose screen — win shows health remaining and time taken, lose shows a flatline/death screen. "Try Again" starts a new playthrough with a newly randomized disease.
- **Scene architecture:** proposed single persistent scene with a GameManager state machine driving 4 UI panels (Triage / Blood Test / Synthesis / Administer) shown/hidden per phase.
- **Input:** assumed mouse/click only (book checklist clicks, drag-and-drop reagents, and reading the combined blood glyph). Confirm if a controller or touch input is in scope.
- **Glyph rendering per draw:** needs a concrete visual spec — e.g. does "large" always render notably bigger than "medium," or are they close enough to require real attention? Recommend keeping the size difference modest so reading it is genuinely a perceptual task rather than obvious at a glance. Same consideration for telling similar shapes apart (e.g. square vs. diamond) when only one is the tested attribute and the other is a neutral placeholder.

## Suggested Build Order (recap)

1. Empty project exporting to target platform.
2. Generic matching/comparison UI component, reused for book and synthesis.
3. Blood test glyph renderer (per draw: real value for the tested attribute + neutral placeholder for the untested one), plus the "choose which attribute to test" draw UI.
4. Phase transitions wired with placeholder art.
5. Health decay + vitals system.
6. Deterministic administer outcome logic + scoped shortlist-filtering feedback.
7. Real disease data in (all 14, all 3 tiers), placeholder art swapped for real art as it arrives.
8. Random disease selection on playthrough start/retry.
9. Polish only in the final stretch — no new features.
