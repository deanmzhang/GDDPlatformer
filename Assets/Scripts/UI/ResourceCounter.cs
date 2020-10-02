using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResourceCounter : MonoBehaviour {
    public Color badColor;

    Text text;

    public void Init(Sprite icon) {
        GetComponentInChildren<Image>().sprite = icon;
        text = GetComponentInChildren<Text>();
        UpdateCount(0);
    }

    public void Init(CraftingManager.ResourceCount count, bool checkCount = false) {
        Sprite icon = CraftingManager.instance.GetResourceSprite(count.resourceId);
        GetComponentInChildren<Image>().sprite = icon;
        text = GetComponentInChildren<Text>();
        UpdateCount(count.count);

        if (checkCount && !CraftingManager.instance.HasResource(count)) {
            text.color = badColor;
        }
    }

    public void UpdateCount(int newCount) {
        text.text = newCount.ToString();
    }
}
