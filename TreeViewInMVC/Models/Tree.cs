﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.WebPages;

namespace TreeViewInMVC.Models
{
    public static class TreeViewHelper
    {
        /// <summary>
        /// Create an HTML tree from a recursive collection of items
        /// </summary>
        public static TreeView<T> TreeView<T>(this HtmlHelper html, IEnumerable<T> items)
        {
            return new TreeView<T>(html, items);
        }
    }

    /// <summary>
    /// Create an HTML tree from a resursive collection of items
    /// </summary>
    public class TreeView<T> : IHtmlString
    {
        private readonly HtmlHelper _html;
        private readonly IEnumerable<T> _items = Enumerable.Empty<T>();
        private Func<T, string> _displayProperty = item => item.ToString();
        private Func<T, IEnumerable<T>> _childrenProperty;
        private string _emptyContent = "No children";
        private IDictionary<string, object> _htmlAttributes = new Dictionary<string, object>();
        private IDictionary<string, object> _childHtmlAttributes = new Dictionary<string, object>();
        private Func<T, HelperResult> _itemTemplate;

        public TreeView(HtmlHelper html, IEnumerable<T> items)
        {
            if (html == null) throw new ArgumentNullException("html");
            _html = html;
            _items = items;
            // The ItemTemplate will default to rendering the DisplayProperty
            _itemTemplate = item => new HelperResult(writer => writer.Write(_displayProperty(item)));
        }

        /// <summary>
        /// The property which will display the text rendered for each item
        /// </summary>
        public TreeView<T> ItemText(Func<T, string> selector)
        {
            if (selector == null) throw new ArgumentNullException("selector");
            _displayProperty = selector;
            return this;
        }


        /// <summary>
        /// The template used to render each item in the tree view
        /// </summary>
        public TreeView<T> ItemTemplate(Func<T, HelperResult> itemTemplate)
        {
            if (itemTemplate == null) throw new ArgumentNullException("itemTemplate");
            _itemTemplate = itemTemplate;
            return this;
        }


        /// <summary>
        /// The property which returns the children items
        /// </summary>
        public TreeView<T> Children(Func<T, IEnumerable<T>> selector)
        {
            if (selector == null) throw new ArgumentNullException("selector");
            _childrenProperty = selector;
            return this;
        }

        /// <summary>
        /// Content displayed if the list is empty
        /// </summary>
        public TreeView<T> EmptyContent(string emptyContent)
        {
            if (emptyContent == null) throw new ArgumentNullException("emptyContent");
            _emptyContent = emptyContent;
            return this;
        }

