using System;
using System.Diagnostics;
using System.Text;

namespace GeneXus.Application
{

	public static class GxRunner
	{
		public static void RunAsync(
			string commandLine,
			string workingDir,
			string virtualPath,
			string schema,
			Action<int> onExit = null)
		{
			var stdout = new StringBuilder();
			var stderr = new StringBuilder();

			using var proc = new Process
			{
				StartInfo = new ProcessStartInfo
				{
					FileName = commandLine,
					WorkingDirectory = workingDir,
					UseShellExecute = false,          // required for redirection
					CreateNoWindow = true,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					RedirectStandardInput = false,    // flip to true only if you need to write to stdin
					StandardOutputEncoding = Encoding.UTF8,
					StandardErrorEncoding = Encoding.UTF8
				},
				EnableRaisingEvents = true
			};

			proc.StartInfo.ArgumentList.Add(virtualPath);
			proc.StartInfo.ArgumentList.Add(schema);

			proc.OutputDataReceived += (_, e) =>
			{
				if (e.Data is null) return;
				stdout.AppendLine(e.Data);
				Console.WriteLine(e.Data);          // forward to parent console (stdout)
			};

			proc.ErrorDataReceived += (_, e) =>
			{
				if (e.Data is null) return;
				stderr.AppendLine(e.Data);
				Console.Error.WriteLine(e.Data);        // forward to parent console (stderr)
			};

			proc.Exited += (sender, e) =>
			{
				var p = (Process)sender!;
				int exitCode = p.ExitCode;
				p.Dispose();

				Console.WriteLine($"[{DateTime.Now:T}] Process exited with code {exitCode}");

				// Optional: call user-provided callback
				onExit?.Invoke(exitCode);
			};

			if (!proc.Start())
				throw new InvalidOperationException("Failed to start process");
			Console.WriteLine($"[{DateTime.Now:T}] MCP Server Started.");
		}
	}

}
