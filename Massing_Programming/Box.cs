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
        public string name { get; set; }
        public Point3D boxCenter { get; set; }
        public float[] dims { get; set; }
        public GeometryModel3D visualizationBox { get; set; }
        public string function { get; set; }
        public float keyRoomsValue { get; set; }
        public float DGSF { get; set; }
        public int floor { get; set; }

        public Box(string name, Point3D boxCenter, float[] dims, Material boxMaterial, Material boxInsideMaterial)
        {
            this.name = name;
            this.boxCenter = boxCenter;
            this.dims = dims;
            this.visualizationBox = VisualizationMethods.GenerateBox(this.boxCenter, this.dims, boxMaterial, boxInsideMaterial);
            visualizationBox.SetName("ProjectBox");
        }
    }
}
