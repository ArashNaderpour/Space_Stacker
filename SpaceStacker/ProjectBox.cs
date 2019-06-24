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
    public class ProjectBox
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MainViewModel"/> class.
        /// </summary>
        /// 
        public Model3DGroup modelGroup;

        // ProjectBox Properties
        public float projectWidth;
        public float projectLength;
        public float projectHeight;

        public ProjectBox(float projectWidth, float projectLength, float projectHeight)
        {
            // Set ProjectBox Properties
            this.projectWidth = projectWidth;
            this.projectLength = projectLength;
            this.projectHeight = projectHeight;

            // Create a model group
            modelGroup = new Model3DGroup();
            modelGroup.SetName("ModelGroup");

            // Create a mesh builder and add a box to it
            var meshBuilder = new MeshBuilder(false, false);
            meshBuilder.AddBox(new Point3D(0, 0, 0.5 * this.projectHeight), this.projectWidth, this.projectLength, this.projectHeight);

            // Create a mesh from the builder (and freeze it)
            var mesh = meshBuilder.ToMesh(true);

            // Create some materials
            var greenMaterial = MaterialHelper.CreateMaterial(Colors.YellowGreen);
            var redMaterial = MaterialHelper.CreateMaterial(Colors.Red);
            var blueMaterial = MaterialHelper.CreateMaterial(Colors.Blue);
            var insideMaterial = MaterialHelper.CreateMaterial(Colors.Yellow);

            // Add 3 models to the group (using the same mesh, that's why we had to freeze it)
            modelGroup.Children.Add(new GeometryModel3D { Geometry = mesh, Material = greenMaterial, BackMaterial = insideMaterial });
            //modelGroup.Children.Add(new GeometryModel3D { Geometry = mesh, Transform = new TranslateTransform3D(-2, 0, 0), Material = redMaterial, BackMaterial = insideMaterial });
            //modelGroup.Children.Add(new GeometryModel3D { Geometry = mesh, Transform = new TranslateTransform3D(2, 0, 0), Material = blueMaterial, BackMaterial = insideMaterial });

            // Set the property, which will be bound to the Content property of the ModelVisual3D (see MainWindow.xaml)
            this.Model = modelGroup;
        }

        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        /// <value>The model.</value>
        public Model3D Model { get; set; }
    }
}
