# Sinag -- Technical Architecture Review

**Reviewer:** Senior Tech Lead / Software Architect
**Date:** 2026-03-26
**Documents Reviewed:** PRD v1.0, TDD v1.0, Implementation Plan v1.0, Market Research
**Status:** Review Complete

---

## Executive Summary

The Sinag planning documents are well-structured and demonstrate strong product thinking. The mobile-backend split is sound, the risk-first implementation ordering is correct, and the core value proposition (OCR bill scanning) is clearly articulated. However, there are several technical issues that range from an incorrect package name (blocking) to missing architectural details that will cause friction during implementation. This review identifies 7 critical issues, 10 important issues, and 12 suggestions.

---

## 1. Architecture Review

### 1.1 Mobile-Backend Split

**SUGGESTION** -- Consider moving calculation logic to the device.

The TDD places all calculation logic on the backend with the rationale that "pricing data changes over time." This is valid, but the calculation engine itself is pure math with no external dependencies beyond reference data. An alternative architecture worth considering:

- Backend serves **reference data only** (pricing tiers, DLPC rates, irradiance cache) via `GET` endpoints.
- Mobile app performs all **calculations locally** using cached reference data.
- This eliminates the `POST /api/estimate` round-trip, improves offline capability, and reduces API surface area.

The current architecture is not wrong -- centralizing calculation does simplify updates. But it creates a hard dependency on network connectivity for the core feature. The PRD lists offline calculation as "out of scope" but the TDD then describes offline calculation with cached data in Section 3.4. This contradiction needs resolution (see Section 7).

### 1.2 API Contract Design

**IMPORTANT** -- The `POST /api/estimate` request is missing fields the backend needs.

The `EstimateRequest` includes `kwhConsumed`, `billingPeriodDays`, `rateClass`, `totalAmountDue`, and `includeBattery`. However:

- The calculation engine description (Section 4.3) references "panel wattage" as a variable (e.g., 550W), but the request doesn't let the client specify this. Is it always 550W? This should be documented as a backend constant or made configurable.
- `totalAmountDue` is sent but never used in the calculation formulas described. The system sizes from kWh, not from peso amount. Either remove it from the request or document how it's used (it could be used for financial validation -- "your bill of P6,215 at P13.82/kWh implies ~450 kWh" as a sanity check).
- The response includes `estimatedMonthlyBillAfterSolar` of P1,200 but the calculation section doesn't explain how the post-solar bill is computed. What components remain? Grid connection fee? Demand charges? This needs to be specified.

**SUGGESTION** -- Add API versioning from day one.

The endpoint paths (`/api/estimate`, `/api/pricing`) have no version prefix. Use `/api/v1/estimate` to allow non-breaking evolution. This is trivial to add now and painful to retrofit after the app is in the Play Store.

**SUGGESTION** -- Add a `GET /api/rates` endpoint.

The app needs DLPC rate data for offline calculations and for the review screen (validating extracted bill amounts). The TDD mentions `RateService.cs` but doesn't expose a rates endpoint. Add `GET /api/rates` to serve current DLPC rate schedules.

### 1.3 Database Schema

**IMPORTANT** -- `equipment_prices` table is missing a composite unique constraint.

There is no unique constraint on `(category, tier, effective_date)`. Without this, seed data could be inserted multiple times, and queries like "get current price for budget panels" would return duplicates. Add:

```sql
UNIQUE (category, tier, effective_date)
```

**IMPORTANT** -- `equipment_prices` schema doesn't support the BOM generation logic described.

The calculation engine needs to map a system size (e.g., 5 kWp) to specific component specs (e.g., "550W x 10 panels", "5kW Hybrid inverter"). The `equipment_prices` table stores per-unit prices but has no columns for:

- `spec_template` (e.g., "550W x {count}" for panels, "{size}kW Hybrid" for inverters)
- `size_options` (available panel wattages, inverter sizes, battery capacities)
- `scaling_unit` (e.g., panels scale per-watt, inverters scale per-kW step)

The response shows specs like `"550W x 10"` and `"5kW Hybrid"`, but there's no data structure to drive this. The calculation service would need to hardcode these mappings, making the "update pricing without an app update" benefit moot.

**Fix:** Add a `component_specs` table or extend `equipment_prices` with spec-related columns:

