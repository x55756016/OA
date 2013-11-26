using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT_FB_EFModel;
using System.Data.Objects.DataClasses;
using FlowWFService = SMT.SaaS.BLLCommonServices.FlowWFService;
using System.Runtime.Serialization;
using System.Xml.Linq;
using System.Linq.Expressions;
using SMT.Foundation.Log;
using System.Collections;
using SMT.FB.DAL;

namespace SMT.FB.BLL
{

    public class AuditBLL:IDisposable
    {
        /// <summary>
        /// 实体与xml的集合类
        /// </summary>
        public static Dictionary<string, XElement> dictXML_XE = null;

        public const string NAME_AUDITEDBY = "AuditedBy";
        FlowWFService.ServiceClient flowSerivice = new FlowWFService.ServiceClient();
        public void Open()
        {
        }
        public void Close()
        {
        }
        public void RollBack()
        {
        }
        public FlowWFService.DataResult Audit(FBEntity fbEntity)
        {
            VirtualAudit auditEntity = fbEntity.Entity as VirtualAudit;

            //flowSerivice.Open();
            if (string.IsNullOrEmpty(auditEntity.GUID))
            {
                auditEntity.GUID = "";
            }

            //string xml = GetAuditXml(fbEntity);
            string xml = GetAuditXmlForMobile(fbEntity);
            //Tracer.Debug(xml);

            FlowWFService.SubmitData AuditSubmitData = new FlowWFService.SubmitData();
            AuditSubmitData.FormID = auditEntity.FormID;
            AuditSubmitData.ModelCode = auditEntity.ModelCode;
            AuditSubmitData.ApprovalUser = new FlowWFService.UserInfo
            {
                CompanyID = auditEntity.CREATECOMPANYID,
                CompanyName = auditEntity.CREATECOMPANYNAME,
                DepartmentID = auditEntity.CREATEDEPARTMENTID,
                DepartmentName = auditEntity.CREATEDEPARTMENTNAME,
                PostID = auditEntity.CREATEPOSTID,
                PostName = auditEntity.CREATEPOSTNAME,
                UserID = auditEntity.CREATEUSERID,
                UserName = auditEntity.CREATEUSERNAME
            };
            AuditSubmitData.ApprovalContent = auditEntity.Content;
            AuditSubmitData.NextStateCode = auditEntity.NextStateCode;
            AuditSubmitData.NextApprovalUser = new FlowWFService.UserInfo
            {
                UserID = auditEntity.OWNERID,
                UserName = auditEntity.OWNERNAME,
                CompanyID = auditEntity.OWNERCOMPANYID,
                CompanyName = auditEntity.OWNERCOMPANYNAME,
                DepartmentID = auditEntity.OWNERDEPARTMENTID,
                DepartmentName = auditEntity.OWNERDEPARTMENTNAME,
                PostID = auditEntity.OWNERPOSTID,
                PostName = auditEntity.OWNERPOSTNAME
            };
            AuditSubmitData.ApprovalResult = (FlowWFService.ApprovalResult)auditEntity.Result;
            AuditSubmitData.FlowSelectType = (FlowWFService.FlowSelectType)auditEntity.FlowSelectType;

            FlowWFService.SubmitFlag AuditSubmitFlag = auditEntity.Op.ToUpper() == "ADD" ? FlowWFService.SubmitFlag.New : FlowWFService.SubmitFlag.Approval;
            AuditSubmitData.SubmitFlag = AuditSubmitFlag;
            AuditSubmitData.XML = xml;
            //#region 
            //FlowWFService.DataResult ar = new FlowWFService.DataResult();
            //ar.FlowResult = FlowWFService.FlowResult.FAIL;

            //return testResult;
            //#endregion

            FlowWFService.DataResult ar = flowSerivice.SubimtFlow(AuditSubmitData);

            if (ar.FlowResult == FlowWFService.FlowResult.FAIL)
            {
                string msg = @"流程提交or审核失败. 参数: "
                      + string.Format("\r\n ApprovalUser.CompanyID: {0} ", AuditSubmitData.ApprovalUser.CompanyID)
                      + string.Format("\r\n ApprovalUser.DepartmentID: {0} ", AuditSubmitData.ApprovalUser.DepartmentID)
                      + string.Format("\r\n ApprovalUser.PostID: {0} ", AuditSubmitData.ApprovalUser.PostID)
                      + string.Format("\r\n ApprovalUser.UserName: {0} ", AuditSubmitData.ApprovalUser.UserName)
                      + string.Format("\r\n FormID: {0} ", AuditSubmitData.FormID)
                      + string.Format("\r\n ModelCode: {0} ", AuditSubmitData.ModelCode)
                      + string.Format("\r\n ApprovalContent: {0} ", AuditSubmitData.ApprovalContent)
                      + string.Format("\r\n NextStateCode: {0} ", AuditSubmitData.NextStateCode)
                      + string.Format("\r\n ApprovalResult: {0} ", AuditSubmitData.ApprovalResult)
                      + string.Format("\r\n FlowSelectType: {0} ", AuditSubmitData.FlowSelectType.ToString())
                      + string.Format("\r\n SubmitFlag: {0} ", AuditSubmitData.SubmitFlag.ToString())
                      + string.Format("\r\n NextApprovalUser.UserID: {0} ", AuditSubmitData.NextApprovalUser.UserName)
                      + string.Format("\r\n NextApprovalUser.UserName: {0} ", AuditSubmitData.NextApprovalUser.UserName)
                      + string.Format("\r\n ErrMessage: {0} ", ar.Err);

                Tracer.Debug(msg);
                //Tracer.Debug(xml);
            }
            return ar;
        }

        public string GetAuditXml(FBEntity fbEntity)
        {

            //            string xml = @"<?xml version=""1.0"" encoding=""utf-8""?><System><Name>FB</Name><Object Name=""{0}"" Description=""{0}"">
            //                             <Attribute  Name=""{1}"" Description=""{1}"" DataType=""NVARCHAR2"" DataValue=""{2}""></Attribute>
            //                           </Object></System>";

            string xml = @"<?xml version=""1.0"" encoding=""utf-8""?><System><Name>FB</Name>{0}</System>";
            VirtualAudit auditEntity = fbEntity.Entity as VirtualAudit;
            List<EntityInfo> Modules = SubjectBLL.FBCommonEntityList;
            EntityInfo entityInfo = Modules.FirstOrDefault(entity =>
            {
                return entity.EntityCode == auditEntity.ModelCode;
            });
            if (entityInfo != null)
            {
                #region beyond
                //SMT.SaaS.BLLCommonServices.OrganizationWS.T_HR_POST th;
                //th.POSTLEVEL
                //SMT.SaaS.BLLCommonServices.OrganizationWS.OrganizationServiceClient osc;

                //T_FB_CHARGEAPPLYMASTER t;
                //t.TOTALMONEY
                //string FileName=AppDomain.CurrentDomain.BaseDirectory + @"xml\" + auditEntity.ModelCode+".xml";
                ////E:\beyond\fb\SMT.Saas.FB\SMT.FB.Services\xml\T_FB_BORROWAPPLYMASTER.xml
                //XElement xeroot = XElement.Load(FileName);
                XElement xe = dictXML_XE[auditEntity.ModelCode.ToLower()];
                xe = XElement.Parse(xe.ToString());  // 新建一个实例的XML。
                List<XElement> list = xe.Elements("Attribute").ToList();

                string AttName = "";
                string AttValue = "";
                list.ForEach(item =>
                {
                    AttName = item.Attribute("Name").Value;
                    if (AttName.ToLower() == "ordertypename")
                    {
                        AttValue = auditEntity.FormID;
                    }
                    else if (AttName == "POSTLEVEL")
                    {
                        object objOWNERPOSTID = fbEntity.ReferencedEntity[0].FBEntity.Entity.GetValue("OWNERPOSTID");
                        object objOWNERID = fbEntity.ReferencedEntity[0].FBEntity.Entity.GetValue("OWNERID");
                        if (objOWNERPOSTID != null && objOWNERID != null)
                        {
                            SMT.SaaS.BLLCommonServices.PersonnelWS.PersonnelServiceClient psl = new SaaS.BLLCommonServices.PersonnelWS.PersonnelServiceClient();
                            SMT.SaaS.BLLCommonServices.PersonnelWS.V_EMPLOYEEDETAIL epd = psl.GetEmployeeDetailViewByID(objOWNERID.ToString());
                            SMT.SaaS.BLLCommonServices.PersonnelWS.V_EMPLOYEEPOSTBRIEF vPost = epd.EMPLOYEEPOSTS.Where(c => c.POSTID == objOWNERPOSTID.ToString()).FirstOrDefault();
                            //decimal? dPostLevel = epd.EMPLOYEEPOSTS.Where(c => c.POSTID == objOWNERPOSTID.ToString()).FirstOrDefault().POSTLEVEL;
                            if (vPost != null)
                            {
                                AttValue = vPost.POSTLEVEL == null ? "0" : vPost.POSTLEVEL.Value.ToString();
                            }
                            else
                            {
                                AttValue = "0"; //针对岗位作废时处理：出现此情况的原因是人员岗位异动前，单据已提交审核中，异动完毕，原岗位作废
                            }
                        }
                    }
                    else if (AttName == "BorrowedMoney")
                    {
                        AttValue = GetBorrowedMoney(fbEntity.ReferencedEntity[0].FBEntity.Entity);
                    }
                    else if (AttName == "TravelMoney")
                    {
                        AttValue = GetTravelMoney(fbEntity.ReferencedEntity[0].FBEntity.Entity);
                    }
                    else if (AttName == "EntertainmentMoney")
                    {
                        AttValue = GetEntertainmentMoney(fbEntity.ReferencedEntity[0].FBEntity.Entity);
                    }
                    else
                    {
                        object obj = fbEntity.ReferencedEntity[0].FBEntity.Entity.GetValue(AttName);
                        AttValue = obj == null ? "" : obj.ToString();

                    }

                    item.Attribute("DataValue").SetValue(AttValue);
                });
                #endregion


                return string.Format(xml, xe.ToString());
            }
            return "";
        }

