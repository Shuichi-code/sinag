# Sinag — Product Requirements Document (PRD)

**Version:** 1.1
**Date:** 2026-03-26
**Status:** Draft (revised after PM + Tech Lead review)

---

## 1. Problem Statement

Davao City homeowners interested in solar energy have no easy way to get an accurate, personalized estimate of what a solar installation would cost them. Existing Philippine solar calculators require manual input of bill amounts and produce generic estimates that don't account for DLPC-specific rate structures or actual consumption patterns.

A DLPC electric bill contains rich data — kWh consumption, rate class, itemized charges, historical usage — but no tool currently extracts this information automatically. Homeowners are left comparing vague price ranges for a P150K–P600K purchase with no way to understand what they specifically need.

## 2. Product Vision

Sinag lets Davao City homeowners photograph their DLPC electric bill and instantly receive a detailed, personalized solar installation estimate — including system size, component breakdown by budget tier, ROI timeline, and estimated monthly savings.

## 3. Target Users

**Primary:** Davao City residential electricity customers (DLPC account holders) who are:
- Exploring solar as an alternative to rising electricity costs
- In the research/consideration phase (not ready to talk to an installer yet)
- Comfortable using a smartphone camera

**User profile:**
- Filipino homeowner, likely 30–55 years old
- Monthly DLPC bill of P3,000–P15,000+
- Owns an Android smartphone (primary), possibly iOS
- May or may not have prior knowledge of solar energy

## 4. Core User Flow

```
1. Open app
2. Tap "Scan Your Bill"
3. Take photo of DLPC bill (or select from gallery)
4. App extracts data via on-device OCR:
   - Monthly kWh consumption
   - Billing period
   - Rate class
   - Total amount due
   - Itemized charges (generation, transmission, distribution, system loss)
5. User reviews extracted data (can correct if OCR misread)
6. App calculates:
   - Recommended system size (kWp)
   - Number and wattage of panels
   - Inverter size
   - Battery capacity (optional add-on)
   - Mounting and wiring requirements
7. App displays results:
   - Bill of Materials with 3 tiers (Budget / Mid-Range / Premium)
   - Estimated total cost per tier
   - Monthly savings estimate
   - ROI / payback period
   - Davao-specific solar production estimate (via NASA POWER data)
8. User can optionally:
   - Toggle battery on/off to see hybrid vs. grid-tied estimate
   - Share estimate as image (screenshot-ready summary card)
   - Save a copy by entering email (app sends one-time email with estimate)
9. App shows "What's Next" guidance:
   - Brief explanation of next steps (get site survey, contact installer)
   - Static list of Davao solar installers with contact info
```

**Edge cases handled in the flow:**
- Non-DLPC bill scanned → "This doesn't appear to be a DLPC bill. Sinag currently supports Davao Light bills only."
- Camera permission denied → explain why camera is needed, link to settings
- Very low consumption (<100 kWh/month) → show estimate but note: "At your consumption level, the payback period is long. Solar may not be cost-effective yet."
- OCR fails entirely → "Could not read your bill. Try again with better lighting, or enter your kWh manually."
- Manual entry fallback → user can skip OCR and type kWh + billing period directly

**Seasonal variation disclaimer:** The app estimates based on a single bill. A notice is shown: "This estimate is based on one month's consumption. Your actual usage may vary by season. For the most accurate sizing, scan your highest-consumption month's bill (typically March–May)."

## 5. MVP Features

### 5.1 Bill Scanning (On-Device OCR)
- Camera capture with guided overlay (frame alignment guide for bill)
- Gallery image selection as alternative
- On-device OCR processing (no internet required for scanning)
- Extraction of: kWh consumed, billing period, rate class, total amount, itemized charges
- DLPC bill format only
- Manual correction screen for extracted values

### 5.2 Solar System Calculator
- System sizing based on actual kWh consumption and Davao solar irradiance
- Accounts for system losses (inverter efficiency, cable loss, panel degradation, soiling)
- Supports grid-tied and hybrid (with battery) configurations
- Davao-specific solar hours and irradiance data from NASA POWER API

### 5.3 Bill of Materials (BOM) Output
- Component-category breakdown: panels, inverter, battery (optional), mounting, wiring/protection, labor
- Three pricing tiers: Budget, Mid-Range, Premium
- Pricing sourced from LakaSolar data, adjusted for Davao market (8–12% discount)
- Disclaimer: "Prices are estimates based on current Davao market rates. Actual costs may vary by installer."

### 5.4 Financial Analysis
- Monthly savings based on **generation charge offset only** — solar offsets the generation component of the DLPC rate, not fixed charges (distribution, metering, taxes). Using the blended rate would overstate savings.
- ROI / payback period calculation
- 25-year projection accounting for panel degradation (~0.5%/year) and electricity rate increases
- Comparison: total cost of solar vs. total electricity cost over payback period

### 5.5 Result Sharing & Next Steps
- Save estimate locally on device
- Optional: enter email to receive a one-time copy of the estimate (no account created)
- Share as image (screenshot-ready summary card)
- **"What's Next" screen** — brief guidance on next steps (site survey, installer consultation) with a static list of Davao solar installers and their contact info. This bridges the gap between "I have an estimate" and "I can take action."

## 6. Out of Scope for MVP

- User accounts / authentication
- Installer marketplace or lead generation
- Non-DLPC bill formats (Meralco, VECO, BOHECO, etc.)
- Financing calculator / loan comparisons
- Roof area analysis or satellite imagery
- Multi-scenario comparison (side-by-side configurations)
- Multi-bill scanning for seasonal averaging (future enhancement)
- Real-time equipment pricing or product catalog with specific brands
- iOS release (architecture supports it, but Android ships first)
- Backend analytics dashboard
- Push notifications
- Parameter adjustment beyond battery toggle (changing panel count, wattage, system size override — post-MVP)
- PDF export of estimate (image sharing only for MVP)