```sql
CREATE TABLE component_specs (
    id SERIAL PRIMARY KEY,
    category VARCHAR(50) NOT NULL,
    tier VARCHAR(20) NOT NULL,
    wattage_or_capacity DECIMAL(10,2),  -- 550 for panels, 5 for inverters, 10 for batteries
    unit VARCHAR(20),                    -- W, kW, kWh
    spec_label_template VARCHAR(100),   -- "{wattage}W x {count}"
    available_sizes JSONB,              -- [550, 450, 400] for panels; [3, 5, 8, 10] for inverters
    effective_date DATE NOT NULL
);
```

**SUGGESTION** -- `saved_estimates.contact_info` encryption approach needs clarification.

The TDD says "encrypted at rest (AES-256)" but doesn't specify the implementation. In EF Core, this is typically done via value converters. The encryption key management strategy is not documented. For MVP, consider:

- Using PostgreSQL's `pgcrypto` extension for column-level encryption, or
- Using an EF Core value converter with a key from environment variables.

Document the chosen approach in the TDD so implementers aren't left guessing.

### 1.4 Deployment Architecture

**CRITICAL** -- Railway does not have a permanent free tier.

The TDD states "PostgreSQL free tier limits" as a low-risk item, implying Railway offers a free tier. Railway's current pricing model offers only a one-time $5 trial credit valid for 30 days. After that, the Hobby plan costs $5/month (with $5 of included usage), and the Pro plan is $20/month. PostgreSQL on Railway is billed by resource usage on top of this.

This is not a blocker for the project, but the documents should explicitly acknowledge the ongoing hosting cost. At MVP traffic levels, the Hobby plan ($5/month) should be sufficient, but this needs to be stated.

**Fix:** Update TDD Section 6 and Section 9 (risk table) to reflect actual Railway pricing.

**IMPORTANT** -- Railway cold start mitigation is underspecified.

The TDD mentions "health check pings to keep container warm" but doesn't describe the implementation. Railway containers on the Hobby plan will sleep after periods of inactivity. Options:

1. An external cron service (e.g., UptimeRobot free tier) pinging `/api/health` every 5-10 minutes.
2. The MAUI app sending a "warm-up" request on app launch before the user finishes scanning.
3. Accepting cold starts and showing a graceful loading state (simplest for MVP).

Document the chosen strategy.

---

## 2. Technology Choices

### 2.1 Plugin.Maui.MLKit

**CRITICAL** -- `Plugin.Maui.MLKit` does not exist as a NuGet package.

The TDD references "ML Kit (Google) via Plugin.Maui.MLKit" in the technology stack table. This package does not exist on NuGet. The actual options for ML Kit text recognition in .NET MAUI are:

