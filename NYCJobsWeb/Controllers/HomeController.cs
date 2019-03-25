using BingGeocoder;
using NYCJobsWeb.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NYCJobsWeb.Controllers
{
    public class HomeController : Controller
    {
        private DataSearch _datastoreSearch = new DataSearch();

        // GET: Home
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult JobDetails()
        {
            return View();
        }
        /// <summary>
        ///             {
            //    q: q,
            //    typeFacet: typeFacet,
            //    sourceFacet: sourceFacet,
            //    peopleFacet: peopleFacet,
            //    sortType: sortType,
            //    organizationsFacet: organizationsFacet,
            //    locationsFacet: locationsFacet,
            //    currentPage: currentPage,
            //    keyphrasesFacet: keyphrasesFacet,
            //    languageFacet: languageFacet
            //}
        /// </summary>
        /// <param name="q"></param>
        /// <param name="typeFacet"></param>
        /// <param name="sourceFacet"></param>
        /// <param name="peopleFacet"></param>
        /// <param name="organizationsFacet"></param>
        /// <param name="locationsFacet"></param>
        /// <param name="keyphrasesFacet"></param>
        /// <param name="languageFacet"></param>
        /// <param name="sortType"></param>
        /// <param name="currentPage"></param>
        /// <returns></returns>
        public ActionResult Search(
            string q = "", 
            string typeFacet = "", 
            string sourceFacet = "", 
            string peopleFacet = "",
            string sortType = "",
            string organizationsFacet = "", 
            string locationsFacet = "",
            int currentPage = 0,
            string keyphrasesFacet = "", 
            string languageFacet = ""
            )
        {
            // If blank search, assume they want to search everything
            if (string.IsNullOrWhiteSpace(q))
                q = "*";

            /*
            (string searchText, string typeFacet, string sourceFacet, string peopleFacet,
             string organizationsFacet, string locationsFacet, string keyphrasesFacet, string languageFacet,
             string sortType, int currentPage)
            */
            var response = _datastoreSearch.Search(
                q, typeFacet, sourceFacet, peopleFacet, organizationsFacet, locationsFacet, keyphrasesFacet,
                languageFacet, sortType, currentPage);
            return new JsonResult
            {
                // ***************************************************************************************************************************
                // If you get an error here, make sure to check that you updated the SearchServiceName and SearchServiceApiKey in Web.config
                // ***************************************************************************************************************************

                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new DataItem() { Results = response.Results, Facets = response.Facets, Count = Convert.ToInt32(response.Count) }
            };
        }

        [HttpGet]
        public ActionResult Suggest(string term, bool fuzzy = true)
        {
            // Call suggest query and return results
            var response = _datastoreSearch.Suggest(term, fuzzy);
            List<string> suggestions = new List<string>();
            foreach (var result in response.Results)
            {
                suggestions.Add(result.Text);
            }

            // Get unique items
            List<string> uniqueItems = suggestions.Distinct().ToList();

            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = uniqueItems
            };

        }

        public ActionResult LookUp(string id)
        {
            // Take a key ID and do a lookup to get the job details
            if (id != null)
            {
                var response = _datastoreSearch.LookUp(id);
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new NYCJobLookup() { Result = response }
                };
            }
            else
            {
                return null;
            }

        }

    }
}