        public List<FlowWFService.FLOW_FLOWRECORDDETAIL_T> GetAuditList(string auditedBy, string modelName)
        {
            if (string.IsNullOrEmpty(auditedBy))
            {
                auditedBy = "";
            }

            if (string.IsNullOrEmpty(modelName))
            {
                modelName = "";
            }
            
            try
            {
               
                var flows = flowSerivice.GetFlowInfo("", "", "", "0", modelName, "", auditedBy);
                if (flows == null)
                {
                    return new List<FlowWFService.FLOW_FLOWRECORDDETAIL_T>();
                }
                return flows.ToList();
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("流程服务异常，方法：GetFlowInfo, 参数：'','','','0','{0}','',{1}",modelName, auditedBy), ex);
            }
        }

        #region 获取审核人相关的流程数据
        /// <summary>
        /// 通过条件（审核人）获取流程的数据(主要是单据ID）并产生相应的单据
        /// </summary>
        /// <param name="qe"></param>
        /// <returns></returns>
        public List<FBEntity> GetAuditedFBEntity(QueryExpression qe)
        {
            return GetAuditedFBEntity(qe, "");
        }

        /// <summary>
        /// 通过条件（审核人，单据类型）获取流程的数据(主要是单据ID）并产生相应的单据
        /// </summary>
        /// <param name="qe"></param>
        /// <param name="entityTypeName"></param>
        /// <returns></returns>
        public List<FBEntity> GetAuditedFBEntity(QueryExpression qe, string entityTypeName)
        {
            using (QueryEntityBLL bll = new QueryEntityBLL())
            {
                List<FBEntity> listResult = new List<FBEntity>();
                List<QueryExpression> qes = GetFlowQueryExpression(qe, entityTypeName);
                qes.ForEach(item =>
                {
                    item.Pager = qe.Pager;
                    item.OrderBy = qe.OrderBy;
                    item.RelatedExpression = qe.RelatedExpression;
                    var eObj = bll.GetFBEntities(item).ToList();
                    listResult.AddRange(eObj);
                });
                return listResult;

            }
        }

        /// <summary>
        /// 查出相应模块的实体信息
        /// </summary>
        /// <param name="entityTypeName"></param>
        /// <returns></returns>
        private List<EntityInfo> GetFlowModules(string entityTypeName)
        {
            List<EntityInfo> Modules = SubjectBLL.FBCommonEntityList;
            if (!string.IsNullOrEmpty(entityTypeName))
            {
                var entityInfo = SubjectBLL.FBCommonEntityList.FirstOrDefault(item =>
                {
                    return item.EntityCode == entityTypeName;
                });
                Modules = new List<EntityInfo>();
                if (entityInfo != null)
                {
                    Modules.Add(entityInfo);
                }
            }
            return Modules;
        }

        /// <summary>
        /// 通过条件（审核人，单据类型）获取流程的数据(主要是单据ID）并产生相应的查询条件
        /// </summary>
        /// <param name="qe">目前只有审核人</param>
        /// <param name="entityTypeName">单据类型</param>
        /// <returns>查询条件</returns>
        public List<QueryExpression> GetFlowQueryExpression(QueryExpression qe, string entityTypeName)
        {
            QueryExpression qeAuditedBy = qe.GetQueryExpression("AuditedBy");
            List<EntityInfo> Modules = GetFlowModules(entityTypeName);

            // 从流程系统中找出待审核的单
            List<FlowWFService.FLOW_FLOWRECORDDETAIL_T> list = this.GetAuditList(qeAuditedBy.PropertyValue, entityTypeName);
          
            List<QueryExpression> result = Modules.CreateList(item =>
            {
                QueryExpression qeTemp = null;
                var flows = list.FindAll(flowD =>
                {
                    return flowD.FLOW_FLOWRECORDMASTER_T.MODELCODE == item.EntityCode;
                });
                if (flows.Count > 0)
                {
                    qeTemp = new QueryExpression();
                    qeTemp.IsUnCheckRight = true;
                    qeTemp.QueryType = item.Type;
                    qeTemp.PropertyName = item.KeyName;
                    qeTemp.Operation = QueryExpression.Operations.IsChildFrom;
                    qeTemp.RightPropertyValue = flows.CreateList(flow => flow.FLOW_FLOWRECORDMASTER_T.FORMID);
                }
                return qeTemp;
            });
            return result;

        }
        #endregion

        #region 流程XML数据所需
        public string GetBorrowedMoney(EntityObject entity)
        {
            string ownerID = System.Convert.ToString(entity.GetValue(FieldName.OwnerID));
            decimal? bMoney = GetBorrowedMoney(ownerID);

            T_FB_BORROWAPPLYMASTER master = entity as T_FB_BORROWAPPLYMASTER;
            if (master != null)
            {
                bMoney = bMoney.Add(master.TOTALMONEY);
            }

            return Convert.ToString(bMoney.Add(0));
        }

        public string GetTravelMoney(EntityObject entity)
        {
            T_FB_SYSTEMSETTINGS setting = SystemBLL.GetSetting(null);
            string tranverlSubjectid = setting.TRANVERLSUBJECTID;
            decimal? tranvelMoney = 0;
            if (entity.GetType() == typeof(T_FB_BORROWAPPLYMASTER))
            {
                string id = entity.GetValue("BORROWAPPLYMASTERID").ToString();
                tranvelMoney = GetSumData<T_FB_BORROWAPPLYDETAIL>("T_FB_BORROWAPPLYMASTER.BORROWAPPLYMASTERID", id,
                    tranverlSubjectid, item => item.BORROWMONEY);

            }
            else if (entity.GetType() == typeof(T_FB_TRAVELEXPAPPLYMASTER))
            {
                string id = entity.GetValue("TRAVELEXPAPPLYMASTERID").ToString();
                tranvelMoney = GetSumData<T_FB_TRAVELEXPAPPLYDETAIL>("T_FB_TRAVELEXPAPPLYMASTER.TRAVELEXPAPPLYMASTERID", id,
                    tranverlSubjectid, item => item.TOTALCHARGE);

            }
            else if (entity.GetType() == typeof(T_FB_CHARGEAPPLYMASTER))
            {
                string id = entity.GetValue("CHARGEAPPLYMASTERID").ToString();
                tranvelMoney = GetSumData<T_FB_CHARGEAPPLYDETAIL>("T_FB_CHARGEAPPLYMASTER.CHARGEAPPLYMASTERID", id,
                    tranverlSubjectid, item => item.CHARGEMONEY);

            }
            else if (entity.GetType() == typeof(T_FB_REPAYAPPLYMASTER))
            {

                string id = entity.GetValue("REPAYAPPLYMASTERID").ToString();
                tranvelMoney = GetSumData<T_FB_REPAYAPPLYDETAIL>("T_FB_REPAYAPPLYMASTER.REPAYAPPLYMASTERID", id,
                    tranverlSubjectid, item => item.REPAYMONEY);

            }
            return Convert.ToString(tranvelMoney.Add(0));
        }

        public string GetEntertainmentMoney(EntityObject entity)
        {
            T_FB_SYSTEMSETTINGS setting = SystemBLL.GetSetting(null);
            string enSubjectid = setting.ENTERTAINMENTLSUBJECTID;
            decimal? enMoney = 0;
            if (entity.GetType() == typeof(T_FB_BORROWAPPLYMASTER))
            {
                string id = entity.GetValue("BORROWAPPLYMASTERID").ToString();
                enMoney = GetSumData<T_FB_BORROWAPPLYDETAIL>("T_FB_BORROWAPPLYMASTER.BORROWAPPLYMASTERID", id,
                    enSubjectid, item => item.BORROWMONEY);

            }
            else if (entity.GetType() == typeof(T_FB_TRAVELEXPAPPLYMASTER))
            {
                string id = entity.GetValue("TRAVELEXPAPPLYMASTERID").ToString();
                enMoney = GetSumData<T_FB_TRAVELEXPAPPLYDETAIL>("T_FB_TRAVELEXPAPPLYMASTER.TRAVELEXPAPPLYMASTERID", id,
                    enSubjectid, item => item.TOTALCHARGE);

            }
            else if (entity.GetType() == typeof(T_FB_CHARGEAPPLYMASTER))
            {
                string id = entity.GetValue("CHARGEAPPLYMASTERID").ToString();
                enMoney = GetSumData<T_FB_CHARGEAPPLYDETAIL>("T_FB_CHARGEAPPLYMASTER.CHARGEAPPLYMASTERID", id,
                    enSubjectid, item => item.CHARGEMONEY);

            }
            else if (entity.GetType() == typeof(T_FB_REPAYAPPLYMASTER))
            {
                string id = entity.GetValue("REPAYAPPLYMASTERID").ToString();
                enMoney = GetSumData<T_FB_REPAYAPPLYDETAIL>("T_FB_REPAYAPPLYMASTER.REPAYAPPLYMASTERID", id,
                    enSubjectid, item => item.REPAYMONEY);

            }
            return Convert.ToString(enMoney.Add(0));
        }

