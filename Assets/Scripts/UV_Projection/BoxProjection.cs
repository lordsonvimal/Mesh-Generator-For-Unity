using UnityEngine;

namespace MeshLib
{
    public class BoxProjection
    {
        /// <summary>
        /// Box uv projection.
        /// texture scale is set to 1 metres
        /// Default rotation is set to 0
        /// Default projection axis is set to Y
        /// </summary>
        public void BoxUvProjection(Mesh mesh, AXIS projectAxis = AXIS.Y, float textureScale = 1f, float uvRot = 0f)
        {
            Vector3[] vertices = mesh.vertices;
            Vector2[] uvs = new Vector2[vertices.Length];
            Vector2[] rotated_uv = new Vector2[vertices.Length];

            if (projectAxis == AXIS.X)
            {
                for (int i = 0; i < vertices.Length; i++)
                {
                    uvs[i] = new Vector2(vertices[i].z / (textureScale), vertices[i].y / (textureScale));
                }
            }
            else if (projectAxis == AXIS.Y)
            {
                for (int i = 0; i < vertices.Length; i++)
                {
                    uvs[i] = new Vector2(vertices[i].x / (textureScale), vertices[i].z / (textureScale));
                }
            }
            else if (projectAxis == AXIS.Z)
            {
                for (int i = 0; i < vertices.Length; i++)
                {
                    uvs[i] = new Vector2(vertices[i].x / (textureScale), vertices[i].y / (textureScale));
                }
            }

            //Rotate UV
            for (int index = 0; index < uvs.Length; index++)
            {
                rotated_uv[index] = Quaternion.AngleAxis(uvRot, Vector3.forward) * uvs[index];
            }

            //Apply UV to mesh
            mesh.uv = rotated_uv;
        }
    }
}
