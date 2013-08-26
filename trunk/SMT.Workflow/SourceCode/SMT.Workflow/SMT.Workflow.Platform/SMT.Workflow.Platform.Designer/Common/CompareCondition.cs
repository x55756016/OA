using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace SMT.Workflow.Platform.Designer.Common
{
    /// <summary>
    /// 连线的条件
    /// </summary>
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
        /// <summary>
        /// 条件名称
        /// </summary>
        public string Description
        {
            get
            { return _Description; }
            set
            { _Description = value; }
        }
        /// <summary>
        /// 名称(guid)
        /// </summary>
        public string Name
        {
            get
            { return _Name; }
            set
            { _Name = value; }
        }
        /// <summary>
        ///  条件字段ID
        /// </summary>
        public string CompAttr
        {
            get
            { return _CompAttr; }
            set
            { _CompAttr = value; }
        }
        /// <summary>
        /// 数据类型
        /// </summary>
        public string DataType
        {
            get
            { return _DataType; }
            set
            { _DataType = value; }
        }
        /// <summary>
        /// 比较操作
        /// </summary>
        public string Operate
        {
            get
            { return _Operate; }
            set
            { _Operate = value; }
        }
        /// <summary>
        /// 比较值
        /// </summary>
        public string CompareValue
        {
            get
            { return _CompareValue; }
            set
            { _CompareValue = value; }
        }
        /// <summary>
        /// 比较值备注
        /// </summary>
        public string CompareValueMark
        {
            get;
            set;
        }
    }
}
