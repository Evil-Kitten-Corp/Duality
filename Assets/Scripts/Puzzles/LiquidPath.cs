using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

namespace Puzzles
{
    [RequireComponent(typeof(Collider2D))]
    public class LiquidPath : MonoBehaviour
    {
        public DualChoice type;
        [FormerlySerializedAs("spriteRenderer")] public Tilemap tilemapRenderer;
        
        public DualGlobalData globals;
        public bool debug;

        private void Start()
        {
            if (globals == null)
            {
                Debug.LogError("Couldn't find globals in Resource folder!");
            }

            switch (type)
            {
                case DualChoice.BananaBoy:
                    
                    if (debug)
                    {
                        tilemapRenderer.color = Color.red;
                    }

                    gameObject.layer = LayerMask.NameToLayer(globals.fireElementalLayer);
                    break;
                case DualChoice.StrawberryBoy:
                    
                    if (debug)
                    {
                        tilemapRenderer.color = Color.blue;
                    }
                    
                    gameObject.layer = LayerMask.NameToLayer(globals.waterElementalLayer);
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