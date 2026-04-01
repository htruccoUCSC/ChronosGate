using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class AugmentInventoryUI : MonoBehaviour
{
    [Header("Info Panel")]
    [SerializeField] private Button viewAllAugmentsButton;
    [SerializeField] private Button closeInfoPanelButton;
    [SerializeField] private Transform augmentInfoContainer;
    
    private AugmentManager augmentManager;
    
    private void Start()
    {
        augmentManager = FindFirstObjectByType<AugmentManager>();
        
        if (augmentManager == null)
        {
            Debug.LogError("[AugmentInventoryUI] AugmentManager not found in scene!");
            return;
        }

        if (viewAllAugmentsButton != null)
        {
            viewAllAugmentsButton.gameObject.SetActive(false);
        }

        // Hook up close info panel button
        if (closeInfoPanelButton != null)
        {
            closeInfoPanelButton.onClick.AddListener(HideAugmentInfo);
        }

        // Hide this panel after setting up listeners
        gameObject.SetActive(false);
    }
    
    /// <summary>
    /// Show detailed information for all owned augments with vertical columns of name and description
    /// </summary>
    private void ShowAllAugmentInfo()
    {
        if (augmentManager == null) return;

        gameObject.SetActive(true);

        // Clear existing displays
        if (augmentInfoContainer != null)
        {
            foreach (Transform child in augmentInfoContainer)
            {
                Destroy(child.gameObject);
            }
        }

        List<Augment> ownedAugments = augmentManager.GetAugmentInventory();
        if (ownedAugments.Count == 0) return;

        // Create main horizontal container for all augment columns
        GameObject mainContainer = new GameObject("MainContainer", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        mainContainer.transform.SetParent(augmentInfoContainer, false);
        RectTransform mainRT = mainContainer.GetComponent<RectTransform>();
        mainRT.anchorMin = Vector2.zero;
        mainRT.anchorMax = Vector2.one;
        mainRT.offsetMin = Vector2.zero;
        mainRT.offsetMax = Vector2.zero;

        HorizontalLayoutGroup mainHLG = mainContainer.GetComponent<HorizontalLayoutGroup>();
        mainHLG.spacing = 10;
        mainHLG.childForceExpandWidth = true;
        mainHLG.childForceExpandHeight = true;
        mainHLG.padding = new RectOffset(10, 10, 10, 10);

        // Create a column for each augment (vertical stack of name button + description)
        foreach (Augment augment in ownedAugments)
        {
            // Create vertical column container
            GameObject columnContainer = new GameObject($"Column_{augment.Name}", typeof(RectTransform), typeof(VerticalLayoutGroup));
            columnContainer.transform.SetParent(mainContainer.transform, false);

            VerticalLayoutGroup vlg = columnContainer.GetComponent<VerticalLayoutGroup>();
            vlg.spacing = 5;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = true;

            // Create name button
            GameObject nameButton = new GameObject($"NameButton_{augment.Name}", typeof(RectTransform), typeof(Button), typeof(Image), typeof(LayoutElement));
            nameButton.transform.SetParent(columnContainer.transform, false);

            Image btnImage = nameButton.GetComponent<Image>();
            btnImage.color = new Color(0.3f, 0.3f, 0.3f, 1f);

            LayoutElement nameBtnLayout = nameButton.GetComponent<LayoutElement>();
            nameBtnLayout.preferredHeight = 60;

            // Add text to button
            GameObject nameText = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            nameText.transform.SetParent(nameButton.transform, false);
            TextMeshProUGUI nameTextComp = nameText.GetComponent<TextMeshProUGUI>();
            nameTextComp.text = augment.Name;
            nameTextComp.fontSize = 24;
            nameTextComp.alignment = TextAlignmentOptions.Center;

            RectTransform nameTextRT = nameText.GetComponent<RectTransform>();
            nameTextRT.anchorMin = Vector2.zero;
            nameTextRT.anchorMax = Vector2.one;
            nameTextRT.offsetMin = Vector2.zero;
            nameTextRT.offsetMax = Vector2.zero;

            // Create description area
            GameObject descContainer = new GameObject($"Description_{augment.Name}", typeof(RectTransform), typeof(Image));
            descContainer.transform.SetParent(columnContainer.transform, false);

            Image descImage = descContainer.GetComponent<Image>();
            descImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);

            // Add description text
            GameObject descText = new GameObject("DescriptionText", typeof(RectTransform), typeof(TextMeshProUGUI));
            descText.transform.SetParent(descContainer.transform, false);
            TextMeshProUGUI descTextComp = descText.GetComponent<TextMeshProUGUI>();
            descTextComp.text = augment.Description;
            descTextComp.fontSize = 16;
            descTextComp.wordWrappingRatios = 0.3f;

            RectTransform descTextRT = descText.GetComponent<RectTransform>();
            descTextRT.anchorMin = Vector2.zero;
            descTextRT.anchorMax = Vector2.one;
            descTextRT.offsetMin = new Vector2(5, 5);
            descTextRT.offsetMax = new Vector2(-5, -5);
        }
    }

    /// <summary>
    /// Hide the augment information panel
    /// </summary>
    private void HideAugmentInfo()
    {
        gameObject.SetActive(false);
    }
}
