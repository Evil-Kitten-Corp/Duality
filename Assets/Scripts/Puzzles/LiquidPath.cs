using System;
using UnityEngine;
using Utils;

namespace Puzzles
{
    [RequireComponent(typeof(Collider2D))]
    public class LiquidPath : MonoBehaviour
    {
        public DualChoice type;
        public SpriteRenderer spriteRenderer;
        
        private DualGlobalData _globals;

        private void Start()
        {
            _globals = this.GetGlobalData();
        
            if (_globals == null)
            {
                Debug.LogError("Couldn't find globals in Resource folder!");
            }

            switch (type)
            {
                case DualChoice.FireElemental:
                    spriteRenderer.color = Color.red;
                    gameObject.layer = _globals.fireElementalAffinities;
                    break;
                case DualChoice.WaterElemental:
                    spriteRenderer.color = Color.blue;
                    gameObject.layer = _globals.waterElementalAffinities;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnCollisionEnter2D(Collision2D col)
        {
            if (col.gameObject.layer != gameObject.layer && col.gameObject.TryGetComponent<DualCharacter>(out var p))
            {
                p.Die();
            }
        }
    }
}