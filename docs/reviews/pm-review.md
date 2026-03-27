# Sinag -- Product Manager Review

**Reviewer:** PM Review (Automated)
**Date:** 2026-03-26
**Documents Reviewed:**
- PRD v1.0
- TDD v1.0
- Implementation Plan v1.0
- Market Research

**Severity Scale:**
- **CRITICAL** -- must fix before development starts
- **IMPORTANT** -- should fix but won't block development
- **SUGGESTION** -- nice to have improvement

---

## 1. PRD Completeness

### 1.1 User Stories & Flows

**[IMPORTANT] No formal user stories written.** The PRD describes a core user flow (Section 4) but does not include user stories in standard format ("As a [user], I want to [action], so that [benefit]"). This makes it harder for developers to understand the *why* behind each feature and makes acceptance criteria ambiguous. Consider adding 5-8 core user stories with explicit acceptance criteria for MVP features.

**[IMPORTANT] Missing edge-case flows.** The core flow (Section 4) covers the happy path well, but the following scenarios are not addressed:
- What happens when a user scans a non-DLPC bill (e.g., Meralco, water bill, or random document)?
- What happens when the user scans a bill that is heavily crumpled, torn, or partially obscured?
- What happens when the camera permission is denied?
- What happens when the user's consumption is extremely low (e.g., <50 kWh/month) where solar may not make economic sense? Should the app advise against solar in such cases?
- What about commercial rate class users? The PRD mentions residential focus but the TDD schema includes commercial rates.

**[SUGGESTION] Multi-month averaging not addressed.** The PRD flow scans a single bill. Philippine electricity consumption varies seasonally. A single month's bill may produce a misleading system size. Consider noting whether the MVP will address this limitation (even with a disclaimer) or whether multi-bill scanning is a future feature.

### 1.2 Success Metrics

**[IMPORTANT] "500+ MAU at 3 months" lacks a user acquisition plan.** The PRD sets a target of 500 monthly active users 3 months post-launch but contains no distribution or marketing strategy. For a Davao-only Android app, 500 MAU is achievable but not automatic. The PRD should at least reference how users will discover the app (organic search, partnerships with solar companies, social media, etc.).

**[SUGGESTION] Missing retention metric.** All metrics are acquisition or single-session focused. There is no metric for return usage. Since this is a research tool (users may only need it once), consider tracking: "% of users who share their estimate" or "% of users who adjust parameters and regenerate" as engagement proxies.

**[SUGGESTION] Missing metric for manual correction rate.** The OCR accuracy target is >=90%, but there is no metric tracking how often users actually correct extracted values. This real-world correction rate is the true measure of OCR quality and would inform whether the 90% target is sufficient.

### 1.3 MVP Scope Clarity

**[CRITICAL] "Adjust parameters" feature is listed but underspecified.** In the core user flow (Step 8), users can "adjust parameters (e.g., add/remove battery, change panel count)." The battery toggle is addressed in the TDD and implementation plan, but changing panel count is not mentioned anywhere else. This creates ambiguity:
- Can users override the recommended system size?
- Can they change panel wattage or count manually?
- Does this trigger a full recalculation including new financial projections?

This needs to be explicitly scoped as IN or OUT for MVP. If OUT, remove it from the core flow. If IN, the TDD and implementation plan need corresponding tasks.

**[IMPORTANT] PDF sharing listed in PRD but absent from TDD and implementation plan.** PRD Section 5.5 mentions "Share as image (screenshot-ready summary card)" but the core flow (Step 8) also mentions "Share estimate as image or PDF." The TDD only references ShareService for generating a shareable image. PDF generation is not addressed technically. Clarify: is PDF sharing in MVP or not?

