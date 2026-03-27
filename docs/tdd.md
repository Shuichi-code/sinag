# Sinag — Technical Design Document (TDD)

**Version:** 1.1
**Date:** 2026-03-26
**Status:** Draft (revised after PM + Tech Lead review)
**Depends on:** [PRD v1.1](prd.md)

---

## 1. Architecture Overview

Sinag is a mobile-first application with a lightweight backend API. The mobile app handles bill scanning (on-device OCR) and UI. The backend serves reference data (pricing, rates, irradiance) and performs system calculations.

```
┌─────────────────────────────────────────────────────┐
│                   .NET MAUI App                      │
│                  (Android-first)                     │
│                                                      │
│  ┌──────────┐  ┌──────────────┐  ┌───────────────┐  │
│  │  Camera   │  │  OCR Engine  │  │  Results UI   │  │
│  │  Module   │→ │ (On-Device)  │→ │  + Share      │  │
│  └──────────┘  └──────────────┘  └───────┬───────┘  │
│                                          │           │
│                              ┌───────────┴────────┐  │
│                              │ Local Cache         │  │
│                              │ (pricing, rates,    │  │
│                              │  irradiance)        │  │
│                              └───────────┬────────┘  │
│                                          │           │
└──────────────────────────────────────┬───────────────┘
                                       │ API calls
                                       ▼
                          ┌────────────────────────┐
                          │   ASP.NET Core Web API  │
                          │   (Railway — Hobby $5/mo)│
                          │                         │
                          │  ┌───────────────────┐  │
                          │  │ Calculation Engine │  │
                          │  │ - System sizing    │  │
                          │  │ - BOM generation   │  │
                          │  │ - Financial calcs   │  │
                          │  └───────────────────┘  │
                          │                         │
                          │  ┌───────────────────┐  │
                          │  │   PostgreSQL       │  │
                          │  │ - Equipment prices │  │
                          │  │ - DLPC rates       │  │
                          │  │ - Irradiance cache │  │
                          │  └───────────────────┘  │
                          └────────────────────────┘
                                       │
                                       ▼
                          ┌────────────────────────┐
                          │   NASA POWER API        │
                          │   (External, free)      │
                          └────────────────────────┘
```

### Why This Split?

- **OCR on-device:** No internet required for scanning. No per-call cloud OCR costs. Bill photos never leave the phone (privacy).
- **Calculation on backend:** Pricing, DLPC rates, and irradiance data change over time. Centralizing calculation means the app doesn't need an update to reflect new prices — just update the backend seed data and redeploy.
- **Offline fallback:** The app caches reference data (pricing tiers, DLPC rates, irradiance) locally after the first successful API call. When offline, the app can still compute estimates using cached data with a "prices as of [date]" notice.
- **Lightweight backend:** No user auth, no session management, no file storage. A calculation API with cached reference data.

## 2. Technology Stack

| Layer | Technology | Rationale |
|-------|-----------|-----------|
| Mobile app | .NET MAUI (.NET 8 LTS) | Cross-platform (Android + iOS from single codebase). Required stack. .NET 8 LTS chosen for long-term support (Nov 2026). |
| On-device OCR | ML Kit via `Plugin.Maui.OCR` | Free, on-device text recognition. Wraps ML Kit on Android, Vision Framework on iOS. Provides bounding box data for spatial parsing. If bounding box detail is insufficient, fallback to `Xamarin.Google.MLKit.TextRecognition` via platform-specific code. |
| Backend API | ASP.NET Core 8 Minimal API | Lightweight, fast, .NET ecosystem. Minimal API reduces boilerplate for a small API surface. |
| Database | PostgreSQL | Railway has native PostgreSQL support. Well-supported with EF Core via Npgsql. |
| ORM | Entity Framework Core + Npgsql | Standard .NET ORM. Code-first migrations. Railway provides `DATABASE_URL` in URI format — parse explicitly in `Program.cs`. |
| Deployment | Railway (Docker) — Hobby plan ($5/mo) | Required platform. Supports .NET via Dockerfile. Auto-deploy from GitHub. No permanent free tier — Hobby plan includes $5/mo usage credit. |
| Solar data | NASA POWER API | Free, covers Davao, no API key required. Cached in DB + app. |
| Pricing data | Manual seed data + redeployment | Seeded from LakaSolar data. Updated by modifying seed data and redeploying — no admin UI or admin API for MVP. |

