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

namespace DccGridDesktop.GridEditor2
{
    class Heap<T>
    {
        T[][] heap;
        public Heap (int minWidth)
        {
            if (minWidth < 1)
            {
                return;
            }
            int power = (int)Math.Ceiling(Math.Log(minWidth) / Math.Log(2));
            int N = (int)Math.Pow(2, power);

            heap = new T[power][];

            int n = N;
            for (int i = 0; i < power; i++)
            {
                heap[i] = new T[n];
                n /= 2;
            }
        }

        public Heap<T> Copy()
        {
            var result = new Heap<T>(heap.Length > 0 ? heap[0].Length : 0);

            for (int i = 0; i < Length; i++)
            {
                for (int j = 0; j < this[i].Length; j++)
                {
                    result[i, j] = heap[i][j];
                }
            }

            return result;
        }

        public int Length
        {
            get
            {
                return heap.Length;
            }
        }

        public bool inHeap (T x)
        {
            for (int i = 0; i < Length; i++)
            {
                for (int j = 0; j < this[i].Length; j++)
                {
                    if (heap[i][j] != null && heap[i][j].Equals(x))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public T[] this[int col]
        {
            get
            {
                return heap[col];
            }
        }

        public Point GetCoords(int col, int row, double x, double y, double width, double height, double rowHeight, double rowWidth)
        {
            var result = new Point();

            int N = heap.Length;
            int M = heap[col].Length;

            double colStep = rowWidth + (width - rowWidth * N) / (N - 1);
            double spaceBetweenRows = 2 * (height - rowHeight * M) / M;
            double rowStep = 2 * rowHeight + spaceBetweenRows;

            result.X = colStep * col;

            var rowPairId = row / 2;

            result.Y = spaceBetweenRows / 2 + rowStep * rowPairId + (row % 2 == 0 ? 0 : rowHeight);

            result.X += x;
            result.Y += y;

            return result;
        }

        public T this[int col, int row]
        {
            get
            {
                return heap[col][row];
            }
            set
            {
                heap[col][row] = value;
            }
        }
    }
    class ParticipantGroup
    {
        public WeightRange weigths;
        public YearRange years;
        public List<Participant> participants;
        public int RoundId { get; set; }
        Heap<Participant> _baseDistribution;
        public Heap<Participant> BaseDistribution
        {
            get
            {
                return _baseDistribution;
            }
            set
            {
                _baseDistribution = value;
                Rounds = new List<Heap<Participant>>();
            }
        }
        public List<Heap<Participant>> Rounds { get; set; }

        int ParticipantsCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < BaseDistribution.Length; i++)
                {
                    for (int j = 0; j < BaseDistribution[i].Length; j++)
                    {
                        if (BaseDistribution[i][j] != null) count++;
                    }
                }
                return count + participants.Count;
            }
        }

        public override string ToString()
        {
            return String.Format("{0} / {1}-{2} ({3})", weigths.name, years.startYear, years.endYear, ParticipantsCount);
        }
    }

    public class MyLabel : Label
    {
        public int row;
        public int col;
    }
    public partial class GridEditor2 : Window
    {
        List<Participant> _participants;
        List<WeightRange> _weights;
        List<YearRange> _years;
        List<ParticipantGroup> _groupedParticipants;
        ParticipantGroup _currentGroup;
        int _groupId;

        bool stopReloading = false;

        public void SetInfo (List<Participant> participants, List<WeightRange> weights, List<YearRange> years)
        {
            weights = weights.OrderBy(x => x.maxWeight).ToList();
            years = years.OrderBy(x => x.startYear).ToList();

            _participants = participants;
            _weights = weights;
            _years = years;
            //Generate groups based on weights and years
            _groupedParticipants = weights.SelectMany(w => years.Select(y => new ParticipantGroup()
            {
                weigths = w,
                years = y,
                participants = participants.Where(p => p.isInWeightRange(w) && p.isInYearRange(y)).ToList(),
                BaseDistribution = new Heap<Participant>(participants.Where(p => p.isInWeightRange(w) && p.isInYearRange(y)).ToList().Count),
                RoundId = -1
            })).Where(g => g.participants.Count != 0).ToList();
        }

        void ReloadGui()
        {
            if (stopReloading) return;

            int selectedGroupIndex = lbGroups.SelectedIndex;
            lbGroups.Items.Clear();
            foreach (var group in _groupedParticipants)
            {
                lbGroups.Items.Add(group.ToString());
            }
            if (lbGroups.Items.Count > selectedGroupIndex)
                lbGroups.SelectedIndex = selectedGroupIndex;

            LoadGroup(selectedGroupIndex);
        }

