using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Management;
using System.Windows.Forms.VisualStyles;
using Microsoft.Win32;
using RegistryUtils;

namespace StartupManager
{
	static class Program
	{
		static readonly NotifyIcon _icon = new NotifyIcon();
		static RegistryMonitor _rm;


		[STAThread]
		static void Main()
		{
			StartWatcher();
			_icon.Text = "test";
			_icon.Icon = new Icon(SystemIcons.Shield, 40, 40);
			_icon.Visible = true;
			_icon.DoubleClick += _icon_DoubleClick;

			Application.Run();
		}

		static void StartWatcher()
		{

			_rm = new RegistryMonitor(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Run");
			_rm.RegChanged += ChangeDetected;
			_rm.Start();
		}

		private static void ChangeDetected(object sender, EventArgs e)
		{
			var vns = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default)
				.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run").GetValueNames();

			var sl = 
				from vn
				in vns
				let v = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run", vn, "") as string
				orderby vn
				select $"{vn} was added or changed to {v}";

			var tip = string.Join(Environment.NewLine, sl);
			Debugger.Log(0, "", tip);
			_icon.ShowBalloonTip(0, "Run Changed", tip, ToolTipIcon.Info);

		}

		private static void _icon_DoubleClick(object sender, EventArgs e)
		{
			_icon.Visible = false;
			_rm.Stop();
			_rm.Dispose();
			Application.Exit();
		}
	}
}
