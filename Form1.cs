using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
	public partial class TheForm : Form
	{
		static TheForm self;
		const int UDP_REBOOT_PORT = 37501;
		UdpClient udp_recv;
		static Thread thread;
		static bool exit_thread = false;

		public TheForm()
		{
			self = this;
			InitializeComponent();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			udp_recv = new UdpClient(UDP_REBOOT_PORT);
			thread = new Thread(ReceiveProc);
			thread.Start();
		}

		public static void ReceiveProc()
		{
			while (!exit_thread)
			{
				self.ReceiveThreadProc();
				Thread.Sleep(1);
			}
		}

		public void ReceiveThreadProc()
		{
			var enc = System.Text.Encoding.UTF8;

			if (udp_recv.Available > 0)
			{
				// Receive udp data
				IPEndPoint remoteEP = null;
				byte[] rcvBytes = udp_recv.Receive(ref remoteEP);
				if (rcvBytes.Length > 0)
				{
					Debug.Print("[udp recv] " + rcvBytes.Length.ToString() + " bytes received");
					string rcvMsg = enc.GetString(rcvBytes);
					rcvMsg = rcvMsg.Replace('\t', ' ').Replace('\n', ' ').Trim();
					string[] cmds = rcvMsg.Split('|');
					if (cmds.Length >= 2)
					{
						DoCommand(cmds[0], cmds[1]);
					}
					else if (cmds.Length>=1)
					{
						DoCommand(cmds[0], "");
					}
				}
			}
		}

		void DoCommand(string cmd, string arg)
		{
			if (cmd == "bye")
			{
				Application.Exit();
			}
			else
			{
				Debug.Print("{0} {1}", cmd, arg);
				var psi = new ProcessStartInfo();
				psi.FileName = cmd;
				psi.Arguments = arg;
				psi.CreateNoWindow = false;
				var p = Process.Start(psi);
			}
		}

		private void TheForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			exit_thread = true;
		}
	}
}
