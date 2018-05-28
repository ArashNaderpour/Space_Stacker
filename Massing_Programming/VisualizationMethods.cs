using System;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;

namespace Massing_Programming
{
    class VisualizationMethods
    {
        /*------------ Generate a box that represents boundaries of the project and programs in each department ------------*/
        public static GeometryModel3D GenerateBox(string name, Point3D center, float[] dimenstions, Material material, Material insideMaterial)
        {
            // Create a mesh builder and add a box to it
            var meshBuilder = new MeshBuilder(false, false);
            meshBuilder.AddBox(center, dimenstions[0], dimenstions[1], dimenstions[2]);

            // Create a mesh from the builder (and freeze it)
            var mesh = meshBuilder.ToMesh(true);

            GeometryModel3D box = new GeometryModel3D { Geometry = mesh, Material = material, BackMaterial = insideMaterial };

            box.SetName(name);

            return box;
        }

        /*------------ Generate gradients of a color ------------*/
        public static byte[] GenerateGradientColor(byte[] color, float stop)
        {
            float stepR = (255 - color[0]) * stop;
            float stepG = (255 - color[1]) * stop;
            float stepB = (255 - color[2]) * stop;

            double R = Math.Min(color[0] + stepR, 255);
            double G = Math.Min(color[1] + stepG, 255);
            double B = Math.Min(color[2] + stepB, 255);

            byte[] result = { Convert.ToByte(R), Convert.ToByte(G), Convert.ToByte(B) };

            return result;
        }
    }
}
