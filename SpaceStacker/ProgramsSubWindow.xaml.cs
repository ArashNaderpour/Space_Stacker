using System;
using System.Windows;


namespace SpaceStacker
{
    /// <summary>
    /// Interaction logic for ProgramsSubWindow.xaml
    /// </summary>
    public partial class ProgramsSubWindow : Window
    {
        public ProgramsSubWindow()
        {
            InitializeComponent();
        }

        private void SaveExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ExtraMethods.ExportGridToExcel(this.ProgramsDataChart);
            }

            catch (Exception error)
            {
                MessageBox.Show(error.ToString());
            }
        }
    }
}
