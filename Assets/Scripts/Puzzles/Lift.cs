using UnityEngine;

namespace Puzzles
{
    public class Lift : StateSwitcher
    {
        public Vector3 endPosition;
        
        private Vector3 _ogPosition;
        private bool _movingToOg;

        private void Start()
        {
            _ogPosition = transform.position;
        }

        private void Update()
        {
            if (_movingToOg)
            {
                if (transform.position != endPosition)
                    transform.position = Vector3.Lerp(transform.position, endPosition, 0.01f);
            }
            else
            {
                if (transform.position != _ogPosition)
                    transform.position = Vector3.Lerp(transform.position, _ogPosition, 0.01f);
            }
        }

        public override void Switch()
        {
            _movingToOg = !_movingToOg;
        }
    }
}