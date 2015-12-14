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
using Excel = Microsoft.Office.Interop.Excel;
using DccGridDesktop.Models;

namespace DccGridDesktop
{
    /// <summary>
    /// Interaction logic for GridEditor.xaml
    /// </summary>
    
   
    public class Grid
    {
        List<Participant> filterParticipants(WeightRange wr, YearRange yr, List<Participant> ps)
        {
            var result = new List<Participant>();

            foreach (var p in ps)
            {
                if (p.isInWeightRange(wr) && p.isInYearRange(yr))
                {
                    result.Add(p);
                }
            }

            return result;
        }
        public Grid(WeightRange wr, YearRange yr, List<Participant> allParticipants)
        {
            weightRange = wr;
            yearRange = yr;
            roundParticipants = new List<List<Participant>>();
            roundParticipants.Add(filterParticipants(wr, yr, allParticipants));
        }

        public Grid() { }

        public Grid(WeightRange wr, YearRange yr, List<List<Participant>> roundParticipants)
        {
            weightRange = wr;
            yearRange = yr;
            this.roundParticipants = roundParticipants;
        }

        public WeightRange weightRange;
        public YearRange yearRange;
        public List<List<Participant>> roundParticipants;
    }

    public partial class GridEditor : Window
    {

        List<Grid> grids;
        public List<WeightRange> weights;
        public List<YearRange> years;
        public List<Participant> participants;

        Grid currentGrid;
        List<Participant> currentRound;
        int currentRoundId;
        List<RadioButton> currentRadioButtons;
        List<RadioButton> currentCheckBoxes;

        string GRIDS = "GRIDS\n";
        string END_WEIGHT = "\n#END_WEIGHT#\n";
        string END_YEAR = "\n#END_YEAR#\n";
        string NEXT = "\n#====#\n";
        string END_ROUNDS = "\n#END_ROUNDS#\n";
        string END_GRID = "\n#END_GRID#\n";

        string gridsToString()
        {
            var result = GRIDS;

            for (int i = 0; i < grids.Count; i++)
            {
                var g = grids[i];

                result += g.weightRange.Encode();
                result += END_WEIGHT;

                result += g.yearRange.Encode();
                result += END_YEAR;

                for (int j = 0; j < g.roundParticipants.Count; j++)
                {
                    for (int k = 0; k < g.roundParticipants[j].Count; k++)
                    {
                        result += g.roundParticipants[j][k].Encode();
                        result += NEXT;
                    }
                    result += END_ROUNDS;
                }

                result += END_GRID;
            }

            return result;
        }

        public GridEditor()
        {
            InitializeComponent();
        }

        void reloadGui()
        {
            groupListBox.Items.Clear();

            foreach (var g in grids)
            {
                groupListBox.Items.Add(String.Format("{0}-{1}гг, {2} (кол-во: {3})", 
                    g.yearRange.startYear, g.yearRange.endYear, g.weightRange.name, g.roundParticipants[0].Count));
            }

            if (currentGrid == null) return;

            roundsListBox.Items.Clear();

            for (int i = 0; i < currentGrid.roundParticipants.Count; i++)
            {
                roundsListBox.Items.Add(i);
            }
        }

        void loadGrid()
        {
            currentRadioButtons = new List<RadioButton>();
            currentCheckBoxes = new List<RadioButton>();

            var content = new StackPanel();
            int k = 1;
            for (int i = 0; i < currentRound.Count; i++)
            {
                var grId = 0;
                if (k > 0)
                {
                    grId = k;
                    k = -k;
                } else
                {
                    grId = -k;
                    k = -k + 1;
                }
                var rb = new RadioButton() { Content = currentRound[i].ToString(), IsChecked = false, GroupName = grId.ToString()};
                

                currentRadioButtons.Add(rb);
                content.Children.Add(rb);

                if (k > 0 || i == currentRound.Count - 1)
                {
                    var cb = new RadioButton() { Content = "Перекинуть", IsChecked = false, GroupName = grId.ToString() };
                    currentCheckBoxes.Add(cb);
                    content.Children.Add(cb);
                    var s = new Separator();
                    s.Height = 10;
                    content.Children.Add(s);
                }                
            }
            scrollView.Content = content;
        }

        private void groupListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (groupListBox.SelectedIndex == -1) return;

            var g = grids[groupListBox.SelectedIndex];

            currentGrid = g;

            roundsListBox.Items.Clear();

