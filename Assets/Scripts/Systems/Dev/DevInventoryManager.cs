using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DevInventoryManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DatabaseLoader databaseLoader;
    [SerializeField] private InventoryUI inventoryUI;

    [Header("UI")]
    [SerializeField] private TMP_Dropdown unitDropdown;
    [SerializeField] private Button addUnitButton;
    [SerializeField] private TextMeshProUGUI statusText;

    private readonly List<UnitDefinition> availableUnits = new List<UnitDefinition>();

    private void Start()
    {
        if (databaseLoader == null)
        {
            databaseLoader = FindFirstObjectByType<DatabaseLoader>();
        }

        if (inventoryUI == null)
        {
            inventoryUI = FindFirstObjectByType<InventoryUI>();
        }

        if (addUnitButton != null)
        {
            addUnitButton.onClick.RemoveListener(AddSelectedUnitToInventory);
            addUnitButton.onClick.AddListener(AddSelectedUnitToInventory);
        }

        StartCoroutine(LoadUnitsWhenReady());
    }

    private IEnumerator LoadUnitsWhenReady()
    {
        if (databaseLoader == null)
        {
            SetStatus("DatabaseLoader not found.");
            yield break;
        }

        while (!databaseLoader.IsLoaded)
        {
            yield return null;
        }

        RefreshUnitList();
    }

    [ContextMenu("Refresh Unit List")]
    public void RefreshUnitList()
    {
        availableUnits.Clear();

        if (databaseLoader == null)
        {
            SetStatus("DatabaseLoader not found.");
            return;
        }

        if (databaseLoader.UnitLookup == null || databaseLoader.UnitLookup.Count == 0)
        {
            SetStatus("No units found in database.");
            PopulateDropdownWithPlaceholder("No units available");
            return;
        }

        foreach (UnitDefinition unitDef in databaseLoader.UnitLookup.Values)
        {
            if (unitDef == null)
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(unitDef.PrefabPath))
            {
                continue;
            }

            GameObject prefab = Resources.Load<GameObject>(unitDef.PrefabPath);
            if (prefab == null)
            {
                continue;
            }

            availableUnits.Add(unitDef);
        }

        availableUnits.Sort((a, b) =>
        {
            string aName = string.IsNullOrWhiteSpace(a.Name) ? a.UnitID : a.Name;
            string bName = string.IsNullOrWhiteSpace(b.Name) ? b.UnitID : b.Name;
            return string.Compare(aName, bName, System.StringComparison.OrdinalIgnoreCase);
        });

        List<UnitDefinition> dedupedUnits = availableUnits
            .GroupBy(u => u.UnitID)
            .Select(g => g.First())
            .ToList();

        availableUnits.Clear();
        availableUnits.AddRange(dedupedUnits);

        if (availableUnits.Count == 0)
        {
            PopulateDropdownWithPlaceholder("No prefab-backed units");
            SetStatus("No valid prefab-backed units found.");
            return;
        }

        if (unitDropdown != null)
        {
            unitDropdown.ClearOptions();
            List<string> options = new List<string>();
            for (int i = 0; i < availableUnits.Count; i++)
            {
                UnitDefinition unit = availableUnits[i];
                string displayName = string.IsNullOrWhiteSpace(unit.Name) ? unit.UnitID : unit.Name;
                options.Add(displayName + " [" + unit.UnitID + "]");
            }
            unitDropdown.AddOptions(options);
            unitDropdown.value = 0;
            unitDropdown.RefreshShownValue();
        }

        SetStatus($"Loaded {availableUnits.Count} units.");
    }

    public void AddSelectedUnitToInventory()
    {
        if (inventoryUI == null)
        {
            SetStatus("InventoryUI not found.");
            return;
        }

        if (availableUnits.Count == 0)
        {
            SetStatus("No selectable units.");
            return;
        }

        int selectedIndex = unitDropdown != null ? unitDropdown.value : 0;
        selectedIndex = Mathf.Clamp(selectedIndex, 0, availableUnits.Count - 1);

        UnitDefinition selectedUnit = availableUnits[selectedIndex];
        if (selectedUnit == null)
        {
            SetStatus("Selected unit is invalid.");
            return;
        }

        bool added = inventoryUI.AddUnit(selectedUnit);
        if (added)
        {
            string displayName = string.IsNullOrWhiteSpace(selectedUnit.Name) ? selectedUnit.UnitID : selectedUnit.Name;
            SetStatus($"Added {displayName} to inventory.");
        }
        else
        {
            SetStatus("Inventory full.");
        }
    }

    private void PopulateDropdownWithPlaceholder(string placeholder)
    {
        if (unitDropdown == null)
        {
            return;
        }

        unitDropdown.ClearOptions();
        unitDropdown.AddOptions(new List<string> { placeholder });
        unitDropdown.value = 0;
        unitDropdown.RefreshShownValue();
    }

    private void SetStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
    }
}
