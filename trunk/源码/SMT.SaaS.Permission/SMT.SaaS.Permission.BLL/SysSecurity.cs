using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMT.SaaS.Permission.BLL
{
    public class SysSecurity
    {
        ///// <summary>
        ///// 根据用户ID获得用户
        ///// </summary>
        ///// <param name="userId"></param>
        ///// <returns></returns>
        //public static User GetUserByUserID(int userId)
        //{
        //    LinqDbContext lbc = LinqDbContext.Instance;
        //    var sus = (from us in lbc.User where us.UserId == userId select us).Single();
        //    return sus;

        //}
        ///// <summary>
        ///// 根据用户登录名获得用户
        ///// </summary>
        ///// <param name="userLogName"></param>
        ///// <returns></returns>
        //public static User GetUserByUserName(string userName)
        //{
        //    LinqDbContext lbc = LinqDbContext.Instance;
        //    var sus = (from us in lbc.User where us.UserName == userName select us).Single();
        //    return sus;
        //}
        ///// <summary>
        ///// 返回某分支机构下的人员。
        ///// </summary>
        ///// <param name="orgCode"></param>
        ///// <returns></returns>
        //public static IQueryable<User> GetUsersByOrgCode(int orgId)
        //{
        //    LinqDbContext lbc = LinqDbContext.Instance;
        //    var query = from us in lbc.User where us.OrgId == orgId select us;
        //    return query;
        //}
        ///// <summary>
        ///// 根据用户名获得某人的所有的角色
        ///// </summary>
        ///// <param name="userName"></param>
        ///// <returns></returns>
        //public IQueryable<SysRole> GetRolesByUserName(string userName)
        //{
        //    LinqDbContext lbc = LinqDbContext.Instance;
        //    var query = from role in lbc.SysRole
        //                join ur in lbc.SysUserRole
        //                on role.Id equals ur.RoleId
        //                where ur.User.UserName == userName
        //                select role;
        //    return query;
        //}
        ///// <summary>
        /////  根据用户ID获得某人的所有的角色
        ///// </summary>
        ///// <param name="userId"></param>
        ///// <returns></returns>
        //public static IQueryable<SysRole> GetRolesByUserID(int userId)
        //{
        //    LinqDbContext lbc = LinqDbContext.Instance;
        //    var query = from role in lbc.SysRole
        //                join ur in lbc.SysUserRole
        //                on role.Id equals ur.RoleId
        //                where ur.User.UserId == userId
        //                select role;
        //    return query;
        //}

        ///// <summary>
        ///// 获得某一应用/某人的所有角色
        ///// </summary>
        ///// <param name="userName"></param>
        ///// <param name="appCode"></param>
        ///// <returns></returns>
        //public static IQueryable<SysRole> GetAppRolesByUserName(string userName, string appCode)
        //{
        //    LinqDbContext lbc = LinqDbContext.Instance;
        //    var query = from role in lbc.SysRole
        //                join ur in lbc.SysUserRole
        //                on role.Id equals ur.RoleId
        //                where role.Application.AppCode == appCode && ur.User.UserName == userName
        //                select role;
        //    return query;
        //}

        ///// <summary>
        ///// 获得某一应用下/某人的所有角色
        ///// </summary>
        ///// <param name="userId"></param>
        ///// <param name="appCode"></param>
        ///// <returns></returns>
        //public static IQueryable<SysRole> GetAppRolesByUserName(int userId, string appCode)
        //{
        //    LinqDbContext lbc = LinqDbContext.Instance;
        //    var query = from role in lbc.SysRole
        //                join ur in lbc.SysUserRole on role.Id equals ur.RoleId
        //                where role.Application.AppCode == appCode && ur.UserId == userId
        //                select role;
        //    return query;
        //}

        ///// <summary>
        ///// 判断某人是否是某角色
        ///// </summary>
        ///// <param name="userName"></param>
        ///// <param name="roleCode"></param>
        ///// <returns></returns>
        //public static bool CheckUserInRole(string userName, string roleCode)
        //{
        //    LinqDbContext lbc = LinqDbContext.Instance;
        //    int count = (from ur in lbc.SysUserRole where ur.User.UserName == userName && ur.SysRole.RoleCode == roleCode select ur.RoleId).Count();
        //    return count > 0;
        //}

        ///// <summary>
        ///// 获得某人所有的功能。
        ///// </summary>
        ///// <param name="userName"></param>
        ///// <returns></returns>
        //public static IQueryable<SysFunction> GetFunctionByUserName(string userName)
        //{
        //    LinqDbContext lbc = LinqDbContext.Instance;
        //    var query = from fu in lbc.SysFunction
        //                join rf in lbc.SysRoleFunction on fu.Id equals rf.FunId
        //                join ur in lbc.SysUserRole on rf.RoleId equals ur.RoleId
        //                where ur.User.UserName == userName
        //                select fu;
        //    return query;
        //}

        ///// <summary>
        ///// 获得某一应用下/某人的所有功能
        ///// </summary>
        ///// <param name="userName"></param>
        ///// <param name="appCode"></param>
        ///// <returns></returns>
        //public static IQueryable<SysFunction> GetAppFunctinsByUserName(string userName, string appCode)
        //{
        //    LinqDbContext lbc = LinqDbContext.Instance;
        //    var query = from fu in lbc.SysFunction
        //                join rf in lbc.SysRoleFunction on fu.Id equals rf.FunId
        //                join ur in lbc.SysUserRole on rf.RoleId equals ur.RoleId
        //                where fu.Application.AppCode == appCode && ur.User.UserName == userName
        //                select fu;
        //    return query;
        //}

        ///// <summary>
        ///// 判断某人是否有某功能
        ///// </summary>
        ///// <param name="userName"></param>
        ///// <param name="roleCode"></param>
        ///// <returns></returns>
        //public static bool CheckUserHasFunction(string userName, string funCode)
        //{
        //    LinqDbContext lbc = LinqDbContext.Instance;
        //    int count = (from fu in lbc.SysFunction
        //                 join rf in lbc.SysRoleFunction on fu.Id equals rf.FunId
        //                 join ur in lbc.SysUserRole on rf.RoleId equals ur.RoleId
        //                 where fu.FunCode == funCode && ur.User.UserName == userName
        //                 select fu.FunCode).Count();
        //    return count > 0;
        //}

        ///// <summary>
        /////用户认证
        ///// </summary>
        ///// <param name="strUserName"></param>
        ///// <param name="strPassword"></param>
        ///// <returns></returns>
        //public static bool Authenticate(string strUserName, string strPassword)
        //{
        //    LinqDbContext lbc = LinqDbContext.Instance;
        //    int count = (from us in lbc.User where us.UserName == strUserName && us.UserPwd == strPassword select us).Count();
        //    return count == 1;
        //}

    }
}
