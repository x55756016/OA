using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using SMT_System_EFModel;
using System.ServiceModel.Activation;
using SMT.SaaS.Permission.BLL;

namespace SMT.SaaS.Permission.Services
{
    // 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码、svc 和配置文件中的类名“UseractLogService”。

     [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceContract(SessionMode = SessionMode.Allowed)]//杨祥红新增

    public class UseractLogService 
    {
         private SysUseractLogBLL Useract = new SysUseractLogBLL();
         #region 操作日志 增 删 改 查 操作
         /// <summary>
         /// 添加操作日志记录
         /// </summary>
         /// <param name="entity">日志对象</param>
         /// <returns></returns>
         [OperationContract]
         public bool AddUseractLog(T_SYS_USERACTLOG entity)
         {
             return Useract.InsertUseractLog(entity);
         }

         /// <summary>
         /// 修改操作日志
         /// </summary>
         /// <param name="entity">日志对象</param>
         /// <returns></returns>
         [OperationContract]
         public bool UpdateUseractLog(T_SYS_USERACTLOG entity)
         {
             return Useract.UpdateUseractLog(entity);
         }

         /// <summary>
         /// 删除操作日志
         /// </summary>
         /// <param name="entity">日志对象</param>
         /// <returns></returns>
         [OperationContract]
         public bool DeleteUseractLog(T_SYS_USERACTLOG entity)
         {
             return Useract.DeleteUseractLog(entity);
         }
         /// <summary>
         /// 操作日志 列表分页查询管理
         /// </summary>
         /// <param name="pageIndex"></param>
         /// <param name="pageSize"></param>
         /// <param name="sort"></param>
         /// <param name="filterString"></param>
         /// <param name="paras"></param>
         /// <param name="pageCount"></param>
         /// <param name="loginUserInfo"></param>
         /// <returns></returns>
         [OperationContract]
         public List<T_SYS_USERACTLOG> GetSysUseractLogWithPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, LoginUserInfo loginUserInfo)
         {
             IQueryable<T_SYS_USERACTLOG> usList;
             //string keyString = "GetSysUseractLogWithPaging" + loginUserInfo.userID + loginUserInfo.companyID; //缓存KEY
             //if (WCFCache.Current[keyString] == null)
             //{
             
                usList =  Useract.GetSysUseractLogWithPaging(pageIndex,pageSize,sort,filterString,paras,ref pageCount,loginUserInfo.userID);
            //    WCFCache.Current.Insert(keyString, usList);
            //}
            //else
            //{
            //    usList = (IQueryable<T_SYS_USERACTLOG>)WCFCache.Current[keyString];
            //}
          
            return usList.Count() > 0 ? usList.ToList() : null;


         }
         /// <summary>
         /// 查询所有操作日志记录集合
         /// </summary>
         /// <returns></returns>
         [OperationContract]
         public IQueryable<T_SYS_USERACTLOG> GetSysUseractLogList()
         {
             return Useract.GetSysUseractLogList();
         }

         /// <summary>
         /// 根据操作日志ID 查询 日志对象
         /// </summary>
         /// <param name="id"></param>
         /// <returns></returns>
         [OperationContract]
         public T_SYS_USERACTLOG GetSysUseractLogByID(string id)
         {
             return Useract.GetSysUseractLogByID(id);
         }
         #endregion
    }
}
