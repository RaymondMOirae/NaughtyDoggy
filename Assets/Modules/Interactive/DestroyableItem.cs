using System.Collections.Generic;
using UnityEngine;
using NaughtyDoggy.UI;

namespace NaughtyDoggy.Interactive
{
    public class DestroyableItem : MonoBehaviour
    {
        private bool edgeSet = false;
        private Vector3 edgeVertex = Vector3.zero;
        private Vector2 edgeUV = Vector2.zero;
        private Plane edgePlane = new Plane();

        public int CutCascades = 3;
        public float ExplodeForce = 250.0f;
        public bool FillCrossSection;
        public bool MakeFadingObj;
        public bool DestroyableRepeatedly;
        public float Score = 200;

        private void Response()
        {
            DestroyMesh();
            if (Score > 0)
            {
                GameManager.Instance.TriggerScoreStaging(Score);
            }
        }

        public void DestroyMesh()
        {
            Mesh originalMesh = GetComponent<MeshFilter>().mesh;
            originalMesh.RecalculateBounds();
            List<PartMesh> parts = new List<PartMesh>();
            List<PartMesh> subParts = new List<PartMesh>();

            PartMesh mainPart = new PartMesh()
            {
                UV = originalMesh.uv,
                Vertices = originalMesh.vertices,
                Normals = originalMesh.normals,
                Triangles = new int[originalMesh.subMeshCount][],
                Bounds = originalMesh.bounds
            };

            for (int i = 0; i < originalMesh.subMeshCount; i++)
                mainPart.Triangles[i] = originalMesh.GetTriangles(i);

            parts.Add(mainPart);

            for (int c = 0; c < CutCascades; c++)
            {
                for (int i = 0; i < parts.Count; i++)
                {
                    Bounds bounds = parts[i].Bounds;
                    bounds.Expand(0.5f);

                    Plane plane = new Plane(UnityEngine.Random.onUnitSphere, new Vector3(0, 0, 0));

                    subParts.Add(GenerateMesh(parts[i], plane, true));
                    subParts.Add(GenerateMesh(parts[i], plane, false));
                }

                parts = new List<PartMesh>(subParts);
                subParts.Clear();
            }

            for (int i = 0; i < parts.Count; i++)
            {
                if(MakeFadingObj)
                    parts[i].MakeFadingGameObject(this);
                else
                    parts[i].MakeGameObject(this);
                    
                
                parts[i].GameObject.GetComponent<Rigidbody>()
                    .AddForceAtPosition(parts[i].Bounds.center * ExplodeForce, transform.position);
            }

            Destroy(gameObject);
        }

