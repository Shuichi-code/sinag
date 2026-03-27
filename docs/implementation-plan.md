# Sinag — Implementation Plan

**Version:** 1.1
**Date:** 2026-03-26
**Status:** Draft (revised after PM + Tech Lead review)
**Depends on:** [PRD v1.1](prd.md), [TDD v1.1](tdd.md)

---

## Guiding Principles

- **Quality over speed** — every phase must be tested before moving to the next
- **Vertical slices** — each phase delivers a working piece, not a horizontal layer
- **Risk-first** — tackle the hardest unknowns (OCR accuracy) before building around them
- **Minimal viable infrastructure** — don't set up what we don't need yet

---

## Phase 0: Project Setup

**Goal:** Solution structure, CI/CD pipeline, development environment, test data, and shared contracts ready.

### Tasks

1. **Create .NET solution structure**
   - `Sinag.sln`
   - `Sinag.App` (.NET MAUI project, target Android)
   - `Sinag.Api` (ASP.NET Core 8 Minimal API)
   - `Sinag.Shared` (shared contracts/models)
   - `Sinag.Api.Tests` (xUnit)
   - `Sinag.App.Tests` (xUnit, for BillParser and non-UI logic)

2. **Define shared contracts in `Sinag.Shared`**
   - `EstimateRequest.cs` — fields: `kwhConsumed`, `billingPeriodDays`, `billingMonth`, `generationChargePerKwh`, `totalAmountDue`, `includeBattery`
   - `EstimateResponse.cs` — system sizing, BOM by tier, financial analysis, metadata
   - `BomTier.cs`, `PricingResponse.cs`, `RatesResponse.cs`
   - These contracts must be agreed upon before Phase 1 and Phase 2 begin, since both phases depend on them.

3. **Configure .NET MAUI for Android**
   - Set minimum Android API level (API 24 / Android 7.0)
   - Configure Android permissions: Camera, Storage (gallery access), Internet
   - Verify MAUI builds and deploys to Android emulator

4. **Set up localization infrastructure**
   - Create string resource files for Tagalog (primary) and English
   - Set up MAUI localization pattern so all UI strings come from resource files from the start
   - Phase 1–3 will use Tagalog strings as they build UI. Phase 4 does final review/polish of translations.

5. **Configure ASP.NET Core API**
   - Minimal API with health check endpoint (`GET /api/health`)
   - PostgreSQL connection via Entity Framework Core + Npgsql
   - Parse Railway's `DATABASE_URL` (URI format) in `Program.cs`
   - Dockerfile (multi-stage build: SDK for build, ASP.NET runtime for deploy)
   - `appsettings.json` and environment variable configuration

6. **Railway deployment**
   - Create Railway project (Hobby plan, $5/month)
   - Provision PostgreSQL add-on
   - Deploy API via Dockerfile
   - Verify health check endpoint responds

7. **GitHub repository setup**
   - Initialize repo with `.gitignore` (Visual Studio / .NET)
   - GitHub Actions workflow: build + test on push
   - Branch protection on `main` (require passing CI)

8. **Generate Android signing key**
   - Generate Android signing keystore for release builds
   - Document secure storage location (never commit to repo)
   - This must be done early — losing the key means the app cannot be updated on Google Play

9. **Collect and catalog DLPC test bill corpus**
   - Organize user-provided sample DLPC bills into a test data directory
   - Minimum 10–15 unique bills
   - Create metadata file per bill: expected kWh, expected total amount, expected billing period, photo conditions
   - This is on the **critical path** for Phase 1 — OCR development cannot begin without test data

### Exit Criteria
- `dotnet build` succeeds for all projects
- Shared contracts defined and compiled in `Sinag.Shared`
- MAUI app launches on Android emulator with a blank home page (using localized strings)
- API health check returns 200 on Railway
- CI pipeline runs on push
- Test bill corpus organized with ≥10 bills and metadata
- Android signing keystore generated and stored securely

---

## Phase 1: OCR Engine (Highest Risk)

