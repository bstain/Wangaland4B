using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ColorCode;

namespace Wangaland4B.Postings.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public MvcHtmlString Index(int id = 0)
        {
            string path = @"D:\Works\VS2013\ProjectsForBlog\Wangaland4B\Wangaland4B.Auth\";
            ILanguage language = Languages.CSharp;
            switch (id) {
                case 1:            
                    path += @"Views\RolesAdmin\Create.cshtml";
                    language = Languages.AspxCs;
                    break;
                default:
                    path += @"App_Start\IdentityConfig.cs";
                    break;
            }
            string sourceCode = string.Empty;
            string colorizedSourcecode = string.Empty;

            if (System.IO.File.Exists(path)) {
                sourceCode = System.IO.File.ReadAllText(path);
                colorizedSourcecode = new CodeColorizer().Colorize(sourceCode, language);
            }
            return MvcHtmlString.Create(colorizedSourcecode);
        }
    }
}