        public decimal? GetBorrowedMoney(string ownerID)
        {
            using (QueryEntityBLL bll = new QueryEntityBLL())
            {
                QueryExpression qe = QueryExpression.NotEqual("ISREPAIED", "1");
                qe = qe.And(FieldName.OwnerID, ownerID);
                qe = qe.And(FieldName.CheckStates, ((int)CheckStates.Approved).ToString());

                qe.QueryType = typeof(T_FB_BORROWAPPLYMASTER).Name;
                qe.IsUnCheckRight = true;
                qe.Include = new string[] { typeof(T_FB_BORROWAPPLYDETAIL).Name };

                var list = bll.InnerGetEntities<T_FB_BORROWAPPLYMASTER>(qe);

                decimal? borrorwedMoney = list.Sum(item => item.T_FB_BORROWAPPLYDETAIL.Sum(detail => detail.UNREPAYMONEY));
                return borrorwedMoney;
            }
        }

        public decimal? GetSumData<TDetail>(string masterIDMember, string id, string subjectID, Expression<Func<TDetail, decimal?>> Selector)
        {
            using (QueryEntityBLL bll = new QueryEntityBLL())
            {
                QueryExpression qeID = QueryExpression.Equal(masterIDMember, id);
                qeID = qeID.And("T_FB_SUBJECT.SUBJECTID", subjectID);

                qeID.QueryType = typeof(TDetail).Name;
                qeID.IsUnCheckRight = true;
                var details = bll.InnerGetEntities<TDetail>(qeID);
                decimal? sumMoney = details.Sum(Selector);

                return sumMoney;
            }
        }
        #endregion

        #region 新XML编码格式

