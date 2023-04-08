using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TreeViewInMVC.Models;
using Newtonsoft.Json;

namespace TreeViewInMVC.Binders
{
    public class TreeModelBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            HttpRequestBase request = controllerContext.HttpContext.Request;

            var listTreeHidden = JsonConvert.DeserializeObject<List<TreeModel>>(request.Form.Get("treeHidden"));

            if (listTreeHidden.Count > 0)
            {
                var selectedIds = JsonConvert.DeserializeObject<List<int>>(request.Form.Get("treeHiddenSelected"));

                if (selectedIds.Count > 0)
                {
                    for (int i = 0; i < listTreeHidden.Count; i++)
                    {
                        if (selectedIds.Contains(listTreeHidden[i].Id))
                            listTreeHidden[i].Selected = true;
                    }

                }

                if (listTreeHidden.Count > 1)
                {
                    var listWithoutParentId = listTreeHidden.Where(x => x.ParentId == null).ToList();

                    var newlist = new List<TreeModel>();

                    for (int i = 0; i < listWithoutParentId.Count; i++)
                    {
                        newlist.Add(OrganizeChildrenItems(listWithoutParentId[i], ref listTreeHidden));
                    }

                    listTreeHidden = newlist;

                }
            }

            return listTreeHidden;
        }

        TreeModel OrganizeChildrenItems(TreeModel item, ref List<TreeModel> list)
        {
            if (!list.Any(x => x.ParentId == item.Id))
                return item;

            if (item.Childs == null)
                item.Childs = new List<TreeModel>();

            foreach (TreeModel child in list.Where(x => x.ParentId == item.Id))
            {
                item.Childs.Add(OrganizeChildrenItems(child, ref list));
            }

            return item;
        }
    }
}