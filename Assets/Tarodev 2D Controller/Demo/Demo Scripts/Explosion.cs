using UnityEngine;

namespace TarodevController {
    public class Explosion : MonoBehaviour {
        [SerializeField] private float _growSpeed = 1;
        [SerializeField] private float _minRadius = 1;
        [SerializeField] private float _maxRadius = 3;
        [SerializeField] private float _explosionForce = 50;
        [HideInInspector] private float _amplitude;
        [HideInInspector] private float _offset;

        private void Update() {
            var scale = _amplitude * Mathf.Sin(Time.time * _growSpeed) + _offset; // (-1 to +1) gets mapped to (_min to _max)
            transform.localScale = scale * Vector3.one;
        }

        private void OnTriggerEnter2D(Collider2D other) {
            if (other.TryGetComponent(out IPlayerController controller)) {
                var dir = (Vector2)other.transform.position + other.offset - (Vector2)transform.position;
                var incomingSpeed = Vector3.Project(controller.Speed, dir); // player speed parralel to direction of explosion
                controller.ApplyVelocity(-incomingSpeed, PlayerForce.Burst); // cancel current incoming speed for more consistent bounces
                controller.SetVelocity(dir.normalized * _explosionForce, PlayerForce.Decay);
            }
        }

        private void OnValidate() {
            _amplitude = 0.5f * (_maxRadius - _minRadius);
            _offset = 1 + 0.5f * _minRadius;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected() {
            // If these gizmos don't match what's seen in game, adjust the circle Sprite's Pixels Per Unit in the Import Settings:
            // 100 is default, but 64 works for a 128x128. And make sure the Circle Collider's radius is 1
            UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.back, _minRadius);
            UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.back, _maxRadius);
        }
#endif
    }
}