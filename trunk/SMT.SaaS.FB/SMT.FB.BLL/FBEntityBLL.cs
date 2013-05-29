using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects.DataClasses;
using SMT_FB_EFModel;
using System.Data.Objects;
using System.Linq.Expressions;
using System.ComponentModel;
using System.Collections;
using System.Data;
using SMT.FB.DAL;
using System.Reflection;


namespace SMT.FB.BLL
{
    public class FBEntityBLL : BaseBLL
    {
        #region 1.	数据查询方法
        public FBEntity FBEntityBLLGetFBEntity(QueryExpression queryExpression)
        {
            FBEntity entity = new FBEntity();

            List<EntityObject> list = BaseGetEntities(queryExpression);
            EntityObject eo = list.FirstOrDefault();

            if (eo == null)
            {
                return null;
            }
            entity.Entity = eo;

            GetFBEntityFull(entity);
            return entity;

        }

        public List<FBEntity> FBEntityBllGetFBEntities(QueryExpression queryExpression)
        {
            List<EntityObject> list = BaseGetEntities(queryExpression);
            List<FBEntity> listResult = list.ToFBEntityList();
            listResult.ForEach(entity =>
            {
                GetFBEntityFull(entity);
            });

            return listResult;

        }

        public FBEntity GetFBEntityFull(FBEntity entity)
        {
            EntityObject eo = entity.Entity;
            var rs = (eo as IEntityWithRelationships).RelationshipManager.GetAllRelatedEnds();

            // Add Relation
            foreach (IRelatedEnd re in rs)
            {
                if (re.GetType().BaseType != typeof(EntityReference))
                {
                    AttachRelationManyEntity(entity, re);
                }
                else
                {
                    AttachRelationOneEntity(entity, re);
                }
            }

            // Child Entity, add Relation
            foreach (RelationManyEntity rEntity in entity.CollectionEntity)
            {
                rEntity.FBEntities.ForEach(subEntity =>
                {
                    var rsSub = (subEntity.Entity as IEntityWithRelationships).RelationshipManager.GetAllRelatedEnds();
                    foreach (IRelatedEnd re in rsSub)
                    {
                        if (re.GetType().BaseType == typeof(EntityReference))
                        {
                            AttachRelationOneEntity(subEntity, re);
                        }
                        else
                        {
                            #region beyond
                            AttachRelationManyEntity(subEntity, re);
                            #endregion
                        }
                    }
                });
            }
            return entity;
        }

        private void AttachRelationManyEntity(FBEntity entity, IRelatedEnd re)
        {
            Type t = re.GetType();
            Type eType = t.GetGenericArguments()[0];
            if (!re.IsLoaded) re.Load();             

            RelationManyEntity rManyE = new RelationManyEntity();
            rManyE.EntityType = eType.Name;
            rManyE.RelationshipName = re.RelationshipName;
            rManyE.PropertyName = re.TargetRoleName;
            entity.CollectionEntity.Add(rManyE);

            foreach (var item in re)
            {

                FBEntity tempEntity = new FBEntity();

                tempEntity.Entity = item as EntityObject;
                rManyE.FBEntities.Add(tempEntity);
            }
            //rManyE.FBEntities.ForEach(item =>
            //    {
            //        try
            //        {
            //            IEntityWithRelationships ie = item.Entity as IEntityWithRelationships;
            //            re.Remove(ie);
            //        }
            //        catch (Exception ex)
            //        {
            //        }
            //    });


        }
        private void AttachRelationOneEntity(FBEntity entity, IRelatedEnd re)
        {
            //Type t = re.GetType();
            //Type eType = t.GetGenericArguments()[0];
            if (re.IsLoaded) return;
            re.Load();
            //RelationOneEntity rOneE = new RelationOneEntity();
            //rOneE.EntityType= eType.Name;
            //rOneE.RelationshipName = re.RelationshipName;
            //rOneE.PropertyName = re.TargetRoleName;
            //entity.ReferencedEntity.Add(rOneE);
            //foreach (var item in re)
            //{
            //    FBEntity tempEntity = new FBEntity();
            //    tempEntity.Entity = item as EntityObject;
            //    rOneE.FBEntity = tempEntity;
            //    // re.Remove(item as IEntityWithRelationships);
            //}
        }
        #endregion