        void LoadGroup(int index)
        {
            if (index < 0 || index >= _groupedParticipants.Count) return;
            
            lbParticipants.Items.Clear();
            foreach (var p in _groupedParticipants[index].participants)
            {
                lbParticipants.Items.Add(p.ToString());
            }

            _currentGroup = _groupedParticipants[index];
            _groupId = index;
        }

        void LoadTree(int groupId, int roundId)
        {
            if (groupId == -1) return;

            if (roundId == -1) scrollview.Background = new SolidColorBrush(Color.FromArgb(255, 228, 232, 248));
            else scrollview.Background = new SolidColorBrush(Color.FromArgb(255, 248, 228, 247));

            Heap<Participant> currentHeap;
            if (roundId == -1)
                currentHeap = _groupedParticipants[groupId].BaseDistribution;
            else
                currentHeap = _groupedParticipants[groupId].Rounds[roundId];

            double rowHeight = 20;
            double rowWidth = 250;
            

            var canvas = new Canvas();
            scrollview.Content = canvas;
            double width = scrollview.ActualWidth;
            double height = scrollview.ActualHeight;

            for (int col = 0; col < currentHeap.Length; col++)
            {
                for (int row = 0; row < currentHeap[col].Length; row++)
                {
                    var lblParticipant = new MyLabel();

                    var elem = currentHeap[col][row];

                    lblParticipant.Content = elem == null ? "<Пусто>" : elem.ToString();
                    lblParticipant.col = col;
                    lblParticipant.row = row;

                    if (col < 2 && roundId == -1)
                    {
                        lblParticipant.AllowDrop = true;
                        lblParticipant.Drop += dropHandler;
                        lblParticipant.MouseDown += lblDragStarted;
                    } 

                    if (roundId == col)
                    {
                        lblParticipant.MouseLeftButtonDown += markWinnerHandler;
                    }

                    if (elem != null && elem.status == ParticipantStatus.Win)
                    {
                        lblParticipant.Foreground = new SolidColorBrush(Colors.DarkRed);
                    }

                    var p = currentHeap.GetCoords(col, row, 0, 0, width, height, rowHeight, rowWidth);

                    canvas.Children.Add(lblParticipant);
                    Canvas.SetLeft(lblParticipant, p.X);
                    Canvas.SetTop(lblParticipant, p.Y);
                }
            }

            btnNext.IsEnabled = true;
            btnBack.IsEnabled = true;
            if (_currentGroup.RoundId == -1)
            {
                lblStatus.Content = "Изначальная разбивка";
                btnBack.IsEnabled = false;
                btnNext.Content = "Начать раунд";
            }
            else if (_currentGroup.RoundId >= _currentGroup.BaseDistribution.Length - 1)
            {
                lblStatus.Content = "Последний раунд";
                btnNext.IsEnabled = false;
            }
            else
            {
                lblStatus.Content = "Раунд " + (_currentGroup.RoundId + 1);
            }
        }

        void markWinnerHandler(object sender, EventArgs e)
        {
            var label = sender as MyLabel;
            var col = label.col;
            var row = label.row;

            var p = _currentGroup.Rounds[_currentGroup.RoundId][col][row];

            if (p == null)
                return;

            if (p.status != ParticipantStatus.Win)
            {
                p.status = ParticipantStatus.Win;
            }
            else
            {
                p.status = ParticipantStatus.None;
            }
            LoadTree(_groupId, _currentGroup.RoundId);
        }

        void dropHandler (object sender, DragEventArgs e)
        {
            //MessageBox.Show("DROPPED! " + (string)e.Data.GetData(DataFormats.Text));
            var label = sender as MyLabel;
            var textId = (string)e.Data.GetData(DataFormats.Text);
            var col = label.col;
            var row = label.row;
            if (textId[0] != 'l')
            {
                var id = int.Parse(textId);
                var p = _currentGroup.participants[id].getCopy();

                var old_p = _currentGroup.BaseDistribution[col][row];
                _currentGroup.BaseDistribution[col][row] = p;
                _currentGroup.participants.RemoveAt(id);

                if (old_p != null)
                {
                    _currentGroup.participants.Add(old_p);
                }
            }
            else
            {
                var s = textId.Substring(1).Split(';').Select(x => int.Parse(x)).ToList();
                var scol = s[0];
                var srow = s[1];

                var p = _currentGroup.BaseDistribution[scol][srow];
                if (p != null) p = p.getCopy();
                _currentGroup.BaseDistribution[scol][srow] = _currentGroup.BaseDistribution[col][row];
                _currentGroup.BaseDistribution[col][row] = p;
            }

            LoadGroup(_groupId);
            LoadTree(_groupId, _currentGroup.RoundId);
        }

