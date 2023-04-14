using UnityEngine;

namespace TarodevController
{
    public class PatrolPlatform : PlatformBase
    {
        [Tooltip("Local offsets from starting position")]
        [SerializeField] private Vector2[] _points = new Vector2[] { Vector2.left, Vector2.right };
        [SerializeField] private float _speed = 1.5f;
        [SerializeField] private bool _looped;
        [SerializeField] private bool _ascending;

        private Vector2 _startPos;
        private Vector2 Target => _startPos + _points[_index];
        private int _index = 0;

        private void Awake() => _startPos = transform.position;

        protected override void FixedUpdate() {
            if ((Vector2)transform.position == Target) UpdateNextIndex();
            transform.position = Vector2.MoveTowards(transform.position, Target, _speed * Time.fixedDeltaTime);
        }

        private void UpdateNextIndex() {
            if (_looped)
                _index = (_ascending ? _index + 1 : _index + _points.Length - 1) % _points.Length;
            else { // ping-pong
                if (_index >= _points.Length - 1) _ascending = false;
                else if (_index <= 0) _ascending = true;
                _index = Mathf.Clamp(_index + (_ascending ? 1 : -1), 0, _points.Length - 1);
            }
        }

        private void OnDrawGizmosSelected() {
            var startPos = Application.isPlaying ? _startPos : (Vector2)transform.position;

            var previous = startPos + _points[0];
            Gizmos.DrawWireSphere(previous, 0.2f);
            if (_looped) Gizmos.DrawLine(previous, startPos + _points[^1]); // ^1 is last index, or _points.Length - 1

            for (var i = 1; i < _points.Length; i++) {
                var p = startPos + _points[i];
                Gizmos.DrawWireSphere(p, 0.2f);
                Gizmos.DrawLine(previous, p);
                previous = p;
            }
        }

        private void OnValidate() {
            if (_points.Length == 0)
                Debug.LogWarning("Set at least 1 position in the Points array to avoid index out-of-bounds exception", this);
        }
    }
}