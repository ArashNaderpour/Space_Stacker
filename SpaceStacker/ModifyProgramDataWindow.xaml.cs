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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SpaceStacker
{
    /// <summary>
    /// Interaction logic for ModifyProgramDataWindow.xaml
    /// </summary>
    public partial class ModifyProgramDataWindow : Window
    {
        // Store Initial Keys In A Seperate List
        List<string> listOfKeys = new List<string>();

        public ModifyProgramDataWindow()
        {
            InitializeComponent();

            foreach (string key in MainWindow.functions.Keys)
            {
                // Initialize And Illustrate MEP
                if (key == "MEP")
                {
                    this.MEPCost.Text = ExtraMethods.CastDollar(MainWindow.functions["MEP"]["cost"]);
                }

                // Initialize And Illustrate Circulatio
                else if (key == "Circulation")
                {
                    this.CirculationCost.Text = ExtraMethods.CastDollar(MainWindow.functions["Circulation"]["cost"]);
                }

                // Initialize And Illustrate BES
                else if (key == "BES")
                {
                    this.BESCost.Text = ExtraMethods.CastDollar(MainWindow.functions["BES"]["cost"]);
                }

                // Other Inputs
                else
                {
                    string addedProgramDataIndex = (this.ProgramsDataChart.RowDefinitions.Count - 1).ToString();

                    RowDefinition gridRow = new RowDefinition();
                    this.ProgramsDataChart.RowDefinitions.Add(gridRow);

                    TextBox programName = new TextBox();
                    programName.Name = "ProgramName" + addedProgramDataIndex;
                    programName.Text = key;
                    // Store Function Names In The Sequence Of Rows
                    this.listOfKeys.Add(programName.Text);
                    programName.Margin = new Thickness(0, 0, 2.5, 10);
                    programName.Padding = new Thickness(2);
                    programName.VerticalAlignment = VerticalAlignment.Center;
                    this.ProgramsDataChart.Children.Add(programName);
                    Grid.SetColumn(programName, 0);
                    Grid.SetRow(programName, this.ProgramsDataChart.RowDefinitions.Count - 1);

                    TextBox programCost = new TextBox();
                    programCost.Name = "ProgramCost" + addedProgramDataIndex;
                    programCost.Text = ExtraMethods.CastDollar(MainWindow.functions[key]["cost"]);
                    programCost.Margin = new Thickness(2.5, 0, 2.5, 10);
                    programCost.Padding = new Thickness(2);
                    programCost.VerticalAlignment = VerticalAlignment.Center;
                    this.ProgramsDataChart.Children.Add(programCost);
                    Grid.SetColumn(programCost, 1);
                    Grid.SetRow(programCost, this.ProgramsDataChart.RowDefinitions.Count - 1);

                    TextBox initialCount = new TextBox();
                    initialCount.Name = "ProgramInitialCount" + addedProgramDataIndex;
                    initialCount.Text = MainWindow.functions[key]["keyVal"].ToString();
                    initialCount.Margin = new Thickness(2.5, 0, 2.5, 10);
                    initialCount.Padding = new Thickness(2);
                    initialCount.VerticalAlignment = VerticalAlignment.Center;
                    this.ProgramsDataChart.Children.Add(initialCount);
                    Grid.SetColumn(initialCount, 2);
                    Grid.SetRow(initialCount, this.ProgramsDataChart.RowDefinitions.Count - 1);

                    TextBox countRange = new TextBox();
                    countRange.Name = "ProgramCountRange" + addedProgramDataIndex;
                    countRange.Text = ((MainWindow.functions[key]["keyMax"] - MainWindow.functions[key]["keyVal"]) * 2).ToString();
                    countRange.Margin = new Thickness(2.5, 0, 2.5, 10);
                    countRange.Padding = new Thickness(2);
                    countRange.VerticalAlignment = VerticalAlignment.Center;
                    this.ProgramsDataChart.Children.Add(countRange);
                    Grid.SetColumn(countRange, 3);
                    Grid.SetRow(countRange, this.ProgramsDataChart.RowDefinitions.Count - 1);

                    TextBox initialGross = new TextBox();
                    initialGross.Name = "ProgramInitialGross" + addedProgramDataIndex;
                    initialGross.Text = MainWindow.functions[key]["DGSFVal"].ToString();
                    initialGross.Margin = new Thickness(2.5, 0, 2.5, 10);
                    initialGross.Padding = new Thickness(2);
                    initialGross.VerticalAlignment = VerticalAlignment.Center;
                    this.ProgramsDataChart.Children.Add(initialGross);
                    Grid.SetColumn(initialGross, 4);
                    Grid.SetRow(initialGross, this.ProgramsDataChart.RowDefinitions.Count - 1);

                    TextBox grossRange = new TextBox();
                    grossRange.Name = "ProgramGrossRange" + addedProgramDataIndex;
                    grossRange.Text = ((MainWindow.functions[key]["DGSFMax"] - MainWindow.functions[key]["DGSFVal"]) * 2).ToString();
                    grossRange.Margin = new Thickness(2.5, 0, 0, 10);
                    grossRange.Padding = new Thickness(2);
                    grossRange.VerticalAlignment = VerticalAlignment.Center;
                    this.ProgramsDataChart.Children.Add(grossRange);
                    Grid.SetColumn(grossRange, 5);
                    Grid.SetRow(grossRange, this.ProgramsDataChart.RowDefinitions.Count - 1);
                }
            }
        }

        /*---------------- Handeling Modify Program Data Event ----------------*/
        private void ModifyInputData_Click(object sender, RoutedEventArgs e)
        {
            // Constant Parameters
            MainWindow.functions["MEP"]["cost"] = float.Parse(this.MEPCost.Text.Replace("$", "").Replace(",", ""));
            MainWindow.functions["Circulation"]["cost"] = float.Parse(this.CirculationCost.Text.Replace("$", "").Replace(",", ""));
            MainWindow.functions["BES"]["cost"] = float.Parse(this.BESCost.Text.Replace("$", "").Replace(",", ""));

            // Other Functionalities
            for (int i = 4; i < this.ProgramsDataChart.RowDefinitions.Count; i++)
            {
                string functionName = "";

                foreach (UIElement element in this.ProgramsDataChart.Children)
                {
                    if (Grid.GetRow(element) >= 4)
                    {
                        TextBox textBox = element as TextBox;

                        if (Grid.GetColumn(textBox) == 0 && Grid.GetRow(textBox) == i)
                        {
                            functionName = textBox.Text;

                            // Change Function Name Of An Existing Funtion
                            if (i - 4 < this.listOfKeys.Count)
                            {
                                Dictionary<string, float> tempDict = MainWindow.functions[this.listOfKeys[i - 4]];

                                MainWindow.functions.Remove(this.listOfKeys[i - 4]);

                                MainWindow.functions.Add(functionName, tempDict);
                            }

                            // New Function Added To The Input Data
                            else
                            {
                                Dictionary<string, float> parameters = new Dictionary<string, float>();
                                parameters.Add("cost", 0);
                                parameters.Add("keyMin", 0);
                                parameters.Add("keyMax", 0);
                                parameters.Add("keyVal", 0);
                                parameters.Add("DGSFMin", 0);
                                parameters.Add("DGSFMax", 0);
                                parameters.Add("DGSFVal", 0);

                                MainWindow.functions.Add(functionName, parameters);
                            }
                        }

                        // Cost Value
                        if (Grid.GetColumn(textBox) == 1 && Grid.GetRow(textBox) == i)
                        {
                            float value;

                            try
                            {
                                value = float.Parse(textBox.Text.Replace("$", "").Replace(",", ""));
                            }
                            catch
                            {
                                MessageBox.Show("All The \"Cost\" Cells Must Have Number Values.");
                                return;
                            }

                            if (value >= 0)
                            {
                                MainWindow.functions[functionName]["cost"] = value;
                            }
                            else
                            {
                                MessageBox.Show("All The \"Cost\" Cells Must Have Positive Number Values.");
                                return;
                            }
                        }

                        // Initial Count Value
                        if (Grid.GetColumn(textBox) == 2 && Grid.GetRow(textBox) == i)
                        {
                            float value;
                            float range = MainWindow.functions[functionName]["keyMax"] - MainWindow.functions[functionName]["keyVal"];

                            try
                            {
                                value = float.Parse(textBox.Text);
                            }
                            catch
                            {
                                MessageBox.Show("All The \"Initial Count\" Must Have Number Values.");
                                return;
                            }

                            if (value >= 0)
                            {
                                if (value > range)
                                {
                                    MainWindow.functions[functionName]["keyMin"] = 0;
                                }
                                else
                                {
                                    MainWindow.functions[functionName]["keyMin"] = value - range;
                                }

                                MainWindow.functions[functionName]["keyMax"] += value + range;

                                MainWindow.functions[functionName]["keyVal"] = value;
                            }
                            else
                            {
                                MessageBox.Show("All The \"Initial Count\" Must Have Positive Number Values.");
                                return;
                            }
                        }

                        // Count Range Value
                        if (Grid.GetColumn(textBox) == 3 && Grid.GetRow(textBox) == i)
                        {
                            float value;

                            try
                            {
                                value = (float)Math.Floor(double.Parse(textBox.Text) / 2);
                            }
                            catch
                            {
                                MessageBox.Show("All The \"Count Range\" Must Have Number Values.");
                                return;
                            }

                            if (value > 0)
                            {
                                if (MainWindow.functions[functionName]["keyVal"] - value <= 0)
                                {
                                    MainWindow.functions[functionName]["keyMin"] = 0;
                                }
                                else
                                {
                                    MainWindow.functions[functionName]["keyMin"] = MainWindow.functions[functionName]["keyVal"] - value;
                                }
                                MainWindow.functions[functionName]["keyMax"] = MainWindow.functions[functionName]["keyVal"] + value;
                            }
                            else
                            {
                                MessageBox.Show("All The \"Count Range\" Must Have Positive Number Values, Larger Than Zero.");
                                return;
                            }
                        }

                        // Initial Gross Value
                        if (Grid.GetColumn(textBox) == 4 && Grid.GetRow(textBox) == i)
                        {
                            float value;
                            float range = MainWindow.functions[functionName]["DGSFMax"] - MainWindow.functions[functionName]["DGSFVal"];

                            try
                            {
                                value = float.Parse(textBox.Text);
                            }
                            catch
                            {
                                MessageBox.Show("All The \"Initial Gross\" Must Have Number Values.");
                                return;
                            }

                            if (value >= 0)
                            {
                                if (value > range)
                                {
                                    MainWindow.functions[functionName]["DGSFMin"] = 0;
                                }
                                else
                                {
                                    MainWindow.functions[functionName]["DGSFMin"] = value - range;
                                }

                                MainWindow.functions[functionName]["DGSFMax"] += value + range;

                                MainWindow.functions[functionName]["DGSFVal"] = value;
                            }
                            else
                            {
                                MessageBox.Show("All The \"Initial Gross\" Must Have Positive Number Values.");
                                return;
                            }
                        }

                        // Gross Range Value
                        if (Grid.GetColumn(textBox) == 5 && Grid.GetRow(textBox) == i)
                        {
                            float value;

                            try
                            {
                                value = (float) Math.Floor(double.Parse(textBox.Text)/2);
                            }
                            catch
                            {
                                MessageBox.Show("All The \"Gross Range\" Must Have Number Values.");
                                return;
                            }

                            if (value > 0)
                            {
                                if (MainWindow.functions[functionName]["DGSFVal"] - value <= 0)
                                {
                                    MainWindow.functions[functionName]["DGSFMin"] = 0;
                                }
                                else
                                {
                                    MainWindow.functions[functionName]["DGSFMin"] = MainWindow.functions[functionName]["DGSFVal"] - value;
                                }
                                MainWindow.functions[functionName]["DGSFMax"] = MainWindow.functions[functionName]["DGSFVal"] + value;
                            }
                            else
                            {
                                MessageBox.Show("All The \"Gross Range\"Must Have Positive Number Values, Larger Than Zero.");
                                return;
                            }
                        }
                    }
                }
            }

            this.Close();
            ((MainWindow)this.Owner).ModifyInputs();
        }

        /*---------------- Handeling Add Program Data Event ----------------*/
        private void AddProgramData_Click(object sender, RoutedEventArgs e)
        {
            string addedProgramDataIndex = (this.ProgramsDataChart.RowDefinitions.Count - 1).ToString();

            RowDefinition gridRow = new RowDefinition();
            this.ProgramsDataChart.RowDefinitions.Add(gridRow);

            TextBox programName = new TextBox();
            programName.Name = "ProgramName" + addedProgramDataIndex;
            programName.Margin = new Thickness(0, 0, 2.5, 10);
            programName.Padding = new Thickness(2);
            programName.VerticalAlignment = VerticalAlignment.Center;
            this.ProgramsDataChart.Children.Add(programName);
            Grid.SetColumn(programName, 0);
            Grid.SetRow(programName, this.ProgramsDataChart.RowDefinitions.Count - 1);

            TextBox programCost = new TextBox();
            programCost.Name = "ProgramCost" + addedProgramDataIndex;
            programCost.Margin = new Thickness(2.5, 0, 2.5, 10);
            programCost.Padding = new Thickness(2);
            programCost.VerticalAlignment = VerticalAlignment.Center;
            this.ProgramsDataChart.Children.Add(programCost);
            Grid.SetColumn(programCost, 1);
            Grid.SetRow(programCost, this.ProgramsDataChart.RowDefinitions.Count - 1);

            TextBox initialCount = new TextBox();
            initialCount.Name = "ProgramInitialCount" + addedProgramDataIndex;
            initialCount.Margin = new Thickness(2.5, 0, 2.5, 10);
            initialCount.Padding = new Thickness(2);
            initialCount.VerticalAlignment = VerticalAlignment.Center;
            this.ProgramsDataChart.Children.Add(initialCount);
            Grid.SetColumn(initialCount, 2);
            Grid.SetRow(initialCount, this.ProgramsDataChart.RowDefinitions.Count - 1);

            TextBox countRange = new TextBox();
            countRange.Name = "ProgramCountRange" + addedProgramDataIndex;
            countRange.Margin = new Thickness(2.5, 0, 2.5, 10);
            countRange.Padding = new Thickness(2);
            countRange.VerticalAlignment = VerticalAlignment.Center;
            this.ProgramsDataChart.Children.Add(countRange);
            Grid.SetColumn(countRange, 3);
            Grid.SetRow(countRange, this.ProgramsDataChart.RowDefinitions.Count - 1);

            TextBox initialGross = new TextBox();
            initialGross.Name = "ProgramInitialGross" + addedProgramDataIndex;
            initialGross.Margin = new Thickness(2.5, 0, 2.5, 10);
            initialGross.Padding = new Thickness(2);
            initialGross.VerticalAlignment = VerticalAlignment.Center;
            this.ProgramsDataChart.Children.Add(initialGross);
            Grid.SetColumn(initialGross, 4);
            Grid.SetRow(initialGross, this.ProgramsDataChart.RowDefinitions.Count - 1);

            TextBox grossRange = new TextBox();
            grossRange.Name = "ProgramGrossRange" + addedProgramDataIndex;
            grossRange.Margin = new Thickness(2.5, 0, 0, 10);
            grossRange.Padding = new Thickness(2);
            grossRange.VerticalAlignment = VerticalAlignment.Center;
            this.ProgramsDataChart.Children.Add(grossRange);
            Grid.SetColumn(grossRange, 5);
            Grid.SetRow(grossRange, this.ProgramsDataChart.RowDefinitions.Count - 1);
        }

        /*---------------- Handeling Remove Program Data Event ----------------*/
        private void RemoveProgramData_Click(object sender, RoutedEventArgs e)
        {
            // A List To Store UI Elements To Remove From The Controller Window
            List<UIElement> elementsToRemove = new List<UIElement>();

            if (this.ProgramsDataChart.RowDefinitions.Count > 4)
            {
                // Removing UI Elemets From The Controller Window
                foreach (UIElement element in this.ProgramsDataChart.Children)
                {
                    if (Grid.GetRow(element) == this.ProgramsDataChart.RowDefinitions.Count - 1)
                    {
                        elementsToRemove.Add(element);
                    }
                }
                foreach (UIElement element in elementsToRemove)
                {
                    this.ProgramsDataChart.Children.Remove(element);
                }

                this.ProgramsDataChart.RowDefinitions.RemoveAt(this.ProgramsDataChart.RowDefinitions.Count - 1);
                elementsToRemove.Clear();
            }
            else
            {
                return;
            }
        }
    }
}
