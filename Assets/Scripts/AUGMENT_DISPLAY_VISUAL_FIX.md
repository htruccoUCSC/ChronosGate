# Augment Buttons Not Showing - Visual Troubleshooting

The buttons ARE being created (we confirmed this), but they're not visible. Here's how to fix it.

---

## Step 1: Play and Check Console Output

After fixing the ReserveAD/ReserveAS error, play the game and select an augment. Look for console messages like:

```
[AugmentInventoryUI] Container size: (400, 200), Position: (0, 0)
[AugmentInventoryUI] Button 0 RectTransform - Size: (80, 80), Position: (0, 0)
[AugmentInventoryUI] Button 0 - Created for augment: 'Reserve AD'
[AugmentInventoryUI] ✓ RefreshAugmentDisplay() complete. Created 1 buttons.
```

**If you see these messages**, the buttons ARE being created. The issue is purely visual.

---

## Step 2: Check AugmentSlotsContainer Properties

Select **AugmentSlotsContainer** in Hierarchy and verify in Inspector:

### RectTransform
- [ ] **Width** > 0 (e.g., 400)
- [ ] **Height** > 0 (e.g., 200)  
- [ ] **Anchors** set properly (e.g., `(0, 1)` for top-left)
- [ ] **Position** is visible on screen (not off-screen)

### Image Component
- [ ] **Image** enabled (checked)
- [ ] **Color** is NOT transparent (alpha 255, not 0)
- [ ] **Raycast Target** can be on or off (doesn't affect display)

### GridLayoutGroup
- [ ] **Cell Size**: `(80, 80)` or larger
- [ ] **Spacing**: `(5, 5)` or similar
- [ ] **Start Axis**: `Horizontal`
- [ ] **Child Alignment**: `Upper Left`
- [ ] **Preferred Width**: Checked (or uncheck if causing issues)
- [ ] **Preferred Height**: Checked (or uncheck if causing issues)

---

## Step 3: Check AugmentSlotButton Prefab

Open the **AugmentSlotButton** prefab file directly.

### Button Component
- [ ] **Button** component exists
- [ ] **Navigation** set to `None` (to avoid nav issues)

### Image Component (on Button)
- [ ] **Image** component exists
- [ ] **Color** is NOT completely transparent
- [ ] Has a sprite or colored background

### TextMeshProUGUI Child
- [ ] **Text** component exists as a CHILD of the button
- [ ] **Text** field is NOT empty (has text)
- [ ] **Color** is visible (white or high contrast)
- [ ] **Font Size** is readable (e.g., 32)

**Child RectTransform should:**
- [ ] Stretch to fill parent (Anchors: `(0,0)` to `(1,1)`)
- [ ] Have `Left/Right/Top/Bottom` offsets = 0

---

## Step 4: Test with a Manual Button

Create a test button directly in the scene (not via prefab):

1. **Right-click AugmentSlotsContainer** → UI → Button - TextMeshPro
2. **Name it** `TestButton`
3. Change text to `"TEST"`
4. **Play the game**
5. Does it show up?
   - **YES** → Prefab is the issue. Copy properties from TestButton to your prefab.
   - **NO** → Container is the issue. Check container size and position.

---

## Step 5: Container Size Issues

If the container appears too small or empty:

1. **Select AugmentSlotsContainer**
2. **Right-click** → Debug
3. **Check if it has GridLayoutGroup** - if yes, verify settings above
4. **Try disabling GridLayoutGroup temporarily** to see if buttons appear (they'll be stacked)
5. If buttons appear when disabled, re-enable and adjust Cell Size smaller

---

## Common Causes & Fixes

| Problem | Cause | Fix |
|---------|-------|-----|
| Buttons created but invisible | Container size is (0, 0) | Set RectTransform size to (400, 200) |
| Buttons are created but stacked | GridLayoutGroup not found | Add GridLayoutGroup to container |
| Buttons outside visible area | Container positioned off-screen | Check Anchors and Position |
| Text not visible | TextMeshProUGUI text color is transparent | Set color to white or black |
| Prefab missing text component | Button created without child text | Add TextMeshProUGUI as child in prefab |
| GridLayout not arranging | Preferred Size checkboxes on | Uncheck Layout Element's Preferred Width/Height |

---

## Emergency Reset

If completely stuck:

1. **Delete AugmentSlotsContainer** from hierarchy
2. **Right-click AugmentInventoryPanel** → UI → Panel
3. **Name it**: `AugmentSlotsContainer`
4. **Assign to inspector** on AugmentInventoryUI script
5. **Apply defaults**:
   - RectTransform Size: `(400, 200)`
   - Position: `(0, -100)` (below the count text)
   - Image Color: White
6. **Add GridLayoutGroup**:
   - Cell Size: `(80, 80)`
   - Spacing: `(5, 5)`
   - Child Alignment: `Upper Left`
7. **Play and check console again**

---

## Console Output to Watch

When you select an augment, you should see **EXACT** debug info:

```
✓ Container size: (400, 200), Position: (200, -100)
✓ Button 0 RectTransform - Size: (80, 80), Position: (0, 0)
✓ Button 0 - Created for augment: 'Reserve AD'
✓ RefreshAugmentDisplay() complete. Created 1 buttons.
```

If you see **different** values, report them - they tell us exactly what's wrong!

---

## Report Back With

When asking for help, include:

1. **Console output** from RefreshAugmentDisplay()
2. **AugmentSlotsContainer RectTransform size** (from Inspector)
3. **Whether test button (Step 4) shows up or not**
4. **Screenshot of AugmentSlotsContainer in scene hierarchy with all properties expanded**

This will pinpoint the exact issue!
