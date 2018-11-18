using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Controls;


namespace StackingProgrammingTool
{
    class Box
    {
        public string name { get; set; }
        public string departmentName { get; set; }
        public int indexInDepartment { get; set; }
        public string departmentHeader { get; set; }
        public Point3D boxCenter { get; set; }
        public float[] boxDims { get; set; }
        public Color boxColor { get; set; }
        public string function { get; set; }
        public int keyRooms { get; set; }
        public float DGSF { get; set; }
        public float cost { get; set; }
        public float boxTotalGSFValue { get; set; }
        public float totalRawCostValue { get; set; }
        public int floor { get; set; }
        public int visualizationIndex { get; set; }
        public string visualizationLabel { get; set; }

        public Box(string name, Point3D boxCenter)
        {
            this.name = name;
            this.boxCenter = boxCenter;

            this.departmentName = name.Replace("ProgramBo", "").Split('x')[0];

            this.indexInDepartment = int.Parse(name.Replace("ProgramBo", "").Split('x')[1]);
        }
    }
}
