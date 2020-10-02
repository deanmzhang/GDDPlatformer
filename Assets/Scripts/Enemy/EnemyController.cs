using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : DamageableObject {
    public List<CraftingManager.ResourceCount> drops;

    protected override void Die() {
        CraftingManager.instance.DropRecipe(drops, transform.position);
        base.Die();
    }
}