1. **`Plugin.Maui.OCR`** (NuGet: [Plugin.Maui.OCR 1.1.1](https://www.nuget.org/packages/Plugin.Maui.OCR)) -- A community plugin that wraps platform-native OCR: ML Kit on Android, Vision Framework on iOS, Windows.Media.Ocr on Windows. Provides confidence scores and bounding box detection. This is the closest match to what the TDD describes.

2. **`Xamarin.Google.MLKit.TextRecognition`** (NuGet: [v116.0.1.5](https://www.nuget.org/packages/Xamarin.Google.MLKit.TextRecognition/)) -- Official Microsoft-maintained .NET for Android bindings for `com.google.mlkit:text-recognition`. These work in MAUI but are Android-only and require writing platform-specific code.

3. **`Xamarin.GooglePlayServices.MLKit.Text.Recognition`** (NuGet: [v119.0.1.5](https://www.nuget.org/packages/Xamarin.GooglePlayServices.MLKit.Text.Recognition/)) -- Play Services bundled version (smaller APK, model downloaded at runtime). Also Android-only.

4. **Platform-specific implementation via MAUI handlers** -- Write Android-native ML Kit code in the `Platforms/Android` folder, expose via dependency injection. Most control, most effort.

**Recommended fix:** For MVP (Android-first), use `Plugin.Maui.OCR` as the primary approach. It wraps ML Kit on Android and provides the bounding box data needed for spatial/anchor-based parsing. If `Plugin.Maui.OCR` doesn't expose sufficient bounding box detail for the anchor-based template matching strategy, fall back to direct use of `Xamarin.Google.MLKit.TextRecognition` via platform-specific code.

**Action:** Update the TDD technology stack table and implementation plan Phase 1 to reference the correct package name. Add a spike task in Phase 1 to evaluate whether `Plugin.Maui.OCR` exposes the bounding box data needed for spatial field extraction, before committing to the BillParser architecture.

### 2.2 .NET MAUI Maturity

**IMPORTANT** -- .NET MAUI is mature enough but has known pain points on Android.

.NET MAUI with .NET 8 (and now .NET 9) is stable for production Android apps. However, common friction areas relevant to Sinag include:

- **Camera APIs:** MAUI doesn't have a built-in camera capture view. The `MediaPicker` API can launch the system camera app, but a custom camera overlay (the "guided rectangle frame" described in Phase 1) requires either a third-party library (e.g., `CommunityToolkit.Maui.Camera`) or a custom platform renderer. The implementation plan doesn't account for this complexity.
- **Image handling:** Converting between MAUI `ImageSource`, byte arrays, and platform-native image types (Android `Bitmap`) for OCR input can be fiddly. Budget time for this.
- **APK size:** The TDD targets <50 MB. A baseline MAUI app is ~15-20 MB. ML Kit bundled model adds ~5-10 MB. This target is achievable but tight if you include localization resources and images.

**Fix:** Add a sub-task to Phase 1 for camera overlay implementation research. Evaluate `CommunityToolkit.Maui.Camera` or the MAUI `CameraView` (if using .NET 9). The current plan assumes camera capture is trivial -- it is not in MAUI.

### 2.3 EF Core with PostgreSQL on Railway

**No issues found.** EF Core + Npgsql + PostgreSQL on Railway is a well-supported, well-documented combination. The `Npgsql.EntityFrameworkCore.PostgreSQL` provider is actively maintained. Railway supports PostgreSQL natively with automatic provisioning. The `DATABASE_URL` environment variable pattern is standard on Railway.

One minor note: Railway provides the connection string in URI format (`postgresql://user:pass@host:port/db`). Npgsql can parse this, but ensure the connection string is converted correctly in `Program.cs`. Use `builder.Configuration.GetConnectionString()` or parse the `DATABASE_URL` env var explicitly.

### 2.4 ASP.NET Core Version

**SUGGESTION** -- Specify .NET 9 instead of .NET 8.

The TDD specifies "ASP.NET Core 8" but .NET 9 has been available since November 2025 and is the current release. While .NET 8 is the LTS version (supported until November 2026), .NET 9 brings performance improvements and MAUI improvements relevant to this project. Either choice is valid, but the documents should explicitly justify the version choice. If sticking with .NET 8 for LTS stability, state that. If using .NET 9 for latest features, update the TDD.

---

## 3. Technical Feasibility

### 3.1 OCR Anchor-Based Template Matching

**CRITICAL** -- The BillParser spatial extraction strategy needs more specificity.

The TDD describes finding anchor text ("DAVAO LIGHT", "kWh", "Total Amount Due") and extracting "adjacent values based on spatial position." This approach is viable for structured utility bills, but the TDD doesn't specify:

1. **What "adjacent" means quantitatively.** Is it "the text block within 50px to the right"? "The next text block in reading order"? ML Kit returns text blocks with bounding boxes -- the parsing logic needs to define spatial relationships precisely (e.g., "find the text block whose bounding box is within the same horizontal band as the anchor, to its right, within 200px").

2. **How to handle multi-line label-value pairs.** On many utility bills, the label is on one line and the value is directly below. The parser needs both horizontal and vertical adjacency logic.

3. **What happens when ML Kit merges or splits text blocks unpredictably.** ML Kit's text block segmentation is not deterministic -- the same image can produce different block boundaries. The parser should work at the `TextLine` or `TextElement` level, not just `TextBlock`.

4. **Rotation and perspective handling.** The TDD mentions "identify bill orientation and normalize" but doesn't specify the approach. ML Kit v2 can detect text orientation, but perspective correction (e.g., photographing a bill at an angle on a table) requires either OpenCV-style homography correction or relying on ML Kit's built-in handling (which is limited).

**Fix:** Add a "BillParser Specification" sub-section to the TDD that defines:
- The ML Kit API level used (TextBlock vs. TextLine vs. TextElement)
- Spatial relationship definitions (e.g., "value is the numeric content within the same TextLine as the anchor, or in the TextLine directly below and horizontally overlapping by >50%")
- A decision on whether to support only flat/aligned photos (simpler) or also angled photos (requires preprocessing)
- A labeled diagram of a DLPC bill showing the 5-6 extraction zones

### 3.2 Performance Targets

**SUGGESTION** -- The "API cold start <5 seconds" target may be optimistic on Railway Hobby plan.

ASP.NET Core cold start in a Docker container involves container pull + .NET runtime initialization + EF Core model compilation. On Railway's shared infrastructure, this can take 8-15 seconds for the first request after sleep. Options:

- Accept longer cold starts and have the app show a loading state.
- Use AOT (Ahead-of-Time) compilation to reduce .NET startup time.
- Keep the container warm (see Section 1.4).

The <15 second scan-to-result target is achievable when the API is warm (OCR ~2-3s + user review ~5-7s + API call ~1-2s = ~10-12s). When cold, it could be 20-25s. Document this distinction.

### 3.3 Offline Calculation Feasibility

**CRITICAL** -- Offline calculation is both in-scope and out-of-scope.

The PRD Section 6 ("Out of Scope for MVP") lists:
> "Offline calculation (OCR is offline, but system calculation needs NASA POWER API data -- can cache Davao irradiance data locally for offline fallback)"

But the TDD Section 3.4 describes offline calculation working with cached data, and the Implementation Plan Phase 3 Task 6 includes "Use cached pricing/irradiance for calculation if available."

This is a direct contradiction. Either:

1. **Offline calculation is in scope:** Remove it from the PRD's out-of-scope list. The implementation plan already accounts for it. This is the better choice since it significantly improves UX (users in areas with spotty connectivity can still get results).

2. **Offline calculation is out of scope:** Remove it from TDD Section 3.4 and Implementation Plan Phase 3 Task 6. Simplify to: "If offline, show message: 'Internet connection required for calculation. Your scanned bill data has been saved.'"

**Recommendation:** Keep offline calculation in scope. The cached data approach is straightforward and the implementation plan already budgets for it. Just fix the PRD to reflect this.

### 3.4 Integration Risks Not Identified

**IMPORTANT** -- Missing risk: Google Play Store ML Kit policy compliance.

Apps using ML Kit must comply with Google's ML Kit Terms of Service, which include data handling requirements. While Sinag processes data on-device (good), the app must still declare ML Kit usage in the Play Store listing and may be subject to review. This is a low-risk item but should be listed in the risk table.

**IMPORTANT** -- Missing risk: DLPC bill format variations.

The documents assume DLPC bills have a single consistent format (Oracle CCB-generated). In practice, bills may vary by:
- Statement type (regular monthly vs. final bill vs. reconnection)
- Customer segment (residential vs. commercial -- different layouts)
- Printing quality (thermal print vs. digital PDF)
- Bill vintage (format may have changed in past years, and users may scan old bills)

The BillParser should handle at least the "this is not a recognized bill format" case gracefully. This is partially addressed but should be an explicit test case.

---

## 4. Security Review

### 4.1 Rate Limiting

**CRITICAL** -- Rate limiting implementation is not specified.

The TDD states "100 requests/IP/hour" but doesn't specify how this will be implemented. In ASP.NET Core, the options are:

1. **Built-in Rate Limiting middleware** (`Microsoft.AspNetCore.RateLimiting`, available since .NET 7) -- Use a fixed window or sliding window limiter. This is the recommended approach.

2. **Third-party** (e.g., `AspNetCoreRateLimit` NuGet package) -- More features but unnecessary for MVP.

3. **Railway/reverse proxy level** -- Railway doesn't provide built-in rate limiting.

**Fix:** Add to TDD Section 5:

```csharp
// Program.cs
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("estimate", opt =>
    {
        opt.PermitLimit = 100;
        opt.Window = TimeSpan.FromHours(1);
    });
});
```

Also note: IP-based rate limiting on mobile networks is unreliable. Many mobile users share IPs via carrier-grade NAT. 100/hour/IP might be too aggressive for shared IPs, or too lenient for a determined abuser. For MVP this is acceptable, but document the limitation.

### 4.2 Data Privacy

**IMPORTANT** -- The `saved_estimates` table stores PII without a retention policy.

The table stores `contact_info` (email or phone) and `estimate_json` (which contains consumption data). The privacy policy mentioned in the implementation plan (Phase 4, Task 4) should define:

- How long saved estimates are retained (e.g., 90 days? 1 year? forever?)
- How users can request deletion of their data (Philippine Data Privacy Act of 2012 / RA 10173 requires this)
- Whether `estimate_json` itself contains PII (it contains consumption patterns, which could be considered personal data)

**Fix:** Add a `expires_at` column to `saved_estimates` and implement a cleanup job. Document the retention policy.

### 4.3 API Input Validation

**SUGGESTION** -- The validation ranges should be tighter and documented.

The TDD mentions "kWh must be 1-99999, billing period 20-40 days" but:
- 99,999 kWh/month is an unrealistic residential bill. Cap at 5,000 kWh for residential rate class.
- Billing period 20-40 days is reasonable, but most DLPC bills are exactly 30-31 days. Consider 25-35 for stricter validation with an override.
- `rateClass` should be validated as an enum, not a free string.
- `totalAmountDue` should have a reasonable ceiling (e.g., P500,000 for residential).

### 4.4 Admin Endpoint Security

**IMPORTANT** -- The pricing update mechanism is underspecified.

The TDD mentions "admin endpoint" for updating pricing but doesn't describe authentication for it. For MVP, options include:

1. **API key in header** -- Simple, adequate for MVP. Store key in Railway environment variables.
2. **Seed data only** -- Don't expose an admin endpoint at all. Update prices via database migration or seed script, redeploy.

Option 2 is simpler and more secure for MVP. If going with option 1, document it in the TDD.

---

## 5. Implementation Plan Technical Review

### 5.1 Phase Ordering

**No issues.** The risk-first ordering (OCR in Phase 1 before backend in Phase 2) is correct. The dependency graph is sound:

```
Phase 0 --> Phase 1 -+-> Phase 3 --> Phase 4
         \-> Phase 2 -/
```

### 5.2 Underestimated or Missing Tasks

**CRITICAL** -- Phase 1 underestimates camera overlay complexity.

Phase 1 Task 3 says "Camera capture page with guided overlay (rectangle frame for bill alignment)." In .NET MAUI, this is non-trivial:

- `MediaPicker.CapturePhotoAsync()` launches the system camera app -- you cannot add a custom overlay to it.
- A custom camera preview with overlay requires either:
  - `CommunityToolkit.Maui.Camera` (community package, evaluate stability)
  - A custom `CameraView` using platform-specific Android `CameraX` API via MAUI handlers
  - Using `MediaPicker` without the overlay (simplest, but loses the guided capture UX)

**Fix:** Add a sub-task: "Evaluate camera overlay approach: MediaPicker (no overlay) vs. CommunityToolkit.Maui.Camera vs. custom CameraX handler. Decide based on MVP scope." For MVP, consider launching the system camera (no overlay) with a "how to photograph your bill" instruction screen shown beforehand. This dramatically reduces Phase 1 complexity.

**IMPORTANT** -- Phase 1 is missing image preprocessing.

Before feeding an image to ML Kit, you typically need to:
- Resize (large phone camera images of 12+ MP will slow OCR)
- Convert color space if needed
- Potentially auto-crop to the bill region

Add a task: "Implement image preprocessing pipeline (resize to ~2MP, auto-rotate based on EXIF)."

**IMPORTANT** -- Phase 2 is missing the `POST /api/estimate/save` endpoint.

Phase 3 Task 4 mentions "Save Estimate -- prompt for email or phone (optional), store via API." But Phase 2 doesn't include building a save endpoint. Either:
- Add `POST /api/estimates/save` to Phase 2, or
- Clarify that saving is local-only for MVP and the `saved_estimates` table is post-MVP

**IMPORTANT** -- Phase 4 is missing Android signing key setup.

Phase 4 Task 4 mentions "Signing key and release build configuration" but this should be done earlier. The signing key should be generated in Phase 0 and securely stored. Losing a signing key means you cannot update the app on Google Play. Add to Phase 0: "Generate Android signing keystore, document secure storage location."

**SUGGESTION** -- Phase 2 Task 5 mentions rate limiting but doesn't list it as a development task.

Rate limiting middleware needs to be configured and tested. Add an explicit sub-task under Phase 2 Task 5: "Configure ASP.NET Core rate limiting middleware (fixed window, 100 req/IP/hour)."

### 5.3 Exit Criteria Review

**SUGGESTION** -- Phase 1 exit criteria should include a bounding box data verification.

Current exit criteria: "OCR extracts kWh and total amount from >=90% of test bill images." Add: "BillParser correctly identifies spatial relationships between anchor text and values on >=90% of test images." This verifies the template matching approach, not just raw OCR.

**SUGGESTION** -- Phase 3 exit criteria should include offline scenario testing.

"Works offline with cached data" is listed but should specify: "After one successful online estimate, subsequent estimates work offline using cached pricing and irradiance data. Displayed results show 'prices as of [date]' notice."

### 5.4 Parallel Phase Feasibility

**Phase 1 and Phase 2 can run in parallel** -- confirmed. They share only the `Sinag.Shared` contracts, which should be defined at the start of both phases. However:

**IMPORTANT** -- The `EstimateRequest`/`EstimateResponse` contracts in `Sinag.Shared` must be agreed upon before Phase 1 and Phase 2 diverge.

If Phase 1 builds the review screen and Phase 2 builds the API endpoint independently, they may disagree on field names, types, or required fields. Add to Phase 0 exit criteria: "Shared contracts (`EstimateRequest`, `EstimateResponse`, `BomTier`) are defined and agreed upon in `Sinag.Shared`."

---

## 6. TDD Quality

### 6.1 Implementability

**IMPORTANT** -- The BOM generation algorithm is not specified.

The TDD shows what the BOM output looks like (Section 4.2 response example) and lists the system sizing formulas (Section 4.3), but doesn't specify the BOM generation algorithm:

- How does system size map to specific components? (e.g., is 5 kWp always 10x550W panels, or could it be 12x450W?)
- How are inverter sizes selected? (Nearest standard size above system kWp? e.g., 4.8 kWp -> 5kW inverter)
- How is battery capacity calculated? (Section 4.3 says "daily consumption x desired autonomy hours (default: 8h)" but 15 kWh/day x 8h = 5 kWh, not the 10 kWh shown in the example. The formula seems to be `daily_consumption * (autonomy_hours / 24)` but this isn't stated.)
- How do the three pricing tiers differ in component selection? (Same components, different brands/quality? Or different panel wattages per tier?)

**Fix:** Add a "BOM Generation Rules" sub-section that specifies:
- Panel wattage per tier (e.g., Budget: 450W, Mid: 550W, Premium: 600W)
- Inverter sizing rule (round up to nearest of [3, 5, 8, 10, 15] kW)
- Battery sizing formula with worked example
- How tiers differ (component quality/brand tier vs. different configurations)

### 6.2 Ambiguous Technical Decisions

**IMPORTANT** -- Navigation pattern is not specified.

The TDD describes MVVM with Community Toolkit but doesn't specify the navigation approach:
- Shell navigation? (recommended for MAUI)
- NavigationPage with push/pop?
- Tab-based?

For the linear flow (Home -> Scan -> Review -> Results), Shell navigation with route-based navigation is appropriate. Document this.

**SUGGESTION** -- Dependency injection registrations are not documented.

The TDD mentions DI but doesn't specify which services are singleton vs. scoped vs. transient. For MAUI:
- `OcrService` -- singleton (stateless, reusable)
- `ApiClient` -- singleton (wraps `HttpClient`, which should be singleton)
- `ConnectivityService` -- singleton
- ViewModels -- transient (new instance per page navigation)

### 6.3 Testing Strategy

**IMPORTANT** -- No testing strategy for the OCR pipeline on CI.

The testing strategy lists "OCR parsing: Unit tests with sample bill images" but ML Kit requires an Android device or emulator. These tests cannot run on a standard CI agent (GitHub Actions Linux runner). Options:

1. **Split the tests:** Unit test `BillParser` with pre-extracted ML Kit output (raw text blocks + bounding boxes as JSON fixtures). This tests the parsing logic without ML Kit. Test ML Kit integration separately on a physical device.
2. **Use Android emulator on CI:** Possible but slow and resource-intensive. Not recommended for MVP.

**Fix:** Add to testing strategy: "BillParser unit tests use fixture data (pre-captured ML Kit output as JSON). End-to-end OCR tests run on physical devices during Phase 1 and Phase 4."

### 6.4 Missing Non-Functional Requirements

**SUGGESTION** -- No accessibility requirements are documented.

Android accessibility (TalkBack, font scaling, color contrast) should be mentioned even for MVP. At minimum: "All interactive elements have content descriptions. App respects system font scaling up to 1.5x."

**SUGGESTION** -- No crash reporting or analytics SDK is specified.

Phase 4 mentions "error tracking in API" but not in the mobile app. For a Play Store app, consider adding:
- **Sentry .NET MAUI** or **App Center** (deprecated but still functional) or **Firebase Crashlytics** for crash reporting.
- **Firebase Analytics** or a lightweight alternative for basic usage metrics.

Add to Phase 4 or document as post-MVP.

**SUGGESTION** -- No app update strategy is documented.

When the DLPC bill format changes or the API contract evolves, how are old app versions handled? Consider:
- Minimum supported app version check on API response headers.
- Force-update mechanism for breaking changes.

This is post-MVP but worth noting in the TDD's risk section.

---

## 7. Cross-Document Technical Consistency

### 7.1 Offline Calculation Contradiction

**CRITICAL** -- Already detailed in Section 3.3.

- PRD Section 6: "Out of scope for MVP"
- TDD Section 3.4: Describes offline calculation with cached data
- Implementation Plan Phase 3 Task 6: Includes offline calculation implementation

**Fix:** Align all three documents. Recommend: keep offline calculation in scope, remove from PRD "out of scope" list.

### 7.2 Shared Models Consistency

**SUGGESTION** -- The `Sinag.Shared` contracts are defined in the TDD (Section 3.1 project structure) but the actual fields are only shown in the API endpoint section (Section 4.2). Consolidate by listing all shared model fields in one canonical location.

Specifically, `BillData.cs` (app-side model from OCR extraction) contains fields that partially overlap with `EstimateRequest.cs` (sent to API). Document the mapping: which BillData fields map to which EstimateRequest fields, and which BillData fields are not sent to the API (e.g., billing period dates, itemized charges).

### 7.3 Itemized Charges Extraction vs. Usage

**IMPORTANT** -- The PRD and OCR pipeline extract itemized charges (generation, transmission, distribution, system loss) but these are never used in the calculation engine.

The PRD Section 4 step 4 lists extraction of "Itemized charges (generation, transmission, distribution, system loss)." The BillParser extracts these. But the `EstimateRequest` doesn't include them, and the calculation engine doesn't use them.

Either:
1. Remove itemized charge extraction from PRD/TDD scope (simplifies OCR work), or
2. Add them to the estimate request and use them for more accurate financial calculations (e.g., solar only offsets generation charges, not all charge components).

Option 2 is more accurate. Solar net metering in the Philippines offsets the generation charge primarily, not the full blended rate. Using the blended rate overstates savings. This is a calculation accuracy issue that affects the core value proposition.

**Fix:** If pursuing accuracy, the financial calculation should be:
```
Monthly savings = offset_kwh * generation_rate_per_kwh
(not: offset_kwh * total_blended_rate_per_kwh)
```

At minimum, document the simplification and its impact on estimate accuracy.

### 7.4 Battery Sizing Inconsistency

**IMPORTANT** -- The battery sizing formula in TDD Section 4.3 doesn't match the example response.

Formula: `Battery capacity = daily consumption x desired autonomy hours (default: 8h backup)`

For 450 kWh/month with 30-day billing:
- Daily consumption = 450 / 30 = 15 kWh/day
- Battery capacity = 15 kWh/day * 8h = 120 kWh (this is absurd)

The example response shows 10 kWh, which suggests the actual formula is something like:
- Battery capacity = daily consumption * (autonomy_hours / 24) = 15 * (8/24) = 5 kWh, then rounded up to a standard size (10 kWh).

Or perhaps:
- Battery capacity = average overnight consumption (e.g., 40% of daily = 6 kWh), rounded to nearest standard battery (5, 10, 15, 20 kWh).

**Fix:** Correct the battery sizing formula and add a worked example that matches the response output.

### 7.5 Success Metrics Alignment

**SUGGESTION** -- The PRD success metric "500+ MAU at 3 months post-launch" has no technical instrumentation plan. How will MAU be measured without user accounts or analytics? The app needs some form of anonymous usage tracking (e.g., Firebase Analytics) or API-side request counting. Add to Phase 4 monitoring setup.

---

## Summary of Issues

### Critical (7) -- Must fix before development starts

| # | Issue | Location | Section |
|---|-------|----------|---------|
| C1 | `Plugin.Maui.MLKit` does not exist as a NuGet package | TDD Section 2 | 2.1 |
| C2 | Offline calculation is both in-scope and out-of-scope | PRD S6, TDD S3.4, IP Phase 3 | 3.3, 7.1 |
| C3 | Battery sizing formula is mathematically incorrect | TDD Section 4.3 | 7.4 |
| C4 | Rate limiting implementation not specified | TDD Section 5 | 4.1 |
| C5 | Railway has no permanent free tier | TDD Sections 6, 9 | 1.4 |
| C6 | BillParser spatial extraction strategy lacks specificity | TDD Section 3.3 | 3.1 |
| C7 | Camera overlay requires custom implementation, not accounted for | IP Phase 1 | 5.2 |

### Important (10) -- Should fix before implementation

| # | Issue | Location | Section |
|---|-------|----------|---------|
| I1 | `EstimateRequest` is missing fields / has unused fields | TDD Section 4.2 | 1.2 |
| I2 | `equipment_prices` table lacks unique constraint | TDD Section 4.4 | 1.3 |
| I3 | `equipment_prices` schema can't drive BOM spec generation | TDD Section 4.4 | 1.3 |
| I4 | BOM generation algorithm not specified | TDD Section 4.3 | 6.1 |
| I5 | Image preprocessing task missing from Phase 1 | IP Phase 1 | 5.2 |
| I6 | Save estimate endpoint missing from Phase 2 | IP Phase 2 | 5.2 |
| I7 | Itemized charges extracted but never used | PRD S4, TDD S4.2 | 7.3 |
| I8 | No CI testing strategy for OCR pipeline | TDD Section 7 | 6.3 |
| I9 | Navigation pattern not specified | TDD Section 3.2 | 6.2 |
| I10 | `saved_estimates` PII retention policy missing | TDD Section 4.4 | 4.2 |

### Suggestions (12) -- Would strengthen the design

| # | Issue | Location | Section |
|---|-------|----------|---------|
| S1 | Consider moving calculation to device | TDD Section 1 | 1.1 |
| S2 | Add API versioning (`/api/v1/...`) | TDD Section 4.2 | 1.2 |
| S3 | Add `GET /api/rates` endpoint | TDD Section 4.2 | 1.2 |
| S4 | Document encryption key management approach | TDD Section 4.4 | 1.3 |
| S5 | Consider .NET 9 vs .NET 8, document reasoning | TDD Section 2 | 2.4 |
| S6 | Cold start target may be optimistic | TDD Section 8 | 3.2 |
| S7 | Tighten input validation ranges | TDD Section 5 | 4.3 |
| S8 | Document DI service lifetimes | TDD Section 3.2 | 6.2 |
| S9 | Add accessibility requirements | TDD | 6.4 |
| S10 | Add crash reporting SDK to plan | IP Phase 4 | 6.4 |
| S11 | Consolidate shared model documentation | TDD Sections 3.1, 4.2 | 7.2 |
| S12 | Define MAU measurement instrumentation | PRD Section 8, IP Phase 4 | 7.5 |

---

## Recommended Next Steps

1. **Fix the 7 critical issues** -- These are all addressable with document updates, no architectural changes needed.
2. **Run a spike** (1-2 days) -- Before committing to Phase 1, do a quick proof-of-concept:
   - Install `Plugin.Maui.OCR` in a blank MAUI project.
   - Capture a photo and run text recognition.
   - Inspect the bounding box data returned.
   - Determine if it's sufficient for anchor-based template matching.
   - This de-risks the biggest unknown and validates the core technology choice.
3. **Define shared contracts in Phase 0** -- `EstimateRequest`, `EstimateResponse`, and `BomTier` should be finalized before Phase 1 and Phase 2 fork.
4. **Update all three documents** for consistency, then re-review the critical items before writing code.
