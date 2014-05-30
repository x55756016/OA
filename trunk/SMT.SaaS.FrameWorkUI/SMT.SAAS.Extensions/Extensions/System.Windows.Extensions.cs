using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
namespace System.Windows
{
    /// <summary>
    ///  Windows扩展方法
    /// </summary>
    public static class Extensions
    {
        # region 获取变换对象的二维矩阵值 Transform.GetMatrix()
        /// <summary>
        /// 获取变换对象的二维矩阵值
        /// </summary>
        /// <param name="transform">变换对象</param>
        /// <returns>对象二维矩阵</returns>
        public static Matrix GetMatrix(this Transform transform)
        {
            TransformGroup group;

            if (transform is TransformGroup)
            {
                group = (TransformGroup)transform;
            }
            else
            {
                group = new TransformGroup();
                group.Children.Add(transform);
            }

            return group.Value;
        }

        # endregion

        # region 获取FrameworkElement的父容器 FrameworkElement.GetFirstAncestorOrNull<T>()
        /// <summary>
        /// 获取对象的的容器，若不为NULL则返回对象否则返回NULL值
        /// </summary>
        /// <typeparam name="T">容器类型，对象必须为DependencyObject</typeparam>
        /// <param name="predicate">当前对象</param>
        /// <returns>根据类型返回的结果</returns>
        public static T GetFirstAncestorOrNull<T>(this DependencyObject predicate) where T : DependencyObject
        {
            var parent = VisualTreeHelper.GetParent(predicate);

            if (parent != null)
            {
                return parent as T ?? parent.GetFirstAncestorOrNull<T>();
            }

            return null;
        }

        /// <summary>
        /// 获取对象的的容器，若不为NULL则返回对象否则抛出异常
        /// </summary>
        /// <typeparam name="T">容器类型，对象必须为DependencyObject</typeparam>
        /// <param name="predicate">当前对象</param>
        /// <returns>根据类型返回的结果</returns>
        public static T GetFirstAncestor<T>(this DependencyObject predicate) where T : DependencyObject
        {
            var parent = predicate.GetFirstAncestorOrNull<T>();

            if (parent.IsNull())
            {
                throw new NullReferenceException(string.Format(
                    "There isn't a ancestor of type \"{0}\".", typeof(T).Name));
            }

            return parent;
        }

        # endregion

        # region 对象是否包含在给定对象中
        /// <summary>
        /// 判断对象是否继承自给定对象的子元素
        /// </summary>
        /// <param name="predicate">需要判断的对象</param>
        /// <param name="candidate">给定的对象</param>
        /// <returns>结果</returns>
        public static bool IsDescendantOf(this DependencyObject predicate, DependencyObject candidate)
        {
            var parent = VisualTreeHelper.GetParent(predicate);

            return parent.IsNotNull() && (parent == candidate || parent.IsDescendantOf(candidate));
        }

        # endregion

        # region 布尔值与容器可见度对象之间进行转换 bool.ToVisibility()   Visibility.ToBoolean()
        /// <summary>
        /// 根据Bool值返回容器能见度对象
        /// </summary>
        /// <param name="value">传入的bool值</param>
        /// <returns>能见度对象</returns>
        public static Visibility ToVisibility(this bool value)
        {
            return value ? Visibility.Visible : Visibility.Collapsed;
        }
 
        /// <summary>
        /// 根据容器可见度对象返回bool值
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool ToBoolean(this Visibility value)
        {
            return value == Visibility.Visible;
        }

        # endregion

        # region 获取DependencyObject对象的子元素集合
        /// <summary>
        /// 获取DependencyObject对象的子元素集合
        /// </summary>
        /// <param name="predicate">需要获取的对象</param>
        /// <returns>返回子元素集合</returns>
        public static IEnumerable<DependencyObject> GetDescendants(this DependencyObject predicate)
        {
            var count = VisualTreeHelper.GetChildrenCount(predicate);

            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(predicate, i);

                yield return child;

                foreach (var _child in child.GetDescendants())
                {
                    yield return _child;
                }
            }
            ///使用”yield return”关键词组时，.net会为你生成一大串管道代码，你可以尽管假装这是个魔法。
            ///当开始在被调用的代码中循环时（这里不是list),实现上发生的是这个函数被一遍一遍的调用，
            ///但每一次都从上一次执行退出的部分开始继续执行
            ///Yield的执行方法
            ///1.调用函数 
            ///2.调用者请求item 
            ///3.下一个item返回
            ///4.回到步骤2 
        }

