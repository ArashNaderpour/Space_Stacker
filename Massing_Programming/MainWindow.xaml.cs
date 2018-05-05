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

        int initialNumberOfDepartments = 4;
        int initialNumberOfPrograms = 4;
        List<String> namesOfDepartments = new List<string>();

        // Random Object
        Random random = new Random(14);

        public MainWindow()
        {
            InitializeComponent();

            // ProjectBox Visualization
            Point3D projectBoxCenter = new Point3D(0, 0, float.Parse(this.ProjectHeight.Text) * 0.5);
            float[] projectBoxDims = { float.Parse(this.ProjectWidth.Text), float.Parse(this.ProjectLength.Text), float.Parse(this.ProjectHeight.Text) };
            Material projectBoxMaterial = new SpecularMaterial(Brushes.Transparent, 1);
            Material projectBoxInsideMaterial = MaterialHelper.CreateMaterial(Colors.Gray);
            GeometryModel3D projectBox = VisualizationMethods.GenerateBox(projectBoxCenter, projectBoxDims,
                projectBoxMaterial, projectBoxInsideMaterial);
            projectBox.SetName("ProjectBox");
            stackingVisualization.Children.Add(projectBox);

            this.NumberOfDepartments.Text = initialNumberOfDepartments.ToString();

            for (int i = 0; i < initialNumberOfDepartments; i++)
            {
                // Setting up initial Departments' expanders
                Expander department = ExtraMethods.DepartmentGernerator(i);
                namesOfDepartments.Add(department.Name);

                ExtraMethods.departmentExpanderGenerator(department, initialNumberOfPrograms, new RoutedEventHandler(DepartmentNameAndNumberButton_Click));

                this.DepartmentsWrapper.Children.Add(department);

                /*--- Setting up initial Departments and Programs visualization ---*/
                // Generating Random R, G, B Values
                Byte R = Convert.ToByte(random.Next(255));
                Byte G = Convert.ToByte(random.Next(255));
                Byte B = Convert.ToByte(random.Next(255));

                Material departmentBoxMaterial = MaterialHelper.CreateMaterial(Color.FromRgb(R, G, B));

                for (int j = 0; j < initialNumberOfPrograms; j++)
                {
                    float[] departmentBoxDims = { float.Parse(this.ProjectWidth.Text), 35, float.Parse(this.FloorHeight.Text) };
                    Point3D departmentBoxCenter = new Point3D(0,
                        ((departmentBoxDims[1] * 0.5) + (j * departmentBoxDims[1])) - (projectBoxDims[1] * 0.5),
                        float.Parse(this.FloorHeight.Text) * 0.5 + (i * float.Parse(this.FloorHeight.Text)));

                    GeometryModel3D departmentBox = VisualizationMethods.GenerateBox(departmentBoxCenter, departmentBoxDims,
                        departmentBoxMaterial, departmentBoxMaterial);
                    departmentBox.SetName(department.Name + "Box");

                    stackingVisualization.Children.Add(departmentBox);
                }
            }

            this.Visualization.Content = stackingVisualization;
        }

        /* -----Handeling Button Event-----*/
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
                    MessageBox.Show("Number of Departments has to be a Counting number.");
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
                        this.DepartmentsWrapper.Children.RemoveRange(input, difference);
                        namesOfDepartments.RemoveRange(input, difference);
                    }

                    /* Increase Number of Departments */
                    if (existingDepartments < input)
                    {
                        int difference = input - existingDepartments;

                        for (int i = 0; i < difference; i++)
                        {
                            Expander department = ExtraMethods.DepartmentGernerator((existingDepartments + i));
                            namesOfDepartments.Add(department.Name);

                            ExtraMethods.departmentExpanderGenerator(department, 4, new RoutedEventHandler(DepartmentNameAndNumberButton_Click));

                            this.DepartmentsWrapper.Children.Add(department);
                        }

                        if (existingDepartments == input)
                        {
                            return;
                        }
                    }
                }

                /* If user input for Number of Departments is equal to zero */
                else
                {
                    MessageBox.Show("Number of Departments has to be a Counting number.");
                    this.NumberOfDepartments.Text = existingDepartments.ToString();
                }
            }
        }

        /* Reset Departments */
        private void ResetDepartments_Click(object sender, RoutedEventArgs e)
        {
            this.DepartmentsWrapper.Children.Clear();
            this.NumberOfDepartments.Text = initialNumberOfDepartments.ToString();

            for (int i = 0; i < initialNumberOfDepartments; i++)
            {
                Expander department = new Expander();
                department.Margin = new Thickness(0, 5, 0, 0);
                department.HorizontalAlignment = HorizontalAlignment.Stretch;
                department.Header = "DEPARTMENT" + " " + (i + 1).ToString();
                department.BorderBrush = Brushes.Black;
                department.Background = new SolidColorBrush(Color.FromRgb(128, 169, 237));
                department.Name = "D" + (i + 1).ToString();
                namesOfDepartments.Add(department.Name);

                ExtraMethods.departmentExpanderGenerator(department, 4, new RoutedEventHandler(DepartmentNameAndNumberButton_Click));

                this.DepartmentsWrapper.Children.Add(department);
            }
        }

        /* The event for Setting Number of the Departments and Their Names */
        private void DepartmentNameAndNumberButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;

            /* Setting the Name of the Department */
            if (namesOfDepartments.Contains(btn.Name.Replace("SetNameButton", "")))
            {
                Expander expan = LogicalTreeHelper.FindLogicalNode(this.DepartmentsWrapper, btn.Name.Replace("SetNameButton", "")) as Expander;
                TextBox nameTextBox = LogicalTreeHelper.FindLogicalNode(this.DepartmentsWrapper, btn.Name.Replace("SetNameButton", "NameInputTextBox")) as TextBox;

                if (nameTextBox.Text != "")
                {
                    expan.Header = nameTextBox.Text; ;
                }
                else
                {
                    MessageBox.Show("Please enter a Name inside the \"Name of Department\" box.");
                    return;
                }
            }

            /* Setting the Number of Programs in the Department */
            else
            {
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
                    MessageBox.Show("Number of Departments has to be a Counting number.");
                    numberTextBox.Text = existingPrograms.ToString();
                    return;
                }

                if (input > 0)
                {
                    if (input > existingPrograms)
                    {
                        int difference = input - existingPrograms;
                        ExtraMethods.AddProgram(programs, difference, existingPrograms);
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
                    MessageBox.Show("Number of Departments has to be a Counting number.");
                    numberTextBox.Text = existingPrograms.ToString();
                }
            }
        }

        private void ProjectSize_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Clicked!!");
        }
    }
}

