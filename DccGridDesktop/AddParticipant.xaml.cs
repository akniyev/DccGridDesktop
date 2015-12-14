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
    /// Interaction logic for AddParticipant.xaml
    /// </summary>
    public partial class AddParticipant : Window
    {
        public List<Club> clubs;

        public string name;
        public string surname;
        public string patronymic;
        public int birthYear;
        public double weight;
        public string clubName;

        public AddParticipant()
        {
            InitializeComponent();

        }

        private void Window_Activated(object sender, EventArgs e)
        {
            if (clubs != null && clubs.Count > 0)
            {
                clubsComboBox.Items.Clear();
                foreach (var club in clubs)
                {
                    clubsComboBox.Items.Add(club.name);
                }
            } 
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.name = txtName.Text;
                this.surname = txtSurname.Text;
                this.patronymic = txtPatronymic.Text;
                this.birthYear = int.Parse(txtYear.Text);
                this.weight = double.Parse(txtWeight.Text);
                this.clubName = clubsComboBox.Text;

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