
using System;
using System.Threading.Tasks;
using GeneXus.Application;
using GeneXus.Core.genexus.common;

namespace GeneXus.WebControls
{
	public class GXGridStateHandlerAsync: GXGridStateHandler
	{
		readonly Func<Task> varsFromState;
		readonly Func<Task> varsToState;
		public GXGridStateHandlerAsync(IGxContext context, string gridName, string programName, Func<Task> varsFromState, Func<Task> varsToState)
		{
			this.gridName = $"{programName}_{gridName}_{GRID_STATE}";
			this.varsFromState = varsFromState;
			this.varsToState = varsToState;
			this.context = context;
			state = new SdtGridState(context);
			dirty = true;
		}
		protected override void ToState()
		{
			varsToState().GetAwaiter().GetResult();
		}
		protected override void FromState()
		{
			varsFromState().GetAwaiter().GetResult();
		}
	}

	public class GXGridStateHandler
	{
		protected string gridName;
		readonly Action varsFromState;
		readonly Action varsToState;
		protected IGxContext context;
		protected SdtGridState state;
		protected bool dirty;
		protected const string GRID_STATE = "GridState";
		public GXGridStateHandler(IGxContext context, string gridName, string programName, Action varsFromState, Action varsToState)
		{
			this.gridName = $"{programName}_{gridName}_{GRID_STATE}";
			this.varsFromState = varsFromState;
			this.varsToState = varsToState;
			this.context = context;
			state = new SdtGridState(context);
			dirty = true;
		}
		internal GXGridStateHandler()
		{
		}
		public void SaveGridState()
		{
			state.FromJSonString(context.GetSession().Get(gridName));
			ToState();
			context.GetSession().Set(gridName, state.ToJSonString());
			dirty = true;
		}
		public void LoadGridState()
		{
			if (context.GetRequestMethod() == "GET")
			{
				state = new SdtGridState(context);
				state.FromJSonString(context.GetSession().Get(gridName));
				FromState();
				dirty = true;
			}
		}

		protected virtual void FromState()
		{
			varsFromState();
		}
		protected virtual void ToState()
		{
			varsToState();
		}
		public string FilterValues(int idx)
		{
			return state.gxTpr_Inputvalues[idx-1].gxTpr_Value;
		}
		public string FilterValues(string filterName)
		{
			int idx = ContainsName(filterName);
			if (idx > 0)
				return FilterValues(idx);
			else
				return string.Empty;
		}
		private int ContainsName(string filterName)
		{
			int idx = 1;
			foreach (var inputvalue in state.gxTpr_Inputvalues)
			{
				if (inputvalue.gxTpr_Name.Equals(filterName, StringComparison.OrdinalIgnoreCase))
					return idx;
				idx++;
			}
			return -1;
		}
		public int CurrentPage
		{
			get
			{
				return state.gxTpr_Currentpage;
			}
			set
			{
				state.gxTpr_Currentpage = value;
			}
		}
		public short OrderedBy
		{
			get
			{
				return state.gxTpr_Orderedby;
			}
			set
			{
				state.gxTpr_Orderedby = value;
			}
		}
		public SdtGridState State {
			get {
				if (dirty || state==null)
				{
					state = new SdtGridState(context);
					state.FromJSonString(context.GetSession().Get(gridName));
					dirty = false;
				}
				return state;
			}
		}

		public void SetState(SdtGridState value)
		{
			state = value;
			context.GetSession().Set(gridName, state.ToJSonString());
		}

		public void ClearFilterValues()
		{
			state.gxTpr_Inputvalues.Clear();
		}
		public void AddFilterValue(string name, string value)
		{
			int idx = ContainsName(name);
			if (idx > 0)
				state.gxTpr_Inputvalues[idx - 1].gxTpr_Value = value;
			else
				state.gxTpr_Inputvalues.Add(new SdtGridState_InputValuesItem() { gxTpr_Name = name, gxTpr_Value = value });
		}
		public int FilterCount
		{
			get { return state.gxTpr_Inputvalues.Count; }
		}

	}

}