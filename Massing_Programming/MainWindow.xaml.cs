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

        float initialProgramHeight = 15;
        float initialProgramLength = 0;

        float[] initialProjectBoxDims = { 150, 200, 100 };

        int initialNumberOfDepartments = 4;
        int initialNumberOfPrograms = 4;

        // Output Variables
        float constructionCost = new float();
        float projectCost = new float();
        float budgetDifference = new float();
        float costPerGSF = new float();
        float totalBGSF = new float();
        float limitOfBGSF = new float();

        // Temp Output Variables
        float totalGSF = new float();
        float totalRawDepartmentCost = new float();

        // Department Properties (Names Colors)
        List<byte[]> colorsOfDepartments = new List<byte[]>();

        // Spread-Sheet Data
        Dictionary<String, Dictionary<String, float>> functions = new Dictionary<String, Dictionary<String, float>>();

        // Random Object
        Random random = new Random(20);

        public MainWindow()
        {
            InitializeComponent();

            // Setting up values of the initial dimensions of the Project Box and Floor Height
            this.ProjectWidth.Text = this.initialProjectBoxDims[0].ToString();
            this.ProjectLength.Text = this.initialProjectBoxDims[1].ToString();
            this.ProjectHeight.Text = this.initialProjectBoxDims[2].ToString();

            this.FloorHeight.Text = this.initialProgramHeight.ToString();

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

        /* ########################################################### End of Windows Load and Start of Handeling Events ########################################################### */

        /*---------------- Handeling Open Spread-Sheet File Event ----------------*/
        private void OpenSpreadSheet_Click(object sender, RoutedEventArgs e)
        {
            // Clear all the lists and Dictionaries
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
                if (this.DepartmentsWrapper.Children.Count > 0)
                {
                    // Clear all the lists
                    this.DepartmentsWrapper.Children.Clear();
                    this.stackingVisualization.Children.Clear();
                    this.NumberOfDepartments.Text = initialNumberOfDepartments.ToString();
                    this.colorsOfDepartments.Clear();

                    // ProjectBox Visualization
                    Point3D projectBoxCenter = new Point3D(0, 0, float.Parse(this.ProjectHeight.Text) * 0.5);
                    Material projectBoxMaterial = new SpecularMaterial(Brushes.Transparent, 1);
                    Material projectBoxInsideMaterial = MaterialHelper.CreateMaterial(Colors.Gray);
                    GeometryModel3D projectBox = VisualizationMethods.GenerateBox(projectBoxCenter,
                        new float[] { float.Parse(ProjectWidth.Text), float.Parse(ProjectLength.Text), float.Parse(ProjectHeight.Text) },
                        projectBoxMaterial, projectBoxInsideMaterial);
                    projectBox.SetName("ProjectBox");
                    this.stackingVisualization.Children.Add(projectBox);

                    filePath = openFileDialog.FileName;
                }
                else
                {
                    filePath = openFileDialog.FileName;
                }

                if (filePath.Substring(filePath.Length - 3).ToLower() != "xls" &&
                    filePath.Substring(filePath.Length - 4).ToLower() != "xlsx")
                {
                    MessageBox.Show("Please Select an Execl File.");
                    return;
                }
            }
            else
            {
                // Nothing Was Selected
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
                        String name = (String)(range.Cells[r, 1] as Excel.Range).Value2;
                        try
                        {
                            tempDictionary.Add("cost", (float)(range.Cells[r, 2] as Excel.Range).Value2);
                        }
                        catch
                        {
                            MessageBox.Show("\"Cost\" Value is not accessable for " + "\"" + name + "\"" + ", \"500\" will be considered as the cost value automatically.");
                            tempDictionary.Add("cost", 500);
                        }
                        if (name == "MEP" || name == "Circulation" || name == "BES" || name == "Building Exterior Stacking")
                        {
                            tempDictionary.Add("keyMin", 0);
                            tempDictionary.Add("keyVal", 0);
                            tempDictionary.Add("keyMax", 0);
                            tempDictionary.Add("DGSFMin", 0);
                            tempDictionary.Add("DGSFVal", 0);
                            tempDictionary.Add("DGSFMax", 0);
                        }
                        else
                        {
                            try
                            {
                                tempDictionary.Add("keyMin", (float)(range.Cells[r, 3] as Excel.Range).Value2);
                            }
                            catch
                            {
                                MessageBox.Show("\"Key Rooms Slider Minimum\" Value is not accessable for " + "\"" + name + "\"" + ", \"1\" will be considered as the cost value automatically.");
                                tempDictionary.Add("keyMin", 1);
                            }
                            try
                            {
                                tempDictionary.Add("keyVal", (float)(range.Cells[r, 4] as Excel.Range).Value2);
                            }
                            catch
                            {
                                MessageBox.Show("\"Key Rooms Slider Value\" Value is not accessable for " + "\"" + name + "\"" + ", \"5\" will be considered as the cost value automatically.");
                                tempDictionary.Add("keyVal", 5);
                            }
                            try
                            {
                                tempDictionary.Add("keyMax", (float)(range.Cells[r, 5] as Excel.Range).Value2);
                            }
                            catch
                            {
                                MessageBox.Show("\"Key Rooms Slider Maximum\" Value is not accessable for " + "\"" + name + "\"" + ", \"10\" will be considered as the cost value automatically.");
                                tempDictionary.Add("keyMax", 10);
                            }
                            try
                            {
                                tempDictionary.Add("DGSFMin", (float)(range.Cells[r, 6] as Excel.Range).Value2);
                            }
                            catch
                            {
                                MessageBox.Show("\"DGSF Slider Minimum\" Value is not accessable for " + "\"" + name + "\"" + ", \"100\" will be considered as the cost value automatically.");
                                tempDictionary.Add("DGSFMin", 100);
                            }
                            try
                            {
                                tempDictionary.Add("DGSFVal", (float)(range.Cells[r, 7] as Excel.Range).Value2);
                            }
                            catch
                            {
                                MessageBox.Show("\"DGSF Slider Value\" Value is not accessable for " + "\"" + name + "\"" + ", \"500\" will be considered as the cost value automatically.");
                                tempDictionary.Add("DGSFVal", 500);
                            }
                            try
                            {
                                tempDictionary.Add("DGSFMax", (float)(range.Cells[r, 8] as Excel.Range).Value2);
                            }
                            catch
                            {
                                MessageBox.Show("\"DGSF Slider Maximum\" Value is not accessable for " + "\"" + name + "\"" + ", \"1000\" will be considered as the cost value automatically.");
                                tempDictionary.Add("DGSFMax", 1000);
                            }
                        }

                        //Adding Data to Main Data Dictionary
                        this.functions.Add((String)(range.Cells[r, 1] as Excel.Range).Value2, tempDictionary);
                    }
                }

                // Adding Department Expanders and Programs to the Controller Window
                this.NumberOfDepartments.Text = this.initialNumberOfDepartments.ToString();

                for (int i = 0; i < this.initialNumberOfDepartments; i++)
                {
                    // Setting up Initial Departments' Expanders
                    Expander department = ExtraMethods.DepartmentGernerator(i);

                    ExtraMethods.departmentExpanderGenerator(department, initialNumberOfPrograms,
                        this.functions, new RoutedEventHandler(DepartmentNameAndNumberButton_Click),
                        new SelectionChangedEventHandler(SelectedProgram_Chenged),
                        Slider_ValueChanged);

                    this.DepartmentsWrapper.Children.Add(department);

                    /*--- Setting up Initial Departments and Programs Visualization ---*/
                    // Generating a random color in the format of an array that contains three bytes
                    byte[] color = { Convert.ToByte(random.Next(255)), Convert.ToByte(random.Next(255)), Convert.ToByte(random.Next(255)) };
                    this.colorsOfDepartments.Add(color);

                    for (int j = 0; j < initialNumberOfPrograms; j++)
                    {
                        // Calculating Length of Each Program Based on Total Area of The Program and Width of The Project Box
                        ComboBox program = LogicalTreeHelper.FindLogicalNode(department, department.Name + "ComboBox" + j.ToString()) as ComboBox;
                        Slider keyRooms = LogicalTreeHelper.FindLogicalNode(department, department.Name + "Rooms" + j.ToString()) as Slider;
                        Slider DGSF = LogicalTreeHelper.FindLogicalNode(department, department.Name + "DGSF" + j.ToString()) as Slider;
                        this.initialProgramLength = ((float)(keyRooms.Value * DGSF.Value)) / this.initialProjectBoxDims[0];

                        // Adding to Total GSF and Total Raw Cost
                        this.totalGSF += ((float)(keyRooms.Value * DGSF.Value));
                        this.totalRawDepartmentCost += ((float)(keyRooms.Value * DGSF.Value)) * this.functions[program.SelectedItem.ToString()]["cost"];

                        // Generate gradient colors for programs of each department
                        float stop = ((float)j) / ((float)initialNumberOfPrograms);
                        byte[] gradient = VisualizationMethods.GenerateGradientColor(color, stop);

                        // Setting Program Label Background Color
                        ExtraMethods.ChangeLabelColor(department, j, gradient);

                        Material programBoxMaterial = MaterialHelper.CreateMaterial(Color.FromRgb(gradient[0], gradient[1], gradient[2]));

                        float[] programBoxDims = { float.Parse(this.ProjectWidth.Text), this.initialProgramLength, this.initialProgramHeight };
                        Point3D programBoxCenter = new Point3D(0,
                            ((programBoxDims[1] * 0.5) + (j * programBoxDims[1])) - (float.Parse(ProjectLength.Text) * 0.5),
                            this.initialProgramHeight * 0.5 + (i * this.initialProgramHeight));

                        GeometryModel3D programBox = VisualizationMethods.GenerateBox(programBoxCenter, programBoxDims,
                            programBoxMaterial, programBoxMaterial);
                        programBox.SetName(department.Name + "Box" + j.ToString());

                        this.stackingVisualization.Children.Add(programBox);
                    }
                }

                xlWorkBook.Close(true, null, null);
                xlApp.Quit();

                System.Runtime.InteropServices.Marshal.ReleaseComObject(xlWorkSheet);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(xlWorkBook);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(xlApp);

                // All The Calculation, Prepration, and Visualization of The Output Data
                CalculationsAndOutputs(this.totalGSF, this.totalRawDepartmentCost);

                // Enabling the Disabled Controllers
                this.ProjectWidth.IsReadOnly = false;
                this.ProjectWidth.Background = Brushes.White;
                this.ProjectWidthButton.IsEnabled = true;
                this.Seperator.Visibility = Visibility.Visible;

                this.ProjectLength.IsReadOnly = false;
                this.ProjectLength.Background = Brushes.White;
                this.ProjectLengthButton.IsEnabled = true;

                this.ProjectHeight.IsReadOnly = false;
                this.ProjectHeight.Background = Brushes.White;
                this.ProjectHeightButton.IsEnabled = true;

                this.BGSFBox.IsEnabled = true;
                this.ProgramLabel.IsEnabled = true;

                this.FloorHeight.IsReadOnly = false;
                this.FloorHeight.Background = Brushes.White;
                this.FloorHeightButton.IsEnabled = true;

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

        /*---------------- Handeling Number of Departments Button Event ----------------*/
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

                // If user Input for Number of Departments is Larger Than Zero
                if (input > 0)
                {
                    // Decrease Number of Departments
                    if (existingDepartments > input)
                    {
                        int difference = existingDepartments - input;

                        for (int i = 0; i < difference; i++)
                        {
                            int lastIndex = this.DepartmentsWrapper.Children.Count - 1;

                            Expander department = this.DepartmentsWrapper.Children[lastIndex] as Expander;
                            TextBox programNumberTextBox = LogicalTreeHelper.FindLogicalNode(department, department.Name + "NumberInputTextBox") as TextBox;

                            int numberOfPrograms = int.Parse(programNumberTextBox.Text);

                            // Removing Departments' Properties
                            this.DepartmentsWrapper.Children.RemoveAt(lastIndex);
                            this.colorsOfDepartments.RemoveAt(lastIndex);

                            for (int j = 0; j < numberOfPrograms; j++)
                            {
                                // Calculating Raw Cost and GSF of Each Program
                                ComboBox program = LogicalTreeHelper.FindLogicalNode(department, department.Name + "ComboBox" + j.ToString()) as ComboBox;
                                Slider keyRooms = LogicalTreeHelper.FindLogicalNode(department, department.Name + "Rooms" + j.ToString()) as Slider;
                                Slider DGSF = LogicalTreeHelper.FindLogicalNode(department, department.Name + "DGSF" + j.ToString()) as Slider;

                                // Subtracting From Total GSF and Total Raw Cost
                                this.totalGSF -= ((float)(keyRooms.Value * DGSF.Value));
                                this.totalRawDepartmentCost -= ((float)(keyRooms.Value * DGSF.Value)) * this.functions[program.SelectedItem.ToString()]["cost"];

                                int lastProgramIndex = this.stackingVisualization.Children.Count - 1;
                                this.stackingVisualization.Children.RemoveAt(lastProgramIndex);
                            }
                        }

                        // All The Calculation, Prepration, and Visualization of The Output Data
                        CalculationsAndOutputs(this.totalGSF, this.totalRawDepartmentCost);
                    }

                    // Increase Number of Departments
                    if (existingDepartments < input)
                    {
                        int difference = input - existingDepartments;

                        for (int i = 0; i < difference; i++)
                        {
                            Expander department = ExtraMethods.DepartmentGernerator((existingDepartments + i));

                            ExtraMethods.departmentExpanderGenerator(department, 4, this.functions,
                                new RoutedEventHandler(DepartmentNameAndNumberButton_Click),
                                new SelectionChangedEventHandler(SelectedProgram_Chenged),
                                Slider_ValueChanged);

                            this.DepartmentsWrapper.Children.Add(department);

                            // Generating a random color in the format of an array that contains three bytes
                            byte[] color = { Convert.ToByte(random.Next(255)), Convert.ToByte(random.Next(255)), Convert.ToByte(random.Next(255)) };
                            this.colorsOfDepartments.Add(color);

                            for (int j = 0; j < initialNumberOfPrograms; j++)
                            {
                                // Calculating Raw Cost and GSF of Each Program
                                ComboBox program = LogicalTreeHelper.FindLogicalNode(department, department.Name + "ComboBox" + j.ToString()) as ComboBox;
                                Slider keyRooms = LogicalTreeHelper.FindLogicalNode(department, department.Name + "Rooms" + j.ToString()) as Slider;
                                Slider DGSF = LogicalTreeHelper.FindLogicalNode(department, department.Name + "DGSF" + j.ToString()) as Slider;
                                this.initialProgramLength = ((float)(keyRooms.Value * DGSF.Value)) / float.Parse(this.ProjectWidth.Text);

                                // Adding To Total GSF and Total Raw Cost
                                this.totalGSF += ((float)(keyRooms.Value * DGSF.Value));
                                this.totalRawDepartmentCost += ((float)(keyRooms.Value * DGSF.Value)) * this.functions[program.SelectedItem.ToString()]["cost"];

                                // Add Program's Boxes for the added Departments
                                float stop = ((float)j) / ((float)initialNumberOfPrograms);
                                byte[] gradient = VisualizationMethods.GenerateGradientColor(color, stop);

                                // Setting Program Label Background Color
                                ExtraMethods.ChangeLabelColor(department, j, gradient);

                                Material programBoxMaterial = MaterialHelper.CreateMaterial(Color.FromRgb(gradient[0], gradient[1], gradient[2]));

                                float[] programBoxDims = { float.Parse(this.ProjectWidth.Text), this.initialProgramLength, float.Parse(this.FloorHeight.Text) };
                                Point3D programBoxCenter = new Point3D(0,
                                    ((programBoxDims[1] * 0.5) + (j * programBoxDims[1])) - (this.initialProjectBoxDims[1] * 0.5),
                                    float.Parse(this.FloorHeight.Text) * 0.5 + ((i + (int.Parse(this.NumberOfDepartments.Text) - difference)) * float.Parse(this.FloorHeight.Text)));

                                GeometryModel3D programBox = VisualizationMethods.GenerateBox(programBoxCenter, programBoxDims,
                                    programBoxMaterial, programBoxMaterial);
                                programBox.SetName(department.Name + "Box" + j.ToString());

                                this.stackingVisualization.Children.Insert(this.stackingVisualization.Children.Count, programBox);
                            }
                        }

                        // All The Calculation, Prepration, and Visualization of The Output Data
                        CalculationsAndOutputs(this.totalGSF, this.totalRawDepartmentCost);
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

        /*---------------- Handeling Reset Departments button ----------------*/
        private void ResetDepartments_Click(object sender, RoutedEventArgs e)
        {
            // Clear all the lists
            this.DepartmentsWrapper.Children.Clear();
            this.stackingVisualization.Children.Clear();
            this.NumberOfDepartments.Text = initialNumberOfDepartments.ToString();
            this.colorsOfDepartments.Clear();

            // Output Variables
            this.constructionCost = 0;
            this.projectCost = 0;
            this.budgetDifference = 0;
            this.costPerGSF = 0;
            this.totalBGSF = 0;
            this.limitOfBGSF = 0;

            // Temp Output Variables
            this.totalGSF = 0;
            this.totalRawDepartmentCost = 0;

            // Setting up values of the initial dimensions of the Project Box
            this.ProjectWidth.Text = initialProjectBoxDims[0].ToString();
            this.ProjectLength.Text = initialProjectBoxDims[1].ToString();
            this.ProjectHeight.Text = initialProjectBoxDims[2].ToString();
            this.FloorHeight.Text = initialProgramHeight.ToString();

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
                ExtraMethods.departmentExpanderGenerator(department, 4, this.functions,
                    new RoutedEventHandler(DepartmentNameAndNumberButton_Click),
                    new SelectionChangedEventHandler(SelectedProgram_Chenged),
                    Slider_ValueChanged);

                this.DepartmentsWrapper.Children.Add(department);

                /*---------------- Setting up initial Departments and Programs visualization ----------------*/
                // Generating a random color in the format of an array that contains three bytes
                byte[] color = { Convert.ToByte(random.Next(255)), Convert.ToByte(random.Next(255)), Convert.ToByte(random.Next(255)) };
                this.colorsOfDepartments.Add(color);

                for (int j = 0; j < initialNumberOfPrograms; j++)
                {
                    // Calculating Raw Cost and GSF of Each Program
                    ComboBox program = LogicalTreeHelper.FindLogicalNode(department, department.Name + "ComboBox" + j.ToString()) as ComboBox;
                    Slider keyRooms = LogicalTreeHelper.FindLogicalNode(department, department.Name + "Rooms" + j.ToString()) as Slider;
                    Slider DGSF = LogicalTreeHelper.FindLogicalNode(department, department.Name + "DGSF" + j.ToString()) as Slider;
                    this.initialProgramLength = ((float)(keyRooms.Value * DGSF.Value)) / this.initialProjectBoxDims[0];

                    // Adding To Total GSF and Total Raw Cost
                    this.totalGSF += ((float)(keyRooms.Value * DGSF.Value));
                    this.totalRawDepartmentCost += ((float)(keyRooms.Value * DGSF.Value)) * this.functions[program.SelectedItem.ToString()]["cost"];

                    // Generate gradient colors for programs of each department
                    float stop = ((float)j) / ((float)initialNumberOfPrograms);
                    byte[] gradient = VisualizationMethods.GenerateGradientColor(color, stop);

                    // Setting Program Label Background Color
                    ExtraMethods.ChangeLabelColor(department, j, gradient);

                    Material programBoxMaterial = MaterialHelper.CreateMaterial(Color.FromRgb(gradient[0], gradient[1], gradient[2]));

                    float[] programBoxDims = { float.Parse(this.ProjectWidth.Text), this.initialProgramLength, float.Parse(this.FloorHeight.Text) };
                    Point3D programBoxCenter = new Point3D(0,
                        ((programBoxDims[1] * 0.5) + (j * programBoxDims[1])) - (float.Parse(ProjectLength.Text) * 0.5),
                        float.Parse(this.FloorHeight.Text) * 0.5 + (i * float.Parse(this.FloorHeight.Text)));

                    GeometryModel3D programBox = VisualizationMethods.GenerateBox(programBoxCenter, programBoxDims,
                        programBoxMaterial, programBoxMaterial);
                    programBox.SetName(department.Name + "Box" + j.ToString());

                    this.stackingVisualization.Children.Add(programBox);
                }
            }

            // All The Calculation, Prepration, and Visualization of The Output Data
            CalculationsAndOutputs(this.totalGSF, this.totalRawDepartmentCost);
        }

        /* ----------------The Event for Setting Name of the Departments and the Number of Programs it contains ---------------- */
        private void DepartmentNameAndNumberButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;

            // Setting the Name of the Department (recognizing which button was pressed)
            if (btn.Name.Contains("SetNameButton"))
            {
                Expander department = LogicalTreeHelper.FindLogicalNode(this.DepartmentsWrapper, btn.Name.Replace("SetNameButton", "")) as Expander;
                TextBox nameTextBox = LogicalTreeHelper.FindLogicalNode(this.DepartmentsWrapper, btn.Name.Replace("SetNameButton", "NameInputTextBox")) as TextBox;

                if (nameTextBox.Text != "")
                {
                    department.Header = nameTextBox.Text; ;
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
                Expander department = LogicalTreeHelper.FindLogicalNode(this.DepartmentsWrapper, btn.Name.Replace("SetNumberButton", "")) as Expander;
                TextBox numberTextBox = LogicalTreeHelper.FindLogicalNode(this.DepartmentsWrapper, btn.Name.Replace("SetNumberButton", "NumberInputTextBox")) as TextBox;
                Grid programs = LogicalTreeHelper.FindLogicalNode(this.DepartmentsWrapper, btn.Name.Replace("SetNumberButton", "") + "Programs") as Grid;
                int departmentIndex = this.DepartmentsWrapper.Children.IndexOf(department);

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
                        int lastProgramBoxIndex = 1;
                        int firstProgramBoxIndex = 1;
                        for (int i = 0; i < departmentIndex + 1; i++)
                        {
                            Expander tempDepartment = this.DepartmentsWrapper.Children[i] as Expander;
                            StackPanel expanderContent = tempDepartment.Content as StackPanel;
                            Grid programsGrid = expanderContent.Children[2] as Grid;
                            lastProgramBoxIndex += programsGrid.RowDefinitions.Count;

                            if (i < departmentIndex)
                            {
                                firstProgramBoxIndex += programsGrid.RowDefinitions.Count;
                            }
                        }

                        int difference = input - existingPrograms;
                        ExtraMethods.AddProgram(programs, difference, existingPrograms, department, this.functions,
                            new SelectionChangedEventHandler(SelectedProgram_Chenged), Slider_ValueChanged);

                        int indexOfDepartment = this.DepartmentsWrapper.Children.IndexOf(department);

                        // Calculating total length of the exsiting programs
                        float totalExistingProgramsLength = new float();

                        for (int i = firstProgramBoxIndex; i < lastProgramBoxIndex; i++)
                        {
                            totalExistingProgramsLength += (float)this.stackingVisualization.Children[i].Bounds.SizeY;
                        }

                        // Extracting Color of Department
                        byte[] color = this.colorsOfDepartments[departmentIndex];

                        for (int i = 0; i < input; i++)
                        {
                            // Generate gradient colors for programs of each department
                            float stop = ((float)i) / ((float)(input));

                            byte[] gradient = VisualizationMethods.GenerateGradientColor(color, stop);

                            // Setting Program Label Background Color
                            ExtraMethods.ChangeLabelColor(department, i, gradient);

                            Material programBoxMaterial = MaterialHelper.CreateMaterial(Color.FromRgb(gradient[0], gradient[1], gradient[2]));

                            if (i < existingPrograms)
                            {
                                ((GeometryModel3D)(this.stackingVisualization.Children[firstProgramBoxIndex + i])).Material = programBoxMaterial;
                            }
                            else
                            {
                                // Calculating Raw Cost and GSF of Each Program
                                ComboBox program = LogicalTreeHelper.FindLogicalNode(department, department.Name + "ComboBox" + i.ToString()) as ComboBox;
                                Slider keyRooms = LogicalTreeHelper.FindLogicalNode(department, department.Name + "Rooms" + i.ToString()) as Slider;
                                Slider DGSF = LogicalTreeHelper.FindLogicalNode(department, department.Name + "DGSF" + i.ToString()) as Slider;

                                // Adding to Total GSF and Total Raw Cost
                                this.totalGSF += ((float)(keyRooms.Value * DGSF.Value));
                                this.totalRawDepartmentCost += ((float)(keyRooms.Value * DGSF.Value)) * this.functions[program.SelectedItem.ToString()]["cost"];

                                // Calculating Length of Each Program Based on Width of The Project Box
                                float[] programBoxDims = { float.Parse(this.ProjectWidth.Text), this.initialProgramLength, float.Parse(this.FloorHeight.Text) };
                                Point3D programBoxCenter = new Point3D(0,
                                    ((totalExistingProgramsLength + ((i - existingPrograms) * programBoxDims[1]) + programBoxDims[1] / 2) - (float.Parse(ProjectLength.Text) * 0.5)),
                                    float.Parse(this.FloorHeight.Text) * 0.5 + (indexOfDepartment * int.Parse(this.FloorHeight.Text)));

                                GeometryModel3D programBox = VisualizationMethods.GenerateBox(programBoxCenter, programBoxDims,
                                    programBoxMaterial, programBoxMaterial);
                                programBox.SetName(department.Name + "Box" + (i).ToString());

                                this.stackingVisualization.Children.Insert(lastProgramBoxIndex, programBox);
                                lastProgramBoxIndex += 1;
                            }
                        }

                        // All The Calculation, Prepration, and Visualization of The Output Data
                        CalculationsAndOutputs(this.totalGSF, this.totalRawDepartmentCost);
                    }

                    // Decrease Number of Programs
                    if (input < existingPrograms)
                    {
                        int lastProgramBoxIndex = 0;
                        int firstProgramBoxIndex = 1;
                        for (int i = 0; i < departmentIndex + 1; i++)
                        {
                            Expander tempDepartment = this.DepartmentsWrapper.Children[i] as Expander;
                            StackPanel expanderContent = tempDepartment.Content as StackPanel;
                            Grid programsGrid = expanderContent.Children[2] as Grid;
                            lastProgramBoxIndex += programsGrid.RowDefinitions.Count;
                            if (i < departmentIndex)
                            {
                                firstProgramBoxIndex += programsGrid.RowDefinitions.Count;
                            }
                        }

                        // Extracting Color of Department
                        byte[] color = this.colorsOfDepartments[departmentIndex];

                        int difference = existingPrograms - input;
                        List<UIElement> elementsToRemove = new List<UIElement>();

                        for (int i = 0; i < existingPrograms; i++)
                        {
                            // Change colors of the remaining programs
                            if (i < input)
                            {
                                // Generate gradient colors for programs of each department
                                float stop = ((float)i) / ((float)(existingPrograms - difference));

                                byte[] gradient = VisualizationMethods.GenerateGradientColor(color, stop);

                                // Setting Program Label Background Color
                                ExtraMethods.ChangeLabelColor(department, i, gradient);

                                Material programBoxMaterial = MaterialHelper.CreateMaterial(Color.FromRgb(gradient[0], gradient[1], gradient[2]));

                                ((GeometryModel3D)(this.stackingVisualization.Children[firstProgramBoxIndex + i])).Material = programBoxMaterial;

                            }
                            // Omit programs' properties and visualizations
                            else
                            {
                                // Calculating Raw Cost and GSF of Each Program
                                ComboBox program = LogicalTreeHelper.FindLogicalNode(department, department.Name + "ComboBox" + (programs.RowDefinitions.Count - 1).ToString()) as ComboBox;
                                Slider keyRooms = LogicalTreeHelper.FindLogicalNode(department, department.Name + "Rooms" + (programs.RowDefinitions.Count - 1).ToString()) as Slider;
                                Slider DGSF = LogicalTreeHelper.FindLogicalNode(department, department.Name + "DGSF" + (programs.RowDefinitions.Count - 1).ToString()) as Slider;

                                // Subtracting From Total GSF and Total Raw Cost
                                this.totalGSF -= ((float)(keyRooms.Value * DGSF.Value));
                                this.totalRawDepartmentCost -= ((float)(keyRooms.Value * DGSF.Value)) * this.functions[program.SelectedItem.ToString()]["cost"];

                                // Removing UI Elemets From The Controller Window
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

                        // All The Calculation, Prepration, and Visualization of The Output Data
                        CalculationsAndOutputs(this.totalGSF, this.totalRawDepartmentCost);
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

                    double TotalDepartmentLength = this.initialProjectBoxDims[1] * -0.5;

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
                            int departmentIndex = int.Parse(this.stackingVisualization.Children[i].GetName()[1].ToString()) - 1;
                            int ProgramIndex = int.Parse(this.stackingVisualization.Children[i].GetName()[this.stackingVisualization.Children[i].GetName().Length - 1].ToString());

                            if (ProgramIndex == 0)
                            {
                                double newLength = (this.stackingVisualization.Children[i].Bounds.SizeY * this.stackingVisualization.Children[i].Bounds.SizeX) / projectWidthInput;

                                float[] programBoxDims = {(float) this.stackingVisualization.Children[0].Bounds.SizeX, (float) newLength,
                                    (float) this.stackingVisualization.Children[i].Bounds.SizeZ };

                                double newProgramCenterY = (this.initialProjectBoxDims[1] * -0.5) + (newLength / 2);

                                Point3D programBoxCenter = new Point3D(0, newProgramCenterY,
                                    float.Parse(this.FloorHeight.Text) * 0.5 + (departmentIndex * int.Parse(this.FloorHeight.Text)));

                                GeometryModel3D programBox = VisualizationMethods.GenerateBox(programBoxCenter, programBoxDims,
                                    ((GeometryModel3D)this.stackingVisualization.Children[i]).Material,
                                    ((GeometryModel3D)this.stackingVisualization.Children[i]).Material);

                                programBox.SetName(this.stackingVisualization.Children[i].GetName());

                                this.stackingVisualization.Children.RemoveAt(i);
                                this.stackingVisualization.Children.Insert(i, programBox);

                                TotalDepartmentLength = (this.initialProjectBoxDims[1] * -0.5) + this.stackingVisualization.Children[i].Bounds.SizeY;
                            }
                            else
                            {
                                double newLength = (this.stackingVisualization.Children[i].Bounds.SizeY * this.stackingVisualization.Children[i].Bounds.SizeX) / projectWidthInput;

                                float[] programBoxDims = {(float) this.stackingVisualization.Children[0].Bounds.SizeX, (float) newLength,
                                    (float) this.stackingVisualization.Children[i].Bounds.SizeZ };

                                double newProgramCenterY = TotalDepartmentLength + (programBoxDims[1] / 2);

                                Point3D programBoxCenter = new Point3D(0, newProgramCenterY,
                                    float.Parse(this.FloorHeight.Text) * 0.5 + (departmentIndex * int.Parse(this.FloorHeight.Text)));

                                GeometryModel3D programBox = VisualizationMethods.GenerateBox(programBoxCenter, programBoxDims,
                                    ((GeometryModel3D)this.stackingVisualization.Children[i]).Material,
                                    ((GeometryModel3D)this.stackingVisualization.Children[i]).Material);

                                programBox.SetName(this.stackingVisualization.Children[i].GetName());

                                this.stackingVisualization.Children.RemoveAt(i);
                                this.stackingVisualization.Children.Insert(i, programBox);

                                TotalDepartmentLength += this.stackingVisualization.Children[i].Bounds.SizeY;
                            }
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
                    this.FloorHeight.Text = this.stackingVisualization.Children[0].Bounds.SizeZ.ToString();
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

            // Handeling Program Height changes events
            if (btn.Name == "FloorHeightButton")
            {
                float floorHeightInput = 0;
                try
                {
                    floorHeightInput = float.Parse(this.FloorHeight.Text);
                }
                catch
                {
                    MessageBox.Show("Please Enter a Number.");
                    this.FloorHeight.Text = this.stackingVisualization.Children[1].Bounds.SizeZ.ToString();
                    return;
                }
                if (floorHeightInput > 0)
                {

                    double scaleCenterY = this.initialProjectBoxDims[1] * -0.5;

                    for (int i = 1; i < this.stackingVisualization.Children.Count; i++)
                    {
                        int departmentIndex = int.Parse(this.stackingVisualization.Children[i].GetName()[1].ToString()) - 1;
                        int programIndex = int.Parse(this.stackingVisualization.Children[i].GetName()[this.stackingVisualization.Children[i].GetName().Length - 1].ToString());

                        if (programIndex == 0)
                        {
                            scaleCenterY = (this.initialProjectBoxDims[1] * -0.5) + (this.stackingVisualization.Children[i].Bounds.SizeY / 2);

                            float[] programBoxDims = {(float) this.stackingVisualization.Children[0].Bounds.SizeX, (float) this.stackingVisualization.Children[i].Bounds.SizeY,
                                    floorHeightInput };

                            Point3D programBoxCenter = new Point3D(0, scaleCenterY,
                                floorHeightInput * 0.5 + (departmentIndex * floorHeightInput));

                            GeometryModel3D programBox = VisualizationMethods.GenerateBox(programBoxCenter, programBoxDims,
                                ((GeometryModel3D)this.stackingVisualization.Children[i]).Material,
                                ((GeometryModel3D)this.stackingVisualization.Children[i]).Material);

                            programBox.SetName(this.stackingVisualization.Children[i].GetName());

                            this.stackingVisualization.Children.RemoveAt(i);
                            this.stackingVisualization.Children.Insert(i, programBox);

                            scaleCenterY += this.stackingVisualization.Children[i].Bounds.SizeY / 2;
                        }
                        else
                        {
                            scaleCenterY += this.stackingVisualization.Children[i].Bounds.SizeY / 2;

                            float[] programBoxDims = {(float) this.stackingVisualization.Children[0].Bounds.SizeX, (float) this.stackingVisualization.Children[i].Bounds.SizeY,
                                    floorHeightInput };

                            Point3D programBoxCenter = new Point3D(0, scaleCenterY,
                                floorHeightInput * 0.5 + (departmentIndex * floorHeightInput));

                            GeometryModel3D programBox = VisualizationMethods.GenerateBox(programBoxCenter, programBoxDims,
                                ((GeometryModel3D)this.stackingVisualization.Children[i]).Material,
                                ((GeometryModel3D)this.stackingVisualization.Children[i]).Material);

                            programBox.SetName(this.stackingVisualization.Children[i].GetName());

                            this.stackingVisualization.Children.RemoveAt(i);
                            this.stackingVisualization.Children.Insert(i, programBox);

                            scaleCenterY += this.stackingVisualization.Children[i].Bounds.SizeY / 2;
                        }

                    }
                }
                else
                {
                    MessageBox.Show("Please Enter a Number Larger than Zero.");
                    this.ProjectHeight.Text = this.stackingVisualization.Children[1].Bounds.SizeZ.ToString();
                    return;
                }
            }

            // BGSF Limit on Output Window Changes
            this.limitOfBGSF = (float.Parse(this.ProjectWidth.Text) * float.Parse(this.ProjectLength.Text)) *
                (float.Parse(this.ProjectHeight.Text) / float.Parse(this.FloorHeight.Text));
            this.BGSFLimit.Text = this.limitOfBGSF.ToString("C0", System.Globalization.CultureInfo.CurrentCulture).Remove(0, 1);

            if (this.totalBGSF < this.limitOfBGSF)
            {
                this.TotalBGSF.Foreground = this.TotalBGSFLabel.Foreground;
                this.TotalBGSF.Text = this.totalBGSF.ToString("C0", System.Globalization.CultureInfo.CurrentCulture).Remove(0, 1);
            }
            else
            {
                this.TotalBGSF.Foreground = Brushes.Red;
                this.TotalBGSF.Text = this.totalBGSF.ToString("C0", System.Globalization.CultureInfo.CurrentCulture).Remove(0, 1);
            }
        }

        /*---------------- Program ComboBox Change Event Handler ----------------*/
        void SelectedProgram_Chenged(object sender, EventArgs e)
        {
            ComboBox cbx = sender as ComboBox;

            // Extracting Department and Program Indices of The Changed ComboBox
            int departmentIndex = int.Parse(cbx.Name[1].ToString()) - 1;
            int programIndex = int.Parse(cbx.Name[cbx.Name.Length - 1].ToString());

            // Extracting the Department That Changed
            Expander expander = this.DepartmentsWrapper.Children[departmentIndex] as Expander;

            // Extracting the Sliders that Need Changes
            String keyRoomsSliderName = cbx.Name.Substring(0, 2) + "Rooms" + programIndex.ToString();
            String DGSFSliderName = cbx.Name.Substring(0, 2) + "DGSF" + programIndex.ToString();

            // Calculating length of each program based on total area of the program and width of the Project Box
            Slider keyRooms = LogicalTreeHelper.FindLogicalNode(expander, keyRoomsSliderName) as Slider;
            keyRooms.Minimum = this.functions[cbx.SelectedItem.ToString()]["keyMin"];
            keyRooms.Value = this.functions[cbx.SelectedItem.ToString()]["keyVal"]; ;
            keyRooms.Maximum = this.functions[cbx.SelectedItem.ToString()]["keyMax"]; ;

            Slider DGSF = LogicalTreeHelper.FindLogicalNode(expander, DGSFSliderName) as Slider;
            DGSF.Minimum = this.functions[cbx.SelectedItem.ToString()]["DGSFMin"];
            DGSF.Value = this.functions[cbx.SelectedItem.ToString()]["DGSFVal"];
            DGSF.Maximum = this.functions[cbx.SelectedItem.ToString()]["DGSFMax"];

            // Calculating indices of First and Last ProgramBox in Each department
            int firstProgramBoxIndex = 1;
            int lastProgramBoxIndex = 0;
            for (int i = 0; i < departmentIndex + 1; i++)
            {
                Expander tempExpander = this.DepartmentsWrapper.Children[i] as Expander;
                StackPanel expanderContent = tempExpander.Content as StackPanel;
                Grid programsGrid = expanderContent.Children[2] as Grid;
                lastProgramBoxIndex += programsGrid.RowDefinitions.Count;
                if (i < departmentIndex)
                {
                    firstProgramBoxIndex += programsGrid.RowDefinitions.Count;
                }
            }

            // Calculating Index of The ProgramBox
            int programBoxIndex = firstProgramBoxIndex + programIndex;

            // Calculating the Scale Factor of Each ProgramBox
            float newProgramLength = (((float)(keyRooms.Value * DGSF.Value)) / float.Parse(this.ProjectWidth.Text));

            // Length of the Program Before Scale
            double totalProgramLength = 0;

            // Calculating Y Cordinate of the Scale Center for Each ProgramBox
            double newProgramCenterY = (this.initialProjectBoxDims[1] * -0.5f);

            for (int i = firstProgramBoxIndex; i <= lastProgramBoxIndex; i++)
            {
                if (i < programBoxIndex)
                {
                    newProgramCenterY += this.stackingVisualization.Children[i].Bounds.SizeY;
                }
                if (i == programBoxIndex)
                {
                    newProgramCenterY += (newProgramLength / 2);

                    float[] programBoxDims = { (float)this.stackingVisualization.Children[0].Bounds.SizeX,
                            newProgramLength, (float)this.stackingVisualization.Children[i].Bounds.SizeZ };

                    Point3D programBoxCenter = new Point3D(0,
                        newProgramCenterY, float.Parse(this.FloorHeight.Text) * 0.5 + (departmentIndex * int.Parse(this.FloorHeight.Text)));

                    GeometryModel3D programBox = VisualizationMethods.GenerateBox(programBoxCenter, programBoxDims,
                        ((GeometryModel3D)this.stackingVisualization.Children[i]).Material,
                        ((GeometryModel3D)this.stackingVisualization.Children[i]).Material);

                    programBox.SetName(expander.Name + "Box" + (i - firstProgramBoxIndex).ToString());

                    totalProgramLength += this.stackingVisualization.Children[i].Bounds.SizeY;

                    this.stackingVisualization.Children.RemoveAt(i);
                    this.stackingVisualization.Children.Insert(i, programBox);

                    newProgramCenterY += (newProgramLength / 2);

                }
                if (i > programBoxIndex)
                {
                    newProgramCenterY += this.stackingVisualization.Children[i].Bounds.SizeY / 2;

                    float[] programBoxDims = { (float)this.stackingVisualization.Children[0].Bounds.SizeX,
                            (float)this.stackingVisualization.Children[i].Bounds.SizeY,
                            (float)this.stackingVisualization.Children[i].Bounds.SizeZ };
                    // MessageBox.Show(scaleCenterY.ToString());
                    Point3D programBoxCenter = new Point3D(0,
                        newProgramCenterY, float.Parse(this.FloorHeight.Text) * 0.5 + (departmentIndex * int.Parse(this.FloorHeight.Text)));

                    GeometryModel3D programBox = VisualizationMethods.GenerateBox(programBoxCenter, programBoxDims,
                        ((GeometryModel3D)this.stackingVisualization.Children[i]).Material,
                        ((GeometryModel3D)this.stackingVisualization.Children[i]).Material);

                    programBox.SetName(expander.Name + "Box" + (i - firstProgramBoxIndex).ToString());

                    totalProgramLength += this.stackingVisualization.Children[i].Bounds.SizeY;

                    this.stackingVisualization.Children.RemoveAt(i);
                    this.stackingVisualization.Children.Insert(i, programBox);

                    newProgramCenterY += this.stackingVisualization.Children[i].Bounds.SizeY / 2;
                }
            }
        }

        /*---------------- Program Slider Change Event Handler ----------------*/
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // ... Get Slider reference.
            var slider = sender as Slider;

            // Extracting Department and Program Indices of The Changed ComboBox
            int departmentIndex = int.Parse(slider.Name[1].ToString()) - 1;
            int programIndex = int.Parse(slider.Name[slider.Name.Length - 1].ToString());

            // Extracting the Department That Changed
            Expander expander = this.DepartmentsWrapper.Children[departmentIndex] as Expander;

            // Extracting Sliders of The Program
            Slider keyRooms = new Slider();
            Slider DGSF = new Slider();

            if (slider.Name.Contains("Rooms"))
            {
                keyRooms = LogicalTreeHelper.FindLogicalNode(expander, slider.Name) as Slider;
                DGSF = LogicalTreeHelper.FindLogicalNode(expander, slider.Name.Replace("Rooms", "DGSF")) as Slider;
            }

            if (slider.Name.Contains("DGSF"))
            {
                DGSF = LogicalTreeHelper.FindLogicalNode(expander, slider.Name) as Slider;
                keyRooms = LogicalTreeHelper.FindLogicalNode(expander, slider.Name.Replace("DGSF", "Rooms")) as Slider;
            }

            // Calculating indices of First and Last ProgramBox in Each department
            int firstProgramBoxIndex = 1;
            int lastProgramBoxIndex = 0;
            for (int i = 0; i < departmentIndex + 1; i++)
            {
                Expander tempExpander = this.DepartmentsWrapper.Children[i] as Expander;
                StackPanel expanderContent = tempExpander.Content as StackPanel;
                Grid programsGrid = expanderContent.Children[2] as Grid;
                lastProgramBoxIndex += programsGrid.RowDefinitions.Count;
                if (i < departmentIndex)
                {
                    firstProgramBoxIndex += programsGrid.RowDefinitions.Count;
                }
            }

            // Calculating Index of The ProgramBox
            int programBoxIndex = firstProgramBoxIndex + programIndex;

            // Calculating the Scale Factor of Each ProgramBox
            float newProgramLength = (((float)(keyRooms.Value * DGSF.Value)) / float.Parse(this.ProjectWidth.Text));

            // Length of the Program Before Scale
            double totalProgramLength = 0;

            // Calculating Y Cordinate of the Scale Center for Each ProgramBox
            double newProgramCenterY = (this.initialProjectBoxDims[1] * -0.5f);

            for (int i = firstProgramBoxIndex; i <= lastProgramBoxIndex; i++)
            {
                if (i < programBoxIndex)
                {
                    newProgramCenterY += this.stackingVisualization.Children[i].Bounds.SizeY;
                }
                if (i == programBoxIndex)
                {
                    newProgramCenterY += (newProgramLength / 2);

                    float[] programBoxDims = { (float)this.stackingVisualization.Children[0].Bounds.SizeX,
                            newProgramLength, (float)this.stackingVisualization.Children[i].Bounds.SizeZ };

                    Point3D programBoxCenter = new Point3D(0,
                        newProgramCenterY, float.Parse(this.FloorHeight.Text) * 0.5 + (departmentIndex * int.Parse(this.FloorHeight.Text)));

                    GeometryModel3D programBox = VisualizationMethods.GenerateBox(programBoxCenter, programBoxDims,
                        ((GeometryModel3D)this.stackingVisualization.Children[i]).Material,
                        ((GeometryModel3D)this.stackingVisualization.Children[i]).Material);

                    programBox.SetName(expander.Name + "Box" + (i - firstProgramBoxIndex).ToString());

                    totalProgramLength += this.stackingVisualization.Children[i].Bounds.SizeY;

                    this.stackingVisualization.Children.RemoveAt(i);
                    this.stackingVisualization.Children.Insert(i, programBox);

                    newProgramCenterY += (newProgramLength / 2);

                }
                if (i > programBoxIndex)
                {
                    newProgramCenterY += this.stackingVisualization.Children[i].Bounds.SizeY / 2;

                    float[] programBoxDims = { (float)this.stackingVisualization.Children[0].Bounds.SizeX,
                            (float)this.stackingVisualization.Children[i].Bounds.SizeY,
                            (float)this.stackingVisualization.Children[i].Bounds.SizeZ };
                    // MessageBox.Show(scaleCenterY.ToString());
                    Point3D programBoxCenter = new Point3D(0,
                        newProgramCenterY, float.Parse(this.FloorHeight.Text) * 0.5 + (departmentIndex * int.Parse(this.FloorHeight.Text)));

                    GeometryModel3D programBox = VisualizationMethods.GenerateBox(programBoxCenter, programBoxDims,
                        ((GeometryModel3D)this.stackingVisualization.Children[i]).Material,
                        ((GeometryModel3D)this.stackingVisualization.Children[i]).Material);

                    programBox.SetName(expander.Name + "Box" + (i - firstProgramBoxIndex).ToString());

                    totalProgramLength += this.stackingVisualization.Children[i].Bounds.SizeY;

                    this.stackingVisualization.Children.RemoveAt(i);
                    this.stackingVisualization.Children.Insert(i, programBox);

                    newProgramCenterY += this.stackingVisualization.Children[i].Bounds.SizeY / 2;
                }
            }
        }

        /* ########################################################### End of Handeling Events and Start of Calculations ########################################################### */

        // The Method For All The Calculations and Visualizations of The Data
        private void CalculationsAndOutputs(float totalGSF, float totalRawDepartmentCost)
        {
            // Other Project Costs
            float landCost = float.Parse(this.LandCost.Text);

            float generalCosts = float.Parse(this.GeneralCosts.Text);
            float designContingency = float.Parse(this.DesignContingency.Text);
            float buildContingency = float.Parse(this.BuildContingency.Text);
            float CCIP = float.Parse(this.CCIP.Text);
            float CMFee = float.Parse(this.CMFee.Text);

            // Calculating Total Construction Cost and Project Cost
            float circulationCost = (((float)this.CirculationSlider.Value) / 100) * totalGSF * this.functions["Circulation"]["cost"];

            float MEPCost = (((float)this.MEPSlider.Value) / 100) * totalGSF * this.functions["MEP"]["cost"];

            float exteriorStackCost = (((float)this.ExteriorStackSlider.Value) / 100) * totalGSF * this.functions["BES"]["cost"];

            this.constructionCost = totalRawDepartmentCost + circulationCost + MEPCost + exteriorStackCost +
                landCost + generalCosts + designContingency + buildContingency + CCIP + CMFee;

            this.projectCost = this.constructionCost * float.Parse(this.IndirectMultiplier.Text);

            // Information Outputs
            this.ConstructionCost.Text = ExtraMethods.CastDollar(this.constructionCost);
            this.ProjectCost.Text = ExtraMethods.CastDollar(this.projectCost);

            this.budgetDifference = float.Parse(this.TotalBudget.Text) - this.projectCost;

            if (budgetDifference > 0)
            {
                this.BudgetDifference.Foreground = Brushes.Green;
                this.BudgetDifference.Text = ExtraMethods.CastDollar(this.budgetDifference);
            }
            else
            {
                this.BudgetDifference.Foreground = Brushes.Red;
                this.BudgetDifference.Text = ExtraMethods.CastDollar(this.budgetDifference);
            }

            // BGSF Limit
            this.limitOfBGSF = (float.Parse(this.ProjectWidth.Text) * float.Parse(this.ProjectLength.Text)) *
                (float.Parse(this.ProjectHeight.Text) / float.Parse(this.FloorHeight.Text));
            this.BGSFLimit.Text = this.limitOfBGSF.ToString("C0", System.Globalization.CultureInfo.CurrentCulture).Remove(0, 1);

            // Calculating Total BGSF Used
            float circulationGSF = (((float)this.CirculationSlider.Value) / 100) * totalGSF;

            float MEPGSF = (((float)this.MEPSlider.Value) / 100) * (totalGSF + circulationGSF);

            float exteriorStackGSF = (((float)this.ExteriorStackSlider.Value) / 100) * (totalGSF + circulationGSF + MEPGSF);

            this.totalBGSF = totalGSF + circulationGSF + MEPGSF + exteriorStackGSF;

            if (this.totalBGSF < this.limitOfBGSF)
            {
                this.TotalBGSF.Foreground = this.TotalBGSFLabel.Foreground;
                this.TotalBGSF.Text = this.totalBGSF.ToString("C0", System.Globalization.CultureInfo.CurrentCulture).Remove(0, 1);
            }
            else
            {
                this.TotalBGSF.Foreground = Brushes.Red;
                this.TotalBGSF.Text = this.totalBGSF.ToString("C0", System.Globalization.CultureInfo.CurrentCulture).Remove(0, 1);
            }

            // Calculating and Visualizing Cost/GSF
            this.costPerGSF = this.constructionCost / this.totalBGSF;
            this.CostPerGSF.Text = this.costPerGSF.ToString("C0", System.Globalization.CultureInfo.CurrentCulture);
        }
    }
}


