using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GeneXus.Utils;

namespace GeneXus.Notifications.ProgressIndicator
{
    public class GXWebProgressIndicatorInfo : GxUserType
    {
        private string _action; /*  0: Show, 1:ShowWithTitle, 2: ShowWithTitleAndDesc, 3:Hide */
        private string _class;
        private string _title;
        private string _description;
        private int _maxValue;
        private int _value;
        private short _type;

        public override void ToJSON()
        {
            AddObjectProperty("Action", _action);
            AddObjectProperty("Class", _class);
            AddObjectProperty("Title", _title);
            AddObjectProperty("Description", _description);
            AddObjectProperty("MaxValue", _maxValue);
            AddObjectProperty("Value", _value);
            AddObjectProperty("Type", _type);
        }

        public string Class
        {
            get { return _class; }
            set { this._class = value; }
        }
        public string Title
        {
            get { return _title; }
            set { this._title = value; }
        }
        public string Description
        {
            get { return _description; }
            set { this._description = value; }
        }

        public string Action
        {
            get { return _action; }
            set { this._action = value; }
        }

        public short Type
        {
            get { return _type; }
            set { this._type = value; }
        }
        public int Value
        {
            get { return _value; }
            set { this._value = value; }
        }

        public int MaxValue
        {
            get { return _maxValue; }
            set { this._maxValue = value; }
        }
    }
}
