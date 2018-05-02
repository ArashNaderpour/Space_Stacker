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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Massing_Programming
{
    class ExtraMethods
    {
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

        public static void departmentExpanderGenerator(Expander department, int numberOfProgramsInput, RoutedEventHandler ButtonClicked)
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

            // Button for setting the Departments Name
            Button setName = new Button();
            setName.Content = "SET";
            setName.Name = department.Name + "SetNameButton";
            setName.Click += ButtonClicked;

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

            // Button for setting the Departments Name
            Button setNumber = new Button();
            setNumber.Content = "SET";
            setNumber.Name = department.Name + "SetNumberButton";
            setNumber.Click += ButtonClicked;

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
                programLabel.Content = alphabet[i];
                programLabel.HorizontalAlignment = HorizontalAlignment.Stretch;
                programLabel.Width = 25;
                ComboBox program = new ComboBox();
                program.Items.Add("Arash");
                program.SelectedIndex = 0;
                program.HorizontalAlignment = HorizontalAlignment.Stretch;
                program.Margin = new Thickness(0, 5, 2, 0);

                p.Children.Add(programLabel);
                p.Children.Add(program);

                programs.Children.Add(p);
                Grid.SetColumn(p, 0);
                Grid.SetRow(p, i);

                // Keys
                Label keyLabel = new Label();
                keyLabel.Content = "Rooms";
                Slider keyRooms = new Slider();
                keyRooms.Minimum = 1;
                keyRooms.Maximum = 10;
                keyRooms.TickFrequency = 1;
                keyRooms.IsSnapToTickEnabled = true;
                keyRooms.TickPlacement = System.Windows.Controls.Primitives.TickPlacement.BottomRight;
                keyRooms.AutoToolTipPlacement = System.Windows.Controls.Primitives.AutoToolTipPlacement.TopLeft;

                k.Children.Add(keyLabel);
                k.Children.Add(keyRooms);

                programs.Children.Add(k);
                Grid.SetColumn(k, 1);
                Grid.SetRow(k, i);

                // DGSF
                Label DGSFLabel = new Label();
                DGSFLabel.Content = "DGSF";
                Slider DGSF = new Slider();
                DGSF.Minimum = 1;
                DGSF.Maximum = 10;
                DGSF.TickFrequency = 1;
                DGSF.IsSnapToTickEnabled = true;
                DGSF.TickPlacement = System.Windows.Controls.Primitives.TickPlacement.BottomRight;
                DGSF.AutoToolTipPlacement = System.Windows.Controls.Primitives.AutoToolTipPlacement.TopLeft;

                r.Children.Add(DGSFLabel);
                r.Children.Add(DGSF);
                programs.Children.Add(r);
                Grid.SetColumn(r, 2);
                Grid.SetRow(r, i);
            }
            expanderWrapper.Children.Add(programs);

            department.Content = expanderWrapper;
        }

        public static void AddProgram(Grid ppt, int count, int start)
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
                programLabel.HorizontalAlignment = HorizontalAlignment.Stretch;
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
                program.Items.Add("Arash");
                program.SelectedIndex = 0;
                program.HorizontalAlignment = HorizontalAlignment.Stretch;
                program.Margin = new Thickness(0, 5, 2, 0);

                p.Children.Add(programLabel);
                p.Children.Add(program);

                ppt.Children.Add(p);
                Grid.SetColumn(p, 0);
                Grid.SetRow(p, i);

                // Keys
                Label keyLabel = new Label();
                keyLabel.Content = "Rooms";
                Slider keyRooms = new Slider();
                keyRooms.Minimum = 1;
                keyRooms.Maximum = 10;
                keyRooms.TickFrequency = 1;
                keyRooms.IsSnapToTickEnabled = true;
                keyRooms.TickPlacement = System.Windows.Controls.Primitives.TickPlacement.BottomRight;
                keyRooms.AutoToolTipPlacement = System.Windows.Controls.Primitives.AutoToolTipPlacement.TopLeft;

                k.Children.Add(keyLabel);
                k.Children.Add(keyRooms);

                ppt.Children.Add(k);
                Grid.SetColumn(k, 1);
                Grid.SetRow(k, i);

                // DGSF
                Label DGSFLabel = new Label();
                DGSFLabel.Content = "DGSF";
                Slider DGSF = new Slider();
                DGSF.Minimum = 1;
                DGSF.Maximum = 10;
                DGSF.TickFrequency = 1;
                DGSF.IsSnapToTickEnabled = true;
                DGSF.TickPlacement = System.Windows.Controls.Primitives.TickPlacement.BottomRight;
                DGSF.AutoToolTipPlacement = System.Windows.Controls.Primitives.AutoToolTipPlacement.TopLeft;

                r.Children.Add(DGSFLabel);
                r.Children.Add(DGSF);
                ppt.Children.Add(r);
                Grid.SetColumn(r, 2);
                Grid.SetRow(r, i);
            }
        }

    }

}

