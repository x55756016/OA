using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT_HRM_EFModel;

namespace SMT.HRM.BLL.Report
{
    public class AddEmployeechangehistoryMethod : BaseBll<T_HR_EMPLOYEEENTRY>
    {
        /// <summary>
        /// 插入员工异动历史信息
        /// </summary>
        /// <param name="POSTID">岗位ID</param>
        /// <param name="EMPLOYEEID">员工ID</param>
        /// <param name="EMPLOYEECNAME">员工姓名</param>
        /// <param name="FINGERPRINTID">指纹ID</param>
        /// <param name="EMPLOYEEENTRYID">员工入职表ID</param>
        /// <param name="ISAGENCY">是否代理</param>
        /// <param name="ENTRYDATE">异动时间</param>
        /// <param name="EMPLOYEEPOSTID">异动前岗位id</param>
        /// <param name="REMARK">备注</param>
        /// <param name="OWNERID">所属员工ID</param>
        /// <param name="OWNERPOSTID">所属岗位ID</param>
        /// <param name="OWNERDEPARTMENTID">所属部门ID</param>
        /// <param name="OWNERCOMPANYID">所属公司ID</param>
        /// <returns></returns>
        public T_HR_EMPLOYEECHANGEHISTORY AddEmployeeMethod(T_HR_EMPLOYEECHANGEHISTORY employeeEntity)
        {
            T_HR_EMPLOYEECHANGEHISTORY employeechangehistory = new T_HR_EMPLOYEECHANGEHISTORY();
            //记录ID NEW
            employeechangehistory.RECORDID = Guid.NewGuid().ToString();
            //员工ID
            employeechangehistory.T_HR_EMPLOYEE.EMPLOYEEID = employeeEntity.T_HR_EMPLOYEE.EMPLOYEEID;
            //员工姓名
            employeechangehistory.EMPOLYEENAME = employeeEntity.EMPOLYEENAME;
            //指纹编号
            employeechangehistory.FINGERPRINTID = employeeEntity.FINGERPRINTID;
            //0.入职1.异动2.离职3.薪资级别变更4.签订合同
            employeechangehistory.FORMTYPE = employeeEntity.FORMTYPE;

            //记录原始单据id（员工入职表ID）
            employeechangehistory.FORMID = employeeEntity.FORMID;
            //0 主岗位非主岗位？（是否代理）
            employeechangehistory.ISMASTERPOSTCHANGE = employeeEntity.ISMASTERPOSTCHANGE;

            //包括 异动类型及离职类型 0:1=异动类型：离职类型
            employeechangehistory.CHANGETYPE = employeeEntity.CHANGETYPE;

            //异动时间
            employeechangehistory.CHANGETIME = employeeEntity.CHANGETIME;
            //异动原因
            employeechangehistory.CHANGEREASON = employeeEntity.CHANGEREASON;
            //异动前岗位id
            employeechangehistory.OLDPOSTID = employeeEntity.OLDPOSTID;
            //更具岗位ID得到岗位名称
            //var postDictionary = dal.GetObjects<T_HR_POSTDICTIONARY>().FirstOrDefault(s => s.POSTDICTIONARYID == postInfo.T_HR_POSTDICTIONARY.POSTDICTIONARYID);
            //if (postDictionary != null)
            //{
                    
               
            //}
            //异动前岗位名称
            employeechangehistory.OLDPOSTNAME = employeeEntity.OLDPOSTNAME;
            //异动前岗位级别
            employeechangehistory.OLDPOSTLEVEL = employeeEntity.OLDPOSTLEVEL;
            //异动前薪资级别
            employeechangehistory.OLDSALARYLEVEL = employeeEntity.OLDSALARYLEVEL;
            //异动前部门id
            employeechangehistory.OLDDEPARTMENTID = employeeEntity.OLDDEPARTMENTID;
            //异动前部门名称
            employeechangehistory.OLDDEPARTMENTNAME = employeeEntity.OLDDEPARTMENTNAME;
            //异动前公司id
            employeechangehistory.OLDCOMPANYID = employeeEntity.OLDCOMPANYID;

            //更具公司ID得到公司信息
            //var companyInfo = dal.GetObjects<T_HR_COMPANY>().FirstOrDefault(s => s.COMPANYID == postInfo.COMPANYID);
            //if (companyInfo != null)
            //{
                    
            //}

            //异动前公司名称
            employeechangehistory.OLDCOMPANYNAME = employeeEntity.OLDCOMPANYNAME;
            //异动前薪资额度
            employeechangehistory.OLDSALARYSUM = employeeEntity.OLDSALARYSUM;


            //异动后岗位id
            employeechangehistory.NEXTPOSTID = employeeEntity.NEXTPOSTID;
            //异动后岗位名称
            employeechangehistory.NEXTPOSTNAME = employeeEntity.NEXTPOSTNAME;
            //异动后岗位级别
            employeechangehistory.NEXTPOSTLEVEL = employeeEntity.NEXTPOSTLEVEL;
            //异动后薪资级别
            employeechangehistory.NEXTSALARYLEVEL = employeeEntity.NEXTSALARYLEVEL;
            //异动后部门id
            employeechangehistory.NEXTDEPARTMENTID = employeeEntity.NEXTDEPARTMENTID;
            //异动后部门名称
            employeechangehistory.NEXTDEPARTMENTNAME = employeeEntity.NEXTDEPARTMENTNAME;
            //异动后公司id
            employeechangehistory.NEXTCOMPANYID = employeeEntity.NEXTCOMPANYID;
            //异动后公司名称
            employeechangehistory.NEXTCOMPANYNAME = employeeEntity.NEXTCOMPANYNAME;

            //备注
            employeechangehistory.REMART = employeeEntity.REMART;
            //创建时间
            employeechangehistory.CREATEDATE = employeeEntity.CREATEDATE;
            //所属员工ID
            employeechangehistory.OWNERID = employeeEntity.OWNERID;
            //所属岗位ID
            employeechangehistory.OWNERPOSTID = employeeEntity.OWNERPOSTID;
            //所属部门ID
            employeechangehistory.OWNERDEPARTMENTID = employeeEntity.OWNERDEPARTMENTID;
            //所属公司ID
            employeechangehistory.OWNERCOMPANYID = employeeEntity.OWNERCOMPANYID;
            return employeechangehistory;
        }
    }
}
