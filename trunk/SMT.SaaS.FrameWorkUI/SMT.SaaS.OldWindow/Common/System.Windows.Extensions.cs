// (c) Copyright Vitor de Souza (sincorde.com)
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

# region Using Directives

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

# endregion

namespace System.Windows
{
    public static class Extensions
    {
        # region Transform.GetMatrix()

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

        # region FrameworkElement.GetFirstAncestorOrNull<T>()

        //public static T GetFirstAncestorOrNull<T>(this FrameworkElement predicate) where T : FrameworkElement
        //{
        //    var parent = predicate.Parent as FrameworkElement;

        //    if (parent != null)
        //    {
        //        return parent as T ?? (predicate.Parent as FrameworkElement).GetFirstAncestorOrNull<T>();
        //    }

        //    return null;
        //} 

        //public static T GetFirstAncestor<T>(this FrameworkElement predicate) where T : FrameworkElement
        //{
        //    var parent = predicate.GetFirstAncestorOrNull<T>();

        //    if (parent.IsNull())
        //    {
        //        throw new NullReferenceException(string.Format(
        //            "There isn't a ancestor of type \"{0}\".", typeof(T).Name));
        //    }

        //    return parent;
        //}
        public static T GetFirstAncestorOrNull<T>(this DependencyObject predicate) where T : DependencyObject
        {
            var parent = VisualTreeHelper.GetParent(predicate);

            if (parent != null)
            {
                return parent as T ?? parent.GetFirstAncestorOrNull<T>();
            }

            return null;
        }

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

        # region // Size.IsInfinity()

        //public static bool IsInfinity(this Size predicate)
        //{
        //    return
        //        double.IsInfinity(predicate.Width) ||
        //        double.IsInfinity(predicate.Height);
        //}

        # endregion

        # region IsDescendantOf

        public static bool IsDescendantOf(this DependencyObject predicate, DependencyObject candidate)
        {
            var parent = VisualTreeHelper.GetParent(predicate);

            return parent.IsNotNull() && (parent == candidate || parent.IsDescendantOf(candidate));
        }

        # endregion

        # region bool.ToVisibility() Extension Method

        public static Visibility ToVisibility(this bool value)
        {
            return value ? Visibility.Visible : Visibility.Collapsed;
        }

        # endregion

        # region Visibility.ToBoolean() Extension Method

        public static bool ToBoolean(this Visibility value)
        {
            return value == Visibility.Visible;
        }

        # endregion

        # region GetDescendants

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

        # region Subtract

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

        # region GetContentProperty

        public static PropertyInfo GetContentProperty(this FrameworkElement container)
        {
            var type = container.GetType();

            var contentProperty = type.GetCustomAttributes(
                typeof(ContentPropertyAttribute), true).FirstOrDefault() as ContentPropertyAttribute;

            if (contentProperty.IsNull())
            {
                throw new MissingMemberException(
                    "Unable to find content property object.");
            }
            else
            {
                var property = type.GetProperty(contentProperty.Name);
                return property;
            }
        }

        # endregion

        # region BringToFront

        public static void TryBringToFront(this UIElement element)
        {
            try
            {
                element.BringToFront();
            }
            catch
            {
            }
        }
        public static void BringToFront(this UIElement element)
        {
            var parent = VisualTreeHelper.GetParent(element);

            if (parent == null)
            {
                throw new InvalidOperationException(
                    "Unable to found a parent of this element.");
            }
            if (!(parent is Panel))
            {
                throw new InvalidOperationException(
                    "The parent of this element is not of the \"Panel\" type.");
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