        #region 2.	数据保存方法
        public bool FBEntityBLLSaveList(List<FBEntity> fbEntityList)
        {
            try
            {
                BeginTransaction();
                fbEntityList.ForEach(entity =>
                {
                    InnerSave(entity);
                });
                CommitTransaction();
                return true;
            }
            catch (Exception ex)
            {
                RollbackTransaction();
                throw ex;
            }
        }

        public bool FBEntityBLLSaveListNoTrans(List<FBEntity> fbEntityList)
        {
            try
            {
                fbEntityList.ForEach(entity =>
                {
                    InnerSave(entity);
                });
                return true;
            }
            catch (Exception ex)
            {
                RollbackTransaction();
                throw ex;
            }
        }

        public bool FBEntityBllSave(FBEntity fbEntity)
        {
            try
            {
                //BeginTransaction();

                InnerSave(fbEntity);

                //CommitTransaction();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            if (typeof(VirtualEntityObject).IsAssignableFrom(fbEntity.GetType()))
            {
                return false;
            }
            //EntityObject newEntity = GetEntity(fbEntity.Entity.EntityKey);
            
            //FBEntity newFBEntity = new FBEntity();
            //newFBEntity.Entity = newEntity;
            //GetFBEntityFull(newFBEntity);
            //return newFBEntity;
            return true;
            
        }

        internal void InnerSave(FBEntity fbEntity)
        {
           
            List<FBEntity> listSave = new List<FBEntity>();
            if (fbEntity != null)
            {
                foreach (RelationManyEntity rEntity in fbEntity.CollectionEntity)
                {

                    rEntity.FBEntities.ForEach(subEntity =>
                    {
                        // 没改变的记录不修改
                        if (subEntity.FBEntityState == FBEntityState.Unchanged)
                        {
                            return;
                        }
                        RefreshData(subEntity.Entity);
                        listSave.Add(subEntity);
                    });
                }
                RefreshData(fbEntity.Entity);
                listSave.Add(fbEntity);
            }

            if ((fbEntity.FBEntityState == FBEntityState.Added) && (!typeof(VirtualEntityObject).IsAssignableFrom(fbEntity.GetType()))) // 添加的实体也一同把子实体也添加.
            {
                BassBllSave(fbEntity.Entity, fbEntity.FBEntityState);
            }
            else
            {
                listSave.ForEach(saveEntity =>
                {
                    BassBllSave(saveEntity.Entity, saveEntity.FBEntityState);
                });
            }

        }
        #endregion


        #region Save FBEntityList
        public bool FBEntityBLLSave(List<FBEntity> fbEntityList)
        {
            if (fbEntityList.Count == 0)
            {
                return true;
            }
            string returnType = fbEntityList[0].Entity.GetType().Name;
            Type type = this.GetType();
            MethodInfo method = type.GetMethod("SaveList" + returnType);
            if (method != null)
            {
                try
                {

                    object result = method.Invoke(this, new object[] { fbEntityList });
                    return (bool)result;
                }
                catch (Exception ex)
                {

                    throw ex.InnerException;
                }
            }
            else
            {
                return SaveDefault(fbEntityList);
            }

        }

        public bool SaveDefault(List<FBEntity> fbEntityList)
        {
            return FBEntityBLLSaveList(fbEntityList);
        }

        public bool SaveListT_FB_SUMSETTINGSMASTER(List<FBEntity> fbEntityList)
        {
            fbEntityList.ForEach(item =>
            {
                T_FB_SUMSETTINGSMASTER Master = item.Entity as T_FB_SUMSETTINGSMASTER;
                if (Master.EDITSTATES == 0)
                {
                    QueryExpression qeID = QueryExpression.Equal("T_FB_SUMSETTINGSMASTER.SUMSETTINGSMASTERID", Master.SUMSETTINGSMASTERID);

                    qeID.QueryType = "T_FB_SUMSETTINGSDETAIL";
                    var result = FBEntityBllGetFBEntities(qeID);
                    if (result != null)
                    {
                        List<FBEntity> fbEntity = result;
                        fbEntity.ForEach(p =>
                        {
                            //QueryExpression qeCompany = QueryExpression.Equal("SUMSETTINGSMASTERID", Master.SUMSETTINGSMASTERID);
                            //qeCompany.QueryType = "T_FB_COMPANYBUDGETSUMMASTER";
                            //var v = GetFBEntity(qeCompany);
                            //QueryExpression qeDept = QueryExpression.Equal("SUMSETTINGSMASTERID", Master.SUMSETTINGSMASTERID);
                            //qeDept.QueryType = "T_FB_DEPTBUDGETSUMMASTER";
                            //var q = GetFBEntity(qeCompany);

                            //if (v != null||q!=null)
                            //{
                            //    throw new FBBLLException("以下公司已经有汇总使用，不能删除！");
                            //}

                            p.FBEntityState = FBEntityState.Modified;
                            T_FB_SUMSETTINGSDETAIL detail = p.Entity as T_FB_SUMSETTINGSDETAIL;
                            detail.EDITSTATES = 0;
                            FBEntityBLLSaveList(fbEntity);
                        });
                    }
                }
            });

            return FBEntityBLLSaveList(fbEntityList);
        }

        /// <summary>
        /// 保存公司科目维护
        ///   级联的去除不可用的部门科目和岗位科目
        /// </summary>
        /// <param name="fbEntityList"></param>
        /// <returns></returns>
        public bool SaveListT_FB_SUBJECTCOMPANY(List<FBEntity> fbEntityList)
        {
            QueryExpression qeSCom = new QueryExpression();
            QueryExpression qeTop = qeSCom;
            string StrCompanyID = "";//公司ID
            bool IsExistPlus = false;
            // 找出没有设置年度预算而后又允许年度预算的
            List<T_FB_SUBJECTCOMPANY> inActivedlist = fbEntityList.CreateList(item =>
            {
                T_FB_SUBJECTCOMPANY entity = item.Entity as T_FB_SUBJECTCOMPANY;
                if (string.IsNullOrEmpty(StrCompanyID))
                {
                    StrCompanyID = entity.OWNERCOMPANYID;
                    QueryExpression qe = QueryExpression.Equal("SUBJECTCOMPANYID", entity.SUBJECTCOMPANYID);
                    var baData = this.InnerGetEntities<T_FB_SUBJECTCOMPANY>(qe);
                    if (baData.Count() > 0)
                    {
                        T_FB_SUBJECTCOMPANY OldSub = new T_FB_SUBJECTCOMPANY();
                        OldSub = baData.FirstOrDefault();
                        if (OldSub.ISYEARBUDGET == 0)
                        {
                            if (entity.ISYEARBUDGET == 1)
                            {
                                QueryExpression qeAccount = QueryExpression.Equal("OWNERCOMPANYID", entity.OWNERCOMPANYID);
                                QueryExpression qeAccount1 = QueryExpression.Equal("T_FB_SUBJECT.SUBJECTID", entity.T_FB_SUBJECT != null ? entity.T_FB_SUBJECT.SUBJECTID : entity.T_FB_SUBJECTReference.EntityKey.EntityKeyValues[0].Value.ToString());
                                QueryExpression qeAccount2 = QueryExpression.Equal("ACCOUNTOBJECTTYPE", "1");
                                QueryExpression qeAccount3 = new QueryExpression();
                                qeAccount3.PropertyName = "USABLEMONEY";
                                qeAccount3.PropertyValue = "0";
                                qeAccount3.Operation = QueryExpression.Operations.LessThanOrEqual;
                                qeAccount3.Operation = QueryExpression.Operations.LessThan;//是否有问题
                                qeAccount.RelatedType = QueryExpression.RelationType.And;
                                qeAccount1.RelatedType = QueryExpression.RelationType.And;
                                qeAccount2.RelatedType = QueryExpression.RelationType.And;
                                qeAccount3.RelatedType = QueryExpression.RelationType.And;

                                qeAccount.RelatedExpression = qeAccount1;
                                qeAccount2.RelatedExpression = qeAccount1;
                                qeAccount3.RelatedExpression = qeAccount2;
                                qeAccount3.QueryType = typeof(T_FB_BUDGETACCOUNT).Name;


                                //var baDataAccount = this.InnerGetEntities<T_FB_BUDGETACCOUNT>(qeAccount);
                                //if(baDataAccount.Count() >0)
                                //{
                                //    //IsExistPlus= true;
                                //}
                            }
                        }
                    }
                }
                //return entity.ACTIVED != 1 ? entity : null;

                return entity;
            });
            if (IsExistPlus)
            {
                return IsExistPlus;
            }

            //var baData = this.InnerGetEntities<T_FB_SUBJECTCOMPANY>(qeDept);
            // 查出公司科目相关的部门科目及岗位科目
            inActivedlist.ForEach(item =>
            {
                qeTop.RelatedExpression = QueryExpression.Equal("T_FB_SUBJECTCOMPANY.SUBJECTCOMPANYID", item.SUBJECTCOMPANYID);
                qeTop.RelatedType = QueryExpression.RelationType.Or;
                qeTop = qeTop.RelatedExpression;
            });
            // 将部门科目及岗位科目置为不可用
            if (qeSCom.RelatedExpression != null)
            {
                qeSCom = qeSCom.RelatedExpression;
                qeSCom.Include = new string[] { "T_FB_SUBJECTPOST" };
                List<T_FB_SUBJECTDEPTMENT> inActiveDataList = GetEntities<T_FB_SUBJECTDEPTMENT>(qeSCom.RelatedExpression);
                inActiveDataList.ForEach(item =>
                {
                    item.ACTIVED = 0;
                    item.T_FB_SUBJECTPOST.ToList().ForEach(itemPost =>
                    {
                        itemPost.ACTIVED = 0;
                    });
                });
            }

            if (fbEntityList.Count > 0)
            {
                //记录公司部门科目设置修改流水
                SaveListT_FB_WFSUBJECTSETTING(fbEntityList, "1");
            }
            return FBEntityBLLSaveList(fbEntityList);
        }


        /// <summary>
        ///   部门科目记录修改流水
        /// </summary>
        /// <param name="fbEntityList"></param>
        /// <returns></returns>
        public bool SaveListT_FB_SUBJECTDEPTMENT(List<FBEntity> fbEntityList)
        {
            if (fbEntityList.Count > 0)
            {
                //记录公司部门科目设置修改流水
                SaveListT_FB_WFSUBJECTSETTING(fbEntityList, "2");

                //修改部门启用时，同时更新岗位启用。
                fbEntityList.ForEach(item =>
                {
                    T_FB_SUBJECTDEPTMENT entity = item.Entity as T_FB_SUBJECTDEPTMENT;
                    if (entity != null && entity.ACTIVED == 0)
                    {
                        List<FBEntity> EntityListPost = new List<FBEntity>();
                        QueryExpression qe = QueryExpression.Equal("T_FB_SUBJECTDEPTMENT.SUBJECTDEPTMENTID", entity.SUBJECTDEPTMENTID);

                        List<T_FB_SUBJECTPOST> PostList = GetEntities<T_FB_SUBJECTPOST>(qe);
                        PostList.ForEach(p =>
                        {
                            FBEntity a = new FBEntity();
                            a.FBEntityState = FBEntityState.Modified;

                            p.ACTIVED = 0;//1 : 可用 ; 0 : 不可用

                            a.Entity = p;
                            a.EntityKey = null;
                            EntityListPost.Add(a);
                        });
                        FBEntityBLLSaveList(EntityListPost);
                    }
                });
            }
            return FBEntityBLLSaveList(fbEntityList);
        }

        /// <summary>
        ///   部门科目记录修改流水
        /// </summary>
        /// <param name="fbEntityList"></param>
        /// <returns></returns>
        public bool SaveListT_FB_SUBJECTPOST(List<FBEntity> fbEntityList)
        {
            if (fbEntityList.Count > 0)
            {
                //记录修改流水
                SaveListT_FB_WFSUBJECTSETTING(fbEntityList, "3");

                // 写死活动经费科目 可用；
                //string MoneyAssign = SystemBLL.etityT_FB_SYSTEMSETTINGS.MONEYASSIGNSUBJECTID;
                //fbEntityList.ForEach(item =>
                //{
                //    T_FB_SUBJECTPOST entity = item.Entity as T_FB_SUBJECTPOST;
                //    string strSubjectID = entity.T_FB_SUBJECTReference.EntityKey.EntityKeyValues[0].Value.ToString();
                //    if (strSubjectID == MoneyAssign)
                //    {
                //        FBEntity a = new FBEntity();
                //        a.FBEntityState = FBEntityState.Modified;

                //        entity.ACTIVED = 1;//1 : 可用 ; 0 : 不可用

                //        a.Entity = entity;
                //        a.EntityKey = null;
                //        item = a;

                //        return;
                //    }
                //});
            }
            return FBEntityBLLSaveList(fbEntityList);
        }


        /// <summary>        
        ///   保存科目设置流水
        /// </summary>
        /// <param name="fbEntityList"></param>
        /// <returns></returns>
        public bool SaveListT_FB_WFSUBJECTSETTING(List<FBEntity> fbEntityList, string strfig)
        {
            List<FBEntity> inActivedlist = fbEntityList.CreateList(item =>
            {
                T_FB_WFSUBJECTSETTING fbEntity = new T_FB_WFSUBJECTSETTING();

                if (strfig == "1")
                {
                    T_FB_SUBJECTCOMPANY SubjectEntity = item.Entity as T_FB_SUBJECTCOMPANY;

                    fbEntity.WFSUBJECTSETTINGID = Guid.NewGuid().ToString();
                    fbEntity.SUBJECTID = SubjectEntity.T_FB_SUBJECTReference.EntityKey.EntityKeyValues[0].Value.ToString();
                    fbEntity.ACTIVED = SubjectEntity.ACTIVED;
                    fbEntity.ISMONTHADJUST = SubjectEntity.ISMONTHADJUST;
                    fbEntity.ISMONTHLIMIT = SubjectEntity.ISMONTHLIMIT;
                    fbEntity.ISPERSON = SubjectEntity.ISPERSON;
                    fbEntity.ISYEARBUDGET = SubjectEntity.ISYEARBUDGET;
                    fbEntity.CONTROLTYPE = SubjectEntity.CONTROLTYPE;
                    fbEntity.OWNERCOMPANYID = SubjectEntity.OWNERCOMPANYID;
                    fbEntity.OWNERCOMPANYNAME = SubjectEntity.OWNERCOMPANYNAME;
                    fbEntity.OWNERDEPARTMENTID = SubjectEntity.OWNERDEPARTMENTID;
                    fbEntity.OWNERDEPARTMENTNAME = SubjectEntity.OWNERDEPARTMENTNAME;
                    fbEntity.OWNERPOSTID = SubjectEntity.OWNERPOSTID;
                    fbEntity.OWNERPOSTNAME = SubjectEntity.OWNERPOSTNAME;
                    fbEntity.CREATEUSERID = SubjectEntity.CREATEUSERID;
                    fbEntity.UPDATEUSERID = SubjectEntity.UPDATEUSERID;
                    fbEntity.UPDATEDATE = DateTime.Now;
                    fbEntity.CREATEDATE = DateTime.Now;
                    fbEntity.ORDERTYPE = strfig;//1 公司 2部门 3岗位
                }
                else if (strfig == "2")
                {
                    T_FB_SUBJECTDEPTMENT SubjectEntity = item.Entity as T_FB_SUBJECTDEPTMENT;
                    if (SubjectEntity == null)
                    {
                        T_FB_SUBJECTPOST SubjectEntity1 = item.Entity as T_FB_SUBJECTPOST;

                        fbEntity.WFSUBJECTSETTINGID = Guid.NewGuid().ToString();
                        fbEntity.SUBJECTID = SubjectEntity1.T_FB_SUBJECTReference.EntityKey.EntityKeyValues[0].Value.ToString();
                        fbEntity.ACTIVED = SubjectEntity1.ACTIVED;
                        fbEntity.LIMITBUDGEMONEY = SubjectEntity1.LIMITBUDGEMONEY;
                        fbEntity.OWNERCOMPANYID = SubjectEntity1.OWNERCOMPANYID;
                        fbEntity.OWNERCOMPANYNAME = SubjectEntity1.OWNERCOMPANYNAME;
                        fbEntity.OWNERDEPARTMENTID = SubjectEntity1.OWNERDEPARTMENTID;
                        fbEntity.OWNERDEPARTMENTNAME = SubjectEntity1.OWNERDEPARTMENTNAME;
                        fbEntity.OWNERPOSTID = SubjectEntity1.OWNERPOSTID;
                        fbEntity.OWNERPOSTNAME = SubjectEntity1.OWNERPOSTNAME;
                        fbEntity.CREATEUSERID = SubjectEntity1.CREATEUSERID;
                        fbEntity.UPDATEUSERID = SubjectEntity1.UPDATEUSERID;
                        fbEntity.UPDATEDATE = DateTime.Now;
                        fbEntity.CREATEDATE = DateTime.Now;
                        fbEntity.ORDERTYPE = "3";//1 公司 2部门 3岗位
                    }
                    else
                    {
                        fbEntity.WFSUBJECTSETTINGID = Guid.NewGuid().ToString();
                        fbEntity.SUBJECTID = SubjectEntity.T_FB_SUBJECTReference.EntityKey.EntityKeyValues[0].Value.ToString();
                        fbEntity.ACTIVED = SubjectEntity.ACTIVED;
                        fbEntity.LIMITBUDGEMONEY = SubjectEntity.LIMITBUDGEMONEY;
                        fbEntity.OWNERCOMPANYID = SubjectEntity.OWNERCOMPANYID;
                        fbEntity.OWNERCOMPANYNAME = SubjectEntity.OWNERCOMPANYNAME;
                        fbEntity.OWNERDEPARTMENTID = SubjectEntity.OWNERDEPARTMENTID;
                        fbEntity.OWNERDEPARTMENTNAME = SubjectEntity.OWNERDEPARTMENTNAME;
                        fbEntity.OWNERPOSTID = SubjectEntity.OWNERPOSTID;
                        fbEntity.OWNERPOSTNAME = SubjectEntity.OWNERPOSTNAME;
                        fbEntity.CREATEUSERID = SubjectEntity.CREATEUSERID;
                        fbEntity.UPDATEUSERID = SubjectEntity.UPDATEUSERID;
                        fbEntity.UPDATEDATE = DateTime.Now;
                        fbEntity.CREATEDATE = DateTime.Now;
                        fbEntity.ORDERTYPE = strfig;//1 公司 2部门 3岗位
                    }
                }
                else if (strfig == "3")
                {
                    T_FB_SUBJECTPOST SubjectEntity = item.Entity as T_FB_SUBJECTPOST;

                    fbEntity.WFSUBJECTSETTINGID = Guid.NewGuid().ToString();
                    fbEntity.SUBJECTID = SubjectEntity.T_FB_SUBJECTReference.EntityKey.EntityKeyValues[0].Value.ToString();
                    fbEntity.ACTIVED = SubjectEntity.ACTIVED;
                    fbEntity.LIMITBUDGEMONEY = SubjectEntity.LIMITBUDGEMONEY;
                    fbEntity.OWNERCOMPANYID = SubjectEntity.OWNERCOMPANYID;
                    fbEntity.OWNERCOMPANYNAME = SubjectEntity.OWNERCOMPANYNAME;
                    fbEntity.OWNERDEPARTMENTID = SubjectEntity.OWNERDEPARTMENTID;
                    fbEntity.OWNERDEPARTMENTNAME = SubjectEntity.OWNERDEPARTMENTNAME;
                    fbEntity.OWNERPOSTID = SubjectEntity.OWNERPOSTID;
                    fbEntity.OWNERPOSTNAME = SubjectEntity.OWNERPOSTNAME;
                    fbEntity.CREATEUSERID = SubjectEntity.CREATEUSERID;
                    fbEntity.UPDATEUSERID = SubjectEntity.UPDATEUSERID;
                    fbEntity.UPDATEDATE = DateTime.Now;
                    fbEntity.CREATEDATE = DateTime.Now;
                    fbEntity.ORDERTYPE = strfig;//1 公司 2部门 3岗位
                }
                FBEntity a = new FBEntity();
                a.Entity = fbEntity;
                a.FBEntityState = FBEntityState.Added;
                a.EntityKey = null;
                return a;
            });
            return FBEntityBLLSaveList(inActivedlist);
        }
        #endregion



    }

}