        /// <summary>
        /// 构造提交给流程的新XML并对其填充数据
        /// </summary>
        /// <param name="fbEntity"></param>
        /// <returns></returns>
        public string GetAuditXmlForMobile(FBEntity fbEntity)
        {
            string xml = string.Empty;
            VirtualAudit auditEntity = fbEntity.Entity as VirtualAudit;
            List<EntityInfo> Modules = SubjectBLL.FBCommonEntityList;
            EntityInfo entityInfo = Modules.FirstOrDefault(entity =>
            {
                return entity.EntityCode == auditEntity.ModelCode;
            });

            if (entityInfo == null)
            {
                return xml;
            }
            xml = @"<?xml version=""1.0"" encoding=""utf-8""?><System><Name>FB</Name><Version>1.0</Version>{0}{1}{2}</System>";
            XElement xe = dictXML_XE[auditEntity.ModelCode.ToLower()].Element("Object");
            XElement xeCSys = dictXML_XE[auditEntity.ModelCode.ToLower()].Element("System");
            XElement xeMsg = dictXML_XE[auditEntity.ModelCode.ToLower()].Element("MsgOpen");
            xe = XElement.Parse(xe.ToString());  // 新建一个实例的XML。
            string StrSource = string.Empty;

            List<XElement> list = xe.Elements("Attribute").ToList();
            List<XElement> listChild = new List<XElement>();
            List<XElement> listGrandson = new List<XElement>();

            if (xe.Elements("ObjectList").Elements("Object") != null)
            {
                listChild = xe.Elements("ObjectList").ToList();

                if (xe.Elements("ObjectList").Elements("Object").Elements("ObjectList") != null)
                {
                    listGrandson = xe.Elements("ObjectList").Elements("Object").Elements("ObjectList").ToList();
                }
            }

            #region 填充XML一级节点
            EntityObject ent = fbEntity.ReferencedEntity[0].FBEntity.Entity;

            //预算预算，修改直接提交后fbEntity.ReferencedEntity[0].FBEntity.Entity的数据为没有更改的值，这时要加上修改的数据
            if (entityInfo.EntityCode == "T_FB_DEPTBUDGETAPPLYMASTER")//月度部门预算
            {
                #region 月度预算，修改直接提交后fbEntity.ReferencedEntity[0].FBEntity.Entity的数据为没有更改的值，这时要加上修改的数据

                var tempEntity = fbEntity.ReferencedEntity[0].FBEntity.CollectionEntity[0].FBEntities;
                if (tempEntity.Count > 0)
                {
                    tempEntity.ForEach(item =>
                        {
                            T_FB_DEPTBUDGETAPPLYDETAIL depDetail = item.Entity as T_FB_DEPTBUDGETAPPLYDETAIL;
                            QueryEntityBLL bll = new QueryEntityBLL();
                            QueryExpression qeDetail = QueryExpression.Equal("DEPTBUDGETAPPLYDETAILID", depDetail.DEPTBUDGETAPPLYDETAILID);
                            qeDetail.QueryType = "T_FB_DEPTBUDGETAPPLYDETAIL";
                            qeDetail.Include = new string[] { typeof(T_FB_SUBJECT).Name };
                            var dept = bll.InnerGetEntities<T_FB_DEPTBUDGETAPPLYDETAIL>(qeDetail).FirstOrDefault();//部门分配明细
                            depDetail.T_FB_SUBJECT = new T_FB_SUBJECT();
                            depDetail.T_FB_SUBJECT.SUBJECTCODE = dept.T_FB_SUBJECT.SUBJECTCODE;
                            depDetail.T_FB_SUBJECT.SUBJECTNAME = dept.T_FB_SUBJECT.SUBJECTNAME;
                            ((T_FB_DEPTBUDGETAPPLYMASTER)(ent)).T_FB_DEPTBUDGETAPPLYDETAIL.Add(depDetail);//手动加进去
                        });
                }
                #endregion
            }
            else if (entityInfo.EntityCode == "T_FB_DEPTBUDGETADDMASTER")//月度预算增补
            {
                #region 月度预算增补
                var tempEntity = fbEntity.ReferencedEntity[0].FBEntity.CollectionEntity[0].FBEntities;
                if (tempEntity.Count > 0)
                {
                    tempEntity.ForEach(item =>
                    {
                        SMT_FB_EFModel.T_FB_DEPTBUDGETADDDETAIL depDetail = item.Entity as T_FB_DEPTBUDGETADDDETAIL;
                        ((SMT_FB_EFModel.T_FB_DEPTBUDGETADDMASTER)(ent)).T_FB_DEPTBUDGETADDDETAIL.Add(depDetail);
                    });
                }
                #endregion
            }
            else if (entityInfo.EntityCode == "T_FB_COMPANYBUDGETMODMASTER")
            {
                #region 年度增补
                var tempEntity = fbEntity.ReferencedEntity[0].FBEntity.CollectionEntity[0].FBEntities;
                if (tempEntity.Count > 0)
                {
                    tempEntity.ForEach(item =>
                    {
                        SMT_FB_EFModel.T_FB_COMPANYBUDGETMODDETAIL depDetail = item.Entity as T_FB_COMPANYBUDGETMODDETAIL;
                        ((SMT_FB_EFModel.T_FB_COMPANYBUDGETMODMASTER)(ent)).T_FB_COMPANYBUDGETMODDETAIL.Add(depDetail);
                    });
                }
                #endregion
            }
            else if (entityInfo.EntityCode == "T_FB_COMPANYBUDGETAPPLYMASTER")
            {
                #region 年度预算
                var tempEntity = fbEntity.ReferencedEntity[0].FBEntity.CollectionEntity[0].FBEntities;
                if (tempEntity.Count > 0)
                {
                    tempEntity.ForEach(item =>
                    {
                        T_FB_COMPANYBUDGETAPPLYDETAIL compDetail = item.Entity as T_FB_COMPANYBUDGETAPPLYDETAIL;
                        QueryEntityBLL bll = new QueryEntityBLL();
                        QueryExpression qeDetail = QueryExpression.Equal("COMPANYBUDGETAPPLYDETAILID", compDetail.COMPANYBUDGETAPPLYDETAILID);
                        qeDetail.QueryType = "T_FB_COMPANYBUDGETAPPLYDETAIL";
                        qeDetail.Include = new string[] { typeof(T_FB_SUBJECT).Name };
                        var comp = bll.InnerGetEntities<T_FB_COMPANYBUDGETAPPLYDETAIL>(qeDetail).FirstOrDefault();//部门分配明细
                        compDetail.T_FB_SUBJECT = new T_FB_SUBJECT();
                        compDetail.T_FB_SUBJECT.SUBJECTCODE = comp.T_FB_SUBJECT.SUBJECTCODE;
                        compDetail.T_FB_SUBJECT.SUBJECTNAME = comp.T_FB_SUBJECT.SUBJECTNAME;
                        ((T_FB_COMPANYBUDGETAPPLYMASTER)(ent)).T_FB_COMPANYBUDGETAPPLYDETAIL.Add(compDetail);//手动加进去
                    });
                }
                #endregion
            }
            #region 部门分配没有去到字表数据，这里组一下
            if (entityInfo.EntityCode == "T_FB_DEPTTRANSFERMASTER")
            {
                QueryEntityBLL bll = new QueryEntityBLL();
                QueryExpression qeDetail = QueryExpression.Equal("DEPTTRANSFERMASTERID", ent.EntityKey.EntityKeyValues[0].Value.ToString());
                qeDetail.QueryType = "T_FB_DEPTTRANSFERMASTER";
                qeDetail.Include = new string[] { typeof(T_FB_DEPTTRANSFERDETAIL).Name };
                var deptDetail = bll.InnerGetEntities<T_FB_DEPTTRANSFERMASTER>(qeDetail).FirstOrDefault();///部门分配明细
                foreach (var item in deptDetail.T_FB_DEPTTRANSFERDETAIL)
                {
                    QueryExpression qePer = QueryExpression.Equal("DEPTTRANSFERDETAILID", item.DEPTTRANSFERDETAILID);
                    qePer.QueryType = "T_FB_DEPTTRANSFERDETAIL";
                    qePer.Include = new string[] { typeof(T_FB_PERSONTRANSFERDETAIL).Name };
                    var perDetail = bll.InnerGetEntities<T_FB_DEPTTRANSFERDETAIL>(qePer).FirstOrDefault();///部门分配给个人的明细
                    perDetail.T_FB_PERSONTRANSFERDETAIL.ForEach(it =>
                    {
                        item.T_FB_PERSONTRANSFERDETAIL.Add(it);
                    });
                }

                ent = (EntityObject)deptDetail;
            }
            #endregion
            string strFirstKeyName = xe.Attribute("Key").Value;
            if (!string.IsNullOrWhiteSpace(strFirstKeyName))
            {
                xe.Attribute("id").SetValue(ent.GetValue(strFirstKeyName));
            }

            SMT.SaaS.BLLCommonServices.PersonnelWS.PersonnelServiceClient psl = new SaaS.BLLCommonServices.PersonnelWS.PersonnelServiceClient();

            #region foreach遍历取到xml相应的值
            foreach (XElement item in list)
            {
                string AttName = string.Empty, AttValue = string.Empty, StrCheck = string.Empty;
                AttName = item.Attribute("Name").Value;

                if (AttName == "OrderTypeName")
                {
                    AttValue = item.Attribute("DataValue").Value;
                    StrCheck = item.Attribute("DataText").Value;
                }
                else if (AttName == "CHECKSTATES")
                {
                    AttValue = "1";
                    StrCheck = "审核中";
                }
                else if (AttName == "EDITSTATES")
                {
                    object oEditStates = ent.GetValue("EDITSTATES");
                    if (oEditStates != null)
                    {
                        switch (oEditStates.ToString())
                        {
                            case "0":
                                AttValue = "0";
                                StrCheck = "未生效";
                                break;
                            case "1":
                                AttValue = "1";
                                StrCheck = "已生效";
                                break;
                            default:
                                AttValue = oEditStates.ToString();
                                StrCheck = "未生效";
                                break;
                        }
                    }
                }
                else if (AttName == "ISVALID")
                {
                    object oCheckStates = ent.GetValue("ISVALID");
                    if (oCheckStates != null)
                    {
                        switch (oCheckStates.ToString())
                        {
                            case "0":
                                AttValue = "0";
                                StrCheck = "未汇总";
                                break;
                            case "1":
                                AttValue = "1";
                                StrCheck = "生效";
                                break;
                            case "2":
                                AttValue = "2";
                                StrCheck = "未生效";
                                break;
                        }
                    }
                }
                else if (AttName == "CURRENTEMPLOYEENAME")
                {
                    object objOWNERID = ent.GetValue("OWNERID");
                    object objOWNERName = ent.GetValue("OWNERNAME");
                    string strCurrEmployeeName = string.Empty;

                    if (objOWNERName != null)
                    {
                        strCurrEmployeeName = objOWNERName.ToString();
                    }

                    if (string.IsNullOrWhiteSpace(strCurrEmployeeName))
                    {
                        if (objOWNERID != null)
                        {
                            SMT.SaaS.BLLCommonServices.PersonnelWS.T_HR_EMPLOYEE epd = psl.GetEmployeeByID(objOWNERID.ToString());
                            if (epd != null)
                            {
                                strCurrEmployeeName = epd.EMPLOYEECNAME;
                            }
                            else
                            {
                                strCurrEmployeeName = objOWNERID.ToString();
                            }
                        }
                    }

                    AttValue = strCurrEmployeeName;
                }
                else if (AttName == "POSTLEVEL")
                {
                    object objOWNERPOSTID = ent.GetValue("OWNERPOSTID");
                    object objOWNERID = ent.GetValue("OWNERID");
                    if (objOWNERPOSTID != null && objOWNERID != null)
                    {
                        SMT.SaaS.BLLCommonServices.PersonnelWS.V_EMPLOYEEDETAIL epd = psl.GetEmployeeDetailViewByID(objOWNERID.ToString());
                        SMT.SaaS.BLLCommonServices.PersonnelWS.V_EMPLOYEEPOSTBRIEF vPost = epd.EMPLOYEEPOSTS.Where(c => c.POSTID == objOWNERPOSTID.ToString()).FirstOrDefault();

                        if (vPost != null)
                        {
                            AttValue = vPost.POSTLEVEL == null ? "0" : vPost.POSTLEVEL.Value.ToString();
                        }
                        else
                        {
                            AttValue = "0"; //针对岗位作废时处理：出现此情况的原因是人员岗位异动前，单据已提交审核中，异动完毕，原岗位作废
                        }
                        StrCheck = AttValue;
                    }
                }
                else if (AttName == "BorrowedMoney")
                {
                    AttValue = GetBorrowedMoney(ent);
                    StrCheck = AttValue;
                }
                else if (AttName == "TravelMoney")
                {
                    AttValue = GetTravelMoney(ent);
                    StrCheck = AttValue;
                }
                else if (AttName == "EntertainmentMoney")
                {
                    AttValue = GetEntertainmentMoney(ent);
                    StrCheck = AttValue;
                }
                else if (AttName == "TRANSFERFROM")
                {
                    AttValue = GetTransferDepartemntName(ent, AttName);
                }
                else if (AttName == "TRANSFERTO")
                {
                    AttValue = GetTransferDepartemntName(ent, AttName);
                }
                else if (AttName == "BUDGETARYMONTH")
                {
                    object objValue = ent.GetValue(AttName);
                    if (objValue != null)
                    {
                        DateTime dt = Convert.ToDateTime(objValue.ToString());
                        AttValue = dt.Year.ToString() + "年" + dt.Month + "月";
                    }
                }
                else
                {
                    object objValue = ent.GetValue(AttName);
                    if (objValue != null)
                    {
                        AttValue = objValue.ToString();
                    }
                }

                item.Attribute("DataValue").SetValue(AttValue);
                if (!string.IsNullOrWhiteSpace(StrCheck))
                {
                    item.Attribute("DataText").SetValue(StrCheck);
                }
            };
            #endregion             
            #endregion
            #region try-catch的try
            try
            {
                //年度汇总
                if (entityInfo.EntityCode == "T_FB_COMPANYBUDGETSUMMASTER")
                {
                    string strCompanyID = ((SMT_FB_EFModel.T_FB_COMPANYBUDGETSUMMASTER)(ent)).OWNERCOMPANYID;
                    if (strCompanyID != "7a613fc2-4431-4a46-ae01-232222e9fcb5")//物流公司
                    {
                        //填充二级节点
                        List<EntityObject> listChildEnts = new List<EntityObject>();
                        ResetAndFillDataSecondNode(ent, xe, listChild, listGrandson, ref listChildEnts);

                        if (listGrandson.Count() > 0)
                        {
                            //三级节点
                            ResetAndFillDataThirdNode(xe, listChildEnts, listGrandson);
                        }
                    }
                }
                //月度汇总
                else if (entityInfo.EntityCode == "T_FB_DEPTBUDGETSUMMASTER")
                {
                    string strCompanyID = ((SMT_FB_EFModel.T_FB_DEPTBUDGETSUMMASTER)(ent)).OWNERCOMPANYID;
                    if (strCompanyID != "7a613fc2-4431-4a46-ae01-232222e9fcb5")//物流公司
                    {
                        //填充二级节点
                        List<EntityObject> listChildEnts = new List<EntityObject>();
                        ResetAndFillDataSecondNode(ent, xe, listChild, listGrandson, ref listChildEnts);

                        if (listGrandson.Count() > 0)
                        {
                            //三级节点
                            ResetAndFillDataThirdNode(xe, listChildEnts, listGrandson);
                        }
                    }
                }
                else if(entityInfo.EntityCode != "T_FB_COMPANYBUDGETSUMMASTER" && entityInfo.EntityCode != "T_FB_DEPTBUDGETSUMMASTER")
                {
                    //填充二级节点
                    List<EntityObject> listChildEnts = new List<EntityObject>();
                    ResetAndFillDataSecondNode(ent, xe, listChild, listGrandson, ref listChildEnts);

                    if (listGrandson.Count() > 0)
                    {
                        //三级节点
                        ResetAndFillDataThirdNode(xe, listChildEnts, listGrandson);
                    }
                }

                StrSource = string.Format(xml, xeCSys.ToString(), xeMsg.ToString(), xe.ToString());
            }
            #endregion
            catch (Exception ex)
            {
                if (entityInfo.EntityCode != "T_FB_COMPANYBUDGETSUMMASTER" && entityInfo.EntityCode != "T_FB_DEPTBUDGETSUMMASTER")
                {
                    //填充二级节点
                    List<EntityObject> listChildEnts = new List<EntityObject>();
                    ResetAndFillDataSecondNode(ent, xe, listChild, listGrandson, ref listChildEnts);

                    if (listGrandson.Count() > 0)
                    {
                        //三级节点
                        ResetAndFillDataThirdNode(xe, listChildEnts, listGrandson);
                    }
                }
                StrSource = string.Format(xml, xeCSys.ToString(), xeMsg.ToString(), xe.ToString());
            }
            return StrSource;
        }

