﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Collections.Generic;
using HelixToolkit.Wpf;
using System.Windows.Media.Media3D;

namespace StackingProgrammingTool
{
    class ExtraMethods
    {
        /* ------------------------ Method for Casting Float To Dollar ------------------------ */
        public static string CastDollar(float dollar)
        {
            if (dollar >= 0)
            {
                return dollar.ToString("C0", System.Globalization.CultureInfo.CurrentCulture);
            }
            else
            {
                dollar = Math.Abs(dollar);

                return "-" + dollar.ToString("C0", System.Globalization.CultureInfo.CurrentCulture);
            }
        }

        /* ------------------------ Method For Mapping A Value ------------------------ */
        public static float MapValue(float min1, float max1, float min2, float max2, float val)
        {
            return min2 + (max2 - min2) * ((val - min1) / (max1 - min1));
        }

        /* ------------------------ Method For Generating Expander For Each Department ------------------------ */
        public static Expander DepartmentGernerator(int index)
        {
            Expander department = new Expander();
            department.Margin = new Thickness(0, 5, 0, 0);
            department.HorizontalAlignment = HorizontalAlignment.Stretch;
            department.Header = "DEPARTMENT" + " " + (index + 1).ToString();
            department.BorderBrush = Brushes.Black;
            department.Background = new SolidColorBrush(Color.FromRgb(128, 169, 237));
            department.Name = "D" + (index + 1).ToString();

            return department;
        }

        /* ------------------------ Method For Changing Color Of Labels ------------------------ */
        public static void ChangeLabelColor(Expander department, int index, byte[] color)
        {
            Label programLabel = LogicalTreeHelper.FindLogicalNode(department, department.Name + "Label" + index.ToString()) as Label;

            int mid = (Convert.ToInt32(color[0]) + Convert.ToInt32(color[1]) + Convert.ToInt32(color[2])) / 3;

            if (mid < 120)
            {
                programLabel.Foreground = Brushes.White;
            }

            programLabel.Background = new SolidColorBrush(Color.FromRgb(color[0], color[1], color[2]));

        }

