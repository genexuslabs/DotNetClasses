using GeneXus.Application;
using log4net;
using System;
using System.Diagnostics;
using System.IO;

namespace GeneXus.Utils
{
	public class GxProcessFactory : IProcessFactory
	{
		static GxProcessFactory()
		{
			GXProcessHelper.ProcessFactory = new GxProcessFactory();
		}
		public IProcessHelper GetProcessHelper()
		{
			return new GxProcess();
		}
	}
	public class GxProcess : IProcessHelper
	{
		static readonly ILog log = log4net.LogManager.GetLogger(typeof(GeneXus.Utils.GxProcess));

		public short OpenPrintDocument(string commandString)
		{
			Process p = new Process();
			p.StartInfo.FileName = "print";
			p.StartInfo.Arguments = commandString;
			p.StartInfo.CreateNoWindow = true;
			p.StartInfo.UseShellExecute = true;
			p.Start();
			return 0;
		}
		public int ExecProcess(string filename, string[] args, string basePath, string executable, DataReceivedEventHandler dataReceived)
		{
			Process p = new Process();
			p.StartInfo.FileName = string.Format("\"{0}\"", filename);

			for (int i = 0; i < args.Length; i++)
			{
				if (args[i].ToLower().StartsWith("\\config:") || args[i].ToLower().StartsWith("-config:"))
					args[i] = string.Format("\"{0}\"", args[i]);
			}
			p.StartInfo.Arguments = string.Format("\"{0}\" {1}", Path.Combine(basePath, executable), string.Join(" ", args));
			p.StartInfo.CreateNoWindow = true;
			p.StartInfo.UseShellExecute = false;
			p.StartInfo.RedirectStandardOutput = true;
			p.StartInfo.RedirectStandardError = true;
			p.StartInfo.RedirectStandardInput = true;
			if (dataReceived != null)
			{
				p.OutputDataReceived += dataReceived;
				p.ErrorDataReceived += dataReceived;
			}
			p.StartInfo.WorkingDirectory = Directory.GetCurrentDirectory(); //It must run in the same directory as the current process
			GXLogging.Debug(log, filename, p.StartInfo.WorkingDirectory, p.StartInfo.Arguments);
			p.Start();
			p.BeginOutputReadLine();
			p.BeginErrorReadLine();
			p.WaitForExit();
			return p.ExitCode;
		}

		public int Shell(string commandString, int modal)
		{
			return Shell(commandString, modal, 0);
		}
		public int Shell(string commandString, int modal, int redirectOutput)
		{
			try
			{
				GXLogging.Debug(log, "Shell commandString:'", commandString, "',modal:'", modal.ToString(), "', redirectOutput:" + redirectOutput);
				int startArgs;
				string file, args;
				commandString = commandString.TrimStart();

				file = "";
				args = "";
				bool in_string = false;
				startArgs = -1;
				for (int i = 0; i < commandString.Length; i++)
				{
					if (commandString[i] == '"' || commandString[i] == '\'')
					{
						if (in_string)
							in_string = false;
						else
							in_string = true;
					}
					if (in_string)
						continue;
					if (commandString[i] == ' ')
					{
						startArgs = i;
						break;
					}
				}
				if (startArgs == -1)
				{
					file = commandString;
					args = "";
				}
				else
				{
					file = commandString.Substring(0, startArgs);
					args = commandString.Substring(startArgs + 1);
				}
				file = file.Replace("\'", String.Empty).Replace("\"", String.Empty);//If the file name was delimited with 'it is changed to ".
				Process p = new Process();
				p.StartInfo.Arguments = args;
				p.StartInfo.CreateNoWindow = true;

				if (redirectOutput==1)
				{
					p.StartInfo.UseShellExecute = false;
					//UseShellExecute must be false if RedirectStandardOutput is true
					//When UseShellExecute is false, the FileName property can be either a fully qualified path to the executable,
					//or a simple executable name that the system will attempt to find within folders specified by the PATH environment variable.
					p.StartInfo.RedirectStandardOutput = true;
					p.StartInfo.RedirectStandardError = true;
					p.OutputDataReceived += Shell_DataReceived;
					p.ErrorDataReceived += Shell_DataReceived;
				}
				else
				{
					p.StartInfo.UseShellExecute = true;
					//When UseShellExecute is true, the WorkingDirectory property specifies the location of the executable. 
					//If WorkingDirectory is an empty string, the current directory is understood to contain the executable.
					try
					{
						if (Path.IsPathRooted(file))
						{
							p.StartInfo.WorkingDirectory = "\"" + Path.GetDirectoryName(file) + "\"";
						}
						else
						{
							p.StartInfo.WorkingDirectory = GxContext.StaticPhysicalPath();
						}
					}
					catch (Exception e)
					{
						GXLogging.Warn(log, "Setting Working Directory", e);
					}
				}
				try
				{
					p.StartInfo.FileName = "\"" + file + "\"";
				}
				catch (Exception e)
				{
					GXLogging.Warn(log, "Setting Path Rooted", e);
				}

				GXLogging.Debug(log, "Shell FileName:'" + p.StartInfo.FileName + "',Arguments:'" + p.StartInfo.Arguments + "'");
				GXLogging.Debug(log, "Shell Working directory:'" + p.StartInfo.WorkingDirectory + "'");
				bool res = p.Start();
				GXLogging.Debug(log, "Shell new process resource is started:" + res);
				if (redirectOutput==1)
				{
					p.BeginOutputReadLine();
					p.BeginErrorReadLine();
				}

				if (modal > 0)
				{
					p.WaitForExit();
					GXLogging.Debug(log, "Shell ExitCode:" + p.ExitCode);
					return p.ExitCode;
				}
				return 0;
			}
			catch (Exception e)
			{
				GXLogging.Error(log, "Shell Error", e);
				throw e;
			}

		}

		private void Shell_DataReceived(object sender, DataReceivedEventArgs e)
		{
			if (e.Data != null)
				Console.WriteLine(e.Data);
		}
	}
}
