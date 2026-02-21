# UNITY SETUP CHANGES REQUIRED

## ✅ COMPLETED CODE FIXES
- [x] CurrencyManager.cs - privatized currency field, added GetCurrency() getter, added event system
- [x] TowerSlot.cs - integrated currency spending validation and deduction
- [x] ReserveAD.cs - fixed to use GetCurrency() instead of direct field access
- [x] ReserveAS.cs - fixed to use GetCurrency() instead of direct field access
- [x] AugmentManager.cs - has augmentInventory list and GetAugmentInventory() method
- [x] CurrencyUI.cs - CREATED - displays gold on screen
- [x] AugmentInventoryUI.cs - CREATED - displays owned augments

---

## 🎮 THINGS THAT NEED TO BE CHANGED IN UNITY EDITOR

### 1. **CRITICAL: Check Unit Costs in JSON or Database**
**Problem:** All unit costs show as 100 in the shop.

**What to check:**
- [ ] Open `Assets/StreamingAssets/units.json`
- [ ] Check the `Cost` field for each unit - should vary by unit (e.g., Archer: 10, Rockman: 30)
- [ ] If all costs are 100, edit the JSON to set appropriate costs per unit
- [ ] Save the file and reload Unity

**Example JSON structure to check:**
```json
{
  "AllUnits": [
    {
      "UnitID": "archer_01",
      "Name": "Archer",
      "Cost": 10,  // <- CHECK THIS VALUE
      ...
    }
  ]
}
```

### 2. **ADD: CurrencyManager to Main Scene**
**If not already present:**
- [ ] Create an empty GameObject in your main scene (e.g., "Managers")
- [ ] Add the `CurrencyManager` script to it
- [ ] Set initial currency value in inspector: `currency = 50` (or desired starting amount)
- [ ] Ensure this object persists across scenes (optional DontDestroyOnLoad if using multiple scenes)

**Recommended settings:**
```
Currency: 50
Threshold: 10
MaxInterest: 10
```

### 3. **ADD: Currency Display UI (TOP-RIGHT CORNER)**

**Create new UI element:**
1. [ ] In Canvas, create a new TextMeshProUGUI element in top-right corner
2. [ ] Name it "CurrencyDisplay"
3. [ ] Set text to "Gold: 50"
4. [ ] Style as desired (font size, color, outline, etc.)

**Attach CurrencyUI script:**
1. [ ] Create an empty GameObject as parent of the text element (e.g., "CurrencyPanel")
2. [ ] Add `CurrencyUI` script to the parent GameObject
3. [ ] In inspector, assign the TextMeshProUGUI element to "Currency Text" field
4. [ ] Optional: modify "Display Format" to customize the text (default: "Gold: {0}")

**Result:** Currency display updates in real-time when spent/earned

---

### 4. **ADD: Augment Inventory Panel (MAIN SCREEN)**

**Create UI Panel:**
1. [ ] In Canvas, create a new Panel element
2. [ ] Name it "AugmentInventoryPanel"
3. [ ] Position on left/right side of screen (visible at all times)
4. [ ] Set background color/image to match game style

**Create Panel Structure:**
```
AugmentInventoryPanel
├── TitleText (TextMeshProUGUI) - "Owned Augments"
├── CountText (TextMeshProUGUI) - "Augments: 0"
└── ScrollView
    └── Viewport
        └── Content (GridLayoutGroup)
            └── (Augment slots will spawn here)
```

**Configure GridLayoutGroup on Content:**
1. [ ] Cell Size: (200, 80) - adjust based on your button design
2. [ ] Spacing: (8, 8)
3. [ ] Child Force Expand: Width ON, Height ON
4. [ ] Child Control Size: Width ON, Height ON

**Create Augment Button Prefab:**
1. [ ] Create a Button prefab with these children:
   - Button (Image component for background)
   - TextMeshProUGUI (child - shows augment name)
2. [ ] Save to: `Assets/Prefabs/AugmentInventorySlot.prefab`

**Attach AugmentInventoryUI Script:**
1. [ ] Click "AugmentInventoryPanel"
2. [ ] Add `AugmentInventoryUI` script component
3. [ ] Assign in inspector:
   - **Augment Container**: Drag the "Content" object from GridLayoutGroup
   - **Augment Slot Prefab**: Drag the button prefab you created
   - **Augment Count Text**: Drag CountText from panel
   - **Selected Augment Name Text**: Optional - create TextMeshProUGUI element for details
   - **Selected Augment Description Text**: Optional - create TextMeshProUGUI element for details

---

### 5. **UPDATE: TowerSlot Button Affordability State**

**Problem:** TowerSlot buttons don't change state when player can't afford unit.