**Goal:** Prove that on-device OCR can reliably extract data from DLPC bills.

### Why First?
This is the core value proposition and the biggest technical risk. If OCR can't extract DLPC bill data reliably, the product concept needs to change. Validate this before building anything else.

### Tasks

1. **OCR plugin spike (do this first, 1-2 days)**
   - Install `Plugin.Maui.OCR` in a blank test project
   - Capture a photo and run text recognition
   - Inspect the returned data: does it include bounding boxes per `TextLine`?
   - Determine if bounding box detail is sufficient for anchor-based spatial parsing
   - **If insufficient:** switch to `Xamarin.Google.MLKit.TextRecognition` via platform-specific Android code in `Platforms/Android/`
   - Document the chosen approach before proceeding

2. **Image preprocessing**
   - Create `ImagePreprocessor.cs`
   - Resize captured images to ~2MP (12+ MP camera images slow OCR significantly)
   - Auto-rotate based on EXIF orientation data
   - Basic brightness/contrast normalization if needed

3. **Integrate OCR service**
   - Create `OcrService.cs` — takes preprocessed image bytes, returns text lines with bounding box positions
   - Test on emulator with sample bill images from the test corpus

4. **Build DLPC BillParser**
   - Analyze sample DLPC bills to map field positions and label text
   - Implement anchor-based extraction per the BillParser Specification in TDD Section 3.4:
     - Find "DAVAO LIGHT" or "DLPC" to confirm bill type → reject non-DLPC bills
     - Find "kWh" label → extract adjacent numeric value (same-line rule)
     - Find "Total Amount Due" → extract peso amount
     - Find "Billing Period" → extract date range (below-line rule)
     - Find "Generation" charge → extract per-kWh rate
     - Find account number
   - Implement regex validators for each field
   - Add confidence scoring per field (low-confidence = flag for user review)
   - Create BillParser unit tests using **fixture data** (pre-captured OCR output as JSON) — these run on CI without needing a device

5. **Build Scan UI**
   - Camera instruction screen: "Place your bill on a flat surface and photograph from directly above"
   - Launch system camera via `MediaPicker.CapturePhotoAsync()` (no custom overlay for MVP)
   - Gallery selection as alternative
   - Loading indicator during OCR processing
   - Note: Custom camera overlay with guided rectangle is a Phase 4 polish task

6. **Build Review UI**
   - Display extracted fields in editable form
   - Highlight low-confidence extractions in amber
   - "Looks Good" button to proceed, "Rescan" button to retry
   - If OCR fails entirely → offer "Enter Manually" link to ManualEntryPage

7. **Build Manual Entry fallback**
   - Simple form: kWh consumed, billing period (days), total amount due
   - Serves users whose bills can't be OCR'd (damaged, old format, etc.)

8. **Test with real DLPC bills**
   - Run full pipeline on all bills in the test corpus
   - Test under varying conditions: different lighting, angles, slightly crumpled
   - Measure extraction accuracy per field
   - Target: ≥90% accuracy on kWh and total amount fields
   - Track which bills fail and why — document failure patterns

### Exit Criteria
- OCR spike completed; OCR package chosen and documented
- OCR extracts kWh and total amount from ≥90% of test bill images
- BillParser correctly identifies spatial relationships between anchor text and values on ≥90% of test images
- User can scan a bill, see extracted data, correct any errors, and proceed
- Manual entry fallback works for users who can't scan
- BillParser has unit tests using fixture data (runs on CI)
- Works offline (no internet needed for scanning)

---

## Phase 2: Backend API + Calculation Engine

**Goal:** Backend calculates solar system recommendation from bill data.

### Tasks

1. **Database schema and seed data**
   - Create EF Core migrations for: `equipment_prices`, `component_specs`, `dlpc_rates`, `irradiance_cache`, `saved_estimates`
   - Seed equipment pricing from LakaSolar data (3 tiers × 6 categories, with unique constraints)
   - Seed component specs (panel wattages, inverter sizes, battery capacities per tier)
   - Seed DLPC residential rate schedule (all components — generation, transmission, distribution, system loss, taxes, subsidies)
   - Seed Davao irradiance data (12 monthly averages, manually entered from NASA POWER)

