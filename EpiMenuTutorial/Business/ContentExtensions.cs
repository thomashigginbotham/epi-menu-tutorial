using EPiServer.Core;
using EPiServer.Filters;
using EPiServer.Framework.Web;
using EPiServer.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EpiMenuTutorial.Business
{
    public static class ContentExtensions
    {
        /// <summary>
        /// Filters our content that should be be visible to the user.
        /// </summary>
        public static IEnumerable<T> FilterForDisplay<T>(
            this IEnumerable<T> contents,
            bool requirePageTemplate = false,
            bool requireVisibleInMenu = false) where T : IContent
        {
            var accessFilter = new FilterAccess();
            var publishedFilter = new FilterPublished();

            contents = contents.Where(x =>
                !publishedFilter.ShouldFilter(x) &&
                !accessFilter.ShouldFilter(x));

            if (requirePageTemplate)
            {
                var templateFilter = ServiceLocator.Current
                    .GetInstance<FilterTemplate>();

                templateFilter.TemplateTypeCategories = TemplateTypeCategories.Page;
                contents = contents.Where(x => !templateFilter.ShouldFilter(x));
            }

            if (requireVisibleInMenu)
            {
                contents = contents.Where(x => VisibleInMenu(x));
            }

            return contents;
        }

        /// <summary>
        /// Returns wheter a page is set to be visible in menus.
        /// </summary>
        private static bool VisibleInMenu(IContent content)
        {
            if (content is PageData page)
            {
                return page.VisibleInMenu;
            }

            return true;
        }
    }
}