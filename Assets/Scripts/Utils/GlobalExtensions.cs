using UnityEngine;

namespace Utils
{
    public static class GlobalExtensions
    {
        public static DualGlobalData GetGlobalData(this MonoBehaviour mb)
        {
            var globalParametersAssets = Resources.FindObjectsOfTypeAll(typeof(DualGlobalData));
        
            if (globalParametersAssets.Length > 0)
            {
                return globalParametersAssets[0] as DualGlobalData;
            }

            return null;
        }
    }
}