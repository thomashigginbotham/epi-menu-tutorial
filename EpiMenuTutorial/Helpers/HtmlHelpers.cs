using EPiServer;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using EPiServer.Web.Mvc.Html;
using EPiServer.Web.Routing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages;

namespace EpiMenuTutorial.Helpers
{
    public static class HtmlHelpers
    {
        /// <summary>
        /// An item for use in a MenuList.
        /// </summary>
        public class MenuItem
        {
            public MenuItem(PageData page)
            {
                Page = page;
            }

            public PageData Page { get; set; }
            public bool Selected { get; set; }
            public bool AncestorOfSelected { get; set; }
            public Lazy<bool> HasChildren { get; set; }
        }

        /// <summary>
        /// Returns HTML for each child page for use in a menu.
        /// </summary>
        public static IHtmlString MenuList(
            this HtmlHelper helper,
            ContentReference rootLink,
            Func<MenuItem, HelperResult> itemTemplate = null,
            bool includeRoot = false,
            bool requireVisibleInMenu = true,
            bool requirePageTemplate = true)
        {
            itemTemplate = itemTemplate ?? new Func<MenuItem, HelperResult>(
                x => new HelperResult(
                    textWriter => textWriter.Write(helper.PageLink(x.Page))));

            var currentContentLink = helper.ViewContext.RequestContext
                .GetContentLink();
            var contentLoader = ServiceLocator.Current.GetInstance<IContentLoader>();
            var ancestors = contentLoader.GetAncestors(currentContentLink)
                .Reverse()
                .Select(x => x.ContentLink)
                .SkipWhile(x => !x.CompareToIgnoreWorkID(rootLink))
                .ToList();
            var menuItems = contentLoader.GetChildren<PageData>(rootLink)
                .Select(x => CreateMenuItem(
                    x,
                    currentContentLink,
                    ancestors,
                    contentLoader))
                .ToList();

            if (includeRoot)
            {
                menuItems.Insert(
                    0,
                    CreateMenuItem(
                        contentLoader.Get<PageData>(rootLink),
                        currentContentLink,
                        ancestors,
                        contentLoader));
            }

            var buffer = new StringBuilder();
            var writer = new StringWriter(buffer);

            menuItems.ForEach(menuItem => itemTemplate(menuItem).WriteTo(writer));

            return new MvcHtmlString(buffer.ToString());
        }

        /// <summary>
        /// Creates a new MenuItem from provided PageData.
        /// </summary>
        /// <returns></returns>
        public static MenuItem CreateMenuItem(
            PageData page,
            ContentReference currentContentLink,
            List<ContentReference> ancestors,
            IContentLoader contentLoader)
        {
            bool selected = page.ContentLink.CompareToIgnoreWorkID(currentContentLink);
            bool ancestorOfSelected = !selected && ancestors.Contains(page.ContentLink);

            var menuItem = new MenuItem(page)
            {
                Selected = selected,
                AncestorOfSelected = ancestorOfSelected,
                HasChildren = new Lazy<bool>(() =>
                    contentLoader.GetChildren<PageData>(page.ContentLink).Any())
            };

            return menuItem;
        }
    }
}