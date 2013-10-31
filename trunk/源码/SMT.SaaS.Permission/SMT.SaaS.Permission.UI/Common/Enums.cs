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

namespace SMT.SaaS.Permission.UI
{
    //public enum FormTypes
    //{         
    //    New,
    //    Edit
    //}
    public enum OrgTreeItemTypes
    {
        Company,
        Department,
        Post,
        Personnel,
        All
    }
    public enum Action
    {
        /// <summary>
        /// 新增
        /// </summary>
        Add,
        /// <summary>
        /// 修改
        /// </summary>
        Edit,
        /// <summary>
        /// 删除
        /// </summary>
        Delete,
        /// <summary>
        /// 查看
        /// </summary>
        Read,
        /// <summary>
        /// 借出
        /// </summary>
        Lend,
        /// <summary>
        /// 归还
        /// </summary>
        Return,
        /// <summary>
        /// 审批
        /// </summary>
        AUDIT
    }

    public enum MessageTypes
    {
        Message,
        Caution,
        Error
    }

    public enum PortalType
    {
        Silverlight,
        AspMVC
    }
}