## 3. Mobile App Architecture

### 3.1 Project Structure

```
Sinag/
├── Sinag.App/                    # .NET MAUI project
│   ├── Features/
│   │   ├── Scan/
│   │   │   ├── ScanPage.xaml      # Camera/gallery capture UI
│   │   │   ├── ScanViewModel.cs
│   │   │   ├── OcrService.cs      # Plugin.Maui.OCR text recognition wrapper
│   │   │   ├── BillParser.cs      # DLPC-specific field extraction from OCR output
│   │   │   └── ImagePreprocessor.cs # Resize, rotate, prep image for OCR
│   │   ├── Review/
│   │   │   ├── ReviewPage.xaml    # Show extracted data, allow corrections
│   │   │   └── ReviewViewModel.cs
│   │   ├── Results/
│   │   │   ├── ResultsPage.xaml   # BOM, costs, ROI display
│   │   │   ├── ResultsViewModel.cs
│   │   │   └── ShareService.cs    # Generate shareable summary card image
│   │   ├── NextSteps/
│   │   │   ├── NextStepsPage.xaml  # "What's Next" + installer directory
│   │   │   └── NextStepsViewModel.cs
│   │   ├── ManualEntry/
│   │   │   ├── ManualEntryPage.xaml # Fallback: type kWh manually
│   │   │   └── ManualEntryViewModel.cs
│   │   └── Home/
│   │       ├── HomePage.xaml      # Landing screen with "Scan Your Bill" CTA
│   │       └── HomeViewModel.cs
│   ├── Services/
│   │   ├── ApiClient.cs           # HTTP client for backend API
│   │   ├── CacheService.cs        # Local caching of pricing/rates/irradiance
│   │   └── ConnectivityService.cs # Online/offline detection
│   ├── Models/
│   │   ├── BillData.cs            # Extracted bill fields (app-side)
│   │   └── InstallerInfo.cs       # Static installer directory data
│   ├── Resources/
│   │   ├── Strings/               # Localization (Tagalog + English) — set up from Phase 0
│   │   └── Images/
│   ├── Platforms/
│   │   └── Android/               # Platform-specific code (ML Kit fallback if needed)
│   └── MauiProgram.cs             # DI registration, service config
│
├── Sinag.Api/                     # ASP.NET Core backend
│   └── (see Section 4)
│
├── Sinag.Shared/                  # Shared contracts between app and API
│   └── Contracts/
│       ├── EstimateRequest.cs
│       ├── EstimateResponse.cs
│       ├── BomTier.cs
│       ├── PricingResponse.cs
│       └── RatesResponse.cs
│
├── Sinag.Api.Tests/               # xUnit — API + calculation engine tests
├── Sinag.App.Tests/               # xUnit — BillParser + non-UI logic tests
│
└── Sinag.sln
```

### 3.2 MVVM Pattern + Navigation

The app uses MVVM with the MAUI Community Toolkit:
- **Views:** XAML pages (no code-behind logic)
- **ViewModels:** Handle UI state, commands, navigation
- **Services:** Injected via DI (OCR, API client, cache, share)
- **Navigation:** Shell navigation with route-based navigation for the linear flow:
  `Home → Scan → Review → Results → NextSteps`

**DI Service Lifetimes:**
- `OcrService` — singleton (stateless, reusable)
- `ApiClient` — singleton (wraps `HttpClient`, which should be long-lived)
- `CacheService` — singleton (manages local data store)
- `ConnectivityService` — singleton
- ViewModels — transient (new instance per page navigation)

### 3.3 On-Device OCR Pipeline

