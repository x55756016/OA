/*---------------------------------------------------------------------  
	 * 版　权：Copyright ©   2011    
	 * 文件名：FlowCategoryDAL.cs  
	 * 创建者： 亢晓方
	 * 创建日期：2011/11/9 9:07:17   
	 * CLR版本： 4.0.30319.239  
	 * 命名空间：SMT.Workflow.Platform.DAL 
	 * 模块名称：
	 * 描　　述： 	 
* ---------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.Workflow.Common.Model;
using System.Data.OracleClient;

namespace SMT.Workflow.Platform.DAL
{
    public class FlowCategoryDAL : BaseDAL
    {

        /// <summary>
        /// 判断是否存在类型名称
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        public int GetExistsFlowCategory(string Name)
        {
            try
            {
                string sql = "SELECT  count(FLOWCATEGORYID)  FROM FLOW_FLOWCATEGORY WHERE FLOWCATEGORYDESC='" + Name + "'";
                dao.Open();
                object count = dao.ExecuteScalar(sql);
                return Int16.Parse(count.ToString());
            }
            catch (Exception ex)
            {
                Log.WriteLogFrist(this, "GetExistsFlowCategory()", "判断是否存在流程类型", ex);
                return 0;
            }
            finally
            {
                dao.Close();
            }
        }
        /// <summary>
        /// 新增流程类型
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public int AddFlowCategory(FLOW_FLOWCATEGORY entity)
        {
            try
            {
                //string sql = "INSERT INTO FLOW_FLOWCATEGORY ( FLOWCATEGORYID,FLOWCATEGORYDESC) VALUES ('" + entity.FLOWCATEGORYID + "','" + entity.FLOWCATEGORYDESC + "' )";
                //dao.Open();
                //int result = dao.ExecuteNonQuery(sql);
                //return result;
                dao.Open();
                string insSql = "INSERT INTO FLOW_FLOWCATEGORY (FLOWCATEGORYID,FLOWCATEGORYDESC,COMPANYID) VALUES (:FLOWCATEGORYID,:FLOWCATEGORYDESC,:COMPANYID)";
            OracleParameter[] pageparm =
                { 
                    new OracleParameter(":FLOWCATEGORYID",GetValue(entity.FLOWCATEGORYID)), //流程分类ID 
                    new OracleParameter(":FLOWCATEGORYDESC",GetValue(entity.FLOWCATEGORYDESC)), //流程分类描述 
                     new OracleParameter(":COMPANYID",GetValue(entity.COMPANYID)) //公司ID                

                };         
               int result= dao.ExecuteNonQuery(insSql, System.Data.CommandType.Text, pageparm);
               return result;
            }
            catch (Exception ex)
            {
                Log.WriteLog(this, "AddFlowCategory()", "新增流程类型", ex);
                return 0;
            }
            finally
            {
                dao.Close();
            }
        }

        /// <summary>
        /// 修改流程类型
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public int UpdateFlowCategory(FLOW_FLOWCATEGORY entity)
        {
            try
            {
               // string sql = "UPDATE FLOW_FLOWCATEGORY SET FLOWCATEGORYDESC='" + entity.FLOWCATEGORYDESC + "' WHERE FLOWCATEGORYID= '" + entity.FLOWCATEGORYID + "'";
                dao.Open();

                 string updSql = "UPDATE FLOW_FLOWCATEGORY SET FLOWCATEGORYDESC=:FLOWCATEGORYDESC WHERE   FLOWCATEGORYID=:FLOWCATEGORYID";
                OracleParameter[] pageparm =
                { 
                    new OracleParameter(":FLOWCATEGORYID",GetValue(entity.FLOWCATEGORYID)), //流程分类ID 
                    new OracleParameter(":FLOWCATEGORYDESC",GetValue(entity.FLOWCATEGORYDESC)) //流程分类描述 

                };
                int result = dao.ExecuteNonQuery(updSql, System.Data.CommandType.Text, pageparm);
                return result;

            }
            catch (Exception ex)
            {
                Log.WriteLogFrist(this, "UpdateFlowCategory()", "修改流程类型", ex);
                return 0;
            }
            finally
            {
                dao.Close();
            }
        }

        /// <summary>
        /// 删除流程类型
        /// </summary>
        /// <param name="CategoryID"></param>
        /// <returns></returns>
        public int DeleteFlowCategory(string CategoryID)
        {
            try
            {
                string sql = "DELETE FLOW_FLOWCATEGORY WHERE  FLOWCATEGORYID= '" + CategoryID + "'";
                dao.Open();
                int result = dao.ExecuteNonQuery(sql);
                return result;

            }
            catch (Exception ex)
            {
                Log.WriteLogFrist(this, "DeleteFlowCategory()", "流程类型", ex);
                return 0;
            }
            finally
            {
                dao.Close();
            }
        }
    }
}
