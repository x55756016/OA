
namespace SMT.SaaS.LocalData.Tables
{
    /// <summary>
    /// 系统模块参数字典项实体
    /// </summary>
    public class V_ModuleInfo_Params
    {
        #region V_ModuleInfo_Params成员
        private string _userModuleID;
        private string _userID;
        private string _moduleid;
        private string _paramkey;
        private string _paramvalue;
        #endregion

        /// <summary>
        /// 主键
        /// </summary>
        public string UserModuleID
        {
            set { _userModuleID = value; }
            get { return _userModuleID; }
        }
        /// <summary>
        /// 员工ID
        /// </summary>
        public string UserID
        {
            set { _userID = value; }
            get { return _userID; }
        }
        /// <summary>
        /// 子项目ID
        /// </summary>
        public string ModuleID
        {
            set { _moduleid = value; }
            get { return _moduleid; }
        }

        /// <summary>
        /// 字典项Key
        /// </summary>
        public string ParamKey
        {
            set { _paramkey = value; }
            get { return _paramkey; }
        }

        /// <summary>
        /// 字典项值
        /// </summary>
        public string ParamValue
        {
            set { _paramvalue = value; }
            get { return _paramvalue; }
        }
    }
}