        /* ------------------------ Method For Generating Elements Inisde Each Department's Expander ------------------------ */
        public static void departmentExpanderGenerator(Expander department, int numberOfProgramsInput,
            Dictionary<string, Dictionary<string, float>> functions,
            RoutedEventHandler Button_Clicked, SelectionChangedEventHandler ComboBox_SelectionChanged,
            RoutedPropertyChangedEventHandler<double> Slider_ValueChanged)
        {
            // The Main Container of the Expander
            StackPanel expanderWrapper = new StackPanel();
            expanderWrapper.Name = department.Name + "Wrapper";
            expanderWrapper.Background = Brushes.White;

            // Column Definition for Grids
            ColumnDefinition c0 = new ColumnDefinition();
            ColumnDefinition c1 = new ColumnDefinition();
            ColumnDefinition c2 = new ColumnDefinition();

            /*--- The Grid for setting up the name of the department input---*/
            Grid departmentName = new Grid();
            departmentName.Margin = new Thickness(2, 5, 2, 0);

            c0.Width = new GridLength(1, GridUnitType.Auto);
            c1.Width = new GridLength(2, GridUnitType.Star);
            c2.Width = new GridLength(1, GridUnitType.Star);

            departmentName.ColumnDefinitions.Add(c0);
            departmentName.ColumnDefinitions.Add(c1);
            departmentName.ColumnDefinitions.Add(c2);

            // Label of "Name of the Department"
            TextBlock name = new TextBlock();
            name.Text = "Name of Department";
            name.Margin = new Thickness(0, 0, 2, 0);

            // TextBox for getting the Department's Name
            TextBox nameInput = new TextBox();
            nameInput.HorizontalAlignment = HorizontalAlignment.Stretch;
            nameInput.Name = department.Name + "NameInputTextBox";
            nameInput.Padding = new Thickness(2);

            // Button for setting the Departments Name
            Button setName = new Button();
            setName.Content = "SET";
            setName.Name = department.Name + "SetNameButton";
            setName.Click += Button_Clicked;

            departmentName.Children.Add(name);
            Grid.SetColumn(name, 0);
            departmentName.Children.Add(nameInput);
            Grid.SetColumn(nameInput, 1);
            departmentName.Children.Add(setName);
            Grid.SetColumn(setName, 2);

            expanderWrapper.Children.Add(departmentName);

            /*--- The Grid for setting up the Number of Programs input ---*/
            Grid numOfPrograms = new Grid();
            numOfPrograms.Margin = new Thickness(2, 5, 2, 15);

            c0 = new ColumnDefinition();
            c1 = new ColumnDefinition();
            c2 = new ColumnDefinition();
            c0.Width = new GridLength(1, GridUnitType.Auto);
            c1.Width = new GridLength(2, GridUnitType.Star);
            c2.Width = new GridLength(1, GridUnitType.Star);

            numOfPrograms.ColumnDefinitions.Add(c0);
            numOfPrograms.ColumnDefinitions.Add(c1);
            numOfPrograms.ColumnDefinitions.Add(c2);

            // Label of "Name of the Department"
            TextBlock number = new TextBlock();
            number.Text = "Number of Programs";
            number.Margin = new Thickness(0, 0, 2, 0);

            // TextBox for getting the Department's Name
            TextBox numberInput = new TextBox();
            numberInput.Text = numberOfProgramsInput.ToString();
            numberInput.HorizontalAlignment = HorizontalAlignment.Stretch;
            numberInput.Name = department.Name + "NumberInputTextBox";
            numberInput.Padding = new Thickness(2);

            // Button for setting the Departments Name
            Button setNumber = new Button();
            setNumber.Content = "SET";
            setNumber.Name = department.Name + "SetNumberButton";
            setNumber.Click += Button_Clicked;

            numOfPrograms.Children.Add(number);
            Grid.SetColumn(number, 0);
            numOfPrograms.Children.Add(numberInput);
            Grid.SetColumn(numberInput, 1);
            numOfPrograms.Children.Add(setNumber);
            Grid.SetColumn(setNumber, 2);

            expanderWrapper.Children.Add(numOfPrograms);

            /*--- Adding the programs properties ---*/
            Grid programs = new Grid();
            programs.Name = department.Name + "Programs";

            c0 = new ColumnDefinition();
            c1 = new ColumnDefinition();
            c2 = new ColumnDefinition();
            c0.Width = new GridLength(1, GridUnitType.Star);
            c1.Width = new GridLength(1, GridUnitType.Star);
            c2.Width = new GridLength(1, GridUnitType.Star);

            programs.ColumnDefinitions.Add(c0);
            programs.ColumnDefinitions.Add(c1);
            programs.ColumnDefinitions.Add(c2);

            char[] alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

            for (int i = 0; i < numberOfProgramsInput; i++)
            {
                //Dynamically adding Rows to the Grid
                RowDefinition rowDef = new RowDefinition();
                rowDef.Height = new GridLength(1, GridUnitType.Star);
                programs.RowDefinitions.Add(rowDef);

                // Defining Dock panels for the properties of the Department Programs
                DockPanel p = new DockPanel();
                p.HorizontalAlignment = HorizontalAlignment.Stretch;
                DockPanel k = new DockPanel();
                //k.Orientation = Orientation.Horizontal;
                k.HorizontalAlignment = HorizontalAlignment.Stretch;
                k.Name = "keys";
                DockPanel r = new DockPanel();
                r.HorizontalAlignment = HorizontalAlignment.Stretch;

                // Programs
                Label programLabel = new Label();
                programLabel.Name = department.Name + "Label" + i.ToString();
                programLabel.Content = alphabet[i];
                programLabel.HorizontalAlignment = HorizontalAlignment.Stretch;
                programLabel.HorizontalContentAlignment = HorizontalAlignment.Center;
                programLabel.Margin = new Thickness(2, 5, 2, 0);
                programLabel.Width = 25;

                ComboBox program = new ComboBox();
                program.Name = department.Name + "ComboBox" + i.ToString();
                foreach (string functionName in functions.Keys)
                {
                    if (functions[functionName]["DGSFMax"] != 0 && functions[functionName]["keyMax"] != 0)
                    {
                        program.Items.Add(functionName);
                    }
                }
                program.SelectedIndex = 0;
                program.HorizontalAlignment = HorizontalAlignment.Stretch;
                program.Margin = new Thickness(0, 5, 2, 0);
                program.SelectionChanged += ComboBox_SelectionChanged;

                p.Children.Add(programLabel);
                p.Children.Add(program);

                programs.Children.Add(p);
                Grid.SetColumn(p, 0);
                Grid.SetRow(p, i);

                // Keys
                Label keyLabel = new Label();
                keyLabel.Content = "Rooms";
                Slider keyRooms = new Slider();
                keyRooms.Name = department.Name + "Rooms" + i.ToString();
                keyRooms.Minimum = functions[program.Items[0].ToString()]["keyMin"];
                keyRooms.Value = functions[program.Items[0].ToString()]["keyVal"];
                keyRooms.Maximum = functions[program.Items[0].ToString()]["keyMax"];
                keyRooms.TickFrequency = 1;
                keyRooms.IsSnapToTickEnabled = true;
                keyRooms.TickPlacement = System.Windows.Controls.Primitives.TickPlacement.BottomRight;
                keyRooms.AutoToolTipPlacement = System.Windows.Controls.Primitives.AutoToolTipPlacement.TopLeft;
                keyRooms.Margin = new Thickness(0, 5, 0, 0);
                keyRooms.ValueChanged += Slider_ValueChanged;

                k.Children.Add(keyLabel);
                k.Children.Add(keyRooms);

                programs.Children.Add(k);
                Grid.SetColumn(k, 1);
                Grid.SetRow(k, i);

                // DGSF
                Label DGSFLabel = new Label();
                DGSFLabel.Content = "DGSF";
                Slider DGSF = new Slider();
                DGSF.Name = department.Name + "DGSF" + i.ToString();
                DGSF.Minimum = functions[program.Items[0].ToString()]["DGSFMin"];
                DGSF.Value = functions[program.Items[0].ToString()]["DGSFVal"];
                DGSF.Maximum = functions[program.Items[0].ToString()]["DGSFMax"];
                DGSF.TickFrequency = 10;
                DGSF.IsSnapToTickEnabled = true;
                DGSF.TickPlacement = System.Windows.Controls.Primitives.TickPlacement.BottomRight;
                DGSF.AutoToolTipPlacement = System.Windows.Controls.Primitives.AutoToolTipPlacement.TopLeft;
                DGSF.Margin = new Thickness(0, 5, 0, 0);
                DGSF.ValueChanged += Slider_ValueChanged;

                r.Children.Add(DGSFLabel);
                r.Children.Add(DGSF);
                programs.Children.Add(r);
                Grid.SetColumn(r, 2);
                Grid.SetRow(r, i);
            }
            expanderWrapper.Children.Add(programs);

            department.Content = expanderWrapper;
        }

