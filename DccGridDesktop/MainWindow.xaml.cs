using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
using DccGridDesktop.Models;

namespace DccGridDesktop
{
    public partial class MainWindow : Window
    {
        List<Club> clubs;
        List<Participant> participants;
        List<YearRange> years;
        List<WeightRange> weights;

        bool stopReloading = false;

        void init(string filename = null)
        {
            clubs = new List<Club>();
            participants = new List<Participant>();
            years = new List<YearRange>();
            weights = new List<WeightRange>();
        }

        void reloadGui()
        {
            if (stopReloading) return;

            clubListBox.Items.Clear();

            foreach (var c in clubs)
            {
                clubListBox.Items.Add(c.name);
            }

            weightListBox.Items.Clear();

            foreach (var c in weights)
            {
                weightListBox.Items.Add(String.Format("{0} ({1} - {2})", c.name, c.minWeight, c.maxWeight));
            }

            yearListBox.Items.Clear();

            foreach (var c in years)
            {
                yearListBox.Items.Add(String.Format("{0} - {1}", c.startYear, c.endYear));
            }

            participantsListBox.Items.Clear();

            foreach (var c in participants)
            {
                participantsListBox.Items.Add(String.Format("{0} {1} {2}, {3}кг, {4}г. ({5})", 
                    c.surname, c.name, c.patronymic, c.weight, c.birthYear, c.clubName));
            }
        }

        void addClub (string name, string description)
        {
            if (name.Length == 0) return;

            var c = new Club { name = name, description = description };
            clubs.Add(c);

            reloadGui();
        }

        void addWeight (string name, double minWeight, double maxWeight)
        {
            var w = new WeightRange { name = name, minWeight = minWeight, maxWeight = maxWeight };
            weights.Add(w);

            reloadGui();
        }

        void addYearRange (int startYear, int endYear)
        {
            var y = new YearRange { startYear = startYear, endYear = endYear };
            years.Add(y);

            reloadGui();
        }

        void addParticipants(string name, string surname, string patronymic, int year, double weight, string clubName)
        {
            var p = new Participant { name = name, surname = surname, patronymic = patronymic, birthYear = year, weight = weight, clubName = clubName };
            participants.Add(p);

            reloadGui();
        }

        public MainWindow()
        {
            InitializeComponent();
            init();
        }

        private void btnAddClub_Click(object sender, RoutedEventArgs e)
        {
            var addClubWindow = new AddClub();
            var sd = addClubWindow.ShowDialog();
            if (sd != null && sd == true)
            {
                addClub(addClubWindow.txtName.Text, addClubWindow.txtDescription.Text);
            }
        }

        private void btnRemoveClub_Click(object sender, RoutedEventArgs e)
        {
            if (clubListBox.SelectedIndex != -1)
            {
                MessageBoxResult result = MessageBox.Show("Точно удалить?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    clubs.RemoveAt(clubListBox.SelectedIndex);
                    reloadGui();
                }
            }
        }

        private void btnAddWeight_Click(object sender, RoutedEventArgs e)
        {
            var addWeightWindow = new AddWeight();
            var sd = addWeightWindow.ShowDialog();
            if (sd != null && sd == true)
            {
                addWeight(addWeightWindow.txtWeightName.Text, addWeightWindow.minWeight, addWeightWindow.maxWeight);
            }
        }

        private void btnRemoveWeight_Click(object sender, RoutedEventArgs e)
        {
            if (weightListBox.SelectedIndex != -1)
            {
                MessageBoxResult result = MessageBox.Show("Точно удалить?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    weights.RemoveAt(weightListBox.SelectedIndex);
                    reloadGui();
                }
            }
        }

        private void btnAddYear_Click(object sender, RoutedEventArgs e)
        {
            var addYearWindow = new AddYear();
            var sd = addYearWindow.ShowDialog();
            if (sd != null && sd == true)
            {
                addYearRange(addYearWindow.startYear, addYearWindow.endYear);
            }
        }

        private void btnRemoveYear_Click(object sender, RoutedEventArgs e)
        {
            if (yearListBox.SelectedIndex != -1)
            {
                MessageBoxResult result = MessageBox.Show("Точно удалить?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    years.RemoveAt(yearListBox.SelectedIndex);
                    reloadGui();
                }
            }
        }

        string END_CLUBS = "#END_CLUBS#";
        string END_WEIGHTS = "#END_WEIGHTS#";
        string END_YEARS = "#END_YEARS#";
        string NEXT = "#>>#";

        string infoToString()
        {
            var result = "INFO";

            foreach (var club in clubs)
            {
                result += club.Encode() + NEXT;
            }

            result += END_CLUBS;

            foreach (var year in years)
            {
                result += year.Encode() + NEXT;
            }

            result += END_YEARS;

            foreach (var weight in weights)
            {
                result += weight.Encode() + NEXT;
            }

            result += END_WEIGHTS;

            foreach (var p in participants)
            {
                result += p.Encode() + NEXT; 
            }

            return result;
        }

