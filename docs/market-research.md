# Sinag - Market Research

**App Concept:** A mobile app where users photograph their electric bill, and via OCR + calculation engine, receive an estimated cost and materials list for a solar panel installation tailored to their consumption.

**Research Date:** 2026-03-26

---

## 1. Existing Philippine Solar Calculators

All existing PH tools require **manual input** (typing your monthly bill amount). None offer photo/OCR scanning.

| Tool | URL | What It Does | Photo/OCR? |
|------|-----|-------------|------------|
| Solar Panda | https://solarpanda.ph/ | Input monthly bill -> recommends inverter, battery, panel size, breakers, wire sizes, estimated cost. Available on Google Play. | No |
| SolarNRG Calculator | https://solarnrg.ph/solar-calculator/ | Bill-based calculator targeting 40-60% savings | No |
| Solar Tayo | https://solartayo.com/ | Estimates panels needed, costs, and savings | No |
| SolarLab.ph | https://solarlab.ph/solar-calculator/ | Web calculator | No |
| SolarCalculatorPhilippines.com | https://www.solarcalculatorphilippines.com/ | Web calculator | No |
| Sulit.ph Electric Solar | https://electricsolar.sulit.ph/ | Meralco bill calculator + solar savings estimator | No |
| NATIV Techniks | https://nativtechniks.com/solar-calculator/ | Web calculator | No |

### Solar Panda (Closest Competitor)

Solar Panda is the most feature-rich PH-specific tool. It recommends:
- Appropriate hybrid inverters
- Batteries
- Solar panel sizes
- Breakers and wire sizes
- Estimated costs

However, it still requires manual bill amount input and does not extract detailed consumption data from actual bills.

---

## 2. AI-Powered Global Solutions (Not PH-Focused)

| Tool | URL | Approach |
|------|-----|----------|
| Aurora Solar | https://aurorasolar.com/ | Address + bill amount -> 3D roof model + estimate (B2B, for solar companies) |
| GreenMetricAI | https://greenmetricai.com/ | AI analyzes bill amount -> savings estimate |
| Google Project Sunroof | https://sunroof.withgoogle.com/ | Satellite imagery of roof -> solar potential (US only) |
| Solar-Estimate.org | https://solar-estimate.org/ | AI panel layout tool (US-focused) |
| Solar Bill Review | https://play.google.com/store/apps/details?id=com.SolarBillReview.solarbillreview | Google Play app, appears US-focused |
| dida.do | https://dida.do/projects/automatic-planning-of-solar-systems | Automatic solar system planning via AI |

**Key takeaway:** AI-powered solar estimation exists globally but is entirely US/EU-focused. No one has brought this approach to the Philippines.

---

## 3. The Differentiation: Why Photo Scanning Matters

A Davao Light (DLPC) or Meralco bill contains much more than just the total amount:

- **kWh consumption** (not just peso amount) - enables accurate system sizing
- **Demand charges** and generation/transmission/distribution breakdowns
- **Historical usage** (some bills show 12-month consumption charts)
- **Meter number and account details** for follow-up quotes
- **Rate class** (residential vs. commercial vs. industrial)
- **Utility provider identification** - auto-detect rate structures

Extracting all of this via OCR produces a significantly more accurate system recommendation than "type in your monthly bill."

---

## 4. Philippine Solar Market Data

### Market Size & Growth
- Philippines solar market: **4.25 GW (2025) -> 18.49 GW by 2031** (Mordor Intelligence)
- Over 1,800 MW potential solar capacity nationwide (ICSC)
- DOE target: 50% renewable power by 2040

### Mindanao Specifically
- Current Mindanao solar capacity: **61 MW** (vs. 1,309 MW Luzon, 472 MW Visayas) - lowest adoption
- 500 MW allocated to Mindanao under Green Energy Auction Program
- 185.365 MW of committed solar projects in pipeline
- Transmission infrastructure delays (only 6 of 16 priority projects completed by 2023)

### Installation Costs (2026)
- Installed prices: **P55-75 per watt** (2026 rates)
- 3 kWp system: **P150,000 - P250,000**
- 5 kWp system: **P275,000 - P375,000**
- 8 kWp system: **P440,000 - P600,000**
- Davao installations run **8-12% cheaper** than Metro Manila (lower labor + simpler logistics)