        /* --------------------- Method For Adding Programs To An Existing Department --------------------- */
        public static void AddProgram(Grid ppt, int count, int start, Expander department,
            Dictionary<string, Dictionary<string, float>> functions, SelectionChangedEventHandler ComboBox_SelectionChanged,
            RoutedPropertyChangedEventHandler<double> Slider_ValueChanged)
        {
            char[] alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

            for (int i = start; i < start + count; i++)
            {
                //Dynamically adding Rows to the Grid
                RowDefinition rowDef = new RowDefinition();
                rowDef.Height = new GridLength(1, GridUnitType.Star);
                ppt.RowDefinitions.Add(rowDef);

                // Defining Dock panels for the properties of the Department Programs
                DockPanel p = new DockPanel();
                p.HorizontalAlignment = HorizontalAlignment.Stretch;
                DockPanel k = new DockPanel();
                //k.Orientation = Orientation.Horizontal;
                k.HorizontalAlignment = HorizontalAlignment.Stretch;
                k.Name = "keys";
                DockPanel r = new DockPanel();
                r.HorizontalAlignment = HorizontalAlignment.Stretch;

                // Programs
                Label programLabel = new Label();
                programLabel.Name = department.Name + "Label" + (i).ToString();
                programLabel.HorizontalAlignment = HorizontalAlignment.Stretch;
                programLabel.HorizontalContentAlignment = HorizontalAlignment.Center;
                programLabel.Margin = new Thickness(2, 5, 2, 0);
                programLabel.Width = 25;

                if (i < alphabet.Length)
                {
                    programLabel.Content = alphabet[i];
                }
                else
                {
                    programLabel.Content = (i - alphabet.Length).ToString();
                }
                ComboBox program = new ComboBox();
                program.Name = department.Name + "ComboBox" + (i).ToString();
                foreach (string functionName in functions.Keys)
                {
                    if (functions[functionName]["DGSFMax"] != 0 && functions[functionName]["keyMax"] != 0)
                    {
                        program.Items.Add(functionName);
                    }
                }
                program.SelectedIndex = 0;
                program.HorizontalAlignment = HorizontalAlignment.Stretch;
                program.Margin = new Thickness(0, 5, 2, 0);
                program.SelectionChanged += ComboBox_SelectionChanged;

                p.Children.Add(programLabel);
                p.Children.Add(program);

                ppt.Children.Add(p);
                Grid.SetColumn(p, 0);
                Grid.SetRow(p, i);

                // Keys
                Label keyLabel = new Label();
                keyLabel.Content = "Rooms";
                Slider keyRooms = new Slider();
                keyRooms.Name = department.Name + "Rooms" + (i).ToString();
                keyRooms.Minimum = functions[program.Items[0].ToString()]["keyMin"];
                keyRooms.Value = functions[program.Items[0].ToString()]["keyVal"];
                keyRooms.Maximum = functions[program.Items[0].ToString()]["keyMax"];
                keyRooms.TickFrequency = 1;
                keyRooms.IsSnapToTickEnabled = true;
                keyRooms.TickPlacement = System.Windows.Controls.Primitives.TickPlacement.BottomRight;
                keyRooms.AutoToolTipPlacement = System.Windows.Controls.Primitives.AutoToolTipPlacement.TopLeft;
                keyRooms.Margin = new Thickness(0, 5, 0, 0);
                keyRooms.ValueChanged += Slider_ValueChanged;

                k.Children.Add(keyLabel);
                k.Children.Add(keyRooms);

                ppt.Children.Add(k);
                Grid.SetColumn(k, 1);
                Grid.SetRow(k, i);

                // DGSF
                Label DGSFLabel = new Label();
                DGSFLabel.Content = "DGSF";
                Slider DGSF = new Slider();
                DGSF.Name = department.Name + "DGSF" + (i).ToString();
                DGSF.Minimum = functions[program.Items[0].ToString()]["DGSFMin"];
                DGSF.Value = functions[program.Items[0].ToString()]["DGSFVal"];
                DGSF.Maximum = functions[program.Items[0].ToString()]["DGSFMax"];
                DGSF.TickFrequency = 10;
                DGSF.IsSnapToTickEnabled = true;
                DGSF.TickPlacement = System.Windows.Controls.Primitives.TickPlacement.BottomRight;
                DGSF.AutoToolTipPlacement = System.Windows.Controls.Primitives.AutoToolTipPlacement.TopLeft;
                DGSF.Margin = new Thickness(0, 5, 0, 0);
                DGSF.ValueChanged += Slider_ValueChanged;

                r.Children.Add(DGSFLabel);
                r.Children.Add(DGSF);
                ppt.Children.Add(r);
                Grid.SetColumn(r, 2);
                Grid.SetRow(r, i);
            }
        }

