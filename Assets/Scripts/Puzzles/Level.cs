using System;
using UnityEngine;

namespace Puzzles
{
    [Serializable]
    public struct Level
    {
        public Transform waterSpawnPoint;
        public Transform fireSpawnPoint;
        public string nextLevelName;
    }
}