**What to do:**
1. [ ] Create a script or update TowerSlot with affordability checking
2. [ ] Listen to `CurrencyManager.OnCurrencyChanged` event
3. [ ] When currency changes, check if each unit can be afforded
4. [ ] Set `button.interactable = (currency >= unitCost);`

**Example implementation to add to TowerSlot:**
```csharp
private void Start()
{
    // ... existing code ...
    
    CurrencyManager currencyManager = CurrencyManager.Instance;
    if (currencyManager != null)
    {
        currencyManager.OnCurrencyChanged += OnCurrencyChanged;
        
        // Initial state
        if (unitDefinition != null)
        {
            button.interactable = currencyManager.GetCurrency() >= unitDefinition.Cost;
        }
    }
}

private void OnCurrencyChanged(int newCurrency)
{
    if (active && unitDefinition != null)
    {
        button.interactable = newCurrency >= unitDefinition.Cost;
    }
}

private void OnDestroy()
{
    CurrencyManager currencyManager = CurrencyManager.Instance;
    if (currencyManager != null)
    {
        currencyManager.OnCurrencyChanged -= OnCurrencyChanged;
    }
}
```

---

### 6. **VERIFY: Shop Manager Initialization**

Ensure ShopManager properly initializes TowerSlots:
1. [ ] Check that `PopulateTowerSlots()` is being called
2. [ ] Verify tower slot prefabs have proper UI references
3. [ ] Confirm DatabaseLoader exists in scene and has loaded unit data
4. [ ] Check console for errors about missing references

---

### 7. **VERIFY: GameLoopManager Setup**

Ensure all manager references are assigned:
1. [ ] GameLoopManager has `AugmentSelectionUI` assigned
2. [ ] GameLoopManager has `ShopManager` assigned
3. [ ] GameLoopManager has `WaveManager` assigned
4. [ ] All managers are in the scene and initialized in correct order

---

### 8. **VERIFY: DatabaseLoader Configuration**

1. [ ] DatabaseLoader exists in scene
2. [ ] Set `fileName` to "units.json"
3. [ ] Verify `Assets/StreamingAssets/units.json` exists
4. [ ] Check console on play - should log successful unit loading

---

### 9. **OPTIONAL: Add Cost Display Feedback**

**When unit is purchased, show feedback:**
1. [ ] Add FloatingText prefab that shows "-{cost}" in gold color
2. [ ] Spawn at player's position when purchase is made
3. [ ] Animate upward and fade out over 1 second

---

## 📋 UNITY CHECKLIST

**Scene Setup:**
- [ ] CurrencyManager GameObject exists in scene
- [ ] Currency Display UI element created and configured
- [ ] CurrencyUI script attached to its parent
- [ ] Augment Inventory Panel created with GridLayoutGroup
- [ ] AugmentInventoryUI script attached with all references assigned
- [ ] Augment Button Prefab created and saved

**Database/Assets:**
- [ ] units.json has correct cost values for each unit
- [ ] DatabaseLoader successfully loads all units on play
- [ ] All unit prefabs exist at specified paths

**Managers:**
- [ ] CurrencyManager initialized with correct starting currency
- [ ] GameLoopManager has all references assigned
- [ ] AugmentManager exists in scene
- [ ] ShopManager exists with TowerSlots configured

**Testing:**
- [ ] Currency displays correctly on startup
- [ ] Currency updates in real-time when spent
- [ ] Can purchase units and currency is deducted
- [ ] Cannot purchase unit with insufficient funds
- [ ] Button becomes disabled when can't afford unit
- [ ] Augment inventory displays owned augments
- [ ] No console errors on startup

---

## 🔧 DEBUGGING TIPS

If currency isn't deducting:
1. Check console for error messages
2. Verify CurrencyManager.Instance is not null
3. Ensure TowerSlot can find CurrencyManager.Instance
4. Check that `TrySpendCurrency()` returns true in console logs

If UI doesn't update:
1. Verify OnCurrencyChanged event is being invoked
2. Check that UI script is subscribed to the event
3. Ensure TextMeshProUGUI component exists and is assigned
4. Look for null reference errors in console

If augment inventory shows nothing:
1. Verify AugmentManager.GetAugmentInventory() is returning augments
2. Check that augment slot prefab is assigned in AugmentInventoryUI
3. Ensure augmentContainer (Content) is assigned correctly
4. Verify augments are being acquired (check console logs)

---

## 🚀 QUICK SETUP ORDER

1. Fix units.json costs (if needed)
2. Create CurrencyManager in scene
3. Create Currency Display UI
4. Create Augment Inventory Panel
5. Attach all UI scripts with inspector references
6. Playtest and verify
7. Fix any affordability check issues
8. Add optional feedback systems

