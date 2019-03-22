using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Spatial;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Web;

namespace NYCJobsWeb
{
    public class DataSearch
    {
        private static SearchServiceClient _searchClient;
        private static ISearchIndexClient _indexClient;
        private static string IndexName = ConfigurationManager.AppSettings["SearchIndexName"];
        private static string SearchSuggestor = ConfigurationManager.AppSettings["SearchSuggestor"];

        public static string errorMessage;

        static DataSearch()
        {
            try
            {
                string searchServiceName = ConfigurationManager.AppSettings["SearchServiceName"];
                string apiKey = ConfigurationManager.AppSettings["SearchServiceApiKey"];

                // Create an HTTP reference to the catalog index
                _searchClient = new SearchServiceClient(searchServiceName, new SearchCredentials(apiKey));
                _indexClient = _searchClient.Indexes.GetClient(IndexName);

            }
            catch (Exception e)
            {
                errorMessage = e.Message.ToString();
            }
        }

        public DocumentSearchResult Search(string searchText, string typeFacet, string sourceFacet, string peopleFacet,
            string organizationsFacet, string locationsFacet, string keyphrasesFacet, string languageFacet,
            string sortType, int currentPage)
        {
            // Execute search based on query string
            try
            {
                SearchParameters sp = new SearchParameters()
                {
                    SearchMode = SearchMode.Any,
                    Top = 10,
                   // Skip = currentPage - 1,
                    Skip = 0,
                    // Limit results
                    Select = new List<String>() {"id", "title", "description", "type", "source", 
                        "url", "people", "organizations", "locations", "keyphrases",
                        "language"},
                    // Add count
                    IncludeTotalResultCount = true,
                    // Add search highlights
                    HighlightFields = new List<String>() { "description" },
                    HighlightPreTag = "<b>",
                    HighlightPostTag = "</b>",
                    // Add facets
                    Facets = new List<String>() { "type", "source","people", "organizations", "locations", "keyphrases", "language" },
                };
                // Define the sort type
                if (sortType == "type")
                    sp.OrderBy = new List<String>() { "type desc" };
                else if (sortType == "source")
                    sp.OrderBy = new List<String>() { "source desc" };


                // Add filtering
                string filter = null;
                if (typeFacet != "")
                    filter = "type eq '" + typeFacet + "'";
                if (sourceFacet != "")
                {
                    if (filter != null)
                        filter += " and ";
                    filter += "source eq '" + sourceFacet + "'";

                }
                if (peopleFacet != "")
                {
                    if (filter != null)
                        filter += " and ";
                    filter += "people eq '" + peopleFacet + "'";

                }
                if (organizationsFacet != "")
                {
                    if (filter != null)
                        filter += " and ";
                    filter += "organizations eq '" + organizationsFacet + "'";

                }

                if (locationsFacet != "")
                {
                    if (filter != null)
                        filter += " and ";
                    filter += "locations eq '" + locationsFacet + "'";

                }

                if (keyphrasesFacet != "")
                {
                    if (filter != null)
                        filter += " and ";
                    filter += "keyphrases eq '" + keyphrasesFacet + "'";

                }

                if (languageFacet != "")
                {
                    if (filter != null)
                        filter += " and ";
                    filter += "language eq '" + languageFacet + "'";

                }




                sp.Filter = filter;
                
                DocumentSearchResult  results = _indexClient.Documents.Search(searchText, sp);
                return results;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error querying index: {0}\r\n", ex.Message.ToString());
            }
            return null;
        }


        public DocumentSuggestResult Suggest(string searchText, bool fuzzy)
        {
            string suggestorName = ConfigurationManager.AppSettings["SearchSuggestor"];
            // Execute search based on query string
            try
            {
                SuggestParameters sp = new SuggestParameters()
                {
                    UseFuzzyMatching = fuzzy,
                    Top = 8
                };

                return _indexClient.Documents.Suggest(searchText, suggestorName, sp);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error querying index: {0}\r\n", ex.Message.ToString());
            }
            return null;
        }

        public Document LookUp(string id)
        {
            // Execute geo search based on query string
            try
            {
                return _indexClient.Documents.Get(id);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error querying index: {0}\r\n", ex.Message.ToString());
            }
            return null;
        }

    }
}