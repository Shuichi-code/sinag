# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Sinag** is a mobile app for the Philippines that estimates solar panel installation costs and materials by photographing electric bills. Users take a photo of their bill, OCR extracts consumption data, and a calculation engine produces a tailored system recommendation with a detailed Bill of Materials and ROI timeline.

**Status:** Planning complete (PRD, TDD, Implementation Plan reviewed and aligned). Ready for Phase 0 implementation.

**Target market:** Davao City residents. DLPC (Davao Light) bills only for v1.
**Target users:** Regular homeowners exploring solar.

## Key Differentiators (Do Not Lose Sight Of)

- **Photo/OCR scanning** of electric bills — no Philippine competitor does this; all require manual bill amount entry
- **Davao Light (DLPC) rate-aware** — most competitors are Meralco-centric
- **Detailed BOM with local pricing** — not just system size, but full materials list
- **Mobile-first** — camera is a core input device, not an afterthought

## Tech Stack

- **Mobile:** .NET MAUI (Android-first, iOS support required)
- **Backend:** ASP.NET Core Web API
- **Deployment:** Railway (backend)
- **OCR:** On-device via `Plugin.Maui.OCR` (wraps ML Kit on Android). Fallback: `Xamarin.Google.MLKit.TextRecognition` via platform-specific code if bounding box data is insufficient.
- **Solar irradiance:** NASA POWER API (free, covers Davao)
- **Equipment pricing:** LakaSolar data with Davao 8-12% discount, three tiers (budget/mid/premium). Updated via seed data + redeployment.
- **Database:** PostgreSQL on Railway (Hobby plan $5/mo)
- **API versioning:** `/api/v1/` prefix on all endpoints

## Domain Context

### DLPC Bills (Davao Light)
DLPC still sends physical paper bills (confirmed). Digital options exist (eBillTxt SMS with password-protected PDF, MobileAP app with PDF download) but paper remains dominant. Bills are system-generated from Oracle CCB, meaning **consistent layout** — good for OCR template matching.

Key data points to extract via OCR:
- Total kWh consumed
- Billing period
- Total amount due
- **Generation charge per kWh** (critical — solar savings offset this component only, not the full blended rate)
- Account number

### Solar Market
- Philippine solar: 4.25 GW (2025) growing to 18.49 GW by 2031
- Mindanao has the **lowest** solar adoption (61 MW vs. 1,309 MW Luzon) despite strong sunlight
- Installed prices: P55-75/watt; Davao installations run 8-12% cheaper than Metro Manila
- Residential systems typically range 3-8 kWp (P150K-P600K)

## Key Technical Decisions

- **Financial savings use generation charge only** — solar offsets the generation component of the DLPC rate, not fixed charges (distribution, metering, taxes). Using blended rate would overstate savings and erode user trust.
- **Offline supported** — OCR is always offline. Calculation works offline using cached pricing/rates/irradiance (cached after first API call). Shows "prices as of [date]" notice.
- **No user accounts** — optional email-a-copy (one-time, auto-expires 90 days). No auth system.
- **Residential only** — commercial rate class is out of scope for MVP.
- **Camera: system camera for MVP** — no custom overlay. Instruction screen shown before capture. Custom overlay is Phase 4 polish.

## MVP Scope

- Scan DLPC bill → extract consumption data → system recommendation + BOM + ROI
- Manual entry fallback if OCR fails
- Battery toggle (grid-tied vs. hybrid)
- Share estimate as image, email-a-copy
- "What's Next" screen with static Davao installer directory
- DLPC residential bills only

## Monetization Model (Post-MVP)

Primary: installer lead generation and marketplace commission. Secondary: premium features and B2B tool for solar companies.

## Document Workflow

PRD → TDD → Implementation Plan. All three documents are then reviewed by PM agent and Tech Lead agent for cross-document alignment before any code is written.

## Project Structure

```
docs/
  prd.md                 # Product Requirements Document v1.1
  tdd.md                 # Technical Design Document v1.1
  implementation-plan.md # Implementation Plan v1.1
  market-research.md     # Competitive analysis, market data
  reviews/
    pm-review.md         # PM agent review (incorporated into v1.1)
    tech-lead-review.md  # Tech Lead agent review (incorporated into v1.1)
```

## Implementation Phases

```
Phase 0 (Setup) → Phase 1 (OCR) ──→ Phase 3 (E2E) → Phase 4 (Polish + Launch)
                → Phase 2 (API)  ──┘
```
Phases 1 and 2 can run in parallel. See `docs/implementation-plan.md` for details.
