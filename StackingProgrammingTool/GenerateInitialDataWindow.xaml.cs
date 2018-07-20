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

namespace StackingProgrammingTool
{
    /// <summary>
    /// Interaction logic for GenerateInitialDataWindow.xaml
    /// </summary>
    public partial class GenerateInitialDataWindow : Window
    {
        public static bool generateDataError = false;
        public static bool dataWindow = false;
        public Button generateDataButton = new Button();

        public GenerateInitialDataWindow()
        {
            InitializeComponent();
        }

        /*---------------- Handeling AddProgramData Event ----------------*/
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

        /*---------------- Handeling RemoveProgramData Event ----------------*/
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

        /*---------------- Handeling RemoveProgramData Event ----------------*/
        private void GenerateProgramData_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.functions.Clear();

            for (int i = 1; i < this.ProgramsDataChart.RowDefinitions.Count; i++)
            {
                Dictionary<String, float> tempDictionary = new Dictionary<String, float>();

                string key = "";

                foreach (UIElement element in this.ProgramsDataChart.Children)
                {
                    if (i < 4)
                    {

                        if (Grid.GetColumn(element) == 0 && Grid.GetRow(element) == i)
                        {
                            Label label = element as Label;

                            key = label.Content.ToString();

                            MainWindow.functions.Add(key, null);

                            tempDictionary.Add("keyMin", 0);
                            tempDictionary.Add("keyVal", 0);
                            tempDictionary.Add("keyMax", 0);
                            tempDictionary.Add("DGSFMin", 0);
                            tempDictionary.Add("DGSFVal", 0);
                            tempDictionary.Add("DGSFMax", 0);

                        }
                        if (Grid.GetColumn(element) > 0 && Grid.GetRow(element) == i)
                        {
                            TextBox textBox = element as TextBox;

                            float value;

                            try
                            {
                                value = float.Parse(textBox.Text);
                            }
                            catch
                            {
                                MessageBox.Show("\"Cost\" Has To Have A Number Value.");
                                generateDataError = true;
                                return;
                            }

                            if (value >= 0)
                            {
                                tempDictionary.Add("cost", value);
                            }
                            else
                            {
                                MessageBox.Show("\"Cost\" Has To Have A Positive Number Value.");
                                generateDataError = true;
                                return;
                            }
                        }
                    }
                    else
                    {
                        if (Grid.GetColumn(element) == 0 && Grid.GetRow(element) == i)
                        {
                            TextBox textBox = element as TextBox;

                            key = textBox.Text;

                            if (key.Replace(" ", string.Empty).Length > 0 && key.Length > 0)
                            {
                                MainWindow.functions.Add(key, null);
                            }
                            else
                            {
                                MessageBox.Show("\"Name\" Has To Have A Value");
                                generateDataError = true;
                                return;
                            }
                        }
                        if (Grid.GetColumn(element) == 1 && Grid.GetRow(element) == i)
                        {
                            TextBox textBox = element as TextBox;

                            float value;

                            try
                            {
                                value = float.Parse(textBox.Text);
                            }
                            catch
                            {
                                MessageBox.Show("\"Cost\" Has To Have A Number Value.");
                                generateDataError = true;
                                return;
                            }
                            if (value >= 0)
                            {
                                tempDictionary.Add("cost", value);
                            }
                            else
                            {
                                MessageBox.Show("\"Cost\" Has To Have A Positive Number Value.");
                                generateDataError = true;
                                return;
                            }
                        }

                        if (Grid.GetColumn(element) == 2 && Grid.GetRow(element) == i)
                        {
                            TextBox textBox = element as TextBox;

                            int value;

                            try
                            {
                                value = int.Parse(textBox.Text);
                            }
                            catch
                            {
                                MessageBox.Show("\"Initial Count\" Has To Have A Number Value.");
                                generateDataError = true;
                                return;
                            }
                            if (value >= 0)
                            {
                                tempDictionary.Add("keyVal", (float)value);
                            }
                            else
                            {
                                MessageBox.Show("\"Initial Count\" Has To Have A Positive Number Value.");
                                generateDataError = true;
                                return;
                            }
                        }

                        if (Grid.GetColumn(element) == 3 && Grid.GetRow(element) == i)
                        {
                            TextBox textBox = element as TextBox;

                            int value;

                            try
                            {
                                value = int.Parse(textBox.Text);
                            }
                            catch
                            {
                                MessageBox.Show("\"Count Range\" Has To Have A Number Value.");
                                generateDataError = true;
                                return;
                            }
                            if (value >= 0)
                            {
                                float keyMin;

                                if ((tempDictionary["keyVal"] - ((int)value / 2) > 0))
                                {
                                    keyMin = (float)(tempDictionary["keyVal"] - ((int)value / 2));
                                }
                                else
                                {
                                    keyMin = 0;
                                }
                                
                                float keyMax = (float)(tempDictionary["keyVal"] + ((int)value / 2));

                                tempDictionary.Add("keyMin", keyMin);
                                tempDictionary.Add("keyMax", keyMax);
                            }
                            else
                            {
                                MessageBox.Show("\"Count Range\" Has To Have A Positive Number Value.");
                                generateDataError = true;
                                return;
                            }
                        }

                        if (Grid.GetColumn(element) == 4 && Grid.GetRow(element) == i)
                        {
                            TextBox textBox = element as TextBox;

                            int value;

                            try
                            {
                                value = int.Parse(textBox.Text);
                            }
                            catch
                            {
                                MessageBox.Show("\"Initial Gross\" Has To Have A Number Value.");
                                generateDataError = true;
                                return;
                            }
                            if (value >= 0)
                            {
                                tempDictionary.Add("DGSFVal", (float)value);
                            }
                            else
                            {
                                MessageBox.Show("\"Initial Gross\" Has To Have A Positive Number Value.");
                                generateDataError = true;
                                return;
                            }
                        }

                        if (Grid.GetColumn(element) == 5 && Grid.GetRow(element) == i)
                        {
                            TextBox textBox = element as TextBox;

                            int value;

                            try
                            {
                                value = int.Parse(textBox.Text);
                            }
                            catch
                            {
                                MessageBox.Show("\"Gross Range\" Has To Have A Number Value.");
                                generateDataError = true;
                                return;
                            }
                            if (value >= 0)
                            {
                                float keyMin;

                                if ((tempDictionary["DGSFVal"] - ((int)value / 2)) > 0)
                                {
                                    keyMin = (float)(tempDictionary["DGSFVal"] - ((int)value / 2));
                                }
                                else
                                {
                                    keyMin = 0;
                                }

                                float keyMax = (float)(tempDictionary["DGSFVal"] + ((int)value / 2));

                                tempDictionary.Add("DGSFMin", keyMin);
                                tempDictionary.Add("DGSFMax", keyMax);
                            }
                            else
                            {
                                MessageBox.Show("\"Gross Range\" Has To Have A Positive Number Value.");
                                generateDataError = true;
                                return;
                            }
                        }
                    }
                }

                MainWindow.functions[key] = tempDictionary;
            }
            
            dataWindow = true;
            this.generateDataButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            this.Close();
        }
    }
}