2. **NASA POWER API integration**
   - Create `IrradianceService.cs`
   - Fetch monthly average GHI for Davao City (7.0707°N, 125.6087°E)
   - Cache results in `irradiance_cache` table
   - Fallback to seed data if API is unreachable

3. **Calculation engine**
   - Create `CalculationService.cs`
   - Implement system sizing: kWh → daily consumption → required kWp (using month-specific peak sun hours) → panel count → inverter size
   - Implement battery sizing: `daily consumption × 0.33 (overnight fraction)`, rounded up to nearest standard size [5, 10, 15, 20] kWh
   - Apply performance ratio (0.78): inverter efficiency, cable loss, soiling, temperature derating
   - Create `BomService.cs` — generate BOM with 3 tiers using `component_specs` table, applying Davao discount to pricing

4. **Financial calculations**
   - Create `FinancialService.cs`
   - Monthly savings = offset kWh × **generation charge per kWh** (not blended rate)
   - Remaining bill = current bill − monthly savings (fixed charges remain)
   - Payback period = system cost / monthly savings
   - 25-year projection with panel degradation (−0.5%/yr) and rate increases (+3%/yr estimated)

5. **Estimate endpoint**
   - `POST /api/v1/estimate` — accepts `EstimateRequest`, returns `EstimateResponse`
   - Input validation: kWh 1–5000, billing period 25–35 days, billing month 1–12, generation charge 0.01–50.00
   - Sanity check: `totalAmountDue / kwhConsumed` should approximate blended DLPC rate; log warning if far off
   - Configure ASP.NET Core rate limiting middleware (fixed window, 100 req/IP/hour)

6. **Pricing and rates endpoints**
   - `GET /api/v1/pricing` — returns current tier prices (for app-side caching)
   - `GET /api/v1/rates` — returns current DLPC residential rate schedule (for app-side caching and display)

7. **Save estimate endpoint**
   - `POST /api/v1/estimates/save` — accepts estimate JSON + optional contact email
   - Email encrypted via EF Core value converter (AES-256, key from `ENCRYPTION_KEY` env var)
   - Records auto-expire after 90 days (`expires_at` column)
   - Cleanup: delete expired records on API startup

8. **Unit tests**
   - Calculation engine: verify system sizing math with known inputs/outputs
   - BOM generation: verify panel counts, inverter sizing, battery sizing match BOM rules
   - Financial calculations: verify savings use generation rate (not blended), payback period, 25-year projection
   - **Davao discount: explicitly verify discount is applied correctly to all pricing tiers**
   - Edge cases: very low consumption (50 kWh), very high (2000 kWh), with/without battery

9. **Integration tests**
   - Estimate endpoint: valid request → correct response structure
   - Validation: invalid inputs → appropriate error messages
   - Rate limiting: verify 429 response after exceeding limit
   - Pricing and rates endpoints return data
   - Save endpoint stores and encrypts correctly

### Exit Criteria
- `POST /api/v1/estimate` returns a complete BOM and financial analysis for test inputs
- Calculation math verified against manual calculations (including Davao discount)
- Financial savings correctly use generation charge, not blended rate
- Battery sizing formula produces correct results (worked example verified)
- All tests pass (including rate limiting and Davao discount verification)
- API deployed and reachable on Railway

---

## Phase 3: End-to-End Mobile Flow

**Goal:** Connect the mobile app to the backend. Full user journey from scan to "what's next."

### Tasks

1. **API client and caching**
   - Create `ApiClient.cs` in MAUI app
   - Create `CacheService.cs` — cache pricing, rates, and irradiance data locally
   - Handle connectivity detection (online/offline)
   - On first launch (online): fetch and cache pricing, rates, irradiance
   - Refresh cache when online and data is >7 days old

