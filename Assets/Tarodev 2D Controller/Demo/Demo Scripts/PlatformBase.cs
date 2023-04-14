using UnityEngine;

namespace TarodevController {
    [RequireComponent(typeof(Collider2D))]
    public abstract class PlatformBase : MonoBehaviour {
        // set the transform's position in FixedUpdate, not in Update
        protected abstract void FixedUpdate();

        protected virtual void OnCollisionEnter2D(Collision2D col)
        {
            if (IsOnTop(col.GetContact(0).normal))
                col.transform.SetParent(transform);
        }

        protected virtual void OnCollisionExit2D(Collision2D col)
        {
            col.transform.SetParent(null);
        }

        // true if the platform's "up" and the other collider's "normal" are in opposite directions
        protected virtual bool IsOnTop(Vector2 normal) => Vector2.Dot(transform.up, normal) < -0.5f;
    }
}