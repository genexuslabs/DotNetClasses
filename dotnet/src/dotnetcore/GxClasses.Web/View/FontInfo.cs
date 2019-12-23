namespace GeneXus.WebControls
{
	public class FontInfo
	{
		public string Name { get; set; }
		public FontUnit Size { get; set; }
		public bool Bold { get; internal set; }
		public bool Italic { get; internal set; }
		public bool Strikeout { get; internal set; }
		public bool Underline { get; internal set; }

		public void CopyFrom(FontInfo f)
		{
			if (f == null)
				return;
			this.Name = f.Name;
			this.Size = f.Size;
			this.Bold = f.Bold;
			this.Italic = f.Italic;
			this.Strikeout = f.Strikeout;
			this.Underline = f.Underline;
		}
	}
	public struct FontUnit
	{
		private readonly Unit value;
		public FontUnit(int value)
		{
			this.value = Unit.Point(value);
		}
		public static FontUnit Point(int n)
		{
			return new FontUnit(n);
		}
		public Unit Unit
		{
			get
			{
				return this.value;
			}
		}
	}
	public enum FontSize
	{
		NotSet,
		AsUnit,
		Smaller,
		Larger,
		XXSmall,
		XSmall,
		Small,
		Medium,
		Large,
		XLarge,
		XXLarge,
	}
}