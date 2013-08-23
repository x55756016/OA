using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMT.Workflow.SMTCache
{
    public class CacheObject<T>
    {
        private DateTime createtime;
        /// <summary>
        /// 缓存时间
        /// </summary>
        public DateTime CreateTime
        {
            get
            {
                return createtime;
            }
            set { createtime = value; }
        }
        private T obj;
        /// <summary>
        /// 缓存对象
        /// </summary>
        public T Object
        {
            get
            {
                return obj;
            }
            set
            {
                obj = value;
            }
        }
        public CacheObject(T obj)
        {
            this.createtime = DateTime.Now;
            this.obj = obj;
        }
    }
}
