//using UnityEngine;
//
//public static class MeshExtensions
//{
//    /// <summary>
//    /// Uvprojection the specified mesh.
//    /// </summary>
//    /// <param name="mesh">Mesh.</param>
//    public static void BoxUvProjection(this Mesh mesh)
//    {
//        Vector3[] vertices = mesh.vertices;
//        Vector2[] uvs = new Vector2[vertices.Length];
//        Vector2[] rotated_uv = new Vector2[vertices.Length];
//        Bounds bounds = mesh.bounds;
//        //scale based on feet unit
//        float textureScale = 3.048f;
//        int i = 0;
//        float UVrot = 0f;
//        while (i < uvs.Length)
//        {
//            uvs[i] = new Vector2(vertices[i].x / (textureScale), vertices[i].z / (textureScale));
//            rotated_uv[i] = Quaternion.AngleAxis(UVrot, Vector3.forward) * uvs[i];
//            i++;
//        }
//        mesh.uv = rotated_uv;
//    }
//}
