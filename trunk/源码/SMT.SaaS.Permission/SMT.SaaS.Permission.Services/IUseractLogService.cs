using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using SMT_System_EFModel;

namespace SMT.SaaS.Permission.Services
{
    // 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码和配置文件中的接口名“IUseractLogService”。
    [ServiceContract]
    public interface IUseractLogService
    {
        [OperationContract]
        void DoWork();
        [OperationContract]
        bool AddUseractLog();
        [OperationContract]
        bool UpdateUseractLog();
        [OperationContract]
        bool DeleteUseractLog();
        [OperationContract]
        IQueryable<T_SYS_USERACTLOG> GetSysUseractLogWithPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID);
        [OperationContract]
        IQueryable<T_SYS_USERACTLOG> GetSysUseractLogList();

    }
}