```
Photo (from camera or gallery)
  → ImagePreprocessor.cs:
    - Resize to ~2MP (large 12+ MP camera images slow OCR)
    - Auto-rotate based on EXIF orientation
  → OcrService.cs (Plugin.Maui.OCR):
    - ML Kit Text Recognition (on-device, no internet)
    - Returns recognized text with bounding box positions per TextLine
  → BillParser.cs (DLPC template matching):
    - Confirm DLPC bill: search for "DAVAO LIGHT" or "DLPC" anchor
    - Extract fields using spatial relationships (see BillParser Specification below)
    - Validate with regex (kWh: \d{1,4}, amount: [\d,.]+)
    - Assign confidence score per field
  → BillData object
  → ReviewPage (user confirms/corrects; low-confidence fields highlighted amber)
  → If OCR fails entirely → offer ManualEntryPage fallback
```

### 3.4 BillParser Specification

**ML Kit API level:** Work at the `TextLine` level (not `TextBlock`, which merges unpredictably). Each `TextLine` has a bounding box (x, y, width, height) and recognized text.

**Spatial relationship rules:**
- **Same-line value:** The value is in the same `TextLine` as the anchor label, to the right. Example: a line reading `"Total kWh Used    450"` — the anchor is `"kWh"` and the value `"450"` is in the same line.
- **Below-line value:** The value is in the `TextLine` directly below the anchor, with horizontal overlap >50%. Example: `"Billing Period"` on line N, `"Feb 26 - Mar 25, 2026"` on line N+1.
- **Horizontal band:** Two text elements are "in the same band" if their vertical midpoints are within 15px of each other (normalized to image height).

**Extraction zones (fields to extract):**

| Field | Anchor Text | Spatial Rule | Validation |
|-------|------------|-------------|------------|
| kWh consumed | `"kWh"` or `"KWH"` | Same-line, numeric to the right | `\d{1,4}`, range 1–5000 |
| Total amount | `"Total Amount Due"` or `"TOTAL"` | Same-line or below-line | `[\d,.]+`, range 100–500000 |
| Billing period | `"Billing Period"` or `"Service Period"` | Below-line | Date pattern `\w+ \d{1,2}.*\d{4}` |
| Rate class | `"Rate"` or `"Customer Class"` | Same-line | Contains "residential" or "R" |
| Generation charge | `"Generation"` | Same-line, numeric to the right | `[\d,.]+` |
| Account number | `"Account"` or `"Acct"` | Same-line | `\d{8,12}` |

**Bill validation:** If the "DAVAO LIGHT" or "DLPC" anchor is not found, reject with: "This doesn't appear to be a DLPC bill."

**Photo quality:** MVP supports flat/aligned photos only (bill on a table, photographed from above). Angled perspective correction is post-MVP. The camera instruction screen advises: "Place your bill on a flat surface and photograph from directly above."

### 3.5 Offline Behavior

| Feature | Online | Offline (with cached data) | Offline (no cache) |
|---------|--------|---------------------------|-------------------|
| Bill scanning (OCR) | Works | Works | Works |
| System calculation | API call | Uses cached pricing + irradiance, shows "prices as of [date]" | "Connect to internet once to download pricing data" |
| Results display | Works | Works | N/A |
| Save/share estimate | Works | Local save + share via device | Local save + share via device |

The app caches pricing tiers, DLPC rates, and irradiance data locally after the first successful API call. Cache is refreshed whenever the app is online and data is older than 7 days.

## 4. Backend API Architecture

### 4.1 Project Structure

```
Sinag.Api/
├── Endpoints/
│   ├── EstimateEndpoints.cs      # POST /api/v1/estimate
│   ├── PricingEndpoints.cs       # GET /api/v1/pricing
│   ├── RatesEndpoints.cs         # GET /api/v1/rates
│   └── HealthEndpoints.cs        # GET /api/health
├── Services/
│   ├── CalculationService.cs     # System sizing + BOM generation
│   ├── BomService.cs             # BOM generation rules (component specs, tier mapping)
│   ├── FinancialService.cs       # Savings, payback, 25-year projection
│   ├── IrradianceService.cs      # NASA POWER API client + cache
│   ├── PricingService.cs         # Equipment pricing by tier
│   └── RateService.cs            # DLPC rate schedule
├── Data/
│   ├── AppDbContext.cs
│   ├── Migrations/
│   └── SeedData.cs               # Initial pricing, rates, irradiance, component specs
├── Models/
│   ├── EquipmentPrice.cs
│   ├── ComponentSpec.cs
│   ├── DlpcRate.cs
│   └── IrradianceData.cs
├── Program.cs
├── Dockerfile
└── appsettings.json
```

