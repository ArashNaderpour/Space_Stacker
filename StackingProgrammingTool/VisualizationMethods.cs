using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;
using Xceed.Wpf.Toolkit;


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

            labelLeft.Padding = new Thickness(2);
            labelRight.Padding = new Thickness(2);

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
        public static void ReplaceVisualizationLabel(TextGroupVisual3D textGroup, int oldVisBoxIndex, int newBoxIndex, string content,
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

            labelLeft.Padding = new Thickness(2);
            labelRight.Padding = new Thickness(2);

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

            textGroup.Children.RemoveAt((2 * oldVisBoxIndex) - 1);
            textGroup.Children.RemoveAt((2 * oldVisBoxIndex) - 2);

            textGroup.Children.Insert((2 * newBoxIndex) - 2, labelLeft);
            textGroup.Children.Insert((2 * newBoxIndex) - 1, labelRight);
            
        }

        /*------------ Replace Visualization Boxes' Labels ------------*/
        public static void AddVisualizationLabel(TextGroupVisual3D textGroup, int visBoxIndex, string content,
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

            labelLeft.Padding = new Thickness(2);
            labelRight.Padding = new Thickness(2);

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
            
            textGroup.Children.Insert((2 * visBoxIndex) - 2, labelLeft);
            textGroup.Children.Insert((2 * visBoxIndex) - 1, labelRight);
        }

        /*------------ Generate Color Picker For Each Department ------------*/
        public static void GenerateColorPicker(Grid grid, string name, byte[] color, RoutedPropertyChangedEventHandler<Color?> Color_Changed)
        {
            int rowIndex = grid.RowDefinitions.Count;

            // Add Row For Each Program
            RowDefinition gridRow = new RowDefinition();
            gridRow.Height = new GridLength(40);
            grid.RowDefinitions.Add(gridRow);

            // Generate And Display Label Of Each Department
            Label departmentName = new Label();
            departmentName.Content = name;
            departmentName.Height = 30;
            departmentName.FontSize = 14;
            departmentName.BorderBrush = Brushes.Black;
            departmentName.BorderThickness = new Thickness(0.3);
            departmentName.FontWeight = FontWeights.DemiBold;
            departmentName.HorizontalContentAlignment = HorizontalAlignment.Left;
            departmentName.VerticalContentAlignment = VerticalAlignment.Center;
            departmentName.HorizontalAlignment = HorizontalAlignment.Stretch;
            departmentName.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(departmentName, 0);
            Grid.SetRow(departmentName, rowIndex);
            grid.Children.Add(departmentName);

            // Generate And Display ColorPicker Of Each Department
            ColorPicker colorPicker = new ColorPicker();
            colorPicker.SelectedColor = Color.FromRgb(color[0], color[1], color[2]);
            colorPicker.Height = 30;
            colorPicker.HorizontalAlignment = HorizontalAlignment.Stretch;
            colorPicker.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(colorPicker, 1);
            Grid.SetRow(colorPicker, rowIndex);
            grid.Children.Add(colorPicker);
            colorPicker.SelectedColorChanged += Color_Changed;
        }
    }
}
