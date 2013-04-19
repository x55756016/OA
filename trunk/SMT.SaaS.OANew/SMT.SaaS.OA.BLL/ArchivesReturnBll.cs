using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT.SaaS.OA.DAL;
using SMT_OA_EFModel;
using SMT.SaaS.OA.DAL.Views;
using System.Linq.Dynamic;

namespace SMT.SaaS.OA.BLL
{
    public class ArchivesReturnBll : BaseBll<T_OA_LENDARCHIVES>
    {
        //private SMT_OA_EFModelContext archivesContext = new SMT_OA_EFModelContext();
        //private ArchivesLendingDal archivesLendingDal = new ArchivesLendingDal();
        //private ArchivesManagementDal archivesDal = new ArchivesManagementDal();
        /// <summary>
        /// 根据用户编号查询已借阅的档案信息
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        public List<T_OA_LENDARCHIVES> GetArchivesReturnInfo(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID)
        {
            
            var ents = from a in dal.GetObjects().Include("T_OA_ARCHIVES")
                        where !a.ENDDATE.HasValue && a.CHECKSTATE == "2"
                        select a;
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);
            UtilityClass.SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_OA_LENDARCHIVES");
            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.ToList().AsQueryable().Where(filterString, queryParas.ToArray());
            }
            ents = ents.OrderBy(sort);
            ents = Utility.Pager<T_OA_LENDARCHIVES>(ents, pageIndex, pageSize, ref pageCount);
            return ents.ToList();
                        
            
        }

        public bool AddArchivesReturn(T_OA_LENDARCHIVES archivesObj)
        {
            try
            {              
                

                T_OA_LENDARCHIVES tmpobj = dal.GetObjectByEntityKey(archivesObj.EntityKey) as T_OA_LENDARCHIVES;
                tmpobj.T_OA_ARCHIVES = dal.GetObjectByEntityKey(archivesObj.T_OA_ARCHIVES.EntityKey) as T_OA_ARCHIVES;
                //archivesContext.ApplyPropertyChanges(archivesObj.EntityKey.EntitySetName, archivesObj);
                int i = dal.Update(archivesObj);
                if (i > 0)
                {
                    return true;
                }
                return false;
                
            }
            catch (Exception ex)
            {
                //return false;
                throw (ex);
            }
        }

        

    }
}