### Electricity Rates
- Meralco residential rate: ~P13.82/kWh (March 2026)
- Davao Light (DLPC) rates may differ from Meralco

### Sources
- Mordor Intelligence: https://www.mordorintelligence.com/industry-reports/philippines-solar-energy-market
- LakaSolar: https://lakasolar.ph/news/magkano-solar-panel-philippines-2026
- Sunollo Guide: https://www.sunollo.com/blog/complete-solar-guide-philippines-2026
- Pinas Solar: https://pinas.solar/solar-guides/solar-panel-price-philippines/
- Solaric: https://solaric.com.ph/solar-panel-installation-cost/
- ICSC: https://icsc.ngo/philippines-has-over-1800mw-potential-solar-capacity-nationwide-icsc/

---

## 5. Demand Signals

### Evidence Supporting Demand
- **6+ competing PH solar calculators** already exist - people clearly want this information
- **Market growing 4x in 6 years** (4.25 GW to 18.49 GW)
- **Mindanao has lowest solar adoption** despite strong sunlight - room to grow
- **Davao installations are cheaper** than Manila - favorable economics
- Solar is a **P150K-600K purchase** - people want to research before committing
- DOE pushing renewable targets aggressively

### Davao-Specific Context
- Davao fuel/electricity prices have been surging (March 2026 saw major hikes)
- PUV drivers considering halting operations due to energy costs
- City government stepping up price monitoring
- Davao Light rates create pain point that solar directly addresses

### Uncertainties
- Whether photo-scan alone is enough to pull users from existing free calculators
- Whether users trust an app estimate enough to act, or still want in-person site survey
- Monetization path - solar calculators are typically free (monetized via installer lead gen)
- Accuracy of OCR on varied Philippine bill formats (Meralco, DLPC, VECO, etc.)

---

## 6. Potential Differentiators & Monetization

### Feature Angles Beyond Basic Estimation
1. **OCR bill scanning** - the primary UX differentiator; no PH competitor does this
2. **Detailed BOM (Bill of Materials)** - full materials list with local pricing, not just system size
3. **ROI timeline** - payback period based on actual bill breakdown, not averages
4. **Davao Light rate-aware** - most calculators use Meralco rates; DLPC accuracy serves underserved market
5. **Installer marketplace** - connect users with local Davao solar installers for quotes
6. **Financing calculator** - show monthly payment options vs. current electric bill

### Monetization Models
1. **Lead generation** - sell qualified leads to solar installation companies (most common model globally)
2. **Installer marketplace commission** - take a cut when users book installations
3. **Premium features** - detailed reports, financing comparisons, multi-scenario analysis
4. **B2B tool** - solar companies use it to qualify leads faster

---

## 7. Technical Considerations

### OCR Challenges
- Philippine electric bills vary by provider (Meralco, DLPC, VECO, BOHECO, etc.)
- Mixed Filipino/English text on bills
- Bill format changes over time
- Photo quality from phone cameras varies (lighting, angle, blur)
- Need training data: sample bills from each major utility

### Key Data Points to Extract
- Total kWh consumed
- Billing period
- Total amount due
- Rate class (residential/commercial)
- Utility provider
- Historical consumption (if shown)
- Demand charges (if applicable)

### Calculation Engine Needs
- Solar irradiance data for Davao/Mindanao region
- Current panel efficiency ratings
- Local equipment pricing database (panels, inverters, batteries, wiring)
- Installation labor cost estimates by region
- Utility rate schedules for ROI calculations
- System degradation curves (25-year projections)

---

## 8. Competitive Summary

| Aspect | Existing PH Calculators | Sinag (Proposed) |
|--------|------------------------|------------------|
| Input method | Manual (type bill amount) | Photo scan (OCR) |
| Data extracted | Bill amount only | kWh, rates, history, provider |
| Materials list | Basic (Solar Panda) or none | Detailed BOM with local pricing |
| Region focus | Metro Manila / generic | Davao-first, Mindanao-aware |
| Rate accuracy | Meralco-centric | DLPC and other Mindanao utilities |
| Monetization | Ads or lead gen | Installer marketplace + lead gen |
| Platform | Mostly web-based | Mobile-first (camera required) |
