using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT.HRM.DAL;
using SMT.HRM.CustomModel;
using System.Data;

namespace SMT.HRM.BLL
{
    public class PersonalLandStatisticBLL : BaseBll<V_LandStatistic>
    {
        public List<V_LandStatistic> GetPersonalLandStatisticListByMultSearch(string strOwnerID, string strOrderBy, string filterString, List<object> objArgs)
        {
            List<V_LandStatistic> entList = new List<V_LandStatistic>();
            PersonalLandStatisticDAL dalPersonalLandStatistic = new PersonalLandStatisticDAL();
            if (!string.IsNullOrEmpty(strOwnerID))
            {
                SetOrganizationFilter(ref filterString, ref objArgs, strOwnerID, "V_LandStatistic");
            }

            if (string.IsNullOrEmpty(strOrderBy))
            {
                strOrderBy = "p.ownercompanyid";
            }

            bool isCountSub = true;    //计算子公司的登录人数
            entList = dalPersonalLandStatistic.GetPersonalLandStatisticList(isCountSub, strOrderBy, filterString, objArgs.ToArray());
            return entList;
        }

        /// <summary>
        /// 获取部门的执行一览的导出数据
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
        public byte[] OutFileLandStatisticList(string strOwnerID, string strOrderBy, string filterString, List<object> objArgs)
        {
            List<V_LandStatistic> entList = new List<V_LandStatistic>();
            PersonalLandStatisticDAL dalPersonalLandStatistic = new PersonalLandStatisticDAL();
            if (!string.IsNullOrEmpty(strOwnerID))
            {
                SetOrganizationFilter(ref filterString, ref objArgs, strOwnerID, "V_LandStatistic");
            }

            if (string.IsNullOrEmpty(strOrderBy))
            {
                strOrderBy = "p.ownercompanyid";
            }

            bool isCountSub = true;    //计算子公司的登录人数
            entList = dalPersonalLandStatistic.GetPersonalLandStatisticList(isCountSub, strOrderBy, filterString, objArgs.ToArray());

            DataTable dt = MakeTableToExportLandStatistic();
            DataTable dtExport = GetExportDataForLandStatistic(dt, entList);

            return Utility.OutFileStream("员工登录记录统计", dtExport);
        }

        private DataTable MakeTableToExportLandStatistic()
        {
            DataTable dt = new DataTable();
            DataColumn colOrgObj = new DataColumn();
            colOrgObj.ColumnName = "机构";
            colOrgObj.DataType = typeof(string);
            dt.Columns.Add(colOrgObj);
            
            DataColumn colJanTimes = new DataColumn();
            colJanTimes.ColumnName = "一月";
            colJanTimes.DataType = typeof(decimal);
            dt.Columns.Add(colJanTimes);

            DataColumn colFebTimes = new DataColumn();
            colFebTimes.ColumnName = "二月";
            colFebTimes.DataType = typeof(decimal);
            dt.Columns.Add(colFebTimes);

            DataColumn colMarTimes = new DataColumn();
            colMarTimes.ColumnName = "三月";
            colMarTimes.DataType = typeof(decimal);
            dt.Columns.Add(colMarTimes);

            DataColumn colAprTimes = new DataColumn();
            colAprTimes.ColumnName = "四月";
            colAprTimes.DataType = typeof(decimal);
            dt.Columns.Add(colAprTimes);

            DataColumn colMayTimes = new DataColumn();
            colMayTimes.ColumnName = "五月";
            colMayTimes.DataType = typeof(decimal);
            dt.Columns.Add(colMayTimes);

            DataColumn colJunTimes = new DataColumn();
            colJunTimes.ColumnName = "六月";
            colJunTimes.DataType = typeof(decimal);
            dt.Columns.Add(colJunTimes);

            DataColumn colJulTimes = new DataColumn();
            colJulTimes.ColumnName = "七月";
            colJulTimes.DataType = typeof(decimal);
            dt.Columns.Add(colJulTimes);

            DataColumn colAugTimes = new DataColumn();
            colAugTimes.ColumnName = "八月";
            colAugTimes.DataType = typeof(decimal);
            dt.Columns.Add(colAugTimes);

            DataColumn colSepTimes = new DataColumn();
            colSepTimes.ColumnName = "九月";
            colSepTimes.DataType = typeof(decimal);
            dt.Columns.Add(colSepTimes);

            DataColumn colOctTimes = new DataColumn();
            colOctTimes.ColumnName = "十月";
            colOctTimes.DataType = typeof(decimal);
            dt.Columns.Add(colOctTimes);

            DataColumn colNovTimes = new DataColumn();
            colNovTimes.ColumnName = "十一月";
            colNovTimes.DataType = typeof(decimal);
            dt.Columns.Add(colNovTimes);

            DataColumn colDecTimes = new DataColumn();
            colDecTimes.ColumnName = "十二月";
            colDecTimes.DataType = typeof(decimal);
            dt.Columns.Add(colDecTimes);
            
            DataColumn colSubtotal = new DataColumn();
            colSubtotal.ColumnName = "小计";
            colSubtotal.DataType = typeof(decimal);
            dt.Columns.Add(colSubtotal);
            return dt;
        }

        private DataTable GetExportDataForLandStatistic(DataTable dt, List<V_LandStatistic> ents)
        {
            for (int i = 0; i < ents.Count(); i++)
            {
                DataRow row = dt.NewRow();
                for (int n = 0; n < dt.Columns.Count; n++)
                {
                    switch (n)
                    {
                        case 0:
                            row[n] = ents[i].OrganizationName;
                            break;
                        case 1:
                            row[n] = ents[i].JanTimes;
                            break;
                        case 2:
                            row[n] = ents[i].FebTimes;
                            break;
                        case 3:
                            row[n] = ents[i].MarTimes;
                            break;
                        case 4:
                            row[n] = ents[i].AprTimes;
                            break;
                        case 5:
                            row[n] = ents[i].MayTimes;
                            break;
                        case 6:
                            row[n] = ents[i].JunTimes;
                            break;
                        case 7:
                            row[n] = ents[i].JulTimes;
                            break;
                        case 8:
                            row[n] = ents[i].AugTimes;
                            break;
                        case 9:
                            row[n] = ents[i].SepTimes;
                            break;
                        case 10:
                            row[n] = ents[i].OctTimes;
                            break;
                        case 11:
                            row[n] = ents[i].NovTimes;
                            break;
                        case 12:
                            row[n] = ents[i].DecTimes;
                            break;
                        case 13:
                            row[n] = ents[i].Subtotal;
                            break;
                    }
                }

                dt.Rows.Add(row);
            }

            return dt;
        }
    }
}