### 4.2 API Endpoints

All endpoints use the `/api/v1/` prefix for versioning.

#### `POST /api/v1/estimate`

Primary endpoint. Receives extracted bill data, returns full solar estimate.

**Request:**
```json
{
  "kwhConsumed": 450,
  "billingPeriodDays": 30,
  "billingMonth": 3,
  "generationChargePerKwh": 6.52,
  "totalAmountDue": 6215.50,
  "includeBattery": true
}
```

Field notes:
- `billingMonth` (1–12): Used to look up month-specific irradiance data from cache
- `generationChargePerKwh`: Extracted from bill or looked up from DLPC rates. Used for accurate savings calculation — solar offsets generation charges only, not the full blended rate
- `totalAmountDue`: Used as a sanity check — `totalAmountDue / kwhConsumed` should approximate the blended DLPC rate. Flags a warning if the ratio is far from expected.
- `rateClass` is omitted — MVP supports residential only. Backend assumes residential.

**Response:**
```json
{
  "systemSizeKwp": 5.0,
  "dailyConsumptionKwh": 15.0,
  "peakSunHours": 4.8,
  "bom": {
    "budget": {
      "panels": { "spec": "450W x 12", "estimatedCost": { "min": 55000, "max": 70000 } },
      "inverter": { "spec": "5kW Hybrid", "estimatedCost": { "min": 22000, "max": 30000 } },
      "battery": { "spec": "5kWh LiFePO4", "estimatedCost": { "min": 35000, "max": 44000 } },
      "mounting": { "spec": "Roof-mount kit", "estimatedCost": { "min": 7000, "max": 10000 } },
      "wiring": { "spec": "MC4, breakers, cables", "estimatedCost": { "min": 4400, "max": 7000 } },
      "labor": { "spec": "Installation", "estimatedCost": { "min": 13000, "max": 22000 } },
      "totalEstimate": { "min": 136400, "max": 183000 }
    },
    "midRange": { "..." : "..." },
    "premium": { "..." : "..." }
  },
  "financial": {
    "currentMonthlyBill": 6215.50,
    "generationChargeOffsetKwh": 450,
    "monthlyGenerationSavings": 2934.00,
    "remainingFixedCharges": 3281.50,
    "estimatedMonthlyBillAfterSolar": 3281.50,
    "monthlySavings": 2934.00,
    "paybackPeriodMonths": 52,
    "twentyFiveYearSavings": 680000.00
  },
  "metadata": {
    "pricingAsOf": "2026-03-01",
    "irradianceMonth": 3,
    "peakSunHoursSource": "NASA POWER (Davao City, 20-year average)",
    "davaoDiscountApplied": true,
    "disclaimer": "Prices are estimates based on current Davao market rates. Actual costs may vary by installer."
  }
}
```

#### `GET /api/v1/pricing`

Returns current pricing tiers for all component categories. Used by the app to cache pricing data for offline calculation.

#### `GET /api/v1/rates`

Returns current DLPC residential rate schedule (all components: generation, transmission, distribution, etc.). Used by the app to display rate breakdown and for offline calculation.

#### `GET /api/health`

Standard health check for Railway deployment monitoring.

### 4.3 Calculation Engine Logic

#### System Sizing

```
Daily consumption = kWh per month / billing period days
Required system output = daily consumption / performance ratio (0.78)
System size (kWp) = required system output / peak sun hours (month-specific from irradiance cache)
```

#### Performance Ratio (PR = 0.78)
- Inverter efficiency: 96%
- Cable losses: 2%
- Soiling losses: 3%
- Temperature derating: 5% (Davao tropical average)
- System degradation: 0.5%/year (for financial projections, not initial sizing)

#### BOM Generation Rules

The system size drives component selection per tier:

