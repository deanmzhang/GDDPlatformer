using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostItem : MonoBehaviour {
    Camera mainCam;

    GameObject prefab;
    SpriteRenderer srend;
    Color originalColor;
    int overlapCount = 0;
    int itemId;

    const float GHOST_ALPHA = 0.5f;
    static Color badColor = new Color(0.8f, 0.2f, 0.2f, GHOST_ALPHA);

    Transform player;

    public void Init(GameObject original, int id) {
        itemId = id;
        prefab = original;
        mainCam = Camera.main;

        srend = GetComponent<SpriteRenderer>();
        SpriteRenderer otherRend = original.GetComponent<SpriteRenderer>();

        originalColor = new Color(otherRend.color.r, otherRend.color.g, otherRend.color.b, GHOST_ALPHA);
        srend.color = originalColor;
        srend.sprite = otherRend.sprite;

        gameObject.layer = LayerMask.NameToLayer("CraftCheck");

        CopyCollider(original.GetComponent<Collider2D>());

        //TODO: replace with real player
        //player = GameObject.Find("Player").transform;
    }

    void CopyCollider(Collider2D originalCol) {
        if (originalCol == null) {
            Debug.LogError("No collider on the original!");
            return;
        }

        if (originalCol is BoxCollider2D) {
            BoxCollider2D originalBox = originalCol as BoxCollider2D;
            BoxCollider2D newBox = gameObject.AddComponent<BoxCollider2D>();
            newBox.size = originalBox.size;
            newBox.offset = originalBox.offset;
            newBox.isTrigger = true;
        } else if (originalCol is CircleCollider2D) {
            CircleCollider2D originalCircle = originalCol as CircleCollider2D;
            CircleCollider2D newCircle = gameObject.AddComponent<CircleCollider2D>();
            newCircle.radius = originalCircle.radius;
            newCircle.offset = originalCircle.offset;
            newCircle.isTrigger = true;
        } else {
            Debug.LogError("Collider was not a circle or box");
        }

        Rigidbody2D rb = gameObject.AddComponent<Rigidbody2D>();
        rb.isKinematic = true;
    }

    private void OnTriggerEnter2D(Collider2D other) {
        overlapCount++;
        UpdateColor();
    }

    private void OnTriggerExit2D(Collider2D other) {
        overlapCount--;
        if (overlapCount < 0) {
            Debug.LogError("Somehow we went under");
            overlapCount = 0;
        }

        UpdateColor();
    }

    void UpdateColor() {
        srend.color = (overlapCount == 0) ? originalColor : badColor;
    }

    void LateUpdate() {
        Vector3 mouseWorldPos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        transform.position = new Vector3(mouseWorldPos.x, mouseWorldPos.y, 0);

        //???
        if (Input.GetMouseButtonDown(0)) {
            Craft();
        }

        if (Input.GetKeyDown(KeyCode.Escape)) {
            Cancel();
        }
    }

    void Craft() {
        if (overlapCount > 0) {
            return;
        }

        CraftingManager.instance.ApplyCraft(itemId);
        Instantiate(prefab, transform.position, transform.rotation);
        Destroy(gameObject);
    }

    void Cancel() {
        CraftingManager.instance.CancelCraft(itemId);
        Destroy(gameObject);
    }
}
