using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using SMT_FB_EFModel;
using System.Data.Objects.DataClasses;
using System.Data.Objects;
using System.Collections;
using System.Reflection;

using FlowWFService = SMT.SaaS.BLLCommonServices.FlowWFService;
using System.Runtime.Serialization;
using System.Xml.Linq;

namespace SMT.FB.BLL
{
    public class FBCommonBLL : SaveEntityBLL
    {
  


        #region 2.	数据保存操作方法
        /// <summary>
        /// 保存实体
        /// </summary>
        /// <param name="fbEntityList"></param>
        /// <returns></returns>
        public SaveResult Save(SaveEntity saveEntity)
        {

            SaveResult result = new SaveResult();
            try
            {
                result.FBEntity = base.SaveEntityBLLSave(saveEntity);
                result.Successful = true;
            }
            catch (FBBLLException ex)
            {
                result.Successful = false;
                result.Exception = ex.Message;
                SystemBLL.Debug(ex.ToString());

            }
            return result;
        }

      
     
        #endregion

     



    }


}
