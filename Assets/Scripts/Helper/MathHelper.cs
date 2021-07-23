using System.Runtime.CompilerServices;
using UnityEngine;
using Random = UnityEngine.Random;

namespace NaughtyDoggy.Helper
{
    public class MathHelper
    {
        // vector converters
        public static Vector2 Vec3XZ(Vector3 v) 
            => new Vector2(v.x, v.z);

        public static Vector3 Vec3X0Z(Vector3 v) 
            => new Vector3(v.x, 0.0f, v.z);

        public static Vector3 Vec3Random()
            => new Vector3(Random.value, Random.value, Random.value);
        
        public static Vector3 Vec3Mul(Vector3 v1, Vector3 v2)
            => new Vector3(v1.x * v2.x, v1.y * v2.y, v1.z * v2.z);
    }
}