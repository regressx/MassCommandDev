namespace OrionMassCommandSenderOld
{
    using System;
    using System.Data;
    using System.IO;
    using System.IO.Ports;
    using System.Text;

    public class Orion
    {
        private States MyState;
        private SerialPort Port;
        private StringBuilder sb;

        public Orion(SerialPort sp)
        {
            this.sb = new StringBuilder();
            this.Port = sp;
            this.Port.DataReceived += new SerialDataReceivedEventHandler(this.Port_DataReceived);
        }

        private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                byte[] numArray = new byte[this.Port.BytesToRead];
                this.Port.Read(numArray, 0, numArray.Length);
                this.CatchAnyWord(numArray);
            }
            catch (IOException ex)
            {
                Logger.AddLog(string.Format(
                    "Ошибка ввода/вывода при получении данных из порта {0}, а в стеке вызовов метода {1} с текстом: {2}",
                    (object) this.Port.PortName, (object) ex.TargetSite, (object) ex.Message));
            }
            catch (Exception ex)
            {
                Logger.AddLog(string.Format(
                    "Ошибка при получении данных из порта {0}, а в стеке вызовов метода {1} с текстом: {2}",
                    (object) this.Port.PortName, (object) ex.TargetSite, (object) ex.Message));
            }
        }

        public void Open()
        {
            try
            {
                if (this.Port.IsOpen)
                    return;
                this.Port.Open();
                this.Port.DiscardInBuffer();
                this.Port.DiscardOutBuffer();
            }
            catch (IOException ex)
            {
                Logger.AddLog(string.Format(
                    "Ошибка ввода/вывода в методе Open порта {0}, а в стеке вызовов метода {1} с текстом: {2}",
                    (object) this.Port.PortName, (object) ex.TargetSite, (object) ex.Message));
                if (!this.Port.IsOpen)
                    return;
                this.Port.Close();
            }
            catch (Exception ex)
            {
                Logger.AddLog(string.Format(
                    "Ошибка в методе Open порта {0}, а в стеке вызовов метода {1} с текстом: {2}",
                    (object) this.Port.PortName, (object) ex.TargetSite, (object) ex.Message));
            }
        }

        public void Send(byte[] buf)
        {
            if (this.Port.IsOpen)
            {
                try
                {
                    this.Port.Write(buf, 0, buf.Length);
                }
                catch (IOException ex)
                {
                    Logger.AddLog(string.Format(
                        "Ошибка ввода/вывода в методе Send порта {0}, а в стеке вызовов метода {1} с текстом: {2}",
                        (object) this.Port.PortName, (object) ex.TargetSite, (object) ex.Message));
                }
                catch (Exception ex)
                {
                    Logger.AddLog(string.Format(
                        "Ошибка в методе Send порта {0}, а в стеке вызовов метода {1} с текстом: {2}",
                        (object) this.Port.PortName, (object) ex.TargetSite, (object) ex.Message));
                }
            }
            else
                Logger.AddLog(string.Format("Не могу послать запрос: порт {0} закрыт", (object) this.Port.PortName));
        }

        private bool CatchOrionSharp(StringBuilder sb)
        {
            string str = sb.ToString();
            int num = str.LastIndexOf(' ');
            return num > 0 && str[num - 1] == '#';
        }

        private void CatchAnyWord(byte[] buf)
        {
            switch (this.MyState)
            {
                case States.WaitingForConnection:
                    for (int index = 0; index < buf.Length; ++index)
                    {
                        if (buf[index] == (byte) 36)
                        {
                            this.Send(BitConverter.GetBytes('~'));
                            this.MyState = States.SendingTilda;
                            this.MyState = States.WaitingForArrow;
                        }
                    }

                    break;
                case States.WaitingForArrow:
                    for (int index = 0; index < buf.Length; ++index)
                    {
                        if ((int) buf[index] == (int) BitConverter.GetBytes('>')[0])
                        {
                            this.Send(Encoding.ASCII.GetBytes("setmode dinamit\r"));
                            this.MyState = States.DinamitMode;
                            this.MyState = States.WaitingForWelcome;
                        }
                    }

                    break;
                case States.WaitingForWelcome:
                    this.sb.Append(Encoding.ASCII.GetString(buf));
                    if (!this.CatchOrionSharp(this.sb))
                        break;
                    this.MyState = States.Connected;
                    this.sb.Clear();
                    break;
                case States.WaitingForComExec:
                    this.sb.Append(Encoding.ASCII.GetString(buf));
                    if (!this.CatchOrionSharp(this.sb))
                        break;
                    this.MyState = States.Done;
                    break;
                case States.Done:
                    this.MyState = States.WaitingForConnection;
                    break;
                case States.ShuttedDown:
                    this.MyState = States.WaitingForConnection;
                    break;
            }
        }

        public States CurrentState
        {
            get { return this.MyState; }
            set { this.MyState = value; }
        }

        public static Orion[] CreateDevices(DataTable tbl)
        {
            if (tbl == null)
            {
                Logger.AddLog("Программа не смогла создать устройства из-за отсутствия верного файла настроек");
                return (Orion[]) null;
            }

            Orion[] orionArray = new Orion[tbl.Rows.Count];
            for (int index = 0; index < orionArray.Length; ++index)
            {
                string portName = (string) tbl.Rows[index][0];
                int baudRate = (int) tbl.Rows[index][1];
                orionArray[index] = new Orion(new SerialPort(portName, baudRate, Parity.None, 8, StopBits.One));
            }

            return orionArray;
        }

        public string PortName
        {
            get { return this.Port.PortName; }
        }
    }
}