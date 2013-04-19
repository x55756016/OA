using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;

namespace SMT.HRM.UI.Active
{
    public partial class RuleConditions : Object
    {
        public RuleConditions()
        {
            Name = System.Guid.NewGuid().ToString();
        }

        private string _Name;
        private string _ConditionObject;
        private string _CodiCombMode;

        public string Name
        {
            set
            { _Name = value; }
            get
            { return _Name; }
        }

        public string ConditionObject
        {
            set
            { _ConditionObject = value; }
            get
            { return _ConditionObject; }
        }

        public string CodiCombMode
       {
            set
           { _CodiCombMode = value; }
            get
           { return _CodiCombMode; }
        }

        List<CompareCondition> _subCondition = new List<CompareCondition>();

        public List<CompareCondition> subCondition
        {
            get { return _subCondition; }
            set { _subCondition = value; }
        }



    }
}