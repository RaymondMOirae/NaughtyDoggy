using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NaughtyDoggy.Interactive;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.UIElements;

public class Grass : MonoBehaviour
{
    private bool _isCut = false;
    private Plane _cutPlane;
    [Range(0.02f, 0.3f)] [SerializeField]
    private float CutHeight = 0.2f;
    
    private MeshFilter _originalMeshFilter;
    private Mesh _originalMesh;
    
    private List<Vector3> _oldVertices;
    private List<List<int>> _oldTriangles;
    private List<Vector3> _oldNormals;
    
    private List<Vector3> _newVertices = new List<Vector3>();
    private List<List<int>> _newTriangles = new List<List<int>>();
    private List<Vector3> _newNormals = new List<Vector3>();

    private Bounds _newBounds;
    
    private bool edgeSet;
    private Vector3 edgeVert;
    private Plane edgePlane;

    // Start is called before the first frame update
    void Start()
    {
        _originalMeshFilter = GetComponent<MeshFilter>();
        _originalMesh = _originalMeshFilter.mesh;
        _cutPlane = new Plane(Vector3.up, new Vector3(0.0f, CutHeight, 0.0f));
    }

    private void Response()
    {
        if (!_isCut)
        {
            CutMesh(_cutPlane, false);
            _isCut = true;
        }
    }

