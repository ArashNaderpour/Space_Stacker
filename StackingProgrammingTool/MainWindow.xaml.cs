using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;
using Excel = Microsoft.Office.Interop.Excel;

namespace StackingProgrammingTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        // Visualization Variables
        Model3DGroup stackingVisualization = new Model3DGroup();
        TextGroupVisual3D programVisualizationLabelsGroup = new TextGroupVisual3D();
        Dictionary<string, int[]> labelIndexes = new Dictionary<string, int[]>();

        // Initial Project Variables
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

        // Cost Variables
        float totalBudget = 150000000;
        float indirectMultiplier = 1;
        float landCost = 10000000;
        float generalCosts = new float();
        float designContingency = new float();
        float buildContingency = new float();
        float cCIP = new float();
        float cMFee = new float();

        // Temp Output Variables
        float totalGSF = new float();
        float totalRawDepartmentCost = new float();

        // Essential Data
        List<byte[]> colorsOfDepartments = new List<byte[]>();
        Dictionary<String, Box> boxesOfTheProject = new Dictionary<string, Box>();

        // Spread-Sheet Data
        Dictionary<String, Dictionary<String, float>> functions = new Dictionary<String, Dictionary<String, float>>();

        // SubWindows: Programs Window
        ProgramsSubWindow programsWindow = new ProgramsSubWindow();

        // Random Object
        Random random = new Random(20);

        public MainWindow()
        {
            InitializeComponent();

            // Setting Up Values of The Initial Dimensions of The Project Box and Floor Height
            this.ProjectWidth.Text = this.initialProjectBoxDims[0].ToString();
            this.ProjectLength.Text = this.initialProjectBoxDims[1].ToString();
            this.ProjectHeight.Text = this.initialProjectBoxDims[2].ToString();
            this.FloorHeight.Text = this.initialProgramHeight.ToString();

            // Setting Up Initial Values of The Project Cost Tab
            this.TotalBudget.Text = ExtraMethods.CastDollar(this.totalBudget);
            this.IndirectMultiplier.Text = this.indirectMultiplier.ToString();
            this.LandCost.Text = ExtraMethods.CastDollar(this.landCost);
            this.GeneralCosts.Text = ExtraMethods.CastDollar(this.generalCosts);
            this.DesignContingency.Text = ExtraMethods.CastDollar(this.designContingency);
            this.BuildContingency.Text = ExtraMethods.CastDollar(this.buildContingency);
            this.CCIP.Text = ExtraMethods.CastDollar(this.cCIP);
            this.CMFee.Text = ExtraMethods.CastDollar(this.cMFee);

            // ProjectBox Visualization
            string projectBoxName = "ProjectBox";
            Point3D projectBoxCenter = new Point3D(0, 0, float.Parse(this.ProjectHeight.Text) * 0.5);
            Material projectBoxMaterial = new SpecularMaterial(Brushes.Transparent, 1);
            Material projectBoxInsideMaterial = MaterialHelper.CreateMaterial(Colors.Gray);
            GeometryModel3D projectBox = VisualizationMethods.GenerateBox(projectBoxName, projectBoxCenter,
                new float[] { float.Parse(ProjectWidth.Text), float.Parse(ProjectLength.Text), float.Parse(ProjectHeight.Text) },
                projectBoxMaterial, projectBoxInsideMaterial);

            // Visualization Boxes In The Viewport3D
            this.stackingVisualization.Children.Add(projectBox);
            this.Visualization.Content = stackingVisualization;

            // Visualization Labels In The Viewport3D
            this.Visualization.Children.Add(this.programVisualizationLabelsGroup);

            // Terminating The Thread After Closing The Window
            this.Closed += (sender, e) => this.Dispatcher.InvokeShutdown();
        }

        /* ########################################################### End of Windows Load and Start of Handeling Events ########################################################### */

        /*---------------- Handeling Open Spread-Sheet File Event ----------------*/
        private void OpenSpreadSheet_Click(object sender, RoutedEventArgs e)
        {
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
                    this.functions.Clear();
                    this.DepartmentsWrapper.Children.Clear();
                    this.stackingVisualization.Children.Clear();
                    this.NumberOfDepartments.Text = initialNumberOfDepartments.ToString();
                    this.colorsOfDepartments.Clear();

                    // ProjectBox Visualization
                    string projectBoxName = "ProjectBox";
                    Point3D projectBoxCenter = new Point3D(0, 0, float.Parse(this.ProjectHeight.Text) * 0.5);
                    float[] projectBoxDims = new float[] { float.Parse(ProjectWidth.Text), float.Parse(ProjectLength.Text), float.Parse(ProjectHeight.Text) };

                    Box projectBox = new Box(projectBoxName, projectBoxCenter);

                    GeometryModel3D projectVisualizationBox = VisualizationMethods.GenerateBox(projectBoxName, projectBoxCenter, projectBoxDims,
                        new SpecularMaterial(Brushes.Transparent, 1), MaterialHelper.CreateMaterial(Colors.Gray));

                    this.boxesOfTheProject.Add(projectBox.name, projectBox);
                    this.stackingVisualization.Children.Add(projectVisualizationBox);

                    filePath = openFileDialog.FileName;
                }
                else
                {
                    filePath = openFileDialog.FileName;
                }

                if (filePath.Substring(filePath.Length - 3).ToLower() != "xls" &&
                    filePath.Substring(filePath.Length - 4).ToLower() != "xlsx")
                {
                    MessageBox.Show("Please Select An Execl File.");
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
                            MessageBox.Show("\"Cost\" Value Is Not Accessable For " + "\"" + name + "\"" + ", \"500\" Will Be Considered As The Cost Value Automatically.");
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
                                MessageBox.Show("\"Key Rooms Slider Minimum\" Value Is Not Accessable For " + "\"" + name + "\"" + ", \"1\" Will Be Considered As The Cost Value Automatically.");
                                tempDictionary.Add("keyMin", 1);
                            }
                            try
                            {
                                tempDictionary.Add("keyVal", (float)(range.Cells[r, 4] as Excel.Range).Value2);
                            }
                            catch
                            {
                                MessageBox.Show("\"Key Rooms Slider Value\" Value Is Not Accessable For " + "\"" + name + "\"" + ", \"5\" Will Be Considered As The Cost Value Automatically.");
                                tempDictionary.Add("keyVal", 5);
                            }
                            try
                            {
                                tempDictionary.Add("keyMax", (float)(range.Cells[r, 5] as Excel.Range).Value2);
                            }
                            catch
                            {
                                MessageBox.Show("\"Key Rooms Slider Maximum\" Value Is Not Accessable For " + "\"" + name + "\"" + ", \"10\" Will Be Considered As The Cost Value Automatically.");
                                tempDictionary.Add("keyMax", 10);
                            }
                            try
                            {
                                tempDictionary.Add("DGSFMin", (float)(range.Cells[r, 6] as Excel.Range).Value2);
                            }
                            catch
                            {
                                MessageBox.Show("\"DGSF Slider Minimum\" Value Is Not Accessable For " + "\"" + name + "\"" + ", \"100\" Will Be Considered As The Cost Value Automatically.");
                                tempDictionary.Add("DGSFMin", 100);
                            }
                            try
                            {
                                tempDictionary.Add("DGSFVal", (float)(range.Cells[r, 7] as Excel.Range).Value2);
                            }
                            catch
                            {
                                MessageBox.Show("\"DGSF Slider Value\" Value Is Not Accessable For " + "\"" + name + "\"" + ", \"500\" Will Be Considered As The Cost Value Automatically.");
                                tempDictionary.Add("DGSFVal", 500);
                            }
                            try
                            {
                                tempDictionary.Add("DGSFMax", (float)(range.Cells[r, 8] as Excel.Range).Value2);
                            }
                            catch
                            {
                                MessageBox.Show("\"DGSF Slider Maximum\" Value Is Not Accessable For " + "\"" + name + "\"" + ", \"1000\" Will Be Considered As The Cost Value Automatically.");
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
                        this.functions, DepartmentNameAndNumberButton_Click, SelectedProgram_Chenged, ProgramSlider_ValueChanged);

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
                        Label labelElement = LogicalTreeHelper.FindLogicalNode(department, department.Name + "Label" + j.ToString()) as Label;
                        this.initialProgramLength = ((float)(keyRooms.Value * DGSF.Value)) / this.initialProjectBoxDims[0];

                        // Adding to Total GSF and Total Raw Cost
                        float GSF = ((float)(keyRooms.Value * DGSF.Value));
                        float rawCost = GSF * this.functions[program.SelectedItem.ToString()]["cost"];
                        this.totalGSF += GSF;
                        this.totalRawDepartmentCost += rawCost;

                        // Generate gradient colors for programs of each department
                        float stop = ((float)j) / ((float)initialNumberOfPrograms);
                        byte[] gradient = VisualizationMethods.GenerateGradientColor(color, stop);

                        // Setting Program Label Background Color
                        ExtraMethods.ChangeLabelColor(department, j, gradient);

                        float[] programBoxDims = { float.Parse(this.ProjectWidth.Text), this.initialProgramLength, this.initialProgramHeight };
                        string programBoxName = department.Name + "ProgramBox" + j.ToString();
                        Point3D programBoxCenter = new Point3D(0, ((programBoxDims[1] * 0.5) + (j * programBoxDims[1])) - (float.Parse(ProjectLength.Text) * 0.5),
                            this.initialProgramHeight * 0.5 + (i * this.initialProgramHeight));
                        Material programBoxMaterial = MaterialHelper.CreateMaterial(Color.FromRgb(gradient[0], gradient[1], gradient[2]));

                        Box programBox = new Box(programBoxName, programBoxCenter);
                        programBox.boxDims = programBoxDims;
                        programBox.departmentName = department.Header.ToString();
                        programBox.boxColor = Color.FromRgb(gradient[0], gradient[1], gradient[2]);
                        programBox.function = program.SelectedItem.ToString();
                        programBox.keyRooms = (int)keyRooms.Value;
                        programBox.DGSF = (float)DGSF.Value;
                        programBox.cost = this.functions[program.SelectedItem.ToString()]["cost"];
                        programBox.boxTotalGSFValue = GSF;
                        programBox.totalRawCostValue = rawCost;
                        programBox.floor = Convert.ToInt32(Math.Floor(((float)programBox.boxCenter.Z) / programBoxDims[2]));
                        programBox.visualizationLabel = labelElement.Content.ToString();

                        GeometryModel3D programBoxVisualization = VisualizationMethods.GenerateBox(programBoxName, 
                            programBoxCenter, programBoxDims, programBoxMaterial, programBoxMaterial);
                        
                        // Visualizations Of The Labels Of The Boxes
                        VisualizationMethods.GenerateVisualizationLabel(this.programVisualizationLabelsGroup, labelElement.Content.ToString(),
                            programBoxCenter, programBoxDims, programBox.boxColor);

                        // Storing Indexes Of The Labels
                        this.labelIndexes.Add(programBoxName, new int[] { this.programVisualizationLabelsGroup.Children.Count - 1,
                            this.programVisualizationLabelsGroup.Children.Count - 2 });

                        this.boxesOfTheProject.Add(programBox.name, programBox);
                        this.stackingVisualization.Children.Add(programBoxVisualization);
                        
                        // Add Index Of The Box To The Dictionary
                        this.boxesOfTheProject[programBox.name].visualizationIndex = this.stackingVisualization.Children.IndexOf(programBoxVisualization);
                    }
                }

                xlWorkBook.Close(true, null, null);
                xlApp.Quit();
               
                System.Runtime.InteropServices.Marshal.ReleaseComObject(xlWorkSheet);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(xlWorkBook);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(xlApp);

                // All The Calculation, Prepration, and Visualization of The Output Data
                CalculationsAndOutputs(this.totalGSF, this.totalRawDepartmentCost);

                // Generate And Visualize Stacking Data Of The Stacking Tab
                ExtraMethods.GenerateProgramsStacking(this.boxesOfTheProject, 
                    this.stackingVisualization, this.ProgramsStackingGrid, StackingButton_Click);

                // Enabling the Disabled Controllers
                this.ProjectWidth.IsEnabled = true;
                this.ProjectWidthButton.IsEnabled = true;
                this.Seperator.Visibility = Visibility.Visible;

                this.ProjectLength.IsEnabled = true;
                this.ProjectLengthButton.IsEnabled = true;

                this.ProjectHeight.IsEnabled = true;
                this.ProjectHeightButton.IsEnabled = true;

                this.BGSFBox.IsEnabled = true;
                this.ProgramLabel.IsEnabled = true;

                this.FloorHeight.IsEnabled = true;
                this.FloorHeightButton.IsEnabled = true;

                this.NumberOfDepartments.IsEnabled = true;

                this.NumberOfDepartmentsButton.IsEnabled = true;
                this.ResetDepartmentsButton.IsEnabled = true;

                this.TotalBudget.IsEnabled = true;
                this.TotalBudgetButton.IsEnabled = true;

                this.CirculationSlider.IsEnabled = true;
                this.MEPSlider.IsEnabled = true;
                this.ExteriorStackSlider.IsEnabled = true;

                this.IndirectMultiplier.IsEnabled = true;
                this.IndirectMultiplierButton.IsEnabled = true;

                this.LandCost.IsEnabled = true;
                this.LandCostButton.IsEnabled = true;

                this.GeneralCosts.IsEnabled = true;
                this.GeneralCostsButton.IsEnabled = true;

                this.DesignContingency.IsEnabled = true;
                this.DesignContingencyButton.IsEnabled = true;

                this.BuildContingency.IsEnabled = true;
                this.BuildContingencyButton.IsEnabled = true;

                this.CCIP.IsEnabled = true;
                this.CCIPButton.IsEnabled = true;

                this.CMFee.IsEnabled = true;
                this.CMFeeButton.IsEnabled = true;

                this.ProgramsCheckBox.IsEnabled = true;
            }
            else
            {
                MessageBox.Show("Format Of The Data In The Excel File Is Inappropriate.");
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
                    MessageBox.Show("Please Enter A Number.");
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

                            for (int j = this.stackingVisualization.Children.Count - 1; j > 0 ; j--)
                            {
                                // Name of The Program Box
                                string programBoxName = this.stackingVisualization.Children[j].GetName();
                               
                                // ProgramBoxes To Remove
                                if (programBoxName.Contains(department.Name)) {

                                    int programFloor = this.boxesOfTheProject[programBoxName].floor;

                                    // Move Programs Of The Other Departments That Exists In The Removed Department's Floor
                                    for(int k = 1; k < this.stackingVisualization.Children.Count; k++)
                                    {
                                        if (this.boxesOfTheProject[this.stackingVisualization.Children[k].GetName()].floor == programFloor && j < k && 
                                            department.Name != this.stackingVisualization.Children[k].GetName().Replace("ProgramBo", "").Split('x')[0])
                                        {
                                            string newProgramBoxName = this.stackingVisualization.Children[k].GetName();

                                            float[] newProgramBoxDims = { (float)this.stackingVisualization.Children[0].Bounds.SizeX,
                                                (float)this.stackingVisualization.Children[k].Bounds.SizeY,
                                                (float)this.stackingVisualization.Children[k].Bounds.SizeZ };
                                           
                                            Point3D newProgramBoxCenter = new Point3D(0, this.boxesOfTheProject[newProgramBoxName].boxCenter.Y - 
                                                this.stackingVisualization.Children[j].Bounds.SizeY,
                                                this.boxesOfTheProject[newProgramBoxName].boxCenter.Z);

                                            GeometryModel3D programBoxVisualization = VisualizationMethods.GenerateBox(newProgramBoxName, newProgramBoxCenter, newProgramBoxDims,
                                                ((GeometryModel3D)this.stackingVisualization.Children[k]).Material,
                                                ((GeometryModel3D)this.stackingVisualization.Children[k]).Material);

                                            // Visualizations Of The Labels Of The Boxes
                                            VisualizationMethods.ReplaceVisualizationLabel(this.programVisualizationLabelsGroup, this.labelIndexes[newProgramBoxName], this.boxesOfTheProject[newProgramBoxName].visualizationLabel,
                                                newProgramBoxCenter, newProgramBoxDims, this.boxesOfTheProject[newProgramBoxName].boxColor);

                                            this.stackingVisualization.Children.RemoveAt(k);
                                            this.stackingVisualization.Children.Insert(k, programBoxVisualization);
                                            this.boxesOfTheProject[newProgramBoxName].boxCenter = newProgramBoxCenter;
                                            this.boxesOfTheProject[newProgramBoxName].boxDims = newProgramBoxDims;

                                            // Add Index Of The Box To The Dictionary
                                            this.boxesOfTheProject[newProgramBoxName].visualizationIndex = this.stackingVisualization.Children.IndexOf(programBoxVisualization);
                                        }
                                    }

                                    // Calculating Raw Cost and GSF of Each Program
                                    ComboBox program = LogicalTreeHelper.FindLogicalNode(department, programBoxName.Replace("ProgramBox", "ComboBox")) as ComboBox;
                                    Slider keyRooms = LogicalTreeHelper.FindLogicalNode(department, programBoxName.Replace("ProgramBox", "Rooms")) as Slider;
                                    Slider DGSF = LogicalTreeHelper.FindLogicalNode(department, programBoxName.Replace("ProgramBox", "DGSF")) as Slider;
                                    
                                    // Subtracting From Total GSF and Total Raw Cost
                                    this.totalGSF -= ((float)(keyRooms.Value * DGSF.Value));
                                    this.totalRawDepartmentCost -= ((float)(keyRooms.Value * DGSF.Value)) * this.functions[program.SelectedItem.ToString()]["cost"];

                                    // Remove The Visualization Boxes
                                    this.stackingVisualization.Children.RemoveAt(j);

                                    // Remove The Removed Box From The Dictionary of The Boxes
                                    this.boxesOfTheProject.Remove(programBoxName);

                                    // Remove Visualization Labels
                                    foreach (int index in this.labelIndexes[programBoxName]) {
                                        this.programVisualizationLabelsGroup.Children.RemoveAt(index);
                                    }
                                    // Remove Labels From The Dictionary
                                    this.labelIndexes.Remove(programBoxName);
                                }
                            }

                            // Removing Departments' Expanders' Properties
                            this.DepartmentsWrapper.Children.RemoveAt(lastIndex);
                            this.colorsOfDepartments.RemoveAt(lastIndex);
                        }

                        // All The Calculation, Prepration, and Visualization of The Output Data
                        CalculationsAndOutputs(this.totalGSF, this.totalRawDepartmentCost);

                        // Omit Stacking Data From The Stacking Tab
                        ExtraMethods.GenerateProgramsStacking(this.boxesOfTheProject, this.stackingVisualization, this.ProgramsStackingGrid, StackingButton_Click);
                    }

                    // Increase Number of Departments
                    if (existingDepartments < input)
                    {
                        int difference = input - existingDepartments;

                        for (int i = 0; i < difference; i++)
                        {
                            Expander department = ExtraMethods.DepartmentGernerator((existingDepartments + i));

                            ExtraMethods.departmentExpanderGenerator(department, 4, this.functions,
                                DepartmentNameAndNumberButton_Click, SelectedProgram_Chenged, ProgramSlider_ValueChanged);

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
                                Label labelElement = LogicalTreeHelper.FindLogicalNode(department, department.Name + "Label" + j.ToString()) as Label;
                                this.initialProgramLength = ((float)(keyRooms.Value * DGSF.Value)) / float.Parse(this.ProjectWidth.Text);

                                // Adding To Total GSF and Total Raw Cost
                                float GSF = ((float)(keyRooms.Value * DGSF.Value));
                                float rawCost = GSF * this.functions[program.SelectedItem.ToString()]["cost"];
                                this.totalGSF += GSF;
                                this.totalRawDepartmentCost += rawCost;

                                // Add Program's Boxes for the added Departments
                                float stop = ((float)j) / ((float)initialNumberOfPrograms);
                                byte[] gradient = VisualizationMethods.GenerateGradientColor(color, stop);

                                // Setting Program Label Background Color
                                ExtraMethods.ChangeLabelColor(department, j, gradient);

                                string programBoxName = department.Name + "ProgramBox" + j.ToString();
                                float[] programBoxDims = { float.Parse(this.ProjectWidth.Text), this.initialProgramLength, float.Parse(this.FloorHeight.Text) };
                                Point3D programBoxCenter = new Point3D(0,
                                    ((programBoxDims[1] * 0.5) + (j * programBoxDims[1])) - (this.initialProjectBoxDims[1] * 0.5),
                                    float.Parse(this.FloorHeight.Text) * 0.5 + ((i + (int.Parse(this.NumberOfDepartments.Text) - difference)) * float.Parse(this.FloorHeight.Text)));
                                Material programBoxMaterial = MaterialHelper.CreateMaterial(Color.FromRgb(gradient[0], gradient[1], gradient[2]));

                                Box programBox = new Box(programBoxName, programBoxCenter);
                                programBox.boxDims = programBoxDims;
                                programBox.departmentName = department.Header.ToString();
                                programBox.boxColor = Color.FromRgb(gradient[0], gradient[1], gradient[2]);
                                programBox.function = program.SelectedItem.ToString();
                                programBox.keyRooms = (int)keyRooms.Value;
                                programBox.DGSF = (float)DGSF.Value;
                                programBox.cost = this.functions[program.SelectedItem.ToString()]["cost"];
                                programBox.boxTotalGSFValue = GSF;
                                programBox.totalRawCostValue = rawCost;
                                programBox.floor = Convert.ToInt32(Math.Floor(((float)programBox.boxCenter.Z) / programBoxDims[2]));
                                programBox.visualizationLabel = labelElement.Content.ToString();

                                GeometryModel3D programBoxVisualization = VisualizationMethods.GenerateBox(programBoxName, programBoxCenter, programBoxDims,
                                    programBoxMaterial, programBoxMaterial);

                                // Visualizations Of The Labels Of The Boxes
                                VisualizationMethods.GenerateVisualizationLabel(this.programVisualizationLabelsGroup, labelElement.Content.ToString(),
                                    programBoxCenter, programBoxDims, programBox.boxColor);

                                // Storing Indexes Of The Labels
                                this.labelIndexes.Add(programBoxName, new int[] { this.programVisualizationLabelsGroup.Children.Count - 1,
                                    this.programVisualizationLabelsGroup.Children.Count - 2 });

                                this.boxesOfTheProject.Add(programBox.name, programBox);
                                this.stackingVisualization.Children.Add(programBoxVisualization);

                                // Add Index Of The Box To The Dictionary
                                this.boxesOfTheProject[programBox.name].visualizationIndex = this.stackingVisualization.Children.IndexOf(programBoxVisualization);
                            }
                        }

                        // All The Calculation, Prepration, and Visualization of The Output Data
                        CalculationsAndOutputs(this.totalGSF, this.totalRawDepartmentCost);

                        // Add Stacking Data To The Stacking Tab
                        ExtraMethods.GenerateProgramsStacking(this.boxesOfTheProject, this.stackingVisualization, this.ProgramsStackingGrid, StackingButton_Click);
                    }
                    // Input is equal to existing number of Departments
                    if (existingDepartments == input)
                    {
                        return;
                    }
                }

                // If user input for Number of Departments is Equal to Zero
                else
                {
                    MessageBox.Show("Please Enter A Number Larger Than Zero.");
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
            this.boxesOfTheProject.Clear();
            this.programVisualizationLabelsGroup.Children.Clear();
            this.labelIndexes.Clear();

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

            // CheckBoxes
            this.ProgramsCheckBox.IsChecked = false;

            // SubWindows: Programs Window
            if (this.programsWindow != null)
            {
                this.programsWindow.Close();
            }

            // ProjectBox Visualization
            string projectBoxName = "ProjectBox";
            Point3D projectBoxCenter = new Point3D(0, 0, float.Parse(this.ProjectHeight.Text) * 0.5);
            float[] projectBoxDims = new float[] { float.Parse(ProjectWidth.Text), float.Parse(ProjectLength.Text), float.Parse(ProjectHeight.Text) };

            Box projectBox = new Box(projectBoxName, projectBoxCenter);

            GeometryModel3D projectVisualizationBox = VisualizationMethods.GenerateBox(projectBoxName, projectBoxCenter, projectBoxDims,
                new SpecularMaterial(Brushes.Transparent, 1), MaterialHelper.CreateMaterial(Colors.Gray));

            this.boxesOfTheProject.Add(projectBox.name, projectBox);
            this.stackingVisualization.Children.Add(projectVisualizationBox);

            // Generating initial Expanders and programs visualization
            for (int i = 0; i < initialNumberOfDepartments; i++)
            {
                Expander department = ExtraMethods.DepartmentGernerator(i);
                ExtraMethods.departmentExpanderGenerator(department, 4, this.functions,
                    DepartmentNameAndNumberButton_Click, SelectedProgram_Chenged, ProgramSlider_ValueChanged);

                this.DepartmentsWrapper.Children.Add(department);

                /* Setting up initial Departments and Programs visualization */
                // Generating a random color in the format of an array that contains three bytes
                byte[] color = { Convert.ToByte(random.Next(255)), Convert.ToByte(random.Next(255)), Convert.ToByte(random.Next(255)) };
                this.colorsOfDepartments.Add(color);

                for (int j = 0; j < initialNumberOfPrograms; j++)
                {
                    // Calculating Raw Cost and GSF of Each Program
                    ComboBox program = LogicalTreeHelper.FindLogicalNode(department, department.Name + "ComboBox" + j.ToString()) as ComboBox;
                    Slider keyRooms = LogicalTreeHelper.FindLogicalNode(department, department.Name + "Rooms" + j.ToString()) as Slider;
                    Slider DGSF = LogicalTreeHelper.FindLogicalNode(department, department.Name + "DGSF" + j.ToString()) as Slider;
                    Label labelElement = LogicalTreeHelper.FindLogicalNode(department, department.Name + "Label" + j.ToString()) as Label;
                    this.initialProgramLength = ((float)(keyRooms.Value * DGSF.Value)) / this.initialProjectBoxDims[0];

                    // Adding To Total GSF and Total Raw Cost
                    float GSF = ((float)(keyRooms.Value * DGSF.Value));
                    float rawCost = GSF * this.functions[program.SelectedItem.ToString()]["cost"];
                    this.totalGSF += GSF;
                    this.totalRawDepartmentCost += rawCost;

                    // Generate gradient colors for programs of each department
                    float stop = ((float)j) / ((float)initialNumberOfPrograms);
                    byte[] gradient = VisualizationMethods.GenerateGradientColor(color, stop);

                    // Setting Program Label Background Color
                    ExtraMethods.ChangeLabelColor(department, j, gradient);

                    string programBoxName = department.Name + "ProgramBox" + j.ToString();
                    Material programBoxMaterial = MaterialHelper.CreateMaterial(Color.FromRgb(gradient[0], gradient[1], gradient[2]));
                    float[] programBoxDims = { float.Parse(this.ProjectWidth.Text), this.initialProgramLength, float.Parse(this.FloorHeight.Text) };
                    Point3D programBoxCenter = new Point3D(0,
                        ((programBoxDims[1] * 0.5) + (j * programBoxDims[1])) - (float.Parse(ProjectLength.Text) * 0.5),
                        float.Parse(this.FloorHeight.Text) * 0.5 + (i * float.Parse(this.FloorHeight.Text)));

                    Box programBox = new Box(programBoxName, programBoxCenter);
                    programBox.boxDims = programBoxDims;
                    programBox.departmentName = department.Header.ToString();
                    programBox.boxColor = Color.FromRgb(gradient[0], gradient[1], gradient[2]);
                    programBox.function = program.SelectedItem.ToString();
                    programBox.keyRooms = (int)keyRooms.Value;
                    programBox.DGSF = (float)DGSF.Value;
                    programBox.cost = this.functions[program.SelectedItem.ToString()]["cost"];
                    programBox.boxTotalGSFValue = GSF;
                    programBox.totalRawCostValue = rawCost;
                    programBox.floor = Convert.ToInt32(Math.Floor(((float)programBox.boxCenter.Z) / programBoxDims[2]));
                    programBox.visualizationLabel = labelElement.Content.ToString();

                    GeometryModel3D programBoxVisualization = VisualizationMethods.GenerateBox(programBoxName, programBoxCenter, programBoxDims,
                        programBoxMaterial, programBoxMaterial);

                    // Visualizations Of The Labels Of The Boxes
                    VisualizationMethods.GenerateVisualizationLabel(this.programVisualizationLabelsGroup, labelElement.Content.ToString(),
                        programBoxCenter, programBoxDims, programBox.boxColor);

                    // Storing Indexes Of The Labels
                    this.labelIndexes.Add(programBoxName, new int[] { this.programVisualizationLabelsGroup.Children.Count - 1,
                        this.programVisualizationLabelsGroup.Children.Count - 2 });

                    this.boxesOfTheProject.Add(programBox.name, programBox);
                    this.stackingVisualization.Children.Add(programBoxVisualization);

                    // Add Index Of The Box To The Dictionary
                    this.boxesOfTheProject[programBox.name].visualizationIndex = this.stackingVisualization.Children.IndexOf(programBoxVisualization);
                }
            }

            // All The Calculation, Prepration, and Visualization of The Output Data
            CalculationsAndOutputs(this.totalGSF, this.totalRawDepartmentCost);

            // Generate And Visualize Stacking Data To The Stacking Tab
            ExtraMethods.GenerateProgramsStacking(this.boxesOfTheProject, this.stackingVisualization, this.ProgramsStackingGrid, StackingButton_Click);
        }

        /* ----------------The Event for Setting Name of The Departments and The Number of Programs It Contains ---------------- */
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
                    department.Header = nameTextBox.Text;
                    for (int i = 0; i < this.stackingVisualization.Children.Count; i++)
                    {
                        if (this.stackingVisualization.Children[i].GetName().Contains(department.Name))
                        {
                            this.boxesOfTheProject[this.stackingVisualization.Children[i].GetName()].departmentName = nameTextBox.Text;
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Please Enter A Name Inside The \"Name of Department\" Box.");
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
                    MessageBox.Show("Please Enter A Number.");
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
                            SelectedProgram_Chenged, ProgramSlider_ValueChanged);

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
                                float GSF = ((float)(keyRooms.Value * DGSF.Value));
                                float rawCost = GSF * this.functions[program.SelectedItem.ToString()]["cost"];
                                this.totalGSF += GSF;
                                this.totalRawDepartmentCost += rawCost;

                                // Calculating Length of Each Program Based on Width of The Project Box
                                string programBoxName = department.Name + "ProgramBox" + (i).ToString();
                                float[] programBoxDims = { float.Parse(this.ProjectWidth.Text), this.initialProgramLength, float.Parse(this.FloorHeight.Text) };
                                Point3D programBoxCenter = new Point3D(0,
                                    ((totalExistingProgramsLength + ((i - existingPrograms) * programBoxDims[1]) + programBoxDims[1] / 2) - (float.Parse(ProjectLength.Text) * 0.5)),
                                    float.Parse(this.FloorHeight.Text) * 0.5 + (indexOfDepartment * int.Parse(this.FloorHeight.Text)));

                                Box programBox = new Box(programBoxName, programBoxCenter);
                                programBox.departmentName = department.Header.ToString();
                                programBox.boxColor = Color.FromRgb(gradient[0], gradient[1], gradient[2]);
                                programBox.function = program.SelectedItem.ToString();
                                programBox.keyRooms = (int)keyRooms.Value;
                                programBox.DGSF = (float)DGSF.Value;
                                programBox.cost = this.functions[program.SelectedItem.ToString()]["cost"];
                                programBox.boxTotalGSFValue = GSF;
                                programBox.totalRawCostValue = rawCost;
                                programBox.floor = Convert.ToInt32(Math.Floor(((float)programBox.boxCenter.Z) / programBoxDims[2]));

                                GeometryModel3D programBoxVisualization = VisualizationMethods.GenerateBox(programBoxName, programBoxCenter, programBoxDims,
                                    programBoxMaterial, programBoxMaterial);

                                this.stackingVisualization.Children.Insert(lastProgramBoxIndex, programBoxVisualization);
                                this.boxesOfTheProject.Add(programBoxName, programBox);

                                // Add Index Of The Box To The Dictionary
                                this.boxesOfTheProject[programBox.name].visualizationIndex = this.stackingVisualization.Children.IndexOf(programBoxVisualization);

                                lastProgramBoxIndex += 1;
                            }
                        }

                        // All The Calculation, Prepration, and Visualization of The Output Data
                        CalculationsAndOutputs(this.totalGSF, this.totalRawDepartmentCost);

                        // Add Stacking Data To The Stacking Tab
                        ExtraMethods.GenerateProgramsStacking(this.boxesOfTheProject, this.stackingVisualization, this.ProgramsStackingGrid, StackingButton_Click);
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

                                string programBoxName = department.Name + "ProgramBox" + (i).ToString();

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

                                // Remove The Removed Program From the Dictionary of Boxes
                                this.boxesOfTheProject.Remove(programBoxName);

                                this.stackingVisualization.Children.RemoveAt(lastProgramBoxIndex);
                                lastProgramBoxIndex += -1;
                            }
                        }

                        // All The Calculation, Prepration, and Visualization of The Output Data
                        CalculationsAndOutputs(this.totalGSF, this.totalRawDepartmentCost);

                        // Omit Stacking Data From The Stacking Tab
                        ExtraMethods.GenerateProgramsStacking(this.boxesOfTheProject, this.stackingVisualization, this.ProgramsStackingGrid, StackingButton_Click);
                    }
                    if (input == existingPrograms)
                    {
                        return;
                    }
                }
                else
                {
                    MessageBox.Show("Please Enter A Number Larger Than Zero.");
                    numberTextBox.Text = existingPrograms.ToString();
                }
            }
        }

        /*------------------ Handeling Project Size Change Events ------------------*/
        private void ProjectSize_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;


            // Clear Visualization Labels
            this.programVisualizationLabelsGroup.Children.Clear();
            this.labelIndexes.Clear();

            // Handeling Project Width Changes Events
            if (btn.Name == "ProjectWidthButton")
            {
                float projectWidthInput = new float();

                try
                {
                    projectWidthInput = float.Parse(this.ProjectWidth.Text);
                }
                catch
                {
                    MessageBox.Show("Please Enter A Number.");
                    this.ProjectWidth.Text = this.stackingVisualization.Children[0].Bounds.SizeX.ToString();
                    return;
                }
                if (projectWidthInput > 0)
                {

                    double totalDepartmentLength = this.initialProjectBoxDims[1] * -0.5;

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
                            string programBoxName = this.stackingVisualization.Children[i].GetName();
                            int departmentIndex = int.Parse(programBoxName.Replace("ProgramBo", "").Split('x')[0].Replace("D", "")) - 1;
                            int programIndex = int.Parse(programBoxName.Replace("ProgramBo", "").Split('x')[1]);

                            // First Box In Each Floor
                            if ((this.boxesOfTheProject[programBoxName].boxCenter.Y - this.stackingVisualization.Children[i].Bounds.SizeY / 2) == this.initialProjectBoxDims[1] * -0.5)
                            {
                                double newLength = (this.stackingVisualization.Children[i].Bounds.SizeY * this.stackingVisualization.Children[i].Bounds.SizeX) / projectWidthInput;

                                string newProgramBoxName = this.stackingVisualization.Children[i].GetName();
                                float[] newProgramBoxDims = {(float) this.stackingVisualization.Children[0].Bounds.SizeX, (float) newLength,
                                    (float) this.stackingVisualization.Children[i].Bounds.SizeZ };
                                double newProgramCenterY = (this.initialProjectBoxDims[1] * -0.5) + (newLength / 2);
                                Point3D newProgramBoxCenter = new Point3D(0, newProgramCenterY, this.boxesOfTheProject[programBoxName].boxCenter.Z);

                                GeometryModel3D programBoxVisualization = VisualizationMethods.GenerateBox(newProgramBoxName, newProgramBoxCenter, newProgramBoxDims,
                                    ((GeometryModel3D)this.stackingVisualization.Children[i]).Material,
                                    ((GeometryModel3D)this.stackingVisualization.Children[i]).Material);
                                
                                // Visualizations Of The Labels Of The Boxes
                                VisualizationMethods.GenerateVisualizationLabel(this.programVisualizationLabelsGroup, this.boxesOfTheProject[newProgramBoxName].visualizationLabel,
                                    newProgramBoxCenter, newProgramBoxDims, this.boxesOfTheProject[newProgramBoxName].boxColor);

                                // Storing Indexes Of The Labels
                                this.labelIndexes.Add(newProgramBoxName, new int[] { this.programVisualizationLabelsGroup.Children.Count - 1,
                                    this.programVisualizationLabelsGroup.Children.Count - 2 });

                                this.stackingVisualization.Children.RemoveAt(i);
                                this.stackingVisualization.Children.Insert(i, programBoxVisualization);
                                this.boxesOfTheProject[newProgramBoxName].boxCenter = newProgramBoxCenter;
                                this.boxesOfTheProject[newProgramBoxName].boxDims = newProgramBoxDims;

                                // Add Index Of The Box To The Dictionary
                                this.boxesOfTheProject[newProgramBoxName].visualizationIndex = this.stackingVisualization.Children.IndexOf(programBoxVisualization);

                                totalDepartmentLength = (this.initialProjectBoxDims[1] * -0.5) + this.stackingVisualization.Children[i].Bounds.SizeY;
                            }

                            // Other Boxes Of Each Floor
                            else
                            {
                                double newLength = (this.stackingVisualization.Children[i].Bounds.SizeY * this.stackingVisualization.Children[i].Bounds.SizeX) / projectWidthInput;

                                string newProgramBoxName = this.stackingVisualization.Children[i].GetName();
                                float[] newProgramBoxDims = {(float) this.stackingVisualization.Children[0].Bounds.SizeX, (float) newLength,
                                    (float) this.stackingVisualization.Children[i].Bounds.SizeZ };
                                double newProgramCenterY = totalDepartmentLength + (newProgramBoxDims[1] / 2);

                                Point3D newProgramBoxCenter = new Point3D(0, newProgramCenterY, this.boxesOfTheProject[programBoxName].boxCenter.Z);

                                GeometryModel3D programBoxVisualization = VisualizationMethods.GenerateBox(newProgramBoxName, newProgramBoxCenter, newProgramBoxDims,
                                    ((GeometryModel3D)this.stackingVisualization.Children[i]).Material,
                                    ((GeometryModel3D)this.stackingVisualization.Children[i]).Material);

                                // Visualizations Of The Labels Of The Boxes
                                VisualizationMethods.GenerateVisualizationLabel(this.programVisualizationLabelsGroup, this.boxesOfTheProject[newProgramBoxName].visualizationLabel,
                                    newProgramBoxCenter, newProgramBoxDims, this.boxesOfTheProject[newProgramBoxName].boxColor);

                                // Storing Indexes Of The Labels
                                this.labelIndexes.Add(newProgramBoxName, new int[] { this.programVisualizationLabelsGroup.Children.Count - 1, this.programVisualizationLabelsGroup.Children.Count - 2 });

                                this.stackingVisualization.Children.RemoveAt(i);
                                this.stackingVisualization.Children.Insert(i, programBoxVisualization);
                                this.boxesOfTheProject[newProgramBoxName].boxCenter = newProgramBoxCenter;
                                this.boxesOfTheProject[newProgramBoxName].boxDims = newProgramBoxDims;

                                // Add Index Of The Box To The Dictionary
                                this.boxesOfTheProject[newProgramBoxName].visualizationIndex = this.stackingVisualization.Children.IndexOf(programBoxVisualization);

                                totalDepartmentLength += this.stackingVisualization.Children[i].Bounds.SizeY;
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Please Enter A Number Larger Than Zero.");
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
                    MessageBox.Show("Please Enter A Number.");
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
                    MessageBox.Show("Please Enter A Number Larger Than Zero.");
                    this.ProjectLength.Text = this.stackingVisualization.Children[0].Bounds.SizeY.ToString();
                    return;
                }
            }

            // Handeling Project Height Changes Events
            if (btn.Name == "ProjectHeightButton")
            {
                float projectHeightInput = 0;

                try
                {
                    projectHeightInput = float.Parse(this.ProjectHeight.Text);
                }
                catch
                {
                    MessageBox.Show("Please Enter A Number.");
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
                    MessageBox.Show("Please Enter A Number Larger Than Zero.");
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
                    MessageBox.Show("Please Enter A Number.");
                    this.FloorHeight.Text = this.stackingVisualization.Children[1].Bounds.SizeZ.ToString();
                    return;
                }
                if (floorHeightInput > 0)
                {

                    double newProgramCenterY = this.initialProjectBoxDims[1] * -0.5;

                    for (int i = 1; i < this.stackingVisualization.Children.Count; i++)
                    {
                        string programBoxName = this.stackingVisualization.Children[i].GetName();
                        int departmentIndex = int.Parse(programBoxName.Replace("ProgramBo", "").Split('x')[0].Replace("D", "")) - 1;
                        int programIndex = int.Parse(programBoxName.Replace("ProgramBo", "").Split('x')[1]);

                        // First Box Of Each Floor
                        if ((this.boxesOfTheProject[programBoxName].boxCenter.Y - this.stackingVisualization.Children[i].Bounds.SizeY / 2) == this.initialProjectBoxDims[1] * -0.5)
                        {
                            string newProgramBoxName = this.stackingVisualization.Children[i].GetName();
                            newProgramCenterY = (this.initialProjectBoxDims[1] * -0.5) + (this.stackingVisualization.Children[i].Bounds.SizeY / 2);
                            float[] newProgramBoxDims = {(float) this.stackingVisualization.Children[0].Bounds.SizeX, (float) this.stackingVisualization.Children[i].Bounds.SizeY,
                                    floorHeightInput };
                            Point3D newProgramBoxCenter = new Point3D(0, newProgramCenterY,
                                floorHeightInput * 0.5 + (this.boxesOfTheProject[newProgramBoxName].floor * floorHeightInput));

                            GeometryModel3D programBoxVisualization = VisualizationMethods.GenerateBox(newProgramBoxName, newProgramBoxCenter, newProgramBoxDims,
                                ((GeometryModel3D)this.stackingVisualization.Children[i]).Material,
                                ((GeometryModel3D)this.stackingVisualization.Children[i]).Material);

                            // Visualizations Of The Labels Of The Boxes
                            VisualizationMethods.GenerateVisualizationLabel(this.programVisualizationLabelsGroup, this.boxesOfTheProject[newProgramBoxName].visualizationLabel,
                                newProgramBoxCenter, newProgramBoxDims, this.boxesOfTheProject[newProgramBoxName].boxColor);

                            // Storing Indexes Of The Labels
                            this.labelIndexes.Add(newProgramBoxName, new int[] { this.programVisualizationLabelsGroup.Children.Count - 1,
                                    this.programVisualizationLabelsGroup.Children.Count - 2 });

                            this.stackingVisualization.Children.RemoveAt(i);
                            this.stackingVisualization.Children.Insert(i, programBoxVisualization);
                            this.boxesOfTheProject[newProgramBoxName].boxCenter = newProgramBoxCenter;
                            this.boxesOfTheProject[newProgramBoxName].boxDims = newProgramBoxDims;

                            // Add Index Of The Box To The Dictionary
                            this.boxesOfTheProject[newProgramBoxName].visualizationIndex = this.stackingVisualization.Children.IndexOf(programBoxVisualization);

                            newProgramCenterY += this.stackingVisualization.Children[i].Bounds.SizeY / 2;
                        }
                        // Other Boxes Of Each Floor
                        else
                        {
                            newProgramCenterY += this.stackingVisualization.Children[i].Bounds.SizeY / 2;

                            string newProgramBoxName = this.stackingVisualization.Children[i].GetName();
                            float[] newProgramBoxDims = {(float) this.stackingVisualization.Children[0].Bounds.SizeX, (float) this.stackingVisualization.Children[i].Bounds.SizeY,
                                    floorHeightInput };
                            Point3D newProgramBoxCenter = new Point3D(0, newProgramCenterY,
                                floorHeightInput * 0.5 + (this.boxesOfTheProject[newProgramBoxName].floor * floorHeightInput));

                            GeometryModel3D programBoxVisualization = VisualizationMethods.GenerateBox(newProgramBoxName, newProgramBoxCenter, newProgramBoxDims,
                                ((GeometryModel3D)this.stackingVisualization.Children[i]).Material,
                                ((GeometryModel3D)this.stackingVisualization.Children[i]).Material);

                            // Visualizations Of The Labels Of The Boxes
                            VisualizationMethods.GenerateVisualizationLabel(this.programVisualizationLabelsGroup, this.boxesOfTheProject[newProgramBoxName].visualizationLabel,
                                newProgramBoxCenter, newProgramBoxDims, this.boxesOfTheProject[newProgramBoxName].boxColor);

                            // Storing Indexes Of The Labels
                            this.labelIndexes.Add(newProgramBoxName, new int[] { this.programVisualizationLabelsGroup.Children.Count - 1,
                                    this.programVisualizationLabelsGroup.Children.Count - 2 });

                            this.stackingVisualization.Children.RemoveAt(i);
                            this.stackingVisualization.Children.Insert(i, programBoxVisualization);
                            this.boxesOfTheProject[newProgramBoxName].boxCenter = newProgramBoxCenter;
                            this.boxesOfTheProject[newProgramBoxName].boxDims = newProgramBoxDims;

                            // Add Index Of The Box To The Dictionary
                            this.boxesOfTheProject[newProgramBoxName].visualizationIndex = this.stackingVisualization.Children.IndexOf(programBoxVisualization);

                            newProgramCenterY += this.stackingVisualization.Children[i].Bounds.SizeY / 2;
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Please Enter A Number Larger Than Zero.");
                    this.ProjectHeight.Text = this.stackingVisualization.Children[1].Bounds.SizeZ.ToString();
                    return;
                }
            }

            // BGSF Limit On Output Window Changes
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

        /* ------------------------ Handeling Program ComboBox Change Event ------------------------ */
        void SelectedProgram_Chenged(object sender, EventArgs e)
        {
            ComboBox cbx = sender as ComboBox;

            // Extracting The Department and The Program Indices of The Changed ComboBox
            string programBoxName = cbx.Name.Replace("ComboBox", "ProgramBox");
            int departmentIndex = int.Parse(programBoxName.Replace("ProgramBo", "").Split('x')[0].Replace("D", "")) - 1;
            int programIndex = int.Parse(programBoxName.Replace("ProgramBo", "").Split('x')[1]);
            
            // Extracting The Department That Changed
            Expander department = this.DepartmentsWrapper.Children[departmentIndex] as Expander;

            // Extracting The Sliders That Need Changes
            String keyRoomsSliderName = cbx.Name.Replace("ComboBox", "Rooms");
            String DGSFSliderName = cbx.Name.Replace("ComboBox", "DGSF");

            // Calculating Length of Each Program Based on Total Area of The Program and Width of The Project Box
            Slider keyRooms = LogicalTreeHelper.FindLogicalNode(department, keyRoomsSliderName) as Slider;
            keyRooms.Minimum = this.functions[cbx.SelectedItem.ToString()]["keyMin"];
            keyRooms.Value = this.functions[cbx.SelectedItem.ToString()]["keyVal"];
            keyRooms.Maximum = this.functions[cbx.SelectedItem.ToString()]["keyMax"];

            Slider DGSF = LogicalTreeHelper.FindLogicalNode(department, DGSFSliderName) as Slider;
            DGSF.Minimum = this.functions[cbx.SelectedItem.ToString()]["DGSFMin"];
            DGSF.Value = this.functions[cbx.SelectedItem.ToString()]["DGSFVal"];
            DGSF.Maximum = this.functions[cbx.SelectedItem.ToString()]["DGSFMax"];

            // Extracting Floor And Visualization Index Of The ProgramBox
            int programBoxFloor = this.boxesOfTheProject[programBoxName].floor;
            int programBoxVisualizationIndex = this.boxesOfTheProject[programBoxName].visualizationIndex;
           
            // Calculating the Scale Factor of Each ProgramBox
            float newProgramLength = (((float)(keyRooms.Value * DGSF.Value)) / float.Parse(this.ProjectWidth.Text));
            // Calculating The Length Difference Of The ProgramBox 
            float programLengthDifference = newProgramLength - this.boxesOfTheProject[programBoxName].boxDims[1];
            
            for (int i = programBoxVisualizationIndex; i < this.stackingVisualization.Children.Count; i++)
            {
                if (this.boxesOfTheProject[this.stackingVisualization.Children[i].GetName()].floor == programBoxFloor)
                {
                    // The Changed Program
                    if (i == programBoxVisualizationIndex)
                    {
                        string newProgramBoxName = this.stackingVisualization.Children[i].GetName();
                        float[] newProgramBoxDims = { (float)this.stackingVisualization.Children[0].Bounds.SizeX,
                            newProgramLength, (float)this.stackingVisualization.Children[i].Bounds.SizeZ };
                        Point3D newProgramBoxCenter = new Point3D(0, this.boxesOfTheProject[newProgramBoxName].boxCenter.Y + (programLengthDifference / 2),
                            this.boxesOfTheProject[newProgramBoxName].boxCenter.Z);

                        GeometryModel3D programBoxVisualization = VisualizationMethods.GenerateBox(newProgramBoxName, newProgramBoxCenter, newProgramBoxDims,
                            ((GeometryModel3D)this.stackingVisualization.Children[i]).Material,
                            ((GeometryModel3D)this.stackingVisualization.Children[i]).Material);

                        // Visualizations Of The Labels Of The Boxes
                        VisualizationMethods.ReplaceVisualizationLabel(this.programVisualizationLabelsGroup, this.labelIndexes[newProgramBoxName], this.boxesOfTheProject[newProgramBoxName].visualizationLabel,
                            newProgramBoxCenter, newProgramBoxDims, this.boxesOfTheProject[newProgramBoxName].boxColor);

                        // Calculating GSF and Cost Difference and Updating Values of The Boxes Dictionary
                        float oldGSF = this.boxesOfTheProject[newProgramBoxName].boxTotalGSFValue;
                        float oldRawProgramCost = this.boxesOfTheProject[newProgramBoxName].totalRawCostValue;
                        float newGSF = (float)(keyRooms.Value * DGSF.Value);
                        float newRawProgramCost = newGSF * this.functions[cbx.SelectedItem.ToString()]["cost"];
                        float GSFDifference = newGSF - oldGSF;
                        float rawProgramCostDifference = newRawProgramCost - oldRawProgramCost;

                        this.totalGSF += GSFDifference;
                        this.totalRawDepartmentCost += rawProgramCostDifference;

                        this.boxesOfTheProject[newProgramBoxName].boxCenter = newProgramBoxCenter;
                        this.boxesOfTheProject[newProgramBoxName].boxDims = newProgramBoxDims;
                        this.boxesOfTheProject[newProgramBoxName].function = cbx.SelectedItem.ToString();
                        this.boxesOfTheProject[newProgramBoxName].keyRooms = (int)keyRooms.Value;
                        this.boxesOfTheProject[newProgramBoxName].DGSF = (float)DGSF.Value;
                        this.boxesOfTheProject[newProgramBoxName].boxTotalGSFValue = newGSF;
                        this.boxesOfTheProject[newProgramBoxName].cost = this.functions[cbx.SelectedItem.ToString()]["cost"];
                        this.boxesOfTheProject[newProgramBoxName].totalRawCostValue = newRawProgramCost;

                        this.stackingVisualization.Children.RemoveAt(i);
                        this.stackingVisualization.Children.Insert(i, programBoxVisualization);

                        // Add Index Of The Box To The Dictionary
                        this.boxesOfTheProject[newProgramBoxName].visualizationIndex = this.stackingVisualization.Children.IndexOf(programBoxVisualization);
                    }
                    // Programs After The Changed One
                    if (i > programBoxVisualizationIndex)
                    {
                        string newProgramBoxName = this.stackingVisualization.Children[i].GetName();
                        float[] newProgramBoxDims = { (float)this.stackingVisualization.Children[0].Bounds.SizeX,
                            (float)this.stackingVisualization.Children[i].Bounds.SizeY,
                            (float)this.stackingVisualization.Children[i].Bounds.SizeZ };
                        Point3D newProgramBoxCenter = new Point3D(0, this.boxesOfTheProject[newProgramBoxName].boxCenter.Y + programLengthDifference,
                            this.boxesOfTheProject[newProgramBoxName].boxCenter.Z);

                        GeometryModel3D newProgramBox = VisualizationMethods.GenerateBox(newProgramBoxName, newProgramBoxCenter, newProgramBoxDims,
                            ((GeometryModel3D)this.stackingVisualization.Children[i]).Material,
                            ((GeometryModel3D)this.stackingVisualization.Children[i]).Material);

                        // Visualizations Of The Labels Of The Boxes
                        VisualizationMethods.ReplaceVisualizationLabel(this.programVisualizationLabelsGroup, this.labelIndexes[newProgramBoxName], this.boxesOfTheProject[newProgramBoxName].visualizationLabel,
                            newProgramBoxCenter, newProgramBoxDims, this.boxesOfTheProject[newProgramBoxName].boxColor);

                        this.stackingVisualization.Children.RemoveAt(i);
                        this.stackingVisualization.Children.Insert(i, newProgramBox);

                        this.boxesOfTheProject[newProgramBoxName].boxCenter = newProgramBoxCenter;
                        this.boxesOfTheProject[newProgramBoxName].boxDims = newProgramBoxDims;
                    }
                }
            }

            // All The Calculation, Prepration, and Visualization of The Output Data
            CalculationsAndOutputs(this.totalGSF, this.totalRawDepartmentCost);
        }

        /*---------------- Handeling Program Slider Change Event ----------------*/
        private void ProgramSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // ... Get Slider reference.
            Slider slider = sender as Slider;

            // Extracting Name Of The Program Box
            string programBoxName = "";

            // Extracting Department and Program Indices of The Changed Slider
            int departmentIndex = new int();
            int programIndex = new int();

            Expander department = new Expander();

            // Extracting Sliders of The Program
            Slider keyRooms = new Slider();
            Slider DGSF = new Slider();

            if (slider.Name.Contains("Rooms"))
            {
                // Extracting Name Of The Program Box
                programBoxName = slider.Name.Replace("Rooms", "ProgramBox");

                departmentIndex = int.Parse(slider.Name.Replace("Room", "").Split('s')[0].Replace("D", "")) - 1;
                programIndex = int.Parse(slider.Name.Replace("Room", "").Split('s')[1]);

                // Extracting the Department That Changed
                department = this.DepartmentsWrapper.Children[departmentIndex] as Expander;

                keyRooms = LogicalTreeHelper.FindLogicalNode(department, slider.Name) as Slider;
                DGSF = LogicalTreeHelper.FindLogicalNode(department, slider.Name.Replace("Rooms", "DGSF")) as Slider;
            }

            if (slider.Name.Contains("DGSF"))
            {
                // Extracting Name Of The Program Box
                programBoxName = slider.Name.Replace("DGSF", "ProgramBox");

                departmentIndex = int.Parse(slider.Name.Replace("DGS", "").Split('F')[0].Replace("D", "")) - 1;
                programIndex = int.Parse(slider.Name.Replace("DGS", "").Split('F')[1]);

                // Extracting the Department That Changed
                department = this.DepartmentsWrapper.Children[departmentIndex] as Expander;

                DGSF = LogicalTreeHelper.FindLogicalNode(department, slider.Name) as Slider;
                keyRooms = LogicalTreeHelper.FindLogicalNode(department, slider.Name.Replace("DGSF", "Rooms")) as Slider;
            }

            // Extracting Floor And Visualization Index Of The ProgramBox
            int programBoxFloor = this.boxesOfTheProject[programBoxName].floor;
            int programBoxVisualizationIndex = this.boxesOfTheProject[programBoxName].visualizationIndex;

            // Calculating The Scale Factor Of The ProgramBox
            float newProgramLength = (((float)(keyRooms.Value * DGSF.Value)) / float.Parse(this.ProjectWidth.Text));
            // Calculating The Length Difference Of The ProgramBox 
            float programLengthDifference = newProgramLength - this.boxesOfTheProject[programBoxName].boxDims[1];
            
            for (int i = programBoxVisualizationIndex; i < this.stackingVisualization.Children.Count; i++)
            {
                if (this.boxesOfTheProject[this.stackingVisualization.Children[i].GetName()].floor == programBoxFloor)
                {
                    // The Changed Program
                    if (i == programBoxVisualizationIndex)
                    {
                        string newProgramBoxName = this.stackingVisualization.Children[i].GetName();
                        float[] newProgramBoxDims = { (float)this.stackingVisualization.Children[0].Bounds.SizeX,
                            newProgramLength, (float)this.stackingVisualization.Children[i].Bounds.SizeZ };
                        Point3D newProgramBoxCenter = new Point3D(0, this.boxesOfTheProject[newProgramBoxName].boxCenter.Y + (programLengthDifference / 2),
                            this.boxesOfTheProject[newProgramBoxName].boxCenter.Z);

                        GeometryModel3D programBoxVisualization = VisualizationMethods.GenerateBox(newProgramBoxName, newProgramBoxCenter, newProgramBoxDims,
                            ((GeometryModel3D)this.stackingVisualization.Children[i]).Material,
                            ((GeometryModel3D)this.stackingVisualization.Children[i]).Material);

                        // Visualizations Of The Labels Of The Boxes
                        VisualizationMethods.ReplaceVisualizationLabel(this.programVisualizationLabelsGroup,this.labelIndexes[newProgramBoxName] ,
                            this.boxesOfTheProject[newProgramBoxName].visualizationLabel,
                            newProgramBoxCenter, newProgramBoxDims, this.boxesOfTheProject[newProgramBoxName].boxColor);

                        // Calculating GSF and Cost Difference and Updating Values of The Boxes Dictionary
                        float oldGSF = this.boxesOfTheProject[newProgramBoxName].boxTotalGSFValue;
                        float oldRawProgramCost = this.boxesOfTheProject[newProgramBoxName].totalRawCostValue;
                        float newGSF = (float)(keyRooms.Value * DGSF.Value);
                        float newRawProgramCost = newGSF * this.functions[this.boxesOfTheProject[newProgramBoxName].function]["cost"];
                        float GSFDifference = newGSF - oldGSF;
                        float rawProgramCostDifference = newRawProgramCost - oldRawProgramCost;
            
                        this.totalGSF += GSFDifference;
                        this.totalRawDepartmentCost += rawProgramCostDifference;

                        this.boxesOfTheProject[newProgramBoxName].boxCenter = newProgramBoxCenter;
                        this.boxesOfTheProject[newProgramBoxName].boxDims = newProgramBoxDims;
                        this.boxesOfTheProject[newProgramBoxName].boxCenter = newProgramBoxCenter;
                        this.boxesOfTheProject[newProgramBoxName].keyRooms = (int)keyRooms.Value;
                        this.boxesOfTheProject[newProgramBoxName].DGSF = (float)DGSF.Value;
                        this.boxesOfTheProject[newProgramBoxName].boxTotalGSFValue = newGSF;
                        this.boxesOfTheProject[newProgramBoxName].totalRawCostValue = newRawProgramCost;

                        this.stackingVisualization.Children.RemoveAt(i);
                        this.stackingVisualization.Children.Insert(i, programBoxVisualization);

                        // Add Index Of The Box To The Dictionary
                        this.boxesOfTheProject[newProgramBoxName].visualizationIndex = this.stackingVisualization.Children.IndexOf(programBoxVisualization);
                    }
                    // Programs After The Changed One
                    if (i > programBoxVisualizationIndex)
                    {
                        string newProgramBoxName = this.stackingVisualization.Children[i].GetName();
                        float[] newProgramBoxDims = { (float)this.stackingVisualization.Children[0].Bounds.SizeX,
                            (float)this.stackingVisualization.Children[i].Bounds.SizeY,
                            (float)this.stackingVisualization.Children[i].Bounds.SizeZ };
                        Point3D newProgramBoxCenter = new Point3D(0, this.boxesOfTheProject[newProgramBoxName].boxCenter.Y + programLengthDifference, 
                            this.boxesOfTheProject[newProgramBoxName].boxCenter.Z);

                        GeometryModel3D programBoxVisualization = VisualizationMethods.GenerateBox(newProgramBoxName, newProgramBoxCenter, newProgramBoxDims,
                            ((GeometryModel3D)this.stackingVisualization.Children[i]).Material,
                            ((GeometryModel3D)this.stackingVisualization.Children[i]).Material);

                        // Visualizations Of The Labels Of The Boxes
                        VisualizationMethods.ReplaceVisualizationLabel(this.programVisualizationLabelsGroup, this.labelIndexes[newProgramBoxName], 
                            this.boxesOfTheProject[newProgramBoxName].visualizationLabel,
                            newProgramBoxCenter, newProgramBoxDims, this.boxesOfTheProject[newProgramBoxName].boxColor);

                        this.stackingVisualization.Children.RemoveAt(i);
                        this.stackingVisualization.Children.Insert(i, programBoxVisualization);
                        this.boxesOfTheProject[newProgramBoxName].boxCenter = newProgramBoxCenter;
                        this.boxesOfTheProject[newProgramBoxName].boxDims = newProgramBoxDims;

                        // Add Index Of The Box To The Dictionary
                        this.boxesOfTheProject[newProgramBoxName].visualizationIndex = this.stackingVisualization.Children.IndexOf(programBoxVisualization);
                    }
                }
                // Break The Loop On Next Floor
                else
                {
                    break;
                }
            }

            // All The Calculation, Prepration, and Visualization of The Output Data
            CalculationsAndOutputs(this.totalGSF, this.totalRawDepartmentCost);
        }

        /* ########################################################### Handeling Events of The Cost Tab ########################################################### */

        /* ----------------------------------- Handeling Total Budget Button Event ----------------------------------- */
        private void TotalBudgetButton_Click(object sender, RoutedEventArgs e)
        {
            float tempTotalBudget = new float();

            // Only Work When a Project is Open
            if (this.functions.Count > 0)
            {
                try
                {
                    tempTotalBudget = float.Parse(this.TotalBudget.Text);
                }
                catch
                {
                    MessageBox.Show("Please Enter A Number.");
                    this.TotalBudget.Text = ExtraMethods.CastDollar(this.totalBudget);
                    return;
                }

                if (tempTotalBudget > 0)
                {
                    this.totalBudget = tempTotalBudget;

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

                    this.TotalBudget.Text = ExtraMethods.CastDollar(this.totalBudget);
                }
                else
                {
                    MessageBox.Show("Please Enter A Number Larger Than Zero.");
                    this.TotalBudget.Text = ExtraMethods.CastDollar(this.totalBudget);
                    return;
                }
            }
            else
            {
                MessageBox.Show("Please Open A Spread Sheet That Contains Information Of The Project.");
                this.TotalBudget.Text = ExtraMethods.CastDollar(this.totalBudget);
                return;
            }
        }

        /*---------------- Handeling Program Slider Change Event ----------------*/
        private void GrossMultiplierSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // All The Calculation, Prepration, and Visualization of The Output Data
            CalculationsAndOutputs(this.totalGSF, this.totalRawDepartmentCost);
        }

        /* ----------------------------------- Handeling Indirect Multiplier Button Event ----------------------------------- */
        private void IndirectMultiplier_Click(object sender, RoutedEventArgs e)
        {
            float tempIndirectMultiplier = new float();

            try
            {
                tempIndirectMultiplier = float.Parse(this.IndirectMultiplier.Text);
            }
            catch
            {
                MessageBox.Show("Please Enter A Number.");
                this.IndirectMultiplier.Text = this.indirectMultiplier.ToString();
                return;
            }

            if (tempIndirectMultiplier > 0)
            {
                this.indirectMultiplier = tempIndirectMultiplier;

                this.projectCost = this.constructionCost * this.indirectMultiplier;

                // Information Outputs
                this.ConstructionCost.Text = ExtraMethods.CastDollar(this.constructionCost);
                this.ProjectCost.Text = ExtraMethods.CastDollar(this.projectCost);

                // Budget Difference
                this.budgetDifference = this.totalBudget - this.projectCost;

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
            }
            else
            {
                MessageBox.Show("Please Enter A Number Larger Than Zero.");
                this.IndirectMultiplier.Text = this.indirectMultiplier.ToString();
                return;
            }
        }

        /* ----------------------------------- Handeling Land Cost Button Event ----------------------------------- */
        private void LandCost_Click(object sender, RoutedEventArgs e)
        {
            float tempLandCost = new float();

            try
            {
                tempLandCost = float.Parse(this.LandCost.Text);
            }
            catch
            {
                MessageBox.Show("Please Enter A Number.");
                this.LandCost.Text = ExtraMethods.CastDollar(this.landCost);
                return;
            }

            if (tempLandCost >= 0)
            {
                this.landCost = tempLandCost;

                // All The Calculation, Prepration, and Visualization of The Output Data
                CalculationsAndOutputs(this.totalGSF, this.totalRawDepartmentCost);

                this.LandCost.Text = ExtraMethods.CastDollar(this.landCost);
            }
            else
            {
                MessageBox.Show("Please Enter A Number Larger Than, Or Equal To Zero.");
                this.LandCost.Text = ExtraMethods.CastDollar(this.landCost);
                return;
            }
        }

        /* ----------------------------------- Handeling General Costs Button Event ----------------------------------- */
        private void GeneralCosts_Click(object sender, RoutedEventArgs e)
        {
            float tempGeneralCosts = new float();

            try
            {
                tempGeneralCosts = float.Parse(this.GeneralCosts.Text);
            }
            catch
            {
                MessageBox.Show("Please Enter A Number.");
                this.GeneralCosts.Text = ExtraMethods.CastDollar(this.generalCosts);
                return;
            }

            if (tempGeneralCosts >= 0)
            {
                this.generalCosts = tempGeneralCosts;

                // All The Calculation, Prepration, and Visualization of The Output Data
                CalculationsAndOutputs(this.totalGSF, this.totalRawDepartmentCost);

                this.GeneralCosts.Text = ExtraMethods.CastDollar(this.generalCosts);
            }
            else
            {
                MessageBox.Show("Please Enter A Number Larger Than, Or Equal To Zero.");
                this.GeneralCosts.Text = ExtraMethods.CastDollar(this.generalCosts);
                return;
            }
        }

        /* ----------------------------------- Handeling Design Contingency Button Event ----------------------------------- */
        private void DesignContingency_Click(object sender, RoutedEventArgs e)
        {
            float tempDesignContingency = new float();

            try
            {
                tempDesignContingency = float.Parse(this.DesignContingency.Text);
            }
            catch
            {
                MessageBox.Show("Please Enter A Number.");
                this.DesignContingency.Text = ExtraMethods.CastDollar(this.designContingency);
                return;
            }

            if (tempDesignContingency >= 0)
            {
                this.designContingency = tempDesignContingency;

                // All The Calculation, Prepration, and Visualization of The Output Data
                CalculationsAndOutputs(this.totalGSF, this.totalRawDepartmentCost);

                this.DesignContingency.Text = ExtraMethods.CastDollar(this.designContingency);
            }
            else
            {
                MessageBox.Show("Please Enter A Number Larger Than, Or Equal To Zero.");
                this.DesignContingency.Text = ExtraMethods.CastDollar(this.designContingency);
                return;
            }
        }

        /* ----------------------------------- Handeling Build Contingency Button Event ----------------------------------- */
        private void BuildContingency_Click(object sender, RoutedEventArgs e)
        {
            float tempBuildContingency = new float();

            try
            {
                tempBuildContingency = float.Parse(this.BuildContingency.Text);
            }
            catch
            {
                MessageBox.Show("Please Enter A Number.");
                this.BuildContingency.Text = ExtraMethods.CastDollar(this.buildContingency);
                return;
            }

            if (tempBuildContingency >= 0)
            {
                this.buildContingency = tempBuildContingency;

                // All The Calculation, Prepration, and Visualization of The Output Data
                CalculationsAndOutputs(this.totalGSF, this.totalRawDepartmentCost);

                this.BuildContingency.Text = ExtraMethods.CastDollar(this.buildContingency);
            }
            else
            {
                MessageBox.Show("Please Enter A Number Larger Than, Or Equal To Zero.");
                this.BuildContingency.Text = ExtraMethods.CastDollar(this.buildContingency);
                return;
            }
        }

        /* ----------------------------------- Handeling CCIP Button Event ----------------------------------- */
        private void CCIP_Click(object sender, RoutedEventArgs e)
        {
            float tempCCIP = new float();

            try
            {
                tempCCIP = float.Parse(this.CCIP.Text);
            }
            catch
            {
                MessageBox.Show("Please Enter A Number.");
                this.CCIP.Text = ExtraMethods.CastDollar(this.cCIP);
                return;
            }

            if (tempCCIP >= 0)
            {
                this.cCIP = tempCCIP;

                // All The Calculation, Prepration, and Visualization of The Output Data
                CalculationsAndOutputs(this.totalGSF, this.totalRawDepartmentCost);

                this.CCIP.Text = ExtraMethods.CastDollar(this.cCIP);
            }
            else
            {
                MessageBox.Show("Please Enter A Number Larger Than, Or Equal To Zero.");
                this.CCIP.Text = ExtraMethods.CastDollar(this.cCIP);
                return;
            }
        }

        /* ----------------------------------- Handeling CMFee Button Event ----------------------------------- */
        private void CMFee_Click(object sender, RoutedEventArgs e)
        {
            float tempCMFee = new float();

            try
            {
                tempCMFee = float.Parse(this.CMFee.Text);
            }
            catch
            {
                MessageBox.Show("Please Enter A Number.");
                this.CMFee.Text = ExtraMethods.CastDollar(this.cMFee);
                return;
            }

            if (tempCMFee >= 0)
            {
                this.cMFee = tempCMFee;

                // All The Calculation, Prepration, and Visualization of The Output Data
                CalculationsAndOutputs(this.totalGSF, this.totalRawDepartmentCost);

                this.CMFee.Text = ExtraMethods.CastDollar(this.cMFee);
            }
            else
            {
                MessageBox.Show("Please Enter A Number Larger Than, Or Equal To Zero.");
                this.CMFee.Text = ExtraMethods.CastDollar(this.cMFee);
                return;
            }
        }


        /* ########################################################### Stacking and Programs Events ########################################################### */

        /* ----------------------------------- Handeling Programs CheckBox Event and Programs SubWindow ----------------------------------- */
        private void Programs_Click(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;

            if (checkBox.IsChecked == true)
            {
                if (this.programsWindow != null)
                {
                    // Close The Open Program
                    this.programsWindow.Close();

                    // Initiate A New Program Window
                    this.programsWindow = new ProgramsSubWindow();
                    this.programsWindow.Owner = this;

                    // Generating Programs' Data And Add Them To The Programs SubWindow
                    ExtraMethods.DisplayProgramData(this.boxesOfTheProject, this.stackingVisualization, this.programsWindow);

                    // Display Programs SubWindow
                    this.programsWindow.Show();
                }
                else
                {
                    // Generating Programs' Data And Add Them To The Programs SubWindow
                    ExtraMethods.DisplayProgramData(this.boxesOfTheProject, this.stackingVisualization, programsWindow);

                    // Display Programs SubWindow
                    programsWindow.Show();
                }
            }
            else
            {
                this.programsWindow.Close();
            }
        }

        /* ----------------------------------- Handeling Stacking Button Event----------------------------------- */
        private void StackingButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;

            TextBox programNumberTextBox = LogicalTreeHelper.FindLogicalNode(this.ProgramsStackingGrid, btn.Name.Replace("SetButton", "TextBox")) as TextBox;

            string programBoxName = btn.Name.Replace("SetButton", "");

            int inputFloor = new int();

            try
            {
                // User Input
                inputFloor = int.Parse(programNumberTextBox.Text);
            }
            catch
            {
                MessageBox.Show("Please Enter A Number.");
                programNumberTextBox.Text = this.boxesOfTheProject[programBoxName].floor.ToString();
                return;
            }

            if (inputFloor >= 0)
            {
                // If New Input Is Simillar To Old Old Value
                if (inputFloor == this.boxesOfTheProject[programBoxName].floor)
                {
                    return;
                }

                // If New Value Is Different With Old Value
                else
                {
                    string targetBoxDepartment = programBoxName.Replace("ProgramBo", "").Split('x')[0];

                    // Box Number In Its Department
                    int targetBoxNumber = int.Parse(programBoxName.Replace("ProgramBo", "").Split('x')[1]);

                    // Indexes Of The Visualization Box
                    int oldVisualizationBoxIndex = this.boxesOfTheProject[programBoxName].visualizationIndex;
                    int newVisualizationBoxIndex = 0;

                    float targetBoxLength = new float();

                    // Extract Length And Index Of The Target Visualization Box
                    for (int i = 0; i < this.stackingVisualization.Children.Count; i++)
                    {
                        if (i > 0)
                        {
                            // Get Target Program Box Index And Length
                            if (this.stackingVisualization.Children[i].GetName() == programBoxName)
                            {
                                // Length Of The Target Visualization Box
                                targetBoxLength = (float)this.stackingVisualization.Children[i].Bounds.SizeY;
                            }
                            if (this.boxesOfTheProject[this.stackingVisualization.Children[i].GetName()].floor < inputFloor + 1)
                            {
                                newVisualizationBoxIndex += 1;
                            }
                        }
                    }

                    // Y Value Of The Center Of The New Box
                    float newTargetCenterY = (this.initialProjectBoxDims[1] * -0.5f);

                    // Moving The Boxes After The Target Box
                    for (int i = 0; i < this.stackingVisualization.Children.Count; i++)
                    {
                        if (i > 0)
                        {
                            string newProgramBoxName = this.stackingVisualization.Children[i].GetName();

                            // Program Boxes After Target Program Box
                            if (this.boxesOfTheProject[this.stackingVisualization.Children[i].GetName()].floor == this.boxesOfTheProject[programBoxName].floor &&
                                this.boxesOfTheProject[this.stackingVisualization.Children[i].GetName()].boxCenter.Y > this.boxesOfTheProject[programBoxName].boxCenter.Y)
                            {
                                float[] newProgramBoxDims = { (float)this.stackingVisualization.Children[0].Bounds.SizeX,
                                    (float)this.stackingVisualization.Children[i].Bounds.SizeY,
                                    (float)this.stackingVisualization.Children[i].Bounds.SizeZ };

                                float newProgramCenterY = ((float)this.boxesOfTheProject[newProgramBoxName].boxCenter.Y) - targetBoxLength;

                                Point3D newProgramBoxCenter = new Point3D(0,
                                    newProgramCenterY, ((float)this.boxesOfTheProject[newProgramBoxName].boxCenter.Z));

                                GeometryModel3D programBoxVisualization = VisualizationMethods.GenerateBox(newProgramBoxName, newProgramBoxCenter, newProgramBoxDims,
                                    ((GeometryModel3D)this.stackingVisualization.Children[i]).Material,
                                    ((GeometryModel3D)this.stackingVisualization.Children[i]).Material);

                                // Visualizations Of The Labels Of The Boxes
                                VisualizationMethods.ReplaceVisualizationLabel(this.programVisualizationLabelsGroup, this.labelIndexes[newProgramBoxName],
                                    this.boxesOfTheProject[newProgramBoxName].visualizationLabel, newProgramBoxCenter, newProgramBoxDims, 
                                    this.boxesOfTheProject[newProgramBoxName].boxColor);

                                this.boxesOfTheProject[newProgramBoxName].boxCenter = newProgramBoxCenter;

                                this.stackingVisualization.Children.RemoveAt(i);
                                this.stackingVisualization.Children.Insert(i, programBoxVisualization);
                            }

                            // Calculating Y Value Of The Center Of The New Box
                            if (this.boxesOfTheProject[newProgramBoxName].floor == inputFloor)
                            {
                                newTargetCenterY += (float)this.stackingVisualization.Children[i].Bounds.SizeY;
                            }

                            // Add Index Of The Boxes insert Into The Dictionary
                            if (i > oldVisualizationBoxIndex) { 
                            this.boxesOfTheProject[newProgramBoxName].visualizationIndex = i - 1;
                            }
                        }
                    }

                    newTargetCenterY += targetBoxLength / 2;

                    // Generating New Visualization Box To Replace The Old Box
                    float[] programBoxDims = { float.Parse(this.ProjectWidth.Text), targetBoxLength, float.Parse(this.FloorHeight.Text) };
                    Point3D programBoxCenter = new Point3D(0, newTargetCenterY, (inputFloor * float.Parse(this.FloorHeight.Text)) + (float.Parse(this.FloorHeight.Text) / 2));
                    Material newBoxMaterial = MaterialHelper.CreateMaterial(Color.FromRgb(this.boxesOfTheProject[programBoxName].boxColor.R,
                        this.boxesOfTheProject[programBoxName].boxColor.G, this.boxesOfTheProject[programBoxName].boxColor.B));

                    GeometryModel3D newProgramBoxVisualization = VisualizationMethods.GenerateBox(programBoxName, programBoxCenter, programBoxDims, newBoxMaterial, newBoxMaterial);

                    // Visualizations Of The Labels Of The Boxes
                    VisualizationMethods.ReplaceVisualizationLabel(this.programVisualizationLabelsGroup, this.labelIndexes[programBoxName], this.boxesOfTheProject[programBoxName].visualizationLabel,
                        programBoxCenter, programBoxDims, this.boxesOfTheProject[programBoxName].boxColor);

                    this.stackingVisualization.Children.RemoveAt(oldVisualizationBoxIndex);
                    
                    if (newVisualizationBoxIndex < oldVisualizationBoxIndex)
                    {
                        newVisualizationBoxIndex = newVisualizationBoxIndex + 1;
                    }
                    this.stackingVisualization.Children.Insert(newVisualizationBoxIndex, newProgramBoxVisualization);
                    
                    // Add Index Of The Box To The Dictionary
                    this.boxesOfTheProject[programBoxName].visualizationIndex = this.stackingVisualization.Children.IndexOf(newProgramBoxVisualization);

                    this.boxesOfTheProject[programBoxName].boxCenter = programBoxCenter;
                    this.boxesOfTheProject[programBoxName].floor = inputFloor;
                }
            }
            else
            {
                MessageBox.Show("Please Enter A Number Larger Than, Or Equal To Zero.");
                programNumberTextBox.Text = this.boxesOfTheProject[programBoxName].floor.ToString();
                return;
            }
        }


        /* ########################################################### End of Handeling Events and Start of Calculations ########################################################### */

        /* ----------------------------------- The Method For All The Calculations and Visualizations of The Data ----------------------------------- */
        private void CalculationsAndOutputs(float totalGSF, float totalRawDepartmentCost)
        {
            // Calculating Total Construction Cost and Project Cost
            float circulationCost = (((float)this.CirculationSlider.Value) / 100) * totalGSF * this.functions["Circulation"]["cost"];

            float MEPCost = (((float)this.MEPSlider.Value) / 100) * totalGSF * this.functions["MEP"]["cost"];

            float exteriorStackCost = (((float)this.ExteriorStackSlider.Value) / 100) * totalGSF * this.functions["BES"]["cost"];

            this.constructionCost = totalRawDepartmentCost + circulationCost + MEPCost + exteriorStackCost +
                this.landCost + this.generalCosts + this.designContingency + this.buildContingency + this.cCIP + this.cMFee;

            this.projectCost = this.constructionCost * this.indirectMultiplier;

            // Information Outputs
            this.ConstructionCost.Text = ExtraMethods.CastDollar(this.constructionCost);
            this.ProjectCost.Text = ExtraMethods.CastDollar(this.projectCost);

            // Budget Difference
            this.budgetDifference = this.totalBudget - this.projectCost;

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