**[IMPORTANT] "Save estimate (provide email or phone number)" flow is unclear.** The PRD lists this as optional, and the TDD includes a `saved_estimates` table. However:
- The PRD says "User accounts / authentication" is out of scope. So how does a user retrieve a saved estimate later? Is this save only for the app to send a one-time email/SMS? Or is there a retrieval mechanism?
- If the save is just "we store it server-side with your contact info," what is the user benefit? They cannot retrieve it without an account.
- Consider clarifying: is this "save locally on device" (no contact info needed) or "email me a copy" (send a one-time email with the estimate)?

### 1.4 UX Principles

**[SUGGESTION] "Tagalog-first" principle needs clarification on default behavior.** The PRD says "default to Filipino/Tagalog" and the implementation plan says "User can switch language in settings (default: Tagalog)." However, many Filipino users set their phone language to English. Consider whether the app should respect the device locale or always default to Tagalog regardless.

**[SUGGESTION] No accessibility considerations mentioned.** No mention of font sizes for older users (target age 30-55), color contrast requirements, or screen reader compatibility. Given the target demographic, at minimum, ensure text is large enough to read and that the results screen does not rely solely on color to convey information.

---

## 2. TDD Alignment with PRD

### 2.1 Features the TDD Delivers

The TDD covers the core features well:
- On-device OCR with ML Kit
- DLPC bill template matching
- Backend calculation engine with system sizing, BOM, and financial analysis
- Three pricing tiers
- Offline behavior with cached data
- Save and share functionality
- NASA POWER integration

### 2.2 PRD Features the TDD Does Not Address

**[CRITICAL] Parameter adjustment (beyond battery toggle) has no technical design.** As noted above, the PRD mentions users can "change panel count" and "adjust parameters." The TDD only designs for the battery toggle (`includeBattery` boolean in the API request). There is no mechanism for:
- Overriding system size
- Changing panel count or wattage
- Adjusting autonomy hours for battery
- Changing the target offset percentage (e.g., offset 80% of consumption instead of 100%)

If these are MVP features, the API contract needs additional request parameters. If they are not MVP, the PRD should remove them.

**[IMPORTANT] Itemized charges extraction has no corresponding API usage.** The PRD (Section 5.1) and TDD OCR pipeline both mention extracting itemized charges (generation, transmission, distribution, system loss). However, the `EstimateRequest` API contract does not include these fields. The extracted itemized data is never sent to the backend or used in calculations. Either:
- The itemized extraction should be removed from MVP scope (simplifies OCR), or
- The API should accept and use these values for more accurate savings calculations (e.g., only offsetting the generation component since solar does not reduce fixed distribution charges).

**[IMPORTANT] The financial calculation oversimplifies savings.** The TDD calculates savings as `monthly offset kWh x DLPC rate per kWh`. But in reality, solar only offsets the generation charge component. Fixed charges (distribution, metering, taxes, subsidies) still apply. The PRD acknowledges this complexity by requiring itemized charge extraction, but the TDD calculation engine ignores it. This could produce significantly overstated savings estimates, undermining trust -- one of the PRD's core UX principles.

**[SUGGESTION] Historical consumption data not utilized.** The market research (Section 3) highlights that some bills show 12-month consumption charts as a key differentiator. The PRD does not mention extracting this, and the TDD does not design for it. This is fine for MVP, but it should be explicitly listed as a future enhancement.

### 2.3 TDD Features Not Justified by PRD

**[SUGGESTION] `saved_estimates` table stores more than needed.** The TDD database schema includes `selected_tier` in `saved_estimates`, implying the user selects a preferred tier. The PRD never mentions tier selection or preference -- it shows all three tiers. This is minor but indicates a small scope creep in the TDD.

### 2.4 API Contract Issues

**[IMPORTANT] `EstimateRequest` is missing `billingPeriod` context.** The request includes `billingPeriodDays` (integer) but not the actual billing period dates. Without the dates, the backend cannot determine which month the bill corresponds to, which means it cannot use month-specific irradiance data from the `irradiance_cache` table. Currently, the calculation uses a static 4.8 peak sun hours. If the irradiance cache stores 12 monthly values (as designed), the API should use the relevant month's data for accuracy.