        void lblDragStarted(object sender, MouseButtonEventArgs e)
        {
            var label = sender as MyLabel;
            var p = _currentGroup.BaseDistribution[label.col][label.row];

            if (_currentGroup.BaseDistribution[label.col, label.row] != null)            
                DragDrop.DoDragDrop(label, String.Format("l{0};{1}", label.col, label.row), DragDropEffects.Move);
        }

        public GridEditor2()
        {
            InitializeComponent();
        }

        private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ReloadGui();
        }

        private void lbGroups_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadGroup(lbGroups.SelectedIndex);
            LoadTree(lbGroups.SelectedIndex, _currentGroup.RoundId);
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            if (_currentGroup.RoundId == -1)
            {
                _currentGroup.Rounds = new List<Heap<Participant>>();

                var gc = _currentGroup.BaseDistribution.Copy();

                for (int i = 0; i < gc.Length; i++)
                {
                    for (int j = 0; j < gc[i].Length; j++)
                    {
                        if (gc[i, j] != null) gc[i, j] = gc[i, j].getCopy();
                    }
                }

                _currentGroup.Rounds.Add(gc);
                _currentGroup.RoundId = 0;
            }
            else if (_currentGroup.RoundId > -1)
            {
                var roundId = _currentGroup.RoundId;
                //Надо скопировать текущую пирамидку в новый раунд и перенести выигравших
                var gc = _currentGroup.Rounds[roundId].Copy();

                for (int i = 0; i < gc.Length; i++)
                {
                    for (int j = 0; j < gc[i].Length; j++)
                    {
                        if (gc[i, j] != null) gc[i, j] = gc[i, j].getCopy();
                    }
                }

                _currentGroup.Rounds.Add(gc);


                var list = _currentGroup.Rounds[roundId][roundId];
                var winners = new List<Participant>();

                for (int i = 0; i < list.Length; i++)
                {
                    if (list[i] != null && list[i].status == ParticipantStatus.Win)
                    {
                        var c = list[i].getCopy();
                        c.status = ParticipantStatus.None;
                        winners.Add(c);
                    }
                }

                var list1 = _currentGroup.Rounds[roundId + 1][roundId + 1];
                for (int i = 0; i < list1.Length; i++)
                {
                    if (winners.Count == 0) break;

                    if (list1[i] == null)
                    {
                        list1[i] = winners[0];
                        winners.RemoveAt(0);
                    }
                }

                _currentGroup.RoundId++;
            }

            LoadTree(_groupId, _currentGroup.RoundId);
        }

        private void lbParticipants_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var item = ItemsControl.ContainerFromElement(lbParticipants, e.OriginalSource as DependencyObject) as ListBoxItem;
            if (item != null)
            {
                int id = -1;
                for (int i = 0; i < lbParticipants.Items.Count; i++)
                {
                    if ((string)lbParticipants.Items[i] == (string)item.Content)
                    {
                        id = i;
                        break;
                    }
                }

                if (_currentGroup.BaseDistribution.inHeap(_currentGroup.participants[id]))
                {
                    return;
                }
                
                ListBox lb = (ListBox)sender;
                DragDrop.DoDragDrop(lb, id.ToString(), DragDropEffects.Move);
            }
        }

        private void lbParticipants_Drop(object sender, DragEventArgs e)
        {
            var textId = (string)e.Data.GetData(DataFormats.Text);
            if (textId[0] != 'l') return;

            var s = textId.Substring(1).Split(';').Select(x => int.Parse(x)).ToList();
            var col = s[0];
            var row = s[1];

            var p = _currentGroup.BaseDistribution[col][row];
            _currentGroup.BaseDistribution[col][row] = null;
            _currentGroup.participants.Add(p);

            LoadGroup(_groupId);
            LoadTree(_groupId, _currentGroup.RoundId);
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            if (_currentGroup.RoundId > -1)
            {
                _currentGroup.Rounds.RemoveAt(_currentGroup.RoundId);
                _currentGroup.RoundId--;
            }
            LoadTree(_groupId, _currentGroup.RoundId);
        }
    }
}