        /* --------------------- Method For Adding Data Of The Programs To The Program Window --------------------- */
        public static void DisplayProgramData(Dictionary<String, Box> boxes, Model3DGroup stackingVisualization,
            ProgramsSubWindow subWindow)
        {
            char[] alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

            for (int i = 0; i < stackingVisualization.Children.Count; i++)
            {
                RowDefinition gridRow = new RowDefinition();
                gridRow.Height = new GridLength(40);
                subWindow.ProgramsDataChart.RowDefinitions.Add(gridRow);

                // Project Box Is Not Included
                if (i > 0)
                {
                    string boxName = ((GeometryModel3D)stackingVisualization.Children[i]).GetName();
                    int index = int.Parse(boxName.Split('x')[1]);

                    SolidColorBrush backgroundColor = new SolidColorBrush(boxes[boxName].boxColor);
                    SolidColorBrush foregroundColor = new SolidColorBrush();

                    if ((boxes[boxName].boxColor.R + boxes[boxName].boxColor.G + boxes[boxName].boxColor.B) / 3 < 120)
                    {
                        foregroundColor = Brushes.White;
                    }
                    else
                    {
                        foregroundColor = Brushes.Black;
                    }

                    // Generate And Display Label Of Each Program
                    Label programLabel = new Label();
                    if (index < alphabet.Length)
                    {
                        programLabel.Content = alphabet[index].ToString();
                    }
                    else
                    {
                        programLabel.Content = (index - alphabet.Length).ToString();
                    }
                    programLabel.Height = 30;
                    programLabel.FontSize = 14;
                    programLabel.FontWeight = FontWeights.DemiBold;
                    programLabel.Foreground = foregroundColor;
                    programLabel.Background = backgroundColor;
                    programLabel.HorizontalContentAlignment = HorizontalAlignment.Center;
                    programLabel.VerticalContentAlignment = VerticalAlignment.Center;
                    programLabel.HorizontalAlignment = HorizontalAlignment.Stretch;
                    programLabel.VerticalAlignment = VerticalAlignment.Center;
                    Grid.SetColumn(programLabel, 0);
                    Grid.SetRow(programLabel, i);
                    subWindow.ProgramsDataChart.Children.Add(programLabel);

                    // Generate And Display Department Of Each Program
                    Label programDepartment = new Label();
                    programDepartment.Content = boxes[boxName].departmentName;
                    programDepartment.Height = 30;
                    programDepartment.FontSize = 14;
                    programDepartment.FontWeight = FontWeights.DemiBold;
                    programDepartment.Foreground = foregroundColor;
                    programDepartment.Background = backgroundColor;
                    programDepartment.HorizontalContentAlignment = HorizontalAlignment.Center;
                    programDepartment.VerticalContentAlignment = VerticalAlignment.Center;
                    programDepartment.HorizontalAlignment = HorizontalAlignment.Stretch;
                    programDepartment.VerticalAlignment = VerticalAlignment.Center;
                    Grid.SetColumn(programDepartment, 1);
                    Grid.SetRow(programDepartment, i);
                    subWindow.ProgramsDataChart.Children.Add(programDepartment);

                    // Generate And Display Function Of Each Program
                    Label programFunction = new Label();
                    programFunction.Content = boxes[boxName].function;
                    programFunction.Height = 30;
                    programFunction.FontSize = 14;
                    programFunction.FontWeight = FontWeights.DemiBold;
                    programFunction.Foreground = foregroundColor;
                    programFunction.Background = backgroundColor;
                    programFunction.HorizontalContentAlignment = HorizontalAlignment.Center;
                    programFunction.VerticalContentAlignment = VerticalAlignment.Center;
                    programFunction.HorizontalAlignment = HorizontalAlignment.Stretch;
                    programFunction.VerticalAlignment = VerticalAlignment.Center;
                    Grid.SetColumn(programFunction, 2);
                    Grid.SetRow(programFunction, i);
                    subWindow.ProgramsDataChart.Children.Add(programFunction);

                    // Generate And Display Floor Of Each Program
                    Label programFloor = new Label();
                    programFloor.Content = boxes[boxName].floor;
                    programFloor.Height = 30;
                    programFloor.FontSize = 14;
                    programFloor.FontWeight = FontWeights.DemiBold;
                    programFloor.Foreground = foregroundColor;
                    programFloor.Background = backgroundColor;
                    programFloor.HorizontalContentAlignment = HorizontalAlignment.Center;
                    programFloor.VerticalContentAlignment = VerticalAlignment.Center;
                    programFloor.HorizontalAlignment = HorizontalAlignment.Stretch;
                    programFloor.VerticalAlignment = VerticalAlignment.Center;
                    Grid.SetColumn(programFloor, 3);
                    Grid.SetRow(programFloor, i);
                    subWindow.ProgramsDataChart.Children.Add(programFloor);

                    // Generate And Display Total Program GSF Of Each Program
                    Label programTotalGSF = new Label();
                    programTotalGSF.Content = boxes[boxName].boxTotalGSFValue.ToString("C0",
                        System.Globalization.CultureInfo.CurrentCulture).Remove(0, 1);
                    programTotalGSF.Height = 30;
                    programTotalGSF.FontSize = 14;
                    programTotalGSF.FontWeight = FontWeights.DemiBold;
                    programTotalGSF.Foreground = foregroundColor;
                    programTotalGSF.Background = backgroundColor;
                    programTotalGSF.HorizontalContentAlignment = HorizontalAlignment.Center;
                    programTotalGSF.VerticalContentAlignment = VerticalAlignment.Center;
                    programTotalGSF.HorizontalAlignment = HorizontalAlignment.Stretch;
                    programTotalGSF.VerticalAlignment = VerticalAlignment.Center;
                    Grid.SetColumn(programTotalGSF, 4);
                    Grid.SetRow(programTotalGSF, i);
                    subWindow.ProgramsDataChart.Children.Add(programTotalGSF);

                    // Generate And Display Cost Per GSF Of Each Program
                    Label costPerGSF = new Label();
                    costPerGSF.Content = ExtraMethods.CastDollar(boxes[boxName].cost);
                    costPerGSF.Height = 30;
                    costPerGSF.FontSize = 14;
                    costPerGSF.FontWeight = FontWeights.DemiBold;
                    costPerGSF.Foreground = foregroundColor;
                    costPerGSF.Background = backgroundColor;
                    costPerGSF.HorizontalContentAlignment = HorizontalAlignment.Center;
                    costPerGSF.VerticalContentAlignment = VerticalAlignment.Center;
                    costPerGSF.HorizontalAlignment = HorizontalAlignment.Stretch;
                    costPerGSF.VerticalAlignment = VerticalAlignment.Center;
                    Grid.SetColumn(costPerGSF, 5);
                    Grid.SetRow(costPerGSF, i);
                    subWindow.ProgramsDataChart.Children.Add(costPerGSF);

                    // Generate And Display Total Raw Cost Program
                    Label programRawCost = new Label();
                    programRawCost.Content = ExtraMethods.CastDollar(boxes[boxName].totalRawCostValue);
                    programRawCost.Height = 30;
                    programRawCost.FontSize = 14;
                    programRawCost.FontWeight = FontWeights.DemiBold;
                    programRawCost.Foreground = foregroundColor;
                    programRawCost.Background = backgroundColor;
                    programRawCost.HorizontalContentAlignment = HorizontalAlignment.Center;
                    programRawCost.VerticalContentAlignment = VerticalAlignment.Center;
                    programRawCost.HorizontalAlignment = HorizontalAlignment.Stretch;
                    programRawCost.VerticalAlignment = VerticalAlignment.Center;
                    Grid.SetColumn(programRawCost, 6);
                    Grid.SetRow(programRawCost, i);
                    subWindow.ProgramsDataChart.Children.Add(programRawCost);
                }
            }
        }

