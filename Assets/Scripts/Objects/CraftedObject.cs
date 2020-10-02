using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CraftedObject : DamageableObject {
    public int id;

    protected override void Die() {
        CraftingManager.instance.DropRecipe(id, transform.position);
        base.Die();
    }
}
