using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMT.HRM.BLL
{
    public class CustomResult
    {
        /// <summary>
        /// 结果的值，大于0表示正常，小于等于0表示有错误
        /// </summary>
        public int ResultValue { get; set; }
        /// <summary>
        /// 错误编码，可用于实现多语言
        /// </summary>
        public string ResultCode { get; set; }
        /// <summary>
        /// 错误的详细信息
        /// </summary>
        public string ResultMessage { get; set; }
    }
}
