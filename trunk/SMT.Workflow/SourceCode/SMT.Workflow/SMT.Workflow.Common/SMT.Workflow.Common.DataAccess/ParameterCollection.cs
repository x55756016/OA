/*
版权信息：SMT
作    者：向寒咏
日    期：2009-09-22
内容摘要： 数据访问参数集合
*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMT.Workflow.Common.DataAccess
{
    /// <summary>
    /// 参数实体集合类
    /// </summary>
    [Serializable]
    public class ParameterCollection : IEnumerable, IEnumerator
    {
        #region 集合功能实现

        // 索引
        private int _position = -1;
        // 所包含的值
        private List<Parameter> _values = new List<Parameter>();

        /// <summary>
        /// 构造器
        /// </summary>
        public ParameterCollection()
        {
        }

        /// <summary>
        /// 通过索引器获取
        /// </summary>
        public Parameter this[int index]
        {
            get { return Get(index); }
            set { Set(value, index); }
        }

        /// <summary>
        /// 通过索引获取
        /// </summary>
        /// <param name="index">索引</param>
        /// <returns>实体</returns>
        public Parameter Get(int index)
        {
            return _values[index];
        }

        /// <summary>
        /// 设置实体
        /// </summary>
        /// <param name="param">实体</param>
        /// <param name="index">索引</param>
        public void Set(Parameter param, int index)
        {
            _values[index] = param;
        }

        /// <summary>
        /// 增加
        /// </summary>
        /// <param name="param">实体</param>
        public void Add(Parameter param)
        {
            if (param != null)
            {
                _values.Add(param);
            }
        }

        /// <summary>
        /// 通过索引移除
        /// </summary>
        /// <param name="index">索引</param>
        public void Remove(int index)
        {
            _values.RemoveAt(index);
        }

        /// <summary>
        /// 全部移除
        /// </summary>
        public void Clear()
        {
            _values.Clear();
        }

        /// <summary>
        /// 获取个数
        /// </summary>
        public int Count
        {
            get { return _values.Count; }
        }

        /// <summary>
        /// 增加
        /// </summary>
        /// <param name="parameterName">参数名</param>
        /// <param name="parameterValue">参数值</param>
        public void Add(string parameterName, object parameterValue)
        {
            Parameter p = new Parameter();
            p.ParameterName = parameterName;
            p.ParameterValue = parameterValue;
            _values.Add(p);
        }

        #endregion

        #region IEnumerable 成员

        /// <summary>
        /// 用于枚举
        /// </summary>
        /// <returns>枚举值</returns>
        public IEnumerator GetEnumerator()
        {
            return this;
        }

        #endregion

        #region IEnumerator 成员

        /// <summary>
        /// 重置集合
        /// </summary>
        public void Reset()
        {
            _position = -1;
        }

        /// <summary>
        /// 获取当前值
        /// </summary>
        public object Current
        {
            get
            {
                return Get(_position);
            }
        }

        /// <summary>
        /// 移到下个值
        /// </summary>
        /// <returns></returns>
        public bool MoveNext()
        {
            if (_position < this.Count - 1)
            {
                _position++;
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion
    }
}