            for (int i = 0; i < g.roundParticipants.Count; i++)
            {
                roundsListBox.Items.Add(i);
            } 
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            grids = new List<Grid>();
            for (int i = 0; i < years.Count; i++)
            {
                for (int j = 0; j < weights.Count; j++)
                {
                    var g = new Grid(weights[j], years[i], participants);
                    grids.Add(g);
                }
            }

            reloadGui();
        }

        private void roundsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (roundsListBox.SelectedIndex == -1)
            {
                var content = new StackPanel();
                scrollView.Content = content;
                return;
            }

            currentRoundId = roundsListBox.SelectedIndex;

            currentRound = currentGrid.roundParticipants[currentRoundId];

            loadGrid();
        }

        private void btnNextRound_Click(object sender, RoutedEventArgs e)
        {
            if (currentRound == null) return;
            if (currentRound.Count < 2)
            {
                MessageBox.Show("Дальше всё!");
                return;
            }

            if (currentRoundId != currentGrid.roundParticipants.Count - 1)
            {
                MessageBoxResult result = MessageBox.Show("Вы действительно хотите переписать раунды?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.No) return;
            }

            int checkedCount = 0;
            int transferredCount = 0;
            int totalCount = currentRound.Count / 2 + currentRound.Count % 2;
            for (int i = 0; i < currentRadioButtons.Count; i++)
            {
                if (currentRadioButtons[i].IsChecked != null && currentRadioButtons[i].IsChecked == true)
                {
                    checkedCount++;
                }
            }
            for (int i = 0; i < currentCheckBoxes.Count; i++)
            {
                if (currentCheckBoxes[i].IsChecked == true) transferredCount++;
            }
            if (checkedCount + transferredCount != totalCount)
            {
                MessageBox.Show("Необходимо выбрать всех участников!");
                return;
            } else if (checkedCount == 0)
            {
                MessageBox.Show("Всех переносить нельзя!");
                return;
            }

            var nRound = new List<Participant>();
            for (int i = 0; i < currentRadioButtons.Count; i++)
            {
                if (currentRadioButtons[i].IsChecked != null && currentRadioButtons[i].IsChecked == true)
                {
                    var p = currentRound[i].getCopy();
                    p.status = ParticipantStatus.None;
                    nRound.Add(p);

                    currentRound[i].status = ParticipantStatus.Win;
                }
                else if (currentCheckBoxes[i/2].IsChecked == true)
                {
                    var p = currentRound[i].getCopy();
                    p.status = ParticipantStatus.None;
                    nRound.Add(p);
                    currentRound[i].status = ParticipantStatus.Transfer;
                }
                else
                {
                    currentRound[i].status = ParticipantStatus.None;
                }
            }

            if (currentRoundId < currentGrid.roundParticipants.Count - 1)
            {
                currentGrid.roundParticipants.RemoveRange(currentRoundId + 1, currentGrid.roundParticipants.Count - currentRoundId - 1);
            }

            currentGrid.roundParticipants.Add(nRound);

            reloadGui();
            loadGrid();
        }

        private void btnOrder_Click(object sender, RoutedEventArgs e)
        {
            if (currentRound == null) return;
            if (currentRoundId != currentGrid.roundParticipants.Count - 1)
            {
                MessageBoxResult result = MessageBox.Show("Вы действительно хотите переписать раунды?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.No) return;
            }

            var roundOrder = new RoundOrder();
            roundOrder.participants = currentRound;
            var rs = roundOrder.ShowDialog();

            if (rs != true) return;

            if (currentRoundId < currentGrid.roundParticipants.Count - 1)
            {
                currentGrid.roundParticipants.RemoveRange(currentRoundId + 1, currentGrid.roundParticipants.Count - currentRoundId - 1);
            }

            currentGrid.roundParticipants[currentRoundId] = roundOrder.participants;

            currentRound = currentGrid.roundParticipants[currentRoundId];

            reloadGui();
            loadGrid();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // Create SaveFileDialog
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();

            // Set filter for file extension and default file extension
            dlg.DefaultExt = ".grids";
            dlg.Filter = "Text documents (.grids)|*.grids";

            // Display OpenFileDialog by calling ShowDialog method
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox
            if (result == true)
            {
                // Open document
                string filename = dlg.FileName;
                var r = gridsToString();
                System.IO.File.WriteAllText(filename, r);
            }
        }

        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension
            dlg.DefaultExt = ".grids";
            dlg.Filter = "Text documents (.grids)|*.grids";

            // Display OpenFileDialog by calling ShowDialog method
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox
            if (result == true)
            {
                // Open document
                string filename = dlg.FileName;
                var r = System.IO.File.ReadAllText(filename);
                loadGridsFromString(r);
            }
        }

        private void loadGridsFromString(string r)
        {
            if (!r.StartsWith(GRIDS)) return;

            r = r.Substring(GRIDS.Length);

            var grids = new List<Grid>();

            var g1 = r.Split(new string[] { END_GRID }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Split(new string[] { END_WEIGHT, END_YEAR }, StringSplitOptions.None))
                .ToList();

            foreach (var x in g1)
            {
                var w = new WeightRange();
                var y = new YearRange();

                w.LoadFromString(x[0]);
                y.LoadFromString(x[1]);

                var g2 = x[2].Split(new string[] { END_ROUNDS }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(a => a.Split(new string[] { NEXT }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(b => new Participant(b)).ToList()).ToList();

                if (g2.Count == 0) g2.Add(new List<Participant>());

                var g = new Grid(w, y, g2);
                grids.Add(g);
            }

            this.grids = grids;
            reloadGui();
        }

        int half(int col)
        {
            if (col < 0) throw new Exception();
            if (col == 0) return 1;

            return half(col - 1) * 2 + 1;
        }

        int step(int col) { return half(col) * 2; }

        int xCoord (int col)
        {
            return 10 * col + 1;
        }

        int yCoord(int col, int row)
        {
            return half(col) + (row / 2) * (2 + step(col)) + row % 2;
        }

        private void AllBorders(Excel.Borders _borders)
        {
            _borders[Excel.XlBordersIndex.xlEdgeLeft].LineStyle = Excel.XlLineStyle.xlContinuous;
            _borders[Excel.XlBordersIndex.xlEdgeRight].LineStyle = Excel.XlLineStyle.xlContinuous;
            _borders[Excel.XlBordersIndex.xlEdgeTop].LineStyle = Excel.XlLineStyle.xlContinuous;
            _borders[Excel.XlBordersIndex.xlEdgeBottom].LineStyle = Excel.XlLineStyle.xlContinuous;
            _borders.Color = System.Drawing.Color.Black;
        }

        private void CustomBorders(Excel.Borders _borders, bool left, bool right, bool top, bool bottom)
        {
            _borders[Excel.XlBordersIndex.xlEdgeLeft].LineStyle = left ? Excel.XlLineStyle.xlContinuous : Excel.XlLineStyle.xlLineStyleNone;
            _borders[Excel.XlBordersIndex.xlEdgeRight].LineStyle = right ? Excel.XlLineStyle.xlContinuous : Excel.XlLineStyle.xlLineStyleNone;
            _borders[Excel.XlBordersIndex.xlEdgeTop].LineStyle = top ? Excel.XlLineStyle.xlContinuous : Excel.XlLineStyle.xlLineStyleNone;
            _borders[Excel.XlBordersIndex.xlEdgeBottom].LineStyle = bottom ? Excel.XlLineStyle.xlContinuous : Excel.XlLineStyle.xlLineStyleNone;

            if (left)
                _borders[Excel.XlBordersIndex.xlEdgeLeft].Color = System.Drawing.Color.Black;
            if (right)
                _borders[Excel.XlBordersIndex.xlEdgeRight].Color = System.Drawing.Color.Black;
            if (top)
                _borders[Excel.XlBordersIndex.xlEdgeTop].Color = System.Drawing.Color.Black;
            if (bottom)
                _borders[Excel.XlBordersIndex.xlEdgeBottom].Color = System.Drawing.Color.Black;
            //_borders.Color = System.Drawing.Color.Black;
        }

        void exportToExcel(string filename)
        {
            Excel.Application excel = new Excel.Application();
            excel.Visible = true;
            Excel.Workbook wb = excel.Workbooks.Add();

            foreach (var g in grids)
            {
                Excel.Worksheet sh = wb.Sheets.Add();
                sh.Name = String.Format("{0}-{1}гг, {2}",
                    g.yearRange.startYear, g.yearRange.endYear, g.weightRange.name);

                sh.StandardWidth = 3;
                var rng = sh.UsedRange;
                rng.Style.VerticalAlignment = Excel.XlHAlign.xlHAlignCenter;

                int treeBaseWidth = 1;

                for (int i = 0; i < g.roundParticipants.Count; i++)
                {
                    while (treeBaseWidth / Math.Pow(2, i) < g.roundParticipants[i].Count)
                        treeBaseWidth *= 2;
                }

                //Здесь ячейки только рисуются
                for (int col = 0; treeBaseWidth / Math.Pow(2, col) > 1; col++)
                {
                    for (int row = 0; row < treeBaseWidth / Math.Pow(2, col); row++)
                    {
                        if (row % 2 == 0)
                        {
                            bool down = (row / 2) % 2 == 0;

                            if (treeBaseWidth / Math.Pow(2, col) > 2)
                                if (down)
                                {
                                    var b3 = sh.Range[sh.Cells[yCoord(col, row) + 1, xCoord(col) + 8], sh.Cells[yCoord(col + 1, row / 2), xCoord(col) + 8]].Borders();
                                    CustomBorders(b3, false, true, false, false);

                                    var b1 = sh.Range[sh.Cells[yCoord(col, row) + 1, xCoord(col) + 8], sh.Cells[yCoord(col, row) + 1, xCoord(col) + 8]].Borders();
                                    CustomBorders(b1, false, true, true, false);
                                    var b2 = sh.Range[sh.Cells[yCoord(col + 1, row / 2), xCoord(col + 1) - 1], sh.Cells[yCoord(col + 1, row / 2), xCoord(col + 1) - 1]].Borders();
                                    CustomBorders(b2, true, false, false, true);
                                }
                                else
                                {
                                    var b3 = sh.Range[sh.Cells[yCoord(col, row), xCoord(col) + 8], sh.Cells[yCoord(col + 1, row / 2), xCoord(col) + 8]].Borders();
                                    CustomBorders(b3, false, true, false, false);

                                    var b1 = sh.Range[sh.Cells[yCoord(col, row), xCoord(col) + 8], sh.Cells[yCoord(col, row), xCoord(col) + 8]].Borders();
                                    CustomBorders(b1, false, true, false, true);
                                    var b2 = sh.Range[sh.Cells[yCoord(col + 1, row / 2), xCoord(col + 1) - 1], sh.Cells[yCoord(col + 1, row / 2), xCoord(col + 1) - 1]].Borders();
                                    CustomBorders(b2, true, false, true, false);
                                }

                            sh.Range[sh.Cells[yCoord(col, row), xCoord(col)], sh.Cells[yCoord(col, row) + 1, xCoord(col)]].Merge();
                            sh.Range[sh.Cells[yCoord(col, row), xCoord(col) + 2], sh.Cells[yCoord(col, row), xCoord(col) + 6]].Merge();
                            sh.Range[sh.Cells[yCoord(col, row + 1), xCoord(col) + 2], sh.Cells[yCoord(col, row + 1), xCoord(col) + 6]].Merge();

                            AllBorders(
                            sh.Range[sh.Cells[yCoord(col, row), xCoord(col)], sh.Cells[yCoord(col, row) + 1, xCoord(col) + 7]].Borders()
                            );
                        }
                    }
                }

                //Здесь ячейки заполняются
                int counter = 1;
                for (int col = 0; col < g.roundParticipants.Count; col++)
                {
                    var roundParticipants = g.roundParticipants[col].Where(x => x.status != ParticipantStatus.Transfer).ToList();

                    for (int row = 0; row < roundParticipants.Count; row++)
                    {
                        if (roundParticipants.Count <= 1) continue;

                        var p = roundParticipants[row];
                        sh.Cells[yCoord(col, row), xCoord(col) + 2] = p.surname + " " + p.name[0] + "." + p.patronymic[0] + ".";

                        if (p.status == ParticipantStatus.Win)
                            sh.Cells[yCoord(col, row), xCoord(col) + 7] = "+";

                        if (p.status == ParticipantStatus.Transfer)
                            sh.Cells[yCoord(col, row), xCoord(col) + 7] = ">>";

                        if (row % 2 == 0)
                        {
                            bool down = (row / 2) % 2 == 0;

                            sh.Cells[yCoord(col, row), xCoord(col)] = counter.ToString();
                            counter++;
                        }
                    }
                }
            }

            wb.SaveAs(filename);
            wb.Close(true);
            excel.Quit();
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();

            // Set filter for file extension and default file extension
            dlg.DefaultExt = ".xlsx";
            dlg.Filter = "Text documents (.xlsx)|*.xlsx";

            // Display OpenFileDialog by calling ShowDialog method
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox
            if (result == true)
            {
                // Open document
                string filename = dlg.FileName;
                exportToExcel(filename);
            }
        }
    }
}