        /* --------------------- Method For Generating And Displaying Stacking Controllers --------------------- */
        public static void GenerateProgramsStacking(Dictionary<String, Box> boxes, Model3DGroup stackingVisualization,
            Grid programsStackingGrid, RoutedEventHandler Button_Clicked)
        {
            // Clear Rows Of The Grid
            programsStackingGrid.Children.Clear();
            programsStackingGrid.RowDefinitions.Clear();
            
            char[] alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
            
            for (int i = 0; i < stackingVisualization.Children.Count; i++)
            {
                RowDefinition gridRow = new RowDefinition();
                gridRow.Height = new GridLength(1, GridUnitType.Auto);
                programsStackingGrid.RowDefinitions.Add(gridRow);

                // Project Box Is Not Included
                if (i > 0)
                {
                    string boxName = ((GeometryModel3D)stackingVisualization.Children[i]).GetName();
                    int index = int.Parse(boxName.Split('x')[1]);
                   
                    SolidColorBrush backgroundColor = new SolidColorBrush(boxes[boxName].boxColor);
                    SolidColorBrush foregroundColor = new SolidColorBrush();

                    if ((boxes[boxName].boxColor.R + boxes[boxName].boxColor.G + boxes[boxName].boxColor.B) / 3 < 120)
                    {
                        foregroundColor = Brushes.White;
                    }
                    else
                    {
                        foregroundColor = Brushes.Black;
                    }

                    // Generate And Display Label Of Each Program
                    Label programLabel = new Label();
                    if (index < alphabet.Length)
                    {
                        programLabel.Content = alphabet[index].ToString();
                    }
                    else
                    {
                        programLabel.Content = (index - alphabet.Length).ToString();
                    }
                    programLabel.Width = 30;
                    programLabel.Height = 30;
                    programLabel.FontSize = 14;
                    programLabel.Margin = new Thickness(0, 0, 0, 5);
                    programLabel.FontWeight = FontWeights.DemiBold;
                    programLabel.Foreground = foregroundColor;
                    programLabel.Background = backgroundColor;
                    programLabel.HorizontalContentAlignment = HorizontalAlignment.Center;
                    programLabel.VerticalContentAlignment = VerticalAlignment.Center;
                    programLabel.HorizontalAlignment = HorizontalAlignment.Stretch;
                    programLabel.VerticalAlignment = VerticalAlignment.Center;
                    Grid.SetColumn(programLabel, 0);
                    Grid.SetRow(programLabel, i - 1);
                    programsStackingGrid.Children.Add(programLabel);
                    
                    // Generate And Display Text Box For Each Program
                    TextBox programFloor = new TextBox();
                    programFloor.Height = 30;
                    programFloor.FontSize = 14;
                    programFloor.Margin = new Thickness(0, 0, 0, 5);
                    programFloor.Text = boxes[boxName].floor.ToString();
                    programFloor.Name = boxName + "TextBox";
                    programFloor.HorizontalAlignment = HorizontalAlignment.Stretch;
                    programFloor.VerticalAlignment = VerticalAlignment.Center;
                    programFloor.VerticalContentAlignment = VerticalAlignment.Center;
                    programFloor.Padding = new Thickness(2);
                    Grid.SetColumn(programFloor, 1);
                    Grid.SetRow(programFloor, i - 1);
                    programsStackingGrid.Children.Add(programFloor);

                    // Generate And Display Button For Each Program
                    Button setFloor = new Button();
                    setFloor.Height = 30;
                    setFloor.FontSize = 14;
                    setFloor.Margin = new Thickness(0, 0, 0, 5);
                    setFloor.Content = "SET";
                    setFloor.Name = boxName + "SetButton";
                    setFloor.Click += Button_Clicked;
                    setFloor.HorizontalAlignment = HorizontalAlignment.Stretch;     
                    setFloor.VerticalContentAlignment = VerticalAlignment.Center;
                    Grid.SetColumn(setFloor, 2);
                    Grid.SetRow(setFloor, i - 1);
                    programsStackingGrid.Children.Add(setFloor);
                }
            }
        }
    }
}
