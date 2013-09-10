using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.SaaS.OA.DAL;
using SMT_OA_EFModel;
using System.Linq.Dynamic;

namespace SMT.SaaS.OA.BLL
{


    #region 会议类型
    public class MeetingTypeManagementBll : BaseBll<T_OA_MEETINGTYPE>
    {
        //private MeetingTypeDal MeetingType = new MeetingTypeDal();


        //添加会议室内容
        public string AddMeetingTypeInfo(T_OA_MEETINGTYPE RoomInfo)
        {
            try
            {
                string StrReturn = "";
                var tempEnt = dal.GetObjects().FirstOrDefault(s => s.MEETINGTYPE == RoomInfo.MEETINGTYPE);
                if (tempEnt != null)
                {
                    StrReturn = "REPETITION"; //{0}已存在，保存失败！
                    //throw new Exception("Repetition");
                }

                int i = dal.Add(RoomInfo);


                if (!(i > 0))
                {
                    StrReturn = "SAVEFAILED";//保存失败
                }
                return StrReturn;
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }



        public bool DeleteMeetingTypeInfo(string MeetingTypeID)
        {
            try
            {
                var entitys = (from ent in dal.GetObjects()
                               where ent.MEETINGTYPEID == MeetingTypeID
                               select ent);
                if (entitys.Count() > 0)
                {
                    var entity = entitys.FirstOrDefault();
                    dal.Delete(entity);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
                throw (ex);
            }
        }

        //批量删除会议类型信息
        public string BatchDeleteMeetingTypeInfo(string[] ArrTypeIDs)
        {
            try
            {
                string StrReturn = "";
                foreach (string id in ArrTypeIDs)
                {
                    var ents = from e in dal.GetObjects()
                               where e.MEETINGTYPEID == id
                               select e;

                    var ent = ents.Count() > 0 ? ents.FirstOrDefault() : null;

                    if (ent != null)
                    {
                        var enttemplate = from p in dal.GetObjects<T_OA_MEETINGTEMPLATE>().Include("T_OA_MEETINGTYPE")
                                          where p.T_OA_MEETINGTYPE.MEETINGTYPE == ents.FirstOrDefault().MEETINGTYPE
                                          select p;
                        var entmeeting = from n in dal.GetObjects<T_OA_MEETINGINFO>().Include("T_OA_MEETINGTYPE")
                                         where n.T_OA_MEETINGTYPE.MEETINGTYPE == ents.FirstOrDefault().MEETINGTYPE
                                         select n;
                        if (enttemplate.Count() > 0 || entmeeting.Count() > 0)
                        {
                            if (enttemplate.Count() > 0 && entmeeting.Count() > 0)
                            {

                                StrReturn = "PLEASEFIRSTDELETTEMPLATEANDEMEETINGINFO";//请先删除会议类型对应的摸扳和会议信息
                            }
                            else
                            {
                                if (entmeeting.Count() > 0)
                                {
                                    StrReturn = "PLEASEFIRSTDELETETEMPLATE";//请先删除摸扳对应的会议类型
                                }
                                else
                                {
                                    StrReturn = "PLEASEFIRSTDELETETEMPLATE";//请先删除摸扳对应的会议类型
                                }
                            }
                            break;
                        }
                        else
                        {
                            //DataContext.DeleteObject(ent);
                            dal.DeleteFromContext(ent);
                            
                        }
                    }
                }

                if (StrReturn == "")
                {
                    int i = dal.SaveContextChanges();

                    StrReturn = i > 0 ? "" : "ERROR";
                }
                return StrReturn;

            }
            catch (Exception ex)
            {
                return ex.ToString();
                throw (ex);
            }
        }



        public void UpdateMeetingTypeInfo(T_OA_MEETINGTYPE MeetingTypeInfo)
        {
            try
            {

                var users = from ent in dal.GetTable()
                            where ent.MEETINGTYPEID == MeetingTypeInfo.MEETINGTYPEID
                            select ent;

                if (users.Count() > 0)
                {
                    var user = users.FirstOrDefault();
                    Utility.CloneEntity(MeetingTypeInfo, user);
                    dal.Update(user);
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }


        public IQueryable<T_OA_MEETINGTYPE> GetMeetingMeetingTypeListByID(string MeetingTypeID)
        {
            try
            {
                var q = from ent in dal.GetObjects()
                        where ent.MEETINGTYPEID == MeetingTypeID
                        select ent;


                if (q.Count() > 0)
                {
                    return q;
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
                throw (ex);
            }
        }


        //判断是否有存在的会议类型
        //TypeName 会议类型
        public bool GetMeetingTypeByMeetingType(string TypeName)
        {
            try
            {
                bool IsExist = false;
                var q = from ent in dal.GetObjects()
                        where ent.MEETINGTYPE == TypeName
                        orderby ent.MEETINGTYPEID
                        select ent;
                if (q.Count() > 0)
                {
                    //return q.FirstOrDefault();
                    IsExist = true;
                }
                return IsExist;

            }
            catch (Exception ex)
            {
                return false;
                throw (ex);
            }
            //return null;
        }

        public T_OA_MEETINGTYPE GetMeetingTypeNameById(string TypeNameId)
        {
            try
            {
                var q = from ent in dal.GetObjects()
                        where ent.MEETINGTYPEID == TypeNameId
                        orderby ent.MEETINGTYPEID
                        select ent;
                if (q.Count() > 0)
                {
                    return q.FirstOrDefault();
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
                throw (ex);
            }
        }



        public IQueryable<T_OA_MEETINGTYPE> GetMeetingTypeInfos(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID)
        {
            try
            {
                var ents = from ent in dal.GetObjects()

                           select ent;
                List<object> queryParas = new List<object>();
                queryParas.AddRange(paras);
                //UtilityClass.SetOrganizationFilter(ref filterString, ref queryParas, userID, "OAMEETINGTYPE");
                if (queryParas.Count > 0)
                {
                    if (!string.IsNullOrEmpty(filterString))
                    {
                        ents = ents.ToList().AsQueryable().Where(filterString, queryParas.ToArray());
                    }
                }
                ents = ents.OrderBy(sort);
                ents = Utility.Pager<T_OA_MEETINGTYPE>(ents, pageIndex, pageSize, ref pageCount);
                return ents;

            }
            catch (Exception ex)
            {
                return null;

            }

        }





        public IQueryable<T_OA_MEETINGTYPE> GetMeetingTypeNameInfos()
        {
            try
            {
                var query = from p in dal.GetObjects()
                            orderby p.CREATEDATE descending
                            select p;

                //return null;
                return query.Count() > 0 ? query : null;

            }
            catch (Exception ex)
            {
                return null;
                throw (ex);
            }
        }




        //获取查询的会议类型信息
        public IQueryable<T_OA_MEETINGTYPE> GetMeetingTypeListBySearch(T_OA_MEETINGTYPE searchMeetingTypeInfo)
        {
            try
            {
                var q = from ent in dal.GetObjects()
                        select ent;
                if (searchMeetingTypeInfo != null)
                {
                    if (searchMeetingTypeInfo.MEETINGTYPE.Trim() != null && searchMeetingTypeInfo.MEETINGTYPE.Trim() != string.Empty)//会议类型
                    {
                        q = q.Where(s => searchMeetingTypeInfo.MEETINGTYPE.Contains(s.MEETINGTYPE));
                    }
                    if (searchMeetingTypeInfo.REMARK.Trim() != null && searchMeetingTypeInfo.REMARK.Trim() != string.Empty)//重复提醒类型
                    {
                        q = q.Where(s => searchMeetingTypeInfo.REMARK.Contains(s.REMARK));
                    }
                }
                q = q.OrderByDescending(s => s.CREATEDATE);
                if (q.Count() > 0)
                {
                    return q;
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
                throw (ex);
            }
        }

        public T_OA_MEETINGTYPE GetSingleMeetingTypeInfoByTypeId(string TypeId)
        {
            try
            {
                var q = from ent in dal.GetObjects()
                        where ent.MEETINGTYPEID == TypeId
                        orderby ent.MEETINGTYPEID
                        select ent;
                if (q.Count() > 0)
                {
                    return q.FirstOrDefault();

                }


                return null;
            }
            catch (Exception ex)
            {
                return null;
                throw (ex);
            }

        }




    }
    #endregion

    #region 会议类型模板
    public class MeetingTemplateManagementBll : BaseBll<T_OA_MEETINGTEMPLATE>
    {
        
        //添加会议类型模板
        public string AddMeetingTemplateInfo(T_OA_MEETINGTEMPLATE TemplateInfo)
        {
            try
            {

                string StrReturn = "";
                var tempEnt = dal.GetObjects().FirstOrDefault(s => s.TEMPLATENAME == TemplateInfo.TEMPLATENAME
                    && s.T_OA_MEETINGTYPE.MEETINGTYPEID == TemplateInfo.T_OA_MEETINGTYPE.MEETINGTYPEID);
                if (tempEnt != null)
                {
                    StrReturn = "REPETITION"; //{0}已存在，保存失败！
                    //throw new Exception("Repetition");
                }
                else
                {

                    //T_OA_MEETINGTEMPLATE entity = new T_OA_MEETINGTEMPLATE();
                    Utility.RefreshEntity(TemplateInfo);


                    int i = dal.Add(TemplateInfo);

                    //SMT_OA_EFModel
                    if (!(i > 0))
                    {
                        StrReturn = "SAVEFAILED";//保存失败
                    }
                }
                return StrReturn;



            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }



        public bool DeleteMeetingTemplateInfo(string MeetingTemplateID)
        {
            try
            {
                var entitys = (from ent in dal.GetObjects()
                               where ent.MEETINGTEMPLATEID == MeetingTemplateID
                               select ent);
                if (entitys.Count() > 0)
                {
                    var entity = entitys.FirstOrDefault();
                    dal.Delete(entity);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
                throw (ex);
            }
        }
        public string UpdateMeetingTemplateInfo(T_OA_MEETINGTEMPLATE MeetingTemplateInfo)
        {

            string StrReturn = "";
            try
            {

                var q = from a in dal.GetObjects().Include("T_OA_MEETINGTYPE")
                        select a;
                q = q.Where(s => s.TEMPLATENAME == MeetingTemplateInfo.TEMPLATENAME
                    && s.T_OA_MEETINGTYPE.MEETINGTYPEID == MeetingTemplateInfo.T_OA_MEETINGTYPE.MEETINGTYPEID);
                                
                if (q.Count() > 1)
                {
                    StrReturn = "REPETITION"; //{0}已存在，保存失败！

                    return StrReturn;
                    //throw new Exception("Repetition");
                }

                var users = from ent in dal.GetObjects()
                            where ent.MEETINGTEMPLATEID == MeetingTemplateInfo.MEETINGTEMPLATEID
                            select ent;
                if (users.Count() > 0)
                {
                    var user = users.FirstOrDefault();
                    Utility.CloneEntity(MeetingTemplateInfo, user);
                    dal.Update(user);
                }
                return StrReturn;
                //T_OA_MEETINGTEMPLATE tmpobj = DataContext.GetObjectByKey(MeetingTemplateInfo.EntityKey) as T_OA_MEETINGTEMPLATE;
                //tmpobj.T_OA_MEETINGTYPE = DataContext.GetObjectByKey(MeetingTemplateInfo.T_OA_MEETINGTYPE.EntityKey) as T_OA_MEETINGTYPE;
                //DataContext.ApplyPropertyChanges(MeetingTemplateInfo.EntityKey.EntitySetName, MeetingTemplateInfo);
                //int i = DataContext.SaveChanges();


            }
            catch (Exception ex)
            {
                return "ERROR";
                throw (ex);
            }
        }


        //批量删除会议类型模板信息
        public bool BatchDeleteMeetingTypeTemplateInfos(string[] ArrMeetingTypeTemplateIDs)
        {
            try
            {
                foreach (string id in ArrMeetingTypeTemplateIDs)
                {
                    var ents = from e in dal.GetObjects()
                               where e.MEETINGTEMPLATEID == id
                               select e;
                    var ent = ents.Count() > 0 ? ents.FirstOrDefault() : null;

                    if (ent != null)
                    {
                        dal.DeleteFromContext(ent);
                    }
                }

                int i = dal.SaveContextChanges();
                return i > 0 ? true : false;
                
            }
            catch (Exception ex)
            {
                return false;
                throw (ex);
            }
        }


        public IQueryable<T_OA_MEETINGTEMPLATE> GetMeetingTemplateListByID(string MeetingTemplateID)
        {
            try
            {
                var q = from ent in dal.GetObjects().Include("T_OA_MEETINGTYPE")
                        where ent.MEETINGTEMPLATEID == MeetingTemplateID
                        select ent;


                if (q.Count() > 0)
                {
                    return q;
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
                throw (ex);
            }
        }


        //判断是否有存在的会议类型
        //TypeName 会议类型 
        // TemplateName 模板名称
        // 会议类型和模板名称不能重复
        public bool GetMeetingTemplateByTemplateNameAndType(string TypeName, string TemplateName)
        {
            try
            {
                bool IsExist = false;
                var q = from ent in dal.GetObjects()
                        //where ent.MEETINGTYPE == TypeName && ent.TEMPLATENAME == TemplateName
                        where ent.T_OA_MEETINGTYPE.MEETINGTYPEID == TypeName && ent.TEMPLATENAME == TemplateName
                        orderby ent.MEETINGTEMPLATEID
                        select ent;
                if (q.Count() > 0)
                {
                    //return q.FirstOrDefault();
                    IsExist = true;
                }

                return IsExist;
            }
            catch (Exception ex)
            {
                return false;
                throw (ex);
            }
        }
        //获取模板内容
        public string GetMeetingTemplateContentByTemplateNameAndType(string TypeName, string TemplateName)
        {
            
            return "";
        }


        public T_OA_MEETINGTEMPLATE GetMeetingTemplateNameById(string StrTemplateId)
        {
            try
            {
                var q = from ent in dal.GetObjects().Include("T_OA_MEETINGTYPE")
                        where ent.MEETINGTEMPLATEID == StrTemplateId
                        orderby ent.MEETINGTEMPLATEID
                        select ent;
                if (q.Count() > 0)
                {
                    return q.FirstOrDefault();
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
                throw (ex);
            }
        }


        public IQueryable<T_OA_MEETINGTEMPLATE> GetMeetingTypeTemplateInfos(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID)
        {
            try
            {
                var ents = from ent in dal.GetObjects().Include("T_OA_MEETINGTYPE")

                           select ent;
                List<object> queryParas = new List<object>();
                queryParas.AddRange(paras);
                UtilityClass.SetOrganizationFilter(ref filterString, ref queryParas, userID, "OAMEETINGTYPETEMPLATE");
                if (queryParas.Count > 0)
                {
                    if (!string.IsNullOrEmpty(filterString))
                    {
                       ents = ents.ToList().AsQueryable().Where(filterString, queryParas.ToArray());
                    }
                }
                ents = ents.OrderBy(sort);
                ents = Utility.Pager<T_OA_MEETINGTEMPLATE>(ents, pageIndex, pageSize, ref pageCount);
                T_OA_MEETINGTEMPLATE e = new T_OA_MEETINGTEMPLATE();

                return ents;

            }
            catch (Exception ex)
            {
                return null;
                throw (ex);
            }

        }

        //获取某一类型的模板名称
        public List<T_OA_MEETINGTEMPLATE> GetMeetingTemplateNameInfosByMeetingType(string StrMeetingType)
        {
            try
            {
                var query = from p in dal.GetObjects()
                            //where p.MEETINGTYPE == StrMeetingType
                            where p.T_OA_MEETINGTYPE.MEETINGTYPE == StrMeetingType
                            orderby p.CREATEDATE descending
                            select p;
                return query.ToList<T_OA_MEETINGTEMPLATE>();
            }
            catch (Exception ex)
            {
                return null;
                throw (ex);
            }
        }

        //获取查询的会议类型信息

        // StrMeetingType 会议类型
        // StrTemplateName 模板名
        // StrContent 模板内容
        public IQueryable<T_OA_MEETINGTEMPLATE> GetMeetingTypeTemplateInfosListBySearch(string StrMeetingType, string StrTemplateName, string strContent)
        {
            try
            {
                var q = from ent in dal.GetObjects()
                        select ent;

                //if (!string.IsNullOrEmpty(StrMeetingType))
                //{
                //    q = q.Where(s => StrMeetingType.Contains(s.MEETINGTYPE));
                //}
                //if (!string.IsNullOrEmpty(StrTitle))
                //{
                //    q = q.Where(s => StrTitle.Contains(s.T));
                //}
                if (!string.IsNullOrEmpty(StrTemplateName))
                {
                    q = q.Where(s => StrTemplateName.Contains(s.TEMPLATENAME));
                }
                //if (!string.IsNullOrEmpty(strContent))
                //{
                //    q = q.Where(s => strContent.Contains(s.CONTENT));
                //}
                q = q.OrderByDescending(s => s.CREATEDATE);

                if (q.Count() > 0)
                {
                    return q;
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
                throw (ex);
            }
        }








    }
    #endregion
}
