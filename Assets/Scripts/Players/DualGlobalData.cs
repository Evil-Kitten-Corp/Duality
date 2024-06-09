using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Create Global Data", fileName = "DualGlobalData", order = 0)]
public class DualGlobalData : ScriptableObject
{
    public Sprite waterElementalSprite;
    public RuntimeAnimatorController waterElementalAnimator;
    public string waterElementalLayer;
    
    public Sprite fireElementalSprite;
    public RuntimeAnimatorController fireElementalAnimator;
    public string fireElementalLayer;

    public void GetDualSetUp(DualChoice dual, out Sprite s, out RuntimeAnimatorController anim, out int layer)
    {
        switch (dual)
        {
            case DualChoice.StrawberryBoy:
                s = fireElementalSprite;
                anim = fireElementalAnimator;
                layer = LayerMask.NameToLayer(fireElementalLayer);
                break;
            case DualChoice.BananaBoy:
                s = waterElementalSprite;
                anim = waterElementalAnimator;
                layer = LayerMask.NameToLayer(waterElementalLayer);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(dual), dual, null);
        }
    }
}
