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
            programName.Margin = new Thickness(0, 0, 2.5, 5);
            programName.Padding = new Thickness(2);
            this.ProgramsDataChart.Children.Add(programName);
            Grid.SetColumn(programName, 0);
            Grid.SetRow(programName, this.ProgramsDataChart.RowDefinitions.Count - 1);

            TextBox programCost = new TextBox();
            programCost.Name = "ProgramCost" + addedProgramDataIndex;
            programCost.Margin = new Thickness(2.5, 0, 2.5, 5);
            programCost.Padding = new Thickness(2);
            this.ProgramsDataChart.Children.Add(programCost);
            Grid.SetColumn(programCost, 1);
            Grid.SetRow(programCost, this.ProgramsDataChart.RowDefinitions.Count - 1);

            TextBox initialCount = new TextBox();
            initialCount.Name = "ProgramInitialCount" + addedProgramDataIndex;
            initialCount.Margin = new Thickness(2.5, 0, 2.5, 5);
            initialCount.Padding = new Thickness(2);
            this.ProgramsDataChart.Children.Add(initialCount);
            Grid.SetColumn(initialCount, 2);
            Grid.SetRow(initialCount, this.ProgramsDataChart.RowDefinitions.Count - 1);

            TextBox countRange = new TextBox();
            countRange.Name = "ProgramCountRange" + addedProgramDataIndex;
            countRange.Margin = new Thickness(2.5, 0, 2.5, 5);
            countRange.Padding = new Thickness(2);
            this.ProgramsDataChart.Children.Add(countRange);
            Grid.SetColumn(countRange, 3);
            Grid.SetRow(countRange, this.ProgramsDataChart.RowDefinitions.Count - 1);

            TextBox initialGross = new TextBox();
            initialGross.Name = "ProgramInitialGross" + addedProgramDataIndex;
            initialGross.Margin = new Thickness(2.5, 0, 2.5, 5);
            initialGross.Padding = new Thickness(2);
            this.ProgramsDataChart.Children.Add(initialGross);
            Grid.SetColumn(initialGross, 4);
            Grid.SetRow(initialGross, this.ProgramsDataChart.RowDefinitions.Count - 1);

            TextBox grossRange = new TextBox();
            grossRange.Name = "ProgramGrossRange" + addedProgramDataIndex;
            grossRange.Margin = new Thickness(2.5, 0, 0, 5);
            grossRange.Padding = new Thickness(2);
            this.ProgramsDataChart.Children.Add(grossRange);
            Grid.SetColumn(grossRange, 5);
            Grid.SetRow(grossRange, this.ProgramsDataChart.RowDefinitions.Count - 1);
        }

        /*---------------- Handeling RemoveProgramData Event ----------------*/
        private void RemoveProgramData_Click(object sender, RoutedEventArgs e)
        {
            // A List To Store UI Elements To Remove From The Controller Window
            List<UIElement> elementsToRemove = new List<UIElement>();

            if (this.ProgramsDataChart.RowDefinitions.Count > 1) {
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

        }
    }
}