**[SUGGESTION] Response does not include grid-tied vs. hybrid comparison.** The PRD (Section 5.2) says the calculator "supports grid-tied and hybrid configurations." The implementation plan (Phase 2, Task 4) says "Output both grid-tied and hybrid scenarios." But the API response only returns a single BOM structure. Consider whether the response should include two separate BOMs (one without battery, one with) or if the battery toggle approach is sufficient.

---

## 3. Implementation Plan Alignment

### 3.1 Phase Structure

**[SUGGESTION] Phase structure is well-designed.** The risk-first approach (OCR in Phase 1) is correct. The parallel execution option for Phase 1 and Phase 2 is a good call. The dependency graph is sound.

### 3.2 Missing or Underspecified Tasks

**[CRITICAL] No task for collecting and organizing DLPC sample bills.** Phase 1 depends on testing with real DLPC bills, but there is no task for acquiring, cataloging, or digitizing test bill images. The exit criteria require >=90% accuracy on test bills, but the test corpus is undefined. Before Phase 1 begins, there should be:
- A defined set of test bills (minimum 10-15 unique bills)
- Metadata per bill (expected kWh, expected amount, photo conditions)
- A test harness that runs OCR against all bills and reports accuracy

**[IMPORTANT] No task for the admin pricing update endpoint.** The TDD (Section 2, Technology Stack) mentions "Updated via protected API endpoint (no admin UI needed for MVP)." But the implementation plan Phase 2 only includes `POST /api/estimate` and `GET /api/pricing`. There is no task to build the protected admin endpoint for updating equipment prices or DLPC rates. Without this, updating pricing requires direct database access, which is fragile.

**[IMPORTANT] Phase 4 localization is too late.** Localization (Tagalog-first UI) is deferred to Phase 4 (Polish). But the PRD's first UX principle is "Tagalog-first, English-supported." Building all UI in English through Phases 1-3 and then retrofitting Tagalog means:
- All user testing in Phases 1-3 is in English, not reflecting the intended experience
- Retrofitting localization is more work than building it in from the start
- String resource architecture should be established in Phase 0

Consider moving the string resource file setup to Phase 0 and using Tagalog strings from Phase 1 onward.

**[SUGGESTION] No task for the guided camera overlay design.** Phase 1 includes "Camera capture page with guided overlay (rectangle frame for bill alignment)" but no task for designing what this overlay looks like. Is it a static rectangle? Does it auto-detect bill edges? Does it provide real-time feedback ("move closer," "hold steady")? This UX detail significantly impacts OCR accuracy and should be specified.

**[SUGGESTION] No task for the shareable summary card design.** Phase 3 includes "generate a summary card image" but no task for designing the card layout. This is a user-facing artifact that represents the product's quality. It should have a design specification.

### 3.3 Exit Criteria Gaps

**[IMPORTANT] Phase 2 exit criteria do not verify Davao discount application.** The calculation engine applies an 8-12% Davao discount to national pricing. The exit criteria say "Calculation math verified against manual calculations" but do not specifically call out verifying the discount logic. This is a Sinag-specific differentiator and should be explicitly tested.

**[SUGGESTION] Phase 3 exit criteria require testing on "at least 2 physical Android devices" but do not specify device tiers.** The implementation plan Phase 4 mentions "low-end, mid-range, flagship" testing. Phase 3 should specify at least one low-to-mid-range device since that is the target user's likely device class.

### 3.4 Phase Dependencies

The dependency graph is correct and well-structured. Phase 1 (OCR) and Phase 2 (Backend) can genuinely run in parallel. Phase 3 (E2E) correctly requires both. No issues here.

---

## 4. Cross-Document Consistency

### 4.1 Contradictions

