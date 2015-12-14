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
    /// Interaction logic for RoundOrder.xaml
    /// </summary>
    /// 
    public partial class RoundOrder : Window
    {
        public List<Participant> participants;
        public RoundOrder()
        {
            InitializeComponent();
        }

        void reloadGui()
        {
            listBox.Items.Clear();
            for (int i = 0; i < participants.Count; i++)
            {
                listBox.Items.Add(participants[i].ToString());
            }
        }

        private void button_Copy1_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            if (listBox.SelectedIndex == -1 || listBox.SelectedIndex == 0) return;

            var id = listBox.SelectedIndex;

            var c = participants[id];
            participants[id] = participants[id - 1];
            participants[id - 1] = c;

            reloadGui();

            listBox.SelectedIndex = id - 1;
        }

        private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            reloadGui();
        }

        private void button_Copy_Click(object sender, RoutedEventArgs e)
        {
            if (listBox.SelectedIndex == -1 || listBox.SelectedIndex == listBox.Items.Count - 1) return;

            var id = listBox.SelectedIndex;

            var c = participants[id];
            participants[id] = participants[id + 1];
            participants[id + 1] = c;

            reloadGui();

            listBox.SelectedIndex = id + 1;
        }
    }
}
