using UnityEngine;

namespace TarodevController {
    public class Bouncer : MonoBehaviour {
        [SerializeField] private float _bounceForce = 60f;

        // Using Trigger instead of regular collision.
        // Otherwise, we'd have to make a new Layer so it's not counted as Ground so that you can't stack it with jump power (unless that's desired)
        // Should visually make it clear that the Bouncers won't reset your jumps (e.g. trampoline, Sonic springs, etc)
        private void OnTriggerEnter2D(Collider2D collision) {
            if (collision.TryGetComponent(out IPlayerController controller)) {
                var incomingSpeedNormal = Vector3.Project(controller.Speed, transform.up); // vertical speed in direction of Bouncer
                controller.ApplyVelocity(-incomingSpeedNormal, PlayerForce.Burst); // cancel current vertical speed for more consistent heights
                controller.SetVelocity(transform.up * _bounceForce, PlayerForce.Decay);
            }
        }
    }
}