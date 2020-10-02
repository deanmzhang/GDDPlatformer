using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour {
    public HandController mainHand;

    public Transform attackTarget;
    Transform attackCenter;
    public float attackCooldown;
    public float damage;
    public float attackRange;
    public LayerMask attackableLayers;
    bool isAttacking;

    Player player;

    void Awake() {
        attackCenter = attackTarget.parent;
        player = GetComponent<Player>();
    }

    void Update() {
        if (Input.GetMouseButtonDown(1) && !isAttacking && !CraftingManager.instance.isCrafting) {
            StartCoroutine(AttackSequence());
        }

        if (!isAttacking) {
            Vector3 mousePos = player.mousePos;
            Vector3 dir = (mousePos - attackCenter.position).normalized;
            attackTarget.transform.localPosition = dir * attackRange;

            float offset = player.isFlipped ? -50 : -130; //sort of arbitrary
            attackTarget.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + offset);
        }
    }

    #region Attack
    IEnumerator AttackSequence() {
        isAttacking = true;

        Vector3 startOffset = mainHand.transform.position - attackCenter.position;

        mainHand.Reparent(!player.isFlipped, false, attackTarget);
        yield return new WaitForSeconds(attackCooldown / 4f);

        Vector3 endOffset = mainHand.transform.position - attackCenter.position;

        //add some buffer
        Vector3 dir = endOffset - startOffset;

        RaycastHit2D[] hits = Physics2D.CircleCastAll(attackCenter.position + startOffset, 0.3f, dir.normalized, dir.magnitude, attackableLayers);
        foreach (RaycastHit2D hit in hits) {
            DamageableObject health = hit.collider.GetComponent<DamageableObject>();
            if (health != this) {
                health.TakeDamage(damage);
            }
        }

        yield return new WaitForSeconds(attackCooldown / 4f);

        mainHand.Reparent(player.isFlipped, false);
        yield return new WaitForSeconds(attackCooldown / 2f);

        isAttacking = false;
    }
    #endregion

    public void OnDeath() {
        Destroy(mainHand);
    }
}