2. **Results UI**
   - BOM display with tier tabs or toggle (Budget / Mid-Range / Premium)
   - Component breakdown: panels, inverter, battery, mounting, wiring, labor
   - Price ranges per component and total
   - Financial summary: monthly savings (showing "generation charge savings"), payback period, 25-year projection
   - Davao solar production context ("Based on [X] peak sun hours in Davao for [month]...")
   - Seasonal variation disclaimer
   - Pricing disclaimer with "as of [date]"

3. **Battery toggle**
   - Option to include/exclude battery in estimate
   - Recalculate and redisplay when toggled (API call or offline recalculation)

4. **Save and share**
   - "Share" — generate a summary card image, share via Android share intent
   - "Email Me a Copy" — prompt for email, send via `POST /api/v1/estimates/save` (one-time, no account created)
   - Local save to device storage always available

5. **"What's Next" screen**
   - Shown after results, accessible from results page via button
   - Brief guidance: "Here's what to do next with your estimate"
   - Steps: 1) Review your estimate with family, 2) Contact an installer for a site survey, 3) Compare quotes
   - Static directory of Davao solar installers (company name, phone, website/Facebook)
   - Data stored locally in the app (hardcoded or JSON resource file)

6. **Home screen**
   - App landing page with "I-scan ang Iyong Bill" primary CTA (Tagalog)
   - Brief explainer in Tagalog with English technical terms
   - "Paano Ito Gumagana" section (3 steps: Scan → Review → Get Estimate)

7. **Offline handling**
   - Detect when offline
   - Allow scanning (OCR is on-device)
   - If cached data exists: compute estimate using cached pricing/rates/irradiance, show "prices as of [date]"
   - If no cached data exists: "Connect to the internet once to download current pricing data"
   - Queue email-save requests for when connectivity returns

8. **Error handling**
   - Non-DLPC bill → "Hindi ito mukhang DLPC bill. Ang Sinag ay sumusuporta lamang sa Davao Light bills."
   - OCR fails → "Hindi mabasa ang bill. Subukan muli sa mas maliwanag." + offer manual entry
   - API unreachable → attempt offline calculation or show retry
   - Camera permission denied → explain why camera is needed, link to app settings
   - Very low consumption (<100 kWh) → show estimate + advisory note about long payback

### Exit Criteria
- Full flow works: scan → review → calculate → display results → what's next
- Works offline with cached data, showing "prices as of [date]" notice
- Works offline with no cache, showing appropriate message
- Share generates a readable summary card
- Email-a-copy saves successfully via API
- "What's Next" screen shows installer directory
- Error states handled gracefully (all edge cases from PRD)
- Tested on at least 2 physical Android devices (at least one mid-range or lower)

---

## Phase 4: Polish and Launch Prep

**Goal:** App is ready for Google Play Store submission.

### Tasks

1. **Localization review**
   - Review all Tagalog strings for accuracy and naturalness
   - Ensure English technical terms are used consistently (kWh, kWp, etc.)
   - Language toggle in settings (default: follow device locale; if device is neither Tagalog nor English, default to Tagalog)

2. **UI/UX refinement**
   - Consistent visual design across all screens
   - Typography and spacing review
   - Ensure results are scannable — homeowners should understand the estimate within 10 seconds
   - Ensure text respects system font scaling up to 1.5x
   - All interactive elements have content descriptions (Android accessibility/TalkBack)
   - Test with non-technical users: "Do you understand what this estimate is telling you?"

3. **Camera overlay (optional polish)**
   - Evaluate `CommunityToolkit.Maui.Camera` or custom `CameraView` with `CameraX`
   - If feasible: add guided rectangle overlay to camera preview for bill alignment
   - If not feasible: keep system camera with instruction screen (already working from Phase 1)

4. **Performance optimization**
   - Profile OCR processing time on mid-range Android devices
   - Verify image preprocessing (resize to ~2MP) keeps OCR under 3 seconds
   - Lazy load results sections
   - Measure API warm response time (<2s target)
   - Send API warm-up request on app launch (while user is scanning, ~10s, API warms up in parallel)

