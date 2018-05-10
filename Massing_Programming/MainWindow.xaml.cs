using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using HelixToolkit.Wpf;

namespace Massing_Programming
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /*----- Initial Parameters -----*/
        Model3DGroup stackingVisualization = new Model3DGroup();

        float initialFloorHeight = 15;

        float[] initialProjectBoxDims = { 150, 200, 100 };

        int initialNumberOfDepartments = 4;
        int initialNumberOfPrograms = 4;
        List<String> namesOfDepartments = new List<string>();

        // Random Object
        Random random = new Random(10);

        public MainWindow()
        {
            InitializeComponent();

            // Setting up values of the initial dimensions of the Project Box
            this.ProjectWidth.Text = initialProjectBoxDims[0].ToString();
            this.ProjectLength.Text = initialProjectBoxDims[1].ToString();
            this.ProjectHeight.Text = initialProjectBoxDims[2].ToString();
            this.FloorHeight.Text = initialFloorHeight.ToString();

            // ProjectBox Visualization
            Point3D projectBoxCenter = new Point3D(0, 0, float.Parse(this.ProjectHeight.Text) * 0.5);
            Material projectBoxMaterial = new SpecularMaterial(Brushes.Transparent, 1);
            Material projectBoxInsideMaterial = MaterialHelper.CreateMaterial(Colors.Gray);
            GeometryModel3D projectBox = VisualizationMethods.GenerateBox(projectBoxCenter,
                new float[] {float.Parse(ProjectWidth.Text), float.Parse(ProjectLength.Text), float.Parse(ProjectHeight.Text) },
                projectBoxMaterial, projectBoxInsideMaterial);
            projectBox.SetName("ProjectBox");
            this.stackingVisualization.Children.Add(projectBox);

            this.NumberOfDepartments.Text = initialNumberOfDepartments.ToString();

            for (int i = 0; i < initialNumberOfDepartments; i++)
            {
                // Setting up initial Departments' expanders
                Expander department = ExtraMethods.DepartmentGernerator(i);
                namesOfDepartments.Add(department.Name);

                ExtraMethods.departmentExpanderGenerator(department, initialNumberOfPrograms,
                    new RoutedEventHandler(DepartmentNameAndNumberButton_Click));

                this.DepartmentsWrapper.Children.Add(department);

                /*---------------------------------------------------------------------------------*/

                /*--- Setting up initial Departments and Programs visualization ---*/
                // Generating a random color in the format of an array that contains three bytes
                byte[] color = { Convert.ToByte(random.Next(255)), Convert.ToByte(random.Next(255)), Convert.ToByte(random.Next(255)) };

                for (int j = 0; j < initialNumberOfPrograms; j++)
                {
                    // Calculating length of each program based on total area of the program and width of the Project Box
                    Slider keyRooms = LogicalTreeHelper.FindLogicalNode(department, department.Name + "Rooms" + j.ToString()) as Slider;
                    Slider DGSF = LogicalTreeHelper.FindLogicalNode(department, department.Name + "DGSF" + j.ToString()) as Slider;
                    float programLength = ((float) (keyRooms.Value * DGSF.Value)) / float.Parse(this.ProjectWidth.Text);
                    
                    // Generate gradient colors for programs of each department
                    float stop = ((float)j) / ((float)initialNumberOfPrograms);
                    byte[] gradient = VisualizationMethods.GenerateGradientColor(color, stop);
                    Material programBoxMaterial = MaterialHelper.CreateMaterial(Color.FromRgb(gradient[0], gradient[1], gradient[2]));

                    float[] programBoxDims = { float.Parse(this.ProjectWidth.Text), programLength, float.Parse(this.FloorHeight.Text) };
                    Point3D programBoxCenter = new Point3D(0,
                        ((programBoxDims[1] * 0.5) + (j * programBoxDims[1])) - (float.Parse(ProjectLength.Text) * 0.5),
                        float.Parse(this.FloorHeight.Text) * 0.5 + (i * float.Parse(this.FloorHeight.Text)));

                    GeometryModel3D programBox = VisualizationMethods.GenerateBox(programBoxCenter, programBoxDims,
                        programBoxMaterial, programBoxMaterial);
                    programBox.SetName(department.Name + "Box" + i.ToString());

                    this.stackingVisualization.Children.Add(programBox);
                }
            }

            this.Visualization.Content = stackingVisualization;
        }
        /*-----------------------------------------------------------------End of Windows Load-------------------------------------------------------------------*/

        /* -----Handeling Number of Departments Button Event-----*/
        private void NumberOfDepartments_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;

            /* Set Number of Departments Event*/
            if (btn.Name == "NumberOfDepartmentsButton")
            {
                int input = new int();
                int existingDepartments = this.DepartmentsWrapper.Children.Count;

                try
                {
                    input = Int32.Parse(this.NumberOfDepartments.Text);
                }
                catch
                {
                    MessageBox.Show("Number of Departments has to be a Counting Number.");
                    this.NumberOfDepartments.Text = existingDepartments.ToString();
                    return;
                }

                /* If user input for Number of Departments is larger than zero */
                if (input > 0)
                {
                    /* Decrease Number of Departments */
                    if (existingDepartments > input)
                    {
                        int difference = existingDepartments - input;
                        //this.DepartmentsWrapper.Children.RemoveRange(input, difference);
                        //namesOfDepartments.RemoveRange(input, difference);

                        for (int i = 0; i < difference; i++)
                        {
                            int lastIndex = this.DepartmentsWrapper.Children.Count - 1;

                            Expander expander = this.DepartmentsWrapper.Children[lastIndex] as Expander;
                            TextBox programNumberTextBox = LogicalTreeHelper.FindLogicalNode(expander, expander.Name + "NumberInputTextBox") as TextBox;

                            int numberOfPrograms = int.Parse(programNumberTextBox.Text);

                            this.DepartmentsWrapper.Children.RemoveAt(lastIndex);
                            namesOfDepartments.RemoveAt(lastIndex);

                            for (int j = 0; j < numberOfPrograms; j++)
                            {
                                int lastProgramIndex = this.stackingVisualization.Children.Count - 1;
                                this.stackingVisualization.Children.RemoveAt(lastProgramIndex);
                            }
                        }
                    }

                    // Increase Number of Departments
                    if (existingDepartments < input)
                    {
                        int difference = input - existingDepartments;

                        for (int i = 0; i < difference; i++)
                        {
                            Expander department = ExtraMethods.DepartmentGernerator((existingDepartments + i));
                            namesOfDepartments.Add(department.Name);

                            ExtraMethods.departmentExpanderGenerator(department, 4, new RoutedEventHandler(DepartmentNameAndNumberButton_Click));

                            this.DepartmentsWrapper.Children.Add(department);

                            // Generating a random color in the format of an array that contains three bytes
                            byte[] color = { Convert.ToByte(random.Next(255)), Convert.ToByte(random.Next(255)), Convert.ToByte(random.Next(255)) };

                            for (int j = 0; j < initialNumberOfPrograms; j++)
                            {
                                // Calculating length of each program based on total area of the program and width of the Project Box
                                Slider keyRooms = LogicalTreeHelper.FindLogicalNode(department, department.Name + "Rooms" + j.ToString()) as Slider;
                                Slider DGSF = LogicalTreeHelper.FindLogicalNode(department, department.Name + "DGSF" + j.ToString()) as Slider;
                                float programLength = ((float)(keyRooms.Value * DGSF.Value)) / float.Parse(this.ProjectWidth.Text);

                                // Add Program's Boxes for the added Departments
                                float stop = ((float)j) / ((float)initialNumberOfPrograms);
                                byte[] gradient = VisualizationMethods.GenerateGradientColor(color, stop);
                                Material programBoxMaterial = MaterialHelper.CreateMaterial(Color.FromRgb(gradient[0], gradient[1], gradient[2]));

                                float[] programBoxDims = { float.Parse(this.ProjectWidth.Text), programLength, float.Parse(this.FloorHeight.Text) };
                                Point3D programBoxCenter = new Point3D(0,
                                    ((programBoxDims[1] * 0.5) + (j * programBoxDims[1])) - (this.initialProjectBoxDims[1] * 0.5),
                                    float.Parse(this.FloorHeight.Text) * 0.5 + ((i + (int.Parse(this.NumberOfDepartments.Text) - difference)) * float.Parse(this.FloorHeight.Text)));

                                GeometryModel3D programBox = VisualizationMethods.GenerateBox(programBoxCenter, programBoxDims,
                                    programBoxMaterial, programBoxMaterial);
                                programBox.SetName(department.Name + "Box" + j.ToString());
                                
                                this.stackingVisualization.Children.Add(programBox);
                            }
                        }
                    }
                    // Input is equal to existing number of Departments
                    if (existingDepartments == input)
                    {
                        return;
                    }
                }

                // If user input for Number of Departments is equal to zero
                else
                {
                    MessageBox.Show("Number of Departments has to be a Counting Number.");
                    this.NumberOfDepartments.Text = existingDepartments.ToString();
                }
            }
        }

        /* ---------- Handeling Reset Departments button ---------- */
        private void ResetDepartments_Click(object sender, RoutedEventArgs e)
        {
            // Clear all the lists
            this.DepartmentsWrapper.Children.Clear();
            this.stackingVisualization.Children.Clear();
            this.NumberOfDepartments.Text = initialNumberOfDepartments.ToString();
            namesOfDepartments.Clear();

            // Setting up values of the initial dimensions of the Project Box
            this.ProjectWidth.Text = initialProjectBoxDims[0].ToString();
            this.ProjectLength.Text = initialProjectBoxDims[1].ToString();
            this.ProjectHeight.Text = initialProjectBoxDims[2].ToString();
            this.FloorHeight.Text = initialFloorHeight.ToString();

            // ProjectBox Visualization
            Point3D projectBoxCenter = new Point3D(0, 0, float.Parse(this.ProjectHeight.Text) * 0.5);
            Material projectBoxMaterial = new SpecularMaterial(Brushes.Transparent, 1);
            Material projectBoxInsideMaterial = MaterialHelper.CreateMaterial(Colors.Gray);
            GeometryModel3D projectBox = VisualizationMethods.GenerateBox(projectBoxCenter,
                new float[] { float.Parse(ProjectWidth.Text), float.Parse(ProjectLength.Text), float.Parse(ProjectHeight.Text) },
                projectBoxMaterial, projectBoxInsideMaterial);
            projectBox.SetName("ProjectBox");
            this.stackingVisualization.Children.Add(projectBox);

            // Generating initial Expanders and programs visualization
            for (int i = 0; i < initialNumberOfDepartments; i++)
            {
                Expander department = ExtraMethods.DepartmentGernerator(i);
                ExtraMethods.departmentExpanderGenerator(department, 4, new RoutedEventHandler(DepartmentNameAndNumberButton_Click));
                namesOfDepartments.Add(department.Name);

                this.DepartmentsWrapper.Children.Add(department);

                /*---------------------------------------------------------------------------------*/

                /*--- Setting up initial Departments and Programs visualization ---*/
                // Generating a random color in the format of an array that contains three bytes
                byte[] color = { Convert.ToByte(random.Next(255)), Convert.ToByte(random.Next(255)), Convert.ToByte(random.Next(255)) };

                for (int j = 0; j < initialNumberOfPrograms; j++)
                {
                    // Calculating length of each program based on total area of the program and width of the Project Box
                    Slider keyRooms = LogicalTreeHelper.FindLogicalNode(department, department.Name + "Rooms" + j.ToString()) as Slider;
                    Slider DGSF = LogicalTreeHelper.FindLogicalNode(department, department.Name + "DGSF" + j.ToString()) as Slider;
                    float programLength = ((float)(keyRooms.Value * DGSF.Value)) / float.Parse(this.ProjectWidth.Text);

                    //Slider DGSF = LogicalTreeHelper.FindLogicalNode(decimal, expander.Name + "NumberInputTextBox") as TextBox;
                    // Generate gradient colors for programs of each department
                    float stop = ((float)j) / ((float)initialNumberOfPrograms);
                    byte[] gradient = VisualizationMethods.GenerateGradientColor(color, stop);
                    Material programBoxMaterial = MaterialHelper.CreateMaterial(Color.FromRgb(gradient[0], gradient[1], gradient[2]));

                    float[] programBoxDims = { float.Parse(this.ProjectWidth.Text), programLength, float.Parse(this.FloorHeight.Text) };
                    Point3D programBoxCenter = new Point3D(0,
                        ((programBoxDims[1] * 0.5) + (j * programBoxDims[1])) - (float.Parse(ProjectLength.Text) * 0.5),
                        float.Parse(this.FloorHeight.Text) * 0.5 + (i * float.Parse(this.FloorHeight.Text)));

                    GeometryModel3D programBox = VisualizationMethods.GenerateBox(programBoxCenter, programBoxDims,
                        programBoxMaterial, programBoxMaterial);
                    programBox.SetName(department.Name + "Box" + i.ToString());

                    this.stackingVisualization.Children.Add(programBox);
                }
            }
        }

        /* ----------------The event for Setting Name of the Departments and the Number of Programs it contains ---------------- */
        private void DepartmentNameAndNumberButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;

            // Setting the Name of the Department (recognizing which button was pressed)
            if (namesOfDepartments.Contains(btn.Name.Replace("SetNameButton", "")))
            {
                Expander expander = LogicalTreeHelper.FindLogicalNode(this.DepartmentsWrapper, btn.Name.Replace("SetNameButton", "")) as Expander;
                TextBox nameTextBox = LogicalTreeHelper.FindLogicalNode(this.DepartmentsWrapper, btn.Name.Replace("SetNameButton", "NameInputTextBox")) as TextBox;

                if (nameTextBox.Text != "")
                {
                    expander.Header = nameTextBox.Text; ;
                }
                else
                {
                    MessageBox.Show("Please Enter a Name Inside the \"Name of Department\" Box.");
                    return;
                }
            }

            // Setting the Number of Programs in the Department (Number of Programs button was pressed) 
            else
            {
                Expander expander = LogicalTreeHelper.FindLogicalNode(this.DepartmentsWrapper, btn.Name.Replace("SetNumberButton", "")) as Expander;
                TextBox numberTextBox = LogicalTreeHelper.FindLogicalNode(this.DepartmentsWrapper, btn.Name.Replace("SetNumberButton", "NumberInputTextBox")) as TextBox;
                Grid programs = LogicalTreeHelper.FindLogicalNode(this.DepartmentsWrapper, btn.Name.Replace("SetNumberButton", "") + "Programs") as Grid;

                int input = new int();
                int existingPrograms = programs.RowDefinitions.Count;

                try
                {
                    input = Int32.Parse(numberTextBox.Text);
                }
                catch
                {
                    MessageBox.Show("Number of Departments has to be a Counting Number.");
                    numberTextBox.Text = existingPrograms.ToString();
                    return;
                }

                if (input > 0)
                {
                    // Increase Number of Programs
                    if (input > existingPrograms)
                    {

                        int difference = input - existingPrograms;
                        ExtraMethods.AddProgram(programs, difference, existingPrograms, expander);
                        int indexOfDepartment = this.DepartmentsWrapper.Children.IndexOf(expander);

                        // Generating a random color in the format of an array that contains three bytes
                        byte[] color = { Convert.ToByte(random.Next(255)), Convert.ToByte(random.Next(255)), Convert.ToByte(random.Next(255)) };

                        for (int i = 0; i < difference; i++)
                        {
                            // Calculating length of each program based on total area of the program and width of the Project Box
                            Slider keyRooms = LogicalTreeHelper.FindLogicalNode(expander, expander.Name + "Rooms" + (i + existingPrograms).ToString()) as Slider;
                            Slider DGSF = LogicalTreeHelper.FindLogicalNode(expander, expander.Name + "DGSF" + (i + existingPrograms).ToString()) as Slider;
                            float programLength = ((float)(keyRooms.Value * DGSF.Value)) / float.Parse(this.ProjectWidth.Text);

                            // Generate gradient colors for programs of each department
                            float stop = ((float)i) / ((float)initialNumberOfPrograms);
                            byte[] gradient = VisualizationMethods.GenerateGradientColor(color, stop);
                            Material programBoxMaterial = MaterialHelper.CreateMaterial(Color.FromRgb(gradient[0], gradient[1], gradient[2]));

                            float[] programBoxDims = { float.Parse(this.ProjectWidth.Text), programLength, float.Parse(this.FloorHeight.Text) };
                            Point3D programBoxCenter = new Point3D(0,
                                (((programBoxDims[1] * 0.5) + (i * programBoxDims[1])) - (float.Parse(ProjectLength.Text) * 0.5)),
                                float.Parse(this.FloorHeight.Text) * 0.5 + (indexOfDepartment * int.Parse(this.FloorHeight.Text)));

                            GeometryModel3D programBox = VisualizationMethods.GenerateBox(programBoxCenter, programBoxDims,
                                programBoxMaterial, programBoxMaterial);
                            programBox.SetName(expander.Name + "Box" + (i + existingPrograms).ToString());
                            //MessageBox.Show(programBox.Bounds.SizeY.ToString());
                            this.stackingVisualization.Children.Add(programBox);
                        }
                    }
                    if (input < existingPrograms)
                    {
                        int difference = programs.RowDefinitions.Count - input;
                        List<UIElement> elementsToRemove = new List<UIElement>();

                        for (int i = 0; i < difference; i++)
                        {
                            foreach (UIElement element in programs.Children)
                            {
                                if (Grid.GetRow(element) == programs.RowDefinitions.Count - 1)
                                {
                                    elementsToRemove.Add(element);
                                }
                            }
                            foreach (UIElement element in elementsToRemove)
                            {
                                programs.Children.Remove(element);
                            }
                            programs.RowDefinitions.RemoveAt(programs.RowDefinitions.Count - 1);
                            elementsToRemove.Clear();
                        }
                    }
                    if (input == existingPrograms)
                    {
                        return;
                    }
                }
                else
                {
                    MessageBox.Show("Number of Departments has to be a Counting Number.");
                    numberTextBox.Text = existingPrograms.ToString();
                }
            }
        }

        /*------------------ Project Size Change Events ------------------*/
        private void ProjectSize_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;

            // Handeling Project Width changes events
            if (btn.Name == "ProjectWidthButton")
            {
                float projectWidthInput = new float();

                try
                {
                    projectWidthInput = float.Parse(this.ProjectWidth.Text);
                }
                catch
                {
                    MessageBox.Show("Please Enter a Number.");
                    this.ProjectWidth.Text = this.stackingVisualization.Children[0].Bounds.SizeX.ToString();
                    return;
                }
                if (projectWidthInput > 0)
                {
                    for (int i = 0; i < this.stackingVisualization.Children.Count; i++)
                    {
                        if (i == 0)
                        {
                            this.stackingVisualization.Children[i].Transform = new ScaleTransform3D(projectWidthInput / this.initialProjectBoxDims[0],
                                this.stackingVisualization.Children[0].Bounds.SizeY / this.initialProjectBoxDims[1],
                                this.stackingVisualization.Children[0].Bounds.SizeZ / this.initialProjectBoxDims[2],
                                0, this.initialProjectBoxDims[1] * -0.5, 0);
                        }
                        else
                        {
                            this.stackingVisualization.Children[i].Transform = new ScaleTransform3D(projectWidthInput / this.initialProjectBoxDims[0],
                                this.initialProjectBoxDims[0] / projectWidthInput, 1,
                                0, this.initialProjectBoxDims[1] * -0.5, 0);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Please Enter a Number Larger than Zero.");
                    this.ProjectWidth.Text = this.stackingVisualization.Children[0].Bounds.SizeX.ToString();
                    return;
                }
            }

            // Handeling Project Length changes events
            if (btn.Name == "ProjectLengthButton")
            {
                float projectLengthInput = new float();

                try
                {
                    projectLengthInput = float.Parse(this.ProjectLength.Text);
                }
                catch
                {
                    MessageBox.Show("Please Enter a Number.");
                    this.ProjectLength.Text = this.stackingVisualization.Children[0].Bounds.SizeY.ToString();
                    return;
                }
                if (projectLengthInput > 0)
                {
                    this.stackingVisualization.Children[0].Transform = new ScaleTransform3D(this.stackingVisualization.Children[0].Bounds.SizeX / this.initialProjectBoxDims[0],
                        projectLengthInput / this.initialProjectBoxDims[1],
                        this.stackingVisualization.Children[0].Bounds.SizeZ / this.initialProjectBoxDims[2],
                        0, this.initialProjectBoxDims[1] * -0.5, 0);
                }
                else
                {
                    MessageBox.Show("Please Enter a Number Larger than Zero.");
                    this.ProjectLength.Text = this.stackingVisualization.Children[0].Bounds.SizeY.ToString();
                    return;
                }
            }

            // Handeling Project Height changes events
            if (btn.Name == "ProjectHeightButton")
            {
                float projectHeightInput = 0;

                try
                {
                    projectHeightInput = float.Parse(this.ProjectHeight.Text);
                }
                catch
                {
                    MessageBox.Show("Please Enter a Number.");
                    this.ProjectHeight.Text = this.stackingVisualization.Children[0].Bounds.SizeZ.ToString();
                    return;
                }
                if (projectHeightInput > 0)
                {
                    this.stackingVisualization.Children[0].Transform = new ScaleTransform3D(this.stackingVisualization.Children[0].Bounds.SizeX / this.initialProjectBoxDims[0],
                        this.stackingVisualization.Children[0].Bounds.SizeY / this.initialProjectBoxDims[1],
                        projectHeightInput / this.initialProjectBoxDims[2], 0, this.initialProjectBoxDims[1] * -0.5, 0);
                }
                else
                {
                    MessageBox.Show("Please Enter a Number Larger than Zero.");
                    this.ProjectHeight.Text = this.stackingVisualization.Children[0].Bounds.SizeZ.ToString();
                    return;
                }
            }
        }
    }
}

