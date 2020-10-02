using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CraftButton : MonoBehaviour {
    public Transform tooltipParent;

    Button button;
    Image visual;
    int id;

    GameObject curRecipeDisplay;

    const float HOVER_TIME = 0.75f;
    float hoverTimer;

    bool isUnlocked;

    public void Init(int id, Sprite icon) {
        this.id = id;

        button = GetComponent<Button>();
        visual = GetComponent<Image>();
        visual.sprite = icon;
        visual.enabled = false;
    }

    public void OnClick() {
        CraftingManager.instance.TryCraft(id);
        StopHover();
    }

    public void OnHoverStart() {
        hoverTimer = HOVER_TIME;
    }

    public void OnHoverEnd() {
        StopHover();
    }

    public void UpdateState() {
        bool isCraftable = CraftingManager.instance.CanCraft(id);
        if (!isUnlocked && isCraftable) {
            UIManager.instance.CraftableUnlocked(id);
            isUnlocked = true;
            visual.enabled = true;
            transform.SetAsLastSibling(); //move to front of list
        }

        button.interactable = isCraftable;
    }

    public void DisableCrafting() {
        button.interactable = false;
    }

    void LateUpdate() {
        if (hoverTimer > 0f) {
            hoverTimer -= Time.deltaTime;
            if (hoverTimer <= 0) {
                OnHoverHeld();
            }
        }
    }

    void OnHoverHeld() {
        curRecipeDisplay = Instantiate(UIManager.instance.recipeTooltip, tooltipParent);
        //curRecipeDisplay.transform.localPosition = Vector3.zero;
        curRecipeDisplay.GetComponent<RecipeDisplay>().Init(id);
    }

    void StopHover() {
        hoverTimer = 0f;
        if (curRecipeDisplay != null) {
            Destroy(curRecipeDisplay);
        }
    }
}
