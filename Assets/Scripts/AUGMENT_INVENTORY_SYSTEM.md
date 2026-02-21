# Augment Inventory System - Architecture & Flow

## Overview

The Augment Inventory System manages the collection and display of augments that players acquire during gameplay. It provides:
- **Inventory Grid** - displays owned augments on the main screen
- **Augment Count** - shows total number of owned augments
- **Info Modal** - detailed view of all augments with names and descriptions

---

## System Architecture

### Core Components

#### 1. **AugmentManager.cs** (Data Layer)
Manages augment data and tracks owned augments.

**Key Properties:**
```csharp
public AugmentList augmentList;           // Active & Inactive augments
private List<Augment> augmentInventory;   // Owned augments (displayed in UI)
```

**Key Methods:**
- `AcquireAugment(Augment)` - Adds augment to inventory & triggers UI refresh
- `GetAugmentInventory()` - Returns list of owned augments
- `AddActiveAugment(Augment)` - Moves augment to active use
- `ApplyAllActiveAugments()` - Applies all active augment effects each round

**Data Flow:**
```
Selection UI calls SelectAugment()
    ↓
SelectAugment() calls AcquireAugment()
    ↓
AcquireAugment() adds to list + calls FindObjectOfType<AugmentInventoryUI>()?.RefreshAugmentDisplay()
    ↓
UI automatically updates with new augment
```

---

#### 2. **AugmentInventoryUI.cs** (UI Display Layer)
Displays owned augments as a grid of buttons on the main screen.

**Inspector References Required:**
| Field | Purpose |
|-------|---------|
| `augmentInventoryContainer` | Grid container (has GridLayoutGroup) |
| `augmentSlotPrefab` | Button prefab for each augment |
| `augmentCountText` | TextMeshProUGUI showing count |
| `augmentInfoPanel` | Modal panel (initially hidden) |
| `viewAllAugmentsButton` | Opens detailed info modal |
| `closeInfoPanelButton` | Closes info modal |
| `augmentInfoContainer` | Where augment details are displayed |

**Key Methods:**
- `RefreshAugmentDisplay()` - Rebuilds grid with current inventory
  1. Clears old buttons
  2. Gets augments from AugmentManager
  3. Creates button for each augment
  4. Updates count text
  
- `ShowAllAugmentInfo()` - Opens modal with full augment details
  1. Shows augmentInfoPanel
  2. Creates Name + Description for each augment
  3. Displays with proper spacing
  
- `HideAugmentInfo()` - Closes the modal

---

#### 3. **AugmentSelectionUI.cs** (Selection Layer)
Handles the augment selection screen when players pick a new augment.

**Key Call:**
```csharp
private void SelectAugment(int index)
{
    // ... validation and movement to activeAugments ...
    
    // THIS CRITICAL LINE triggers the UI update:
    augmentManager.AcquireAugment(selectedAugment);
}
```

---

## Data Flow Diagram

```
Game Start
    ↓
AugmentSetup initializes augmentList with augments
    ├─ inactiveAugments (available to select)
    └─ activeAugments (currently equipped)
    
Player gets augment selection screen every 3 rounds
    ↓
SelectAugment(index) called on button press
    ├─ Removes from inactiveAugments
    ├─ Adds to activeAugments
    ├─ Calls AcquireAugment() ← KEY TRIGGER
    └─ Hides selection panel
    
AcquireAugment() executes
    ├─ Adds to augmentInventory (display list)
    └─ Calls RefreshAugmentDisplay()
    
RefreshAugmentDisplay() executes
    ├─ Clears old buttons
    ├─ Gets augmentInventory list
    ├─ Creates button for each augment
    ├─ Updates count text
    └─ UI updates instantly

Player sees new augment in grid on main screen
    ↓
Player clicks "View All" button
    ↓
ShowAllAugmentInfo() displays full details in modal
```

---

## UI Hierarchy