**[IMPORTANT] PRD says "on-device calculation" is possible for offline; TDD says calculation is backend-only.** PRD Section 6 (Out of Scope) says: "Offline calculation... can cache Davao irradiance data locally for offline fallback." The TDD Section 3.4 says offline mode can "use cached irradiance data + last-known pricing if previously fetched." The implementation plan Phase 3 includes "Use cached pricing/irradiance for calculation if available." However, the TDD architecture places ALL calculation logic in the backend `CalculationService.cs`. If the app is offline, it cannot call the API. For offline calculation to work, either:
- The calculation logic must be duplicated on the client, or
- The app must cache the last API response and show stale results, or
- Offline calculation is actually out of scope and should be stated clearly.

This is a significant architectural question that the three documents answer inconsistently.

**[IMPORTANT] Rate class handling is inconsistent.** The PRD mentions "rate class" extraction but focuses on residential users. The TDD API contract includes `rateClass` as a string parameter and the database schema has a `rate_class` column supporting "residential, commercial." The implementation plan Phase 2 edge cases mention "commercial rate class." But the PRD explicitly targets residential users only. Should the MVP support commercial rate calculations or not?

### 4.2 Terminology Inconsistencies

**[SUGGESTION] Minor naming inconsistency: "BOM" vs. "Bill of Materials."** The PRD uses "Bill of Materials (BOM)" consistently, but the TDD response schema uses `bom` as the JSON key. This is fine technically, but ensure the user-facing UI never shows the acronym "BOM" without explanation -- most homeowners will not know what it means.

**[SUGGESTION] "Peak Sun Hours" vs. "Solar Irradiance."** Documents use both terms somewhat interchangeably. Consider standardizing: "peak sun hours" is the user-facing term; "GHI" and "irradiance" are technical/backend terms.

### 4.3 Scope Agreement

All three documents generally agree on the MVP scope with the exceptions noted above (parameter adjustment, PDF sharing, offline calculation). The out-of-scope list in the PRD is clear and the TDD and implementation plan respect it.

---

## 5. Product Risks & Gaps

### 5.1 User Experience Issues

**[CRITICAL] No guidance on what happens after the user gets their estimate.** The app produces a solar estimate, and then... what? The user has a cost range, a materials list, and an ROI projection. But the PRD offers no next step. The user cannot:
- Request a quote from an installer
- Find a local installer
- Get financing information
- Schedule a site survey

The "installer marketplace" is listed as Post-MVP, but the MVP needs *some* call to action beyond "share this image." Even a static list of Davao solar installers with phone numbers would dramatically increase the app's utility. Without a next step, the app is interesting but not actionable, and users will not return or recommend it.

**[IMPORTANT] Single-bill estimation may mislead users.** Philippine electricity consumption varies significantly by season (higher in hot months due to AC). A user scanning their March bill (hot season, high consumption) will get a larger system recommendation than they might need year-round. A user scanning their December bill (cooler, lower consumption) will get an undersized recommendation. The app should at minimum:
- Ask the user if this is a typical month
- Display a disclaimer about seasonal variation
- Ideally, prompt the user to input their average monthly consumption if they know it

**[SUGGESTION] No onboarding or educational content.** The PRD's target user "may or may not have prior knowledge of solar energy." The app jumps straight to "Scan Your Bill." Consider a brief first-time onboarding flow (2-3 screens) explaining: what solar is, how rooftop solar works, and what this app will tell you. This builds trust and reduces confusion, especially for users unfamiliar with solar.

### 5.2 Market Positioning Concerns

**[IMPORTANT] Davao-only focus limits discoverability.** A Google Play Store search for "solar calculator Philippines" will surface national tools. Sinag's Davao-only focus is a strength for accuracy but a weakness for discoverability. The store listing should still target national keywords while making clear the app is optimized for Davao/DLPC users. Consider whether the architecture allows easy expansion to other utilities (the PRD roadmap mentions this but the TDD's template-based OCR approach supports it well).

**[SUGGESTION] No competitive response plan.** The market research identifies Solar Panda as the closest competitor. If Solar Panda adds OCR scanning (which is technically straightforward), Sinag loses its primary differentiator. The PRD should identify a secondary differentiator beyond OCR -- the detailed BOM and DLPC-specific accuracy are good candidates but should be explicitly positioned.

