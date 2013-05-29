using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using SMT_FB_EFModel;
using System.Data.Objects.DataClasses;
using SMT.FB.DAL;
using SMT.Foundation.Log;

namespace SMT.FB.BLL
{
    public class SaveEntityBLL : QueryEntityBLL
    {

        #region ReSubmit
        public FBEntity ReSubmit(FBEntity fbEntity)
        {
            EntityObject entity = fbEntity.Entity;

            string returnType = fbEntity.Entity.GetType().Name;
            Type type = this.GetType();
            MethodInfo method = type.GetMethod("ReSubmit" + returnType);
            if (method != null)
            {
                try
                {
                    object result = method.Invoke(this, new object[] { entity });
                    return result as FBEntity;
                }
                catch (Exception ex)
                {

                    throw ex.InnerException;
                }
            }
            else
            {
                return null; // SaveDefault(fbEntity);
            }
        }

        public FBEntity ReSubmitT_FB_COMPANYBUDGETAPPLYMASTER(T_FB_COMPANYBUDGETAPPLYMASTER entity)
        {

            FBEntity fbEntity = GetFBEntityByEntityKey(entity.EntityKey);
            fbEntity.FBEntityState = FBEntityState.ReSubmit;

            var master = fbEntity.Entity as T_FB_COMPANYBUDGETAPPLYMASTER;
            var details = fbEntity.GetRelationFBEntities(typeof(T_FB_COMPANYBUDGETAPPLYDETAIL).Name);

            T_FB_COMPANYBUDGETAPPLYMASTER resultEntity = master.CopyEntity();
            resultEntity.COMPANYBUDGETAPPLYMASTERID = Guid.NewGuid().ToString();
            resultEntity.COMPANYBUDGETAPPLYMASTERCODE = "自动生成";
            resultEntity.CHECKSTATES = (int)CheckStates.UnSubmit;
            resultEntity.ISVALID = "0"; // 未汇总
            resultEntity.T_FB_COMPANYBUDGETSUMDETAIL = null;

            var result = resultEntity.ToFBEntity();
            var resultDetails = result.GetRelationFBEntities(typeof(T_FB_COMPANYBUDGETAPPLYDETAIL).Name);

            var tempDetails = details.ToEntityList<T_FB_COMPANYBUDGETAPPLYDETAIL>();
            foreach (var item in tempDetails)
            {
                T_FB_COMPANYBUDGETAPPLYDETAIL detail = item.CopyEntity();
                detail.COMPANYBUDGETAPPLYDETAILID = Guid.NewGuid().ToString();
                detail.T_FB_COMPANYBUDGETAPPLYMASTER = resultEntity;
                FBEntity detailFBEntity = detail.ToFBEntity();
                detailFBEntity.FBEntityState = FBEntityState.Added;
                resultDetails.Add(detailFBEntity);
            }

            result.FBEntityState = FBEntityState.ReSubmit;
            return result;

        }

        public FBEntity ReSubmitT_FB_DEPTBUDGETAPPLYMASTER(T_FB_DEPTBUDGETAPPLYMASTER entity)
        {
            FBEntity fbEntity = GetFBEntityByEntityKey(entity.EntityKey);
            fbEntity.FBEntityState = FBEntityState.ReSubmit;

            var master = fbEntity.Entity as T_FB_DEPTBUDGETAPPLYMASTER;
            var details = fbEntity.GetRelationFBEntities(typeof(T_FB_DEPTBUDGETAPPLYDETAIL).Name);

            var resultEntity = master.CopyEntity();
            resultEntity.DEPTBUDGETAPPLYMASTERID = Guid.NewGuid().ToString();
            resultEntity.DEPTBUDGETAPPLYMASTERCODE = "自动生成";
            resultEntity.CHECKSTATES = (int)CheckStates.UnSubmit;
            resultEntity.ISVALID = "0"; // 未汇总
            resultEntity.T_FB_DEPTBUDGETAPPLYDETAIL = null;

            var result = resultEntity.ToFBEntity();
            var resultDetails = result.GetRelationFBEntities(typeof(T_FB_DEPTBUDGETAPPLYDETAIL).Name);

            var tempDetails = details.ToEntityList<T_FB_DEPTBUDGETAPPLYDETAIL>();
            foreach (var item in tempDetails)
            {
                T_FB_DEPTBUDGETAPPLYDETAIL detail = item.CopyEntity();
                detail.DEPTBUDGETAPPLYDETAILID = Guid.NewGuid().ToString();
                detail.T_FB_DEPTBUDGETAPPLYMASTER = resultEntity;

                FBEntity detailFBEntity = detail.ToFBEntity();
                detailFBEntity.FBEntityState = FBEntityState.Added;
                resultDetails.Add(detailFBEntity);
            }

            result.FBEntityState = FBEntityState.ReSubmit;
            return result;

        }

        public FBEntity ReSubmitT_FB_EXTENSIONALORDER(T_FB_EXTENSIONALORDER entity)
        {
            FBEntityBLL bll = this;
            FBEntity fbOldEntity = GetFBEntityByEntityKey(entity.EntityKey);


            var master = fbOldEntity.Entity as T_FB_EXTENSIONALORDER;
            var details = fbOldEntity.GetRelationFBEntities(typeof(T_FB_EXTENSIONORDERDETAIL).Name);
            details.ForEach(item =>
            {
                bll.InnerRemove(item.Entity);
            });

            master.INNERORDERID = string.Empty;
            master.CHECKSTATES = (int)CheckStates.Approving;
            master.TOTALMONEY = entity.TOTALMONEY;

            var result = master.ToFBEntity();
            List<FBEntity> listDetail = entity.T_FB_EXTENSIONORDERDETAIL.ToList().CreateList(item =>
                {
                    T_FB_EXTENSIONORDERDETAIL detail = new T_FB_EXTENSIONORDERDETAIL();
                    detail.EXTENSIONORDERDETAILID = Guid.NewGuid().ToString();

                    detail.APPLIEDMONEY = item.APPLIEDMONEY;
                    detail.CHARGETYPE = item.CHARGETYPE;
                    detail.CREATEDATE = DateTime.Now;
                    detail.CREATEUSERID = item.CREATEUSERID;
                    detail.REMARK = item.REMARK;
                    detail.T_FB_SUBJECT = item.T_FB_SUBJECT;
                    detail.UPDATEUSERID = item.UPDATEUSERID;
                    detail.USABLEMONEY = item.USABLEMONEY;
                    detail.T_FB_EXTENSIONALORDER = entity;

                    FBEntity fbEntityDetail = detail.ToFBEntity();
                    fbEntityDetail.FBEntityState = FBEntityState.Added;
                    return fbEntityDetail;
                });
            result.AddFBEntities<T_FB_EXTENSIONORDERDETAIL>(listDetail);

            result.FBEntityState = FBEntityState.ReSubmit;
            SaveT_FB_EXTENSIONALORDER(result);
            return result;

        }
        #endregion

    }
}
