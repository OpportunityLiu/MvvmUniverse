using System;
using System.Collections.Generic;
using System.Linq;
using static Windows.UI.Xaml.Media.VisualTreeHelper;

namespace Windows.UI.Xaml.Media
{
    public static class VisualTreeHelperExtension
    {
        #region Children

        public static IEnumerable<FrameworkElement> Children(this DependencyObject reference, string childName)
        {
            return Children<FrameworkElement>(reference, childName);
        }

        public static IEnumerable<T> Children<T>(this DependencyObject reference, string childName)
            where T : FrameworkElement
        {
            return Children<T>(reference).Where(item => item.Name == childName);
        }

        public static IEnumerable<T> Children<T>(this DependencyObject reference)
            where T : DependencyObject
        {
            return Children(reference).OfType<T>();
        }

        public static IEnumerable<DependencyObject> Children(this DependencyObject reference)
        {
            var childrenCount = GetChildrenCount(reference);
            for(var i = 0; i < childrenCount; i++)
                yield return GetChild(reference, i);
        }

        public static DependencyObject ChildAt(this DependencyObject reference, int childIndex)
        {
            return GetChild(reference, childIndex);
        }

        public static int ChildrenCount(this DependencyObject reference)
        {
            return GetChildrenCount(reference);
        }

        #endregion Children

        #region Descendants

        public static IEnumerable<FrameworkElement> Descendants(this DependencyObject reference, string descendantName)
        {
            return Descendants<FrameworkElement>(reference, descendantName);
        }

        public static IEnumerable<T> Descendants<T>(this DependencyObject reference, string descendantName)
            where T : FrameworkElement
        {
            return Descendants<T>(reference).Where(item => item.Name == descendantName);
        }

        public static IEnumerable<T> Descendants<T>(this DependencyObject reference)
            where T : DependencyObject
        {
            return Descendants(reference).OfType<T>();
        }

        public static IEnumerable<DependencyObject> Descendants(this DependencyObject reference)
        {
            return reference.DescendantsAndSelf().Skip(1);
        }

        #endregion Descendants

        #region Ancestors

        public static IEnumerable<FrameworkElement> Ancestors(this DependencyObject reference, string ancestorName)
        {
            return Ancestors<FrameworkElement>(reference, ancestorName);
        }

        public static IEnumerable<T> Ancestors<T>(this DependencyObject reference, string ancestorName)
            where T : FrameworkElement
        {
            return Ancestors<T>(reference).Where(item => item.Name == ancestorName);
        }

        public static IEnumerable<T> Ancestors<T>(this DependencyObject reference)
            where T : DependencyObject
        {
            return Ancestors(reference).OfType<T>();
        }

        public static IEnumerable<DependencyObject> Ancestors(this DependencyObject reference)
        {
            return reference.AncestorsAndSelf().Skip(1);
        }

        #endregion Ancestors

        #region Parent

        public static T Parent<T>(this DependencyObject reference)
            where T : DependencyObject
        {
            return GetParent(reference) as T;
        }

        public static DependencyObject Parent(this DependencyObject reference)
        {
            return GetParent(reference);
        }

        #endregion Parent

        #region ChildrenAndSelf

        public static IEnumerable<FrameworkElement> ChildrenAndSelf(this DependencyObject reference, string childName)
        {
            return ChildrenAndSelf<FrameworkElement>(reference, childName);
        }

        public static IEnumerable<T> ChildrenAndSelf<T>(this DependencyObject reference, string childName)
            where T : FrameworkElement
        {
            return ChildrenAndSelf<T>(reference).Where(item => item.Name == childName);
        }

        public static IEnumerable<T> ChildrenAndSelf<T>(this DependencyObject reference)
            where T : DependencyObject
        {
            return ChildrenAndSelf(reference).OfType<T>();
        }

        public static IEnumerable<DependencyObject> ChildrenAndSelf(this DependencyObject reference)
        {
            var childrenCount = GetChildrenCount(reference);
            yield return reference;
            for(var i = 0; i < childrenCount; i++)
                yield return GetChild(reference, i);
        }

        #endregion ChildrenAndSelf

        #region DescendantsAndSelf

        public static IEnumerable<FrameworkElement> DescendantsAndSelf(this DependencyObject reference, string descendantName)
        {
            return DescendantsAndSelf<FrameworkElement>(reference, descendantName);
        }

        public static IEnumerable<T> DescendantsAndSelf<T>(this DependencyObject reference, string descendantName)
            where T : FrameworkElement
        {
            return DescendantsAndSelf<T>(reference).Where(item => item.Name == descendantName);
        }

        public static IEnumerable<T> DescendantsAndSelf<T>(this DependencyObject reference)
            where T : DependencyObject
        {
            return DescendantsAndSelf(reference).OfType<T>();
        }

        public static IEnumerable<DependencyObject> DescendantsAndSelf(this DependencyObject reference)
        {
            if(reference == null)
                throw new ArgumentNullException(nameof(reference));
            var searchQueue = new Queue<DependencyObject>(10);
            searchQueue.Enqueue(reference);
            while(searchQueue.Count != 0)
            {
                var currentSearching = searchQueue.Dequeue();
                yield return currentSearching;
                foreach(var item in currentSearching.Children())
                {
                    searchQueue.Enqueue(item);
                }
            }
        }

        #endregion DescendantsAndSelf

        #region AncestorsAndSelf

        public static IEnumerable<FrameworkElement> AncestorsAndSelf(this DependencyObject reference, string ancestorName)
        {
            return AncestorsAndSelf<FrameworkElement>(reference, ancestorName);
        }

        public static IEnumerable<T> AncestorsAndSelf<T>(this DependencyObject reference, string ancestorName)
            where T : FrameworkElement
        {
            return AncestorsAndSelf<T>(reference).Where(item => item.Name == ancestorName);
        }

        public static IEnumerable<T> AncestorsAndSelf<T>(this DependencyObject reference)
            where T : DependencyObject
        {
            return AncestorsAndSelf(reference).OfType<T>();
        }

        public static IEnumerable<DependencyObject> AncestorsAndSelf(this DependencyObject reference)
        {
            if(reference == null)
                throw new ArgumentNullException(nameof(reference));
            do
            {
                yield return reference;
            } while((reference = GetParent(reference)) != null);
        }

        #endregion AncestorsAndSelf
    }
}
