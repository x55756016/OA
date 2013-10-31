// -----------------------------------------------------------------------
// <copyright file="V_PermissionUpdateState.cs" company="SMT-ONLINE">
// TODO: 权限的更新状态
// </copyright>
// -----------------------------------------------------------------------

namespace SMT.SaaS.Permission.DAL
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// 包含更新时间和相应表的数据量
    /// </summary>
    public class V_PermissionUpdateState
    {
        /// <summary>
        /// 更新时间
        /// </summary>
        public Dictionary<string, DateTime?> Timer { get; set; }
        /// <summary>
        /// 现有数据量
        /// </summary>
        public Dictionary<string, int> Counter { get; set; }
    }
}
