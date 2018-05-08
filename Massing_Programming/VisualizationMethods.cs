using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;

namespace Massing_Programming
{
    class VisualizationMethods
    {
        public static GeometryModel3D GenerateBox(Point3D center, float[] dimenstions, Material material, Material insideMaterial)
        {
            // Create a mesh builder and add a box to it
            var meshBuilder = new MeshBuilder(false, false);
            meshBuilder.AddBox(center, dimenstions[0], dimenstions[1], dimenstions[2]);

            // Create a mesh from the builder (and freeze it)
            var mesh = meshBuilder.ToMesh(true);

            GeometryModel3D box = new GeometryModel3D { Geometry = mesh, Material = material, BackMaterial = insideMaterial };

            return box;
        }

        public static byte[] GenerateGradientColor(byte[] color, float stop)
        {
            float stepR = (255 - color[0]) * stop;
            float stepG = (255 - color[1]) * stop;
            float stepB = (255 - color[2]) * stop;

            double R = color[0] + stepR;
            double G = color[1] + stepG;
            double B = color[2] + stepB;

            byte[] result = { Convert.ToByte(R), Convert.ToByte(G), Convert.ToByte(B) };

            return result;
        }
    }
}
