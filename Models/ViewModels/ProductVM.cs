using Microsoft.AspNetCore.Mvc.Rendering;
using Rocky.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rocky.Models.ViewModels
{
    public class ProductVM
    {
        public IEnumerable<SelectListItem> CategorySelectList { get; set; }
        public IEnumerable<SelectListItem> ApplicationSelectList { get; set; }
        public Product Product { get; set; }
    }
}
