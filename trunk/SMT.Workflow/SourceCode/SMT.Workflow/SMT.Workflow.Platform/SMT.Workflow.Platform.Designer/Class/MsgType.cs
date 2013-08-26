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
using System.Collections.Generic;

namespace SMT.Workflow.Platform.Designer.Class
{

    /// <summary>
    /// 默认消息类型
    /// </summary>
    public class MsgType
    {
        public string MsgTypeName
        {
            get;
            set;

        }
        public decimal MsgTypeCode
        {
            get;
            set;
        }
        public List<MsgType> Init()
        {
            return new List<MsgType> { 
                new MsgType{ MsgTypeName="默认消息",MsgTypeCode= 0},
                new MsgType{MsgTypeName="撤消待办",MsgTypeCode= 1}
            };
        }
    }
}
