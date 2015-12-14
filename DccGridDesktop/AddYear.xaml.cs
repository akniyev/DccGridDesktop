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
using DccGridDesktop.Models;

namespace DccGridDesktop
{
    /// <summary>
    /// Interaction logic for AddYear.xaml
    /// </summary>
    public partial class AddYear : Window
    {
        public int startYear;
        public int endYear;

        public AddYear()
        {
            InitializeComponent();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                startYear = int.Parse(txtStartYear.Text);
                endYear = int.Parse(txtEndYear.Text);
                if (startYear > endYear) throw new Exception();
                this.DialogResult = true;
                this.Close();
            }
            catch
            {
                MessageBox.Show("Некорректно заполнены поля!");
            }
        }
    }
}
