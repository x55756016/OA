using System;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Collections.Generic;
using System.Text;
using SMT_OA_EFModel;
using SMT.SaaS.OA.BLL;

namespace SMT.SaaS.OA.Services
{

    public partial class SmtOAPersonOffice
    {
        private OrderMealManagementBll MealBll = new OrderMealManagementBll();
        [OperationContract]
        //添加        
        public string OrderMealInfoAdd(T_OA_ORDERMEAL obj)
        {
            return MealBll.AddOrderMeal(obj);
            
        }


        [OperationContract]
        private bool IsExistOrderMealInfoByAdd(string StrTitle,string StrDepart , string StrCreatUser, DateTime startdt)
        {
            return MealBll.GetOrderMealInfoByAdd(StrTitle,StrDepart,StrCreatUser, startdt);
            

        }

        [OperationContract]
        public bool OrderMealInfoDel(string[] MealIDs)
        {
            return MealBll.BatchDeleteOrderMealInfos(MealIDs);

        }


        [OperationContract]
        public bool OrderMealInfoUpdate(T_OA_ORDERMEAL obj)
        {
            return MealBll.UpdateOrderMeal(obj);
        }




        [OperationContract]
        public T_OA_ORDERMEAL GetOrderMealSingleInfoById(string StrMealInfoId)
        {
            return MealBll.GetOrderMealInfoById(StrMealInfoId);
        }


        //[OperationContract]
        ////获取信息
        //public List<T_OA_ORDERMEAL> GetOrderMealInfos(string StrUserID,string StrState)
        //{
        //    List<T_OA_ORDERMEAL> OrderMealInfosList = MealBll.GetOrderMealInfos(StrUserID,StrState);
        //    if (OrderMealInfosList == null)
        //    {
        //        return null;
        //    }
        //    else
        //    {
        //        return OrderMealInfosList;
        //    }
        //}


        [OperationContract]
        public List<T_OA_ORDERMEAL> GetOrderMealInfos(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, LoginUserInfo loginUserInfo)
        {

            IQueryable<T_OA_ORDERMEAL> GradeList = MealBll.GetOrderMealInfos(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, loginUserInfo.userID);

            return GradeList != null ? GradeList.ToList() : null;
        }

        
        
        [OperationContract]
        public List<T_OA_ORDERMEAL> GetOrderMealInfosListByTitleTimeSearch(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, LoginUserInfo loginUserInfo)
        {

            IQueryable<T_OA_ORDERMEAL> OrderMealInfoList = MealBll.GetOrderMealInfosListByTitleContentTimeSearch(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, loginUserInfo.userID);

            return OrderMealInfoList != null ? OrderMealInfoList.ToList() : null;
        }


        


        

    }
}
