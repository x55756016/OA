using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Data.Objects.DataClasses;
using SMT_FB_EFModel;
using SMT.FB.BLL;
using System.Collections;

namespace SMT.FB.Services
{
    // 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码、svc 和配置文件中的类名“OaServerce”。
    public class OaServerce : IOaService
    {
        public void DoWork()
        {
        }

        #region IOaServerce 成员


        /// <summary>
        /// 根据扩展单编号查询申请单编号。
        /// </summary>
        /// <param name="extensionalOrderID">扩展单编号。</param>
        /// <returns>返回申请单编号。</returns>
        public string GetTravelExpApplyMasterCode(string extensionalOrderID)
        {
            using (FBCommonBLL fbCommonBLL = new FBCommonBLL())
            {
                if (string.IsNullOrWhiteSpace(extensionalOrderID))
                {
                    return string.Empty;
                }

                QueryExpression qe = QueryExpression.Equal("T_FB_EXTENSIONALORDER.EXTENSIONALORDERID", extensionalOrderID);
                qe.QueryType = typeof(T_FB_TRAVELEXPAPPLYMASTER).Name;

                FBEntity entFBEntity = fbCommonBLL.GetFBEntity(qe);
                if (entFBEntity == null)
                {
                    return string.Empty;
                }

                if (entFBEntity.Entity == null)
                {
                    return string.Empty;
                }

                return ((T_FB_TRAVELEXPAPPLYMASTER)entFBEntity.Entity).TRAVELEXPAPPLYMASTERID;
            }
        }

        public List<EntityObject> GetTravelExpApplyMaster(string travelExpApplyMasterID)
        {
            using (FBCommonBLL bll = new FBCommonBLL())
            {
                List<EntityObject> list = new List<EntityObject>();
                QueryExpression qe = QueryExpression.Equal("T_FB_EXTENSIONALORDER.TRAVELEXPAPPLYMASTERID", travelExpApplyMasterID);
                qe.QueryType = typeof(T_FB_TRAVELEXPAPPLYMASTER).Name;
                FBEntity entitys = bll.GetFBEntity(qe);

                if (entitys == null || entitys.Entity == null)
                {
                    return null;
                }
                IEnumerable listResult = entitys as IEnumerable;
                foreach (var entity in listResult)
                {
                    list.Add(entity as EntityObject);
                }

                return list;
            }
        }

        #endregion
    }
}
