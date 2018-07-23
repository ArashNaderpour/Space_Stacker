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
    /// Interaction logic for ModifyProgramDataWindow.xaml
    /// </summary>
    public partial class ModifyProgramDataWindow : Window
    {
        public ModifyProgramDataWindow()
        {
            InitializeComponent();

            foreach (string key in MainWindow.functions.Keys) {
                // Initialize And Illustrate MEP
                if (key == "MEP") { 
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
                    countRange.Text = (MainWindow.functions[key]["keyMax"] - MainWindow.functions[key]["keyMin"]).ToString();
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
                    grossRange.Text = (MainWindow.functions[key]["DGSFMax"] - MainWindow.functions[key]["DGSFMin"]).ToString();
                    grossRange.Margin = new Thickness(2.5, 0, 0, 10);
                    grossRange.Padding = new Thickness(2);
                    grossRange.VerticalAlignment = VerticalAlignment.Center;
                    this.ProgramsDataChart.Children.Add(grossRange);
                    Grid.SetColumn(grossRange, 5);
                    Grid.SetRow(grossRange, this.ProgramsDataChart.RowDefinitions.Count - 1);
                }
            }


        }


    }
}
