using UnityEngine;

namespace TarodevController {
    public class RadialPlatform : PlatformBase {
        [SerializeField] private float _speed = 1.5f;
        [SerializeField] private float _radius = 2;

        private Vector2 _startPos;

        private void Awake() => _startPos = transform.position;

        protected override void FixedUpdate() => transform.position = _startPos + new Vector2(Mathf.Cos(Time.time * _speed), Mathf.Sin(Time.time * _speed)) * _radius;

#if UNITY_EDITOR
        private void OnDrawGizmosSelected() {
            var center = Application.isPlaying ? _startPos : (Vector2)transform.position;
            UnityEditor.Handles.DrawWireDisc(center, Vector3.back, _radius);
        }
#endif
    }
}