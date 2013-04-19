
/*
 * 文件名：AttendanceSolutionAsignDAL.cs
 * 作  用：考勤方案应用 数据操作类
 * 创建人：吴鹏
 * 创建时间：2010-3-5 11:10:39
 * 修改人：
 * 修改时间：
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Collections;
using System.Data.Objects.DataClasses;
using System.Linq.Dynamic;
using System.Linq.Expressions;

using SMT_HRM_EFModel;
namespace SMT.HRM.DAL
{
    public class AttendanceSolutionAsignDAL : CommDal<T_HR_ATTENDANCESOLUTIONASIGN>
    {
        public AttendanceSolutionAsignDAL()
        {
        }


        /// <summary>
        /// xiedx
        /// 2012-8-20
        /// 判断时间是否重叠
        /// </summary>
        /// <returns>True/False(是/否)</returns>
        public bool IsExistsDateTime(string dateTime, string strAttendanceSolutionID, List<string> strAssignedobjectID)
        {
            //检查重复记录，是为了检查同一机构的同一考勤方案的分配记录，是否有时间重叠的记录；
            //将上面的问题分解开，1.首先检查在这个机构对应的机构类型，有没有考勤方案分配记录；
            //2.如果没有，即可返回false；
            //3.如果有，对上叙第1步的记录再进行时间上的查询过滤(ENDDATE > DateTime.Parse(dateTime))；
            //4.如果查询后记录不存在，即可返回false；
            //5.如果查询后记录存在，对上叙第3步的记录再进行循环判断（for/foreach）；
          
            bool flagDateTime = false;
            var q = from v in GetObjects().Include("T_HR_ATTENDANCESOLUTION")
                    where v.T_HR_ATTENDANCESOLUTION.ATTENDANCESOLUTIONID == strAttendanceSolutionID && v.CHECKSTATE != "3"
                    select v;
         
            //请原谅我用了这么多循环，还没想到好的办法
            foreach (var entity in q)
            {
                string[] strlistID = entity.ASSIGNEDOBJECTID.ToString().Split(',');

                for (int j = 0; j < strlistID.Count(); j++)
                    for (int i = 0; i < strAssignedobjectID.Count();i++ )
                        if (strAssignedobjectID[i] == strlistID[j])
                        {
                            //判断这个日期是否在范围内，在的话就不对，时间就重叠了
                            if ((DateTime.Parse(entity.STARTDATE.ToString()) <= DateTime.Parse(dateTime)) && (DateTime.Parse(dateTime) <= DateTime.Parse(entity.ENDDATE.ToString())))
                            {
                                flagDateTime = true;
                            }
                        }
                
            }

                return flagDateTime;
            
        }


        /// <summary>
        /// xiedx
        /// 2012-8-21
        /// 获取有相同的ASSIGNEDOBJECTID,只要有一个相同的就不保存
        /// </summary>
        /// <param name="strFilter">查询语句</param>
        /// <param name="objArgs">查询参数集合</param>
        /// <returns>IsExistsID（ASSIGNEDOBJECTID）</returns>
        public List<string> GetExistsID(string strFilter, params string[] objArgs)
        {
  
            string[] strlist = objArgs[1].ToString().Split(',');
            List<string> strAssignedobjectID = new List<string>();

            //这里要把传入的值（objArgs）赋值给一个参数（strID）再带进linq语句，不然出现错误，
            //linq执行的时候要转换，可以试试
            string strID = objArgs[0].ToString();
            var q = from v in GetObjects().Include("T_HR_ATTENDANCESOLUTION")
                    where v.T_HR_ATTENDANCESOLUTION.ATTENDANCESOLUTIONID == strID && v.CHECKSTATE != "3"
                    select v;

            //咳咳，这些循环感觉还得得用，而且这还没去除重复值，得想想办法
                foreach (var temp in q)
                {
                    string[] strlistID = temp.ASSIGNEDOBJECTID.ToString().Split(',');
                    for (int i = 0; i < strlist.Count()-1; i++)
                    {
                        for (int j = 0; j < strlistID.Count(); j++)
                            if (strlistID[j] == strlist[i])
                            {
                                strAssignedobjectID.Add(strlist[i]);
                             
                            }
                    }
                }

             
            return strAssignedobjectID;
        }

        public bool IsExistSame(string strFilter, params object[] objArgs)
        {
            //xiedx
            //2012-8-21
            //这个函数看起来没用，但之前存在，不用也不好，而且一般会让一个用一种方案而且还是不同时间段
            //有时候还要选几个，其中还有重复的人，这样选择的人除了测试人员和忘记选了哪些人的之外，应该没有
            //所以用上这个函数，如果返回false那么就不要执行以后的函数了
            string[] strlist = objArgs[1].ToString().Split(',');
            string strAssignedobjectID = objArgs[0].ToString();
            bool flag = false;
            var q = from v in GetObjects().Include("T_HR_ATTENDANCESOLUTION")
                    where v.T_HR_ATTENDANCESOLUTION.ATTENDANCESOLUTIONID == strAssignedobjectID && v.CHECKSTATE != "3"
                    select v;
            if (q.Count() > 0)
            {
                //请再一次原谅我用了这么多循环...
                foreach (var temp in q)
                {
                    string[] strlistID = temp.ASSIGNEDOBJECTID.ToString().Split(',');
                    for (int i = 0; i < strlist.Count() - 1; i++)
                    {
                        for (int j = 0; j < strlistID.Count(); j++)
                            if (strlist[i] == strlistID[j])
                            {
                                flag = true;
                                break;
                            }
                    }
                }
            }
            return flag;
        }

        /// <summary>
        /// 根据参数，检查是否存在指定记录(方便进行数据增删改)
        /// </summary>
        /// <param name="strFilter">查询语句</param>
        /// <param name="objArgs">查询参数集合</param>
        /// <returns>True/False(是/否)</returns>
        public bool IsExistsRd(string strFilter, params object[] objArgs)
        {
            bool flag = false;

            var q = from v in GetObjects().Include("T_HR_ATTENDANCESOLUTION")
                    select v;

            if (objArgs.Count() <= 0 || string.IsNullOrEmpty(strFilter))
            {
                return flag;
            }

            q = q.Where(strFilter, objArgs);

            if (q.Count() > 0)
            {

                flag = true;
            }
            
           
            return flag;
        }

        /// <summary>
        /// 获取指定条件的考勤方案应用信息
        /// </summary>        
        /// <param name="strFilter">查询语句</param>
        /// <param name="objArgs">查询参数集合</param>
        /// <returns>返回考勤方案应用信息</returns>
        public T_HR_ATTENDANCESOLUTIONASIGN GetAttendanceSolutionAsignRdByMultSearch(string strFilter, params object[] objArgs)
        {
            var q = from v in GetObjects().Include("T_HR_ATTENDANCESOLUTION")
                    select v;

            if (objArgs.Count() > 0 && !string.IsNullOrEmpty(strFilter))
            {
                q = q.Where(strFilter, objArgs);
            }

            return q.First();
        }

        /// <summary>
        /// 获取指定条件的考勤方案应用信息
        /// </summary>        
        /// <param name="strOrderBy">排序字段</param>
        /// <param name="strFilter">查询语句</param>
        /// <param name="objArgs">查询参数集合</param>
        /// <returns>返回考勤方案应用信息</returns>
        public IQueryable<T_HR_ATTENDANCESOLUTIONASIGN> GetAttendanceSolutionAsignRdListByMultSearch(string strOrderBy, string strFilter, params object[] objArgs)
        {
            var q = from v in GetObjects().Include("T_HR_ATTENDANCESOLUTION")                    
                    select v;

            if (objArgs.Count() > 0 && !string.IsNullOrEmpty(strFilter))
            {
                q = q.Where(strFilter, objArgs);
            }

            return q.OrderBy(strOrderBy);
        }
    }
}