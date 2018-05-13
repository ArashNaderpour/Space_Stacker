﻿using System;
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
using Excel = Microsoft.Office.Interop.Excel;

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

        // Department Properties (Names Colors)
        List<String> namesOfDepartments = new List<string>();
        List<byte[]> colorsOfDepartments = new List<byte[]>();

        // Spread-Sheet Data
        Dictionary<String, Dictionary<String, float>> functions = new Dictionary<String, Dictionary<String, float>>();

        // Random Object
        Random random = new Random(20);

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
                new float[] { float.Parse(ProjectWidth.Text), float.Parse(ProjectLength.Text), float.Parse(ProjectHeight.Text) },
                projectBoxMaterial, projectBoxInsideMaterial);
            projectBox.SetName("ProjectBox");
            this.stackingVisualization.Children.Add(projectBox);

            this.Visualization.Content = stackingVisualization;
        }
        /*-----------------------------------------------------------------End of Windows Load-------------------------------------------------------------------*/

        /* Handeling Open Spread-Sheet File Event*/
        private void OpenSpreadSheet_Click(object sender, RoutedEventArgs e)
        {
            // Clear The Main Dictionary
            this.functions.Clear();

            // Open the Spread Sheet File
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();

            // Excel File Properties
            Excel.Application xlApp;
            Excel.Workbook xlWorkBook;
            Excel.Worksheet xlWorkSheet;
            Excel.Range range;

            String filePath = "";

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                filePath = openFileDialog.FileName;

                if (filePath.Substring(filePath.Length - 3).ToLower() != "xls" &&
                    filePath.Substring(filePath.Length - 4).ToLower() != "xlsx")
                {
                    MessageBox.Show("Please Select an Execl File.");
                    return;
                }
            }
            else
            {
                MessageBox.Show("Something Went Wrong. Pleas Try Again.");
                return;
            }

            xlApp = new Excel.Application();
            xlWorkBook = xlApp.Workbooks.Open(filePath, 0, true, 5, "", "", true,
                Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false,
                false, 0, true, 1, 0);
            xlWorkSheet = (Excel.Worksheet)xlWorkBook.Worksheets.get_Item(1);

            range = xlWorkSheet.UsedRange;
            int rowCount = range.Rows.Count;
            int columnCount = range.Columns.Count;

            if (rowCount > 1 && columnCount == 8)
            {
                for (int r = 1; r <= rowCount; r++)
                {
                    Dictionary<String, float> tempDictionary = new Dictionary<String, float>();
                    if (r > 1)
                    {
                        tempDictionary.Add("cost", (float)(range.Cells[r, 2] as Excel.Range).Value2);
                        tempDictionary.Add("keyMin", (float)(range.Cells[r, 3] as Excel.Range).Value2);
                        tempDictionary.Add("keyVal", (float)(range.Cells[r, 4] as Excel.Range).Value2);
                        tempDictionary.Add("keyMax", (float)(range.Cells[r, 5] as Excel.Range).Value2);
                        tempDictionary.Add("dgsfMin", (float)(range.Cells[r, 6] as Excel.Range).Value2);
                        tempDictionary.Add("dgsfVal", (float)(range.Cells[r, 7] as Excel.Range).Value2);
                        tempDictionary.Add("dgsfMax", (float)(range.Cells[r, 8] as Excel.Range).Value2);

                        //Adding Data to Main Data Dictionary
                        this.functions.Add((String)(range.Cells[r, 1] as Excel.Range).Value2, tempDictionary);
                    }
                }

                // Adding Department Expanders and Programs to the Controller Window
                this.NumberOfDepartments.Text = this.initialNumberOfDepartments.ToString();

                for (int i = 0; i < this.initialNumberOfDepartments; i++)
                {
                    // Setting up initial Departments' expanders
                    Expander department = ExtraMethods.DepartmentGernerator(i);
                    this.namesOfDepartments.Add(department.Name);

                    ExtraMethods.departmentExpanderGenerator(department, initialNumberOfPrograms,
                        this.functions, new RoutedEventHandler(DepartmentNameAndNumberButton_Click));

                    this.DepartmentsWrapper.Children.Add(department);

                    /*--- Setting up Initial Departments and Programs Visualization ---*/
                    // Generating a random color in the format of an array that contains three bytes
                    byte[] color = { Convert.ToByte(random.Next(255)), Convert.ToByte(random.Next(255)), Convert.ToByte(random.Next(255)) };
                    this.colorsOfDepartments.Add(color);

                    for (int j = 0; j < initialNumberOfPrograms; j++)
                    {
                        // Calculating length of each program based on total area of the program and width of the Project Box
                        Slider keyRooms = LogicalTreeHelper.FindLogicalNode(department, department.Name + "Rooms" + j.ToString()) as Slider;
                        Slider DGSF = LogicalTreeHelper.FindLogicalNode(department, department.Name + "DGSF" + j.ToString()) as Slider;
                        float programLength = ((float)(keyRooms.Value * DGSF.Value)) / float.Parse(this.ProjectWidth.Text);

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

                xlWorkBook.Close(true, null, null);
                xlApp.Quit();

                System.Runtime.InteropServices.Marshal.ReleaseComObject(xlWorkSheet);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(xlWorkBook);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(xlApp);

                // Enabling the Disabled Controllers
                this.BGSFBox.IsEnabled = true;
                this.ProgramLabel.IsEnabled = true;

                this.NumberOfDepartments.IsEnabled = true;
                this.NumberOfDepartments.Background = Brushes.White;

                this.NumberOfDepartmentsButton.IsEnabled = true;
                this.ResetDepartmentsButton.IsEnabled = true;
            }
            else
            {
                MessageBox.Show("Format of the Data in the Excel File is Inappropriate.");
                return;
            }
        }

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

                        for (int i = 0; i < difference; i++)
                        {
                            int lastIndex = this.DepartmentsWrapper.Children.Count - 1;

                            Expander expander = this.DepartmentsWrapper.Children[lastIndex] as Expander;
                            TextBox programNumberTextBox = LogicalTreeHelper.FindLogicalNode(expander, expander.Name + "NumberInputTextBox") as TextBox;

                            int numberOfPrograms = int.Parse(programNumberTextBox.Text);

                            // Removing Departments' Properties
                            this.DepartmentsWrapper.Children.RemoveAt(lastIndex);
                            this.namesOfDepartments.RemoveAt(lastIndex);
                            this.colorsOfDepartments.RemoveAt(lastIndex);

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
                            this.namesOfDepartments.Add(department.Name);

                            ExtraMethods.departmentExpanderGenerator(department, 4, this.functions, new RoutedEventHandler(DepartmentNameAndNumberButton_Click));

                            this.DepartmentsWrapper.Children.Add(department);

                            // Generating a random color in the format of an array that contains three bytes
                            byte[] color = { Convert.ToByte(random.Next(255)), Convert.ToByte(random.Next(255)), Convert.ToByte(random.Next(255)) };
                            this.colorsOfDepartments.Add(color);

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

                                this.stackingVisualization.Children.Insert(this.stackingVisualization.Children.Count, programBox);
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
            this.namesOfDepartments.Clear();
            this.colorsOfDepartments.Clear();

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
                ExtraMethods.departmentExpanderGenerator(department, 4, this.functions, new RoutedEventHandler(DepartmentNameAndNumberButton_Click));
                this.namesOfDepartments.Add(department.Name);

                this.DepartmentsWrapper.Children.Add(department);

                /*--- Setting up initial Departments and Programs visualization ---*/
                // Generating a random color in the format of an array that contains three bytes
                byte[] color = { Convert.ToByte(random.Next(255)), Convert.ToByte(random.Next(255)), Convert.ToByte(random.Next(255)) };
                this.colorsOfDepartments.Add(color);

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
                int departmentIndex = this.DepartmentsWrapper.Children.IndexOf(expander);

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
                        int programBoxIndex = 0;
                        int firstProgramBoxIndex = 1;
                        for (int i = 0; i < departmentIndex + 1; i++)
                        {
                            Expander tempExpander = this.DepartmentsWrapper.Children[i] as Expander;
                            StackPanel expanderContent = tempExpander.Content as StackPanel;
                            Grid programsGrid = expanderContent.Children[2] as Grid;
                            programBoxIndex += programsGrid.RowDefinitions.Count;

                            if (i < departmentIndex)
                            {
                                firstProgramBoxIndex += programsGrid.RowDefinitions.Count;
                            }
                        }

                        int difference = input - existingPrograms;
                        ExtraMethods.AddProgram(programs, difference, existingPrograms, expander, this.functions);
                        int indexOfDepartment = this.DepartmentsWrapper.Children.IndexOf(expander);

                        // Calculating total length of the exsiting programs
                        double totalExistingProgramsLength = new float();
                        for (int i = 0; i < existingPrograms; i++)
                        {
                            totalExistingProgramsLength += this.stackingVisualization.Children[i + 1].Bounds.SizeY;
                        }
                        totalExistingProgramsLength = (float)totalExistingProgramsLength;

                        // Extracting Color of Department
                        byte[] color = this.colorsOfDepartments[departmentIndex];

                        for (int i = 0; i < input; i++)
                        {
                            // Generate gradient colors for programs of each department
                            float stop = ((float)i) / ((float)(input));

                            byte[] gradient = VisualizationMethods.GenerateGradientColor(color, stop);
                            Material programBoxMaterial = MaterialHelper.CreateMaterial(Color.FromRgb(gradient[0], gradient[1], gradient[2]));

                            if (i < existingPrograms)
                            {
                                ((GeometryModel3D)(this.stackingVisualization.Children[firstProgramBoxIndex + i])).Material = programBoxMaterial;
                            }
                            else
                            {
                                // Calculating length of each program based on total area of the program and width of the Project Box
                                Slider keyRooms = LogicalTreeHelper.FindLogicalNode(expander, expander.Name + "Rooms" + (i).ToString()) as Slider;
                                Slider DGSF = LogicalTreeHelper.FindLogicalNode(expander, expander.Name + "DGSF" + (i).ToString()) as Slider;
                                float programLength = ((float)(keyRooms.Value * DGSF.Value)) / float.Parse(this.ProjectWidth.Text);

                                float[] programBoxDims = { float.Parse(this.ProjectWidth.Text), programLength, float.Parse(this.FloorHeight.Text) };
                                Point3D programBoxCenter = new Point3D(0,
                                    ((totalExistingProgramsLength + ((i - existingPrograms) * programBoxDims[1]) + programBoxDims[1] / 2) - (float.Parse(ProjectLength.Text) * 0.5)),
                                    float.Parse(this.FloorHeight.Text) * 0.5 + (indexOfDepartment * int.Parse(this.FloorHeight.Text)));

                                GeometryModel3D programBox = VisualizationMethods.GenerateBox(programBoxCenter, programBoxDims,
                                    programBoxMaterial, programBoxMaterial);
                                programBox.SetName(expander.Name + "Box" + (i).ToString());

                                this.stackingVisualization.Children.Insert(programBoxIndex + 1, programBox);
                                programBoxIndex += 1;
                            }
                        }
                    }

                    // Decrease Number of Programs
                    if (input < existingPrograms)
                    {
                        int lastProgramBoxIndex = 0;
                        for (int i = 0; i < departmentIndex + 1; i++)
                        {
                            Expander tempExpander = this.DepartmentsWrapper.Children[i] as Expander;
                            StackPanel expanderContent = tempExpander.Content as StackPanel;
                            Grid programsGrid = expanderContent.Children[2] as Grid;
                            lastProgramBoxIndex += programsGrid.RowDefinitions.Count;
                        }

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

                            this.stackingVisualization.Children.RemoveAt(lastProgramBoxIndex);
                            lastProgramBoxIndex += -1;
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

