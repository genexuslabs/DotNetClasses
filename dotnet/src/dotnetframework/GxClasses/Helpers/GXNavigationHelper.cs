namespace GeneXus.Application
{
	using System;
	using Jayrock.Json;
	using System.Collections.Generic;
	[Serializable]
	public class GXNavigationHelper
	{
		public static string POPUP_LEVEL = "gxPopupLevel";
		public static string CALLED_AS_POPUP = "gxCalledAsPopup";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("GxFxCopRules", "CR1000:EnforceThreadSafeType")]
		private Dictionary<int, Stack<UrlItem>> referers;

		public GXNavigationHelper()
		{
			referers = new Dictionary<int, Stack<UrlItem>>();
		}
		public string ToJSonString(int lvl)
		{
			JArray array = new JArray();
			if (referers.ContainsKey(lvl))
			{
				var levelStack = referers[lvl];
				foreach (var item in levelStack)
				{
					array.Add(item.url);
				}
			}
			return array.ToString();
		}

		public void PushUrl(string url, bool fromRedirect)
		{
			if (url.IndexOf(CALLED_AS_POPUP) != -1)
				return;
			int popupLevel = GetUrlPopupLevel(url);
			UrlItem urlItem = new UrlItem();
			urlItem.redirect = fromRedirect;
			urlItem.url = url;
			if (!referers.ContainsKey(popupLevel))
			{
				Stack<UrlItem> stack = new Stack<UrlItem>();
				referers.Add(popupLevel, stack);
				stack.Push(urlItem);
			}
			else
				referers[popupLevel].Push(urlItem);
		}

		public void PopUrl(string url)
		{
			if (url.IndexOf(CALLED_AS_POPUP) != -1)
				return;
			int popupLevel = GetUrlPopupLevel(url);
			if (referers.ContainsKey(popupLevel))
			{
				if (referers[popupLevel].Count > 0)
				{
					referers[popupLevel].Pop();
				}
			}
		}

		public UrlItem PopUrlItem(string url)
		{
			if (url.IndexOf(CALLED_AS_POPUP) != -1)
				return null;
			int popupLevel = GetUrlPopupLevel(url);
			if (referers.ContainsKey(popupLevel))
			{
				if (referers[popupLevel].Count > 0)
				{
					UrlItem i = referers[popupLevel].Pop();
					return i;
				}
			}
			return null;
		}

		public string PeekUrl(string url)
		{
			if (url.IndexOf(CALLED_AS_POPUP) != -1)
				return "";
			int popupLevel = GetUrlPopupLevel(url);
			if (referers.ContainsKey(popupLevel))
			{
				if (referers[popupLevel].Count > 0)
				{
					return ((UrlItem)referers[popupLevel].Peek()).url;
				}
			}
			return "";
		}
		public string GetRefererUrl(string url)
		{
			if (url.IndexOf(CALLED_AS_POPUP) != -1)
				return "";
			int popupLevel = GetUrlPopupLevel(url);
			if (referers.ContainsKey(popupLevel))
			{
				if (referers[popupLevel].Count > 1)
				{
					UrlItem i = referers[popupLevel].Pop();
					string referer = ((UrlItem)referers[popupLevel].Peek()).url;
					referers[popupLevel].Push(i);
					return referer;
				}
			}
			return "";
		}
		public void DeleteStack(int popupLevel)
		{
			if (referers.ContainsKey(popupLevel))
				referers.Remove(popupLevel);
		}

		public int Count()
		{
			return referers.Count;
		}

		public int GetUrlPopupLevel(string url)
		{
			Uri uri = null;
			try
			{
				uri = new Uri(url);
			}
			catch { }
			if (uri != null)
			{
				url = uri.GetComponents(UriComponents.Query, UriFormat.Unescaped);
			}
			int popupLevel = -1;
			if (url != null)
			{
				int pIdx = url.IndexOf(POPUP_LEVEL);
				if (pIdx != -1)
				{
					int eqIdx = url.IndexOf("=", pIdx);
					if (eqIdx != -1)
					{
						int cIdx = url.IndexOf(";", eqIdx);
						if (cIdx > eqIdx)
						{
							try
							{
								string strLvl = url.Substring(eqIdx + 1, cIdx - eqIdx - 1);
								popupLevel = int.Parse(strLvl);
							}
							catch
							{
								popupLevel = -1;
							}
						}
					}
				}
			}
			return popupLevel;
		}
	}
}