    private void CutMesh(Plane plane, bool downSide)
    {
        _originalMesh = GetComponent<MeshFilter>().mesh;
        
        _oldVertices = new List<Vector3>(_originalMesh.vertices);
        _oldTriangles = new List<List<int>>();
        _oldNormals = new List<Vector3>(_originalMesh.normals);
        _newBounds = _originalMesh.bounds;
        _newBounds.Expand(0.5f);
        
        for (int i = 0; i < _originalMesh.subMeshCount; i++)
        {
            _oldTriangles.Add(new List<int>(_originalMesh.GetTriangles(i)));
        }

        Ray ray1 = new Ray();
        Ray ray2 = new Ray();

        for (int i = 0; i < _oldTriangles.Count; i++)
        {
            List<int> submeshTris = _oldTriangles[i];
            edgeSet = false;

            for (int j = 0; j < submeshTris.Count; j = j + 3)
            {
                // every 3 ints represents a triangle's vertices' indexes.
                bool sideA = (plane.GetSide(_oldVertices[submeshTris[j]]) == downSide);
                bool sideB = (plane.GetSide(_oldVertices[submeshTris[j + 1]]) == downSide);
                bool sideC = (plane.GetSide(_oldVertices[submeshTris[j + 2]]) == downSide);

                int sideCount = (sideA ? 1 : 0) + (sideB ? 1 : 0) + (sideC ? 1 : 0);
                
                
                // 4 possible type of triangle-plane intersection conditions ( preserve the mesh below the plane )
                //  - type 1: sideCount = 0, no point below the plane       , skip the triangle
                //  - type 2: sideCount = 3, whole triangle below the plane , add the whole triangle
                //  - type 3: sideCount = 1, one point below the plane -∵- , 
                //  - type 4: sideCount = 2, two point below teh plane -∴- ,
                
                if (sideCount == 0) // type1
                {
                    continue;
                }else if (sideCount == 3) // type2
                {
                    AddTriangle(i, _oldVertices[submeshTris[j]], _oldVertices[submeshTris[j + 1]],
                                _oldVertices[submeshTris[j + 2]], _oldNormals[submeshTris[j]],
                                _oldNormals[submeshTris[j + 1]], _oldNormals[submeshTris[j + 2]]);
                    continue;
                }
                
                // for type3 & type4, first find out the separately lise on the other side
                
                int singleIndex = sideB == sideC ? 0 : sideA == sideC ? 1 : 2;
                
                ray1.origin = _oldVertices[submeshTris[j + singleIndex]];
                Vector3 dir1 = _oldVertices[submeshTris[j + (singleIndex + 1) % 3]] -
                               _oldVertices[submeshTris[j + singleIndex]];
                ray1.direction = dir1;
                plane.Raycast(ray1, out float enter1);
                float lerp1 = enter1 / dir1.magnitude;

                ray2.origin = _oldVertices[submeshTris[j + singleIndex]];
                Vector3 dir2 = _oldVertices[submeshTris[j + (singleIndex + 2) % 3]] - 
                               _oldVertices[submeshTris[j + singleIndex]];
                ray2.direction = dir2;
                plane.Raycast(ray2, out float enter2);
                float lerp2 = enter2 / dir2.magnitude;
                
                if (sideCount == 1)
                {
                    AddTriangle(i, _oldVertices[submeshTris[j + singleIndex]],
                        ray1.origin + ray1.direction.normalized * enter1,
                        ray2.origin + ray2.direction.normalized * enter2,
                        _oldNormals[submeshTris[j + singleIndex]],
                        Vector3.Lerp(_oldNormals[submeshTris[j + singleIndex]],
                            _oldNormals[submeshTris[j + (singleIndex + 1) % 3]], lerp1),
                        Vector3.Lerp(_oldNormals[submeshTris[j + (singleIndex + 2) % 3]],
                            _oldNormals[submeshTris[j + (singleIndex + 2) % 3]], lerp2)
                    );
                }else if (sideCount == 2)
                {
                    AddTriangle(i, ray1.origin + ray1.direction.normalized * enter1,
                        _oldVertices[submeshTris[j + (singleIndex + 1) % 3]],
                        _oldVertices[submeshTris[j + (singleIndex + 2) % 3]],
                        Vector3.Lerp(_oldNormals[submeshTris[j + singleIndex]],
                            _oldNormals[submeshTris[j + (singleIndex + 1) % 3]], lerp1),
                        _oldNormals[submeshTris[j + (singleIndex + 1) % 3]],
                        _oldNormals[submeshTris[j + (singleIndex + 2) % 3]]);
                    AddTriangle(i, ray1.origin + ray1.direction.normalized * enter1,
                        _oldVertices[submeshTris[j + (singleIndex + 2) % 3]],
                        ray2.origin + ray2.direction.normalized * enter2,
                        Vector3.Lerp(_oldNormals[submeshTris[j + singleIndex]],
                            _oldNormals[submeshTris[j + (singleIndex + 1) % 3]], lerp1),
                        _oldNormals[submeshTris[j + (singleIndex + 2) % 3]],
                        Vector3.Lerp(_oldNormals[submeshTris[j + singleIndex]],
                            _oldNormals[submeshTris[j + (singleIndex + 2) % 3]], lerp2));
                }
            }
        }
        SetNewMesh();
    }

    
    private void AddTriangle(int submesh, Vector3 vert1, Vector3 vert2, Vector3 vert3,
                                          Vector3 norm1, Vector3 norm2, Vector3 norm3)
    {
        if (_newTriangles.Count - 1 < submesh)
        {
            _newTriangles.Add(new List<int>());
        }
        _newTriangles[submesh].Add(_newVertices.Count);
        _newVertices.Add(vert1);
        _newTriangles[submesh].Add(_newVertices.Count);
        _newVertices.Add(vert2);
        _newTriangles[submesh].Add(_newVertices.Count);
        _newVertices.Add(vert3);
        _newNormals.Add(norm1);
        _newNormals.Add(norm2);
        _newNormals.Add(norm3);
        _newBounds.min = Vector3.Min(_newBounds.min, vert1);
        _newBounds.min = Vector3.Min(_newBounds.min, vert2);
        _newBounds.min = Vector3.Min(_newBounds.min, vert3);
        _newBounds.max = Vector3.Min(_newBounds.max, vert1);
        _newBounds.max = Vector3.Min(_newBounds.max, vert2);
        _newBounds.max = Vector3.Min(_newBounds.max, vert3);
    }

    private void AddEdge(int subMesh, Vector3 planeNormal, Vector3 vert1, Vector3 vert2)
    {
        if (!edgeSet)
        {
            edgeSet = true;
            edgeVert = vert1;
        }
        else
        {
            edgePlane.Set3Points(edgeVert, vert1, vert2);
            
            AddTriangle(subMesh, 
                    edgeVert, 
                    edgePlane.GetSide(edgeVert + planeNormal) ? vert1 : vert2,
                    edgePlane.GetSide(edgeVert + planeNormal) ? vert2 : vert1,
                    planeNormal, planeNormal, planeNormal);
        }
    }

    private void SetNewMesh()
    {
        Mesh _newMesh = new Mesh();
        _newMesh.vertices = _newVertices.ToArray();
        
        for(int i = 0; i < _newTriangles.Count; i++)
        {
            _newMesh.SetTriangles(_newTriangles[i], i, true);
        }

        _newMesh.bounds = _newBounds;
        _originalMeshFilter.mesh = _newMesh;
    }
}
