using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.ServiceModel.Activation;
using SMT.FB.BLL;
using System.Data.Objects.DataClasses;
using SMT_FB_EFModel;
using System.Reflection;
using System.Collections;
using System.Configuration;
using System.Web.Services.Protocols;
using System.Xml;

using System.Web.Services.Description;
using System.Xml.Linq;
using System.IO;
using SMT.Foundation.Log;


namespace SMT.FB.Services
{
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceContract]
    [ServiceKnownType("GetKnownTypes", typeof(Helper))]
    public class FBCommonService :FBServiceBase
    {

        #region 
        [OperationContract]
        public FBEntity GetFBEntity(QueryExpression qp)
        {
            try
            {
                using (FBCommonBLL fbCommonBLL = new FBCommonBLL())
                {
                    return fbCommonBLL.GetFBEntity(qp);
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug(ex.ToString());
                throw ex;
            }
            
        }

        [OperationContract]
        public List<FBEntity> QueryFBEntities(QueryExpression qe)
        {
            try
            {
                using (FBCommonBLL fbCommonBLL = new FBCommonBLL())
                {
                    return fbCommonBLL.QueryFBEntities(qe);
                }
            }
            catch (Exception ex)
            {
                QueryExpression qeTemp = qe;
                string parameter = "";
                while (qeTemp != null)
                {
                    parameter += string.Format("Name:{0} ; Value:{1}  ; QueryType:{2}", qeTemp.PropertyName, qeTemp.PropertyValue, qeTemp.QueryType) + "\r\n";
                    qeTemp = qeTemp.RelatedExpression;
                }

                Tracer.Debug(ex.ToString() + "\r\n参数: " + parameter);
                throw ex;
            }
        }

        #endregion
        [OperationContract]
        public QueryResult Query(QueryExpression qe)
        {
            try
            {
                using (FBCommonBLL fbCommonBLL = new FBCommonBLL())
                {
                    return fbCommonBLL.QueryData(qe);
                }
            }
            catch (Exception ex)
            {
                QueryExpression qeTemp = qe;
                string parameter = "";
                while (qeTemp != null)
                {
                    parameter += string.Format("Name:{0} ; Value:{1}  ; QueryType:{2}", qeTemp.PropertyName, qeTemp.PropertyValue, qeTemp.QueryType) + "\r\n";
                    qeTemp = qeTemp.RelatedExpression;
                }

                Tracer.Debug(ex.ToString() + "\r\n参数: " + parameter);
                //  throw ex;
                QueryResult qr = new QueryResult();
                qr.Exception = ex.ToString();
                qr.Result = new List<FBEntity>();
                qr.Pager = qe.Pager;

                return qr;
            }
        }

        [OperationContract]
        public SaveResult Save(FBEntity fbEntity)
        {
            SaveResult result = new SaveResult();
            try
            {
                using (FBCommonBLL fbCommonBLL = new FBCommonBLL())
                {
                    result = fbCommonBLL.FBCommSaveEntity(fbEntity);
                }
            }
            catch (Exception ex)
            {
                result.Successful = false;
                result.Exception = ex.Message;
                Tracer.Debug(ex.ToString());
            }
            return result;
        }


        [OperationContract]
        public bool SaveList(List<FBEntity> fbEntityList)
        {
            try
            {
                using (FBCommonBLL fbCommonBLL = new FBCommonBLL())
                {
                    return fbCommonBLL.FBcommonBllSaveList(fbEntityList);
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug(ex.ToString());
                throw ex;
            }
            
        }

        [OperationContract]
        public AuditResult AuditFBEntity(FBEntity fbEntity, CheckStates checkStates)
        {
            try
            {
                using (FBCommonBLL fbCommonBLL = new FBCommonBLL())
                {
                    return fbCommonBLL.AuditFBEntity(fbEntity, checkStates);
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug(ex.ToString());
                throw ex;
            }
            
        }

        public List<EntityInfo> GetEntityInfoList()
        {
            return FBCommonBLL.FBCommonEntityList;
        }
       
    }



}