        /// <summary>
        /// 获取下拨人员所在的公司名称(废弃，下拨公司名称已直接记录到主表中，不再从子表去寻找)
        /// </summary>
        /// <param name="ent"></param>
        /// <param name="AttName"></param>
        /// <returns></returns>
        private void GetPersonAssignCompanyName(EntityObject ent, ref string AttValue)
        {
            T_FB_PERSONMONEYASSIGNMASTER entMaster = ent as T_FB_PERSONMONEYASSIGNMASTER;
            if (entMaster == null)
            {
                return;
            }

            if (entMaster.T_FB_PERSONMONEYASSIGNDETAIL == null)
            {
                return;
            }

            if (entMaster.T_FB_PERSONMONEYASSIGNDETAIL.Count() == 0)
            {
                return;
            }

            AttValue = entMaster.T_FB_PERSONMONEYASSIGNDETAIL.FirstOrDefault().OWNERCOMPANYNAME;
        }

        /// <summary>
        /// 获取调拨部门名称
        /// </summary>
        /// <param name="ent"></param>
        /// <returns></returns>
        private string GetTransferDepartemntName(EntityObject ent, string strKeyName)
        {
            string strRes = string.Empty;
            object objValue = ent.GetValue(strKeyName);
            if (objValue != null)
            {
                string strDepartemntID = objValue.ToString();
                if (!string.IsNullOrWhiteSpace(strDepartemntID))
                {
                    SMT.SaaS.BLLCommonServices.OrganizationWS.OrganizationServiceClient osl = new SaaS.BLLCommonServices.OrganizationWS.OrganizationServiceClient();
                    SMT.SaaS.BLLCommonServices.OrganizationWS.T_HR_DEPARTMENT dpt = osl.GetDepartmentById(strDepartemntID);
                    if (dpt != null)
                    {
                        strRes = dpt.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
                    }
                }
            }
            return strRes;
        }

        /// <summary>
        /// 填充第二级节点数据
        /// </summary>
        /// <param name="fbEntity"></param>
        /// <param name="xe"></param>
        /// <param name="listChild"></param>
        /// <param name="listGrandSon"></param>
        /// <param name="listChildEnts"></param>
        private void ResetAndFillDataSecondNode(EntityObject masterEnt, XElement xe, List<XElement> listChild, List<XElement> listGrandSon, ref List<EntityObject> listChildEnts)
        {
            if (listChild.Count() <= 0)
            {
                return;
            }

            XElement xFirst = xe.Elements("ObjectList").Elements("Object").FirstOrDefault();
            if (xFirst.Attribute("Name") == null)
            {
                return;
            }

            string strXItemName = xFirst.Attribute("Name").Value.ToString();
            GetAuditEntityList(masterEnt, strXItemName, ref listChildEnts);
            if (listChildEnts.Count() <= 0)
            {
                return;
            }
            try
            {
                //var tempEntity = listChildEnts.FirstOrDefault();
                //为了按照科目编号排序
                listChildEnts = OrderBudget(listChildEnts);
            }
            catch (Exception ex)
            {
                Tracer.Debug("排序出错"+ex.ToString());
            }
            finally
            {
                CreateNewNode(xFirst, listChild, listGrandSon, listChildEnts.Count());
                FillData(listChild, listChildEnts);
            }
           
        }

        /// <summary>
        ///把数据按照科目编号进行排序，并去掉预算为0的数据，这里应该还有更好的方法，例如泛型
        /// </summary>
        /// <param name="listChildEnts"></param>
        /// <returns></returns>
        private List<EntityObject> OrderBudget(List<EntityObject> listChildEnts)
        {
            try
            {
                List<EntityObject> listTemp = new List<EntityObject>();
                switch (listChildEnts.FirstOrDefault().GetType().ToString())
                {
                    case "SMT_FB_EFModel.T_FB_COMPANYBUDGETAPPLYDETAIL"://年度预算申请明细
                        #region
                        listChildEnts.ForEach(it =>
                        {
                            var entity = it as T_FB_COMPANYBUDGETAPPLYDETAIL;
                            if (entity.BUDGETMONEY > 0)
                            {
                                listTemp.Add(it);
                            }
                        });
                        listChildEnts = listTemp.OrderBy(item => ((T_FB_COMPANYBUDGETAPPLYDETAIL)(item)).T_FB_SUBJECT.SUBJECTCODE).ToList();
                        #endregion
                        break;
                    case "SMT_FB_EFModel.T_FB_COMPANYBUDGETMODDETAIL"://年度增补明细
                        #region
                        QueryEntityBLL bll = new QueryEntityBLL();
                        listChildEnts.ForEach(it =>
                        {
                            T_FB_COMPANYBUDGETMODDETAIL ent = it as T_FB_COMPANYBUDGETMODDETAIL;
                            QueryExpression qeSub = QueryExpression.Equal("T_FB_SUBJECT.SUBJECTID", ent.T_FB_SUBJECT.SUBJECTID); // 总帐表科目ID
                            qeSub.QueryType = typeof(T_FB_BUDGETACCOUNT).Name;
                            QueryExpression qeDeptID = QueryExpression.Equal("OWNERDEPARTMENTID", ent.OWNERDEPARTMENTID); // 总帐表科目ID
                            QueryExpression qeYear = QueryExpression.Equal("BUDGETYEAR", DateTime.Now.Year.ToString()); // 总帐表年份
                            QueryExpression qeType = QueryExpression.Equal("ACCOUNTOBJECTTYPE", "1"); // 预算类型为公司
                            qeYear.RelatedExpression = qeType;
                            qeDeptID.RelatedExpression = qeYear;
                            qeSub.RelatedExpression = qeDeptID;
                            var baData = bll.InnerGetEntities<T_FB_BUDGETACCOUNT>(qeSub).FirstOrDefault();
                            if (baData != null)
                            {
                                decimal? money = baData.BUDGETMONEY;
                                if (money > 0)
                                {
                                    ent.AUDITBUDGETMONEY = money;//该字段在年度增补里面没有使用，所以在这里进行后面的xml数据生成使用
                                }
                                else if (money == 0)
                                {
                                    ent.AUDITBUDGETMONEY = baData.PAIEDMONEY;
                                }
                            }
                            if (ent.BUDGETMONEY > 0)
                            {
                                listTemp.Add(it);
                            }
                        });
                        listChildEnts = listTemp.OrderBy(item => ((T_FB_COMPANYBUDGETMODDETAIL)(item)).T_FB_SUBJECT.SUBJECTCODE).ToList();
                        #endregion
                        break;
                    case "SMT.FB.BLL.V_SubjectCompanySum"://年度预算汇总明细
                        #region
                        listChildEnts.ForEach(it =>
                        {
                            var entity = it as V_SubjectCompanySum;
                            if (entity.T_FB_COMPANYBUDGETAPPLYDETAIL != null && entity.T_FB_COMPANYBUDGETAPPLYDETAIL.Count > 0 && entity.T_FB_COMPANYBUDGETAPPLYDETAIL.FirstOrDefault().BUDGETMONEY > 0)
                            {
                                listTemp.Add(it);
                            }
                        });
                        listChildEnts = listTemp.OrderBy(item => ((V_SubjectCompanySum)(item)).T_FB_SUBJECT.SUBJECTCODE).ToList();
                        #endregion
                        break;
                    case "SMT.FB.BLL.V_SubjectDeptSum"://部门预算汇总明细
                        listChildEnts = listChildEnts.OrderBy(item => ((V_SubjectDeptSum)(item)).T_FB_SUBJECT.SUBJECTCODE).ToList();
                        listChildEnts.ForEach(it =>
                        {
                            var entity = it as V_SubjectDeptSum;
                            if (entity.T_FB_DEPTBUDGETAPPLYDETAIL != null && entity.T_FB_DEPTBUDGETAPPLYDETAIL.Count > 0 && entity.T_FB_DEPTBUDGETAPPLYDETAIL.FirstOrDefault().TOTALBUDGETMONEY > 0)
                            {
                                listTemp.Add(it);
                            }
                        });
                        listChildEnts = listTemp;
                        break;
                    case "SMT_FB_EFModel.T_FB_DEPTBUDGETAPPLYDETAIL": //部门预算申请明细
                        listChildEnts = listChildEnts.OrderBy(item => ((T_FB_DEPTBUDGETAPPLYDETAIL)(item)).T_FB_SUBJECT.SUBJECTCODE).ToList();
                        listChildEnts.ForEach(it =>
                        {
                            var entity = it as T_FB_DEPTBUDGETAPPLYDETAIL;
                            if (entity != null && entity.TOTALBUDGETMONEY > 0)
                            {
                                listTemp.Add(it);
                            }
                        });
                        listChildEnts = listTemp;
                        break;
                    case "SMT_FB_EFModel.T_FB_DEPTBUDGETADDDETAIL"://部门预算增补明细
                        listChildEnts.ForEach(it =>
                        {
                            var entity = it as T_FB_DEPTBUDGETADDDETAIL;
                            if (entity != null && entity.TOTALBUDGETMONEY > 0)
                            {
                                listTemp.Add(it);
                            }
                        });
                        listChildEnts = listTemp.OrderBy(item => ((T_FB_DEPTBUDGETADDDETAIL)(item)).T_FB_SUBJECT.SUBJECTCODE).ToList();
                        break;
                    default: break;
                }
                return listChildEnts;
            }
            catch
            {
                return listChildEnts;
            }

        }


