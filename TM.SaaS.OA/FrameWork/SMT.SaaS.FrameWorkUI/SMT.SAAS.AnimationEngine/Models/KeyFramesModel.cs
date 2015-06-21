using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace SMT.SAAS.AnimationEngine.Model
{
    public class KeyFramesModel<T>:BaseModel
    {
        /// <summary>
        /// 动画中的关键帧集合
        /// </summary>
        public KeyFrames<T>[] KeyFrames { get; set; }
    }
}
