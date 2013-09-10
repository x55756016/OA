using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.Foundation.Log;
using SMT.SaaS.OA.DAL;
using SMT_OA_EFModel;
using SMT.SaaS.OA.DAL.Views;
using System.Linq.Dynamic;

namespace SMT.SaaS.OA.BLL
{
    #region 租房记录费用
    public class HouseHireRecord : BaseBll<T_OA_HIRERECORD>
    {
        //private SMT_OA_EFModelContext hireRecordContext = new SMT_OA_EFModelContext();




        public IQueryable<T_OA_HIRERECORD> GetHireRecordById(string RecordID)
        {
            try
            {
                var ents = from q in dal.GetObjects().Include("T_OA_HIREAPP")
                           where q.HIRERECORD == RecordID
                           select q;
                return ents.Count() > 0 ? ents : null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("房源租赁记录HouseHireRecord-GetHireRecordById" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }




        public string AddHireRecord(T_OA_HIRERECORD hireRecordObj)
        {

            try
            {
                string StrReturn = "";
                var tempEnt = dal.GetObjects().FirstOrDefault(
                    s => s.RENTER == hireRecordObj.RENTER && s.RENTCOST == hireRecordObj.RENTCOST &&
                    s.SETTLEMENTDATE == hireRecordObj.SETTLEMENTDATE && s.T_OA_HIREAPP.HIREAPPID == hireRecordObj.T_OA_HIREAPP.HIREAPPID
                    && s.WATER == hireRecordObj.WATER && s.ELECTRICITY == hireRecordObj.ELECTRICITY
                    && s.ISSETTLEMENT == "0" && s.SETTLEMENTDATE == hireRecordObj.SETTLEMENTDATE);
                if (tempEnt != null)
                {
                    StrReturn = "REPETITION";
                }
                else
                {

                    Utility.RefreshEntity(hireRecordObj);
                    int i = dal.Add(hireRecordObj);

                    if (!(i > 0))
                    {
                        StrReturn = "SAVEFAILED";//保存失败
                    }
                }
                return StrReturn;

            }
            catch (Exception ex)
            {
                Tracer.Debug("房源租赁记录HouseHireRecord-AddHireRecord" + System.DateTime.Now.ToString() + " " + ex.ToString());
                throw (ex);
            }
        }

        public bool UpdateHireRecord(T_OA_HIRERECORD hireAppObj)
        {

            try
            {
                var users = from ent in dal.GetTable()
                            where ent.HIRERECORD == hireAppObj.HIRERECORD
                            select ent;

                if (users.Count() > 0)
                {
                    var user = users.FirstOrDefault();
                    Utility.CloneEntity(hireAppObj, user);
                    int i = dal.Update(user);
                    if (i > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }


                }
                return false;



            }
            catch (Exception ex)
            {
                Tracer.Debug("房源租赁记录HouseHireRecord-UpdateHireRecord" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
            }
        }

        //获取租房费用记录信息
        public IQueryable<V_HireRecord> GetHireRecordQueryWithPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string checkState, string userID)
        {
            try
            {
                var ents = from q in dal.GetObjects()
                           join n in
                               (
                                   from l in dal.GetObjects<T_OA_HIREAPP>()
                                   join k in
                                       (
                                          from o in dal.GetObjects<T_OA_HOUSELIST>()
                                          join m in dal.GetObjects<T_OA_HOUSEINFO>()
                                          on o.T_OA_HOUSEINFO.HOUSEID equals m.HOUSEID
                                          select new { o, m }
                                       )
                                   on l.T_OA_HOUSELIST.HOUSELISTID equals k.o.HOUSELISTID
                                   select new { l, k.o, k.m }
                                )
                            on q.T_OA_HIREAPP.HIREAPPID equals n.l.HIREAPPID
                           //where q.ISSETTLEMENT == checkState
                           select new V_HireRecord
                           {
                               houseAppObj = n.l,
                               houseInfoObj = n.m,
                               houseListObj = n.o,
                               HouseRecordObj = q,
                               OWNERCOMPANYID = q.OWNERCOMPANYID,
                               OWNERID = q.OWNERID,
                               OWNERPOSTID = q.OWNERPOSTID,
                               OWNERDEPARTMENTID = q.OWNERDEPARTMENTID,
                               CREATEUSERID = q.CREATEUSERID
                           };

                List<object> queryParas = new List<object>();
                queryParas.AddRange(paras);
                UtilityClass.SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_OA_HIRERECORD");
                if (!string.IsNullOrEmpty(filterString))
                {
                    ents = ents.ToList().AsQueryable().Where(filterString, queryParas.ToArray());
                }
                ents = ents.OrderBy(sort);
                ents = Utility.Pager<V_HireRecord>(ents, pageIndex, pageSize, ref pageCount);
                return ents;
            }
            catch (Exception ex)
            {
                Tracer.Debug("房源租赁记录HouseHireRecord-GetHireRecordQueryWithPaging" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }
        /// <summary>
        /// 获取租房员工的缴费
        /// </summary>
        /// <param name="EmployeeID"></param>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        public decimal GetEmployeeHireRecordFee(string EmployeeID, int year, int month)
        {
            try
            {
                decimal FeeCount = 0;
                var ents = from q in dal.GetObjects()
                           join n in
                               (
                                   from l in dal.GetObjects<T_OA_HIREAPP>()
                                   join k in
                                       (
                                          from o in dal.GetObjects<T_OA_HOUSELIST>()
                                          join m in dal.GetObjects<T_OA_HOUSEINFO>()
                                          on o.T_OA_HOUSEINFO.HOUSEID equals m.HOUSEID
                                          select new { o, m }
                                       )
                                   on l.T_OA_HOUSELIST.HOUSELISTID equals k.o.HOUSELISTID
                                   where l.OWNERID == EmployeeID && l.ISBACK == "0" && l.ISOK == "1" && l.CHECKSTATE == "2"
                                   //未退房、并确认
                                   select new { l, k.o, k.m }
                                )
                            on q.T_OA_HIREAPP.HIREAPPID equals n.l.HIREAPPID
                           where q.SETTLEMENTTYPE == "0" && q.ISSETTLEMENT == "0" && q.SETTLEMENTDATE.Year == year && q.SETTLEMENTDATE.Month == month
                           //where q.ISSETTLEMENT == checkState
                           select new V_HireRecord { houseAppObj = n.l, houseInfoObj = n.m, houseListObj = n.o, HouseRecordObj = q };
                if (ents.Count() > 0)
                {
                    T_OA_HIRERECORD record = ents.ToList().FirstOrDefault().HouseRecordObj;
                    FeeCount = (decimal)(record.RENTCOST + record.MANAGECOST + record.OTHERCOST + record.WATER + record.ELECTRICITY);
                }
                return FeeCount;
            }
            catch (Exception ex)
            {
                Tracer.Debug("房源租赁记录HouseHireRecord-GetEmployeeHireRecordFee" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return 0;
            }
        }

        /// <summary>
        /// 通过引擎调用生成记录 20100714
        /// </summary>
        /// <param name="hireappID"></param>
        public void GetHireAppToHireRecord(string hireappID)
        {
            try
            {
                var ents = from q in dal.GetObjects<T_OA_HIREAPP>()
                           where q.HIREAPPID == hireappID
                           select q;
                if (ents.Count() > 0)
                {
                    T_OA_HIREAPP hireApp = new T_OA_HIREAPP();
                    hireApp = ents.FirstOrDefault();
                    T_OA_HIRERECORD hirerecord = new T_OA_HIRERECORD();
                    hirerecord.HIRERECORD = System.Guid.NewGuid().ToString();
                    hirerecord.RENTER = hireApp.RENTTYPE;
                    hirerecord.T_OA_HIREAPP = hireApp;
                    hirerecord.MANAGECOST = Convert.ToDecimal(hireApp.MANAGECOST);
                    hirerecord.RENTCOST = Convert.ToDecimal(hireApp.RENTCOST);
                    hirerecord.WATER = 0;
                    hirerecord.ELECTRICITY = 0;
                    hirerecord.OTHERCOST = 0;
                    hirerecord.WATERNUM = 0;
                    hirerecord.ELECTRICITYNUM = 0;
                    hirerecord.SETTLEMENTDATE = System.DateTime.Now;
                    hirerecord.SETTLEMENTTYPE = hireApp.SETTLEMENTTYPE;//付款方式
                    hirerecord.ISSETTLEMENT = "0"; //是否结算

                    hirerecord.CREATEUSERID = hireApp.CREATEUSERID;
                    hirerecord.CREATEUSERNAME = hireApp.CREATEUSERNAME;
                    hirerecord.CREATEPOSTID = hireApp.CREATEPOSTID;
                    hirerecord.CREATEDEPARTMENTID = hireApp.CREATEDEPARTMENTID;
                    hirerecord.CREATECOMPANYID = hireApp.CREATECOMPANYID;
                    hirerecord.CREATEDATE = DateTime.Now;

                    hirerecord.OWNERID = hireApp.OWNERID;
                    hirerecord.OWNERNAME = hireApp.OWNERNAME;
                    hirerecord.OWNERPOSTID = hireApp.OWNERPOSTID;
                    hirerecord.OWNERDEPARTMENTID = hireApp.OWNERDEPARTMENTID;
                    hirerecord.OWNERCOMPANYID = hireApp.OWNERCOMPANYID;
                    int i = dal.Add(hirerecord);
                    string StrReturn = "";
                    if (!(i > 0))
                    {
                        StrReturn = "SAVEFAILED";//保存失败
                    }
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("房源租赁记录HouseHireRecord-GetHireAppToHireRecord" + System.DateTime.Now.ToString() + " " + ex.ToString());
                //StrReturn= "SAVEFAILED";
            }

        }


    }
    #endregion

}