        private PartMesh GenerateMesh(PartMesh original, Plane plane, bool positiveSide)
        {
            PartMesh partMesh = new PartMesh() { };
            Ray ray1 = new Ray();
            Ray ray2 = new Ray();


            for (int i = 0; i < original.Triangles.Length; i++)
            {
                int[] triangles = original.Triangles[i];
                edgeSet = false;

                for (int j = 0; j < triangles.Length; j = j + 3)
                {
                    bool sideA = plane.GetSide(original.Vertices[triangles[j]]) == positiveSide;
                    bool sideB = plane.GetSide(original.Vertices[triangles[j + 1]]) == positiveSide;
                    bool sideC = plane.GetSide(original.Vertices[triangles[j + 2]]) == positiveSide;

                    int sideCount = (sideA ? 1 : 0) + (sideB ? 1 : 0) + (sideC ? 1 : 0);
                    
                    
                    if (sideCount == 0)
                    {
                        continue;
                    }

                    if (sideCount == 3)
                    {
                        partMesh.AddTriangle(i,
                            original.Vertices[triangles[j]], original.Vertices[triangles[j + 1]],
                            original.Vertices[triangles[j + 2]],
                            original.Normals[triangles[j]], original.Normals[triangles[j + 1]],
                            original.Normals[triangles[j + 2]],
                            original.UV[triangles[j]], original.UV[triangles[j + 1]], original.UV[triangles[j + 2]]);
                        continue;
                    }

                    //cut points
                    int singleIndex = sideB == sideC ? 0 : sideA == sideC ? 1 : 2;

                    ray1.origin = original.Vertices[triangles[j + singleIndex]];
                    Vector3 dir1 = original.Vertices[triangles[j + ((singleIndex + 1) % 3)]] -
                               original.Vertices[triangles[j + singleIndex]];
                    ray1.direction = dir1;
                    plane.Raycast(ray1, out var enter1);
                    float lerp1 = enter1 / dir1.magnitude;

                    ray2.origin = original.Vertices[triangles[j + singleIndex]];
                    Vector3 dir2 = original.Vertices[triangles[j + ((singleIndex + 2) % 3)]] -
                               original.Vertices[triangles[j + singleIndex]];
                    ray2.direction = dir2;
                    plane.Raycast(ray2, out var enter2);
                    float lerp2 = enter2 / dir2.magnitude;

                    //first vertex = ancor
                    if(FillCrossSection)
                        AddEdge(i,
                            partMesh,
                            positiveSide ? plane.normal * -1f : plane.normal,
                            ray1.origin + ray1.direction.normalized * enter1,
                            ray2.origin + ray2.direction.normalized * enter2,
                            Vector2.Lerp(original.UV[triangles[j + singleIndex]],
                                original.UV[triangles[j + ((singleIndex + 1) % 3)]], lerp1),
                            Vector2.Lerp(original.UV[triangles[j + singleIndex]],
                                original.UV[triangles[j + ((singleIndex + 2) % 3)]], lerp2));

                    if (sideCount == 1)
                    {
                        partMesh.AddTriangle(i,
                            original.Vertices[triangles[j + singleIndex]],
                            //Vector3.Lerp(originalMesh.vertices[triangles[j + singleIndex]], originalMesh.vertices[triangles[j + ((singleIndex + 1) % 3)]], lerp1),
                            //Vector3.Lerp(originalMesh.vertices[triangles[j + singleIndex]], originalMesh.vertices[triangles[j + ((singleIndex + 2) % 3)]], lerp2),
                            ray1.origin + ray1.direction.normalized * enter1,
                            ray2.origin + ray2.direction.normalized * enter2,
                            original.Normals[triangles[j + singleIndex]],
                            Vector3.Lerp(original.Normals[triangles[j + singleIndex]],
                                original.Normals[triangles[j + ((singleIndex + 1) % 3)]], lerp1),
                            Vector3.Lerp(original.Normals[triangles[j + singleIndex]],
                                original.Normals[triangles[j + ((singleIndex + 2) % 3)]], lerp2),
                            original.UV[triangles[j + singleIndex]],
                            Vector2.Lerp(original.UV[triangles[j + singleIndex]],
                                original.UV[triangles[j + ((singleIndex + 1) % 3)]], lerp1),
                            Vector2.Lerp(original.UV[triangles[j + singleIndex]],
                                original.UV[triangles[j + ((singleIndex + 2) % 3)]], lerp2));

                        continue;
                    }

                    if (sideCount == 2)
                    {
                        partMesh.AddTriangle(i,
                            ray1.origin + ray1.direction.normalized * enter1,
                            original.Vertices[triangles[j + ((singleIndex + 1) % 3)]],
                            original.Vertices[triangles[j + ((singleIndex + 2) % 3)]],
                            Vector3.Lerp(original.Normals[triangles[j + singleIndex]],
                                original.Normals[triangles[j + ((singleIndex + 1) % 3)]], lerp1),
                            original.Normals[triangles[j + ((singleIndex + 1) % 3)]],
                            original.Normals[triangles[j + ((singleIndex + 2) % 3)]],
                            Vector2.Lerp(original.UV[triangles[j + singleIndex]],
                                original.UV[triangles[j + ((singleIndex + 1) % 3)]], lerp1),
                            original.UV[triangles[j + ((singleIndex + 1) % 3)]],
                            original.UV[triangles[j + ((singleIndex + 2) % 3)]]);
                        partMesh.AddTriangle(i,
                            ray1.origin + ray1.direction.normalized * enter1,
                            original.Vertices[triangles[j + ((singleIndex + 2) % 3)]],
                            ray2.origin + ray2.direction.normalized * enter2,
                            Vector3.Lerp(original.Normals[triangles[j + singleIndex]],
                                original.Normals[triangles[j + ((singleIndex + 1) % 3)]], lerp1),
                            original.Normals[triangles[j + ((singleIndex + 2) % 3)]],
                            Vector3.Lerp(original.Normals[triangles[j + singleIndex]],
                                original.Normals[triangles[j + ((singleIndex + 2) % 3)]], lerp2),
                            Vector2.Lerp(original.UV[triangles[j + singleIndex]],
                                original.UV[triangles[j + ((singleIndex + 1) % 3)]], lerp1),
                            original.UV[triangles[j + ((singleIndex + 2) % 3)]],
                            Vector2.Lerp(original.UV[triangles[j + singleIndex]],
                                original.UV[triangles[j + ((singleIndex + 2) % 3)]], lerp2));
                        continue;
                    }
                }
            }

            partMesh.FillArrays();

            return partMesh;
        }