        /// <summary>
        /// HTML attributes appended to the root ul node
        /// </summary>
        public TreeView<T> HtmlAttributes(object htmlAttributes)
        {
            HtmlAttributes(HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
            return this;
        }

        /// <summary>
        /// HTML attributes appended to the root ul node
        /// </summary>
        public TreeView<T> HtmlAttributes(IDictionary<string, object> htmlAttributes)
        {
            if (htmlAttributes == null) throw new ArgumentNullException("htmlAttributes");
            _htmlAttributes = htmlAttributes;
            return this;
        }

        /// <summary>
        /// HTML attributes appended to the children items
        /// </summary>
        public TreeView<T> ChildrenHtmlAttributes(object htmlAttributes)
        {
            ChildrenHtmlAttributes(HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
            return this;
        }

        /// <summary>
        /// HTML attributes appended to the children items
        /// </summary>
        public TreeView<T> ChildrenHtmlAttributes(IDictionary<string, object> htmlAttributes)
        {
            if (htmlAttributes == null) throw new ArgumentNullException("htmlAttributes");
            _childHtmlAttributes = htmlAttributes;
            return this;
        }

        public string ToHtmlString()
        {
            return ToString();
        }

        public void Render()
        {
            var writer = _html.ViewContext.Writer;
            using (var textWriter = new HtmlTextWriter(writer))
            {
                textWriter.Write(ToString());
            }
        }

        private void ValidateSettings()
        {
            if (_childrenProperty == null)
            {
                throw new InvalidOperationException("You must call the Children() method to tell the tree view how to find child items");
            }
        }


        public override string ToString()
        {
            ValidateSettings();

            var listItems = _items.ToList();

            var div = new TagBuilder("div");
            div.MergeAttribute("id", "jstree");

            var ul = new TagBuilder("ul");
            ul.MergeAttributes(_htmlAttributes);
            var li = new TagBuilder("li")
            {
                InnerHtml = _emptyContent
            };
            li.MergeAttribute("id", "-1");

            if (listItems.Count > 0)
            {
                var innerUl = new TagBuilder("ul");
                innerUl.MergeAttributes(_childHtmlAttributes);

                foreach (var item in listItems)
                {
                    BuildNestedTag(innerUl, item, _childrenProperty);
                }
                li.InnerHtml += innerUl.ToString();
            }
            ul.InnerHtml += li.ToString();

            div.InnerHtml += ul.ToString();

            var outerDiv = new TagBuilder("div");
            outerDiv.InnerHtml += div.ToString();

            var hiddenTreeData = new TagBuilder("input");
            hiddenTreeData.MergeAttribute("id", "treeHidden");
            hiddenTreeData.MergeAttribute("name", "treeHidden");
            hiddenTreeData.MergeAttribute("type", "hidden");
            hiddenTreeData.MergeAttribute("value", "");

            outerDiv.InnerHtml += hiddenTreeData.ToString();

            var hiddenTreeIdsSelecteds = new TagBuilder("input");
            hiddenTreeIdsSelecteds.MergeAttribute("id", "treeHiddenSelected");
            hiddenTreeIdsSelecteds.MergeAttribute("name", "treeHiddenSelected");
            hiddenTreeIdsSelecteds.MergeAttribute("type", "hidden");
            hiddenTreeIdsSelecteds.MergeAttribute("value", "");

            outerDiv.InnerHtml += hiddenTreeIdsSelecteds.ToString();

            return outerDiv.ToString();
        }

        private void AppendChildren(TagBuilder parentTag, T parentItem, Func<T, IEnumerable<T>> childrenProperty)
        {

            var enumerableList = childrenProperty(parentItem);

            if (enumerableList == null)
            {
                return;
            }

            var children = enumerableList.ToList();
            if (!children.Any())
            {
                return;
            }

            var innerUl = new TagBuilder("ul");
            innerUl.MergeAttributes(_childHtmlAttributes);

            foreach (var item in children)
            {
                BuildNestedTag(innerUl, item, childrenProperty);
            }

            parentTag.InnerHtml += innerUl.ToString();
        }

        private void BuildNestedTag(TagBuilder parentTag, T parentItem, Func<T, IEnumerable<T>> childrenProperty)
        {
            var li = GetLi(parentItem);
            parentTag.InnerHtml += li.ToString(TagRenderMode.StartTag);
            AppendChildren(li, parentItem, childrenProperty);
            parentTag.InnerHtml += li.InnerHtml + li.ToString(TagRenderMode.EndTag);
        }

        private TagBuilder GetLi(T item)
        {
            var li = new TagBuilder("li")
            {
                InnerHtml = _itemTemplate(item).ToHtmlString()
            };
            Type myType = item.GetType();
            IList<PropertyInfo> props = new List<PropertyInfo>(myType.GetProperties());
            foreach (PropertyInfo prop in props)
            {
                if (prop.Name.ToLower() == "id")
                    li.MergeAttribute("id", prop.GetValue(item, null).ToString());

                if (prop.Name.ToLower() == "title")
                    li.MergeAttribute("data-title", prop.GetValue(item, null).ToString());

                if (prop.Name.ToLower() == "description")
                    li.MergeAttribute("data-description", prop.GetValue(item, null) == null ? "" : prop.GetValue(item, null).ToString());

                if (prop.Name.ToLower() == "parentid")
                    li.MergeAttribute("data-parentId", prop.GetValue(item, null) == null ? "" : prop.GetValue(item, null).ToString());

                //li.GenerateId(prop.GetValue(item, null).ToString());
                //object propValue = prop.GetValue(myObject, null);
                // Do something with propValue
                if (prop.Name.ToLower() == "sortorder")
                    li.MergeAttribute("priority", prop.GetValue(item, null).ToString());
            }

            li.MergeAttribute("treeJsElement", "treeJsElement");

            return li;
        }
    }
}