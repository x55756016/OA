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

namespace SMT.SAAS.Platform.ViewModel.MainPage
{
    public class ShortCutViewModel : Foundation.BasicViewModel
    {
        #region Model
        private string _shortcutid;
        private string _moduleid;
        private string _modulename;
        private string _titel;
        private string _assemplyname;
        private string _iconpath;
        private string _fullname;
        private string _issysneed = "0";
        private string _params;
        private string _userstate = "0";

        /// <summary>
        /// 快捷键ID
        /// </summary>
        public string ShortCutID
        {
            set { SetValue(ref _shortcutid, value, "ShortCutID"); }
            get { return _shortcutid; }
        }
        /// <summary>
        /// 子项目ID
        /// </summary>
        public string ModuleID
        {
            set { SetValue(ref _moduleid, value, "ModuleID"); }
            get { return _moduleid; }
        }

        /// <summary>
        /// 模块名称
        /// </summary>
        public string ModuleName
        {
            set { SetValue(ref _modulename, value, "ModuleName"); }
            get { return _modulename; }
        }
       

        /// <summary>
        /// 标题
        /// </summary>
        public string Titel
        {
            set { SetValue(ref _titel, value, "Titel"); }
            get { return _titel; }
        }
        /// <summary>
        /// 所属程序集名
        /// </summary>
        public string AssemplyName
        {
            set { SetValue(ref _assemplyname, value, "AssemplyName"); }
            get { return _assemplyname; }
        }
        /// <summary>
        /// 图标路径
        /// </summary>
        public string IconPath
        {
            set { SetValue(ref _iconpath, value, "IconPath"); }
            get { return _iconpath; }
        }
        /// <summary>
        /// 完整路径
        /// </summary>
        public string FullName
        {
            set { SetValue(ref _fullname, value, "FullName"); }
            get { return _fullname; }
        }
        /// <summary>
        /// 是否可删除
        /// 0:不可以，1：可以
        /// </summary>
        public string IsSysNeed
        {
            set { SetValue(ref _issysneed, value, "IsSysNeed"); }
            get { return _issysneed; }
        }
        /// <summary>
        /// 初始参数
        /// </summary>
        public string Params
        {
            set { SetValue(ref _params, value, "Params"); }
            get { return _params; }
        }
        /// <summary>
        /// 使用状态
        /// 0:未启用  1:启用
        /// </summary>
        public string UserState
        {
            set { SetValue(ref _userstate, value, "UserState"); }
            get { return _userstate; }


        }
        #endregion Model
    }
}
