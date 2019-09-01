using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace MeshLib
{
    /// <summary>
    /// Mesh controller responsible for creating mesh using a given set of points
    /// Can create MESH AND SUB MESH
    /// CANNOT CREATE MESH WITH POINTS WHICH HAVE DIFFERENT VALUES FOR X,Y,Z
    /// MESH HAVE REVERSED SIDES IF THE POINTS ARE CCW IN A AXIS OR IF THE AXIS IS NEGATIVE (-X,-Y,-Z)
    /// USE ONLY CW POINTS IN A AXIS
    /// </summary>
    public class MeshController
    {
        /// <summary>
        /// Create the mesh from specified points, thickness, name and axis
        /// </summary>
        public Meshes Create(List<Vector3> points, float thickness, string name, AXIS axis = AXIS.Y)
        {
            GameObject meshObject = new GameObject(name);
            meshObject.name = name;
            meshObject.transform.position = GetMidPt(points);

            //get the vertices for building mesh
            Vector3[] verts = GetVertices(meshObject.transform, points, thickness, axis);

            Vector2[] pointsToTriangulate = ConvertPointsTo2DArray(meshObject.transform, verts, axis);

            //get necessary triangles to build mesh
            int[] triangles = GetTriangles(pointsToTriangulate, points.Count);

            //If the thickness is less than 0 then the triangles needs to be reversed
            //so that normals will be proper
            if (thickness < 0)
            {
                triangles = ReverseTriangles(triangles, points.Count);
            }

            //generate mesh using the vertices and triangles
            Mesh mesh = CreateMesh(verts,triangles);

            //UV projection for generated mesh
            BoxProjection b = new BoxProjection();
            b.BoxUvProjection(mesh,axis);

            //add mesh filter
            MeshFilter meshFilter = meshObject.AddComponent(typeof(MeshFilter)) as MeshFilter;
            meshFilter.mesh = mesh;

            //add meshrenderer
            Renderer renderer = meshObject.AddComponent(typeof(MeshRenderer)) as Renderer;
            renderer.material = new Material(Shader.Find("Standard"));

            //create object for floor/ceiling
            Meshes meshObj = new Meshes(name, mesh, renderer, thickness);

            return meshObj;
        }

        /// <summary>
        /// Creates the mesh.
        /// </summary>
        /// <returns>The mesh.</returns>
        Mesh CreateMesh(Vector3[] vertices,int[] triangles)
        {
            Mesh _mesh = new Mesh();
            _mesh.vertices = vertices;
            _mesh.triangles = triangles;
            _mesh.RecalculateNormals();
            _mesh.RecalculateBounds();

            return _mesh;
        }

        /// <summary>
        /// Creates the mesh with sub mesh.
        /// There can be only 3 submesh(one for top, bottom and side)
        /// </summary>
        /// <returns>The sub mesh.</returns>
        public Meshes CreateSubMesh(List<Vector3> points, float thickness, string name, int submeshCount = 3)
        {
            AXIS axis = GetAxisFromPoints(points);
        
            Meshes mesh = Create(points, thickness, name, axis);

            //set sub meshes so that we can add materials for each side
            //Top, Bottom, Side (There can be only 3 sides as per as the logic)
            SetSubMesh(mesh.mesh, mesh.mesh.triangles, points.Count, submeshCount, mesh.renderer);

            return mesh;
        }

        /// <summary>
        /// Sets the sub mesh.
        /// Used for adding materials for each sides
        /// </summary>
        void SetSubMesh(Mesh mesh, int[] triangles, int pointsCount, int subMeshCount, Renderer renderer)
        {
            mesh.subMeshCount = subMeshCount;
            int topSubMeshTrianglesCount = (pointsCount - 2) * subMeshCount;
            mesh.SetTriangles(GetSubmesh(triangles, 0, topSubMeshTrianglesCount), 0);
            mesh.SetTriangles(GetSubmesh(triangles, topSubMeshTrianglesCount, topSubMeshTrianglesCount * 2), 1);
            mesh.SetTriangles(GetSubmesh(triangles, topSubMeshTrianglesCount * 2, triangles.Length), 2);

            Material mat = new Material(Shader.Find("Standard"));
            renderer.materials = new Material[mesh.subMeshCount];
            for (int i = 0; i < mesh.subMeshCount; i++)
            {
                Color color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
                renderer.materials[i] = mat;
                renderer.materials[i].color = color;
            }
        }

        /// <summary>
        /// Gets the middle point.
        /// </summary>
        /// <returns>The middle point.</returns>
        /// <param name="points">Points.</param>
        Vector3 GetMidPt(List<Vector3> points)
        {
            Vector3 total = Vector3.zero;
            for (int i = 0; i < points.Count; i++)
            {
                total += points[i];
            }
            total /= points.Count;

            return total;
        }

        /// <summary>
        /// Gets the vertex count.
        /// </summary>
        /// <returns>The vertex count.</returns>
        int GetVertexCount(int no_of_sides,int vertex_per_side)
        {
            return no_of_sides * vertex_per_side;
        }

        /// <summary>
        /// Gets the vertices.
        /// </summary>
        /// <returns>The vertices.</returns>
        Vector3[] GetVertices(Transform trans, List<Vector3> points, float thickness, AXIS axis = AXIS.Y, int vertexPerSide = 4)
        {
            int sideVertexCount = GetVertexCount(points.Count,vertexPerSide);

            //All the vertices with thickness
            Vector3[] _vertices = new Vector3[(2 * points.Count) + sideVertexCount];

            //Top Vertices
            Vector3[] _topVertices = new Vector3[points.Count];
            for (int i = 0; i < points.Count; i++)
            {
                _topVertices[i] = trans.InverseTransformPoint(points[i]);
            }

            //Bottom Vertices
            Vector3[] _bottomVertices = new Vector3[points.Count];
            for (int i = 0; i < points.Count; i++)
            {
                if (axis == AXIS.X)
                {
                    _bottomVertices[i] = trans.InverseTransformPoint(new Vector3(points[i].x - thickness, points[i].y, points[i].z));
                }
                else if (axis == AXIS.Y)
                {
                    _bottomVertices[i] = trans.InverseTransformPoint(new Vector3(points[i].x, points[i].y - thickness, points[i].z));
                }
                else if (axis == AXIS.Z)
                {
                    _bottomVertices[i] = trans.InverseTransformPoint(new Vector3(points[i].x, points[i].y, points[i].z + thickness));
                }
            }

            //Side Vertices
            Vector3[] _sideVertices = new Vector3[sideVertexCount];
            int _vertexIndex = 0;
            for (int point = 0; point < points.Count; point++)
            {
                int nextPoint = point + 1;
                if (nextPoint == points.Count)
                    nextPoint = 0;

                //For every side quads, create side vertices
                //Side vertices are arranged like topvertex(0,1),botvertex(0,1) to get quad
                _sideVertices[_vertexIndex] = _topVertices[point];
                _sideVertices[_vertexIndex + 1] = _topVertices[nextPoint];
                _sideVertices[_vertexIndex + 2] = _bottomVertices[point];
                _sideVertices[_vertexIndex + 3] = _bottomVertices[nextPoint];
                _vertexIndex += 4;
            }

            _vertices = (_topVertices.Concat(_bottomVertices).Concat(_sideVertices)).ToArray();

            return _vertices;
        }

        /// <summary>
        /// Converts the points.
        /// </summary>
        /// <returns>The points.</returns>
        Vector2[] ConvertPointsTo2DArray(Transform trans, Vector3[] points, AXIS axis)
        {
            Vector2[] convertedPoints = new Vector2[points.Length];

            for (int i = 0; i < points.Length; i++)
            {
                if (axis == AXIS.X)
                {
                    convertedPoints[i] = trans.TransformPoint(new Vector2(points[i].z, points[i].y));
                }
                else if (axis == AXIS.Y)
                {
                    convertedPoints[i] = trans.TransformPoint(new Vector2(points[i].x, points[i].z));
                }
                else if (axis == AXIS.Z)
                {
                    convertedPoints[i] = trans.TransformPoint(new Vector2(points[i].x, points[i].y));
                }
            }

            return convertedPoints;
        }

        /// <summary>
        /// Sets the submesh.
        /// </summary>
        /// <returns>The submesh.</returns>
        int[] GetSubmesh(int[] singleMeshTriangles, int fromIndex, int endIndex)
        {
            int[] subTriangles = new int[endIndex - fromIndex];
            int i = 0;
            while (i < subTriangles.Length)
            {
                subTriangles[i] = singleMeshTriangles[fromIndex + i];
                i++;
            }
            return subTriangles;
        }

        /// <summary>
        /// Gets the triangles for all sides.
        /// Basic Vertex count represents only the count of basic plane
        /// </summary>
        /// <returns>The triangles for all sides.</returns>
        int[] GetTriangles(Vector2[] verts, int countOfPoints)
        {
            //Triangulate Top Verts
            List<Vector2> topVerts = new List<Vector2>();
            for (int topVert = 0; topVert < countOfPoints; topVert++)
            {
                topVerts.Add(new Vector2(verts[topVert].x, verts[topVert].y));
            }
            Triangulator _topTriangulator = new Triangulator(topVerts);
            int[] _topTriangles = _topTriangulator.Triangulate();

            //Triangulate Bottom Verts
            List<Vector2> bottomVerts = new List<Vector2>();
            for (int botVert = countOfPoints; botVert < countOfPoints * 2; botVert++)
            {
                bottomVerts.Add(new Vector2(verts[botVert].x, verts[botVert].y));
            }
            Triangulator _botTriangulator = new Triangulator(bottomVerts);
            int[] _botTriangles = _botTriangulator.Triangulate();
            System.Array.Reverse(_botTriangles);

            //This will give the next set of indices starting from the given value
            _botTriangles = AddOffsetToArray(_botTriangles,countOfPoints);

            //Triangulate Side Verts
            int[] _sideTriangles = new int[countOfPoints * 6];
            int sideVert = countOfPoints * 2;
            int i = 0;

            //Arrange triangles in order
            while (sideVert < verts.Length)
            {
                _sideTriangles[i] = sideVert;
                _sideTriangles[i + 1] = sideVert + 2;
                _sideTriangles[i + 2] = sideVert + 3;
                _sideTriangles[i + 3] = sideVert + 3;
                _sideTriangles[i + 4] = sideVert + 1;
                _sideTriangles[i + 5] = sideVert;
                i += 6;
                sideVert += 4;
            }

            //Concatenating all the sides will give final triangle array for mesh
            return((_topTriangles.Concat(_botTriangles)).Concat(_sideTriangles).ToArray());
        }

        /// <summary>
        /// Reverses the triangles.
        /// </summary>
        /// <returns>The triangles.</returns>
        int[] ReverseTriangles(int[] triangles, int countOfPoints)
        {
            //Reverse Top Triangles
            int[] topTriangles = new int[(countOfPoints - 2) * 3];
            for (int top = 0; top < (countOfPoints - 2) * 3; top++)
            {
                topTriangles[top] = triangles[top];
            }
            System.Array.Reverse(topTriangles);

            //Reverse Bot Triangles
            int[] botTriangles = new int[(countOfPoints - 2) * 3];
            int i = 0;
            for (int bot = (countOfPoints - 2) * 3; bot < (countOfPoints - 2) * 6; bot++)
            {
                botTriangles[i] = triangles[bot];
                i++;
            }
            System.Array.Reverse(botTriangles);

            //Reverse Side Triangles
            int[] sideTriangles = new int[countOfPoints * 6];
            i = 0;
            for (int side = (countOfPoints - 2) * 6; side < triangles.Length; side++)
            {
                sideTriangles[i] = triangles[side];
                i++;
            }
            System.Array.Reverse(sideTriangles);

            int[] finalTriangles = (topTriangles.Concat(botTriangles)).Concat(sideTriangles).ToArray();

            return finalTriangles;
        }

        /// Identifies the axis for the given points.
        AXIS GetAxisFromPoints(List<Vector3> points)
        {
            AXIS axis = AXIS.X;

            if (points.All(x => Mathf.Approximately(Mathf.Abs(Mathf.Floor(x.x)), Mathf.Abs(Mathf.Floor(points.FirstOrDefault().x)))))
            {
                axis = AXIS.X;
            }
            else if (points.All(y => Mathf.Approximately(Mathf.Abs(Mathf.Floor(y.y)), Mathf.Abs(Mathf.Floor(points.FirstOrDefault().y)))))
            {
                axis = AXIS.Y;
            }
            else if (points.All(z => Mathf.Approximately(Mathf.Abs(Mathf.Floor(z.z)), Mathf.Abs(Mathf.Floor(points.FirstOrDefault().z)))))
            {
                axis = AXIS.Z;
            }
            else
            {
                throw new UnityException("Give points with any one axis as constant.AXIS out of alignment");
            }
            return axis;
        }

        /// <summary>
        /// Adds the offset to array.
        /// </summary>
        /// <returns>The offset to array.</returns>
        int[] AddOffsetToArray(int[] array,int value)
        {
            int[] _newArray = new int[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                _newArray[i] = value + array[i];
            }
            return _newArray;
        }
    }
}