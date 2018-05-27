using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;

namespace Massing_Programming
{
    class Box
    {
        public string name;
        public Point3D boxCenter;
        public GeometryModel3D visualizationBox;

        public Box(string name, Point3D boxCenter, float[] dims, Material boxMaterial, Material boxInsideMaterial)
        {
            this.name = name;
            this.boxCenter = boxCenter;
            this.visualizationBox = VisualizationMethods.GenerateBox(this.boxCenter, dims, boxMaterial, boxInsideMaterial);
            visualizationBox.SetName("ProjectBox");
        }
    }
}