        /// <summary>
        /// 填充第三级节点
        /// </summary>
        /// <param name="xe"></param>
        /// <param name="listChildEnts"></param>
        /// <param name="listChild"></param>
        /// <param name="listGrandson"></param>
        private void ResetAndFillDataThirdNode(XElement xe, List<EntityObject> listChildEnts, List<XElement> listGrandson)
        {
            if (xe == null || listChildEnts.Count() <= 0 || listGrandson.Count() == 0)
            {
                return;
            }

            List<XElement> listChild = xe.Elements("ObjectList").Elements("Object").ToList();
            List<EntityObject> listGrandSonEnts = new List<EntityObject>();
            foreach (XElement xitem in listChild)
            {
                string xcIDName = xitem.Attribute("Key").Value;
                string xcIDValue = xitem.Attribute("id").Value;
                if (string.IsNullOrWhiteSpace(xcIDName) || string.IsNullOrWhiteSpace(xcIDValue))
                {
                    continue;
                }

                foreach (EntityObject item in listChildEnts)
                {
                    object entIDValue = null;
                    if (xcIDName == "COMPANYBUDGETSUMDETAILID")
                    {
                        entIDValue = ((V_SubjectCompanySum)item).T_FB_SUBJECT.SUBJECTID;
                    }
                    else if (xcIDName == "DEPTBUDGETSUMDETAILID")
                    {
                        entIDValue = ((V_SubjectDeptSum)item).T_FB_SUBJECT.SUBJECTID;
                    }
                    else
                    {
                        entIDValue = item.GetValue(xcIDName);
                        if (entIDValue == null)
                        {
                            entIDValue = string.Empty;
                        }
                    }


                    if (xcIDValue != entIDValue.ToString())
                    {
                        continue;
                    }

                    if (xitem.Elements("ObjectList") != null)
                    {
                        IEnumerable<XElement> xcElementList = xitem.Elements("ObjectList");
                        foreach (XElement xcElement in xcElementList)
                        {
                            string parentName = xcElement.Attribute("ParentName").Value;
                            if (xcIDName != parentName)
                            {
                                continue;
                            }

                            xcElement.Attribute("ParentID").SetValue(xcIDValue);
                        }
                    }

                    if (xitem.Elements("ObjectList").Elements("Object") != null)
                    {
                        XElement xcitem = xitem.Elements("ObjectList").Elements("Object").FirstOrDefault();

                        if (xcitem.Attribute("Name") == null)
                        {
                            continue;
                        }

                        string strXItemName = xcitem.Attribute("Name").Value.ToString();

                        listGrandSonEnts.Clear();

                        if (xcIDName == "COMPANYBUDGETSUMDETAILID")
                        {
                            listGrandSonEnts.AddRange(((V_SubjectCompanySum)item).T_FB_COMPANYBUDGETAPPLYDETAIL);
                        }
                        else if (xcIDName == "DEPTBUDGETSUMDETAILID")
                        {
                            listGrandSonEnts.AddRange(((V_SubjectDeptSum)item).T_FB_DEPTBUDGETAPPLYDETAIL);
                        }
                        else
                        {
                            GetAuditEntityList(item, strXItemName, ref listGrandSonEnts);
                        }
                        List<EntityObject> listTemp = new List<EntityObject>();
                        //如果是月度预算申请中分配的人员分配则排除掉分配为0的数据
                        if (listGrandSonEnts != null && listGrandSonEnts.Count > 0 && listGrandSonEnts.FirstOrDefault().GetType().ToString() == "SMT_FB_EFModel.T_FB_PERSONBUDGETAPPLYDETAIL")
                        {
                            listGrandSonEnts.ForEach(it =>
                                {
                                    var entity = it as T_FB_PERSONBUDGETAPPLYDETAIL;
                                    if (entity != null && entity.BUDGETMONEY > 0)
                                    {
                                        listTemp.Add(it);
                                    }
                                });
                            listGrandSonEnts = listTemp;
                        }

                        if (listGrandSonEnts != null && listGrandSonEnts.Count() <= 0)
                        {
                            RemoveUnvailableNode(xitem, strXItemName);
                            continue;
                        }

                        List<XElement> listCurs = xitem.Elements("ObjectList").ToList();
                        CreateNewNode(xcitem, listCurs, listGrandSonEnts.Count());
                        FillData(listCurs, listGrandSonEnts);
                    }
                }
            }
        }

        /// <summary>
        /// 根据传入的实体，及实体的子实体名，返回实体关联的子实体集
        /// </summary>
        /// <param name="entityObject"></param>
        /// <param name="strXItemName"></param>
        /// <param name="listAllEnts"></param>
        private void GetAuditEntityList(EntityObject entityObject, string strXItemName, ref List<EntityObject> listAllEnts)
        {
            if (string.IsNullOrWhiteSpace(strXItemName))
            {
                return;
            }

            if (entityObject.EntityKey.EntitySetName == "T_FB_COMPANYBUDGETSUMMASTER" && strXItemName == "T_FB_COMPANYBUDGETSUMDETAIL")
            {
                GetAuditCompanyBudgetSumDetails(entityObject, ref listAllEnts);
                return;
            }

            if (entityObject.EntityKey.EntitySetName == "T_FB_DEPTBUDGETSUMMASTER" && strXItemName == "T_FB_DEPTBUDGETSUMDETAIL")
            {
                GetAuditDeptBudgetSumDetails(entityObject, ref listAllEnts);
                return;
            }

            if (entityObject.GetType().Name == "T_FB_PERSONMONEYASSIGNMASTER" && strXItemName == "T_FB_PERSONMONEYASSIGNDETAIL")
            {
                GetAuditPersonMoneyAssignDetails(entityObject, strXItemName, ref listAllEnts);
                return;
            }

            var rs = (entityObject as IEntityWithRelationships).RelationshipManager.GetAllRelatedEnds();
            
            foreach (IRelatedEnd re in rs)
            {
                IEnumerator etrs = re.GetEnumerator();
                while (etrs.MoveNext())
                {
                    EntityObject obj = etrs.Current as EntityObject;

                    if (obj.GetType().Name != strXItemName)
                    {
                        continue;
                    }
                    listAllEnts.Add(obj);
                }
            }
        }

        private void GetAuditPersonMoneyAssignDetails(EntityObject entityObject, string strXItemName, ref List<EntityObject> listAllEnts)
        {
            using (SubjectBLL bll = new SubjectBLL())
            {
                T_FB_PERSONMONEYASSIGNMASTER entMaster = entityObject as T_FB_PERSONMONEYASSIGNMASTER;
                FBEntity entCur = bll.GetFBEntityByEntityKey(entMaster.EntityKey);

                var rs = (entCur.Entity as IEntityWithRelationships).RelationshipManager.GetAllRelatedEnds();

                foreach (IRelatedEnd re in rs)
                {
                    IEnumerator etrs = re.GetEnumerator();
                    while (etrs.MoveNext())
                    {
                        EntityObject obj = etrs.Current as EntityObject;

                        if (obj.GetType().Name != strXItemName)
                        {
                            continue;
                        }
                        listAllEnts.Add(obj);
                    }
                }
            }
        }

        /// <summary>
        /// 针对月度汇总需要填充数据的移动XML结构，构造自定义实体并填充数据
        /// </summary>
        /// <param name="entityObject"></param>
        /// <param name="listAllEnts"></param>
        private void GetAuditDeptBudgetSumDetails(EntityObject entityObject, ref List<EntityObject> listAllEnts)
        {
            List<EntityObject> listTemp = new List<EntityObject>();
            T_FB_DEPTBUDGETSUMMASTER entMaster = entityObject as T_FB_DEPTBUDGETSUMMASTER;
            if (entMaster.T_FB_DEPTBUDGETSUMDETAIL == null)
            {
                return;
            }

            if (entMaster.T_FB_DEPTBUDGETSUMDETAIL.Count() == 0)
            {
                return;
            }


            foreach (T_FB_DEPTBUDGETSUMDETAIL item in entMaster.T_FB_DEPTBUDGETSUMDETAIL)
            {
                T_FB_DEPTBUDGETAPPLYMASTER entApplyMaster = item.T_FB_DEPTBUDGETAPPLYMASTER;
                if (entApplyMaster == null)
                {
                    continue;
                }

                if (entApplyMaster.T_FB_DEPTBUDGETAPPLYDETAIL == null)
                {
                    continue;
                }

                if (entApplyMaster.T_FB_DEPTBUDGETAPPLYDETAIL.Count() == 0)
                {
                    continue;
                }

                List<T_FB_DEPTBUDGETAPPLYDETAIL> listDetail = entApplyMaster.T_FB_DEPTBUDGETAPPLYDETAIL.ToList();

                var detailSum = from d in listDetail
                                group d by d.T_FB_SUBJECT into p
                                select new V_SubjectDeptSum
                                {
                                    T_FB_SUBJECT = p.Key,
                                    BUDGETMONEY = p.Sum(sumItem =>Convert.ToDecimal(sumItem.TOTALBUDGETMONEY)),
                                    T_FB_DEPTBUDGETAPPLYDETAIL = listDetail.Where(de => de.T_FB_SUBJECT == p.Key).ToList()
                                };
                listTemp.AddRange(detailSum);
            }
            List<V_SubjectDeptSum> listSub = new List<V_SubjectDeptSum>();
            listTemp.ForEach(it =>
            {
                listSub.Add(it as V_SubjectDeptSum);
            });
            var gruopList = listSub.GroupBy(t => t.T_FB_SUBJECT.SUBJECTID).OrderBy(t => t.Key);//根据科目分组
            List<V_SubjectDeptSum> listSubs = new List<V_SubjectDeptSum>();
            gruopList.ForEach(it =>
                {
                    if (it != null && it.Count() > 0)
                    {
                        it.ForEach(t =>
                            {
                                it.FirstOrDefault().T_FB_DEPTBUDGETAPPLYDETAIL.Add(t.T_FB_DEPTBUDGETAPPLYDETAIL.FirstOrDefault());//把相同科目的明细放在一起
                            });
                    }
                    it.FirstOrDefault().T_FB_DEPTBUDGETAPPLYDETAIL.RemoveAt(0);//第一个多加了一次
                    decimal totalMoney=0;
                    it.FirstOrDefault().T_FB_DEPTBUDGETAPPLYDETAIL.ForEach(m =>
                        {
                            totalMoney +=Convert.ToDecimal(m.TOTALBUDGETMONEY);
                        });
                    it.FirstOrDefault().BUDGETMONEY = totalMoney;//总金额等于每个明细金额之和
                    listSubs.Add(it.FirstOrDefault());
                });
            listAllEnts.AddRange(listSubs);
        }

