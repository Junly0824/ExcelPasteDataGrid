using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
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
using Panuon.UI.Silver;

namespace DataGridLib
{
    public class ClipObject {
        /// <summary>
        /// 剪贴板内容
        /// </summary>
        public List<List<string>> Context { get; set; }
        /// <summary>
        /// 计数数量
        /// </summary>
        public int Count { get; set; }
    }
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : WindowX
    {

        System.Data.DataTable appearData = new System.Data.DataTable();
        public MainWindow()
        {
            InitializeComponent();
            
            appearData.Columns.Add("test1");
            appearData.Columns.Add("test2");
            appearData.Columns.Add("test3");
            appearData.Columns.Add("test4");
            appearData.Rows.Add("1", "2", "3", "4");
            appearData.Rows.Add("5", "6", "7", "8");
            appearData.Rows.Add("9", "10", "11", "12");
            appearData.Rows.Add("13", "14", "15", "16");
            testDataGrid.ItemsSource = appearData.DefaultView;
            testDataGrid.RowHeaderWidth = 20;
            testDataGrid.LoadingRow += TestDataGrid_LoadingRow;
        }

        private void TestDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            //throw new NotImplementedException();
            e.Row.Header = e.Row.GetIndex();
        }

        private void testButton_Click(object sender, RoutedEventArgs e)
        {
            IDataObject data = Clipboard.GetDataObject();
            MessageBox.Show(data.ToString());
            if (testDataGrid.Items[2].GetType() == typeof(DataRowView)) {
                (testDataGrid.Items[2] as DataRowView).Row[0] = "100";
            }
            
        }

        private void testDataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.V) && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                ClipObject currentClipObject = handleClipValueToArray();

                List<List<int>> XYCollection = GetCellXYList(testDataGrid);

                int loop_row = 0;
                foreach (int RowItem in XYCollection[0])
                {
                    int loop_Column = 0;
                    foreach (int ColumnItem in XYCollection[1])
                    {
                        if (testDataGrid.Items[ColumnItem].GetType() == typeof(DataRowView))
                        {
                            (testDataGrid.Items[ColumnItem] as DataRowView).Row[RowItem] = currentClipObject.Context[loop_row][loop_Column];
                        }
                        loop_Column++;
                    }
                    loop_row++;
                }

                e.Handled = true;
            }
        }

        private List<List<int>> GetCellXYList(DataGrid dg)
        {
            List<List<int>> coordination = new List<List<int>>();
            Dictionary<int, List<int>> coordinationCopy = new Dictionary<int, List<int>>();
            var _cells = dg.SelectedCells;
            if (_cells.Any())
            {
                foreach (DataGridCellInfo _cellsItem in _cells)
                {
                    int rowIndex = dg.Items.IndexOf(_cellsItem.Item);
                    int columnIndex = _cellsItem.Column.DisplayIndex;
                    if (coordinationCopy.ContainsKey(rowIndex))
                    {
                        List<int> currentCoordination = coordinationCopy[rowIndex];
                        currentCoordination.Add(columnIndex);
                        coordinationCopy[rowIndex] = currentCoordination;
                    }
                    else
                    {
                        coordinationCopy.Add(rowIndex, new List<int>() { columnIndex });
                    }
                }
            }

            //判断当前是否不是正常的矩形框选
            int oldRowCount = -1;
            int startColumn = -1;
            List<int> rowCollection = new List<int>();
            List<int> columnCollection = new List<int>();
            foreach (int coordinationCopyItem in coordinationCopy.Keys)
            {
                if (oldRowCount == -1 || oldRowCount == coordinationCopy[coordinationCopyItem].Count)
                {
                    rowCollection.Add(coordinationCopyItem);
                    oldRowCount = coordinationCopy[coordinationCopyItem].Count;
                    List<int> currentRowCollection = coordinationCopy[coordinationCopyItem];
                    currentRowCollection.Sort();
                    if (startColumn == -1 || currentRowCollection[0] == startColumn)
                    {
                        startColumn = currentRowCollection[0];
                    }
                    else {
                        MessageBox.Show("选择出现问题");
                        break;
                    }
                    columnCollection = coordinationCopy[coordinationCopyItem];
                }
                else {
                    MessageBox.Show("选择出现问题");
                    break;
                }
            }
            rowCollection.Sort();
            columnCollection.Sort();
            coordination.Add(rowCollection);
            coordination.Add(columnCollection);
            return coordination;
        }

        private bool GetCellXY(DataGrid dg, ref int rowIndex, ref int columnIndex)
        {
            var _cells = dg.SelectedCells;
            if (_cells.Any())
            {
                rowIndex = dg.Items.IndexOf(_cells.First().Item);
                columnIndex = _cells.First().Column.DisplayIndex;
                return true;
            }
            return false;
        }


        /// <summary>
        /// 处理当前写字板中信息进入数组列表中
        /// </summary>
        /// <returns></returns>
        public ClipObject handleClipValueToArray()
        {
            int countCurrnt = 0;
            List<List<string>> ClipTableCollection = new List<List<string>>();
            IDataObject data = Clipboard.GetDataObject();
            string dataContext = data.GetData(DataFormats.Text) as string;
            dataContext = dataContext.Replace("\r","");
            //判断当前是否存在多行
            if (dataContext.Contains('\n'))
            {
                string[] dataContextRow = dataContext.Split('\n');
                foreach (string dataContextRowinCell in dataContextRow)
                {
                    string currentDataContextRowinCell = dataContextRowinCell;
                    string[] dataContextCell = currentDataContextRowinCell.Split('\t');
                    List<string> cellCollection = new List<string>();
                    foreach (string cellItem in dataContextCell)
                    {
                        cellCollection.Add(cellItem);
                        countCurrnt++;
                    }
                    ClipTableCollection.Add(cellCollection);
                }
            }
            else {
                //如果不存在多行查看当前是否存在多个单元格
                if (dataContext.Contains('\t'))
                {
                    //排除当前存在空格情况如果为空格进行替换
                    dataContext = dataContext.Replace("\t\t", "NULLWord\t");
                    string[] dataContextCell = dataContext.Split('\t');
                    List<string> cellCollection = new List<string>();
                    foreach (string cellItem in dataContextCell)
                    {
                        cellCollection.Add(cellItem);
                        countCurrnt++;
                    }
                    ClipTableCollection.Add(cellCollection);
                }
                else {
                    //不存在多个单元格情况即一个值
                    ClipTableCollection.Add(new List<string>() { dataContext });
                    countCurrnt++;
                }
            }

            return new ClipObject()
            {
                Context = ClipTableCollection,
                Count = countCurrnt
            };
        }
    }
}