### 5.3 Monetization Timing

**[IMPORTANT] No monetization in MVP and no data collection for future monetization.** The PRD correctly keeps monetization out of MVP. However, the MVP also does not collect any data that would enable future monetization:
- No tracking of which system sizes are most popular (market intelligence for installers)
- No opt-in for "notify me when installer marketplace launches"
- No anonymous usage analytics planned (how many estimates per day, average system size, average bill amount)

The implementation plan Phase 4 mentions "basic analytics: number of estimates generated per day" but this is insufficient. Consider adding an anonymous analytics event layer in Phase 3 that captures aggregate data (no PII) to inform monetization strategy.

### 5.4 Data Freshness Risk

**[IMPORTANT] Equipment pricing update process is undefined.** The PRD says pricing should be checked quarterly and updated when LakaSolar publishes new data. But:
- Who checks for new pricing?
- How is the database updated? (No admin endpoint is built in the implementation plan.)
- What happens if prices change significantly between updates?
- The app shows "prices as of [date]" -- but what if that date is 6 months old?

For a tool that claims to give accurate estimates, stale pricing undermines trust. Define the update process, even if it is manual for MVP.

---

## Summary of Issues by Severity

### CRITICAL (3 issues -- must fix before development)

1. **"Adjust parameters" feature is in the PRD user flow but has no TDD or implementation plan support.** Decide IN or OUT for MVP and update all documents accordingly.
2. **No task for collecting and organizing DLPC test bill corpus.** OCR development cannot begin without test data. Add as a Phase 0 or pre-Phase 1 task.
3. **No post-estimate call to action.** The app produces an estimate but gives the user nowhere to go. Add at minimum a static installer directory or "what to do next" guidance.

### IMPORTANT (12 issues -- should fix)

1. No formal user stories with acceptance criteria in the PRD.
2. Missing edge-case flows (non-DLPC bill, denied permissions, very low consumption).
3. 500 MAU target has no user acquisition plan.
4. PDF sharing mentioned in PRD but absent from TDD/implementation plan.
5. "Save estimate" user benefit is unclear without user accounts.
6. Itemized charge extraction (OCR) is never used by the calculation engine.
7. Financial savings calculation is oversimplified (ignores fixed charges).
8. `EstimateRequest` missing billing month for month-specific irradiance lookup.
9. No admin pricing update endpoint in implementation plan.
10. Localization deferred too late (Phase 4 instead of Phase 0/1).
11. Offline calculation architecture is contradictory across documents.
12. Equipment pricing update process is undefined.

### SUGGESTIONS (12 items -- nice to have)

1. Add retention and engagement metrics (share rate, parameter adjustment rate).
2. Track manual OCR correction rate as a real-world accuracy metric.
3. Clarify Tagalog default vs. device locale behavior.
4. Add basic accessibility considerations for target demographic.
5. Note multi-month/seasonal averaging as a future enhancement.
6. Standardize "peak sun hours" vs. "irradiance" terminology.
7. Design the guided camera overlay UX before Phase 1.
8. Design the shareable summary card layout before Phase 3.
9. Specify device tiers for Phase 3 testing.
10. Add first-time onboarding/educational flow.
11. Plan competitive response if Solar Panda adds OCR.
12. Add anonymous analytics event layer in Phase 3 for future monetization data.

---

## Recommended Next Steps

1. **Resolve the three CRITICAL issues** before any development begins.
2. **Hold an alignment session** to reconcile the offline calculation contradiction and decide the technical approach.
3. **Collect DLPC test bills** immediately -- this is on the critical path for Phase 1.
4. **Update the API contract** to either accept itemized charges or remove itemized extraction from OCR scope.
5. **Add a "What's Next" screen** to the user flow -- even a simple static page with Davao solar installer contacts transforms the app from informational to actionable.
