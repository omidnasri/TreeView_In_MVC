using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TreeViewInMVC.Models
{
    public class TreeModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int? ParentId { get; set; }
        public bool Selected { get; set; }
        [ForeignKey("ParentId")]
        public virtual TreeModel Parent { get; set; }
        public virtual ICollection<TreeModel> Childs { get; set; }
    }
}