# Philippine Solar Calculator Apps: Pricing Data Research

**Date:** 2026-03-26

---

## 1. Solar Panda (solarpanda.ph)

**Approach:** Uses **price-per-watt tiers** rather than exact product prices.

- Has tier lists for batteries and hybrid inverters organized by price ranges:
  - Batteries: ₱5-6/Wh (budget) → ₱6-7/Wh (mid) → ₱12-13/Wh (premium)
  - 48V Hybrid Inverters: tiered similarly up to ₱7.5-8.5/Wh
- Catalogs **specific brands and models** (One Solar, PowMr, Zamdon, Flowatt, etc.) with individual spec pages
- Blog states solar panels dropped from ₱15/W (2017) to ₱7/W
- **Data source:** Not publicly disclosed. Appears to be manually curated from Philippine market prices (Shopee, Lazada, local suppliers). No API or automated data pipeline mentioned.
- Calculator input: monthly electricity bill → outputs recommended system with estimated cost
- Complete system cost examples: ₱25K-45K (small starter) to ₱70K-90K (24V 3.2kW)

**Key takeaway:** Solar Panda is the closest comparable to what Sinag could do. They use price-per-watt ranges by tier, supplemented with a product catalog of specific models.

---

## 2. SolarNRG (solarnrg.ph)

**Approach:** Simple **cost-per-kWp multiplier**, no specific equipment shown.

- Calculator input: monthly electric bill
- Output: recommended system size (kWp), estimated annual savings, payback period
- Uses standardized cost per kWp in the background (~₱40K-60K per kWp installed)
- **Does NOT show specific brands, models, or itemized equipment prices**
- Purpose is lead generation — the calculator drives users to request a quote
- Blog mentions ₱50K/kWp average, 10kW system ~₱800K

**Key takeaway:** SolarNRG uses a simple multiplier approach. No equipment-level pricing. This is the easiest approach to implement but provides the least value to users.

---

## 3. Solar Tayo (solartayo.com)

**Approach:** **Bill-based estimator** with savings projections.

- Default electricity rate based on Meralco's current residential rates (₱13.82/kWh as of March 2026)
- Estimates number of panels needed (e.g., 4-10 × 550W panels for ₱5K-10K monthly bill)
- Provides ROI estimates (5-8 years typical)
- **Does NOT show specific equipment prices or brands**
- Focuses on panel count and savings rather than detailed cost breakdown

**Key takeaway:** Similar to SolarNRG — uses general pricing assumptions, no equipment-level detail.

---

## 4. General Approach: How Solar Calculators Get Pricing Data

### Common Methods (from US/global market research):

| Method | Used By | Applicability to PH |
|--------|---------|-------------------|
| **Manual market research** | Most PH calculators | Most practical for PH |
| **Cost-per-kWp multiplier** | SolarNRG, Solar Tayo, most PH apps | Simple but imprecise |
| **Price-per-watt tiers** | Solar Panda | Good balance of detail vs. maintenance |
| **Live installer pricing** | EnergySage (US) | No PH equivalent exists |
| **Utility rate databases** | Google Solar API, Solar-Estimate.org | EIA (US only); no PH equivalent |
| **Commercial APIs** | OpenSolar, Aurora Solar | Expensive; US/AU focused |

### Key findings:
- **No open API for Philippine solar equipment pricing exists**
- US calculators use EIA data, Clean Power Research databases, and real installer pricing — none available for PH
- OpenSolar API and Aurora Solar API offer pricing features but are commercial products aimed at solar installers, not consumer apps
- Google Solar API is US-only

---

## 5. Public Solar Pricing Databases

### Philippines-Specific:
- **No public database exists** for Philippine solar equipment prices
- Department of Energy (DOE) Philippines does not publish equipment-level pricing
- Philippine pricing data comes from: Shopee/Lazada listings, installer quotes, manufacturer distributor lists

### Global/International:
- **IRENA Renewable Cost Database:** ~20,000 utility-scale projects with cost data. Tracks global weighted average costs (USD 758/kW in 2023, projected below USD 600/kW by 2026). Has Philippines-specific data for 20 tracked countries. Published a **Solar PV Supply Chain Cost Tool** (Feb 2026).
  - URL: https://www.irena.org/Data/View-data-by-topic/Costs/Solar-costs
- **Our World in Data:** Historical solar PV panel prices (global trends, not equipment-level)
  - URL: https://ourworldindata.org/grapher/solar-pv-prices
- **NREL (US):** National Solar Radiation Database — solar resource data, not pricing
- **Bloomberg NEF:** Comprehensive but expensive commercial subscription

### Summary: No freely accessible, equipment-level pricing database exists for the Philippines or globally.

---

## 6. Pricing Display: Exact Prices vs. Ranges

