using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Wangaland4B.Auth.Models
{
    public class RoleViewModel
    {
        public int Id { get; set; }
        [Required(AllowEmptyStrings = false)]
        [Display(Name = "역할 이름")]
        public string Name { get; set; }
        [Display(Name = "역할 설명")]
        public string Description { get; set; }
    }

    public class EditUserViewModel
    {
        public int Id { get; set; }

        [Required(AllowEmptyStrings = false)]
        [Display(Name = "전자 메일")]
        [EmailAddress]
        public string Email { get; set; }

        [Display(Name = "도/시")]
        public string State { get; set; }
        [Display(Name = "시/구/군")]
        public string City { get; set; }
        [Display(Name = "상세 주소")]
        public string Address { get; set; }

        // Use a sensible display name for views:
        [Display(Name = "우편 번호")]
        public string PostalCode { get; set; }

        public IEnumerable<SelectListItem> RolesList { get; set; }
    }
}