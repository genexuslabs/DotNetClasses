
using System;
using GeneXus.Utils;
using GeneXus.Application;
using System.Runtime.Serialization;
using System.Collections.Generic;
using GeneXus.Configuration;
using GeneXus.Metadata;

namespace GeneXus.WebControls
{
	public class GXGridStateHandler
	{
		String gridName;
		readonly Action varsFromState;
		readonly Action varsToState;
		IGxContext context;
		GridState state;
		GxUserType exposedSdtGridState;
		bool dirty;
		const string GRID_STATE = "GridState";
		const string SDTGridStateNamespace = "GeneXus.Core.genexus.common";
		const string SDTGridState = "SdtGridState";

		public GXGridStateHandler(IGxContext context, string gridName, string programName, Action varsFromState, Action varsToState)
		{
			this.gridName = $"{programName}_{gridName}_{GRID_STATE}";
			this.varsFromState = varsFromState;
			this.varsToState = varsToState;
			this.context = context;
			state = new GridState();
			dirty = true;
		}
		public void SaveGridState()
		{
			state = JSONHelper.Deserialize<GridState>(context.GetSession().Get(gridName));
			varsToState();
			context.GetSession().Set(gridName, JSONHelper.Serialize<GridState>(state));
			dirty = true;
		}
		public void LoadGridState()
		{
			if (context.GetRequestMethod() == "GET")
			{
				state = JSONHelper.Deserialize<GridState>(context.GetSession().Get(gridName));
				varsFromState();
				dirty = true;
			}
		}
		public string FilterValues(int idx)
		{
			return state.InputValues[idx - 1].Value;
		}
		public int CurrentPage
		{
			get
			{
				return state.CurrentPage;
			}
			set
			{
				state.CurrentPage = value;
			}
		}
		public short OrderedBy
		{
			get
			{
				return state.OrderedBy;
			}
			set
			{
				state.OrderedBy = value;
			}
		}
		public GxUserType State {
			get {
				if (dirty || exposedSdtGridState==null)
				{
					exposedSdtGridState = (GxUserType)ClassLoader.FindInstance(Config.CoreAssemblyName, SDTGridStateNamespace, SDTGridState, new object[] {context }, null);
					exposedSdtGridState.FromJSonString(context.GetSession().Get(gridName));
					dirty = false;
				}
				return exposedSdtGridState;
			}
		}

		public void SetState(GxUserType value)
		{
			exposedSdtGridState = value;
			String jsonState = exposedSdtGridState.ToJSonString();
			state = JSONHelper.Deserialize<GridState>(jsonState);
			context.GetSession().Set(gridName, jsonState);
		}

		public void ClearFilterValues()
		{
			state.InputValues.Clear();
		}
		public void AddFilterValue(string name, string value)
		{
			state.InputValues.Add(new GridState_InputValuesItem() { Name = name, Value = value });
		}
		public int FilterCount
		{
			get { return state.InputValues.Count; }
		}

	}
	[DataContract]
	internal class GridState
	{
		List<GridState_InputValuesItem> _inputValues;
		public GridState()
		{
		}
		[DataMember]
		internal int CurrentPage { get; set; }
		[DataMember]
		internal short OrderedBy { get; set; }
		[DataMember]
		internal List<GridState_InputValuesItem> InputValues {
			get {
				if (_inputValues == null)
					_inputValues = new List<GridState_InputValuesItem>();
				return _inputValues;
			}
			set {
				_inputValues = value;
			}
		}
	}
	[DataContract]
	internal class GridState_InputValuesItem
	{
		[DataMember]
		internal String Name { get; set; }
		[DataMember]
		internal String Value { get; set; }
	}
}