
namespace SterlingDemoProject.Tables
{
    /// <summary>
    /// 系统模块依赖项实体
    /// </summary>
    public class V_ModuleInfo_DependsOn
    {
        #region V_ModuleInfo_DependsOn成员
        private string _userModuleID;
        private string _userID;
        private string _moduleid;
        private string _modulename;
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
        /// 子项目ID(V_ModuleInfo的ModuleID)
        /// </summary>
        public string ModuleID
        {
            set { _moduleid = value; }
            get { return _moduleid; }
        }

        /// <summary>
        /// 子项目依赖项的系统名称
        /// </summary>
        public string ModuleName
        {
            set { _modulename = value; }
            get { return _modulename; }
        }
    }
}