        void stringsToInfo (string r)
        {
            if (!r.StartsWith("INFO")) return;

            r = r.Substring("INFO".Length);

            var sections = r.Split(new string[] { END_CLUBS, END_YEARS, END_WEIGHTS }, StringSplitOptions.None);

            try
            {
                var clubs = sections[0].Split(new string[] { NEXT }, StringSplitOptions.RemoveEmptyEntries).Select(x => Club.Decode(x)).ToList();
                var years = sections[1].Split(new string[] { NEXT }, StringSplitOptions.RemoveEmptyEntries).Select(x => YearRange.Decode(x)).ToList();
                var weights = sections[2].Split(new string[] { NEXT }, StringSplitOptions.RemoveEmptyEntries).Select(x => WeightRange.Decode(x)).ToList();
                var participants = sections[3].Split(new string[] { NEXT }, StringSplitOptions.RemoveEmptyEntries).Select(x => Participant.Decode(x)).ToList();

                this.clubs = clubs;
                this.years = years;
                this.weights = weights;
                this.participants = participants;

                reloadGui();
            } catch
            {

            }
            //return;
            //var triple = r.Split(new string[] { "######\n" }, StringSplitOptions.None);
            //if (triple.Length != 4) throw new Exception();

            //var r1 = triple[0].Split(new char[] { '\n' }, StringSplitOptions.None).ToList();
            //var r2 = triple[1].Split(new char[] { '\n' }, StringSplitOptions.None).ToList();
            //var r3 = triple[2].Split(new char[] { '\n' }, StringSplitOptions.None).ToList();
            //var r4 = triple[3].Split(new char[] { '\n' }, StringSplitOptions.None).ToList();

            //if (r1[0] != "INFO") throw new Exception();
            //r1.RemoveAt(0);
            //if (r1.Count % 2 != 0) throw new Exception();
            //if (r2.Count % 2 != 0) throw new Exception();
            //if (r3.Count % 3 != 0) throw new Exception();

            //try
            //{
            //    clubs.Clear();
            //    for (int i = 0; i < r1.Count; i += 2)
            //    {
            //        //MessageBox.Show(r1[i] + " " + r1[i+1]);
            //        string clubName = r1[i];
            //        string clubDescription = r1[i + 1];
            //        addClub(clubName, clubDescription);
            //    }

            //    years.Clear();
            //    for (int i = 0; i < r2.Count; i += 2)
            //    {
            //        //MessageBox.Show(r2[i] + " " + r2[i + 1]);
            //        int startYear = int.Parse(r2[i]);
            //        int endYear = int.Parse(r2[i + 1]);
            //        addYearRange(startYear, endYear);
            //    }

            //    weights.Clear();
            //    for (int i = 0; i < r3.Count; i += 3)
            //    {
            //        //MessageBox.Show(r3[i] + " " + r3[i+1] + " " + r3[i+2]);
            //        string weightName = r3[i];
            //        double minWeight = double.Parse(r3[i + 1]);
            //        double maxWeight = double.Parse(r3[i + 2]);
            //        addWeight(weightName, minWeight, maxWeight);
            //    }

            //    participants.Clear();
            //    for (int i = 0; i < r4.Count(); i++)
            //    {
            //        var fields = r4[i].Split(new char[] { '%' }, StringSplitOptions.RemoveEmptyEntries);
            //        string name = fields[0];
            //        string surname = fields[1];
            //        string patronymic = fields[2];
            //        double weight = double.Parse(fields[3]);
            //        int year = int.Parse(fields[4]);
            //        string clubName = fields[5];

            //        addParticipants(name, surname, patronymic, year, weight, clubName);
            //    }
            //}
            //catch
            //{

            //}
        }

        private void btnLoadFromFile_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension
            dlg.DefaultExt = ".ginfo";
            dlg.Filter = "Text documents (.ginfo)|*.ginfo";

            // Display OpenFileDialog by calling ShowDialog method
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox
            if (result == true)
            {
                // Open document
                string filename = dlg.FileName;
                var r = System.IO.File.ReadAllText(filename);
                stringsToInfo(r);
            }
        }

        private void btnSaveToFile_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();

            // Set filter for file extension and default file extension
            dlg.DefaultExt = ".ginfo";
            dlg.Filter = "Text documents (.ginfo)|*.ginfo";

            // Display OpenFileDialog by calling ShowDialog method
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox
            if (result == true)
            {
                // Open document
                string filename = dlg.FileName;
                var r = infoToString();
                System.IO.File.WriteAllText(filename, r);
            }
        }

        private void btnAddParticipant_Click(object sender, RoutedEventArgs e)
        {
            var addParticipantWindow = new AddParticipant();
            addParticipantWindow.clubs = this.clubs;
            var sd = addParticipantWindow.ShowDialog();
            if (sd != null && sd == true)
            {
                //addYearRange(addYearWindow.startYear, addYearWindow.endYear);
                addParticipants(
                    addParticipantWindow.name,
                    addParticipantWindow.surname,
                    addParticipantWindow.patronymic,
                    addParticipantWindow.birthYear,
                    addParticipantWindow.weight,
                    addParticipantWindow.clubName);
            }
        }

        private void btnGridEditor_Click(object sender, RoutedEventArgs e)
        {
            var gridEditor2Window = new GridEditor2.GridEditor2();

            //gridEditorWindow.weights = weights;
            //gridEditorWindow.years = years;
            gridEditor2Window.SetInfo(participants, weights, years);

            gridEditor2Window.Show();
        }

        private void btnRemoveParticipant_Click(object sender, RoutedEventArgs e)
        {
            if (participantsListBox.SelectedIndex != -1)
            {
                MessageBoxResult result = MessageBox.Show("Точно удалить?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    participants.RemoveAt(participantsListBox.SelectedIndex);
                    reloadGui();
                }
            }
        }
    }
}
