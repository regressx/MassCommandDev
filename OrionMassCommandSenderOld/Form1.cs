using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace OrionMassCommandSenderOld
{
    using System.Reflection;

    public partial class Form1 : Form
    {
        private Timer t;
        private Orion[] orions;

        public Form1()
        {
            InitializeComponent();

            this.t = new Timer();
            this.t.Interval = 100;
            this.t.Tick += new EventHandler(this.T_Tick);
            Logger.LogMessageReceived += new EventHandler<LogMessageReceivedEventArgs>(this.Logger_LogMessageReceived);
        }

        private void Logger_LogMessageReceived(object sender, LogMessageReceivedEventArgs e)
        {
            if (this.textBox1.InvokeRequired)
                this.textBox1.Invoke(new Action((() => this.textBox1.AppendText(e.Message))));
            else
                this.textBox1.AppendText(e.Message);
        }


        private void T_Tick(object sender, EventArgs e)
        {
            int index1 = 0;
            while (true)
            {
                int num1 = index1;
                Point tableProperties = Loader.TableProperties;
                int x = tableProperties.X;
                if (num1 < x)
                {
                    int index2 = 0;
                    while (true)
                    {
                        int num2 = index2;
                        tableProperties = Loader.TableProperties;
                        int y1 = tableProperties.Y;
                        if (num2 < y1)
                        {
                            DataGridViewCell dataGridViewCell = this.dataGridView1[index1, index2];
                            Orion[] orions1 = this.orions;
                            int num3 = index1;
                            tableProperties = Loader.TableProperties;
                            int y2 = tableProperties.Y;
                            int index3 = num3 * y2 + index2;
                            string portName = orions1[index3].PortName;
                            Orion[] orions2 = this.orions;
                            int num4 = index1;
                            tableProperties = Loader.TableProperties;
                            int y3 = tableProperties.Y;
                            int index4 = num4 * y3 + index2;

                            // ISSUE: variable of a boxed type
                            States currentState = orions2[index4].CurrentState;
                            string str = string.Format("{0}: {1}", (object) portName, (object) currentState);
                            dataGridViewCell.Value = (object) str;
                            Orion[] orions3 = this.orions;
                            int num5 = index1;
                            tableProperties = Loader.TableProperties;
                            int y4 = tableProperties.Y;
                            int index5 = num5 * y4 + index2;
                            if (orions3[index5].CurrentState == States.Connected)
                                this.dataGridView1[index1, index2].Style.BackColor = Color.Yellow;
                            Orion[] orions4 = this.orions;
                            int num6 = index1;
                            tableProperties = Loader.TableProperties;
                            int y5 = tableProperties.Y;
                            int index6 = num6 * y5 + index2;
                            if (orions4[index6].CurrentState == States.Done)
                                this.dataGridView1[index1, index2].Style.BackColor = Color.LightGreen;
                            ++index2;
                        }
                        else
                            break;
                    }

                    ++index1;
                }
                else
                    break;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Text = string.Format("{0}, ver.{1}", (object) this.Text,
                (object) Assembly.GetEntryAssembly().GetName().Version);
            this.orions = Orion.CreateDevices(Loader.ReadOptions("Ports.xml"));
            if (this.orions == null)
            {
                Logger.AddLog(
                    "Программа не смогла воспроизвести таблицу портов. Проверьте настройки и перезапустите программу");
            }
            else
            {
                for (int index = 0; index < this.orions.Length; ++index)
                    this.orions[index].Open();
                for (int index = 0; index < Loader.TableProperties.X; ++index)
                    this.dataGridView1.Columns.Add("", "");
                DataGridViewRowCollection rows = this.dataGridView1.Rows;
                Point tableProperties = Loader.TableProperties;
                int y1 = tableProperties.Y;
                rows.Add(y1);
                int index1 = 0;
                while (true)
                {
                    int num1 = index1;
                    tableProperties = Loader.TableProperties;
                    int x = tableProperties.X;
                    if (num1 < x)
                    {
                        int index2 = 0;
                        while (true)
                        {
                            int num2 = index2;
                            tableProperties = Loader.TableProperties;
                            int y2 = tableProperties.Y;
                            if (num2 < y2)
                            {
                                DataGridViewCell dataGridViewCell = this.dataGridView1[index1, index2];
                                Orion[] orions = this.orions;
                                int num3 = index1;
                                tableProperties = Loader.TableProperties;
                                int y3 = tableProperties.Y;
                                int index3 = num3 * y3 + index2;
                                string portName = orions[index3].PortName;
                                dataGridViewCell.Value = (object) portName;
                                ++index2;
                            }
                            else
                                break;
                        }

                        ++index1;
                    }
                    else
                        break;
                }

                this.dataGridView1.ClearSelection();
                this.t.Start();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (this.orions == null)
                return;
            for (int index = 0; index < this.orions.Length; ++index)
            {
                if (this.orions[index].CurrentState == States.Connected)
                {
                    this.orions[index].Send(Encoding.ASCII.GetBytes(this.textBox2.Text + "\r"));
                    this.orions[index].CurrentState = States.WaitingForComExec;
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (this.orions == null)
                return;
            for (int index = 0; index < this.orions.Length; ++index)
            {
                if (this.orions[index].CurrentState >= States.Connected)
                {
                    this.orions[index].Send(Encoding.ASCII.GetBytes(this.textBox3.Text + "\r"));
                    this.orions[index].CurrentState = States.WaitingForComExec;
                }
            }
        }

        private void CleanStateDevMenuItem_Click(object sender, EventArgs e)
        {
            Point currentCellAddress = this.dataGridView1.CurrentCellAddress;
            this.orions[currentCellAddress.X * Loader.TableProperties.Y + currentCellAddress.Y].CurrentState =
                States.WaitingForConnection;
            this.dataGridView1[currentCellAddress.X, currentCellAddress.Y].Style.BackColor = Color.White;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (this.orions == null)
                return;
            for (int index = 0; index < this.orions.Length; ++index)
            {
                this.orions[index].Send(Encoding.ASCII.GetBytes("power off\r"));
                this.orions[index].CurrentState = States.ShuttedDown;
            }
        }
    }
}