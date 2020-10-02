using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RecipeDisplay : MonoBehaviour {
    public Transform countParent;
    public Image icon;

    public void Init(int itemId) {
        CraftingManager.Craftable data = CraftingManager.instance.allCraftables[itemId];

        if (icon != null) {
            icon.sprite = data.icon;
        }

        for (int i = 0; i < countParent.childCount; i++) {
            Destroy(countParent.GetChild(i).gameObject);
        }

        GameObject prefab = UIManager.instance.resourceCounter;

        foreach (CraftingManager.ResourceCount ingredient in data.recipe) {
            GameObject newObj = Instantiate(prefab, countParent);
            newObj.GetComponent<ResourceCounter>().Init(ingredient, true);
        }
    }
}