| App | Shows Specific Brands? | Shows Exact Prices? | Format |
|-----|----------------------|-------------------|--------|
| **Solar Panda** | Yes (product catalog) | No — uses ₱/Wh ranges | Price tiers (budget/mid/premium) |
| **SolarNRG** | No | No | Total system cost estimate |
| **Solar Tayo** | No | No | Savings estimate only |
| **Solaric** | Yes (on website) | Yes (on website, not calculator) | Quote-based |
| **GoSolar PH** | No | No | System cost estimate |

**Pattern:** No Philippine solar calculator shows exact, real-time equipment prices. They all use either:
1. **Price-per-watt/Wh tier ranges** (Solar Panda — most detailed)
2. **Cost-per-kWp system multiplier** (SolarNRG, Solar Tayo — simplest)

---

## 7. Solar Irradiance Data: NASA POWER & PVGIS

### NASA POWER
- **Confirmed: Covers Philippines/Davao** ✓
- Free, globally available climate data
- 300+ solar and meteorological parameters
- Resolution: 0.5° × 0.625° (meteorology), 1° × 1° (solar)
- Daily data from 1981 to near real-time
- RESTful API available: https://power.larc.nasa.gov/docs/services/api/
- Key parameters: GHI (Global Horizontal Irradiance), DNI, DHI, temperature

### PVGIS (EU Joint Research Centre)
- **Confirmed: Covers Philippines** ✓ (via PVGIS-SARAH2 for most of Asia, or PVGIS-ERA5 globally)
- Free tool for PV performance estimation
- Multiple radiation databases available
- PVGIS-ERA5 provides worldwide 0.25° × 0.25° coverage
- API available for automated access

### NREL Philippines Solar Data
- NREL hosts a **Philippines-specific NSRDB download** (National Solar Radiation Database)
- URL: https://developer.nrel.gov/docs/solar/nsrdb/philippines-download/
- High-resolution satellite-derived solar resource data

### Other Options
- **Solcast:** Commercial API with high-resolution PH data (paid)
- **Solargis:** Free maps available, commercial API for detailed data

---

## Recommendations for Sinag

### Pricing Strategy Options (ranked by feasibility):

1. **Cost-per-kWp multiplier** (easiest)
   - Use ₱40K-60K/kWp for on-grid, ₱60K-80K/kWp for hybrid/off-grid
   - Update quarterly based on market research
   - Lowest maintenance, least user value

2. **Price-per-watt tiers like Solar Panda** (recommended)
   - Define 3-4 price tiers for each component (panels, inverters, batteries)
   - Show ranges, not exact prices
   - Update monthly or quarterly
   - Good balance of usefulness vs. maintenance burden

3. **Product catalog with indicative prices** (most ambitious)
   - Build a database of specific products available in PH market
   - Scrape or manually track Shopee/Lazada/supplier prices
   - Highest maintenance but most valuable to users
   - Risk: prices go stale quickly

### Solar Irradiance: Use NASA POWER API
- Free, reliable, covers Davao
- Well-documented REST API
- No rate limiting concerns for reasonable usage
- Can supplement with PVGIS for cross-validation

---

## Sources

- [Solar Panda - Main](https://solarpanda.ph/)
- [Solar Panda - Battery Tier List](https://solarpanda.ph/tier/battery-brand-by-price)
- [Solar Panda - Inverter Tier List](https://solarpanda.ph/tier/hybrid-inverter-by-price)
- [Solar Panda - Cost of Solar Setup Blog](https://solarpanda.ph/blog/the-cost-of-a-solar-setup)
- [SolarNRG Calculator](https://solarnrg.ph/solar-calculator/)
- [SolarNRG ROI Blog](https://solarnrg.ph/blog/solar-roi-how-to-calculate-solar-panel-costs-and-savings/)
- [Solar Tayo](https://solartayo.com/)
- [NASA POWER](https://power.larc.nasa.gov/)
- [NASA POWER API Docs](https://power.larc.nasa.gov/docs/services/api/)
- [PVGIS](https://joint-research-centre.ec.europa.eu/photovoltaic-geographical-information-system-pvgis_en)
- [NREL Philippines Solar Data](https://developer.nrel.gov/docs/solar/nsrdb/philippines-download/)
- [IRENA Solar Costs](https://www.irena.org/Data/View-data-by-topic/Costs/Solar-costs)
- [IRENA PV Supply Chain Cost Tool (Feb 2026)](https://www.irena.org/Publications/2026/Feb/Solar-PV-Supply-Chain-Cost-Tool-Methodology-results-and-analysis)
- [OpenSolar API](https://www.opensolar.com/api/)
- [Google Solar API (US only)](https://developers.google.com/maps/documentation/solar/calculate-costs-us)
- [Our World in Data - Solar PV Prices](https://ourworldindata.org/grapher/solar-pv-prices)
