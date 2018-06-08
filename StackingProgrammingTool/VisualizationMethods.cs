using System;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;

namespace StackingProgrammingTool
{
    class VisualizationMethods
    {
        /*------------ Generate A Box That Represents Boundaries Of The Project And Programs In Each Department ------------*/
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

        /*------------ Generate Gradients Of A Color ------------*/
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

        /*------------ Generate Visualization Boxes' Labels ------------*/
        public static void GenerateVisualizationLabel(TextGroupVisual3D textGroup, string content,
            Point3D center, float[] dims, Color color)
        {
            TextVisual3D labelLeft = new TextVisual3D();
            TextVisual3D labelRight = new TextVisual3D();

            labelLeft.Text = content;
            labelRight.Text = content;

            labelLeft.Position = new Point3D(center.X + (dims[0] / 2 + 0.01), center.Y, center.Z);
            labelRight.Position = new Point3D(center.X - (dims[0] / 2 + 0.01), center.Y, center.Z);

            labelLeft.Height = Math.Min(dims[1], dims[2]);
            labelRight.Height = Math.Min(dims[1], dims[2]);

            labelLeft.UpDirection = new Vector3D(0, 0, 1);
            labelRight.UpDirection = new Vector3D(0, 0, 1);

            labelLeft.TextDirection = new Vector3D(0, 1, 0);
            labelRight.TextDirection = new Vector3D(0, 1, 0);

            labelLeft.Padding = new System.Windows.Thickness(2);
            labelRight.Padding = new System.Windows.Thickness(2);

            labelLeft.Background = Brushes.Transparent;
            labelRight.Background = Brushes.Transparent;

            int mid = (color.R + color.G + color.B) / 3;

            if (mid < 120)
            {
                labelLeft.Foreground = Brushes.White;
                labelRight.Foreground = Brushes.White;
            }
            else
            {
                labelLeft.Foreground = Brushes.Black;
                labelRight.Foreground = Brushes.Black;
            }

            textGroup.Children.Add(labelLeft);
            textGroup.Children.Add(labelRight);
        }

        /*------------ Replace Visualization Boxes' Labels ------------*/
        public static void ReplaceVisualizationLabel(TextGroupVisual3D textGroup, int[] indexes, string content,
            Point3D center, float[] dims, Color color)
        {
            TextVisual3D labelLeft = new TextVisual3D();
            TextVisual3D labelRight = new TextVisual3D();

            labelLeft.Text = content;
            labelRight.Text = content;

            labelLeft.Position = new Point3D(center.X + (dims[0] / 2 + 0.01), center.Y, center.Z);
            labelRight.Position = new Point3D(center.X - (dims[0] / 2 + 0.01), center.Y, center.Z);

            labelLeft.Height = Math.Min(dims[1], dims[2]);
            labelRight.Height = Math.Min(dims[1], dims[2]);

            labelLeft.UpDirection = new Vector3D(0, 0, 1);
            labelRight.UpDirection = new Vector3D(0, 0, 1);

            labelLeft.TextDirection = new Vector3D(0, 1, 0);
            labelRight.TextDirection = new Vector3D(0, 1, 0);

            labelLeft.Padding = new System.Windows.Thickness(2);
            labelRight.Padding = new System.Windows.Thickness(2);

            labelLeft.Background = Brushes.Transparent;
            labelRight.Background = Brushes.Transparent;

            int mid = (color.R + color.G + color.B) / 3;

            if (mid < 120)
            {
                labelLeft.Foreground = Brushes.White;
                labelRight.Foreground = Brushes.White;
            }
            else
            {
                labelLeft.Foreground = Brushes.Black;
                labelRight.Foreground = Brushes.Black;
            }

            textGroup.Children.RemoveAt(indexes[0]);
            textGroup.Children.Insert(indexes[0], labelLeft);
            textGroup.Children.RemoveAt(indexes[1]);
            textGroup.Children.Insert(indexes[1], labelRight);
        }
    }
}