        # endregion

        # region PrepareAndGetTransform

        public static T PrepareAndGetTransform<T>(this UIElement predicate) where T : Transform
        {
            TransformGroup group;
            var transform = predicate.RenderTransform;

            if (transform.IsNull())
            {
                transform = predicate.RenderTransform = Activator.CreateInstance<T>();
            }
            else
            {
                var desiredType = typeof(T);

                if (transform is TransformGroup)
                {
                    group = (TransformGroup)transform;

                    transform = group.Children.SingleOrDefault(t => t.GetType() == desiredType);

                    if (transform.IsNull())
                    {
                        transform = Activator.CreateInstance<T>();
                        group.Children.Add(transform);
                    }
                }
                else
                {
                    var currentType = transform.GetType();

                    if (currentType != desiredType)
                    {
                        group = new TransformGroup();

                        transform = Activator.CreateInstance<T>();

                        group.Children.Add(transform);

                        predicate.RenderTransform = group;
                    }
                }
            }
            return (T)transform;
        }

        # endregion

        # region 两个Point之间相减

        public static Point Subtract(this Point minuend, Point subtrahend)
        {
            return new Point(minuend.X - subtrahend.X, minuend.Y - subtrahend.Y);
        }

        # endregion

        # region SetMarginLeft

        public static void SetMarginLeft(this FrameworkElement element, double left)
        {
            var margin = element.Margin;
            margin.Left = left;
            element.Margin = margin;
        }

        # endregion

        # region SetMarginTop

        public static void SetMarginTop(this FrameworkElement element, double Top)
        {
            var margin = element.Margin;
            margin.Top = Top;
            element.Margin = margin;
        }

        # endregion

        # region SetMarginRight

        public static void SetMarginRight(this FrameworkElement element, double Right)
        {
            var margin = element.Margin;
            margin.Right = Right;
            element.Margin = margin;
        }

        # endregion

        # region SetMarginBottom

        public static void SetMarginBottom(this FrameworkElement element, double Bottom)
        {
            var margin = element.Margin;
            margin.Bottom = Bottom;
            element.Margin = margin;
        }

        # endregion

        # region 获取对象的Content GetContentProperty

        public static PropertyInfo GetContentProperty(this FrameworkElement container)
        {
            var type = container.GetType();
            //获取用户定义的Content类型的属性对象集合的第一个元素或默认元素
            var contentProperty = type.GetCustomAttributes(
                typeof(ContentPropertyAttribute), true).FirstOrDefault() as ContentPropertyAttribute;

            if (contentProperty.IsNull())
            {
                throw new MissingMemberException(
                    "未发现对象中具有内容属性(ContentProperty).");
            }
            else
            {
                //根据属性名称返回
                var property = type.GetProperty(contentProperty.Name);
                return property;
            }
        }

        # endregion

        # region 将对象显示到前面 BringToFront

        public static void TryBringToFront(this UIElement element)
        {
            try
            {
                //element.BringToFront();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static void BringToFront(this UIElement element)
        {
            var parent = VisualTreeHelper.GetParent(element);

            if (parent == null)
            {
                throw new InvalidOperationException(
                    "未发现给定元素的父容器.");
            }
            if (!(parent is Panel))
            {
                throw new InvalidOperationException(
                    "给定元素的父对象不为Panel.");
            }

            var panel = (Panel)parent;

            Extensions.BringToFront(panel, element);
        }

        public static void BringToFront(Panel panel, UIElement element)
        {
            var collection = panel.Children;
            var count = collection.Count;

            var index = Canvas.GetZIndex(element);

            Canvas.SetZIndex(element, count - 1);

            for (int i = count - 1; i >= 0; i--)
            {
                var member = collection[i];

                var j = Canvas.GetZIndex(member);

                if (member != element && j > index)
                {
                    Canvas.SetZIndex(member, j - 1);
                }
            }
        }

        # endregion
    }
}
