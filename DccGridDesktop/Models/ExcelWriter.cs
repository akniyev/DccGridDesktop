using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;

namespace DccGridDesktop.Models
{
    static class ExcelWriter
    {
        static int half(int col)
        {
            if (col < 0) throw new Exception();
            if (col == 0) return 1;

            return half(col - 1) * 2 + 1;
        }

        static int step(int col) { return half(col) * 2; }

        static int xCoord(int col)
        {
            return 10 * col + 1;
        }

        static int yCoord(int col, int row)
        {
            return half(col) + (row / 2) * (2 + step(col)) + row % 2;
        }
        private static void AllBorders(Excel.Borders _borders)
        {
            _borders[Excel.XlBordersIndex.xlEdgeLeft].LineStyle = Excel.XlLineStyle.xlContinuous;
            _borders[Excel.XlBordersIndex.xlEdgeRight].LineStyle = Excel.XlLineStyle.xlContinuous;
            _borders[Excel.XlBordersIndex.xlEdgeTop].LineStyle = Excel.XlLineStyle.xlContinuous;
            _borders[Excel.XlBordersIndex.xlEdgeBottom].LineStyle = Excel.XlLineStyle.xlContinuous;
            _borders.Color = System.Drawing.Color.Black;
        }

        private static void CustomBorders(Excel.Borders _borders, bool left, bool right, bool top, bool bottom)
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

        public static void WriteGroupsToExcel(List<ParticipantGroup> group, string filename, bool baseDistribution)
        {
            Excel.Application excel = new Excel.Application();
            excel.DisplayAlerts = false;
            excel.Visible = true;
            Excel.Workbook wb = excel.Workbooks.Add();

            foreach (var g in group)
            {
                Excel.Worksheet sh = wb.Sheets.Add();
                sh.Name = g.ToString().Replace('/', '-');

                sh.StandardWidth = 3;
                var rng = sh.UsedRange;
                rng.Style.VerticalAlignment = Excel.XlHAlign.xlHAlignCenter;

                if (!baseDistribution && g.Rounds.Count == 0) continue;
                
                var dist = baseDistribution ? g.BaseDistribution : g.Rounds[g.Rounds.Count - 1];

                //Здесь ячейки только рисуются
                for (int col = 0; col < dist.Length; col++)
                {
                    for (int row = 0; row < dist[col].Length; row++)
                    {
                        if (row % 2 == 0)
                        {
                            bool down = (row / 2) % 2 == 0;
                            if (col < dist.Length - 1)
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
                for (int col = 0; col < dist.Length; col++)
                {
                    for (int row = 0; row < dist[col].Length; row++)
                    {
                        var p = dist[col][row];

                        if (p == null)
                        {
                            continue;
                        }
                        sh.Cells[yCoord(col, row), xCoord(col) + 2] = p.surname + " " + p.name[0] + "." + p.patronymic[0] + ".";

                        if (p.status == ParticipantStatus.Win)
                            sh.Cells[yCoord(col, row), xCoord(col) + 7] = "+";

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
            excel.DisplayAlerts = false;
            wb.Close(true);
            excel.Quit();
        }
    }
}
