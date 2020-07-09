using System;
using System.Data;
using System.IO;
using System.IO.Ports;
using System.Text;

namespace OrionMassCommandSender.BL
{
    public class Orion
    {
        private OrionStates MyState;

		private SerialPort Port;

		private StringBuilder sb;

		public OrionStates CurrentState
		{
			get
			{
				return this.MyState;
			}
			set
			{
				this.MyState = value;
			}
		}

		public string PortName
		{
			get
			{
				return this.Port.PortName;
			}
		}

		public Orion(SerialPort sp)
		{
			this.sb = new StringBuilder();
			this.Port = sp;
			this.Port.DataReceived += new SerialDataReceivedEventHandler(this.Port_DataReceived);
		}

		private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
		{
			int bytesToRead = this.Port.BytesToRead;
			byte[] array = new byte[bytesToRead];
			this.Port.Read(array, 0, array.Length);
			this.CatchAnyWord(array);
		}

		public void Open()
		{
			bool flag = !this.Port.IsOpen;
			if (flag)
			{
				this.Port.Open();
				this.Port.DiscardInBuffer();
				this.Port.DiscardOutBuffer();
			}
		}

		public void Send(byte[] buf)
		{
            this.Port.Write(buf, 0, buf.Length);
		}

		private bool CatchOrionSharp(StringBuilder sb)
		{
			string text = sb.ToString();
			int num = text.LastIndexOf(' ');
			bool flag = num > 0;
			bool result;
			if (flag)
			{
				bool flag2 = text[num - 1] == '#';
				if (flag2)
				{
					result = true;
					return result;
				}
			}
			result = false;
			return result;
		}

		private void CatchAnyWord(byte[] buf)
		{
			switch (this.MyState)
			{
			case OrionStates.WaitingForConnection:
				for (int i = 0; i < buf.Length; i++)
				{
					bool flag = buf[i] == 36;
					if (flag)
					{
						this.Send(BitConverter.GetBytes('~'));
						this.MyState = OrionStates.SendingTilda;
						this.MyState = OrionStates.WaitingForArrow;
					}
				}
				break;
			case OrionStates.WaitingForArrow:
				for (int j = 0; j < buf.Length; j++)
				{
					bool flag2 = buf[j] == BitConverter.GetBytes('>')[0];
					if (flag2)
					{
						this.Send(Encoding.ASCII.GetBytes("setmode dinamit\r"));
						this.MyState = OrionStates.DinamitMode;
						this.MyState = OrionStates.WaitingForWelcome;
					}
				}
				break;
			case OrionStates.WaitingForWelcome:
			{
				this.sb.Append(Encoding.ASCII.GetString(buf));
				bool flag3 = this.CatchOrionSharp(this.sb);
				if (flag3)
				{
					this.MyState = OrionStates.Connected;
					this.sb.Clear();
				}
				break;
			}
			case OrionStates.WaitingForComExec:
			{
				this.sb.Append(Encoding.ASCII.GetString(buf));
				bool flag4 = this.CatchOrionSharp(this.sb);
				if (flag4)
				{
					this.MyState = OrionStates.Done;
				}
				break;
			}
			case OrionStates.Done:
				this.MyState = OrionStates.WaitingForConnection;
				break;
			case OrionStates.ShuttedDown:
				this.MyState = OrionStates.WaitingForConnection;
				break;
			}
		}

		public static Orion[] CreateDevices(DataTable tbl)
		{
			bool flag = tbl == null;
			Orion[] result;
			if (flag)
			{
				result = null;
			}
			else
			{
				Orion[] array = new Orion[tbl.Rows.Count];
				for (int i = 0; i < array.Length; i++)
				{
					string portName = (string)tbl.Rows[i][0];
					int baudRate = (int)tbl.Rows[i][1];
					array[i] = new Orion(new SerialPort(portName, baudRate, Parity.None, 8, StopBits.One));
				}
				result = array;
			}
			return result;
		}
    }
}