        /// <summary>
        /// 针对年度汇总需要填充数据的移动XML结构，构造自定义实体并填充数据
        /// </summary>
        /// <param name="entityObject"></param>
        /// <param name="listAllEnts"></param>
        private void GetAuditCompanyBudgetSumDetails(EntityObject entityObject, ref List<EntityObject> listAllEnts)
        {
            List<EntityObject> listTemp = new List<EntityObject>();
            T_FB_COMPANYBUDGETSUMMASTER entMaster = entityObject as T_FB_COMPANYBUDGETSUMMASTER;
            if (entMaster.T_FB_COMPANYBUDGETSUMDETAIL == null)
            {
                return;
            }

            if (entMaster.T_FB_COMPANYBUDGETSUMDETAIL.Count() == 0)
            {
                return;
            }


            foreach (T_FB_COMPANYBUDGETSUMDETAIL item in entMaster.T_FB_COMPANYBUDGETSUMDETAIL)
            {
                T_FB_COMPANYBUDGETAPPLYMASTER entApplyMaster = item.T_FB_COMPANYBUDGETAPPLYMASTER;
                if (entApplyMaster == null)
                {
                    continue;
                }

                if (entApplyMaster.T_FB_COMPANYBUDGETAPPLYDETAIL == null)
                {
                    continue;
                }

                if (entApplyMaster.T_FB_COMPANYBUDGETAPPLYDETAIL.Count() == 0)
                {
                    continue;
                }

                List<T_FB_COMPANYBUDGETAPPLYDETAIL> listDetail = entApplyMaster.T_FB_COMPANYBUDGETAPPLYDETAIL.ToList();

                var detailSum = from d in listDetail
                                group d by d.T_FB_SUBJECT into p
                                select new V_SubjectCompanySum
                                {
                                    T_FB_SUBJECT = p.Key,
                                    BUDGETMONEY = p.Sum(sumItem => sumItem.BUDGETMONEY),
                                    T_FB_COMPANYBUDGETAPPLYDETAIL = listDetail.Where(de => de.T_FB_SUBJECT == p.Key).ToList()
                                };

                listTemp.AddRange(detailSum);
            }
            List<V_SubjectCompanySum> listSub = new List<V_SubjectCompanySum>();
            listTemp.ForEach(it =>
            {
                listSub.Add(it as V_SubjectCompanySum);
            });
            var gruopList = listSub.GroupBy(t => t.T_FB_SUBJECT.SUBJECTID).OrderBy(t => t.Key);//根据科目分组
            List<V_SubjectCompanySum> listSubs = new List<V_SubjectCompanySum>();
            gruopList.ForEach(it =>
            {
                if (it != null && it.Count() > 0)
                {
                    it.ForEach(t =>
                    {
                        it.FirstOrDefault().T_FB_COMPANYBUDGETAPPLYDETAIL.Add(t.T_FB_COMPANYBUDGETAPPLYDETAIL.FirstOrDefault());//把相同科目的明细放在一起
                    });
                }
                it.FirstOrDefault().T_FB_COMPANYBUDGETAPPLYDETAIL.RemoveAt(0);//第一个多加了一次
                decimal totalMoney = 0;
                it.FirstOrDefault().T_FB_COMPANYBUDGETAPPLYDETAIL.ForEach(m =>
                {
                    totalMoney += Convert.ToDecimal(m.BUDGETMONEY);
                });
                it.FirstOrDefault().BUDGETMONEY = totalMoney;//总金额等于每个明细金额之和
                listSubs.Add(it.FirstOrDefault());
            });
            listAllEnts.AddRange(listSubs);
        }

        /// <summary>
        /// 清理无用的三级节点
        /// </summary>
        /// <param name="xParentItem"></param>
        /// <param name="strXItemName"></param>
        private void RemoveUnvailableNode(XElement xParentItem, string strXItemName)
        {
            List<XElement> listChildNodes = xParentItem.Elements("ObjectList").Elements("Object").ToList();
            foreach (XElement xnode in listChildNodes)
            {
                if (xnode.Attribute("Name") == null)
                {
                    continue;
                }

                if (xnode.Attribute("Name").Value != strXItemName)
                {
                    continue;
                }

                xnode.Remove();
            }

            List<XElement> listChild = xParentItem.Elements("ObjectList").ToList();

            foreach (XElement xitem in listChild)
            {

                List<XElement> listCurNodes = new List<XElement>();
                if (xitem.Elements("Object") != null)
                {
                    listCurNodes = xitem.Elements("Object").ToList();
                }

                if (listCurNodes.Count() == 0)
                {
                    xitem.Remove();
                }
            }
        }

        /// <summary>
        /// 根据实体集的总数，新增XML节点下的子节点
        /// </summary>
        /// <param name="xFirst"></param>
        /// <param name="listGrandson"></param>
        /// <param name="p"></param>
        private void CreateNewNode(XElement xFirst, List<XElement> listGrandson, int iGrandsonEntCount)
        {
            //i < iChildEntCount - 1，之所以这样写，原因是原有子节点，已经包含了一个子节点；
            //因此减1后，新增后的节点总数即可对应上子记录数
            iGrandsonEntCount = iGrandsonEntCount - 1;
            foreach (XElement xitem in listGrandson)
            {
                for (int i = 0; i < iGrandsonEntCount; i++)
                {
                    XElement xObjectDetail = new XElement("Object",
                         new XAttribute("Description", xFirst.Attribute("Description").Value),
                         new XAttribute("Name", xFirst.Attribute("Name").Value),
                         new XAttribute("LableResourceID", xFirst.Attribute("LableResourceID").Value),
                         new XAttribute("Key", xFirst.Attribute("Key").Value),
                         new XAttribute("id", string.Empty));
                    xitem.Add(xObjectDetail);
                    foreach (XElement xda in xFirst.Elements("Attribute"))
                    {
                        XElement xAttributeDetail = new XElement("Attribute",
                            new XAttribute("Description", xda.Attribute("Description").Value),
                            new XAttribute("Name", xda.Attribute("Name").Value),
                            new XAttribute("LableResourceID", xda.Attribute("LableResourceID").Value),
                            new XAttribute("DataType", xda.Attribute("DataType").Value),
                            new XAttribute("DataValue", string.Empty),
                            new XAttribute("DataText", string.Empty));
                        xObjectDetail.Add(xAttributeDetail);
                    }
                }
            }
        }

        /// <summary>
        /// 根据实体集的总数，新增XML节点及其子节点
        /// </summary>
        /// <param name="xFirst"></param>
        /// <param name="listChild"></param>
        /// <param name="iChildEntCount"></param>
        private void CreateNewNode(XElement xFirst, List<XElement> listChild, List<XElement> listGrandSon, int iChildEntCount)
        {
            //i < iChildEntCount - 1，之所以这样写，原因是原有子节点，已经包含了一个子节点；
            //因此减1后，新增后的节点总数即可对应上子记录数
            iChildEntCount = iChildEntCount - 1;
            foreach (XElement xitem in listChild)
            {
                for (int i = 0; i < iChildEntCount; i++)
                {
                    XElement xObjectDetail = new XElement("Object",
                         new XAttribute("Description", xFirst.Attribute("Description").Value),
                         new XAttribute("Name", xFirst.Attribute("Name").Value),
                         new XAttribute("LableResourceID", xFirst.Attribute("LableResourceID").Value),
                         new XAttribute("Key", xFirst.Attribute("Key").Value),
                         new XAttribute("id", string.Empty));
                    xitem.Add(xObjectDetail);
                    foreach (XElement xda in xFirst.Elements("Attribute"))
                    {
                        XElement xAttributeDetail = new XElement("Attribute",
                            new XAttribute("Description", xda.Attribute("Description").Value),
                            new XAttribute("Name", xda.Attribute("Name").Value),
                            new XAttribute("LableResourceID", xda.Attribute("LableResourceID").Value),
                            new XAttribute("DataType", xda.Attribute("DataType").Value),
                            new XAttribute("DataValue", string.Empty),
                            new XAttribute("DataText", string.Empty));
                        xObjectDetail.Add(xAttributeDetail);
                    }

                    //填充三级节点
                    if (listGrandSon.Count() > 0)
                    {
                        xObjectDetail.Add(listGrandSon.ToArray());
                    }
                }
            }
        }