        private void AddEdge(int subMesh, PartMesh partMesh, Vector3 normal, Vector3 vertex1, Vector3 vertex2,
            Vector2 uv1, Vector2 uv2)
        {
            if (!edgeSet)
            {
                edgeSet = true;
                edgeVertex = vertex1;
                edgeUV = uv1;
            }
            else
            {
                edgePlane.Set3Points(edgeVertex, vertex1, vertex2);

                partMesh.AddTriangle(subMesh,
                    edgeVertex,
                    edgePlane.GetSide(edgeVertex + normal) ? vertex1 : vertex2,
                    edgePlane.GetSide(edgeVertex + normal) ? vertex2 : vertex1,
                    normal, normal, normal, edgeUV, uv1, uv2);
            }
        }

        public class PartMesh
        {
            private List<Vector3> _Verticies = new List<Vector3>();
            private List<Vector3> _Normals = new List<Vector3>();
            private List<List<int>> _Triangles = new List<List<int>>();
            private List<Vector2> _UVs = new List<Vector2>();
            public Vector3[] Vertices;
            public Vector3[] Normals;
            public int[][] Triangles;
            public Vector2[] UV;
            public GameObject GameObject;
            public Bounds Bounds = new Bounds();

            public PartMesh() { }

            public void AddTriangle(int submesh, Vector3 vert1, Vector3 vert2, Vector3 vert3, Vector3 normal1,
                Vector3 normal2, Vector3 normal3, Vector2 uv1, Vector2 uv2, Vector2 uv3)
            {
                if (_Triangles.Count - 1 < submesh)
                    _Triangles.Add(new List<int>());

                _Triangles[submesh].Add(_Verticies.Count);
                _Verticies.Add(vert1);
                _Triangles[submesh].Add(_Verticies.Count);
                _Verticies.Add(vert2);
                _Triangles[submesh].Add(_Verticies.Count);
                _Verticies.Add(vert3);
                _Normals.Add(normal1);
                _Normals.Add(normal2);
                _Normals.Add(normal3);
                _UVs.Add(uv1);
                _UVs.Add(uv2);
                _UVs.Add(uv3);

                Bounds.min = Vector3.Min(Bounds.min, vert1);
                Bounds.min = Vector3.Min(Bounds.min, vert2);
                Bounds.min = Vector3.Min(Bounds.min, vert3);
                Bounds.max = Vector3.Min(Bounds.max, vert1);
                Bounds.max = Vector3.Min(Bounds.max, vert2);
                Bounds.max = Vector3.Min(Bounds.max, vert3);
            }

            public void FillArrays()
            {
                Vertices = _Verticies.ToArray();
                Normals = _Normals.ToArray();
                UV = _UVs.ToArray();
                Triangles = new int[_Triangles.Count][];
                for (int i = 0; i < _Triangles.Count; i++)
                    Triangles[i] = _Triangles[i].ToArray();
            }

            public void MakeGameObject(DestroyableItem original)
            {
                GameObject = new GameObject(original.name);
                GameObject.transform.position = original.transform.position;
                GameObject.transform.rotation = original.transform.rotation;
                GameObject.transform.localScale = original.transform.localScale;
                GameObject.tag = original.tag;
                GameObject.layer = original.gameObject.layer;

                Mesh mesh = new Mesh();
                mesh.name = original.GetComponent<MeshFilter>().mesh.name;

                mesh.vertices = Vertices;
                mesh.normals = Normals;
                mesh.uv = UV;

                for (int i = 0; i < Triangles.Length; i++)
                    mesh.SetTriangles(Triangles[i], i, true);
                Bounds = mesh.bounds;

                MeshRenderer renderer = GameObject.AddComponent<MeshRenderer>();
                renderer.materials = original.GetComponent<MeshRenderer>().materials;

                MeshFilter filter = GameObject.AddComponent<MeshFilter>();
                filter.mesh = mesh;

                MeshCollider collider = GameObject.AddComponent<MeshCollider>();
                collider.convex = true;

                if (original.DestroyableRepeatedly)
                {
                    DestroyableItem destroy = GameObject.AddComponent<DestroyableItem>();
                    destroy.CutCascades = original.CutCascades - 1;
                    destroy.FillCrossSection = original.FillCrossSection;
                    destroy.MakeFadingObj = true;
                    destroy.DestroyableRepeatedly = false;
                    destroy.Score = 0;
                }
                
                Rigidbody rigidbody = GameObject.AddComponent<Rigidbody>();

                
            }

            public void MakeFadingGameObject(DestroyableItem original)
            {
                MakeGameObject(original);
                Destroy(GameObject, UnityEngine.Random.Range(3f, 5f));
            }
        }
    }
}