5. **Google Play Store preparation**
   - App icon and feature graphic
   - Store listing: title, description (Tagalog + English), screenshots
   - Target national keywords ("solar calculator Philippines") while noting Davao/DLPC optimization in description
   - Privacy policy: no bill images leave device, email is optional and encrypted, data auto-deletes after 90 days, compliant with RA 10173 (Philippine Data Privacy Act)
   - Content rating questionnaire
   - Declare ML Kit usage in store listing
   - Release build configuration using signing key from Phase 0

6. **Final testing**
   - Test on 3+ physical Android devices (low-end, mid-range, flagship)
   - Test with all bills in the test corpus (≥10 real DLPC bills)
   - Test offline scenarios (with cache, without cache)
   - Test API under load (basic: 50 concurrent requests)
   - Verify edge cases: very low consumption (50 kWh), high consumption (2000 kWh), OCR failure → manual entry
   - End-to-end OCR accuracy ≥90% across all test bills

7. **Monitoring setup**
   - Railway deployment logs
   - API request logging (no PII) — count estimates per day for MAU tracking
   - Error tracking in API (unhandled exceptions)
   - Consider adding crash reporting SDK to mobile app (Sentry .NET MAUI or Firebase Crashlytics) — post-launch if not feasible before

### Exit Criteria
- App passes Google Play Store review
- OCR accuracy ≥90% across all test bills
- No critical bugs or crashes
- App size <50 MB
- API responds <2 seconds under normal load (warm)
- Accessibility basics pass (TalkBack, font scaling)
- Privacy policy published and linked
- Published to Google Play Store

---

## Phase Dependencies

```
Phase 0 ──→ Phase 1 ──→ Phase 3 ──→ Phase 4
         └──→ Phase 2 ──┘
```

- **Phase 0** must complete first (project infrastructure, shared contracts, test bill corpus)
- **Phase 1** (OCR) and **Phase 2** (Backend) can run in parallel after Phase 0 — they share only `Sinag.Shared` contracts (defined in Phase 0)
- **Phase 3** (E2E) requires both Phase 1 and Phase 2
- **Phase 4** (Polish) requires Phase 3

---

## Risk Checkpoints

| After Phase | Check | Action if Failed |
|-------------|-------|-----------------|
| Phase 0 | ≥10 test bills collected with metadata? | Cannot proceed to Phase 1. Source more bills. |
| Phase 1 (spike) | `Plugin.Maui.OCR` provides bounding box data? | Switch to `Xamarin.Google.MLKit.TextRecognition` via platform-specific code. |
| Phase 1 | OCR accuracy ≥90%? | Investigate cloud OCR fallback (Azure Computer Vision). Consider hybrid: on-device first, cloud retry for low-confidence results. |
| Phase 2 | Calculation accuracy verified? Financial savings use generation rate? | Review formulas against industry standards. Cross-check with Solar Panda's output for same inputs. |
| Phase 3 | Full flow works on real devices? Offline mode works? | Address device-specific issues. Test ML Kit compatibility across Android versions. |
| Phase 4 | User testing feedback positive? | Iterate on UX. Simplify results display. Add more contextual explanations. |

---

## Data Seeding Checklist

Before Phase 2 is complete, the following data must be seeded:

- [ ] Equipment prices: 6 categories × 3 tiers × min/max prices (36 data points) with unique constraints
- [ ] Component specs: panel wattages per tier (450W/550W/600W), inverter sizes [3,5,8,10,15 kW], battery sizes [5,10,15,20 kWh]
- [ ] DLPC residential rate schedule: all components — especially **generation charge per kWh** (used for savings calculation)
- [ ] Davao irradiance data: 12 monthly averages for GHI and peak sun hours (from NASA POWER)
- [ ] Davao solar installer directory: ≥3 local installers with contact info (for "What's Next" screen, stored in app)
