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
    /// Interaction logic for AddClub.xaml
    /// </summary>
    public partial class AddClub : Window
    {
        public AddClub()
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
            if (txtName.Text.Length > 0)
            {
                this.DialogResult = true;
                this.Close();
            } else
            {
                MessageBox.Show("Имя клуба не может быть пустым!");
            }
            

        }
    }
}
