using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Create Global Data", fileName = "DualGlobalData", order = 0)]
public class DualGlobalData : ScriptableObject
{
    public Sprite waterElementalSprite;
    public RuntimeAnimatorController waterElementalAnimator;
    public LayerMask waterElementalAffinities;
    
    public Sprite fireElementalSprite;
    public RuntimeAnimatorController fireElementalAnimator;
    public LayerMask fireElementalAffinities;

    public void GetDualSetUp(DualChoice dual, out Sprite s, out RuntimeAnimatorController anim, out LayerMask layer)
    {
        switch (dual)
        {
            case DualChoice.FireElemental:
                s = waterElementalSprite;
                anim = waterElementalAnimator;
                layer = waterElementalAffinities;
                break;
            case DualChoice.WaterElemental:
                s = fireElementalSprite;
                anim = fireElementalAnimator;
                layer = fireElementalAffinities;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(dual), dual, null);
        }
    }
}
