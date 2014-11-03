using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualBasic.FileIO;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms.DataVisualization.Charting;
using OpcRcw.Da;

namespace MMI_Project_CSharp
{
    public partial class Form1 : Form
    {
        DxpSimpleAPI.DxpSimpleClass opc = new DxpSimpleAPI.DxpSimpleClass();
        string DEV_NAME = MMI_Project_CSharp.Properties.Settings.Default.Device;
        string WORD_PREF = MMI_Project_CSharp.Properties.Settings.Default.Word_Reg_Prefix;
        int patternStart = MMI_Project_CSharp.Properties.Settings.Default.Pattern1Start;
        DataSet ds = null;
        DataTable dt = null;
        public Form1()
        {
            InitializeComponent();

        }
        private void update_Graph()
        {
            double timeLine = 0;
            int point = 0;
            for (int i = 0; i < 24; i++)
            {
                double time=Convert.ToDouble(data1.Rows[0].Cells[i].Value);
                double temp=Convert.ToDouble(data1.Rows[1].Cells[i].Value);
                double humid = Convert.ToDouble(data1.Rows[2].Cells[i].Value);
                if (time != 0)
                {
                    if (timeLine == 0)
                    {
                        plot_Line(timeLine, temp, humid);
                    }
                    timeLine = timeLine + Math.Round((time / 60), 2);
                    plot_Line(timeLine, temp, humid);
                    lineChart.Series["温度"].Points[point].Label = (i + 1).ToString();
                    point++;
                }
                else
                {
                    break;
                }
            }
            if (point > 0)
            {
                lineChart.Series["温度"].Points[point].Label = (Convert.ToInt32(lineChart.Series["温度"].Points[point - 1].Label.ToString()) + 1).ToString();
            }
            lineChart.Series["温度"].Points[1].BorderDashStyle = ChartDashStyle.Dash;
            lineChart.Series["湿度"].Points[1].BorderDashStyle = ChartDashStyle.Dash;
            axisY_Maximum();
            lineChart.ChartAreas[0].AxisX.Maximum = Math.Ceiling(timeLine);
            lineChart.ChartAreas[0].AxisX.Minimum = 0;
            lineChart.ChartAreas[0].AxisX.ScaleView.Zoom(0, 10);
        }
        private void axisY_Maximum()
        {
            double max = 0, min = 0, max2 = 0, min2 = 0, value, value2;
            var x = lineChart.ChartAreas[0];
            for (int a = 0; a < 24; a++)
            {
                value = Convert.ToDouble(data1.Rows[1].Cells[a].Value);
                value2 = Convert.ToDouble(data1.Rows[2].Cells[a].Value);
                if (value > max) max = value;
                else if (value < min) min = value;

                if (value2 > max2) max2 = value2;
                else if (value2 < min2) min2 = value2;
            }
            if (max > 30) 
            {
                max = (10 - (max % 10)) + max;
                x.AxisY.Maximum = Math.Ceiling(max);
            }
            if (min < -30) 
            {
                min = ((-10 - (min % 10)) + min);
                x.AxisY.Minimum = Math.Floor((min)); 
            }
            if (max2 > 100) 
            {
                max2 = (100 - (max2 % 100)) + max2;
                x.AxisY2.Maximum = Math.Ceiling(max2); 
            }
            if (min2 < 0) 
            {
                min2 = ((-100 - (min2 % 100)) + min2);
                x.AxisY2.Minimum = Math.Floor(min2); 
            }
            x.AxisY.Interval = 10;
        }
        private void plot_Line(double timeLine, double temp, double humid)
        {
                lineChart.Series["温度"].Points.AddXY(timeLine, temp);
                lineChart.Series["湿度"].Points.Add(new DataPoint(timeLine, humid));
        }
        private void check()
        {
            if (data1.Rows[0].Cells[0].Value.ToString() != "0")
            {
                check_Time();
                if (!HasErrorText())
                {
                    clear();
                    update_Graph();
                }
                else
                {
                    MessageBox.Show("エラーが出ているためグラフ表示ができません", "グラフ表示ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                clear();
            }
        }
        private void clear()
        {
            lineChart.Series["温度"].Points.Clear();
            lineChart.Series["湿度"].Points.Clear();
        }
        private bool HasErrorText()
        {
            bool hasErrorText = false;
            foreach (DataGridViewRow row in this.data1.Rows)
            {
                foreach (DataGridViewCell cell in row.Cells)
                {
                    if (cell.ErrorText.Length > 0)
                    {
                        hasErrorText = true;
                        break;
                    }
                }
                if (hasErrorText)
                    break;
            }

            return hasErrorText;
        }
        private void Cell_Validated(object sender, DataGridViewCellEventArgs e)
        {
                DataGridViewCell currentCell = data1.Rows[e.RowIndex].Cells[e.ColumnIndex];
                double value=Convert.ToDouble(data1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value);
                var def = MMI_Project_CSharp.Properties.Settings.Default;

                if (e.RowIndex == 0) 
                { 
                    if (!(Convert.ToDouble(value) <= def.UpperTime &&
                         Convert.ToDouble(value) >= 0))
                    {
                        currentCell.ErrorText = "時間は 0~" + def.UpperTime + "分でなければいけません。";
                        currentCell.Value = "0";
                    }
                    else
                    {
                        currentCell.ErrorText = "";
                    }
                }
                else if (e.RowIndex == 1) 
                { 
                    if (!(Convert.ToDouble(value) <= def.UpperTemp &&
                            Convert.ToDouble(value) >= def.LowerTemp))
                    {
                        currentCell.ErrorText = "温度は" + def.LowerTemp + "~" + def.UpperTemp + "℃でなければいけません。";
                        currentCell.Value = "0";
                    }
                    else
                    {
                        currentCell.ErrorText = "";
                    }
                }
                else 
                { 
                    if (!(Convert.ToDouble(value) <= def.UpperHum &&
                            Convert.ToDouble(value) >= def.LowerHum))
                    {
                        currentCell.ErrorText = "湿度は" + def.LowerHum + "~" + def.UpperHum + "%でなければいけません。";
                        currentCell.Value = "0";
                    }
                    else
                    {
                        currentCell.ErrorText = "";
                    }
                }
                check_Time();
        }
        private void btnWrite_Click(object sender, EventArgs e)
        {
            if (!HasErrorText())
            {
                int start = patternStart + ((Convert.ToInt32(numUpDown.Text) - 1) * 72);
                try
                {
                    List<string> targetTime = new List<string>();
                    List<string> targetTmp = new List<string>();
                    List<string> targetHmd = new List<string>();
                    List<object> valTime = new List<object>();
                    List<object> valTmp = new List<object>();
                    List<object> valHmd = new List<object>();

                    for (int i = 0; i < 24; i++) 
                    {
                        targetTime.Add( DEV_NAME + "." + WORD_PREF + (start + i).ToString("X") );
                        targetTmp.Add( DEV_NAME + "." + WORD_PREF + (start + 24 + i).ToString("X") ); 
                        targetHmd.Add( DEV_NAME + "." + WORD_PREF + (start + 48 + i).ToString("X") );
                        valTime.Add( data1.Rows[0].Cells[i].Value );
                        valTmp.Add( Convert.ToDouble(data1.Rows[1].Cells[i].Value) * 10);
                        valHmd.Add( Convert.ToDouble(data1.Rows[2].Cells[i].Value) * 10 );
                    }
                    int[] nErrorArray = new int[]{0};
                    opc.Write(targetTime.ToArray(), valTime.ToArray(), out nErrorArray);
                    opc.Write(targetTmp.ToArray(), valTmp.ToArray(), out nErrorArray);
                    opc.Write(targetHmd.ToArray(), valHmd.ToArray(), out nErrorArray);
                    if (nErrorArray[0] != 0)
                    {
                        return;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
            else
            {
                MessageBox.Show("エラーが出ているため登録できません。", "書込ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
        private void check_Time()
        {
            for (int i = 0; i < 24; i++)
            {
                time_Read(i);
            }
        }
        private void time_Read(int i)
        {
            DataGridViewCell currentCellTime = data1.Rows[0].Cells[i];
            DataGridViewCell currentCellTemp = data1.Rows[1].Cells[i];
            DataGridViewCell currentCellHum = data1.Rows[2].Cells[i];
            if (Convert.ToDouble(currentCellTime.Value) > 0 && i != 0)
            {
                double prevCellTime = 0;
                DataGridViewCell prevCellTemp = data1.Rows[1].Cells[i - 1];
                for (int prev = i; prev > 0; prev--)
                {
                    if (Convert.ToDouble(data1.Rows[0].Cells[prev - 1].Value) > 0)
                    {
                        prevCellTime = Convert.ToDouble(data1.Rows[0].Cells[prev - 1].Value);
                        prevCellTemp = data1.Rows[1].Cells[prev - 1];
                        break;
                    }
                }
                if (prevCellTime > 0)
                {
                    double value = Convert.ToDouble(currentCellTime.Value) * 0.6;
                    if ((Convert.ToDouble(currentCellTemp.Value) <= (Convert.ToDouble(prevCellTemp.Value) + value) &&
                         Convert.ToDouble(currentCellTemp.Value) >= (Convert.ToDouble(prevCellTemp.Value) - value)))
                    {
                        if ((currentCellTemp.ErrorText.Length == 0))
                        {
                            currentCellTemp.ErrorText = "";
                        }
                    }
                    else
                    {
                        currentCellTemp.ErrorText = "温度変化は 0.6℃/分を超えることは出来ません。";
                    }
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {

                if (opc.Connect("localhost", "Takebishi.dxp"))
                {
                    Debug.WriteLine("Connected");

                    ds = new DataSet();
                    dt = new DataTable();


                    dt = initial_Value();
                    ds.Tables.Add(dt);

                    flip();
                }
                else
                {
                    Debug.WriteLine("Failed to connect");
                }
            }
            catch (Exception)
            {
                Debug.WriteLine("Failed to connect");
            }
        }
        private static DataTable initial_Value()
        {
            DataTable table = new DataTable();
            table.TableName = "Graph";

            table.Columns.Add("時間", typeof(int));
            table.Columns.Add("温度", typeof(double));
            table.Columns.Add("湿度", typeof(double));

            for (int i = 0; i < 24; i++)
            {
                table.Rows.Add(new object[] { 0, 0.0, 0.0 });
            }
            table.AcceptChanges();
            return table;
        }

        private void btnRead_Click(object sender, EventArgs e)
        {
            ds = new DataSet();
            dt = new DataTable();

            
            dt = GetGraph(numUpDown.Text);
            ds.Tables.Add(dt);

            flip();

            check();

        }

        private void flip()
        {
            DataSet new_ds = FlipDataSet(ds);
            DataView my_DataView = new_ds.Tables[0].DefaultView;
            data1.DataSource = my_DataView;

            data1.Rows[0].DefaultCellStyle.Format = "N0";
            data1.Rows[1].DefaultCellStyle.Format = data1.Rows[2].DefaultCellStyle.Format = "N1";
            data1.Rows[0].Frozen = data1.Rows[1].Frozen = data1.Rows[2].Frozen = true;
            foreach (DataGridViewColumn column in data1.Columns)
            {
                column.SortMode = DataGridViewColumnSortMode.NotSortable;
            }
        }

        public DataSet FlipDataSet(DataSet my_DataSet)
        {
            DataSet ds = new DataSet();

            foreach (DataTable dt in my_DataSet.Tables)
            {
                DataTable table = new DataTable();

                for (int i = 1; i <= dt.Rows.Count; i++)
                {
                    table.Columns.Add(Convert.ToString(i), typeof(double));
                }
                DataRow r;
                for (int k = 0; k < dt.Columns.Count; k++)
                {
                    r = table.NewRow();
                    for (int j = 0; j < dt.Rows.Count; j++)
                        r[j] = dt.Rows[j][k];
                    table.Rows.Add(r);
                }

                ds.Tables.Add(table);
            }

            return ds;
        }
        private DataTable GetGraph(string num)
        {
            int start = patternStart + ((Convert.ToInt32(num) - 1) * 72);
           
            DataTable table = new DataTable();
            table.TableName = "Graph";

            table.Columns.Add("時間", typeof(double));
            table.Columns.Add("温度", typeof(double));
            table.Columns.Add("湿度", typeof(double));

            string[] target;
            object[] oValueArray;
            short[] wQualityArray;
            FILETIME[] fTimeArray;
            int[] nErrorArray;
            for (int i = 0; i < 24; i++)
            {
                target=new string[] { DEV_NAME + "." + WORD_PREF + "" + (start + i).ToString("X"), 
                                       DEV_NAME + "." + WORD_PREF + "" + (start + 24 + i).ToString("X"),
                                       DEV_NAME + "." + WORD_PREF + "" + (start + 48 + i).ToString("X") };
                try
                {
                    if (opc.Read(target.ToArray(), out oValueArray, out wQualityArray, out fTimeArray, out nErrorArray))
                    {
                        table.Rows.Add(new object[] { oValueArray[0], Convert.ToDouble(oValueArray[1]) / 10, Convert.ToDouble(oValueArray[2]) / 10 });
                    }
                }
                catch (Exception) { }
            }
            table.AcceptChanges();
            return table;
        }
        private void data1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.' && e.KeyChar!='-')
            {
                e.Handled = true;
            }
        }
        private void Form1_Resize(object sender, EventArgs e)
        {
            if (this.Width < 871) this.Width = 871;
            if (this.Height < 526) this.Height = 526;
        }

        private void data1_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (data1.CurrentCell.ColumnIndex >= 0)
            {
                e.Control.KeyPress += new KeyPressEventHandler(data1_KeyPress);
            }
        }

        private void data1_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            double newValue;
            if (data1.Rows[e.RowIndex].IsNewRow) { return; }
            if (!double.TryParse(e.FormattedValue.ToString(), out newValue))
            {
                MessageBox.Show("数字は整数でなければいけません。", "入力ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                e.Cancel = true;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ds = new DataSet();
            dt = new DataTable();
            dt = GetGraphFile(txtFile.Text);
            ds.Tables.Add(dt);
            flip();
            check();
        }
        private DataTable GetGraphFile(string num)
        {            
            DataTable table = new DataTable();
            table.TableName = "Graph";

            table.Columns.Add("時間", typeof(double));
            table.Columns.Add("温度", typeof(double));
            table.Columns.Add("湿度", typeof(double));

            OpenFileDialog oFDialog = new OpenFileDialog();
            oFDialog.FileName = txtFile.Text + ".csv";
            oFDialog.Filter = txtFile.Text + ".csv|*.csv";
            if (oFDialog.ShowDialog() == DialogResult.OK)
            {
                using (TextFieldParser tfp = new TextFieldParser(oFDialog.FileName))
                {
                    tfp.Delimiters = new string[] { "," };
                    while (!tfp.EndOfData)
                    {
                        string[] fields = tfp.ReadFields();
                        table.Rows.Add(new object[] { fields[0], Convert.ToDouble(fields[1]) / 10, Convert.ToDouble(fields[2]) / 10 });
                    }
                    table.AcceptChanges();
                }
            }
            else
            {
                for (int i = 0; i < 24; i++)
                {
                    table.Rows.Add(new object[] { data1.Rows[0].Cells[i].Value,
                                                  Convert.ToDouble(data1.Rows[1].Cells[i].Value),
                                                  Convert.ToDouble(data1.Rows[2].Cells[i].Value)});
                }
                table.AcceptChanges();
            }
            return table;
        }
        private void button3_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 24; i++)
            {
                if (Convert.ToDouble(data1.Rows[0].Cells[i].Value) == 0)
                {
                    data1.Rows[1].Cells[i].Value = "0";
                    data1.Rows[2].Cells[i].Value = "0";
                }
            }
            if (!HasErrorText())
            {
                if(txtFile.Text!="")
                {
                    SaveFileDialog saveFileDialog = new SaveFileDialog();
                    saveFileDialog.FileName = txtFile.Text + ".csv";
                    saveFileDialog.Filter = txtFile.Text + ".csv|*.csv";
                    StringBuilder sb = new StringBuilder();
                    if (saveFileDialog.ShowDialog() == DialogResult.OK) { 
                        for (int a = 0; a < 24; a++)
                        {
                            sb.AppendLine(string.Format("{0},{1},{2}", data1.Rows[0].Cells[a].Value,
                                                                        Convert.ToDouble(data1.Rows[1].Cells[a].Value) * 10,
                                                                        Convert.ToDouble(data1.Rows[2].Cells[a].Value) * 10));
                        }
                        File.WriteAllText(saveFileDialog.FileName, sb.ToString());
                    }
                }
                else
                {
                    MessageBox.Show("保存するファイル名を指定してください。","ファイル書込ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("エラーが出ているためファイルを書き込めません。", "書込ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnUpdate(object sender, EventArgs e)
        {
            check();
        }

        private void Form1_Deactivate(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }
    }
}
