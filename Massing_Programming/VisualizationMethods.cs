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
            float R = ExtraMethods.MapValue(0, 255, 0, 1, color[0]);
            float G = ExtraMethods.MapValue(0, 255, 0, 1, color[1]);
            float B = ExtraMethods.MapValue(0, 255, 0, 1, color[2]);

            float t = ExtraMethods.MapValue(0, 1, 0.5f, 5, stop);

            R = R * t;
            G = G * t;
            B = B * t;

            R = ExtraMethods.MapValue(0, 3, 0, 255, R);
            G = ExtraMethods.MapValue(0, 3, 0, 255, G);
            B = ExtraMethods.MapValue(0, 3, 0, 255, B);

            R = Math.Min(Math.Abs(255 - R), 255);
            G = Math.Min(Math.Abs(255 - G), 255);
            B = Math.Min(Math.Abs(255 - B), 255);

            byte[] result = { Convert.ToByte(R), Convert.ToByte(G), Convert.ToByte(B) };

            return result;
        }
    }
}