**Panel selection:**
| Tier | Panel Wattage | Count Formula |
|------|--------------|---------------|
| Budget | 450W | `ceil(systemKwp * 1000 / 450)` |
| Mid-Range | 550W | `ceil(systemKwp * 1000 / 550)` |
| Premium | 600W | `ceil(systemKwp * 1000 / 600)` |

**Inverter sizing:** Round up system kWp to nearest standard size: `[3, 5, 8, 10, 15] kW`. Example: 4.8 kWp → 5kW inverter.

**Battery sizing (when `includeBattery = true`):**
```
Battery capacity = (daily consumption kWh) * (autonomy fraction)
autonomy fraction = 0.33 (covers ~8 hours of a 24-hour day, i.e., overnight backup)
Round up to nearest standard size: [5, 10, 15, 20] kWh
```
Worked example: 15 kWh/day × 0.33 = 5.0 kWh → 5 kWh battery.

**Mounting, wiring, labor:** Fixed per-system prices by tier, not per-watt. Stored in `equipment_prices` with `unit = 'fixed'`.

**Tier differentiation:** Tiers reflect component quality/brand level at different price points — not different system configurations. All tiers produce the same system size. Higher tiers have higher-efficiency panels (fewer panels needed), better inverters, and higher-quality mounting.

#### Financial Calculations
```
Monthly solar production = system size kWp × peak sun hours × 30 × PR
Monthly offset kWh = min(monthly solar production, monthly consumption)
Monthly savings (₱) = monthly offset kWh × generation charge per kWh
  (NOT the blended rate — solar only offsets the generation component)
Remaining bill = totalAmountDue - monthly savings
Payback period = total system cost / monthly savings (in months)
25-year savings = Σ(monthly savings × 12, adjusted for:
  - Panel degradation: -0.5%/year production decline
  - Rate increases: +3%/year estimated electricity rate increase)
  minus total system cost
```

### 4.4 Database Schema

```sql
-- Equipment pricing by category and tier
CREATE TABLE equipment_prices (
    id SERIAL PRIMARY KEY,
    category VARCHAR(50) NOT NULL,        -- panels, inverter, battery, mounting, wiring, labor
    tier VARCHAR(20) NOT NULL,            -- budget, mid_range, premium
    unit VARCHAR(20) NOT NULL,            -- per_watt, per_kwh, per_kw, fixed
    min_price_php DECIMAL(10,2) NOT NULL,
    max_price_php DECIMAL(10,2) NOT NULL,
    davao_discount_pct DECIMAL(4,2) DEFAULT 10.00,
    source VARCHAR(100),                  -- e.g., "LakaSolar 2026"
    effective_date DATE NOT NULL,
    created_at TIMESTAMP DEFAULT NOW(),
    UNIQUE (category, tier, effective_date)
);

-- Component specifications (drives BOM generation)
CREATE TABLE component_specs (
    id SERIAL PRIMARY KEY,
    category VARCHAR(50) NOT NULL,        -- panels, inverter, battery
    tier VARCHAR(20) NOT NULL,            -- budget, mid_range, premium
    wattage_or_capacity DECIMAL(10,2),    -- 450/550/600 for panels, 3/5/8/10 for inverters, 5/10/15/20 for batteries
    unit VARCHAR(10),                     -- W, kW, kWh
    spec_label_template VARCHAR(100),     -- "{wattage}W x {count}" or "{capacity}kWh LiFePO4"
    available_sizes JSONB,                -- [450, 550, 600] for panels; [3, 5, 8, 10, 15] for inverters
    effective_date DATE NOT NULL,
    UNIQUE (category, tier, effective_date)
);

-- DLPC rate components (residential only for MVP)
CREATE TABLE dlpc_rates (
    id SERIAL PRIMARY KEY,
    component VARCHAR(50) NOT NULL,       -- generation, transmission, distribution, system_loss, etc.
    rate_per_kwh DECIMAL(8,4) NOT NULL,
    effective_date DATE NOT NULL,
    source VARCHAR(100),
    created_at TIMESTAMP DEFAULT NOW()
);

-- Cached irradiance data for Davao (12 monthly averages)
CREATE TABLE irradiance_cache (
    id SERIAL PRIMARY KEY,
    latitude DECIMAL(8,5) NOT NULL,       -- Davao: 7.0707
    longitude DECIMAL(8,5) NOT NULL,      -- Davao: 125.6087
    month INT NOT NULL,                   -- 1-12
    ghi_kwh_m2_day DECIMAL(5,2) NOT NULL, -- Global Horizontal Irradiance
    peak_sun_hours DECIMAL(4,2) NOT NULL,
    source VARCHAR(50),                   -- "NASA POWER"
    fetched_at TIMESTAMP DEFAULT NOW(),
    UNIQUE (latitude, longitude, month)
);

-- Saved estimates (optional email-a-copy feature)
CREATE TABLE saved_estimates (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    contact_email VARCHAR(100),           -- encrypted at rest via EF Core value converter
    kwh_consumed INT NOT NULL,
    system_size_kwp DECIMAL(4,1) NOT NULL,
    estimate_json JSONB NOT NULL,         -- full response snapshot
    created_at TIMESTAMP DEFAULT NOW(),
    expires_at TIMESTAMP NOT NULL DEFAULT (NOW() + INTERVAL '90 days')
);
-- Cleanup job: DELETE FROM saved_estimates WHERE expires_at < NOW(); (run via scheduled task or on API startup)
```

