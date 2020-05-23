using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using GeneXus.Application;
using GeneXus.Utils;
#if NETCORE
using Stubble.Core;
using Stubble.Core.Builders;
#else
using Nustache.Core;
#endif

namespace GeneXus.UserControls
{
	public class UserControlGenerator
	{
		private string m_Type;
#if NETCORE
		private string m_Template;
#else
		private Template m_Template;
#endif
		public UserControlGenerator(string ucType)
		{
			m_Type = ucType;
		}
		private DateTime LastRenderTime { get; set; } = DateTime.MinValue;

		public static Func<string, string> GetTemplateAction = GetTemplateFile;

		private static string GetTemplateFile(string type)
		{
			return Path.Combine(GxContext.StaticPhysicalPath(), $"gxusercontrols\\{type}.view");
		}

		internal string Render(string internalName, GxDictionary propbag)
		{
			if (!File.Exists(GetTemplateAction(m_Type)))
				return String.Empty;
#if NETCORE
			var stubble = new StubbleBuilder().Build();
			string output = string.Empty;
			if (GetTemplateDateTime() > LastRenderTime || m_Template == null)
			{
				m_Template = File.ReadAllText(GetTemplateFile(m_Type));
				LastRenderTime = DateTime.UtcNow;
			}
			return stubble.Render(m_Template, propbag);
#else
			Encoders.HtmlEncode = (input) => input;  

			if (GetTemplateDateTime() > LastRenderTime || m_Template == null)
			{
				m_Template = new Template();
#pragma warning disable SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
				m_Template.Load(new StringReader(File.ReadAllText(GetTemplateAction(m_Type))));
#pragma warning restore SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
				LastRenderTime = DateTime.UtcNow;
				// Do not use template.Compile, it doesnt work with nested SDTs
			}
			StringBuilder sb = new StringBuilder();
			m_Template.Render(propbag, new StringWriter(sb), null);
			return sb.ToString();

#endif
		}

	private DateTime GetTemplateDateTime()
		{
			return File.GetLastWriteTimeUtc(GetTemplateAction(m_Type));
		}
	}
}