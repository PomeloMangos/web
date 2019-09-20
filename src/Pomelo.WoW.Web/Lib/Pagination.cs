using System.Text;
using Pomelo.WoW.Web.Models;
using Microsoft.AspNetCore.Html;

namespace Microsoft.AspNetCore.Mvc.Rendering
{
    public static class PaginationExtension
    {
        public static HtmlString Pagination(this IHtmlHelper html)
        {
            int pageCount = html.ViewBag.PageCount;
            int current = html.ViewBag.Current;
            if (pageCount < 1) pageCount = 1;
            var start = pageCount - 5;
            if (start < 1)
                start = 1;
            var end = (start + 9) > pageCount ? pageCount : (start + 9);
            if (end - start + 1 < 10)
                start -= 4;
            if (start < 1)
                start = 1;
            if (end < start) end = start;
            var sb = new StringBuilder();
            sb.Append((html.ActionLink("«", html.ViewContext.RouteData.Values["action"].ToString(), new { p = 1 }, new { @class = "pagination-item" }) as TagBuilder).ToHtmlString());
            for (var i = start; i <= end; ++i)
            {
                sb.Append((html.ActionLink(i.ToString(), html.ViewContext.RouteData.Values["action"].ToString(), new { p = i }, new { @class = i == current ? "pagination-item active" : "pagination-item" }) as TagBuilder).ToHtmlString());
            }
            sb.Append((html.ActionLink("»", html.ViewContext.RouteData.Values["action"].ToString(), new { p = pageCount }, new { @class = "pagination-item" }) as TagBuilder).ToHtmlString());

            return new HtmlString(sb.ToString());
        }
    }
}