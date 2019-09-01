using UnityEngine;

namespace MeshLib
{
    /// <summary>
    /// Meshes
    /// Contains the details of the mesh
    /// </summary>
    [System.Serializable]
    public class Meshes
    {
        public string name;
        [Range(0.1f, 10f)]
        public float thickness;
        public Mesh mesh;
        public Renderer renderer;
        public Texture texture;

        /// <summary>
        /// Initializes a new instance of the <see cref="MeshLib.Meshes"/> class.
        /// </summary>
        public Meshes(string name, Mesh mesh, Renderer rend, float thickness, Texture texture = null)
        {
            this.name = name;
            this.mesh = mesh;
            this.renderer = rend;
            this.texture = texture;
            this.thickness = thickness;
        }
    }
}