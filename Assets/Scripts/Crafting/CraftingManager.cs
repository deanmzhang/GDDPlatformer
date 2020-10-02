using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CraftingManager : Singleton<CraftingManager> {
    [System.Serializable]
    public struct Resource {
        public string name;
        public Sprite icon;
    }

    [System.Serializable]
    public struct ResourceCount {
        public int resourceId;
        public int count;
    }

    [System.Serializable]
    public struct Craftable {
        public string name;
        public GameObject prefab;
        public Sprite icon;
        public List<ResourceCount> recipe;
    }

    public Resource[] allResources;
    public Craftable[] allCraftables;

    public GameObject resourceDrop;

    public LayerMask craftBlockers; // things that can block line of sight between potential craftable

    int[] inventory;
    public bool isCrafting { get; private set; }

    void Start() {
        inventory = new int[allResources.Length];
    }

    public void TryCraft(int itemId) {
        if (isCrafting || !CanCraft(itemId)) {
            return;
        }

        Craftable item = allCraftables[itemId];
        foreach (ResourceCount ingredient in allCraftables[itemId].recipe) {
            inventory[ingredient.resourceId] -= ingredient.count;
        }

        UIManager.instance.UpdateInventory(inventory);
        UIManager.instance.DisableCrafting();

        GameObject ghostObj = new GameObject(item.name + " (Ghost)");
        ghostObj.AddComponent<SpriteRenderer>();
        ghostObj.AddComponent<GhostItem>().Init(item.prefab, itemId);

        isCrafting = true;
    }

    public void ApplyCraft(int itemId) {
        isCrafting = false;

        UIManager.instance.RefreshCraftables();
    }

    public void CancelCraft(int itemId) {
        isCrafting = false;

        foreach (ResourceCount ingredient in allCraftables[itemId].recipe) {
            inventory[ingredient.resourceId] += ingredient.count;
        }

        UIManager.instance.UpdateInventory(inventory);
        UIManager.instance.RefreshCraftables();
    }

    public bool CanCraft(int itemId) {
        Craftable item = allCraftables[itemId];

        foreach (ResourceCount ingredient in item.recipe) {
            if (!HasResource(ingredient)) {
                return false;
            }
        }

        return true;
    }

    public bool HasResource(ResourceCount count) {
        return inventory[count.resourceId] >= count.count;
    }

    public void PickupResource(int id) {
        inventory[id]++;
        UIManager.instance.UpdateInventory(inventory);
        UIManager.instance.RefreshCraftables();
    }

    public Sprite GetResourceSprite(int id) {
        return allResources[id].icon;
    }

    public void DropRecipe(int itemId, Vector3 position) {
        List<ResourceCount> recipe = allCraftables[itemId].recipe;
        DropRecipe(recipe, position);
    }

    public void DropRecipe(List<ResourceCount> recipe, Vector3 position) {
        foreach (ResourceCount ingredient in recipe) {
            for (int i = 0; i < ingredient.count; i++) {
                GameObject newDrop = Instantiate(resourceDrop, position, Quaternion.identity);
                newDrop.GetComponent<ResourceDrop>().Init(ingredient.resourceId);
            }
        }
    }
}
