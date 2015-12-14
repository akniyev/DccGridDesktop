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
    /// Interaction logic for AddWeight.xaml
    /// </summary>
    public partial class AddWeight : Window
    {
        public AddWeight()
        {
            InitializeComponent();
        }

        public double minWeight;
        public double maxWeight;

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (txtWeightName.Text.Length > 0)
            {
                try
                {
                    minWeight = double.Parse(txtWeightMin.Text);
                    maxWeight = double.Parse(txtWeightMax.Text);
                    this.DialogResult = true;
                    this.Close();
                }
                catch
                {
                    MessageBox.Show("Некорректно заполнены поля!");
                }
            }
            else
            {
                MessageBox.Show("Некорректно заполнены поля!");
            }
        }
    }
}
