using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;

namespace SMT.HRM.UI.Active
{
    public partial class CompareCondition : Object
    {
        public CompareCondition()
        {
            
        }

        private string _Description;
        private string _Name;
        private string _CompAttr;
        private string _DataType;
        private string _Operate;
        private string _CompareValue;

        public string Description
        {
            get
            { return _Description; }
            set
            { _Description = value; }
        }

        public string Name
        {
            get
            {return _Name;}
            set
            {_Name = value;}
        }

        public string CompAttr
        {
            get
            { return _CompAttr; }
            set
            { _CompAttr = value; }
        }

        public string DataType
        {
            get
            { return _DataType; }
            set
            { _DataType = value; }
        }

        public string Operate
        {
            get
            { return _Operate; }
            set
            { _Operate = value; }
        }

        public string CompareValue
        {
            get
            { return _CompareValue; }
            set
            { _CompareValue = value; }
        }


       
    }
}