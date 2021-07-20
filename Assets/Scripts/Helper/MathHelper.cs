using System.Runtime.CompilerServices;
using UnityEngine;

namespace NaughtyDoggy.Helper
{
    public class MathHelper
    {
        // vector converters
        public static Vector2 Vector3XZ(Vector3 v) 
            => new Vector2(v.x, v.z);

        public static Vector3 Vector3X0Z(Vector3 v) 
            => new Vector3(v.x, 0.0f, v.z);
    }
}