### 4.5 NASA POWER API Integration

**Endpoint:** `https://power.larc.nasa.gov/api/temporal/monthly/point`

**Parameters:**
- `latitude`: 7.0707 (Davao City)
- `longitude`: 125.6087 (Davao City)
- `parameters`: ALLSKY_SFC_SW_DWN (surface shortwave downward irradiance)
- `community`: RE (Renewable Energy)
- `format`: JSON

**Caching strategy:**
- Fetch 12 monthly averages (20-year climatology)
- Cache in `irradiance_cache` table and in app locally
- Refresh annually (climate normals don't change meaningfully year to year)
- The `billingMonth` in the estimate request selects the correct month's irradiance

## 5. Security Considerations

- **Bill photos never leave the device.** OCR runs on-device. Only extracted numeric data (kWh, amounts, charges) is sent to the API.
- **No authentication for MVP.** The estimate API is public.
- **Rate limiting** via ASP.NET Core built-in middleware (`Microsoft.AspNetCore.RateLimiting`):
  ```csharp
  builder.Services.AddRateLimiter(options =>
  {
      options.AddFixedWindowLimiter("estimate", opt =>
      {
          opt.PermitLimit = 100;
          opt.Window = TimeSpan.FromHours(1);
      });
  });
  ```
  Note: IP-based rate limiting is imperfect on mobile networks (carrier-grade NAT shares IPs). Acceptable for MVP.
- **Contact email encryption** via EF Core value converter with AES-256 key stored in Railway environment variable (`ENCRYPTION_KEY`).
- **Data retention:** Saved estimates auto-expire after 90 days. Cleanup runs on API startup. Compliant with Philippine Data Privacy Act (RA 10173) — users can request deletion via email.
- **API input validation:**
  - `kwhConsumed`: 1–5,000 (residential ceiling)
  - `billingPeriodDays`: 25–35
  - `billingMonth`: 1–12
  - `generationChargePerKwh`: 0.01–50.00
  - `totalAmountDue`: 100–500,000
- **HTTPS only** for all API communication.
- **No PII in logs.** Request logging excludes contact_email field.

## 6. Deployment Architecture

```
GitHub Repository
       │
       ▼ (push to main)
Railway (auto-deploy) — Hobby plan $5/month
  ├── Sinag.Api (Docker container)
  │     ├── ASP.NET Core 8 app (multi-stage Dockerfile)
  │     └── Port: 8080 (Railway-assigned)
  ├── PostgreSQL (Railway add-on, billed by usage)
  │     └── Connection string via DATABASE_URL env var (URI format, parse in Program.cs)
  └── Custom domain (optional, post-launch)

Google Play Store
  └── Sinag.App (Android AAB)
        └── Built via GitHub Actions or local `dotnet publish`
```

### Railway Configuration
- **Dockerfile:** Multi-stage build (SDK image for build, ASP.NET runtime image for deploy)
- **Environment variables:**
  - `DATABASE_URL` — Railway PostgreSQL connection string (URI format)
  - `NASA_POWER_BASE_URL` — NASA POWER API base URL
  - `ENCRYPTION_KEY` — AES-256 key for contact email encryption
  - `ASPNETCORE_ENVIRONMENT` — Production
- **Health check:** `GET /api/health`
- **Cold starts:** Railway Hobby plan sleeps containers after inactivity. First request after sleep takes 8–15 seconds for .NET container startup. The app shows a loading state during this. For MVP, accept cold starts — no external keep-alive needed.
- **Scaling:** Single instance sufficient for MVP traffic

### CI/CD (GitHub Actions)
- **API:** Push to main → build + run tests → build Docker image → deploy to Railway
- **Mobile:** Manual build + release to Google Play (`dotnet publish` for Android)

## 7. Testing Strategy

| Layer | Approach | Focus |
|-------|----------|-------|
| BillParser | Unit tests with **fixture data** (pre-captured ML Kit output as JSON — text lines + bounding boxes) | Template matching, spatial extraction, regex validation, edge cases. Runs on CI without Android device. |
| OCR end-to-end | Manual testing on physical device with real DLPC bill images | Validates ML Kit + BillParser together. Runs during Phase 1 and Phase 4, not on CI. |
| Calculation engine | Unit tests | System sizing math, BOM generation rules, financial calculations, Davao discount application |
| API endpoints | Integration tests | Request/response contracts, input validation, error handling |
| Database | Migration tests | Schema creation, seed data integrity, unique constraints |
| Mobile UI | Manual testing on device | Camera flow, review screen, results display, offline mode |
| End-to-end | Manual testing | Full flow: photo → OCR → API → results → share |

**Test data:** User-provided sample DLPC bills + fixture data extracted from those bills. Synthetic test cases for edge cases: very low consumption (50 kWh), high consumption (2000 kWh), borderline system sizes.

## 8. Performance Targets

| Metric | Target | Notes |
|--------|--------|-------|
| OCR processing time | <3 seconds | On-device after image preprocessing (resize to ~2MP) |
| API response time (warm) | <2 seconds | Calculation + DB queries |
| API cold start | 8–15 seconds | Railway container spin-up; app shows loading state. Not counted in scan-to-result. |
| Total scan-to-result (warm API) | <15 seconds | OCR ~2-3s + user review ~5-7s + API ~1-2s |
| App size (APK) | <50 MB | ~15-20 MB MAUI baseline + ~5-10 MB ML Kit model + resources |
| Offline capability | Full with cached data | OCR always offline; calculation offline after first API call |

## 9. Key Technical Risks

| Risk | Probability | Impact | Mitigation |
|------|------------|--------|------------|
| `Plugin.Maui.OCR` bounding box data insufficient for spatial parsing | Medium | High | Run spike in Phase 1 to evaluate. Fallback: use `Xamarin.Google.MLKit.TextRecognition` directly via platform-specific Android code. |
| Camera overlay requires custom implementation (MAUI `MediaPicker` can't add overlays) | High | Medium | For MVP, use system camera via `MediaPicker` (no overlay) with a "how to photograph" instruction screen shown before capture. Custom camera view with overlay is a Phase 4 polish task. |
| Railway cold starts (8-15s) feel slow to users | Medium | Medium | App shows loading indicator; sends warm-up request on launch while user is scanning (scan takes ~10s, API warms up in parallel). |
| DLPC bill format variations (final bills, old formats, print quality) | Medium | Medium | MVP supports current regular monthly residential bills only. BillParser rejects unrecognized formats with clear message. |
| Google Play ML Kit policy compliance | Low | Medium | Declare ML Kit usage in store listing. All processing is on-device, no data sent to Google. |
| Railway hosting costs exceed budget | Low | Low | Hobby plan $5/mo is sufficient for MVP. Monitor usage. Scale to Pro ($20/mo) if needed post-launch. |
| NASA POWER API changes | Low | Low | Data is cached locally. Only need annual refresh. |
