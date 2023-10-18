using Microsoft.Extensions.Primitives;
using System.Net;
using System.Web;
using TRTA.OSP.OrganizationManagement.Contracts.Dtos;

namespace TRTA.OSP.OrganizationManagement.API.Extensions
{
    /// <summary>
    /// HttpContextExtensions
    /// </summary>
    public static class HttpContextExtensions
    {

        private const string offset = "offset";
        private const string limit = "limit";

        /// <summary>
        /// returns tenent code from session 
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public static string GetTenantCode(this HttpContext httpContext)
        {
            string tenantCode = string.Empty;

            try
            {
                tenantCode = httpContext.User.Claims.Where(c => c.Type.ToLower() == "tenant").FirstOrDefault().Value;
            }
            catch
            {
                throw new UnauthorizedAccessException("Invalid tenant code");
            }

            return tenantCode;
        }

        /// <summary>
        /// Return user selected tenant code
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public static string GetSelectedTenantCode (this HttpContext httpContext)
        {
            string selectedTenantCode = string.Empty;
            try
            {
                selectedTenantCode = httpContext.User.Claims.Where(c => c.Type == "X-LoneStar-AccountId").FirstOrDefault().Value;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return selectedTenantCode;
        }


        /// <summary>
        /// Return logged in user Id
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public static string GetLoggedInUser(this HttpContext httpContext)
        {
            string universalId = string.Empty;
            try
            {
                universalId = httpContext.User.Claims.Where(c => c.Type == "UniversalId").FirstOrDefault().Value;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return universalId;
        }

        /// <summary>
        /// Return pagination disable flag
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public static bool GetPaginationDisalbed(this HttpContext httpContext)
        {
            try
            {

                if (httpContext.Request.Headers.ContainsKey("X-Concert-API-Disable-Pagination"))
                {
                    StringValues keys;
                    var udsToken = httpContext.Request.Headers.TryGetValue("X-Concert-API-Disable-Pagination", out keys);
                    return (keys.First().ToUpper() == "TRUE" || keys.First().ToUpper() == "1");
                }
            }
            catch
            {
                //Suppress any error as this flag added for investigation
            }

            return false;
        }

       /// <summary>
       /// Returns pagination links
       /// </summary>
       /// <param name="httpContext"></param>
       /// <param name="controllerPath"></param>
       /// <param name="offset"></param>
       /// <param name="limit"></param>
       /// <param name="totalRecords"></param>
       /// <returns></returns>
        public static CollectionNavigationLinks GetNavigationUrls(this HttpContext httpContext, string controllerPath, int offset, int limit, int totalRecords)
        {
            var navLinks = new CollectionNavigationLinks();

            var queryCollection = HttpUtility.ParseQueryString(httpContext.Request.QueryString.ToString());
            queryCollection.Remove(HttpContextExtensions.offset);
            queryCollection.Remove(HttpContextExtensions.limit);
            queryCollection.Add(HttpContextExtensions.offset, (limit + offset).ToString()); //To make the parameter to end of query string 
            queryCollection.Add(HttpContextExtensions.limit, limit.ToString());   //To make the parameter to end of query string

            if (offset + limit < totalRecords)
            {
                var nextLink = WebUtility.UrlDecode(queryCollection.ToString());
                navLinks.Next = $"{controllerPath}?{nextLink}";
            }

            if (offset > 0)
            {
                var previousPageOffset = offset > limit ? (offset - limit) : 0;
                var previousPageLimit = offset < limit ? offset : limit;

                queryCollection.Set(HttpContextExtensions.offset, previousPageOffset.ToString());
                queryCollection.Set(HttpContextExtensions.limit, previousPageLimit.ToString());

                var prevLink = WebUtility.UrlDecode(queryCollection.ToString());
                navLinks.Previous = $"{controllerPath}?{prevLink}";
            }

            return navLinks;
        }
    }
}

