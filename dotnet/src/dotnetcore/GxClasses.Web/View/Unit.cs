using System;

namespace GeneXus.WebControls
{
	public struct Unit
	{
		private readonly UnitType type;
		private readonly double value;
		public static readonly Unit Empty;
		public bool IsEmpty
		{
			get
			{
				return this.type == (UnitType)0;
			}
		}

		public Unit(double value, UnitType type)
		{
			if (value < (double)short.MinValue || value > (double)short.MaxValue)
				throw new ArgumentOutOfRangeException(nameof(value));
			this.value = type != UnitType.Pixel ? value : (double)(int)value;
			this.type = type;
		}
		public static Unit Point(int n)
		{
			return new Unit((double)n, UnitType.Point);
		}
		public double Value
		{
			get
			{
				return this.value;
			}
		}
	}
	public enum UnitType
	{
		Pixel = 1,
		Point = 2,
		Pica = 3,
		Inch = 4,
		Mm = 5,
		Cm = 6,
		Percentage = 7,
		Em = 8,
		Ex = 9,
	}
}