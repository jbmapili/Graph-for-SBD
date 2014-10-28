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
        string BIT_PREF = MMI_Project_CSharp.Properties.Settings.Default.Bit_Reg_Prefix;
        int TEM_POS = MMI_Project_CSharp.Properties.Settings.Default.Temp_Pos;
        int HUM_POS = MMI_Project_CSharp.Properties.Settings.Default.Hum_Pos;
        int TIM_POS = MMI_Project_CSharp.Properties.Settings.Default.Time_Pos;
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
                    lineChart.Series["Temperature"].Points[point].Label = (i + 1).ToString();
                    point++;
                }
                else
                {
                    break;
                }
            }
            if (point > 0)
            { 
                lineChart.Series["Temperature"].Points[point].Label = (Convert.ToInt32(lineChart.Series["Temperature"].Points[point-1].Label.ToString()) + 1).ToString();
            }
            lineChart.Series["Temperature"].Points[1].BorderDashStyle = ChartDashStyle.Dash;
            lineChart.Series["Humidity"].Points[1].BorderDashStyle = ChartDashStyle.Dash;
            axisY_Maximum();
            axisY2_Maximum();
            lineChart.ChartAreas[0].AxisX.Maximum = Math.Ceiling(timeLine);
            lineChart.ChartAreas[0].AxisX.Minimum = 0;
            lineChart.ChartAreas[0].AxisX.ScaleView.Zoom(0, 10);
        }
        private void axisY_Maximum()
        {
            double max=0;
            double min=0;
            for (int a = 0; a < 24; a++)
            {
                double value=Convert.ToDouble(data1.Rows[1].Cells[a].Value);
                if ( value> max) max = value;
                else if (value < min) min = value;
            }
            if (max > 40) { 
                lineChart.ChartAreas[0].AxisY.Maximum = Math.Ceiling(max);
            }
            if (min < -40) { 
                lineChart.ChartAreas[0].AxisY.Minimum = Math.Floor(min);
            }
        }
        private void axisY2_Maximum()
        {
            double max = 0;
            double min = 0;
            for (int a = 0; a < 24; a++)
            {
                double value = Convert.ToDouble(data1.Rows[2].Cells[a].Value);
                if (value > max) max = value;
                else if (value < min) min = value;
            }
            if (max > 100)
            {
                lineChart.ChartAreas[0].AxisY2.Maximum = Math.Ceiling(max);
            }
            else
            {
                lineChart.ChartAreas[0].AxisY2.Maximum = 100;
            }
            if (min < 0)
            {
                lineChart.ChartAreas[0].AxisY2.Minimum = Math.Floor(min);
            }
            else
            {
                lineChart.ChartAreas[0].AxisY2.Minimum = 0;
            }
        }

        private void plot_Line(double timeLine, double temp, double humid)
        {
                lineChart.Series["Temperature"].Points.AddXY(timeLine, temp);
                lineChart.Series["Humidity"].Points.Add(new DataPoint(timeLine, humid));
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            checkStepOne();
        }
        private void write()
        {
            int start = MMI_Project_CSharp.Properties.Settings.Default.Pattern1Start + ((Convert.ToInt32(numUpDown.Text) - 1) * 72);
            try
            {
                for (int i = 0; i < 24; i++)
                {
                    string[] target = new string[] { DEV_NAME + "." + WORD_PREF + (start + i).ToString("X"),
                                                             DEV_NAME + "." + WORD_PREF + (start + 24 + i).ToString("X"), 
                                                             DEV_NAME + "." + WORD_PREF + (start + 48 + i).ToString("X") 
                                                           };
                    object[] val = new object[] { data1.Rows[0].Cells[i].Value, 
                                                  Convert.ToDouble(data1.Rows[1].Cells[i].Value) * 10, 
                                                  Convert.ToDouble(data1.Rows[2].Cells[i].Value) * 10 
                                                };
                    int[] nErrorArray;
                    opc.Write(target, val, out nErrorArray);
                    if (nErrorArray[0] != 0)
                    {
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        private void check()
        {
            check_Time();
            if (!HasErrorText())
            {
                clear();
                update_Graph();
            }
            else
            {
                MessageBox.Show("You can't update the table as long as there is an error.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void clear()
        {
            lineChart.Series["Temperature"].Points.Clear();
            lineChart.Series["Humidity"].Points.Clear();
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
            check_Time();
        }
        private void btnWrite_Click(object sender, EventArgs e)
        {
            check_Time();
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

                    int start = MMI_Project_CSharp.Properties.Settings.Default.Pattern1Start + ((Convert.ToInt32(numUpDown.Text) - 1) * 72);
                　 　try
                    {
                        for (int i = 0; i < 24; i++) 
                        {
                            string[] target = new string[] { DEV_NAME + "." + WORD_PREF + (start + i).ToString("X"),
                                                             DEV_NAME + "." + WORD_PREF + (start + 24 + i).ToString("X"), 
                                                             DEV_NAME + "." + WORD_PREF + (start + 48 + i).ToString("X") 
                                                           };
                            object[] val = new object[] { data1.Rows[0].Cells[i].Value, Convert.ToDouble(data1.Rows[1].Cells[i].Value) * 10, Convert.ToDouble(data1.Rows[2].Cells[i].Value) * 10 };
                            int[] nErrorArray;
                            opc.Write(target, val, out nErrorArray);
                            if (nErrorArray[0] != 0)
                            {
                                return;
                            }
                         }
                        write();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                }
                else
                {
                    MessageBox.Show("You can't write the values as long as there is an error.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            if (!(Convert.ToDouble(currentCellTime.Value) <= MMI_Project_CSharp.Properties.Settings.Default.UpperTime &&
                 Convert.ToDouble(currentCellTime.Value) >= 0))
            {
                currentCellTime.ErrorText = "Time must be between 0~" + MMI_Project_CSharp.Properties.Settings.Default.UpperTime;
                currentCellTime.Value = "0";
            }
            else
            {
                currentCellTime.ErrorText = "";
            }
            if (!(Convert.ToDouble(currentCellTemp.Value) <= MMI_Project_CSharp.Properties.Settings.Default.UpperTemp &&
                    Convert.ToDouble(currentCellTemp.Value) >= MMI_Project_CSharp.Properties.Settings.Default.LowerTemp))
            {
                currentCellTemp.ErrorText = "Temperature must be between" + MMI_Project_CSharp.Properties.Settings.Default.LowerTemp + "~" + MMI_Project_CSharp.Properties.Settings.Default.UpperTemp;
                currentCellTemp.Value = "0";
            }
            else
            {
                currentCellTemp.ErrorText = "";
            }
            if (!(Convert.ToDouble(currentCellHum.Value) <= MMI_Project_CSharp.Properties.Settings.Default.UpperHum &&
                    Convert.ToDouble(currentCellHum.Value) >= MMI_Project_CSharp.Properties.Settings.Default.LowerHum))
            {
                currentCellHum.ErrorText = "Humidity must be between" + MMI_Project_CSharp.Properties.Settings.Default.LowerHum + "~" + MMI_Project_CSharp.Properties.Settings.Default.UpperHum;
                currentCellHum.Value = "0";
            }
            else
            {
                currentCellHum.ErrorText = "";
            }
            if (Convert.ToDouble(currentCellTime.Value) > 0 && i != 0)
            {
                double prevCellTime = 0;
                DataGridViewCell prevCellTemp = data1.Rows[1].Cells[i-1];
                for (int prev = i; prev > 0; prev--)
                {
                    if (Convert.ToDouble(data1.Rows[0].Cells[prev-1].Value) > 0)
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
                        currentCellTemp.ErrorText = "";
                    }
                    else
                    {
                        currentCellTemp.ErrorText="The value may increase or decrease for 0.6 in 1 minute.";                      
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

            table.Columns.Add("Time", typeof(int));
            table.Columns.Add("Temperature", typeof(double));
            table.Columns.Add("Humidity", typeof(double));

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

            checkStepOne();

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

            table.Columns.Add("Time", typeof(double));
            table.Columns.Add("Temperature", typeof(double));
            table.Columns.Add("Humidity", typeof(double));

            object[] oValueArray;
            short[] wQualityArray;
            OpcRcw.Da.FILETIME[] fTimeArray;
            int[] nErrorArray;
            for (int i = 0; i < 24; i++)
            {
                string[] target = {
                                      DEV_NAME + "." + WORD_PREF + "" + (start + i).ToString("X"),
                                      DEV_NAME + "." + WORD_PREF + "" + (start + 24 + i).ToString("X"),
                                      DEV_NAME + "." + WORD_PREF + "" + (start + 48 + i).ToString("X"),                                  
                                  };
                try
                {
                    if (opc.Read(target, out oValueArray, out wQualityArray, out fTimeArray, out nErrorArray))
                    {
                        table.Rows.Add(new object[] { oValueArray[0], Convert.ToDouble(oValueArray[1]) / 10, Convert.ToDouble(oValueArray[2]) / 10 });
                    }
                }
                catch (Exception) { }
            }
            table.AcceptChanges();
            return table;
        }
        private void checkStepOne()
        {
            if (Convert.ToDouble(data1.Rows[0].Cells[0].Value) != 0) 
            { 
                check(); 
            }
            else
            {
                clear();
                MessageBox.Show("Step 1 time value is needed", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void data1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.')
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
                MessageBox.Show("Value must be numeric!", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

            //checkStepOne();
        }
        private DataTable GetGraphFile(string num)
        {            
            DataTable table = new DataTable();
            table.TableName = "Graph";

            table.Columns.Add("Time", typeof(double));
            table.Columns.Add("Temperature", typeof(double));
            table.Columns.Add("Humidity", typeof(double));

            OpenFileDialog oFDialog = new OpenFileDialog();
            try
            {
                oFDialog.FileName = Application.StartupPath + "\\PatternFiles\\" + num + ".csv";
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
            catch (Exception)
            {
                MessageBox.Show("File not found ! Please check the filename", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

            check_Time();
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
                    try
                    {
                        SaveFileDialog saveFileDialog = new SaveFileDialog();
                        saveFileDialog.FileName = Application.StartupPath + "\\PatternFiles\\" + txtFile.Text + ".csv";
                        StringBuilder sb = new StringBuilder();
                        for (int a = 0; a < 24; a++)
                        {
                            sb.AppendLine(string.Format("{0},{1},{2}", data1.Rows[0].Cells[a].Value,
                                                                       Convert.ToDouble(data1.Rows[1].Cells[a].Value) * 10,
                                                                       Convert.ToDouble(data1.Rows[2].Cells[a].Value) * 10));
                        }
                        File.WriteAllText(saveFileDialog.FileName, sb.ToString());
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                }
                else
                {
                    MessageBox.Show("Provide filename before saving","ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("You can't write the values as long as there is an error.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
