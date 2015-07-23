using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using TM_SaaS_OA_EFModel;
using System.Data.Objects.DataClasses;
using System.Data.Objects;
using System.Collections;
using System.Reflection;

using FlowWFService = SMT.SaaS.BLLCommonServices.FlowWFService;
using System.Runtime.Serialization;
using System.Xml.Linq;

namespace SMT.FB.BLL
{
    public class FBCommonBLL : SaveEntityBLL
    {
  


        #region 2.	数据保存操作方法
        /// <summary>
        /// 保存实体
        /// </summary>
        /// <param name="fbEntityList"></param>
        /// <returns></returns>
        public SaveResult Save(SaveEntity saveEntity)
        {

            SaveResult result = new SaveResult();
            try
            {
                result.FBEntity = base.SaveEntityBLLSave(saveEntity);
                result.Successful = true;
            }
            catch (FBBLLException ex)
            {
                result.Successful = false;
                result.Exception = ex.Message;
                SystemBLL.Debug(ex.ToString());

            }
            return result;
        }

      
     
        #endregion


        #region 3.      用于移动版单据审核
        /// <summary>
        /// 根据实体名，实体的主键ID及审核状态，更新指定实体的记录
        /// </summary>
        /// <param name="strModelCode">实体名</param>
        /// <param name="orderID">实体的主键ID</param>
        /// <param name="strCheckStates">审核状态</param>
        public int UpdateCheckState(string strModelCode, string orderID, string strCheckStates, ref string strMsg)
        {
            int i = -1;
            try
            {
                SystemBLL.Debug("UpdateCheckState方法已被调用，参数：strModelCode: " + strModelCode + ", orderID: " + orderID + ", strCheckStates: " + strCheckStates);

                // begin 用于出差报销、事项审批的手机提交
                //if (strModelCode == "Travel")
                //{
                //    var tempResult = UpdateExtensionOrder(strModelCode, orderID, strCheckStates, ref strMsg);
                //    return (tempResult == null) ? -1 : 1;

                //}
                List<EntityInfo> EntityInfoList = FBCommonBLL.FBCommonEntityList;
                if (EntityInfoList == null)
                {
                    strMsg = "预算服务初始化异常，请重试。";
                    return -1;
                }

                if (EntityInfoList.Count() == 0)
                {
                    strMsg = "预算服务初始化异常，请重试。";
                    return -1;
                }

                string strTypeName = "";
                string strKeyName = "";
                CheckStates cs = CheckStates.UnSubmit;
                switch (strCheckStates)
                {
                    case "1":
                        cs = CheckStates.Approving;
                        break;
                    case "2":
                        cs = CheckStates.Approved;
                        break;
                    case "3":
                        cs = CheckStates.UnApproved;
                        break;
                    default:
                        break;
                }

                var entityInfo = EntityInfoList.Where(t => t.Type == strModelCode).FirstOrDefault();
                strTypeName = entityInfo.Type;
                strKeyName = entityInfo.KeyName;
                /////add 2012.12.12
                /////传入报销月份为时间去和当前时间判断，如果不在同一年
                /////说明该报销单是跨年的，则不能进行审核操作，即当年的报销单只能在当年进行报销
                //if (dNewCheckStates == FBAEnums.CheckStates.Approved || dNewCheckStates == FBAEnums.CheckStates.Approving)
                //{
                //    if (IsOverYear(entity.BUDGETARYMONTH))
                //    {
                //        strMsg = "报销单跨年后只能终审不通过(财务规定)";
                //        Tracer.Debug(strMsg);
                //        return;
                //    }
                //}
                using (FBCommonBLL bllCommon = new FBCommonBLL())
                {
                    bllCommon.BeginTransaction();
                    SystemBLL.Debug("BeginTransaction " + strModelCode + " 的单据[" + orderID + "]");
                    try
                    {
                        QueryExpression qe = QueryExpression.Equal(strKeyName, orderID);
                        qe.QueryType = strTypeName;

                        var data = qe.Query(bllCommon);
                        var order = data.FirstOrDefault();
                        if (order == null)
                        {
                            strMsg = "没有可操作的数据";
                            return -1;
                        }

                        bllCommon.AuditFBEntityWithoutFlow(order, cs, ref strMsg);
                        i = 1;
                        if (string.IsNullOrEmpty(strMsg))
                        {
                            bllCommon.CommitTransaction();
                            SystemBLL.Debug("CommitTransaction " + strModelCode + " 的单据[" + orderID + "]");
                        }
                        else
                        {
                            bllCommon.RollbackTransaction();
                            SystemBLL.Debug("RollbackTransaction 审核" + strModelCode + "的单据[" + orderID + "]失败，提示消息为：" + strMsg);

                        }

                    }
                    catch (Exception ex)
                    {
                        bllCommon.RollbackTransaction();
                        SystemBLL.Debug("RollbackTransaction 审核" + strModelCode + "的单据[" + orderID + "]失败，提示消息为：" + strMsg);
                        throw ex;
                    }
                }
            }
            catch (Exception ex)
            {
                strMsg = "单据审核异常，请联系管理员";
                throw ex;
            }

            // 把消息通过异常机制返回
            if (!string.IsNullOrWhiteSpace(strMsg))
            {

                SystemBLL.Debug("审核" + strModelCode + "的单据[" + orderID + "]失败，提示消息为：" + strMsg);
                throw new Exception(strMsg);
            }

            return i;
        }
        #endregion

    }


}