        /// <summary>
        /// 对节点填充数据
        /// </summary>
        /// <param name="listChild"></param>
        /// <param name="listChildEnts"></param>
        private void FillData(List<XElement> listChild, List<EntityObject> listChildEnts)
        {
            List<FBEntity> entPersonAssigns = new List<FBEntity>();
            bool bIsGet = false;
            foreach (XElement xParent in listChild)
            {
                List<XElement> xItems = xParent.Elements("Object").ToList();
                int i = 0;
                foreach (XElement XItem in xItems)
                {
                    if (i < listChildEnts.Count())
                    {
                        EntityObject entCur = listChildEnts[i];

                        if (XItem.Attribute("id") != null && XItem.Attribute("Key") != null)
                        {
                            string strKeyName = XItem.Attribute("Key").Value;
                            object objKey = null;
                            if (strKeyName == "COMPANYBUDGETSUMDETAILID")
                            {
                                objKey = ((V_SubjectCompanySum)entCur).T_FB_SUBJECT.SUBJECTID;
                            }
                            else if (strKeyName == "DEPTBUDGETSUMDETAILID")
                            {
                                objKey = ((V_SubjectDeptSum)entCur).T_FB_SUBJECT.SUBJECTID;
                            }
                            else
                            {
                                objKey = entCur.GetValue(strKeyName);
                            }
                            if(objKey!=null)XItem.Attribute("id").SetValue(objKey);
                        }


                        //读取Xml表节点
                        IEnumerable<XElement> xElementList = XItem.Elements("Attribute");
                        int j = i + 1;
                        foreach (XElement item in xElementList)
                        {
                            string AttName = string.Empty, AttValue = string.Empty;
                            AttName = item.Attribute("Name").Value;

                            object objValue = null;
                            if (AttName.IndexOf(".") > -1)
                            {
                                TryGetSubjectValue(entCur, AttName, ref objValue);
                            }
                            else
                            {
                                if (AttName == "RowIndex")
                                {
                                    objValue = j;
                                }
                                else if (AttName == "UPBUDGETMONEY")
                                {
                                    GetLastMonthPersonAssignMoney(entCur, ref objValue, ref entPersonAssigns, ref bIsGet);
                                }
                                else if (AttName == "SUGGESTBUDGETMONEY")
                                {
                                    //下拨经费中的参考额度
                                    object oSugestMoney = entCur.GetValue("SUGGESTBUDGETMONEY");
                                    if (oSugestMoney != null)
                                    {
                                        objValue = oSugestMoney.ToString();
                                    }
                                }
                                else if (AttName == "POSTINFO")
                                {
                                    //下拨经费中的提示
                                    object oPostInfo = entCur.GetValue("POSTINFO");
                                    if (oPostInfo != null)
                                    {
                                        objValue = oPostInfo.ToString();
                                    }
                                }
                                else if (AttName == "AUDITBUDGETMONEY")
                                {
                                    //增补中的可用结余
                                    object AuditMoney = entCur.GetValue("AUDITBUDGETMONEY");
                                    if (AuditMoney != null)
                                    {
                                        objValue = AuditMoney.ToString();
                                    }
                                }                                
                                else
                                {
                                    objValue = entCur.GetValue(AttName);
                                }
                            }

                            if (objValue != null)
                            {
                                if (AttName == "USABLEMONEY")
                                {
                                    decimal dUsablemoney = 0;
                                    decimal.TryParse(objValue.ToString(), out dUsablemoney);
                                    if (dUsablemoney == 999999 || dUsablemoney == BudgetAccountBLL.Max_Charge)
                                    {
                                        objValue = "无预算额度限制";
                                    }
                                }
                                item.Attribute("DataValue").SetValue(objValue);
                            }
                        }

                        i++;
                    }
                    else
                    {
                        i = 0;
                    }
                }
            }
        }

        /// <summary>
        /// 获取员工上月下拨的活动经费
        /// </summary>
        /// <param name="entCur"></param>
        /// <param name="objValue"></param>
        /// <param name="entPersonAssigns"></param>
        /// <param name="bIsGet"></param>
        private void GetLastMonthPersonAssignMoney(EntityObject entCur, ref object objValue, ref List<FBEntity> entPersonAssigns, ref bool bIsGet)
        {
            objValue = 0;
            T_FB_PERSONMONEYASSIGNDETAIL entDetail = entCur as T_FB_PERSONMONEYASSIGNDETAIL;
            if (entDetail == null)
            {
                return;
            }

            T_FB_PERSONMONEYASSIGNMASTER entMaster = entDetail.T_FB_PERSONMONEYASSIGNMASTER;

            if (entMaster == null)
            {
                return;
            }

            GetLastedMonthPersonAssignEntitys(entMaster, entDetail, ref entPersonAssigns, ref bIsGet);

            if (entPersonAssigns.Count() > 0)
            {
                var ds = entPersonAssigns.ToEntityList<T_FB_PERSONMONEYASSIGNDETAIL>();

                if (ds == null)
                {
                    return;
                }

                var s = from d in ds
                        where d.OWNERID == entDetail.OWNERID
                        select d;

                if (s == null)
                {
                    return;
                }

                if (s.FirstOrDefault() == null)
                {
                    return;
                }

                decimal? dBudgetMonry = s.FirstOrDefault().BUDGETMONEY;

                if (dBudgetMonry == null)
                {
                    return;
                }

                objValue = dBudgetMonry;

            }
        }

        private void GetLastedMonthPersonAssignEntitys(T_FB_PERSONMONEYASSIGNMASTER entMaster, T_FB_PERSONMONEYASSIGNDETAIL entDetail, ref List<FBEntity> entPersonAssigns, ref bool bIsGet)
        {
            using (QueryEntityBLL bll = new QueryEntityBLL())
            {
                if (entPersonAssigns.Count() > 0 || bIsGet)
                {
                    return;
                }

                QueryExpression qe = new QueryExpression();
                qe.PropertyName = FieldName.OwnerID;
                qe.PropertyValue = entMaster.OWNERID;
                qe.Operation = QueryExpression.Operations.Equal;
                qe.RelatedType = QueryExpression.RelationType.And;

                QueryExpression qeAssign = new QueryExpression();
                qeAssign.PropertyName = "ASSIGNCOMPANYID";
                qeAssign.PropertyValue = entDetail.OWNERCOMPANYID;
                qeAssign.IsUnCheckRight = true;
                qeAssign.Operation = QueryExpression.Operations.Equal;
                qeAssign.RelatedType = QueryExpression.RelationType.And;
                qeAssign.QueryType = typeof(T_FB_PERSONMONEYASSIGNMASTER).Name + "FormHR";//Latest
                qeAssign.RelatedExpression = qe;

                entPersonAssigns = bll.QueryFBEntities(qeAssign);
                bIsGet = true;
            }
        }

        /// <summary>
        /// 根据字段名称，从实体中查找出字段值
        /// </summary>
        /// <param name="entCur"></param>
        /// <param name="AttName"></param>
        /// <param name="objValue"></param>
        private void TryGetSubjectValue(EntityObject entCur, string AttName, ref object objValue)
        {
            try
            {
                string[] strlist = AttName.Split('.');
                if (strlist.Length == 2)
                {
                    EntityObject ent = (EntityObject)entCur.GetValue(strlist[0]);
                    if (ent == null)
                    {
                        GetAuditEntityRelatedSubject(entCur, ref ent);
                    }

                    objValue = ent.GetValue(strlist[1]);
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug(ex.ToString());
            }
        }

        /// <summary>
        /// 获取实体关联的科目实体
        /// </summary>
        /// <param name="entCur"></param>
        /// <param name="ent"></param>
        private void GetAuditEntityRelatedSubject(EntityObject entCur, ref EntityObject ent)
        {
            using (QueryEntityBLL bll = new QueryEntityBLL())
            {
                FBEntity fbEntity = bll.GetFBEntityByEntityKey(entCur.EntityKey);
                if (fbEntity != null)
                {
                    ent = (EntityObject)fbEntity.Entity.GetValue("T_FB_SUBJECT");
                }
            }
        }
        #endregion

        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 针对移动平台XML的年度预算汇总单自定义的明细实体
    /// </summary>
    public class V_SubjectCompanySum : EntityObject
    {
        public T_FB_SUBJECT T_FB_SUBJECT { get; set; }
        public decimal BUDGETMONEY { get; set; }
        public List<T_FB_COMPANYBUDGETAPPLYDETAIL> T_FB_COMPANYBUDGETAPPLYDETAIL { get; set; }
    }

    /// <summary>
    /// 针对移动平台XML的年度预算汇总单自定义的明细实体
    /// </summary>
    public class V_SubjectDeptSum : EntityObject
    {
        public T_FB_SUBJECT T_FB_SUBJECT { get; set; }
        public decimal BUDGETMONEY { get; set; }
        public List<T_FB_DEPTBUDGETAPPLYDETAIL> T_FB_DEPTBUDGETAPPLYDETAIL { get; set; }
    }
}
