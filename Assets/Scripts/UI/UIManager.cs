using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : Singleton<UIManager> {
    public GameObject resourceCounter;
    public Transform resourceList;

    public GameObject craftableButton;
    public Transform buttonList;

    public RecipeDisplay unlockSplash;

    public GameObject recipeTooltip;

    ResourceCounter[] inventoryCounts;
    CraftButton[] craftButtons;

    void Awake() {
        InitInventory();
        InitCraftables();

        unlockSplash.gameObject.SetActive(false);
    }

    void InitInventory() {
        CraftingManager.Resource[] resources = CraftingManager.instance.allResources;
        inventoryCounts = new ResourceCounter[resources.Length];

        for (int i = 0; i < resources.Length; i++) {
            CraftingManager.Resource resource = resources[i];

            GameObject newElement = Instantiate(resourceCounter, resourceList);
            inventoryCounts[i] = newElement.GetComponent<ResourceCounter>();
            inventoryCounts[i].Init(resource.icon);
        }
    }

    void InitCraftables() {
        CraftingManager.Craftable[] craftables = CraftingManager.instance.allCraftables;
        craftButtons = new CraftButton[craftables.Length];

        for (int i = 0; i < craftables.Length; i++) {
            GameObject newButton = Instantiate(craftableButton, buttonList);
            craftButtons[i] = newButton.GetComponent<CraftButton>();
            craftButtons[i].Init(i, craftables[i].icon);
        }
    }

    public void UpdateInventory(int[] inventory) {
        for (int i = 0; i < inventoryCounts.Length; i++) {
            inventoryCounts[i].UpdateCount(inventory[i]);
        }
    }

    public void RefreshCraftables() {
        foreach (CraftButton button in craftButtons) {
            button.UpdateState();
        }
    }

    public void DisableCrafting() {
        foreach (CraftButton button in craftButtons) {
            button.DisableCrafting();
        }
    }

    public void CraftableUnlocked(int itemId) {
        Time.timeScale = 0; //pause??
        unlockSplash.gameObject.SetActive(true);
        unlockSplash.Init(itemId);
    }

    public void CloseSplash() {
        Time.timeScale = 1;
        unlockSplash.gameObject.SetActive(false);
    }
}
