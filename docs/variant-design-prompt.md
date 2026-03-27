# Sinag — Variant Design Prompt

## App Overview

**Sinag** (Filipino for "ray of light/sunbeam") is a mobile app for Davao City, Philippines that helps homeowners estimate solar panel installation costs by scanning their electric bill. Think of it as a friendly solar calculator that makes renewable energy feel accessible and exciting — not intimidating.

## Design Direction

**Mood:** Sunny, warm, optimistic, fun — like a bright Davao morning. The app should feel like sunshine in your pocket. Not corporate, not techy — approachable and encouraging. Think tropical solar energy vibes.

**Feeling:** A Filipino homeowner with zero solar knowledge should open this app and feel: "This is easy. This is for me. I can do this."

**Inspiration keywords:** Golden hour, tropical sun, warmth, savings, empowerment, Filipino pride, Davao

**Style:** Modern, clean, rounded corners, generous whitespace. Illustrations or icons preferred over stock photos. Subtle solar/sun motifs (rays, warm gradients, gentle glow effects). Cards with soft shadows.

## Brand

- **Name:** Sinag
- **Tagline:** "Solar Estimate para sa Davao"
- **Language:** Tagalog primary with English technical terms (kWh, ROI)
- **Primary color:** Warm amber/gold (#F59E0B or similar sunny yellow-orange)
- **Secondary color:** Deep tropical blue (#1E3A5F — like a clear Davao sky)
- **Accent:** Fresh green for savings/positive numbers
- **Background:** Warm off-white or very light cream — never stark white
- **Typography:** Rounded, friendly sans-serif. Bold headings, comfortable body text.

## Target User

Filipino homeowner in Davao City, 30-55 years old, owns an Android phone, monthly electric bill P3,000-P15,000+. May have zero knowledge about solar energy. Comfortable with Tagalog UI. Wants to save money on electricity.

## Screens to Design (6 total)

### Screen 1: Home Page
The app's front door. Should feel welcoming and immediately clear.

**Content:**
- App logo/wordmark "Sinag" with a sun motif
- Tagline: "Solar Estimate para sa Davao"
- Big, inviting primary CTA button: **"I-scan ang Iyong Bill"** (with a camera/scan icon)
- "Paano Ito Gumagana" (How It Works) section with 3 illustrated steps:
  1. **"I-scan"** — Icon: camera/phone scanning a bill — "Kunan ng litrato ang iyong electric bill"
  2. **"Suriin"** — Icon: checkmark/review — "Tingnan at i-verify ang mga nakuha"
  3. **"Resulta"** — Icon: solar panel/chart — "Makakuha ng detalyadong solar estimate"
- Secondary link at bottom: "O mag-type ng manual" (Or type manually)

**Design notes:** The CTA button should be the sun — glowing, warm, impossible to miss. The 3 steps should be horizontal cards or a visual timeline. Consider a subtle sun ray illustration in the background.

### Screen 2: Scan Page
Camera capture screen. Should feel simple and encouraging.

**Content:**
- Title: "I-scan ang Bill"
- Instruction card: "Ilagay ang iyong bill sa patag na surface at kunan ng litrato mula sa itaas" (Place your bill on a flat surface and photograph from above)
- Illustration: simple line drawing of a phone photographing a bill from above
- Two buttons:
  - Primary: **"Kunan ng Litrato"** (Take Photo) — camera icon
  - Secondary: **"Pumili mula sa Gallery"** (Choose from Gallery) — gallery icon
- Loading state: sunny animation or pulsing sun icon with "Binabasa ang iyong bill..." (Reading your bill...)

**Design notes:** Keep it minimal. The instruction illustration is key — show the ideal photo angle. The loading state should feel optimistic, not anxious.

### Screen 3: Review Page
Data verification screen. User confirms what the OCR extracted.

**Content:**
- Title: "Suriin ang Iyong Bill Data" (Review Your Bill Data)
- Extracted data fields in a clean card/form:
  - kWh Consumed: **450**
  - Billing Period: **30 araw** (days)
  - Billing Month: **Marso 2026**
  - Generation Charge: **₱6.52/kWh**
  - Total Amount Due: **₱6,215.50**
- Each field is editable (tap to change)
- Fields with low OCR confidence should have a subtle amber/yellow highlight with a small warning icon
- Toggle switch: **"Isama ang Battery?"** (Include Battery?)
- Two buttons:
  - Primary: **"Mukhang Tama"** (Looks Good) — checkmark icon
  - Secondary outline: **"I-scan Muli"** (Scan Again) — refresh icon

**Design notes:** Clean form design. The amber highlighting should be gentle — informative, not alarming. The battery toggle should feel like a fun add-on option.

### Screen 4: Manual Entry Page
Fallback form for users who can't scan. Same feel as Review but empty.

**Content:**
- Title: "I-type ang Iyong Bill Data"
- Empty form fields with placeholder text:
  - kWh Consumed (numeric)
  - Billing Period in Days (default: 30)
  - Billing Month (dropdown picker — Jan through Dec)
  - Generation Charge per kWh (default: ₱6.52)
  - Total Amount Due
- Toggle: "Isama ang Battery?"
- Primary CTA: **"Kalkulahin"** (Calculate) — sun/calculator icon

**Design notes:** Friendly, not form-like. Consider card-based field grouping. Should feel just as warm as the scan flow.

### Screen 5: Results Page ⭐ (Most important screen)
The payoff. User sees their personalized solar estimate. This screen must be clear, exciting, and shareable.

**Content — Top section:**
- Celebratory header: sun illustration or confetti-like rays
- System size badge: **"Inirerekomendang Sistema: 5.0 kWp"**
- Subtitle: "Batay sa iyong 450 kWh/buwan na konsumo at 4.8 peak sun hours sa Davao"

**Content — Tier selector:**
- 3 tier tabs/pills: **Budget** / **Mid-Range** / **Premium**
- Active tier highlighted in amber/gold

**Content — BOM (Bill of Materials) card for selected tier:**
| Component | Spec | Estimated Cost |
|-----------|------|---------------|
| Solar Panels | 550W x 10 | ₱55,000 - ₱70,000 |
| Inverter | 5kW Hybrid | ₱22,000 - ₱30,000 |
| Battery | 5kWh LiFePO4 | ₱35,000 - ₱44,000 |
| Mounting | Roof-mount kit | ₱7,000 - ₱10,000 |
| Wiring | MC4, breakers, cables | ₱4,400 - ₱7,000 |
| Labor | Installation | ₱13,000 - ₱22,000 |
| **Total** | | **₱136,400 - ₱183,000** |

**Content — Financial summary card (highlighted, celebratory):**
- **"Buwanang Savings: ₱2,934"** (big, green, with upward arrow)
- "Payback Period: 52 buwan"
- "25-Year Savings: ₱680,000"
- Small note: "Batay sa generation charge (₱6.52/kWh)"

**Content — Battery toggle:**
- "Isama ang Battery?" switch (recalculates when toggled)

**Content — Disclaimers (subtle, smaller text):**
- "Ang mga presyo ay tantiya batay sa kasalukuyang Davao market rates. Maaaring mag-iba ang aktwal na gastos."
- "Ang estimate na ito ay batay sa isang buwan na konsumo. Para sa pinaka-tumpak, i-scan ang bill sa pinakamataas na buwan (Marso-Mayo)."

**Content — Action buttons:**
- Primary: **"I-share"** (Share) — share icon
- Secondary: **"Susunod na Hakbang"** (Next Steps) — arrow icon

**Design notes:** This is THE screen. The savings number should feel like a reward — big, green, celebratory. The BOM table should be clean and scannable. Tier switching should feel smooth. The total cost range should be prominent but not scary — frame it as an investment. Consider a subtle "sunshine" gradient at the top.

### Screen 6: Next Steps Page
Bridges the user from "I have an estimate" to "I can take action."

**Content — Guidance section:**
- Title: "Ano ang Susunod?" (What's Next?)
- 3 numbered action steps as cards:
  1. "Pag-aralan ang iyong estimate kasama ang pamilya" (Review your estimate with family)
  2. "Makipag-ugnayan sa isang installer para sa site survey" (Contact an installer for a site survey)
  3. "Ikumpara ang mga quotation" (Compare quotations)

**Content — Installer directory:**
- Section title: "Mga Solar Installer sa Davao"
- 7 installer cards, each showing:
  - Company name (bold)
  - Badge if DOE Accredited (green badge for Sunstruck)
  - Phone number (tap to call icon)
  - Website (tap to open icon)
  - Brief note if applicable (e.g., "Est. 1989")

**Installers:**
1. **Sunstruck Solar Solutions** — [DOE Accredited badge] — sunstruck.ph
2. **Electro-Jake Solar** — 0910-555-5655 — electrojake.com
3. **MPPT Solar Energy Corp** — 082-298-4106 — mpptsolarenergy.com
4. **Solar Powerhaus** — 0917-659-7300 — solarpowerhaus.com
5. **Flaretech Solar Technology** — 0917-708-6347 — flaretechsolartechnology.com
6. **Prime Solar PH** — 0915-136-0841 — primesolarph.com
7. **WCTI Solar** — 082-221-2589 — Est. 1989

**Content — Disclaimer:**
- "Ang Sinag ay hindi kaakibat ng alinmang installer. Gawin ang iyong sariling pananaliksik."

**Content — Bottom button:**
- "Bumalik sa Home" (Back to Home)

**Design notes:** Installer cards should feel trustworthy — clean, professional but warm. The DOE badge should stand out as a trust signal. Phone and website actions should be obvious tap targets. The whole page should feel empowering: "You know what you need, here's who can help."

## General Design Notes

- **Android-first** — design for standard Android screen sizes (360-412dp width)
- **Bottom navigation is NOT used** — this is a linear flow (Home → Scan → Review → Results → NextSteps) with back navigation
- **No dark mode** for MVP — keep it sunny
- **Illustrations > photos** — simple, warm line illustrations with amber/gold accents
- **Numbers are key** — kWh, peso amounts, and savings should be immediately scannable with clear typography hierarchy
- **Filipino context** — avoid Western solar imagery (snow-covered roofs, etc.). Think: tropical rooftops, Philippine house styles, warm weather
- **Accessibility** — text should be comfortable for 30-55 year old users. No tiny fonts. Good contrast.
