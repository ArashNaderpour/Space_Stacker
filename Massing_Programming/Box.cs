using System.Windows.Media.Media3D;


namespace Massing_Programming
{
    class Box
    {
        public string name { get; set; }
        public Point3D boxCenter { get; set; }
        public string function { get; set; }
        public int keyRooms { get; set; }
        public float DGSF { get; set; }
        public float GSFValue { get; set; }
        public float rawCostValue { get; set; }
        public int floor { get; set; }

        public Box(string name, Point3D boxCenter)
        {
            this.name = name;
            this.boxCenter = boxCenter;
        }
    }
}
