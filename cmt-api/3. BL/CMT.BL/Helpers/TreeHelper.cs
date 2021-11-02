using CMT.BO.Admin;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CMT.BL.Helpers
{
    public static class TreeHelper
    {
        public static TreeElementBO CreateTree(TreeElementBO bo, List<TreeElementBO> objects, string searchTerm)
        {
            bo.Children = new List<TreeElementBO>();
            Guid objId = bo.ObjectId;
            foreach (TreeElementBO c in objects.Where(p => p.ParentId == objId))
            {
                CreateTree(c, objects, searchTerm);
                bo.Children.Add(c);
            }

            bo.Children = bo.Children.OrderBy(p => p.Name).ToList();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                bo.SearchTermFound = bo.Children.Any(p => p.SearchTermFound) || (bo.Name.ToLowerInvariant().Contains(searchTerm) && bo.Children.Any());
            }
            else
            {
                bo.SearchTermFound = true;
            }

            return bo;
        }
    }
}
