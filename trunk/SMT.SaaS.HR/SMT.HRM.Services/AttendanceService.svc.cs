using System;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Collections.Generic;
using System.Text;

using SMT_HRM_EFModel;
using SMT.HRM.CustomModel;
using SMT.HRM.BLL;
using System.Data.Objects.DataClasses;
using System.Collections;
using System.IO;
using System.Xml;

using System.Configuration;
using System.Web;
using SMT.HRM.CustomModel.Reports;
using SMT.Foundation.Log;

namespace SMT.HRM.Services
{
    [ServiceContract(Namespace = "")]
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class AttendanceService
    {
        [OperationContract]
        public void DoWork()
        {
            // 在此处添加操作实现
            return;
        }

        #region 文件上传服务
        /// <summary>
        /// 
        /// </summary>
        /// <param name="UploadFile">上传载体</param>
        /// <param name="strFilePath">上传文件存储的相对路径</param>
        [OperationContract]
        public void SaveFile(UploadFileModel UploadFile, out string strFilePath)
        {
            // Store File to File System
            string strNewFileName = string.Empty;
            string strVirtualPath = ConfigurationManager.AppSettings["FileUploadLocation"].ToString();
            if (!string.IsNullOrWhiteSpace(UploadFile.FileName))
            {
                strNewFileName = DateTime.Now.ToString("yyMMddhhmmss") + DateTime.Now.Millisecond.ToString() + UploadFile.FileName.Substring(UploadFile.FileName.LastIndexOf("."));
            }

            string strPath = HttpContext.Current.Server.MapPath(strVirtualPath) + strNewFileName;
            if (Directory.Exists(HttpContext.Current.Server.MapPath(strVirtualPath)) == false)
            {
                Directory.CreateDirectory(HttpContext.Current.Server.MapPath(strVirtualPath));
            }
            FileStream FileStream = new FileStream(strPath, FileMode.Create);
            FileStream.Write(UploadFile.File, 0, UploadFile.File.Length);

            FileStream.Close();
            FileStream.Dispose();

            strFilePath = strVirtualPath + strNewFileName;
        }
        #endregion

        #region V_LandStatistic 员工登录记录服务

        /// <summary>
        /// 登录记录统计(只针对公司)
        /// </summary>
        /// <param name="strOwnerID"></param>
        /// <param name="strOrderBy"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageCount"></param>
        /// <param name="filterString"></param>
        /// <param name="objArgs"></param>
        /// <returns></returns>
        [OperationContract]
        public List<V_LandStatistic> GetPersonalLandStatisticListByMultSearch(string strOwnerID, string strOrderBy,
            string filterString, List<object> objArgs)
        {
            using (PersonalLandStatisticBLL bllLand = new PersonalLandStatisticBLL())
            {
                var ents = bllLand.GetPersonalLandStatisticListByMultSearch(strOwnerID, strOrderBy, filterString, objArgs);

                if (ents == null)
                {
                    return null;
                }

                return ents;
            }
        }

        /// <summary>
        /// 登录记录明细
        /// </summary>
        /// <param name="strOwnerID"></param>
        /// <param name="strOrderBy"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageCount"></param>
        /// <param name="iLoginTimes"></param>
        /// <param name="iLoginPersonCount"></param>
        /// <param name="filterString"></param>
        /// <param name="objArgs"></param>
        /// <returns></returns>
        [OperationContract]
        public List<V_LandDetail> GetPersonalLandDetailListByMultSearch(string strOwnerID, string strOrderBy,
            int pageIndex, int pageSize, ref int pageCount, ref int iLoginTimes, ref int iLoginPersonCount, string filterString, List<object> objArgs)
        {
            using (PersonalLandDetailBLL bllDetail = new PersonalLandDetailBLL())
            {
                var ents = bllDetail.GetPersonalLandDetailListByMultSearch(strOwnerID, strOrderBy, pageIndex, pageSize, ref pageCount, ref iLoginTimes, ref iLoginPersonCount, filterString, objArgs);

                if (ents == null)
                {
                    return null;
                }

                return ents;
            }
        }

        /// <summary>
        /// 获取登录记录统计(只针对公司)的导出数据
        /// </summary>
        /// <param name="strOrgType"></param>
        /// <param name="strOrgID"></param>
        /// <param name="strOwnerId"></param>
        /// <param name="iYear"></param>
        /// <param name="iMonthStart"></param>
        /// <param name="iMonthEnd"></param>
        /// <param name="strFilter"></param>
        /// <param name="objArgs"></param>
        /// <param name="strSortKey"></param>
        /// <returns></returns>
        [OperationContract]
        public byte[] OutFileLandStatisticList(string strOwnerID, string strOrderBy, string filterString, List<object> objArgs)
        {
            using (PersonalLandStatisticBLL bllLand = new PersonalLandStatisticBLL())
            {
                var ents = bllLand.OutFileLandStatisticList(strOwnerID, strOrderBy, filterString, objArgs);

                if (ents == null)
                {
                    return null;
                }

                return ents;
            }
        }

        /// <summary>
        /// 获取登录记录明细的导出数据
        /// </summary>
        /// <param name="strOwnerID"></param>
        /// <param name="strOrderBy"></param>
        /// <param name="filterString"></param>
        /// <param name="objArgs"></param>
        /// <returns></returns>
        [OperationContract]
        public byte[] OutFilePersonalLandDetailList(string strOwnerID, string strOrderBy, string filterString, List<object> objArgs)
        {
            using (PersonalLandDetailBLL bllDetail = new PersonalLandDetailBLL())
            {
                var ents = bllDetail.OutFilePersonalLandDetailList(strOwnerID, strOrderBy, filterString, objArgs);

                if (ents == null)
                {
                    return null;
                }

                return ents;
            }
        }

        #endregion T_HR_EMPLOYEECLOCKINRECORD 员工原始打卡记录服务

        #region T_HR_EMPLOYEECLOCKINRECORD 员工原始打卡记录服务
        

        /// <summary>
        /// 根据公司的ID，取得当前录入的员工打卡记录的最新打卡日期
        /// </summary>
        /// <param name="strCompanyId"></param>
        /// <returns></returns>
        [OperationContract]
        public DateTime GetLatestPunchDateByCompanyId(string strCompanyId)
        {
            using (ClockInRecordBLL bllClockInRecord = new ClockInRecordBLL())
            {
                return bllClockInRecord.GetLatestPunchDateByCompanyId(strCompanyId);
            }
        }

        /// <summary>
        /// 获取员工日常打卡信息
        /// </summary>
        /// <param name="sType">查看的员工范围对象类型</param>
        /// <param name="sValue">查看的员工范围对象的ID</param>
        /// <param name="strOwnerID">查看人的员工ID(权限控制)</param>
        /// <param name="strEmployeeID">员工序号(唯一，GUID)</param>
        /// <param name="strPunchDateFrom">打卡搜寻起始日期</param>
        /// <param name="strPunchDateTo">打卡搜寻截止日期</param>
        /// <param name="strSortKey">排序字段</param>
        /// <param name="pageIndex">当前页码</param>
        /// <param name="pageSize">每页显示记录数</param>
        /// <param name="pageCount">总页码</param>
        /// <returns>返回员工日常打卡列表</returns>
        [OperationContract]
        public List<T_HR_EMPLOYEECLOCKINRECORD> GetClockInRdListByMultSearch(string sType, string sValue, string strOwnerID, string strEmployeeID, string strPunchDateFrom,
            string strPunchDateTo, string strTimeFrom, string strTimeTo, string strSortKey, int pageIndex, int pageSize, ref int pageCount)
        {
            using (ClockInRecordBLL bllClockInRecord = new ClockInRecordBLL())
            {
                var ents = bllClockInRecord.GetClockInRdListByMultSearch(sType, sValue, strOwnerID, strEmployeeID, strPunchDateFrom, strPunchDateTo, strTimeFrom, strTimeTo, strSortKey, pageIndex, pageSize, ref pageCount);

                if (ents == null)
                {
                    return null;
                }

                return ents;
            }
        }

        /// <summary>
        /// 通过客户端注册的Windows服务，自动取得打卡机数据，转换后导入到数据库中
        /// </summary>
        /// <param name="strCompanyId">所属公司Id</param>
        /// <param name="entTempList">打卡机临时数据</param>
        /// <param name="dtStart">导入起始日期</param>
        /// <param name="dtEnd">导入截止日期</param>
        /// <param name="strClientIP">客户端IP(即打卡机IP，仅做记录用)</param>
        /// <param name="strMsg">处理消息</param>
        [OperationContract]
        public void ImportClockInRdListByWSRealTime(string strCompanyId, List<T_HR_EMPLOYEECLOCKINRECORD> entTempList, DateTime dtStart, DateTime dtEnd, string strClientIP, ref string strMsg)
        {
            using (ClockInRecordBLL bllClockInRecord = new ClockInRecordBLL())
            {
                bllClockInRecord.ImportClockInRdListByWindowsService(strCompanyId, entTempList, dtStart, dtEnd, strClientIP, ref strMsg);
            }
        }

        /// <summary>
        /// 导入上传文件的员工日常打卡信息
        /// </summary>
        /// <param name="UploadFile">上传载体</param>
        /// <param name="dtStart">导入有效数据起始日期</param>
        /// <param name="dtEnd">导入有效数据截止日期</param>
        /// <param name="strMsg">处理消息</param>
        [OperationContract]
        public void ImportClockInRdListFromFile(UploadFileModel UploadFile, string strFileType, string strUnitType, string strUnitObjectId, DateTime dtStart, DateTime dtEnd, ref string strMsg)
        {
            string strPath = string.Empty;
            SaveFile(UploadFile, out strPath);
            string strPhysicalPath = HttpContext.Current.Server.MapPath(strPath);

            using (ClockInRecordBLL bllClockInRecord = new ClockInRecordBLL())
            {
                if (strFileType.ToLower() == "csv")
                {
                    bllClockInRecord.ImportClockInRdListByImportCSV(strPhysicalPath, strUnitType, strUnitObjectId, dtStart, dtEnd, ref strMsg);
                }
                else if (strFileType.ToLower() == "xls")
                {
                    bllClockInRecord.ImportClockInRdListByImportExcel(strPhysicalPath, strUnitType, strUnitObjectId, dtStart, dtEnd, ref strMsg);
                }
            }
        }

        /// <summary>
        /// 导入系统登入记录的员工日常打卡信息
        /// </summary>
        /// <param name="UploadFile">上传载体</param>
        /// <param name="dtStart">导入有效数据起始日期</param>
        /// <param name="dtEnd">导入有效数据截止日期</param>
        /// <param name="strMsg">处理消息</param>
        [OperationContract]
        public void ImportClockInRdListFromLoginData(string strUnitType, string strUnitObjectId, DateTime dtStart, DateTime dtEnd, ref string strMsg)
        {
            using (ClockInRecordBLL bllClockInRecord = new ClockInRecordBLL())
            {
                bllClockInRecord.ImportClockInRdListByLoginData(strUnitType, strUnitObjectId, dtStart, dtEnd, ref strMsg);
            }
        }

        /// <summary>
        /// 导入上传文件的员工日常打卡信息
        /// </summary>
        /// <param name="UploadFile">上传载体</param>
        /// <param name="dtStart">导入有效数据起始日期</param>
        /// <param name="dtEnd">导入有效数据截止日期</param>
        /// <param name="strMsg">处理消息</param>
        [OperationContract]
        public void ImportClockInRdListFromFileAndLoginData(UploadFileModel UploadFile, string strFileType, string strUnitType, string strUnitObjectId, DateTime dtStart, DateTime dtEnd, ref string strMsg)
        {
            string strPath = string.Empty;
            SaveFile(UploadFile, out strPath);
            string strPhysicalPath = HttpContext.Current.Server.MapPath(strPath);

            using (ClockInRecordBLL bllClockInRecord = new ClockInRecordBLL())
            {
                bllClockInRecord.ImportClockInRdListByImportFileAndLoginData(strPhysicalPath, strFileType, strUnitType, strUnitObjectId, dtStart, dtEnd, ref strMsg);
            }
        }

        /// <summary>
        /// 添加员工日常打卡记录，并返回处理结果
        /// </summary>
        /// <param name="entClockInRd">待添加的员工日常打卡记录</param>
        /// <returns>返回处理结果</returns>
        [OperationContract]
        public string AddNewClockInRd(T_HR_EMPLOYEECLOCKINRECORD entClockInRd)
        {
            using (ClockInRecordBLL bllClockInRecord = new ClockInRecordBLL())
            {
                return bllClockInRecord.AddClockInRecord(entClockInRd);
            }
        }

        /// <summary>
        /// 根据查询条件，获取原始打卡记录信息，并转为html格式的数据流返回以便导出成指定格式文件
        /// </summary>
        /// <param name="dtPunchDateFrom">打卡日期起始时间</param>
        /// <param name="dtPunchDateTo">打卡日期截止时间</param>
        /// <param name="strEmployeeID">员工ID</param>
        /// <param name="strSortKey">排序</param>
        /// <param name="strMsg">返回的消息</param>
        /// <returns></returns>
        [OperationContract]
        public byte[] OutClockInRdListByMultSearch(string sType, string sValue, string strOwnerID, string strEmployeeID, string strPunchDateFrom, string strPunchDateTo, string strSortKey, out string strMsg)
        {
            try
            {
                using (ClockInRecordBLL bllClockInRecord = new ClockInRecordBLL())
                {
                    byte[] byVac = bllClockInRecord.OutClockInRdListByMultSearch(sType, sValue, strOwnerID, strEmployeeID, strPunchDateFrom, strPunchDateTo, strSortKey, out strMsg);
                    return byVac;
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug(ex.ToString());
                strMsg = ex.ToString();
                return null;
            }
        }
        /// <summary>
        /// 强制删除考勤异常及签卡，重新初始化考勤
        /// </summary>
        /// <param name="objType">类型：0公司，1员工</param>
        /// <param name="objId">类型id</param>
        /// <param name="dtStar">开始时间</param>
        /// <param name="dtEnd">结束时间</param>
        /// <returns></returns>
        [OperationContract]        
        public string CompulsoryInitialization(string objType, string objId, DateTime dtStar, DateTime dtEnd)
        {
            string smtmsg = string.Empty;
            using (AttendanceRecordBLL bll = new AttendanceRecordBLL())
            {
                smtmsg=bll.CompulsoryInitialization(objType, objId, dtStar, dtEnd,"2");

                return smtmsg;
            }
        }
        #endregion

        #region T_HR_ATTENDANCESOLUTION 考勤方案服务

        /// <summary>
        /// 获取考勤方案信息
        /// </summary>
        /// <param name="strAttendanceSolutionId">主键索引</param>
        /// <returns></returns>
        [OperationContract]
        public T_HR_ATTENDANCESOLUTION GetAttendanceSolutionByID(string strAttendanceSolutionId)
        {
            using (AttendanceSolutionBLL bllAttendanceSolution = new AttendanceSolutionBLL())
            {
                return bllAttendanceSolution.GetAttendanceSolutionByID(strAttendanceSolutionId);
            }
        }

        /// <summary>
        /// 根据员工ID获取其应用的考勤方案
        /// </summary>
        /// <param name="strEmployeeID"></param>
        /// <returns></returns>
        [OperationContract]
        public T_HR_ATTENDANCESOLUTION GetAttendanceSolutionByEmployeeID(string strEmployeeID)
        {
            using (AttendanceSolutionBLL bllAttendanceSolution = new AttendanceSolutionBLL())
            {
                return bllAttendanceSolution.GetAttendanceSolutionByEmployeeID(strEmployeeID);
            }
        }

        /// <summary>
        /// 根据考勤初始化记录获取考勤方案，获取不到则获取员工最新分配的考勤方案
        /// </summary>
        /// <param name="strEmployeeID">员工ID</param>
        /// <param name="dtStart">考勤记录参考起始日期</param>
        /// <param name="dtEnd">考勤记录参考截止日期</param>
        /// <returns></returns>
        [OperationContract]
        public T_HR_ATTENDANCESOLUTION GetAttendanceSolutionByEmployeeIDAndDate(string strEmployeeID, DateTime dtStart, DateTime dtEnd)
        {
            using (AttendanceSolutionBLL bllAttendanceSolution = new AttendanceSolutionBLL())
            {
                return bllAttendanceSolution.GetAttendanceSolutionByEmployeeIDAndDate(strEmployeeID, dtStart, dtEnd);
            }
        }

        /// <summary>
        /// 获取考勤方案信息
        /// </summary>
        /// <param name="strOwnerID">权限控制所有人的员工序号</param>
        /// <param name="strCheckState">审核状态</param>
        /// <param name="strAttendanceSolutionName">考勤方案名称</param>
        /// <param name="strAttendanceType">考勤方式</param>        
        /// <param name="strSortKey">排序字段</param>
        /// <param name="pageIndex">当前页码</param>
        /// <param name="pageSize">每页显示记录数</param>
        /// <param name="pageCount">总页码</param>
        /// <returns>返回考勤方案信息</returns>
        [OperationContract]
        public List<T_HR_ATTENDANCESOLUTION> GetAttendanceSolutionListByMultSearch(string strOwnerID, string strCheckState, string strAttendanceSolutionName, string strAttendanceType,
            string strSortKey, int pageIndex, int pageSize, ref int pageCount)
        {
            using (AttendanceSolutionBLL bllAttendanceSolution = new AttendanceSolutionBLL())
            {
                var ents = bllAttendanceSolution.GetAttendanceSolutionRdListByMultSearch(strOwnerID, strCheckState, strAttendanceSolutionName, strAttendanceType, strSortKey, pageIndex, pageSize, ref pageCount).ToList();

                if (ents == null)
                {
                    return null;
                }

                return ents.ToList();
            }
        }

        /// <summary>
        /// 新增考勤方案信息
        /// </summary>
        /// <param name="entTemp"></param>
        /// <returns></returns>
        [OperationContract]
        public string AddAttendanceSolution(T_HR_ATTENDANCESOLUTION entTemp)
        {
            using (AttendanceSolutionBLL bllAttendanceSolution = new AttendanceSolutionBLL())
            {
                return bllAttendanceSolution.AddAttSol(entTemp);
            }
        }

        /// <summary>
        /// 修改考勤方案信息
        /// </summary>
        /// <param name="entTemp"></param>
        /// <returns></returns>
        [OperationContract]
        public string ModifyAttendanceSolution(T_HR_ATTENDANCESOLUTION entTemp)
        {
            using (AttendanceSolutionBLL bllAttendanceSolution = new AttendanceSolutionBLL())
            {
                return bllAttendanceSolution.ModifyAttSol(entTemp);
            }
        }

        /// <summary>
        /// 修改考勤方案信息
        /// </summary>
        /// <param name="entTemp"></param>
        /// <returns></returns>
        [OperationContract]
        public string AuditAttSol(string strAttendanceSolutionId, string strCheckState)
        {
            using (AttendanceSolutionBLL bllAttendanceSolution = new AttendanceSolutionBLL())
            {
                return bllAttendanceSolution.AuditAttSol(strAttendanceSolutionId, strCheckState);
            }
        }

        /// <summary>
        /// 根据主键索引，删除考勤方案信息(注：暂定为物理删除)
        /// </summary>
        /// <param name="strAttendanceSolutionId">主键索引</param>
        /// <returns></returns>
        [OperationContract]
        public string RemoveAttendanceSolution(string strAttendanceSolutionId)
        {
            using (AttendanceSolutionBLL bllAttendanceSolution = new AttendanceSolutionBLL())
            {
                return bllAttendanceSolution.DeleteAttSol(strAttendanceSolutionId);
            }
        }

        /// <summary>
        /// 新增考勤方案信息,并建立其关联设置的关系
        /// </summary>
        /// <param name="entTemp"></param>
        /// <returns></returns>
        [OperationContract]
        public string AddAttendanceSolutionAndCreateRelation(T_HR_ATTENDANCESOLUTION entTemp, List<T_HR_ATTENDANCESOLUTIONDEDUCT> entAttendanceSolutionDeducts, List<T_HR_ATTENDFREELEAVE> entAttendFreeLeaves)
        {
            using (AttendanceSolutionBLL bllAttendanceSolution = new AttendanceSolutionBLL())
            {
                return bllAttendanceSolution.AddAndCreateRelation(entTemp, entAttendanceSolutionDeducts, entAttendFreeLeaves);
            }
        }

        /// <summary>
        /// 修改考勤方案信息,并更改其关联设置的关系
        /// </summary>
        /// <param name="entTemp"></param>
        /// <returns></returns>
        [OperationContract]
        public string ModifyAttendanceSolutionAndChangeRelation(T_HR_ATTENDANCESOLUTION entTemp, List<T_HR_ATTENDANCESOLUTIONDEDUCT> entAttendanceSolutionDeducts, List<T_HR_ATTENDFREELEAVE> entAttendFreeLeaves)
        {
            using (AttendanceSolutionBLL bllAttendanceSolution = new AttendanceSolutionBLL())
            {
                return bllAttendanceSolution.ModifyAndChangeRelation(entTemp, entAttendanceSolutionDeducts, entAttendFreeLeaves);
            }
        }

        #endregion

        #region T_HR_ATTENDFREELEAVE 考勤方案带薪假服务

        /// <summary>
        /// 获取信息
        /// </summary>
        /// <param name="strAttendFreeLeaveId">主键索引</param>
        /// <returns></returns>
        [OperationContract]
        public T_HR_ATTENDFREELEAVE GetAttendFreeLeaveByID(string strAttendFreeLeaveId)
        {
            using (AttendFreeLeaveBLL bllAttendFreeLeave = new AttendFreeLeaveBLL())
            {
                return bllAttendFreeLeave.GetAttendFreeLeaveByID(strAttendFreeLeaveId);
            }
        }

        /// <summary>
        /// 获取考勤方案带薪假信息
        /// </summary>
        /// <param name="strAttendFreeLeaveName">考勤方案编号</param>
        /// <param name="strFreeLeaveDaySetID">带薪假设置编号</param>        
        /// <param name="strSortKey">排序字段</param>        
        /// <returns>返回考勤方案带薪假信息</returns>
        [OperationContract]
        public List<T_HR_ATTENDFREELEAVE> GetAllAttendFreeLeaveListByMultSearch(string strAttendanceSolutionID, string strFreeLeaveDaySetID, string strSortKey)
        {
            using (AttendFreeLeaveBLL bllAttendFreeLeave = new AttendFreeLeaveBLL())
            {
                var ents = bllAttendFreeLeave.GetAllAttendFreeLeaveRdListByMultSearch(strAttendanceSolutionID, strFreeLeaveDaySetID, strSortKey).ToList();
                if (ents == null)
                {
                    return null;
                }

                return ents.ToList();
            }
        }

        /// <summary>
        /// 获取考勤方案带薪假信息
        /// </summary>
        /// <param name="strAttendFreeLeaveName">考勤方案编号</param>
        /// <param name="strFreeLeaveDaySetID">带薪假设置编号</param>        
        /// <param name="strSortKey">排序字段</param>
        /// <param name="pageIndex">当前页码</param>
        /// <param name="pageSize">每页显示记录数</param>
        /// <param name="pageCount">总页码</param>
        /// <returns>返回考勤方案带薪假信息</returns>
        [OperationContract]
        public List<T_HR_ATTENDFREELEAVE> GetAttendFreeLeaveListByMultSearch(string strAttendanceSolutionID, string strFreeLeaveDaySetID,
            string strSortKey, int pageIndex, int pageSize, ref int pageCount)
        {
            using (AttendFreeLeaveBLL bllAttendFreeLeave = new AttendFreeLeaveBLL())
            {
                var ents = bllAttendFreeLeave.GetAttendFreeLeaveRdListByMultSearch(strAttendanceSolutionID, strFreeLeaveDaySetID, strSortKey, pageIndex, pageSize, ref pageCount).ToList();

                if (ents == null)
                {
                    return null;
                }

                return ents.ToList();
            }
        }

        /// <summary>
        /// 新增考勤方案带薪假信息
        /// </summary>
        /// <param name="entTemp"></param>
        /// <returns></returns>
        [OperationContract]
        public string AddAttendFreeLeave(T_HR_ATTENDFREELEAVE entTemp)
        {
            using (AttendFreeLeaveBLL bllAttendFreeLeave = new AttendFreeLeaveBLL())
            {
                return bllAttendFreeLeave.AddAttendFreeLeave(entTemp);
            }
        }

        /// <summary>
        /// 修改考勤方案带薪假信息
        /// </summary>
        /// <param name="entTemp"></param>
        /// <returns></returns>
        [OperationContract]
        public string ModifyAttendFreeLeave(T_HR_ATTENDFREELEAVE entTemp)
        {
            using (AttendFreeLeaveBLL bllAttendFreeLeave = new AttendFreeLeaveBLL())
            {
                return bllAttendFreeLeave.ModifyAttendFreeLeave(entTemp);
            }
        }

        /// <summary>
        /// 根据主键索引，删除考勤方案带薪假信息(注：暂定为物理删除)
        /// </summary>
        /// <param name="strAttendFreeLeaveId">主键索引</param>
        /// <returns></returns>
        [OperationContract]
        public string RemoveAttendFreeLeave(string strAttendFreeLeaveId)
        {
            using (AttendFreeLeaveBLL bllAttendFreeLeave = new AttendFreeLeaveBLL())
            {
                return bllAttendFreeLeave.DeleteAttendFreeLeave(strAttendFreeLeaveId);
            }
        }

        #endregion

        #region T_HR_SCHEDULINGTEMPLATEMASTER 排班模板服务

        /// <summary>
        /// 获取排班模板信息
        /// </summary>
        /// <param name="strSchedulingTemplateMasterId">主键索引</param>
        /// <returns></returns>
        [OperationContract]
        public T_HR_SCHEDULINGTEMPLATEMASTER GetSchedulingTemplateMasterByID(string strSchedulingTemplateMasterId)
        {
            using (SchedulingTemplateMasterBLL bllSchedulingTemplateMaster = new SchedulingTemplateMasterBLL())
            {
                return bllSchedulingTemplateMaster.GetSchedulingTemplateMasterByID(strSchedulingTemplateMasterId);
            }
        }

        /// <summary>
        /// 根据考勤方案主键索引，获取排班模板信息
        /// </summary>
        /// <param name="strAttendanceSolutionId">考勤方案主键索引</param>
        /// <returns></returns>
        [OperationContract]
        public T_HR_SCHEDULINGTEMPLATEMASTER GetSchedulingTemplateMasterByAttSolID(string strAttendanceSolutionId)
        {
            using (SchedulingTemplateMasterBLL bllSchedulingTemplateMaster = new SchedulingTemplateMasterBLL())
            {
                return bllSchedulingTemplateMaster.GetSchedulingTemplateMasterByAttSolID(strAttendanceSolutionId);
            }
        }

        /// <summary>
        /// 获取排班模板信息
        /// </summary>
        /// <param name="strSchedulingTemplateName">排班模板名称</param>
        /// <param name="strCircleType">排班模板循环方式</param>        
        /// <param name="strSortKey">排序字段</param>
        /// <param name="pageIndex">当前页码</param>
        /// <param name="pageSize">每页显示记录数</param>
        /// <param name="pageCount">总页码</param>
        /// <returns>返回排班模板信息</returns>
        [OperationContract]
        public List<T_HR_SCHEDULINGTEMPLATEMASTER> GetSchedulingTemplateMasterListByMultSearch(string strOwnerID, string strSchedulingTemplateName, string strCircleType,
            string strSortKey, int pageIndex, int pageSize, ref int pageCount)
        {
            using (SchedulingTemplateMasterBLL bllSchedulingTemplateMaster = new SchedulingTemplateMasterBLL())
            {
                var ents = bllSchedulingTemplateMaster.GetSchedulingTemplateMasterRdListByMultSearch(strOwnerID, strSchedulingTemplateName, strCircleType, strSortKey, pageIndex, pageSize, ref pageCount).ToList();

                if (ents == null)
                {
                    return null;
                }

                return ents.ToList();
            }
        }

        /// <summary>
        /// 新增排班模板信息
        /// </summary>
        /// <param name="entTemp"></param>
        /// <returns></returns>
        [OperationContract]
        public string AddSchedulingTemplateMaster(T_HR_SCHEDULINGTEMPLATEMASTER entTemp)
        {

            using (SchedulingTemplateMasterBLL bllSchedulingTemplateMaster = new SchedulingTemplateMasterBLL())
            {
                return bllSchedulingTemplateMaster.AddTemplateMaster(entTemp);
            }
        }

        /// <summary>
        /// 修改排班模板信息
        /// </summary>
        /// <param name="entTemp"></param>
        /// <returns></returns>
        [OperationContract]
        public string ModifySchedulingTemplateMaster(T_HR_SCHEDULINGTEMPLATEMASTER entTemp)
        {
            using (SchedulingTemplateMasterBLL bllSchedulingTemplateMaster = new SchedulingTemplateMasterBLL())
            {
                return bllSchedulingTemplateMaster.ModifyTemplateMaster(entTemp);
            }
        }

        /// <summary>
        /// 根据主键索引，删除排班模板信息(注：暂定为物理删除)
        /// </summary>
        /// <param name="strSchedulingTemplateMasterId">主键索引</param>
        /// <returns></returns>
        [OperationContract]
        public string RemoveSchedulingTemplateMaster(string strSchedulingTemplateMasterId)
        {
            using (SchedulingTemplateMasterBLL bllSchedulingTemplateMaster = new SchedulingTemplateMasterBLL())
            {
                return bllSchedulingTemplateMaster.DeleteTemplateMaster(strSchedulingTemplateMasterId);
            }
        }

        /// <summary>
        /// 新增排班模板及明细信息
        /// </summary>
        /// <param name="entTemp"></param>
        /// <returns></returns>
        [OperationContract]
        public string AddSchedulingTemplateMasterAndDetail(T_HR_SCHEDULINGTEMPLATEMASTER entMasterTemp, List<T_HR_SCHEDULINGTEMPLATEDETAIL> entDetailTemps)
        {
            using (SchedulingTemplateMasterBLL bllSchedulingTemplateMaster = new SchedulingTemplateMasterBLL())
            {
                return bllSchedulingTemplateMaster.AddSchedulingTemplateMasterAndDetail(entMasterTemp, entDetailTemps);
            }
        }

        /// <summary>
        /// 修改排班模板及明细
        /// </summary>
        /// <param name="entTemp"></param>
        /// <returns></returns>
        [OperationContract]
        public string ModifySchedulingTemplateMasterAndDetail(T_HR_SCHEDULINGTEMPLATEMASTER entMasterTemp, List<T_HR_SCHEDULINGTEMPLATEDETAIL> entDetailTemps)
        {
            using (SchedulingTemplateMasterBLL bllSchedulingTemplateMaster = new SchedulingTemplateMasterBLL())
            {
                return bllSchedulingTemplateMaster.ModifySchedulingTemplateMasterAndDetail(entMasterTemp, entDetailTemps);
            }
        }

        #endregion

        #region T_HR_SCHEDULINGTEMPLATEDETAIL 排班模板明细服务

        /// <summary>
        /// 获取信息
        /// </summary>
        /// <param name="strTemplateDetailId">主键索引</param>
        /// <returns></returns>
        [OperationContract]
        public T_HR_SCHEDULINGTEMPLATEDETAIL GetSchedulingTemplateDetailByID(string strTemplateDetailId)
        {
            using (SchedulingTemplateDetailBLL bllSchedulingTemplateDetail = new SchedulingTemplateDetailBLL())
            {
                return bllSchedulingTemplateDetail.GetSchedulingTemplateDetailByID(strTemplateDetailId);
            }
        }

        /// <summary>
        /// 根据条件，获取排班模板明细信息
        /// </summary>
        /// <param name="strTemplateName">排班模板名称</param>
        /// <param name="strTemplateMasterId">排班模板主表主键索引</param>
        /// <param name="strSortKey">排序字段</param>
        /// <returns></returns>
        [OperationContract]
        public List<T_HR_SCHEDULINGTEMPLATEDETAIL> GetAllSchedulingTemplateDetailRdListByMasterId(string strTemplateMasterId, string strSortKey)
        {
            using (SchedulingTemplateDetailBLL bllSchedulingTemplateDetail = new SchedulingTemplateDetailBLL())
            {
                var ents = bllSchedulingTemplateDetail.GetAllSchedulingTemplateDetailRdListByMultSearch(strTemplateMasterId, strSortKey).ToList();

                if (ents == null)
                {
                    return null;
                }

                return ents.ToList();
            }
        }

        /// <summary>
        /// 获取排班模板明细信息
        /// </summary>
        /// <param name="strTemplateName">排班模板名称</param>
        /// <param name="strTemplateMasterId">排班模板主表主键索引</param>        
        /// <param name="strSortKey">排序字段</param>
        /// <param name="pageIndex">当前页码</param>
        /// <param name="pageSize">每页显示记录数</param>
        /// <param name="pageCount">总页码</param>
        /// <returns>返回排班模板明细信息</returns>
        [OperationContract]
        public List<T_HR_SCHEDULINGTEMPLATEDETAIL> GetSchedulingTemplateDetailListByMultSearch(string strTemplateMasterId, string strSortKey,
            int pageIndex, int pageSize, ref int pageCount)
        {
            using (SchedulingTemplateDetailBLL bllSchedulingTemplateDetail = new SchedulingTemplateDetailBLL())
            {
                var ents = bllSchedulingTemplateDetail.GetSchedulingTemplateDetailRdListByMultSearch(strTemplateMasterId, strSortKey, pageIndex, pageSize, ref pageCount).ToList();

                if (ents == null)
                {
                    return null;
                }

                return ents.ToList();
            }

        }

        /// <summary>
        /// 新增排班模板明细信息
        /// </summary>
        /// <param name="entTemp"></param>
        /// <returns></returns>
        [OperationContract]
        public string AddTemplateDetail(T_HR_SCHEDULINGTEMPLATEDETAIL entTemp)
        {
            using (SchedulingTemplateDetailBLL bllSchedulingTemplateDetail = new SchedulingTemplateDetailBLL())
            {
                return bllSchedulingTemplateDetail.AddTemplateDetail(entTemp);
            }
        }

        /// <summary>
        /// 修改排班模板明细信息
        /// </summary>
        /// <param name="entTemp"></param>
        /// <returns></returns>
        [OperationContract]
        public string ModifyTemplateDetail(T_HR_SCHEDULINGTEMPLATEDETAIL entTemp)
        {
            using (SchedulingTemplateDetailBLL bllSchedulingTemplateDetail = new SchedulingTemplateDetailBLL())
            {
                return bllSchedulingTemplateDetail.ModifyTemplateDetail(entTemp);
            }
        }

        /// <summary>
        /// 根据主键索引，删除排班模板明细信息(注：暂定为物理删除)
        /// </summary>
        /// <param name="strTemplateDetailId">主键索引</param>
        /// <returns></returns>
        [OperationContract]
        public string RemoveTemplateDetail(string strTemplateDetailId)
        {
            using (SchedulingTemplateDetailBLL bllSchedulingTemplateDetail = new SchedulingTemplateDetailBLL())
            {
                return bllSchedulingTemplateDetail.DeleteTemplateDetail(strTemplateDetailId);
            }
        }

        /// <summary>
        /// 根据排班模板主表主键索引，删除关联的排班模板明细记录
        /// </summary>
        /// <param name="strTemplateMasterID">排班模板主表主键索引</param>
        /// <returns>返回处理后的消息</returns>
        [OperationContract]
        public string RemoveTemplateDetailByTemplateMasterID(string strTemplateMasterID)
        {
            using (SchedulingTemplateDetailBLL bllSchedulingTemplateDetail = new SchedulingTemplateDetailBLL())
            {
                return bllSchedulingTemplateDetail.DeleteByTemplateMasterID(strTemplateMasterID);
            }
        }


        /// <summary>
        /// 对指定的排班设置主记录添加其相关的子记录
        /// </summary>
        /// <param name="strTemplateMasterID">排班模板主表主键索引</param>
        /// <param name="entTemps">排班模板主表相关子记录集</param>
        /// <returns>返回处理后的消息</returns>
        [OperationContract]
        public string AddDetailForTemplateMaster(string strTemplateMasterID, List<T_HR_SCHEDULINGTEMPLATEDETAIL> entTemps)
        {
            using (SchedulingTemplateDetailBLL bllSchedulingTemplateDetail = new SchedulingTemplateDetailBLL())
            {
                return bllSchedulingTemplateDetail.AddDetailForTemplateMaster(strTemplateMasterID, entTemps);
            }
        }

        #endregion

        #region T_HR_SHIFTDEFINE 考勤班次服务

        /// <summary>
        /// 获取考勤班次信息
        /// </summary>
        /// <param name="strShiftDefineId">主键索引</param>
        /// <returns></returns>
        [OperationContract]
        public T_HR_SHIFTDEFINE GetShiftDefineByID(string strShiftDefineId)
        {
            using (ShiftDefineBLL bllShiftDefine = new ShiftDefineBLL())
            {
                return bllShiftDefine.GetShiftDefineByID(strShiftDefineId);
            }
        }

        /// <summary>
        /// 获取考勤班次信息
        /// </summary>
        /// <param name="strShiftdEfineName">考勤班次名称</param>
        /// <param name="strCompanyID">公司序号</param>        
        /// <param name="strSortKey">排序字段</param>
        /// <param name="pageIndex">当前页码</param>
        /// <param name="pageSize">每页显示记录数</param>
        /// <param name="pageCount">总页码</param>
        /// <returns>返回考勤班次信息</returns>
        [OperationContract]
        public List<T_HR_SHIFTDEFINE> GetShiftDefineListByMultSearch(string strOwnerID, string strShiftDefineName, string strCompanyID,
            string strSortKey, int pageIndex, int pageSize, ref int pageCount)
        {
            using (ShiftDefineBLL bllShiftDefine = new ShiftDefineBLL())
            {
                var ents = bllShiftDefine.GetShiftDefineListByMultSearch(strOwnerID, strShiftDefineName, strCompanyID, strSortKey, pageIndex, pageSize, ref pageCount).ToList();

                if (ents == null)
                {
                    return null;
                }

                return ents.ToList();
            }

        }

        /// <summary>
        /// 新增考勤班次信息
        /// </summary>
        /// <param name="entTemp"></param>
        /// <returns></returns>
        [OperationContract]
        public string AddShiftDefine(T_HR_SHIFTDEFINE entTemp)
        {
            using (ShiftDefineBLL bllShiftDefine = new ShiftDefineBLL())
            {
                return bllShiftDefine.AddShiftDefine(entTemp);
            }
        }

        /// <summary>
        /// 修改考勤班次信息
        /// </summary>
        /// <param name="entTemp"></param>
        /// <returns></returns>
        [OperationContract]
        public string ModifyShiftDefine(T_HR_SHIFTDEFINE entTemp)
        {
            using (ShiftDefineBLL bllShiftDefine = new ShiftDefineBLL())
            {
                return bllShiftDefine.ModifyShiftDefine(entTemp);
            }
        }

        /// <summary>
        /// 根据主键索引，删除考勤班次信息(注：暂定为物理删除)
        /// </summary>
        /// <param name="strShiftDefineId">主键索引</param>
        /// <returns></returns>
        [OperationContract]
        public string RemoveShiftDefine(string strShiftDefineId)
        {
            using (ShiftDefineBLL bllShiftDefine = new ShiftDefineBLL())
            {
                return bllShiftDefine.DeleteShiftDefine(strShiftDefineId);
            }
        }

        #endregion

        #region T_HR_EMPLOYEEOVERTIMERECORD 员工加班记录服务
        /// <summary>
        /// 用于实体Grid中显示数据的分页查询,获取所有的出差记录信息
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页显示条数</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件中的参数值</param>
        /// <param name="pageCount">返回总页数</param>
        /// <returns>查询结果集</returns>
        [OperationContract]
        public List<T_HR_EMPLOYEEOVERTIMERECORD> EmployeeOverTimeRecordPaging(int pageIndex, int pageSize, string sort, string filterString, List<object> paras, ref int pageCount, string strCheckState, string strOwnerID)
        {
            using (OverTimeRecordBLL bllOverTimeRecord = new OverTimeRecordBLL())
            {
                var ents = bllOverTimeRecord.EmployeeOverTimeRecordPaging(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, strCheckState, strOwnerID);

                if (ents == null)
                {
                    return null;
                }

                return ents.ToList();
            }
        }

        /// <summary>
        /// 根据主键索引，获取员工加班信息
        /// </summary>
        /// <param name="strOverTimeRecordId">主键索引</param>
        /// <returns></returns>
        [OperationContract]
        public T_HR_EMPLOYEEOVERTIMERECORD GetOverTimeRdByID(string strOverTimeRecordId)
        {
            using (OverTimeRecordBLL bllOverTimeRecord = new OverTimeRecordBLL())
            {
                return bllOverTimeRecord.GetOverTimeRdByID(strOverTimeRecordId);
            }
        }

        /// <summary>
        /// 新增员工加班信息
        /// </summary>
        /// <param name="entOTRd"></param>
        /// <returns></returns>
        [OperationContract]
        public string AddOverTimeRd(T_HR_EMPLOYEEOVERTIMERECORD entOTRd)
        {
            using (OverTimeRecordBLL bllOverTimeRecord = new OverTimeRecordBLL())
            {
                return bllOverTimeRecord.OverTimeRecordAdd(entOTRd);
            }
        }

        /// <summary>
        /// 修改员工加班信息
        /// </summary>
        /// <param name="entOTRd"></param>
        /// <returns></returns>
        [OperationContract]
        public string ModifyOverTimeRd(T_HR_EMPLOYEEOVERTIMERECORD entOTRd)
        {
            using (OverTimeRecordBLL bllOverTimeRecord = new OverTimeRecordBLL())
            {
                return bllOverTimeRecord.ModifyOverTimeRd(entOTRd);
            }
        }

        /// <summary>
        /// 删除员工加班信息(注：仅在未提交状态下，方可进行物理删除)
        /// </summary>
        /// <param name="strOverTimeRecordId">主键索引</param>
        /// <returns></returns>
        [OperationContract]
        public bool RemoveOverTimeRd(string[] strOverTimeRecordId)
        {
            using (OverTimeRecordBLL bllOverTimeRecord = new OverTimeRecordBLL())
            {
                int rslt = bllOverTimeRecord.DeleteOverTimeRd(strOverTimeRecordId);
                return (rslt > 0);
            }
        }

        /// <summary>
        /// 审核员工加班信息
        /// </summary>
        /// <param name="strOverTimeRecordId">主键索引</param>
        /// <param name="strCheckState">审核状态</param>
        /// <returns></returns>
        [OperationContract]
        public string AuditOverTimeRd(string strOverTimeRecordID, string strCheckState)
        {
            using (OverTimeRecordBLL bllOverTimeRecord = new OverTimeRecordBLL())
            {
                string rslt = bllOverTimeRecord.AuditOverTimeRd(strOverTimeRecordID, strCheckState);
                return rslt;
            }
        }
        #endregion

        #region T_HR_ATTENDANCESOLUTIONASIGN 考勤方案应用服务


        /// <summary>
        /// 获取考勤方案应用信息
        /// </summary>
        /// <param name="strAttendanceSolutionAsignId">主键索引</param>
        /// <returns></returns>
        [OperationContract]
        public T_HR_ATTENDANCESOLUTIONASIGN GetAttendanceSolutionAsignByID(string strAttendanceSolutionAsignId)
        {
            using (AttendanceSolutionAsignBLL bllAttendanceSolutionAsign = new AttendanceSolutionAsignBLL())
            {
                return bllAttendanceSolutionAsign.GetAttendanceSolutionAsignByID(strAttendanceSolutionAsignId);
            }

        }

        /// <summary>
        /// 获取考勤方案应用信息
        /// </summary>
        /// <param name="strAttendanceSolutionAsignId">主键索引</param>
        /// <returns></returns>
        [OperationContract]
        public int CheckAttSolIsExistsAsignRd(string strAttendanceSolutionId, string strCheckStates)
        {
            using (AttendanceSolutionAsignBLL bllAttendanceSolutionAsign = new AttendanceSolutionAsignBLL())
            {
                return bllAttendanceSolutionAsign.CheckAttSolIsExistsAsignRd(strAttendanceSolutionId, strCheckStates);
            }
        }

        /// <summary>
        /// 获取考勤方案应用信息
        /// </summary>
        /// <param name="AttendanceSolutionName">考勤方案名</param>
        /// <param name="strAssignedObjectType">分配对象类型</param>    
        /// <param name="strSortKey">排序字段</param>
        /// <param name="pageIndex">当前页码</param>
        /// <param name="pageSize">每页显示记录数</param>
        /// <param name="pageCount">总页码</param>
        /// <returns>返回考勤方案应用信息</returns>
        [OperationContract]
        public List<T_HR_ATTENDANCESOLUTIONASIGN> GetAttendanceSolutionAsignRdListByMultSearch(string strOwnerID, string strCheckState, string AttendanceSolutionName, string strAssignedObjectType,
            string strSortKey, int pageIndex, int pageSize, ref int pageCount)
        {
            using (AttendanceSolutionAsignBLL bllAttendanceSolutionAsign = new AttendanceSolutionAsignBLL())
            {
                var ents = bllAttendanceSolutionAsign.GetAttendanceSolutionAsignRdListByMultSearch(strOwnerID, strCheckState, AttendanceSolutionName, strAssignedObjectType, strSortKey, pageIndex, pageSize, ref pageCount);

                if (ents == null)
                {
                    return null;
                }

                return ents.ToList();
            }
        }

        /// <summary>
        /// 新增考勤方案应用信息
        /// </summary>
        /// <param name="entTemp"></param>
        /// <returns></returns>
        [OperationContract]
        public string AddAttendanceSolutionAsign(T_HR_ATTENDANCESOLUTIONASIGN entTemp)
        {

            using (AttendanceSolutionAsignBLL bllAttendanceSolutionAsign = new AttendanceSolutionAsignBLL())
                return bllAttendanceSolutionAsign.AddAttSolAsign(entTemp);

        }

        /// <summary>
        /// 修改考勤方案应用信息
        /// </summary>
        /// <param name="entTemp"></param>
        /// <returns></returns>
        [OperationContract]
        public string ModifyAttendanceSolutionAsign(T_HR_ATTENDANCESOLUTIONASIGN entTemp)
        {

            using (AttendanceSolutionAsignBLL bllAttendanceSolutionAsign = new AttendanceSolutionAsignBLL())
                return bllAttendanceSolutionAsign.ModifyAttSolAsign(entTemp);

        }

        /// <summary>
        /// 根据主键索引，删除考勤方案应用信息(注：暂定为物理删除)
        /// </summary>
        /// <param name="strAttendanceSolutionAsignId">主键索引</param>
        /// <returns></returns>
        [OperationContract]
        public string RemoveAttendanceSolutionAsign(string strAttendanceSolutionAsignId)
        {

            using (AttendanceSolutionAsignBLL bllAttendanceSolutionAsign = new AttendanceSolutionAsignBLL())
                return bllAttendanceSolutionAsign.DeleteAttSolAsign(strAttendanceSolutionAsignId);

        }

        /// <summary>
        /// 根据主键索引，及审核结果更新对应的考勤方案应用信息
        /// </summary>
        /// <param name="strAttendanceSolutionAsignId"></param>
        /// <param name="strCheckState"></param>
        /// <returns></returns>
        [OperationContract]
        public string AuditAttSolAsign(string strAttendanceSolutionAsignId, string strCheckState)
        {
            using (AttendanceSolutionAsignBLL bllAttendanceSolutionAsign = new AttendanceSolutionAsignBLL())
            {
                string strActionType = "Manual";
                return bllAttendanceSolutionAsign.AuditAttSolAsign(strAttendanceSolutionAsignId, strCheckState, strActionType);
            }
        }

        /// <summary>
        /// 根据主键索引，获取对应的考勤方案应用，并实施该方案
        /// </summary>
        /// <param name="strAttendanceSolutionAsignId"></param>
        /// <param name="strCheckState"></param>
        /// <returns></returns>
        [OperationContract]
        public string AsignAttendanceSolution(string strAttendanceSolutionAsignId)
        {
            using (AttendanceSolutionAsignBLL bllAttendanceSolutionAsign = new AttendanceSolutionAsignBLL())
            {
                DateTime dtAsignDate = DateTime.Parse(DateTime.Now.ToString("yyyy-MM") + "-1");
                T_HR_ATTENDANCESOLUTIONASIGN ent = bllAttendanceSolutionAsign.GetAttendanceSolutionAsignByID(strAttendanceSolutionAsignId);
                return bllAttendanceSolutionAsign.AsignAttendanceSolution(ent, dtAsignDate);
            }
        }

        /// <summary>
        /// 对所有公司解决方案生成考勤初始化记录
        /// </summary>
        [OperationContract]
        public void AsignAttendanceSolutionWithAll()
        {
            using (AttendanceSolutionAsignBLL bllAttendanceSolutionAsign = new AttendanceSolutionAsignBLL())
            {
                bllAttendanceSolutionAsign.AsignAttendanceSolutionWithAllCompany();
            }
        }

        /// <summary>
        /// 根据主键索引，获取对应的考勤方案应用，并实施该方案
        /// </summary>
        /// <param name="strAttendanceSolutionAsignId"></param>
        /// <param name="strCheckState"></param>
        /// <returns></returns>
        [OperationContract]
        public string AsignAttendanceSolutionByOrgID(string strOrgType, string strOrgId)
        {
            using (AttendanceSolutionAsignBLL bllAttendanceSolutionAsign = new AttendanceSolutionAsignBLL())
            {
                return bllAttendanceSolutionAsign.AsignAttendanceSolutionByOrgID(strOrgType, strOrgId);
            }
        }

        /// <summary>
        /// 根据主键索引，获取对应的考勤方案应用，并实施该方案到指定的月份2012-2
        /// </summary
        /// <param name="strAttendanceSolutionAsignId"></param>
        /// <param name="strCheckState"></param>
        /// <returns></returns>
        [OperationContract]
        public string AsignAttendanceSolutionByOrgIDAndMonth(string strOrgType, string strOrgId, string strCurYearMonth)
        {
            using (AttendanceSolutionAsignBLL bllAttendanceSolutionAsign = new AttendanceSolutionAsignBLL())
            {
                return bllAttendanceSolutionAsign.AsignAttendanceSolutionByOrgID(strOrgType, strOrgId, strCurYearMonth);
            }
        }
        ///// <summary>
        ///// 2012/12/19
        ///// 根据传入的id获取考勤方案信息
        ///// </summary>
        ///// <param name="objectID">主键id</param>
        ///// <param name="objectType">分配类型（1.公司，2.部门，3.岗位，4.员工）</param>
        ///// <returns></returns>
        //[OperationContract]
        //public List<T_HR_ATTENDANCESOLUTIONASIGN> GetAttendanceSolutionAsignByObjectID(string objectID, int objectType)
        //{
        //    using (AttendanceSolutionAsignBLL bllAttendanceSolutionAsign = new AttendanceSolutionAsignBLL())
        //    {
        //        return bllAttendanceSolutionAsign.GetAttendanceSolutionAsignByObjectID(objectID, objectType);
        //    }
        //}


        /// <summary>
        /// 获取当前月份所有已应用考勤方案的公司ID集合
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        public List<string> GetAllCompanyIDByAttendSolAsign(string strCurYearMonth)
        {
            using (AttendanceSolutionAsignBLL bllAttendanceSolutionAsign = new AttendanceSolutionAsignBLL())
            {
                return bllAttendanceSolutionAsign.GetAllCompanyIDByAttendSolAsign(strCurYearMonth);
            }
        }
        #endregion

        #region T_HR_ATTENDANCEDEDUCTMASTER 考勤异常扣款主表
        /// <summary>
        /// 获取考勤异常扣款信息
        /// </summary>
        /// <param name="strAttendanceDeductMasterId">主键索引</param>
        /// <returns></returns>
        [OperationContract]
        public T_HR_ATTENDANCEDEDUCTMASTER GetAttendanceDeductMasterByID(string strAttendanceDeductMasterId)
        {
            using (AttendanceDeductMasterBLL bllAttendanceDeductMaster = new AttendanceDeductMasterBLL())
            {
                return bllAttendanceDeductMaster.GetAttendanceDeductMasterByID(strAttendanceDeductMasterId);
            }
        }

        /// <summary>
        /// 根据条件，获取考勤异常扣款信息
        /// </summary>
        /// <param name="strAttType">考勤状态</param>
        /// <param name="strFineType">考勤扣款方式</param>
        /// <param name="strSortKey">排序字段</param>
        /// <param name="pageIndex">当前页索引</param>
        /// <param name="pageSize">页码大小</param>
        /// <param name="pageCount">总页数</param>
        /// <returns>考勤异常扣款信息</returns>
        [OperationContract]
        public List<T_HR_ATTENDANCEDEDUCTMASTER> GetAttendanceDeductMasterRdListByMultSearch(string strOwnerID, string strAttType,
            string strSortKey, int pageIndex, int pageSize, ref int pageCount)
        {
            using (AttendanceDeductMasterBLL bllAttendanceDeductMaster = new AttendanceDeductMasterBLL())
            {
                var ents = bllAttendanceDeductMaster.GetAttendanceDeductMasterRdListByMultSearch(strOwnerID, strAttType, strSortKey, pageIndex, pageSize, ref pageCount).ToList();

                if (ents == null)
                {
                    return null;
                }
                return ents.ToList();
            }
        }

        /// <summary>
        /// 新增考勤异常扣款信息
        /// </summary>
        /// <param name="entRd"></param>
        /// <returns></returns>
        [OperationContract]
        public string AddAttendanceDeductMaster(T_HR_ATTENDANCEDEDUCTMASTER entRd)
        {
            using (AttendanceDeductMasterBLL bllAttendanceDeductMaster = new AttendanceDeductMasterBLL())
            {
                return bllAttendanceDeductMaster.AddDeductMaster(entRd);
            }
        }

        /// <summary>
        /// 修改考勤异常扣款信息
        /// </summary>
        /// <param name="entRd"></param>
        /// <returns></returns>
        [OperationContract]
        public string ModifyAttendanceDeductMaster(T_HR_ATTENDANCEDEDUCTMASTER entRd)
        {
            using (AttendanceDeductMasterBLL bllAttendanceDeductMaster = new AttendanceDeductMasterBLL())
            {
                return bllAttendanceDeductMaster.ModifyDeductMaster(entRd);
            }
        }

        /// <summary>
        /// 根据主键索引，删除考勤异常扣款信息(注：暂定为物理删除)
        /// </summary>
        /// <param name="strAttendanceDeductMasterId">主键索引</param>
        /// <returns></returns>
        [OperationContract]
        public string RemoveAttendanceDeductMaster(string strAttendanceDeductMasterId)
        {
            using (AttendanceDeductMasterBLL bllAttendanceDeductMaster = new AttendanceDeductMasterBLL())
            {
                return bllAttendanceDeductMaster.DeleteDeductMaster(strAttendanceDeductMasterId);
            }
        }

        #endregion

        #region T_HR_ATTENDANCEDEDUCTDETAIL 考勤异常扣款明细表
        /// <summary>
        /// 获取考勤异常扣款信息
        /// </summary>
        /// <param name="strDeductDetailId">主键索引</param>
        /// <returns></returns>
        [OperationContract]
        public T_HR_ATTENDANCEDEDUCTDETAIL GetAttendanceDeductDetailByID(string strDeductDetailId)
        {
            using (AttendanceDeductDetailBLL bllAttendanceDeductDetail = new AttendanceDeductDetailBLL())
            {
                return bllAttendanceDeductDetail.GetAttendanceDeductDetailByID(strDeductDetailId);
            }
        }

        /// <summary>
        /// 根据条件，获取考勤异常扣款信息
        /// </summary>
        /// <param name="strOwnerID">登录用户的员工ID(权限控制)</param>
        /// <param name="strDeductMasterID">外键索引</param>
        /// <param name="strSortKey">排序字段</param>
        /// <param name="pageIndex">当前页索引</param>
        /// <param name="pageSize">页码大小</param>
        /// <param name="pageCount">总页数</param>
        /// <returns>考勤异常扣款信息</returns>
        [OperationContract]
        public List<T_HR_ATTENDANCEDEDUCTDETAIL> GetAttendanceDeductDetailRdListByMultSearch(string strOwnerID, string strDeductMasterID,
            string strSortKey, int pageIndex, int pageSize, ref int pageCount)
        {
            using (AttendanceDeductDetailBLL bllAttendanceDeductDetail = new AttendanceDeductDetailBLL())
            {
                var ents = bllAttendanceDeductDetail.GetAttendanceDeductDetailRdListByMultSearch(strOwnerID, strDeductMasterID, strSortKey, pageIndex, pageSize, ref pageCount);

                if (ents == null)
                {
                    return null;
                }

                return ents.ToList();
            }
        }

        /// <summary>
        /// 新增考勤异常扣款信息
        /// </summary>
        /// <param name="entRd"></param>
        /// <returns></returns>
        [OperationContract]
        public string AddAttendanceDeductDetail(T_HR_ATTENDANCEDEDUCTDETAIL entRd)
        {
            using (AttendanceDeductDetailBLL bllAttendanceDeductDetail = new AttendanceDeductDetailBLL())
            {
                return bllAttendanceDeductDetail.AddDeductDetail(entRd);
            }
        }

        /// <summary>
        /// 修改考勤异常扣款信息
        /// </summary>
        /// <param name="entRd"></param>
        /// <returns></returns>
        [OperationContract]
        public string ModifyAttendanceDeductDetail(T_HR_ATTENDANCEDEDUCTDETAIL entRd)
        {
            using (AttendanceDeductDetailBLL bllAttendanceDeductDetail = new AttendanceDeductDetailBLL())
            {
                return bllAttendanceDeductDetail.ModifyDeductDetail(entRd);
            }
        }

        /// <summary>
        /// 根据主键索引，删除考勤异常扣款信息(注：暂定为物理删除)
        /// </summary>
        /// <param name="strDeductDetailId">主键索引</param>
        /// <returns></returns>
        [OperationContract]
        public string RemoveAttendanceDeductDetail(string strDeductDetailId)
        {
            using (AttendanceDeductDetailBLL bllAttendanceDeductDetail = new AttendanceDeductDetailBLL())
            {
                return bllAttendanceDeductDetail.DeleteDeductDetail(strDeductDetailId);
            }
        }

        #endregion

        #region T_HR_ATTENDANCESOLUTIONDEDUCT 考勤方案异常扣款服务

        /// <summary>
        /// 获取信息
        /// </summary>
        /// <param name="strAttendanceSolutionDeductId">主键索引</param>
        /// <returns></returns>
        [OperationContract]
        public T_HR_ATTENDANCESOLUTIONDEDUCT GetAttendanceSolutionDeductByID(string strAttendanceSolutionDeductId)
        {
            using (AttendanceSolutionDeductBLL bllAttendanceSolutionDeduct = new AttendanceSolutionDeductBLL())
            {
                return bllAttendanceSolutionDeduct.GetAttendanceSolutionDeductByID(strAttendanceSolutionDeductId);
            }
        }

        /// <summary>
        /// 获取信息
        /// </summary>
        /// <param name="strAttendanceSolID">考勤方案编号</param>   
        /// <param name="strSortKey">排序字段</param>
        /// <returns>返回信息</returns>
        [OperationContract]
        public List<T_HR_ATTENDANCESOLUTIONDEDUCT> GetAttendanceSolutionDeductRdListByAttSolID(string strAttendanceSolID, string strSortKey)
        {
            using (AttendanceSolutionDeductBLL bllAttendanceSolutionDeduct = new AttendanceSolutionDeductBLL())
            {
                var ents = bllAttendanceSolutionDeduct.GetAttendanceSolutionDeductRdListByAttSolID(strAttendanceSolID, strSortKey);

                if (ents == null)
                {
                    return null;
                }

                return ents.ToList();
            }
        }

        /// <summary>
        /// 新增信息
        /// </summary>
        /// <param name="entTemp"></param>
        /// <returns></returns>
        [OperationContract]
        public string AddAttendanceSolutionDeduct(T_HR_ATTENDANCESOLUTIONDEDUCT entTemp)
        {

            using (AttendanceSolutionDeductBLL bllAttendanceSolutionDeduct = new AttendanceSolutionDeductBLL())
            {
                return bllAttendanceSolutionDeduct.AddAttSolDeduct(entTemp);
            }
        }

        /// <summary>
        /// 修改信息
        /// </summary>
        /// <param name="entTemp"></param>
        /// <returns></returns>
        [OperationContract]
        public string ModifyAttendanceSolutionDeduct(T_HR_ATTENDANCESOLUTIONDEDUCT entTemp)
        {
            using (AttendanceSolutionDeductBLL bllAttendanceSolutionDeduct = new AttendanceSolutionDeductBLL())
            {
                return bllAttendanceSolutionDeduct.ModifyAttSolDeduct(entTemp);
            }
        }

        /// <summary>
        /// 根据主键索引，删除信息(注：暂定为物理删除)
        /// </summary>
        /// <param name="strAttendanceSolutionDeductId">主键索引</param>
        /// <returns></returns>
        [OperationContract]
        public string RemoveAttendanceSolutionDeduct(string strAttendanceSolutionDeductId)
        {
            using (AttendanceSolutionDeductBLL bllAttendanceSolutionDeduct = new AttendanceSolutionDeductBLL())
            {
                return bllAttendanceSolutionDeduct.DeleteAttSolDeduct(strAttendanceSolutionDeductId);
            }
        }

        #endregion

        #region T_HR_FREELEAVEDAYSET 带薪假设置

        /// <summary>
        /// 获取带薪假设置信息
        /// </summary>
        /// <param name="strFreeLeaveDaySetId">主键索引</param>
        /// <returns></returns>
        [OperationContract]
        public T_HR_FREELEAVEDAYSET GetFreeLeaveDaySetByID(string strFreeLeaveDaySetId)
        {
            using (FreeLeaveDaySetBLL bllFreeLeaveDaySet = new FreeLeaveDaySetBLL())
            {
                return bllFreeLeaveDaySet.GetFreeLeaveDaySetByID(strFreeLeaveDaySetId);
            }
        }

        /// <summary>
        /// 根据考勤方案主键索引，获取其配置的带薪假设置信息
        /// </summary>        
        /// <param name="strAttendanceSolutionId">配置带薪假的考勤方案主键索引</param>
        /// <param name="strSortKey">排序字段</param>
        /// <returns>返回带薪假设置信息</returns>
        [OperationContract]
        public List<T_HR_FREELEAVEDAYSET> GetFreeLeaveDaySetRdListForAttendanceSolution(string strAttendanceSolutionId, string strSortKey)
        {
            using (FreeLeaveDaySetBLL bllFreeLeaveDaySet = new FreeLeaveDaySetBLL())
            {
                var ents = bllFreeLeaveDaySet.GetFreeLeaveDaySetRdListForAttendanceSolution(strAttendanceSolutionId, strSortKey);

                if (ents == null)
                {
                    return null;
                }

                return ents.ToList();
            }
        }


        /// <summary>
        /// 根据条件，获取带薪假设置信息
        /// </summary>
        /// <param name="strLeaveTypeSetID">考勤方案外键索引</param>
        /// <param name="strIsFactor">是否扣全勤</param>
        /// <param name="strSortKey">排序字段</param>
        /// <param name="pageIndex">当前页索引</param>
        /// <param name="pageSize">页码大小</param>
        /// <param name="pageCount">总页数</param>
        /// <returns>员工带薪假信息</returns>
        [OperationContract]
        public List<T_HR_FREELEAVEDAYSET> GetFreeLeaveDaySetRdListByMultSearch(string strOwnerID, string strLeaveTypeSetID, string strIsFactor, string strSortKey,
            int pageIndex, int pageSize, ref int pageCount)
        {
            using (FreeLeaveDaySetBLL bllFreeLeaveDaySet = new FreeLeaveDaySetBLL())
            {
                var ents = bllFreeLeaveDaySet.GetFreeLeaveDaySetRdListByMultSearch(strOwnerID, strLeaveTypeSetID, strIsFactor, strSortKey, pageIndex, pageSize, ref pageCount);

                if (ents == null)
                {
                    return null;
                }


                return ents.ToList();
            }
        }

        /// <summary>
        /// 新增带薪假设置信息
        /// </summary>
        /// <param name="entRd"></param>
        /// <returns></returns>
        [OperationContract]
        public string AddFreeLeaveDaySet(T_HR_FREELEAVEDAYSET entRd)
        {

            using (FreeLeaveDaySetBLL bllFreeLeaveDaySet = new FreeLeaveDaySetBLL())
            {
                return bllFreeLeaveDaySet.AddFreeLeaveDaySet(entRd);
            }
        }

        /// <summary>
        /// 修改带薪假设置信息
        /// </summary>
        /// <param name="entRd"></param>
        /// <returns></returns>
        [OperationContract]
        public string ModifyFreeLeaveDaySet(T_HR_FREELEAVEDAYSET entRd)
        {

            using (FreeLeaveDaySetBLL bllFreeLeaveDaySet = new FreeLeaveDaySetBLL())
            {
                return bllFreeLeaveDaySet.ModifyFreeLeaveDaySet(entRd);
            }
        }

        /// <summary>
        /// 根据主键索引，删除带薪假设置信息(注：暂定为物理删除)
        /// </summary>
        /// <param name="strFreeLeaveDaySetId">主键索引</param>
        /// <returns></returns>
        [OperationContract]
        public string RemoveFreeLeaveDaySet(string strFreeLeaveDaySetId)
        {

            using (FreeLeaveDaySetBLL bllFreeLeaveDaySet = new FreeLeaveDaySetBLL())
            {
                return bllFreeLeaveDaySet.DeleteFreeLeaveDaySet(strFreeLeaveDaySetId);
            }

        }
        /// <summary>
        /// 根据员工ID，获取员工当前的带薪假期
        /// </summary>
        /// <param name="employeeID">员工ID</param>
        /// <returns>返回带薪假期</returns>
        [OperationContract]
        public List<V_EMPLOYEELEAVE> GetFreeLeaveDaySetByEmployeeID(string employeeID)
        {
            using (FreeLeaveDaySetBLL bll = new FreeLeaveDaySetBLL())
            {
                return bll.GetFreeLeaveDaySetByEmployeeID(employeeID);
            }
        }
        #endregion

        #region T_HR_LEAVETYPESET 请假类型设置服务

        /// <summary>
        /// 获取请假类型设置信息
        /// </summary>
        /// <param name="strLeaveTypeSetId">主键索引</param>
        /// <returns></returns>
        [OperationContract]
        public T_HR_LEAVETYPESET GetLeaveTypeSetByID(string strLeaveTypeSetId)
        {
            using (LeaveTypeSetBLL bllLeaveTypeSet = new LeaveTypeSetBLL())
            {
                return bllLeaveTypeSet.GetLeaveTypeSetByID(strLeaveTypeSetId);
            }
        }

        /// <summary>
        /// 根据条件，获取请假类型设置信息
        /// </summary>
        /// <param name="strVacName">假期名称</param>
        /// <param name="strVacYear">假期生效年份</param>
        /// <param name="strCountyType">假期执行国家</param>
        /// <param name="strSortKey">排序字段</param>
        /// <param name="pageIndex">当前页索引</param>
        /// <param name="pageSize">页码大小</param>
        /// <param name="pageCount">总页数</param>
        /// <returns>员工请假类型信息</returns>
        [OperationContract]
        public List<T_HR_LEAVETYPESET> GetLeaveTypeSetRdListForAttendanceSolution(string strAttendanceSolutionId, string strSortKey)
        {
            using (LeaveTypeSetBLL bllLeaveTypeSet = new LeaveTypeSetBLL())
            {
                var ents = bllLeaveTypeSet.GetLeaveTypeSetRdListForAttendanceSolution(strAttendanceSolutionId, strSortKey);

                if (ents == null)
                {
                    return null;
                }

                return ents.ToList();
            }
        }
        /// <summary>
        /// 根据权限获取假期标准列表
        /// </summary>
        /// <param name="employeeID">员工ID</param>
        /// <returns>返回假期标准列表</returns>
        [OperationContract]
        public List<T_HR_LEAVETYPESET> GetLeaveTypeSetAll(string employeeID)
        {
            using (LeaveTypeSetBLL bll = new LeaveTypeSetBLL())
            {
                return bll.GetLeaveTypeSetAll(employeeID);
            };
        }
        /// <summary>
        /// 根据条件，获取请假类型设置信息
        /// </summary>
        /// <param name="strOwnerID">登录人的员工ID</param>
        /// <param name="strLeaveTypeValue">假期类别</param>
        /// <param name="strSortKey">排序字段</param>
        /// <param name="pageIndex">当前页索引</param>
        /// <param name="pageSize">页码大小</param>
        /// <param name="pageCount">总页数</param>
        /// <returns>员工请假类型信息</returns>
        [OperationContract]
        public List<T_HR_LEAVETYPESET> GetLeaveTypeSetRdListByMultSearch(string strOwnerID, string strLeaveTypeValue,
            string strSortKey, int pageIndex, int pageSize, ref int pageCount)
        {
            using (LeaveTypeSetBLL bllLeaveTypeSet = new LeaveTypeSetBLL())
            {
                var ents = bllLeaveTypeSet.GetLeaveTypeSetRdListByMultSearch(strOwnerID, strLeaveTypeValue, strSortKey, pageIndex, pageSize, ref pageCount);

                if (ents == null)
                {
                    return null;
                }

                return ents.ToList();
            }
        }

        /// <summary>
        /// 新增请假类型设置信息
        /// </summary>
        /// <param name="entVacRd"></param>
        /// <returns></returns>
        [OperationContract]
        public string AddLeaveTypeSet(T_HR_LEAVETYPESET entLTRd)
        {
            using (LeaveTypeSetBLL bllLeaveTypeSet = new LeaveTypeSetBLL())
            {
                return bllLeaveTypeSet.AddLeaveTypeSet(entLTRd);
            }
        }

        /// <summary>
        /// 修改请假类型设置信息
        /// </summary>
        /// <param name="entLTRd"></param>
        /// <returns></returns>
        [OperationContract]
        public string ModifyLeaveTypeSet(T_HR_LEAVETYPESET entLTRd)
        {
            using (LeaveTypeSetBLL bllLeaveTypeSet = new LeaveTypeSetBLL())
            {
                return bllLeaveTypeSet.ModifyLeaveTypeSet(entLTRd);
            }
        }

        /// <summary>
        /// 根据主键索引，删除请假类型设置信息(注：暂定为物理删除)
        /// </summary>
        /// <param name="strLeaveTypeSetId">主键索引</param>
        /// <returns></returns>
        [OperationContract]
        public string RemoveLeaveTypeSet(string strLeaveTypeSetId)
        {

            using (LeaveTypeSetBLL bllLeaveTypeSet = new LeaveTypeSetBLL())
            {
                return bllLeaveTypeSet.DeleteLeaveTypeSet(strLeaveTypeSetId);
            }
        }

        #endregion

        #region T_HR_OVERTIMEREWARD 加班报酬设置服务
        /// <summary>
        /// 获取加班报酬设置信息
        /// </summary>
        /// <param name="strOvertimeRewardId">主键索引</param>
        /// <returns></returns>
        [OperationContract]
        public T_HR_OVERTIMEREWARD GetOvertimeRewardByID(string strOvertimeRewardId)
        {
            using (OvertimeRewardBLL bllOvertimeReward = new OvertimeRewardBLL())
            {
                return bllOvertimeReward.GetOvertimeRewardByID(strOvertimeRewardId);
            }
        }

        /// <summary>
        /// 根据条件，获取加班报酬设置信息
        /// </summary>
        /// <param name="strOwnerID">权限控制，当前记录所有者的员工序号</param>
        /// <param name="strOverTimePayType">加班报酬方式</param>
        /// <param name="strOverTimeValID">加班生效方式</param>
        /// <param name="strSortKey">排序字段</param>
        /// <param name="pageIndex">当前页索引</param>
        /// <param name="pageSize">页码大小</param>
        /// <param name="pageCount">总页数</param>
        /// <returns>员工加班报酬信息</returns>
        [OperationContract]
        public List<T_HR_OVERTIMEREWARD> GetOvertimeRewardRdListByMultSearch(string strOwnerID, string strOverTimePayType, string strOverTimeValID,
            string strSortKey, int pageIndex, int pageSize, ref int pageCount)
        {
            using (OvertimeRewardBLL bllOvertimeReward = new OvertimeRewardBLL())
            {
                var ents = bllOvertimeReward.GetOvertimeRewardRdListByMultSearch(strOwnerID, strOverTimePayType, strOverTimeValID, strSortKey, pageIndex, pageSize, ref pageCount);

                if (ents == null)
                {
                    return null;
                }


                return ents.ToList();
            }
        }

        /// <summary>
        /// 新增加班报酬设置信息
        /// </summary>
        /// <param name="entRd"></param>
        /// <returns></returns>
        [OperationContract]
        public string AddOvertimeReward(T_HR_OVERTIMEREWARD entRd)
        {
            using (OvertimeRewardBLL bllOvertimeReward = new OvertimeRewardBLL())
            {
                return bllOvertimeReward.AddOvertimeReward(entRd);
            }
        }

        /// <summary>
        /// 修改加班报酬设置信息
        /// </summary>
        /// <param name="entRd"></param>
        /// <returns></returns>
        [OperationContract]
        public string ModifyOvertimeReward(T_HR_OVERTIMEREWARD entRd)
        {

            using (OvertimeRewardBLL bllOvertimeReward = new OvertimeRewardBLL())
            {
                return bllOvertimeReward.ModifyOvertimeReward(entRd);
            }
        }

        /// <summary>
        /// 根据主键索引，删除加班报酬设置信息(注：暂定为物理删除)
        /// </summary>
        /// <param name="strOvertimeRewardId">主键索引</param>
        /// <returns></returns>
        [OperationContract]
        public string RemoveOvertimeReward(string strOvertimeRewardId)
        {
            using (OvertimeRewardBLL bllOvertimeReward = new OvertimeRewardBLL())
            {
                return bllOvertimeReward.DeleteOvertimeReward(strOvertimeRewardId);
            }
        }
        #endregion

        #region T_HR_VACATIONSET 工作日历设置服务

        /// <summary>
        /// 获取公共假期设置信息
        /// </summary>
        /// <param name="strVacationId">主键索引</param>
        /// <returns></returns>
        [OperationContract]
        public T_HR_VACATIONSET GetVacationSetByID(string strVacationId)
        {
            using (VacationSetBLL bllVacationSet = new VacationSetBLL())
            {
                return bllVacationSet.GetVacationSetByID(strVacationId);
            }
        }

        /// <summary>
        /// 根据条件，获取公共假期设置信息
        /// </summary>
        /// <param name="strVacName">假期名称</param>
        /// <param name="strVacYear">假期生效年份</param>
        /// <param name="strCountyType">假期执行国家</param>
        /// <param name="strSortKey">排序字段</param>
        /// <param name="pageIndex">当前页索引</param>
        /// <param name="pageSize">页码大小</param>
        /// <param name="pageCount">总页数</param>
        /// <returns>员工公共假期信息</returns>
        [OperationContract]
        public List<T_HR_VACATIONSET> GetVacationSetRdListByMultSearch(string strOwnerID, string strVacName, string strVacYear, string strCountyType,
            string strSortKey, int pageIndex, int pageSize, ref int pageCount)
        {
            using (VacationSetBLL bllVacationSet = new VacationSetBLL())
            {
                var ents = bllVacationSet.GetVacationSetRdListByMultSearch(strOwnerID, strVacName, strVacYear, strCountyType, strSortKey, pageIndex, pageSize, ref pageCount);


                if (ents == null)
                {
                    return null;
                }

                return ents.ToList();
            }
        }

        /// <summary>
        /// 新增公共假期设置信息
        /// </summary>
        /// <param name="entVacRd"></param>
        /// <returns></returns>
        [OperationContract]
        public string AddVacationSet(T_HR_VACATIONSET entVacRd)
        {
            using (VacationSetBLL bllVacationSet = new VacationSetBLL())
            {
                return bllVacationSet.AddVacationSet(entVacRd);
            }
        }

        /// <summary>
        /// 修改公共假期设置信息
        /// </summary>
        /// <param name="entVacRd"></param>
        /// <returns></returns>
        [OperationContract]
        public string ModifyVacationSet(T_HR_VACATIONSET entVacRd)
        {
            using (VacationSetBLL bllVacationSet = new VacationSetBLL())
            {
                return bllVacationSet.ModifyVacationSet(entVacRd);
            }
        }

        /// <summary>
        /// 根据主键索引，删除公共假期设置信息(注：暂定为物理删除)
        /// </summary>
        /// <param name="strVacationId">主键索引</param>
        /// <returns></returns>
        [OperationContract]
        public string RemoveVacationSet(string strVacationId)
        {
            using (VacationSetBLL bllVacationSet = new VacationSetBLL())
            {
                return bllVacationSet.DeleteVacationSet(strVacationId);
            }
        }

        #endregion

        #region T_HR_OUTPLANDAYS 列外日期设置服务

        /// <summary>
        /// 获取列外日期设置信息
        /// </summary>
        /// <param name="strVacationId">主键索引</param>
        /// <returns></returns>
        [OperationContract]
        public T_HR_OUTPLANDAYS GetOutPlanDaysByID(string strOutPlanDayId)
        {
            using (OutPlanDaysBLL bllOutPlanDays = new OutPlanDaysBLL())
            {
                return bllOutPlanDays.GetOutPlanDaysByID(strOutPlanDayId);
            }
        }

        /// <summary>
        /// 根据条件，获取列外日期信息
        /// </summary>
        /// <param name="strOwnerID">浏览用户的员工ID索引</param>
        /// <param name="strVacationId">关联的T_HR_VACATIONSET(工作日历)主键索引</param>
        /// <param name="strCountyType">国家/地区</param>
        /// <param name="strDayType">列外日期的类型</param>
        /// <param name="strSortKey">排序字段</param>
        /// <returns>列外日期信息</returns>
        [OperationContract]
        public List<T_HR_OUTPLANDAYS> GetAllOutPlanDaysRdListByMultSearch(string strOwnerID, string strVacationId, string strCountyType, string strDayType,
            string strSortKey)
        {
            using (OutPlanDaysBLL bllOutPlanDays = new OutPlanDaysBLL())
            {
                var ents = bllOutPlanDays.GetAllOutPlanDaysRdListByMultSearch(strOwnerID, strVacationId, strCountyType, strDayType, strSortKey);

                if (ents == null)
                {
                    return null;
                }

                return ents.ToList();
            }
        }

        /// <summary>
        /// 根据条件，获取列外日期信息,并进行分页
        /// </summary>
        /// <param name="strOwnerID">浏览用户的员工ID索引</param>
        /// <param name="strVacationId">关联的T_HR_VACATIONSET(工作日历)主键索引</param>
        /// <param name="strCountyType">国家/地区</param>
        /// <param name="strDayType">列外日期的类型</param>
        /// <param name="strSortKey">排序字段</param>
        /// <param name="pageIndex">当前页索引</param>
        /// <param name="pageSize">页码大小</param>
        /// <param name="pageCount">总页数</param>
        /// <returns>列外日期信息</returns>
        [OperationContract]
        public List<T_HR_OUTPLANDAYS> GetOutPlanDaysRdListByMultSearch(string strOwnerID, string strVacationId, string strCountyType, string strDayType,
            string strSortKey, int pageIndex, int pageSize, ref int pageCount)
        {
            using (OutPlanDaysBLL bllOutPlanDays = new OutPlanDaysBLL())
            {
                var ents = bllOutPlanDays.GetOutPlanDaysRdListByMultSearch(strOwnerID, strVacationId, strCountyType, strDayType, strSortKey, pageIndex, pageSize, ref pageCount);

                if (ents == null)
                {
                    return null;
                }

                return ents.ToList();
            }
        }

        /// <summary>
        /// 新增列外日期设置信息
        /// </summary>
        /// <param name="entVacRd"></param>
        /// <returns></returns>
        [OperationContract]
        public string AddOutPlanDays(T_HR_OUTPLANDAYS entRd)
        {
            using (OutPlanDaysBLL bllOutPlanDays = new OutPlanDaysBLL())
            {
                return bllOutPlanDays.AddOutPlanDays(entRd);
            }
        }

        /// <summary>
        /// 修改列外日期设置信息
        /// </summary>
        /// <param name="entVacRd"></param>
        /// <returns></returns>
        [OperationContract]
        public string ModifyOutPlanDays(T_HR_OUTPLANDAYS entRd)
        {
            using (OutPlanDaysBLL bllOutPlanDays = new OutPlanDaysBLL())
            {
                return bllOutPlanDays.ModifyOutPlanDays(entRd);
            }
        }

        /// <summary>
        /// 根据主键索引，删除列外日期设置信息(注：暂定为物理删除)
        /// </summary>
        /// <param name="strVacationId">主键索引</param>
        /// <returns></returns>
        [OperationContract]
        public string RemoveOutPlanDays(string strOutPlanDayId)
        {
            using (OutPlanDaysBLL bllOutPlanDays = new OutPlanDaysBLL())
            {
                return bllOutPlanDays.DeleteOutPlanDays(strOutPlanDayId);
            }
        }

        #endregion

        #region T_HR_EMPLOYEEEVECTIONREPORT 出差报告
        /// <summary>
        /// 用于实体Grid中显示数据的分页查询,获取所有的出差记录信息
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页显示条数</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件中的参数值</param>
        /// <param name="pageCount">返回总页数</param>
        /// <returns>查询结果集</returns>
        [OperationContract]
        public List<T_HR_EMPLOYEEEVECTIONREPORT> EmployeeEvectionReportPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount)
        {
            using (EmployeeEvectionReportBLL bll = new EmployeeEvectionReportBLL())
            {
                var ents = bll.EmployeeEvectionReportPaging(pageIndex, pageSize, sort, filterString, paras, ref  pageCount);

                if (ents == null)
                {
                    return null;
                }

                return ents.ToList();
            }
        }
        /// <summary>
        /// 添加出差记录信息
        /// </summary>
        /// <param name="entity">出差记录实体</param>
        [OperationContract]
        public void EmployeeEvectionReportAdd(T_HR_EMPLOYEEEVECTIONREPORT entity)
        {
            using (EmployeeEvectionReportBLL bll = new EmployeeEvectionReportBLL())
            {
                bll.EmployeeEvectionReportADD(entity);
            }
        }
        #endregion

        #region T_HR_EMPLOYEEEVECTIONRECORD 出差申请
        /// <summary>
        /// 用于实体Grid中显示数据的分页查询,获取所有的出差记录信息
        /// </summary>
        /// <param name="strOwnerID">查看人的员工ID(权限控制)</param>
        /// <param name="strEmployeeID">员工序号(唯一，GUID)</param>
        /// <param name="strDateFrom">出差起始日期</param>
        /// <param name="strDateTo">出差截止日期</param>
        /// <param name="strSortKey">排序字段</param>
        /// <param name="pageIndex">当前页码</param>
        /// <param name="pageSize">每页显示记录数</param>
        /// <param name="pageCount">总页码</param>
        /// <returns>查询结果集</returns>
        [OperationContract]
        public List<T_HR_EMPLOYEEEVECTIONRECORD> EmployeeEvectionRecordPaging(string strOwnerID, string strEmployeeID, string strDateFrom,
            string strDateTo, string strSortKey, int pageIndex, int pageSize, ref int pageCount)
        {
            using (EmployeeEvectionRecordBLL bll = new EmployeeEvectionRecordBLL())
            {
                var ents = bll.EmployeeEvectionRecordPaging(strOwnerID, strEmployeeID, strDateFrom, strDateTo, strSortKey, pageIndex, pageSize, ref pageCount);

                if (ents == null)
                {
                    return null;
                }

                return ents.ToList();
            }
        }
        /// <summary>
        /// 添加出差记录信息
        /// </summary>
        /// <param name="entity">出差记录实体</param>
        [OperationContract]
        public void EmployeeEvectionRecordAdd(T_HR_EMPLOYEEEVECTIONRECORD entity)
        {
            using (EmployeeEvectionRecordBLL bll = new EmployeeEvectionRecordBLL())
            {
                bll.EmployeeEvectionRecordADD(entity);
            }
        }

        /// <summary>
        /// 添加出差记录信息,出差申请，出差报销终审时调用
        /// </summary>
        /// <param name="entity">出差记录实体</param>
        [OperationContract]
        public void AddEmployeeEvectionRdList(List<T_HR_EMPLOYEEEVECTIONRECORD> entTempList)
        {
            using (EmployeeEvectionRecordBLL bll = new EmployeeEvectionRecordBLL())
            {
                bll.AddEvectionRdList(entTempList);
            }
        }

        /// <summary>
        /// 修改出差记录信息
        /// </summary>
        /// <param name="entity">出差记录实体</param>
        [OperationContract]
        public void EmployeeEvectionRecordUpdate(T_HR_EMPLOYEEEVECTIONRECORD entity)
        {
            using (EmployeeEvectionRecordBLL bll = new EmployeeEvectionRecordBLL())
            {
                bll.EmployeeEvectionRecordUpdate(entity);
            }
        }
        /// <summary>
        /// 删除出差记录信息
        /// </summary>
        /// <param name="evectionRecordIDs">出差记录ID组</param>
        /// <returns>返回受影响的行数</returns>
        [OperationContract]
        public bool EmployeeEvectionRecordDelete(string[] evectionRecordIDs)
        {
            using (EmployeeEvectionRecordBLL bll = new EmployeeEvectionRecordBLL())
            {
                int rslt = bll.EmployeeEvectionRecordDelete(evectionRecordIDs);
                return (rslt > 0);
            }
        }
        /// <summary>
        /// 根据出差记录ID获到出差记录信息
        /// </summary>
        /// <param name="strID">出差记录ID</param>
        /// <returns>返回出差记录实体</returns>
        [OperationContract]
        public T_HR_EMPLOYEEEVECTIONRECORD GetEmployeeEvectionRecordByID(string strID)
        {
            using (EmployeeEvectionRecordBLL bll = new EmployeeEvectionRecordBLL())
            {
                return bll.GetEmployeeEvectionRecordByID(strID);
            }
        }
        #endregion

        #region T_HR_ATTENDANCERECORD 考勤记录表
        /// <summary>
        /// 根据员工ID和日期获取班次定义的前两段起止时间
        /// </summary>
        /// <param name="employeeID">员工ID</param>
        /// <param name="date">日期</param>
        /// <returns></returns>
        [OperationContract]
        public List<V_ATTENDANCERECORD> GetAttendanceRecordByEmployeeID(string employeeID, string date)
        {
            using (AttendanceRecordBLL bll = new AttendanceRecordBLL())
            {
                return bll.GetAttendanceRecordByEmployeeID(employeeID, date);
            }
        }

        /// <summary>
        /// 获取员工作息时间记录信息
        /// </summary>
        /// <param name="sType">查看的员工范围对象类型</param>
        /// <param name="sValue">查看的员工范围对象的ID</param>
        /// <param name="strOwnerID">查看人的员工ID(权限控制)</param>
        /// <param name="strEmployeeID">员工序号(唯一，GUID)</param>
        /// <param name="strAttendDateFrom">作息时间搜寻起始日期</param>
        /// <param name="strAttendDateTo">作息时间搜寻截止日期</param>
        /// <param name="strSortKey">排序字段</param>
        /// <param name="pageIndex">当前页码</param>
        /// <param name="pageSize">每页显示记录数</param>
        /// <param name="pageCount">总页码</param>
        /// <returns>返回员工作息时间记录列表</returns>
        [OperationContract]
        public List<T_HR_ATTENDANCERECORD> GetAttendanceRdListByMultSearch(string sType, string sValue, string strOwnerID, string strEmployeeID, string strAttendDateFrom,
            string strAttendDateTo, string strSortKey, int pageIndex, int pageSize, ref int pageCount)
        {
            using (AttendanceRecordBLL bll = new AttendanceRecordBLL())
            {
                var ents = bll.GetAttendanceRdListByMultSearch(sType, sValue, strOwnerID, strEmployeeID, strAttendDateFrom, strAttendDateTo, strSortKey, pageIndex, pageSize, ref pageCount);

                if (ents == null)
                {
                    return null;
                }

                return ents.ToList();
            }
        }

        /// <summary>
        /// 根据主键索引，删除员工作息时间记录信息(注：暂定为物理删除)
        /// </summary>
        /// <param name="strAttendRdId">主键索引</param>
        /// <returns></returns>
        [OperationContract]
        public string RemoveAttendanceRecord(string strAttendRdId)
        {

            using (AttendanceRecordBLL bll = new AttendanceRecordBLL())
            {
                return bll.DeleteAttRd(strAttendRdId);
            }
        }
        #endregion

        #region T_HR_EMPLOYEELEAVERECORD 员工请假调休记录服务
        /// <summary>
        /// 根据请假记录ID获取对应调休假
        /// </summary>
        /// <param name="strLeaveRecordID">请假记录ID</param>
        /// <returns>查询结果集</returns>
        [OperationContract]
        public List<V_ADJUSTLEAVE> GetAdjustLeaveDetailListByLeaveRecordID(string strLeaveRecordID)
        {
            using (AdjustLeaveBLL bll = new AdjustLeaveBLL())
            {
                return bll.GetAdjustLeaveDetailListByLeaveRecordID(strLeaveRecordID);
            }
        }
        #endregion

        #region T_HR_EMPLOYEELEAVERECORD 员工请假记录服务
        /// <summary>
        /// 根据请假记录ID获取员工请假信息
        /// </summary>
        /// <param name="strID"></param>
        /// <returns></returns>
        [OperationContract]
        public T_HR_EMPLOYEELEAVERECORD GetLeaveRecordByID(string strID)
        {
            using (EmployeeLeaveRecordBLL bll = new EmployeeLeaveRecordBLL())
            {
                return bll.GetLeaveRecordByID(strID);
            }
        }

        /// <summary>
        /// 用于实体Grid中显示数据的分页查询
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页显示条数</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件中的参数值</param>
        /// <param name="pageCount">返回总页数</param>
        /// <returns>查询结果集</returns>
        [OperationContract]
        public List<V_EmpLeaveRdInfo> EmployeeLeaveRecordPaging(int pageIndex, int pageSize, string sort, string filterString, List<object> paras, ref int pageCount, string strCheckState, string strOwnerID, string recorderDate)
        {
            using (EmployeeLeaveRecordBLL bll = new EmployeeLeaveRecordBLL())
            {
                var ents = bll.EmployeeLeaveRecordPaging(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, strCheckState, strOwnerID, recorderDate);

                if (ents == null)
                {
                    return null;
                }

                return ents.ToList();
            }
        }

        /// <summary>
        /// 用于实体Grid中显示数据的分页查询
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页显示条数</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件中的参数值</param>
        /// <param name="pageCount">返回总页数</param>
        /// <param name="strCheckState">审核状态</param>
        /// <param name="strOwnerID">所有者</param>
        /// <param name="startDate">开始日期</param>
        /// <param name="recorderDate">截止日期</param>
        /// <returns>查询结果集</returns>
        [OperationContract]
        public List<V_EmpLeaveRdInfo> EmployeeLeaveRecordPaged(int pageIndex, int pageSize, string sort, string filterString, List<object> paras, ref int pageCount, string strCheckState, string strOwnerID, string startDate, string recorderDate,string employeeID, string leaveTypeSetID)
        {
            using (EmployeeLeaveRecordBLL bll = new EmployeeLeaveRecordBLL())
            {
                var ents = bll.EmployeeLeaveRecordPaged(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, strCheckState, strOwnerID, startDate, recorderDate,employeeID,leaveTypeSetID);

                if (ents == null)
                {
                    return null;
                }

                return ents.ToList();
            }
        }
        /// <summary>
        /// 导出考勤报表
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页显示条数</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件中的参数值</param>
        /// <param name="pageCount">返回总页数</param>
        /// <param name="strCheckState">审核状态</param>
        /// <param name="strOwnerID">所有者</param>
        /// <param name="startDate">开始日期</param>
        /// <param name="recorderDate">截止日期</param>
        /// <returns>查询结果集</returns>
        [OperationContract]
        public byte[] ExportEmployeeLeaveRecordReports(int pageIndex, int pageSize, string sort, string filterString, List<object> paras, ref int pageCount, string strCheckState, string strOwnerID, string startDate, string recorderDate,string employeeID,string leaveTypeSetID)
        {
            using (EmployeeLeaveRecordBLL bll = new EmployeeLeaveRecordBLL())
            {
                return bll.ExportEmployeeLeaveRecordReports(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, strCheckState, strOwnerID, startDate, recorderDate,employeeID,leaveTypeSetID);

            }
        }
        /// <summary>
        /// 获取指定员工的实际请假天数(实际请假天数=请假天数-公休假天数-每周休息天数)，实际请假时长(按小时计，实际请假合计时长=非整天请假时长-当日作息间隙休息时间+整天请假时长)
        /// </summary>
        /// <param name="strLeaveRecordId">当前请假记录的ID</param>
        /// <param name="strEmployeeID">员工ID</param>
        /// <param name="dtLeaveStartTime">请假起始时间</param>
        /// <param name="dtLeaveEndTime">请假截止时间</param>
        /// <param name="dLeaveDay">实际请假天数</param>
        /// <param name="dLeaveTime">实际请假时长</param>
        /// <param name="dLeaveTotalTime">实际请假合计时长</param>
        [OperationContract]
        public string GetRealLeaveDayByEmployeeIdAndDate(string strLeaveRecordId, string strEmployeeID, DateTime dtLeaveStartTime,
            DateTime dtLeaveEndTime, ref decimal dLeaveDay, ref decimal dLeaveTime, ref decimal dLeaveTotalTime)
        {
            using (EmployeeLeaveRecordBLL bll = new EmployeeLeaveRecordBLL())
            {
                return bll.GetRealLeaveDayByEmployeeIdAndDate(strLeaveRecordId, strEmployeeID, dtLeaveStartTime, dtLeaveEndTime, ref dLeaveDay, ref dLeaveTime, ref dLeaveTotalTime);
            }
        }

        /// <summary>
        /// 添加请假记录和请假调休记录
        /// </summary>
        /// <param name="LeaveRecord">请假记录实体</param>
        /// <param name="AdjustLeave">请假调休记录实体</param>
        [OperationContract]
        public void EmployeeLeaveRecordAdd(T_HR_EMPLOYEELEAVERECORD LeaveRecord, List<V_ADJUSTLEAVE> AdjustLeaves)
        {
            using (EmployeeLeaveRecordBLL bll = new EmployeeLeaveRecordBLL())
            {
                bll.EmployeeLeaveRecordAdd(LeaveRecord, AdjustLeaves);
            }
        }

        /// <summary>
        /// 修改请假记录和请假调休记录
        /// </summary>
        /// <param name="LeaveRecord"></param>
        /// <param name="AdjustLeave"></param>
        [OperationContract]
        public void EmployeeLeaveRecordUpdate(T_HR_EMPLOYEELEAVERECORD LeaveRecord, List<V_ADJUSTLEAVE> AdjustLeaves)
        {
            using (EmployeeLeaveRecordBLL bll = new EmployeeLeaveRecordBLL())
            {
                bll.EmployeeLeaveRecordUpdate(LeaveRecord, AdjustLeaves);
            }
        }

        /// <summary>
        /// 删除请假记录组
        /// </summary>
        /// <param name="leaveRecordIDs">请假记录ID组</param>
        /// <returns></returns>
        [OperationContract]
        public bool EmployeeLeaveRecordDelete(string[] leaveRecordIDs)
        {
            using (EmployeeLeaveRecordBLL bll = new EmployeeLeaveRecordBLL())
            {
                int rslt = bll.EmployeeLeaveRecordDelete(leaveRecordIDs);
                return (rslt > 0);
            }
        }
        /// <summary>
        /// 根据请假记录ID、员工ID获取信息
        /// </summary>
        /// <param name="strID"></param>
        /// <param name="employeeID"></param>
        /// <returns></returns>
        [OperationContract]
        public V_EMPLOYEELEAVERECORD GetEmployeeLeaveRecordByID(string strID)
        {
            using (EmployeeLeaveRecordBLL bll = new EmployeeLeaveRecordBLL())
            {
                return bll.GetEmployeeLeaveRecordByID(strID);
            }
        }

        /// <summary>
        /// 审核请假申请
        /// </summary>
        /// <param name="strLeaveRecordID">请假申请主键ID</param>
        /// <param name="AdjustLeaves">低假记录</param>
        /// <param name="strCheckState">审核状态</param>
        /// <returns>返回处理消息</returns>
        [OperationContract]
        public string AuditLeaveRecord(string strLeaveRecordID, List<V_ADJUSTLEAVE> AdjustLeaves, string strCheckState)
        {
            using (EmployeeLeaveRecordBLL bll = new EmployeeLeaveRecordBLL())
            {
                return bll.AuditLeaveRecord(strLeaveRecordID, AdjustLeaves, strCheckState);
            }
        }

        /// <summary>
        /// 获取同类假期年度总次数，年度总时长，月度总次数，月度总次数，月度总时长
        /// </summary>
        /// <param name="strLeaveTypeSetId">假期标准Id</param>
        /// <param name="strLeaveRecordId">请假记录Id</param>
        /// <param name="strEmployeeID">员工Id</param>
        /// <param name="dtLeaveStartTime">请假起始时间</param>
        /// <param name="dtLeaveEndTime">请假截止时间</param>
        /// <param name="dLeaveYearTimes">同类假期年度总次数</param>
        /// <param name="dLeaveYearDays">同类假期年度总时长</param>
        /// <param name="dLeaveMonthTimes">同类假期月度总次数</param>
        /// <param name="dLeaveMonthDays">同类假期月度总时长</param>
        [OperationContract]
        public void GetLeaveDaysHistory(string strLeaveTypeSetId, string strLeaveRecordId, string strEmployeeID,
            DateTime dtLeaveStartTime, DateTime dtLeaveEndTime, ref decimal dLeaveYearTimes,
            ref decimal dLeaveYearDays, ref decimal dLeaveMonthTimes, ref decimal dLeaveMonthDays, ref DateTime dLeaveFistDate, ref decimal dLeaveSYearTimes)
        {
            using (EmployeeLeaveRecordBLL bll = new EmployeeLeaveRecordBLL())
            {
                bll.GetLeaveDaysHistory(strLeaveTypeSetId, strLeaveRecordId, strEmployeeID, dtLeaveStartTime, dtLeaveEndTime, ref dLeaveYearTimes,
                    ref dLeaveYearDays, ref dLeaveMonthTimes, ref dLeaveMonthDays,ref dLeaveFistDate,ref dLeaveSYearTimes);
            }
        }
        #endregion

        #region T_HR_EMPLOYEESIGNINRECORD 员工签卡记录

        /// <summary>
        /// 用于实体Grid中显示数据的分页查询
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页显示条数</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件中的参数值</param>
        /// <param name="pageCount">返回总页数</param>
        /// <returns>查询结果集</returns>
        [OperationContract]
        public List<T_HR_EMPLOYEESIGNINRECORD> EmployeeSignInRecordPaging(int pageIndex, int pageSize, string sort, string filterString, List<object> paras, ref int pageCount, string strCheckState, string strOwnerID, string recorderDate)
        {
            using (EmployeeSignInRecordBLL bll = new EmployeeSignInRecordBLL())
            {
                var ents = bll.EmployeeSignInRecordPaging(pageIndex, pageSize, sort, filterString, paras, ref pageCount, strCheckState, strOwnerID, recorderDate);

                if (ents == null)
                {
                    return null;
                }

                return ents.ToList();
            }
        }
        /// <summary>
        /// 添加签卡记录信息
        /// </summary>
        /// <param name="entity">签卡记录实体</param>
        /// <param name="entityList">异常信息实体</param>
        [OperationContract]
        public void EmployeeSignInRecordAdd(T_HR_EMPLOYEESIGNINRECORD entity, List<T_HR_EMPLOYEESIGNINDETAIL> entityList)
        {
            using (EmployeeSignInRecordBLL bll = new EmployeeSignInRecordBLL())
            {
                bll.EmployeeSignInRecordAdd(entity, entityList);
            }
        }
        /// <summary>
        /// 修改签卡记录信息
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="entityList"></param>
        [OperationContract]
        public void EmployeeSigninRecordUpdate(T_HR_EMPLOYEESIGNINRECORD entity, List<T_HR_EMPLOYEESIGNINDETAIL> entityList)
        {
            using (EmployeeSignInRecordBLL bll = new EmployeeSignInRecordBLL())
            {
                bll.EmployeeSigninRecordUpdate(entity, entityList);
            }
        }
        /// <summary>
        /// 根据ID获取签卡记录信息
        /// </summary>
        /// <param name="strid">记录信息ID</param>
        /// <returns></returns>
        [OperationContract]
        public T_HR_EMPLOYEESIGNINRECORD GetEmployeeSigninRecordByID(string strid)
        {
            using (EmployeeSignInRecordBLL bll = new EmployeeSignInRecordBLL())
            {
                return bll.GetEmployeeSigninRecordByID(strid);
            }
        }
        /// <summary>
        /// 删除签卡记录组
        /// </summary>
        /// <param name="leaveRecordIDs">签卡记录ID组</param>
        /// <returns>返回受影响的行数</returns>
        [OperationContract]
        public bool EmployeeSigninRecordDelete(string[] leaveRecordIDs)
        {
            using (EmployeeSignInRecordBLL bll = new EmployeeSignInRecordBLL())
            {
                int rslt = bll.EmployeeSigninRecordDelete(leaveRecordIDs);
                return (rslt > 0);
            }
        }

        /// <summary>
        /// 审核签卡记录信息
        /// </summary>
        /// <param name="strSignInID"></param>
        /// <param name="strCheckState"></param>
        [OperationContract]
        public string EmployeeSigninRecordAudit(string strSignInID, string strCheckState)
        {
            using (EmployeeSignInRecordBLL bll = new EmployeeSignInRecordBLL())
            {
                return bll.EmployeeSigninRecordAudit(strSignInID, strCheckState);
            }
        }
        #endregion

        #region T_HR_EMPLOYEEABNORMRECORD 员工异常记录表
        /// <summary>
        /// 根据员工ID获取该员工所有未签卡记录
        /// </summary>
        /// <param name="EmployeeID">员工ID</param>
        /// <returns></returns>
        [OperationContract]
        public List<T_HR_EMPLOYEEABNORMRECORD> GetAbnormRecordByEmployeeID(string EmployeeID)
        {
            using (AbnormRecordBLL bll = new AbnormRecordBLL())
            {
                var ents = bll.GetAbnormRecordByEmployeeID(EmployeeID);
                if (ents == null)
                {
                    return null;
                }

                return ents.ToList();
            }
        }

        /// <summary>
        /// 获取员工异常记录信息
        /// </summary>
        /// <param name="strOwnerID">权限控制，当前记录所有者的员工序号</param>
        /// <param name="strEmployeeID">异常记录对应关联的员工序号</param>
        /// <param name="strSignInState">签卡状态(参数为"1"，则取未签卡的异常；参数为"2"，则取对应已签卡的异常)</param>
        /// <param name="strCurDateMonth">当前日期(年-月)</param>
        /// <param name="strSortKey">排序字段</param>
        /// <param name="pageIndex">当前页索引</param>
        /// <param name="pageSize">页码大小</param>
        /// <param name="pageCount">总页数</param>
        /// <returns>员工异常记录信息</returns>
        [OperationContract]
        public List<T_HR_EMPLOYEEABNORMRECORD> GetAbnormRecordRdListByMultSearch(string strOwnerID, string strEmployeeID, string strSignInState, string strCurStartDate, string strCurEndDate,
            string strSortKey, int pageIndex, int pageSize, ref int pageCount)
        {
            using (AbnormRecordBLL bll = new AbnormRecordBLL())
            {
                var ents = bll.GetAbnormRecordRdListByMultSearch(strOwnerID, strEmployeeID, strSignInState, strCurStartDate, strCurEndDate, strSortKey, pageIndex, pageSize, ref pageCount).ToList();
                if (ents == null)
                {
                    return null;
                }

                return ents.ToList();
            }
        }


        /// <summary>
        /// 考勤异常提醒消息
        /// </summary>
        /// <param name="strEmployeeId"></param>
        [OperationContract]
        public void AbnormRecordCheckAlarm(string strEmployeeId)
        {
            using (AbnormRecordBLL bll = new AbnormRecordBLL())
            {
                bll.AbnormRecordCheckAlarm(strEmployeeId);
            }
        }
        /// <summary>
        /// 导出员工签卡明细
        /// </summary>
        /// <param name="signinID">签卡单ID</param>
        /// <returns></returns>
        [OperationContract]
        public byte[] ExportEmployeeSignIn(string signinID)
        {
            using (EmployeeSignInDetailBLL bll = new EmployeeSignInDetailBLL())
            {
                return bll.ExportEmployeeSignIn(signinID);
            }
        }
        #endregion

        #region T_HR_EMPLOYEESIGNINDETAIL 员工签卡记录子表

        /// <summary>
        /// 根据员工签卡记录ID获取签卡的异常信息记录
        /// </summary>
        /// <param name="signinID">签卡记录ID</param>
        /// <returns></returns>
        [OperationContract]
        public List<T_HR_EMPLOYEESIGNINDETAIL> GetEmployeeSignInDetailBySigninID(string signinID)
        {
            using (EmployeeSignInDetailBLL bll = new EmployeeSignInDetailBLL())
            {
                var ents = bll.GetEmployeeSignInDetailBySigninID(signinID);

                if (ents == null)
                {
                    return null;
                }

                return ents.ToList();
            }
        }

        /// <summary>
        /// 新增员工异常记录信息
        /// </summary>
        /// <param name="entTemp"></param>
        /// <returns></returns>
        [OperationContract]
        public string AddEmployeesignindetail(T_HR_EMPLOYEESIGNINDETAIL entTemp)
        {
            using (EmployeeSignInDetailBLL bllEmployeeSignInDetail = new EmployeeSignInDetailBLL())
            {
                return bllEmployeeSignInDetail.AddEmployeeSignInDetail(entTemp);
            }
        }

        /// <summary>
        /// 修改员工异常记录信息
        /// </summary>
        /// <param name="entTemp"></param>
        /// <returns></returns>
        [OperationContract]
        public string ModifyEmployeesignindetail(T_HR_EMPLOYEESIGNINDETAIL entTemp)
        {
            using (EmployeeSignInDetailBLL bllEmployeeSignInDetail = new EmployeeSignInDetailBLL())
            {
                return bllEmployeeSignInDetail.ModifyEmployeeSignInDetail(entTemp);
            }
        }
        #endregion

        #region T_HR_EMPLOYEELEVELDAYCOUNT 员工可休假服务


        /// <summary>
        /// 获取员工当前请假时段内可用冲减天数
        /// </summary>
        /// <param name="strEmployeeID">员工ID</param>
        /// <param name="strLeaveRecordId">当前请假记录ID</param>
        /// <param name="strLeaveSetId">当前请假对应的请假标准ID</param>
        /// <param name="strLeaveFineType">请假标准的扣款方式</param>
        /// <param name="dtStartDate">请假起始时间</param>
        /// <param name="dtEndDate">请假结束时间</param>
        /// <param name="dCurLevelDays">当前请假时段内可用冲减天数</param>        
        [OperationContract]
        public void GetCurLevelDaysByEmployeeIDAndLeaveFineType(string strEmployeeID, string strLeaveRecordId, string strLeaveSetId,
            DateTime dtStartDate, DateTime dtEndDate, ref decimal dCurLevelDays)
        {
            using (EmployeeLevelDayCountBLL bllEmployeeleveldaycount = new EmployeeLevelDayCountBLL())
            {
                dCurLevelDays = bllEmployeeleveldaycount.GetCurLevelDaysByEmployeeIDAndLeaveFineType(strEmployeeID, strLeaveRecordId, strLeaveSetId, dtStartDate, dtEndDate);
            }
        }

        /// <summary>
        /// 获取员工可休假信息
        /// </summary>
        /// <param name="strOwnerID">权限控制人的员工ID</param>
        /// <param name="strEmployeeID">员工序号</param>        
        /// <param name="strSortKey">排序字段</param>
        /// <param name="pageIndex">当前页码</param>
        /// <param name="pageSize">每页显示记录数</param>
        /// <param name="pageCount">总页码</param>
        /// <returns>返回员工可休假信息</returns>
        [OperationContract]
        public List<T_HR_EMPLOYEELEVELDAYCOUNT> GetEmployeeLevelDayCountRdListByMultSearch(string sOrgType, string sValue, string strLeaveType, string strOwnerID, string strEmployeeID, string strEfficDateFrom,
            string strEfficDateTo, string strSortKey, int pageIndex, int pageSize, ref int pageCount)
        {
            using (EmployeeLevelDayCountBLL bllEmployeeleveldaycount = new EmployeeLevelDayCountBLL())
            {
                var ents = bllEmployeeleveldaycount.GetEmployeeLevelDayCountRdListByMultSearch(sOrgType, sValue, strLeaveType, strOwnerID, strEmployeeID, strEfficDateFrom, strEfficDateTo, strSortKey, pageIndex, pageSize, ref pageCount).ToList();

                if (ents == null)
                {
                    return null;
                }

                return ents.ToList();
            }
        }

        [OperationContract]
        public List<T_HR_EMPLOYEELEAVERECORD> GetEmployeeleaverecordByMultSearchId(string employeeid, string leavetypesetid, string OWNERCOMPANYID,
           string strEfficDateFrom, string strEfficDateTo, int pageIndex, int pageSize, ref int pageCount) 
        {
            using (EmployeeLevelDayCountBLL bllEmployeeleveldaycount = new EmployeeLevelDayCountBLL())
            {
                var ents = bllEmployeeleveldaycount.GetEmployeeleaverecordByMultSearchId(employeeid, leavetypesetid, OWNERCOMPANYID, strEfficDateFrom, strEfficDateTo,pageIndex, pageSize, ref pageCount);

                if (ents == null)
                {
                    return null;
                }
                return ents.ToList();
            }
        }

        /// <summary>
        /// 根据时间条件，获取员工可休假信息,并进行分页
        /// </summary>
        /// <param name="strOwnerID">权限控制人的员工ID</param>
        /// <param name="strEmployeeID">员工ID</param>
        /// <param name="strSortKey">排序字段</param>
        /// <param name="pageIndex">当前页索引</param>
        /// <param name="pageSize">页码大小</param>
        /// <param name="pageCount">总页数</param>
        /// <returns>员工可休假信息</returns>
        [OperationContract]
        public List<T_HR_EMPLOYEELEVELDAYCOUNT> GetEmployeeLevelDayCountRdListByMultSearchByTime(string strOwnerID, string strEmployeeID,
            string strSortKey, int pageIndex, int pageSize, ref int pageCount, DateTime startTime, DateTime endTime)
        {
            using (EmployeeLevelDayCountBLL bllEmployeeleveldaycount = new EmployeeLevelDayCountBLL())
            {
                var ents = bllEmployeeleveldaycount.GetEmployeeLevelDayCountRdListByMultSearch(strOwnerID, strEmployeeID, strSortKey, pageIndex, pageSize, ref pageCount, startTime, endTime).ToList();

                if (ents == null)
                {
                    return null;
                }

                return ents.ToList();
            }
        }

        /// <summary>
        /// 新增员工可休假信息
        /// </summary>
        /// <param name="entTemp"></param>
        /// <returns></returns>
        [OperationContract]
        public string AddEmployeeLevelDayCount(T_HR_EMPLOYEELEVELDAYCOUNT entTemp)
        {
            using (EmployeeLevelDayCountBLL bllEmployeeleveldaycount = new EmployeeLevelDayCountBLL())
            {
                return bllEmployeeleveldaycount.AddEmployeeLevelDayCount(entTemp);
            }
        }

        /// <summary>
        /// 用于实体Grid中显示数据的分页查询  查询带薪假期明细
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页显示条数</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件中的参数值</param>
        /// <param name="pageCount">返回总页数</param>
        /// <returns>查询结果集</returns>
        [OperationContract]
        public List<T_HR_EMPLOYEELEVELDAYDETAILS> EmployeeLeaveDetailPaging(int pageIndex, int pageSize, string sort, string filterString, string[] paras, ref int pageCount)
        {
            using (EmployeeLevelDayCountBLL bllEmployeeleveldaycount = new EmployeeLevelDayCountBLL())
            {
                return bllEmployeeleveldaycount.EmployeeLeaveDetailPaging(pageIndex, pageSize, sort, filterString, paras, ref pageCount).ToList();
            }
        }

        /// <summary>
        /// 根据员工ID获取此员工当前指定休假类型可休假天数
        /// </summary>
        /// <param name="employeeID">员工ID</param>
        /// <param name="strVacType">休假类型</param>
        /// <param name="currDate">查询的日期</param>
        /// <returns></returns>
        [OperationContract]
        public T_HR_EMPLOYEELEVELDAYCOUNT GetCurLevelDayCountByEmployeeID(string employeeID, string strVacType, DateTime currDate)
        {
            using (EmployeeLevelDayCountBLL bllEmployeeleveldaycount = new EmployeeLevelDayCountBLL())
            {
                return bllEmployeeleveldaycount.GetCurLevelDayCountByEmployeeID(employeeID, strVacType, currDate);
            }
        }

        /// <summary>
        /// 对指定员工应用考勤方案，生成其可休假记录，如存在，则重置
        /// </summary>
        /// <param name="entEmployee"></param>
        /// <param name="entTemp"></param>
        [OperationContract]
        public void CreateLevelDayCountByAsignAttSol(string strAttendanceSolutionAsignId)
        {
            using (EmployeeLevelDayCountBLL bllEmployeeleveldaycount = new EmployeeLevelDayCountBLL())
            {
                bllEmployeeleveldaycount.CreateLevelDayCountByAsignAttSol(strAttendanceSolutionAsignId);
            }
        }

        /// <summary>
        /// 对所有公司生成其可休假记录
        /// </summary>
        [OperationContract]
        public void CreateLevelDayCountWithAll()
        {
            using (EmployeeLevelDayCountBLL bllEmployeeleveldaycount = new EmployeeLevelDayCountBLL())
            {
                bllEmployeeleveldaycount.CreateLevelDayCountWithAllCompany();
            }
        }
        #endregion

        #region T_HR_ATTENDMONTHLYBALANCE 员工考勤月度结算服务

        /// <summary>
        /// 获取信息
        /// </summary>
        /// <param name="strMonthlyBalanceId">主键索引</param>
        /// <returns></returns>
        [OperationContract]
        public T_HR_ATTENDMONTHLYBALANCE GetAttendMonthlyBalanceByID(string strMonthlyBalanceId)
        {
            using (AttendMonthlyBalanceBLL bllAttendMonthlyBalance = new AttendMonthlyBalanceBLL())
            {
                return bllAttendMonthlyBalance.GetAttendMonthlyBalanceByID(strMonthlyBalanceId);
            }
        }

        /// <summary>
        /// 根据条件，获取要进行审批的员工考勤月度结算信息,并进行分页
        /// </summary>
        /// <param name="sType">查看的员工范围对象类型</param>
        /// <param name="sValue">查看的员工范围对象的ID</param>
        /// <param name="strOwnerID">权限控制所有人的员工序号</param>
        /// <param name="strCheckState">审核状态</param>
        /// <param name="dBalanceYear">结算年份</param>
        /// <param name="dBalanceMonth">结算月份</param>
        /// <param name="strSortKey">排序字段</param>
        /// <param name="pageIndex">当前页索引</param>
        /// <param name="pageSize">页码大小</param>
        /// <param name="pageCount">总页数</param>
        /// <returns>员工考勤月度结算信息</returns>
        [OperationContract]
        public List<T_HR_ATTENDMONTHLYBALANCE> GetAttendMonthlyBalanceRdListForAudit(string sType, string sValue, string strOwnerID, string strCheckState,
            decimal dBalanceYear, decimal dBalanceMonth, string strSortKey, int pageIndex, int pageSize, ref int pageCount)
        {
            using (AttendMonthlyBalanceBLL bllAttendMonthlyBalance = new AttendMonthlyBalanceBLL())
            {
                var ents = bllAttendMonthlyBalance.GetAttendMonthlyBalanceRdListForAudit(sType, sValue, strOwnerID, strCheckState, dBalanceYear, dBalanceMonth, strSortKey, pageIndex, pageSize, ref pageCount);

                if (ents == null)
                {
                    return null;
                }

                return ents.ToList();
            }
        }

        /// <summary>
        /// 获取员工考勤月度结算信息
        /// </summary>
        /// <param name="sType">查看的员工范围对象类型</param>
        /// <param name="sValue">查看的员工范围对象的ID</param>
        /// <param name="strOwnerID">查看人的员工ID(权限控制)</param>
        /// <param name="strCheckState">审核状态</param>
        /// <param name="strEmployeeID">员工ID(查询时使用)</param>
        /// <param name="dBalanceYear">结算年份</param>
        /// <param name="dBalanceMonth">结算月份</param>
        /// <param name="strSortKey">排序字段</param>
        /// <param name="pageIndex">当前页码</param>
        /// <param name="pageSize">每页显示记录数</param>
        /// <param name="pageCount">总页码</param>
        /// <returns>返回员工考勤月度结算信息</returns>
        [OperationContract]
        public List<T_HR_ATTENDMONTHLYBALANCE> GetAttendMonthlyBalanceListByMultSearch(string sType, string sValue, string strOwnerID, string strCheckState, string strEmployeeID,
            decimal dBalanceYear, decimal dBalanceMonth, string strSortKey, int pageIndex, int pageSize, ref int pageCount)
        {
            using (AttendMonthlyBalanceBLL bllAttendMonthlyBalance = new AttendMonthlyBalanceBLL())
            {
                var ents = bllAttendMonthlyBalance.GetAttendMonthlyBalanceRdListByMultSearch(sType, sValue, strOwnerID, strCheckState, strEmployeeID, dBalanceYear, dBalanceMonth, strSortKey, pageIndex, pageSize, ref pageCount).ToList();

                if (ents == null)
                {
                    return null;
                }

                return ents.ToList();
            }
        }

        

        /// <summary>
        /// 导出考勤报表
        /// </summary>
        /// <param name="sType"></param>
        /// <param name="sValue"></param>
        /// <param name="strOwnerID"></param>
        /// <param name="strCheckState"></param>
        /// <param name="strEmployeeID"></param>
        /// <param name="dBalanceYear"></param>
        /// <param name="dBalanceMonth"></param>
        /// <param name="strSortKey"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageCount"></param>
        /// <returns></returns>
        [OperationContract]
        public byte[] ExportAttendMonthlyBalanceRdListReports(string sType, string sValue, string strOwnerID, string strCheckState, string strEmployeeID, decimal dBalanceYear, decimal dBalanceMonth, string strSortKey)
        {
            using (AttendMonthlyBalanceBLL bllAttendMonthlyBalance = new AttendMonthlyBalanceBLL())
            {
                return  bllAttendMonthlyBalance.ExportAttendMonthlyBalanceReports(sType, sValue, strOwnerID, strCheckState, strEmployeeID, dBalanceYear, dBalanceMonth, strSortKey);
                                
            }
        }

        /// <summary>
        /// 导入CSV的员工考勤结算信息
        /// </summary>
        /// <param name="UploadFile">上传载体</param>
        /// <param name="strCreateUserID">导入有效数据起始日期</param>
        /// <param name="strUnitType">导入有效数据起始日期</param>
        /// <param name="strUnitObjectId">导入有效数据起始日期</param>
        /// <param name="dtStart">导入有效数据起始日期</param>
        /// <param name="dtEnd">导入有效数据截止日期</param>
        /// <param name="strMsg">处理消息</param>
        [OperationContract]
        public void ImportAttendMonthlyBalanceFromCSV(UploadFileModel UploadFile, string strCreateUserID, string strUnitType, string strUnitObjectId, decimal dBalanceYear, decimal dBalanceMonth, ref string strMsg)
        {
            string strPath = string.Empty;
            SaveFile(UploadFile, out strPath);
            string strPhysicalPath = HttpContext.Current.Server.MapPath(strPath);

            using (AttendMonthlyBalanceBLL bllAttendMonthlyBalance = new AttendMonthlyBalanceBLL())
            {
                bllAttendMonthlyBalance.ImportMonthlyBalance(strCreateUserID, strPhysicalPath, strUnitType, strUnitObjectId, dBalanceYear, dBalanceMonth, ref strMsg);
            }
        }

        /// <summary>
        /// 导入Excel,并绑定DtGrid
        /// </summary>
        /// <param name="UploadFile"></param>
        /// <returns></returns>
        [OperationContract]
        public List<T_HR_ATTENDMONTHLYBALANCE> ImportAttendMonthlyBalanceForShow(UploadFileModel UploadFile)
        {
            string strPath = string.Empty;
            SaveFile(UploadFile, out strPath);
            string strPhysicalPath = HttpContext.Current.Server.MapPath(strPath);
            using (AttendMonthlyBalanceBLL bllAttendMonthlyBalance = new AttendMonthlyBalanceBLL())
            {
                return bllAttendMonthlyBalance.ImportMonthlyBalanceForShow(strPhysicalPath);
            }
        }

        /// <summary>
        /// 新增信息
        /// </summary>
        /// <param name="entTemp"></param>
        /// <returns></returns>
        [OperationContract]
        public string AddAttendMonthlyBalance(T_HR_ATTENDMONTHLYBALANCE entTemp)
        {
            using (AttendMonthlyBalanceBLL bllAttendMonthlyBalance = new AttendMonthlyBalanceBLL())
            {
                return bllAttendMonthlyBalance.AddMonthlyBalance(entTemp);
            }
        }

        /// <summary>
        /// 修改信息
        /// </summary>
        /// <param name="entTemp"></param>
        /// <returns></returns>
        [OperationContract]
        public string ModifyAttendMonthlyBalance(T_HR_ATTENDMONTHLYBALANCE entTemp)
        {
            using (AttendMonthlyBalanceBLL bllAttendMonthlyBalance = new AttendMonthlyBalanceBLL())
            {
                return bllAttendMonthlyBalance.ModifyMonthlyBalance(entTemp);
            }
        }

        /// <summary>
        /// 根据主键索引，删除信息(注：暂定为物理删除)
        /// </summary>
        /// <param name="strAttendMonthlyBalanceId">主键索引</param>
        /// <returns></returns>
        [OperationContract]
        public string RemoveAttendMonthlyBalance(string strMonthlyBalanceId)
        {
            using (AttendMonthlyBalanceBLL bllAttendMonthlyBalance = new AttendMonthlyBalanceBLL())
            {
                return bllAttendMonthlyBalance.DeleteMonthlyBalance(strMonthlyBalanceId);
            }
        }

        /// <summary>
        /// 计算指定月份指定员工的考勤月度结算
        /// </summary>
        /// <param name="strCurDateMonth"></param>
        /// <param name="strAssignedObjectType"></param>
        /// <param name="strAssignedObjectID)"></param>
        [OperationContract]
        public void CalculateEmployeeAttendanceMonthly(string strIsCurrentMonth, string strAssignedObjectType, string strAssignedObjectID)
        {
            using (AttendMonthlyBalanceBLL bllAttendMonthlyBalance = new AttendMonthlyBalanceBLL())
            {
                bllAttendMonthlyBalance.CalculateEmployeeAttendanceMonthly(strIsCurrentMonth, strAssignedObjectType, strAssignedObjectID);
            }
        }

        /// <summary>
        /// 根据指定月份和员工ID，进行考勤月度结算
        /// </summary>
        /// <param name="strCurDateMonth">指定月份</param>
        /// <param name="strDepartmentID">员工ID</param>
        [OperationContract]
        public void CalculateEmployeeAttendanceMonthlyByEmployeeID(string strCurDateMonth, string strEmployeeID)
        {
            using (AttendMonthlyBalanceBLL bllAttendMonthlyBalance = new AttendMonthlyBalanceBLL())
            {
                bllAttendMonthlyBalance.CalculateEmployeeAttendanceMonthlyByEmployeeID(strCurDateMonth, strEmployeeID);
            }
        }

        /// <summary>
        /// 根据指定月份和岗位ID，进行考勤月度结算
        /// </summary>
        /// <param name="strCurDateMonth">指定月份</param>
        /// <param name="strDepartmentID">岗位ID</param>
        [OperationContract]
        public void CalculateEmployeeAttendanceMonthlyByPostID(string strCurDateMonth, string strPostID)
        {
            using (AttendMonthlyBalanceBLL bllAttendMonthlyBalance = new AttendMonthlyBalanceBLL())
            {
                bllAttendMonthlyBalance.CalculateEmployeeAttendanceMonthlyByPostID(strCurDateMonth, strPostID);
            }
        }

        /// <summary>
        /// 根据指定月份和部门ID，进行考勤月度结算
        /// </summary>
        /// <param name="strCurDateMonth">指定月份</param>
        /// <param name="strDepartmentID">部门ID</param>
        [OperationContract]
        public void CalculateEmployeeAttendanceMonthlyByDepartmentID(string strCurDateMonth, string strDepartmentID)
        {
            using (AttendMonthlyBalanceBLL bllAttendMonthlyBalance = new AttendMonthlyBalanceBLL())
            {
                bllAttendMonthlyBalance.CalculateEmployeeAttendanceMonthlyByDepartmentID(strCurDateMonth, strDepartmentID);
            }
        }

        /// <summary>
        /// 根据指定月份和部门ID，进行考勤月度结算
        /// </summary>
        /// <param name="strCurDateMonth">指定月份</param>
        /// <param name="strDepartmentID">部门ID</param>
        [OperationContract]
        public void CalculateEmployeeAttendanceMonthlyByCompanyID(string strCurDateMonth, string strCompanyID)
        {
            using (AttendMonthlyBalanceBLL bllAttendMonthlyBalance = new AttendMonthlyBalanceBLL())
            {
                bllAttendMonthlyBalance.CalculateEmployeeAttendanceMonthlyByCompanyID(strCurDateMonth, strCompanyID);
            }
        }

        #endregion

        #region T_HR_ATTENDMONTHLYBATCHBALANCE 员工考勤月度结算批量审批服务

        /// <summary>
        /// 获取信息
        /// </summary>
        /// <param name="strMonthlyBatchBalanceId">主键索引</param>
        /// <returns></returns>
        [OperationContract]
        public T_HR_ATTENDMONTHLYBATCHBALANCE GetAttendMonthlyBatchBalanceByID(string strMonthlyBatchBalanceId)
        {
            using (AttendMonthlyBatchBalanceBLL bllAttendMonthlyBatchBalance = new AttendMonthlyBatchBalanceBLL())
            {
                return bllAttendMonthlyBatchBalance.GetAttendMonthlyBatchBalanceByID(strMonthlyBatchBalanceId);
            }
        }

        /// <summary>
        /// 获取考勤月度批量结算信息
        /// </summary>
        /// <param name="strAttendMonthlyBalanceId">主键索引</param>
        /// <returns></returns>
        [OperationContract]
        public T_HR_ATTENDMONTHLYBATCHBALANCE GetAttendMonthlyBatchBalanceByMonthlyBalanceId(string strAttendMonthlyBalanceId)
        {
            using (AttendMonthlyBatchBalanceBLL bllAttendMonthlyBatchBalance = new AttendMonthlyBatchBalanceBLL())
            {
                return bllAttendMonthlyBatchBalance.GetAttendMonthlyBatchBalanceByMonthlyBalanceId(strAttendMonthlyBalanceId);
            }
        }

        /// <summary>
        /// 获取员工考勤月度结算批量审批信息
        /// </summary>
        /// <param name="strBalanceObjectType"></param>
        /// <param name="strBalanceObjectId"></param>
        /// <param name="dBalanceYear"></param>
        /// <param name="dBalanceMonth"></param>
        /// <param name="strCheckState"></param>
        /// <returns></returns>
        [OperationContract]
        public T_HR_ATTENDMONTHLYBATCHBALANCE GetAttendMonthlyBatchBalanceByMultSearch(string strOwnerID, string strBalanceObjectType, string strBalanceObjectId, decimal dBalanceYear, decimal dBalanceMonth, string strCheckState)
        {
            using (AttendMonthlyBatchBalanceBLL bllAttendMonthlyBatchBalance = new AttendMonthlyBatchBalanceBLL())
            {
                return bllAttendMonthlyBatchBalance.GetAttendMonthlyBatchBalanceByMultSearch(strOwnerID, strBalanceObjectType, strBalanceObjectId, dBalanceYear, dBalanceMonth, strCheckState);
            }
        }

        /// <summary>
        /// 新增信息
        /// </summary>
        /// <param name="entTemp"></param>
        /// <returns></returns>
        [OperationContract]
        public string AddAttendMonthlyBatchBalance(T_HR_ATTENDMONTHLYBATCHBALANCE entTemp)
        {
            using (AttendMonthlyBatchBalanceBLL bllAttendMonthlyBatchBalance = new AttendMonthlyBatchBalanceBLL())
            {
                return bllAttendMonthlyBatchBalance.AddMonthlyBatchBalance(entTemp);
            }
        }

        /// <summary>
        /// 修改信息
        /// </summary>
        /// <param name="entTemp"></param>
        /// <returns></returns>
        [OperationContract]
        public string ModifyAttendMonthlyBatchBalance(T_HR_ATTENDMONTHLYBATCHBALANCE entTemp)
        {

            using (AttendMonthlyBatchBalanceBLL bllAttendMonthlyBatchBalance = new AttendMonthlyBatchBalanceBLL())
            {
                return bllAttendMonthlyBatchBalance.ModifyMonthlyBatchBalance(entTemp);
            }
        }

        /// <summary>
        /// 根据主键索引，删除信息(注：暂定为物理删除)
        /// </summary>
        /// <param name="strAttendMonthlyBatchBalanceId">主键索引</param>
        /// <returns></returns>
        [OperationContract]
        public string RemoveAttendMonthlyBatchBalance(string strMonthlyBatchBalanceId)
        {
            using (AttendMonthlyBatchBalanceBLL bllAttendMonthlyBatchBalance = new AttendMonthlyBatchBalanceBLL())
            {
                return bllAttendMonthlyBatchBalance.DeleteMonthlyBatchBalance(strMonthlyBatchBalanceId);
            }
        }

        /// <summary>
        /// 月度结算批量审批
        /// </summary>
        /// <param name="entTemp"></param>
        /// <returns></returns>
        [OperationContract]
        public string AuditMonthlyBatchBalance(T_HR_ATTENDMONTHLYBATCHBALANCE entTemp)
        {
            using (AttendMonthlyBatchBalanceBLL bllAttendMonthlyBatchBalance = new AttendMonthlyBatchBalanceBLL())
            {
                return bllAttendMonthlyBatchBalance.AuditMonthlyBatchBalance(entTemp);
            }
        }
        #endregion

        #region T_HR_ATTENDYEARLYBALANCE 员工考勤年度结算服务

        /// <summary>
        /// 获取员工考勤年度结算信息
        /// </summary>
        /// <param name="strYearlyBalanceId">主键索引</param>
        /// <returns></returns>
        [OperationContract]
        public T_HR_ATTENDYEARLYBALANCE GetAttendYearlyBalanceByID(string strYearlyBalanceId)
        {
            using (AttendYearlyBalanceBLL bllAttendYearlyBalance = new AttendYearlyBalanceBLL())
            {
                return bllAttendYearlyBalance.GetAttendYearlyBalanceByID(strYearlyBalanceId);
            }
        }

        /// <summary>
        /// 批量获取员工考勤年度结算信息，分页显示
        /// </summary>
        /// <param name="strEmployeeID">员工序号</param>
        /// <param name="dBalanceYear">结算年份</param>       
        /// <param name="strSortKey">排序字段</param>
        /// <param name="pageIndex">当前页码</param>
        /// <param name="pageSize">每页显示记录数</param>
        /// <param name="pageCount">总页码</param>
        /// <returns>返回信息</returns>
        [OperationContract]
        public List<T_HR_ATTENDYEARLYBALANCE> GetAttendYearlyBalanceListByMultSearch(string strOwnerID, string strEmployeeID, decimal dBalanceYear,
            string strSortKey, int pageIndex, int pageSize, ref int pageCount)
        {
            using (AttendYearlyBalanceBLL bllAttendYearlyBalance = new AttendYearlyBalanceBLL())
            {
                var ents = bllAttendYearlyBalance.GetAttendYearlyBalanceRdListByMultSearch(strOwnerID, strEmployeeID, dBalanceYear, strSortKey, pageIndex, pageSize, ref pageCount);

                if (ents == null)
                {
                    return null;
                }

                return ents.ToList();
            }
        }

        /// <summary>
        /// 新增员工考勤年度结算信息
        /// </summary>
        /// <param name="entTemp"></param>
        /// <returns></returns>
        [OperationContract]
        public string AddAttendYearlyBalance(T_HR_ATTENDYEARLYBALANCE entTemp)
        {
            using (AttendYearlyBalanceBLL bllAttendYearlyBalance = new AttendYearlyBalanceBLL())
            {
                return bllAttendYearlyBalance.AddYearlyBalance(entTemp);
            }
        }

        /// <summary>
        /// 修改员工考勤年度结算信息
        /// </summary>
        /// <param name="entTemp"></param>
        /// <returns></returns>
        [OperationContract]
        public string ModifyAttendYearlyBalance(T_HR_ATTENDYEARLYBALANCE entTemp)
        {
            using (AttendYearlyBalanceBLL bllAttendYearlyBalance = new AttendYearlyBalanceBLL())
            {
                return bllAttendYearlyBalance.ModifyYearlyBalance(entTemp);
            }
        }

        /// <summary>
        /// 根据主键索引，删除员工考勤年度结算信息(注：暂定为物理删除)
        /// </summary>
        /// <param name="strYearlyBalanceId">主键索引</param>
        /// <returns></returns>
        [OperationContract]
        public string RemoveAttendYearlyBalance(string strYearlyBalanceId)
        {
            using (AttendYearlyBalanceBLL bllAttendYearlyBalance = new AttendYearlyBalanceBLL())
            {
                return bllAttendYearlyBalance.DeleteYearlyBalance(strYearlyBalanceId);
            }
        }

        /// <summary>
        /// 计算指定年份指定公司的考勤年度结算
        /// </summary>
        /// <param name="strCurDateYear">指定年份</param>
        /// <param name="strCompanyID">指定公司ID</param>
        [OperationContract]
        public void CalculateEmployeeAttendanceYearlyByCompanyID(string strCurDateYear, string strCompanyID)
        {
            using (AttendYearlyBalanceBLL bllAttendYearlyBalance = new AttendYearlyBalanceBLL())
            {
                bllAttendYearlyBalance.CalculateEmployeeAttendanceYearlyByCompanyID(strCurDateYear, strCompanyID);
            }
        }

        /// <summary>
        /// 计算指定年份指定部门的考勤年度结算
        /// </summary>
        /// <param name="strCurDateYear">指定年份</param>
        /// <param name="strPostID">指定部门ID</param>
        [OperationContract]
        public void CalculateEmployeeAttendanceYearlyByPostID(string strCurDateYear, string strPostID)
        {
            using (AttendYearlyBalanceBLL bllAttendYearlyBalance = new AttendYearlyBalanceBLL())
            {
                bllAttendYearlyBalance.CalculateEmployeeAttendanceYearlyByPostID(strCurDateYear, strPostID);
            }
        }

        /// <summary>
        /// 计算指定年份指定部门的考勤年度结算
        /// </summary>
        /// <param name="strCurDateYear">指定年份</param>
        /// <param name="strDepartmentID">指定部门ID</param>
        [OperationContract]
        public void CalculateEmployeeAttendanceYearlyByDepartmentID(string strCurDateYear, string strDepartmentID)
        {
            using (AttendYearlyBalanceBLL bllAttendYearlyBalance = new AttendYearlyBalanceBLL())
            {
                bllAttendYearlyBalance.CalculateEmployeeAttendanceYearlyByDepartmentID(strCurDateYear, strDepartmentID);
            }
        }

        /// <summary>
        /// 计算指定年份指定部门的考勤年度结算
        /// </summary>
        /// <param name="strCurDateYear">指定年份</param>
        /// <param name="strEmployeeID">指定员工ID</param>
        [OperationContract]
        public void CalculateEmployeeAttendanceYearlyByEmployeeID(string strCurDateYear, string strEmployeeID)
        {
            using (AttendYearlyBalanceBLL bllAttendYearlyBalance = new AttendYearlyBalanceBLL())
            {
                bllAttendYearlyBalance.CalculateEmployeeAttendanceYearlyByEmployeeID(strCurDateYear, strEmployeeID);
            }
        }

        #endregion

        #region T_HR_EMPLOYEECANCELLEAVE 销假记录

        /// <summary>
        /// 获取指定员工的实际销假天数(实际销假天数=销假天数-公休假天数-每周休息天数)，实际销假时长(按小时计，实际销假合计时长=非整天销假时长-当日作息间隙休息时间+整天销假时长)
        /// </summary>
        /// <param name="strCancelLeaveId">当前销假记录的ID</param>
        /// <param name="strEmployeeID">员工ID</param>
        /// <param name="dtLeaveStartTime">销假起始时间</param>
        /// <param name="dtLeaveEndTime">销假截止时间</param>
        /// <param name="dLeaveDay">实际销假天数</param>
        /// <param name="dLeaveTime">实际销假时长</param>
        /// <param name="dLeaveTotalTime">实际销假合计时长</param>
        [OperationContract]
        public string GetRealCancelLeaveDayByEmployeeIdAndDate(string strCancelLeaveId, string strEmployeeID, DateTime dtCancelLeaveStartTime,
            DateTime dtCancelLeaveEndTime, ref decimal dCancelLeaveDay, ref decimal dCancelLeaveTime, ref decimal dCancelLeaveTotalTime)
        {
            using (EmployeeCancelLeaveBLL bll = new EmployeeCancelLeaveBLL())
            {
                return bll.GetRealCancelLeaveDayByEmployeeIdAndDate(strCancelLeaveId, strEmployeeID, dtCancelLeaveStartTime, dtCancelLeaveEndTime, ref dCancelLeaveDay, ref dCancelLeaveTime, ref dCancelLeaveTotalTime);
            }
        }

        /// <summary>
        /// 用于实体Grid中显示数据的分页查询
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页显示条数</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件中的参数值</param>
        /// <param name="pageCount">返回总页数</param>
        /// <returns>查询结果集</returns>
        [OperationContract]
        public List<T_HR_EMPLOYEECANCELLEAVE> EmployeeCancelLeavePaging(int pageIndex, int pageSize, string sort, string filterString, List<object> paras, ref int pageCount, string strCheckState, string strOwnerID)
        {
            using (EmployeeCancelLeaveBLL bll = new EmployeeCancelLeaveBLL())
            {
                var ents = bll.EmployeeCancelLeavePaging(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, strCheckState, strOwnerID);
                if (ents == null)
                {
                    return null;
                }

                return ents.ToList();
            }
        }
        /// <summary>
        /// 添加销假记录
        /// </summary>
        /// <param name="LeaveRecord">销假记录</param>

        [OperationContract]
        public string EmployeeCancelLeaveAdd(T_HR_EMPLOYEECANCELLEAVE obj)
        {
            using (EmployeeCancelLeaveBLL bll = new EmployeeCancelLeaveBLL())
            {
                return bll.EmployeeCancelLeaveAdd(obj);
            }
        }
        /// <summary>
        /// 修改销假记录
        /// </summary>
        /// <param name="obj"></param>

        [OperationContract]
        public string EmployeeCancelLeaveUpdate(T_HR_EMPLOYEECANCELLEAVE obj)
        {
            using (EmployeeCancelLeaveBLL bll = new EmployeeCancelLeaveBLL())
            {
                return bll.EmployeeCancelLeaveUpdate(obj);
            }
        }
        /// <summary>
        /// 删除销假记录
        /// </summary>
        /// <param name="IDs"></param>
        /// <returns></returns>
        [OperationContract]
        public bool EmployeeCancelLeaveDelete(string[] IDs)
        {
            using (EmployeeCancelLeaveBLL bll = new EmployeeCancelLeaveBLL())
            {
                int rslt = bll.EmployeeCancelLeaveDelete(IDs);
                return (rslt > 0);
            }
        }

        [OperationContract]
        public T_HR_EMPLOYEECANCELLEAVE GetEmployeeCancelLeaveByID(string id)
        {
            using (EmployeeCancelLeaveBLL bll = new EmployeeCancelLeaveBLL())
            {
                return bll.GetEmployeeCancelLeaveByID(id);
            }
        }


        /// <summary>
        /// 根据请假记录ID,获取全部的销假记录
        /// </summary>
        /// <param name="strLeaveRecordID">请假记录ID</param>
        /// <param name="strCheckState">审核状态</param>
        /// <returns></returns>
        [OperationContract]
        public List<T_HR_EMPLOYEECANCELLEAVE> GetEmployeeLeaveRdListsByLeaveRecordID(string strLeaveRecordID, string strCheckState)
        {
            using (EmployeeCancelLeaveBLL bll = new EmployeeCancelLeaveBLL())
            {
                List<T_HR_EMPLOYEECANCELLEAVE> rslt = bll.GetEmployeeLeaveRdListsByLeaveRecordID(strLeaveRecordID, strCheckState);
                return rslt;
            }
        }

        #endregion

        #region  T_HR_ATTENDMACHINESET 考勤机设置
        /// <summary>
        /// 用于实体Grid中显示数据的分页查询
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页显示条数</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件中的参数值</param>
        /// <param name="pageCount">返回总页数</param>
        /// <returns>查询结果集</returns>
        [OperationContract]
        public List<T_HR_ATTENDMACHINESET> GetAttendMachineSetPaging(int pageIndex, int pageSize, string sort, string filterString, string[] paras, ref int pageCount, string strCheckState, string userid)
        {
            using (AttendMachineSetBLL bll = new AttendMachineSetBLL())
            {
                var ents = bll.GetAttendMachineSetPaging(pageIndex, pageSize, sort, filterString, paras, ref pageCount, strCheckState, userid);

                if (ents == null)
                {
                    return null;
                }

                return ents.ToList();
            }
        }

        /// <summary>
        /// 新增考勤机设置
        /// </summary>
        /// <param name="entity">考勤机设置实体</param>
        [OperationContract]
        public void AttendMachineSetAdd(T_HR_ATTENDMACHINESET entity)
        {
            using (AttendMachineSetBLL bll = new AttendMachineSetBLL())
            {
                bll.Add(entity);
            }
        }

        /// <summary>
        /// 根据考勤机设置ID查询实体
        /// </summary>
        /// <param name="AttendMachineSetID">考勤机设置ID</param>
        /// <returns>返回考勤机设置实体</returns>
        [OperationContract]
        public T_HR_ATTENDMACHINESET GetAttendMachineSetByID(string AttendMachineSetID)
        {
            using (AttendMachineSetBLL bll = new AttendMachineSetBLL())
            {
                return bll.GetAttendMachineSetByID(AttendMachineSetID);
            }
        }

        /// <summary>
        /// 更新考勤机设置
        /// </summary>
        /// <param name="entity">考勤机设置实体</param>
        [OperationContract]
        public void AttendMachineSetUpdate(T_HR_ATTENDMACHINESET entity)
        {
            using (AttendMachineSetBLL bll = new AttendMachineSetBLL())
            {
                bll.AttendMachineSetUpdate(entity);
            }
        }


        /// <summary>
        /// 删除考勤机设置记录，可同时删除多行记录
        /// </summary>
        /// <param name="attendMachineSetIDs">考勤机设置ID数组</param>
        /// <returns></returns>
        [OperationContract]
        public int AttendMachineSetDelete(string[] attendMachineSetIDs)
        {
            using (AttendMachineSetBLL bll = new AttendMachineSetBLL())
            {
                return bll.AttendMachineSetDelete(attendMachineSetIDs);
            }
        }
        #endregion

        #region 带薪假考勤 Help

        /// <summary>
        /// add 检查请假及出差情况 weirui 11-14
        /// </summary>
        /// <param name="strCompanyID"></param>
        /// <param name="strCurMonth"></param>
        [OperationContract]
        public void UpdateAttendRecordByEvectionAndLeaveRd(string strCompanyID, string strCurMonth)
        {
            using (AttendanceRecordBLL bllAttRd = new AttendanceRecordBLL())
            {
                bllAttRd.UpdateAttendRecordByEvectionAndLeaveRd(strCompanyID, strCurMonth);
            }
        }

        /// <summary>
        /// add 根据公司ID，计算该公司指定月份员工考勤异常情况 weirui 11-14
        /// </summary>
        /// <param name="strCompanyId"></param>
        /// <param name="strPunchMonth"></param>
        /// <param name="strMsg"></param>
        [OperationContract]
        public void CheckAbnormRecordByCompanyId(string strCompanyId, string strPunchMonth, ref string strMsg)
        {
            using (AbnormRecordBLL bllAbnormRecord = new AbnormRecordBLL())
            {
                bllAbnormRecord.CheckAbnormRecordByCompanyId(strCompanyId, strPunchMonth, ref strMsg);
            }
        }

        /// <summary>
        /// add 根据员工指纹编号，计算该员工指定月份考勤异常情况 weirui 11-14
        /// </summary>
        /// <param name="strEmployeeIds"></param>
        /// <param name="strPunchMonth"></param>
        /// <param name="strMsg"></param>
        [OperationContract]
        public void CheckAbnormRecordByEmployeeIds(string strEmployeeIds, string strPunchMonth, ref string strMsg)
        {
            using (AbnormRecordBLL bllAbnormRecord = new AbnormRecordBLL())
            {
                bllAbnormRecord.CheckAbnormRecordByEmployeeIds(strEmployeeIds, strPunchMonth, ref strMsg);
            }
        }

        /// <summary>
        /// add 生成&更新带薪假记录 weirui 11-14
        /// </summary>
        /// <param name="strOrgType"></param>
        /// <param name="strOrgId"></param>
        [OperationContract]
        public void CalculateEmployeeLevelDayCountByOrgID(string strOrgType, string strOrgId)
        {
            using (EmployeeLevelDayCountBLL bllLevelDayCount = new EmployeeLevelDayCountBLL())
            {
                bllLevelDayCount.CalculateEmployeeLevelDayCountByOrgID(strOrgType, strOrgId);
            }
        }

        #endregion


        /// <summary>
        /// 根据员工ID和公司ID查询考勤记录表 weirui 2012-12-6 add
        /// </summary>
        /// <param name="ownerId"></param>
        /// <param name="ownerCompanyId"></param>
        /// <returns></returns>
        [OperationContract]
        public List<AbnormalAttendanceeEntity> ListEMPLOYEEABNORMRECORD(string ownerId, string ownerCompanyId, string startDate, string endDate)
        {
            using (AbnormRecordBLL bllAttRd = new AbnormRecordBLL())
            {
                return bllAttRd.ListEMPLOYEEABNORMRECORD(ownerId, ownerCompanyId, startDate, endDate);
            }
        }


        /// <summary>
        /// 根据公司IDOr员工ID查找员工可以调休的请假天数(掉休假)
        /// </summary>
        /// <param name="ownerId">员工ID</param>
        /// <param name="ownerCompanyId">公司ID</param>
        /// <param name="startDate">开始时间</param>
        /// <param name="endDate">结束时间</param>
        /// <returns></returns>
        [OperationContract]
        public List<AbnormalAttendanceeEntity> GetAdjustableVacation(string ownerId, string ownerCompanyId, string startDate, string endDate)
        {
            using (AbnormRecordBLL bllAttRd = new AbnormRecordBLL())
            {
                return bllAttRd.GetAdjustableVacation(ownerId, ownerCompanyId, startDate, endDate);
            }
        }

        /// <summary>
        /// 根据公司IDOr员工ID查找员工请假记录
        /// </summary>
        /// <param name="ownerId">员工ID</param>
        /// <param name="ownerCompanyId">公司ID</param>
        /// <param name="startDate">开始时间</param>
        /// <param name="endDate">结束时间</param>
        /// <returns></returns>
        [OperationContract]
        public List<AbnormalAttendanceeEntity> GetEmployeeLeaverecord(string ownerId, string ownerCompanyId, string startDate, string endDate)
        {
            using (AbnormRecordBLL bllAttRd = new AbnormRecordBLL())
            {
                return bllAttRd.GetEmployeeLeaverecord(ownerId, ownerCompanyId, startDate, endDate);
            }
        }


        /// <summary>
        /// 得到每天最后的打卡时间
        /// </summary>
        /// <param name="ownerId"></param>
        /// <param name="ownerCompanyId"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        [OperationContract]
        public List<AbnormalAttendanceeEntity> GetLasterClockInRecord(string ownerId, string ownerCompanyId, string startDate, string endDate)
        {
            using (AbnormRecordBLL bllAttRd = new AbnormRecordBLL())
            {
                return bllAttRd.GetLasterClockInRecord(ownerId, ownerCompanyId, startDate, endDate);
            }
        }

        /// <summary>
        /// 考勤汇总导出
        /// </summary>
        /// <param name="ownerId"></param>
        /// <param name="ownerCompanyId"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        [OperationContract]
        public byte[] ExportEmployeesIntime(string ownerId, string ownerCompanyId, string startDate, string endDate)
        {
            using (AbnormRecordBLL bllAttRd = new AbnormRecordBLL())
            {
                return bllAttRd.ExportEmployeesIntime(ownerId, ownerCompanyId, startDate, endDate);
            }
        }

         /// <summary>
        /// 根据公司ID，计算该公司指定时间段内员工考勤异常情况
        /// </summary>
        /// <param name="strCompanyId"></param>
        /// <param name="dtPunchFrom"></param>
        /// <param name="dtPunchTo"></param>
        /// <param name="strMsg"></param>
        [OperationContract]
        public void CheckAbnormRdForCompanyByDate(string strCompanyId, DateTime dtPunchFrom, DateTime dtPunchTo, ref string strMsg)
        {
            using (AbnormRecordBLL bll = new AbnormRecordBLL())
            {
                bll.CheckAbnormRdForCompanyByDate(strCompanyId, dtPunchFrom, dtPunchTo, ref strMsg);
            }
        }

         /// <summary>
        /// 根据员工ID，计算该员工指定时间段内员工考勤异常情况
        /// </summary>
        /// <param name="strEmployeeIds"></param>
        /// <param name="dtPunchFrom"></param>
        /// <param name="dtPunchTo"></param>
        /// <param name="strMsg"></param>
        [OperationContract]
        public void CheckAbnormRdForEmployeesByDate(string strEmployeeIds, DateTime dtPunchFrom, DateTime dtPunchTo, ref string strMsg)
        {
            using (AbnormRecordBLL bll = new AbnormRecordBLL())
            {
                 bll.CheckAbnormRdForEmployeesByDate(strEmployeeIds, dtPunchFrom, dtPunchTo, ref strMsg);
            }
        }
         /// <summary>
        /// 对指定解决方案定时生成考勤记录
        /// </summary>
        /// <param name="strCompanyID"></param>
        /// <returns></returns>
        [OperationContract]
        public void GetAttendSolAsignForOutEngineXml(T_HR_ATTENDANCESOLUTIONASIGN entTemp)
        {
            using (AttendanceSolutionAsignBLL bll = new AttendanceSolutionAsignBLL())
            {
                bll.GetAttendSolAsignForOutEngineXml(entTemp);
            }
        }

        /// <summary>
        /// 获取员工生效的考勤方案
        /// </summary>
        /// <param name="strCompanyID"></param>
        /// <returns></returns>
        [OperationContract]
        public void GetAttendanceSolutionAsignByEmployeeIDAndDate(string strEmployeeID, DateTime dtStart)
        {
            using (AttendanceSolutionAsignBLL bll = new AttendanceSolutionAsignBLL())
            {
                bll.GetAttendanceSolutionAsignByEmployeeIDAndDate(strEmployeeID,dtStart);
            }
        }
    }
}