```
Canvas
├─ CurrencyUI (Gold display)
├─ AugmentSelectionPanel (Selection screen - pauses game)
├─ ToggleAugmentButton (Show/hide selection)
├─ AugmentInventoryPanel ← Main inventory display
│  ├─ AugmentCountText ("Owned Augments: X")
│  ├─ AugmentSlotsContainer (GridLayoutGroup)
│  │  └─ [Dynamically created buttons]
│  └─ ViewAllAugmentsButton
└─ AugmentInfoPanel (Modal - initially inactive)
   ├─ CloseButton
   └─ InfoContainer
      └─ [Dynamically created augment details]
```

---

## Key Features

### 1. Dual Display Modes
- **Grid View (Main Screen)** - Shows only augment names in compact grid
- **Modal View (Detailed)** - Shows full name + description, triggered by "View All" button

### 2. Dynamic Button Creation
- Prefab-based instantiation for flexibility
- GridLayoutGroup auto-arranges buttons
- TextMeshProUGUI for text rendering

### 3. Real-time Updates
- `RefreshAugmentDisplay()` called immediately when augment acquired
- Count text updates synchronously
- No manual refresh required

### 4. Prefab Requirements
**AugmentSlotButton must have:**
```
AugmentSlotButton (Button)
├─ Image (background color/sprite)
└─ TextMeshProUGUI (augment name)
   └─ Set Layout as Preferred (anchored and stretched)
```

---

## Common Patterns

### Getting Augment Inventory
```csharp
List<Augment> inventory = augmentManager.GetAugmentInventory();
int count = inventory.Count;

foreach (Augment augment in inventory)
{
    Debug.Log(augment.Name);
}
```

### Manual Refresh (if needed)
```csharp
AugmentInventoryUI ui = FindObjectOfType<AugmentInventoryUI>();
ui?.RefreshAugmentDisplay();
```

### Acquiring Augment (called by SelectAugment)
```csharp
augmentManager.AcquireAugment(selectedAugment);
// Automatically triggers UI update
```

---

## Augment Data Structure

```csharp
[Serializable]
public class Augment
{
    public Action Apply;           // Function to execute when applied
    public string Name;             // "Reserve AD"
    public string Description;      // "All towers gain +1 Attack..."
    
    public Augment(Action apply, string name, string description)
    {
        Apply = apply;
        Name = name;
        Description = description;
    }
}
```

---

## List Management

### AugmentManager Lists:
- **`augmentList.activeAugments`** - Augments currently equipped (effects active)
- **`augmentList.inactiveAugments`** - Augments available for selection
- **`augmentInventory`** - Augments owned (shown in inventory display)

### Transitions:
```
inactiveAugments → SelectAugment() → activeAugments
                                 ↓
                            augmentInventory
                            (UI displays this)
```

---

## Error Handling

Script validates all Inspector references:
```csharp
if (augmentManager == null)        // Can't display without data
if (augmentInventoryContainer == null)  // Can't place buttons
if (augmentSlotPrefab == null)     // Can't create buttons
if (augmentCountText == null)      // Can't show count
```

Only `Debug.LogError()` calls remain in production code for critical issues.

---

## Performance Notes

- **Instantiation**: Buttons are created only when augments acquired (not every frame)
- **Destruction**: Old buttons destroyed and recreated on refresh (minimal overhead)
- **GridLayout**: Auto-arranges without manual position setting
- **Memory**: Prefab-based approach keeps memory footprint low

---

## Extension Points

### Add new augment:
1. Create `Augment` in `AugmentSetup.cs`
2. Add to `inactiveAugments` list
3. System automatically handles inventory display

### Style augment buttons:
1. Edit `AugmentSlotButton` prefab
2. Change Image color, sprite, text size, font
3. Changes apply to all augments automatically

### Modify info panel layout:
1. Edit values in `ShowAllAugmentInfo()`:
   - `infoRT.sizeDelta` - Container size
   - `nameRT.sizeDelta` - Title height
   - `descRT.anchoredPosition` - Title/description gap
   - `descRT.sizeDelta` - Description height

---

## Summary

The Augment Inventory System is a **data-driven UI system** that:
1. **Tracks** owned augments in `AugmentManager.augmentInventory`
2. **Displays** them dynamically in grid via `AugmentInventoryUI`
3. **Updates** automatically when acquired via `AcquireAugment()`
4. **Shows details** in a separate modal without cluttering main screen

The clean separation between **data (Manager)** and **display (UI)** makes it easy to extend and maintain.
