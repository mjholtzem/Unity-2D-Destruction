using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class ExplosionForce2D
{
    /// <summary>
    /// create an explosion force
    /// </summary>
    /// <param name="position">location of the explosion</param>
    public static void doExplosion(Vector3 position, float force = 2000, float radius = 4, float upliftModifier = 2)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(position, radius);

        foreach (Collider2D coll in colliders)
        {
            if (coll.GetComponent<Rigidbody2D>())
            {
                AddExplosionForce(coll.GetComponent<Rigidbody2D>(), force, position, radius, upliftModifier);
            }
        }
    }

    /// <summary>
    /// adds explosion force to given rigidbody
    /// </summary>
    /// <param name="body">rigidbody to add force to</param>
    /// <param name="explosionForce">base force of explosion</param>
    /// <param name="explosionPosition">location of the explosion source</param>
    /// <param name="explosionRadius">radius of explosion effect</param>
    /// <param name="upliftModifier">factor of additional upward force</param>
    private static void AddExplosionForce(Rigidbody2D body, float explosionForce, Vector3 explosionPosition, float explosionRadius, float upliftModifier = 0)
    {
        var dir = (body.transform.position - explosionPosition);
        float wearoff = 1 - (dir.magnitude / explosionRadius);
        Vector3 baseForce = dir.normalized * explosionForce * wearoff;
        baseForce.z = 0;
        body.AddForce(baseForce);

        if (upliftModifier != 0)
        {
            float upliftWearoff = 1 - upliftModifier / explosionRadius;
            Vector3 upliftForce = Vector2.up * explosionForce * upliftWearoff;
            upliftForce.z = 0;
            body.AddForce(upliftForce);
        }

    }
}

