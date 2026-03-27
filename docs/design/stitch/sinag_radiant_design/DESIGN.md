# Design System Strategy: Radiant Editorial

## 1. Overview & Creative North Star
The Creative North Star for this design system is **"The Sun-Drenched Concierge."** 

Moving away from the sterile, modular appearance of typical utility apps, this system treats energy calculation as a premium, editorial experience. It draws inspiration from high-end hospitality and tropical modernism. We break the "app template" look by using intentional asymmetry—placing hero numbers off-center to create movement—and overlapping elements that mimic the way light and shadow interact in a physical space. 

This is "Sunshine in your pocket" not through literal clip-art, but through a layout that feels airy, layered, and perpetually golden. We prioritize "Breathing Room" over "Data Density," ensuring that the user feels a sense of optimistic calm rather than technical overwhelm.

---

## 2. Color & Tonal Depth
Our palette moves away from digital friction. We use a warm, cream-based foundation to eliminate the "blue light" harshness of standard apps.

### The "No-Line" Rule
**Strict Mandate:** Designers are prohibited from using 1px solid borders to define sections. Boundaries must be established through color-blocking or subtle shifts in background tones. For example, a calculator input area should be a `surface-container-low` (#fefae8) shape resting on a `surface` (#fffbff) background.

### Surface Hierarchy & Nesting
Treat the UI as a series of stacked, organic surfaces. Use the container tiers to create natural focus:
- **Base Layer:** `surface` (#fffbff)
- **Secondary Content Areas:** `surface-container-low` (#fefae8)
- **High-Focus Interactive Cards:** `surface-container-lowest` (#ffffff)
- **Deep Inset/Navigation:** `surface-container-high` (#f2eedb)

### The Glass & Gradient Rule
To achieve a "high-end" feel, main CTA buttons and top-level summary cards should utilize a **Signature Gradient**. Transition from `primary` (#8d5900) to `primary-container` (#f8a010) at a 135-degree angle. For floating navigation or modal overlays, apply **Glassmorphism**: use `surface` at 80% opacity with a `20px` backdrop-blur to allow the "tropical warmth" of the background to bleed through.

---

## 3. Typography
We pair **Plus Jakarta Sans** (for high-impact headings) with **Be Vietnam Pro** (for legible, friendly body copy). This combination provides a sophisticated yet approachable "Davao City" vibe.

- **Display (L/M/S):** *Plus Jakarta Sans Bold.* Used for the primary "Big Numbers" (e.g., your estimated savings). This is the "Hero" of the screen.
- **Headline (L/M/S):** *Plus Jakarta Sans SemiBold.* Used for section titles. These should have generous tracking (-1%) to feel tight and professional.
- **Title (L/M/S):** *Be Vietnam Pro Medium.* Used for card headers and primary navigation labels.
- **Body (L/M/S):** *Be Vietnam Pro Regular.* Used for all descriptive text. Line height must be set to 1.6x to maintain the "airy" feel.
- **Label (M/S):** *Be Vietnam Pro Bold / All Caps.* Used for small metadata or overlines.

---

## 4. Elevation & Depth
In this system, elevation is conveyed through **Tonal Layering** and **Ambient Light**, never through heavy drop shadows.

- **The Layering Principle:** Depth is created by stacking. Place a `surface-container-lowest` card on top of a `surface-container-low` section. The slight color shift creates "lift" without visual noise.
- **Ambient Shadows:** For elements that must "float" (like a primary action button or a floating FAB), use a custom shadow: 
  - `Box-shadow: 0px 12px 32px rgba(141, 89, 0, 0.08);` 
  - Notice the tint: we use a low-opacity `primary` color for the shadow rather than black to maintain the warm, sunny mood.
- **Ghost Borders:** If an edge *must* be defined for accessibility, use a "Ghost Border": `outline-variant` (#bcb9ab) at 15% opacity.

---

## 5. Components & Layout Patterns

### Cards & Surfaces
Forbid the use of divider lines. Instead, use the **Spacing Scale** (specifically `spacing.6` or `spacing.8`) to create "Gaps of Light" between content blocks. All cards must use `roundedness.lg` (2rem) or `roundedness.xl` (3rem) to maintain the soft, friendly persona.

### Buttons
- **Primary:** Gradient fill (`primary` to `primary-container`), `roundedness.full`, with a subtle `primary` ambient shadow.
- **Secondary:** `secondary-container` (#b9d3ff) background with `on-secondary-container` (#2d486e) text. No border.
- **Tertiary:** Text-only using `tertiary` (#00734e) with an icon, used for "Learn More" or less critical actions.

### Data Inputs (Solar Parameters)
Avoid traditional "Box" inputs. Use a "Slab" style: a thick `surface-container-high` base with `roundedness.md`, where the label sits comfortably above the value in `label-md`.

### The "Sunshine" Progress Loader
Instead of a circular spinner, use a custom animation of an expanding `primary` sun-ray motif that pulses softly.

### Contextual Savings Component
A signature "Savings Card" that uses `tertiary-container` (#69f6b8) with a soft radial gradient. This breaks the warm color palette to signify "Money/Growth" but keeps the rounded, soft aesthetic.

---

## 6. Do’s and Don’ts

### Do:
- **Do** use asymmetrical layouts. Let a headline sit on the left while the supporting image or chart overlaps it slightly on the right.
- **Do** use `roundedness.xl` for large image containers to mimic the "pebble" shapes found in nature.
- **Do** lean into the "Warm Cream" background. It is the signature of the brand.

### Don't:
- **Don't** use 100% Black (#000000). Use `on-surface` (#39382d) for high contrast.
- **Don't** use 1px dividers or "hr" tags. Space and color shifts are your only tools for separation.
- **Don't** use sharp corners. Even "small" components like checkboxes must have at least `roundedness.sm` (0.5rem).
- **Don't** cram information. If a screen feels busy, increase the spacing scale and move content to a secondary "Glass" modal.