## 7. Data Requirements

### 7.1 DLPC Bill Template
- Bill layout mapping for OCR field extraction (field positions, labels, formats)
- Sample bills for OCR testing and validation (user has these)
- Bill format: system-generated from Oracle CCB (consistent layout expected)

### 7.2 Solar Irradiance
- Source: NASA POWER API
- Data needed: Global Horizontal Irradiance (GHI) and Peak Sun Hours for Davao City
- Can be cached locally (Davao irradiance doesn't change meaningfully day-to-day for estimation purposes)

### 7.3 Equipment Pricing
- Source: LakaSolar published price guides
- Categories: panels, inverters, batteries, mounting, wiring, labor
- Three tiers per category: Budget, Mid-Range, Premium
- Davao adjustment: apply 8–12% discount to national averages
- Update frequency: when LakaSolar publishes new data (roughly annual, verify quarterly)

### 7.4 DLPC Rate Schedule
- Current **residential** rate components (generation, transmission, distribution, system loss, taxes, subsidies)
- The **generation charge per kWh** is the key value — solar savings are calculated by offsetting this component only, not the blended rate
- Commercial rate class is out of scope for MVP
- Source: DLPC published rate schedules or extracted from sample bills

### 7.5 Davao Solar Installers
- A curated static list of DOE-accredited solar installers serving Davao City
- Contact info: company name, phone number, website/Facebook page
- Used in the "What's Next" screen — no marketplace, just a directory
- Update manually as needed

## 8. Success Metrics

| Metric | Target | Rationale |
|--------|--------|-----------|
| OCR extraction accuracy | ≥90% on kWh and total amount fields | Core value prop depends on reliable extraction |
| Scan-to-result time | <15 seconds | Must feel instant; users won't wait for a free tool |
| App store rating | ≥4.0 stars | Indicates user satisfaction |
| Estimate completion rate | ≥70% of scans result in a full estimate | Measures OCR + UX quality |
| Manual correction rate | Track % of scans where user edits extracted values | Real-world measure of OCR quality beyond lab accuracy |
| Monthly active users (3 months post-launch) | 500+ | Validates market interest in Davao (measured via anonymous API request counting) |

## 9. UX Principles

- **Tagalog-first, English-supported** — UI labels and explanations should default to Filipino/Tagalog with English technical terms where appropriate (e.g., "kilowatt" stays English)
- **Jargon-free results** — homeowners don't know what "5kWp grid-tied system with 10kWh LiFePO4 storage" means. Translate to: "10 solar panels on your roof that can cut your monthly bill by P4,000"
- **Trust through transparency** — always show how the estimate was calculated. "Based on your 450 kWh/month consumption and 4.8 peak sun hours in Davao..."
- **Camera-first UX** — scanning should feel like taking a normal photo. Guided overlay, no complex crop/rotate steps.
- **Results as shareable cards** — homeowners will show the estimate to their spouse or family. Make the summary visually clear and shareable.

## 10. Assumptions and Risks

### Assumptions
- DLPC bill format remains stable (Oracle CCB-generated)
- LakaSolar pricing data is reasonably accurate and regularly updated
- Davao homeowners have Android smartphones with functional cameras
- NASA POWER API remains free and available
- On-device OCR can achieve ≥90% accuracy on DLPC bills with guided capture

### Risks

| Risk | Impact | Mitigation |
|------|--------|------------|
| OCR accuracy below target on real-world photos | High — core feature fails | Manual correction screen; guided camera overlay; test extensively with real bills |
| DLPC changes bill format | Medium — OCR breaks | Template-based extraction allows updating field positions without retraining |
| LakaSolar pricing becomes stale or unavailable | Low — estimates become inaccurate | Fallback to other published guides (Pinas Solar, Solaric); quarterly price check |
| Low adoption in Davao | Medium — product fails | Validate with 20–30 real users before scaling; consider partnerships with local solar companies for distribution |
| NASA POWER API downtime | Low — calculation fails | Cache Davao irradiance data locally as fallback |
| DLPC bill format variations | Medium — OCR misreads | Support regular monthly residential bills only; gracefully reject final bills, reconnection notices, commercial bills with "unsupported bill type" message |
| Seasonal consumption variation | Low — misleading estimate | Disclaimer shown; recommend scanning highest-usage month |

## 11. Offline Behavior

OCR scanning works fully offline (on-device). System calculation requires pricing and irradiance data from the backend API, but the app caches this data locally after the first successful API call. Subsequent calculations can run offline using cached data, with a "prices as of [date]" notice. If the app has never connected to the API, it shows: "Connect to the internet once to download current pricing data."

## 12. Future Roadmap (Post-MVP)

1. **Installer marketplace** — connect users with Davao solar installers for real quotes (monetization via lead gen)
2. **Parameter adjustment** — let users override system size, change panel count/wattage, adjust battery autonomy hours
3. **Additional utilities** — expand OCR to Meralco, VECO, BOHECO bill formats
4. **iOS release** — .NET MAUI already supports it; needs App Store submission
5. **Financing calculator** — show monthly loan payments vs. current bill
6. **User accounts** — save history, track multiple properties
7. **Multi-bill seasonal averaging** — scan multiple months for a more accurate annual estimate
8. **B2B mode** — solar companies use Sinag as a quoting/lead qualification tool
