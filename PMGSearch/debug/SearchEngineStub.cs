using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace PMGSearch.debug
{
    /// <summary>
    /// A debug (stubbed) implementation of the SearchEngine class. This version does not connect to any web service,
    /// instead it simulates the call by delaying for 50-100 ms, and returns some fake search result data. 
    /// </summary>
    public class SearchEngineStub : SearchEngine
    {

        #region Configuration Settings

        /// <summary>
        /// Maximum # of seconds in each hit (clip). Used to come up with some fake offsets.
        /// </summary>
        private const int clipLengthInSeconds = 3600;   // 60-minute clip. Don't set this value lower than (maxNumberOfOcurrences*2).

        /// <summary>
        /// GUID to return for each hit
        /// </summary>
        private const string testGUID = "516878b9-72ae-4469-b78f-7537b2dfd033";    // Completely made up - For testing the UI, replace with a real GUID

        /// <summary>
        /// Minimum number of MS to wait to simulate network call (set this and maxResponseTimeMS to 0 to disable delay)
        /// </summary>
        private const int minResponseTimeMS = 50;

        /// <summary>
        /// Maximum number of MS to wait to simulate network call (set this and minResponseTimeMS to 0 to disable delay)
        /// </summary>
        private const int maxResponseTimeMS = 100;

        /// <summary>
        /// Fake text "occurring" before each search term
        /// </summary>
        private const string fakePrefixText = "lorem ipsum dolor sit amet, consectetur ";

        /// <summary>
        /// Fake text "occurring" after each search term
        /// </summary>
        private const string fakeSuffixText = " adipisicing elit, sed do eiusmod";

        /// <summary>
        /// Maximum number of occurrences of the search term to simulate within each hit.
        /// </summary>
        private const int maxNumberOfOccurrences = 4;

        /// <summary>
        /// Time zone to fake for results
        /// </summary>
        private String fakeTimeZone = "EST";

        /// <summary>
        /// Date and Time to fake for results
        /// </summary>
        private DateTime fakeDateTime = DateTime.Now;

        /// <summary>
        /// Station ID to fake for results.
        /// </summary>
        private const string fakeStationID = "WTXF";    // REPLACE THIS VALUE. This is likely not the correct format - I have not seen the station ID data yet.

        #endregion

        #region Member Variables

        Random rng;

        #endregion


        /// <summary>
        /// Performs a FAKE search to simulate a call to the RESTSearch service.
        /// </summary>
        /// <param name="req">Parameters for the search request</param>
        /// <returns>SearchResult object encapsulating results and associated metadata</returns>
        new public SearchResult search(SearchRequest req)
        {
            SearchResult res = new SearchResult();

            this.rng = new Random();

            // Fake the round trip
            System.Threading.Thread.Sleep(rng.Next(minResponseTimeMS, maxResponseTimeMS));

            res.OriginalRequest = req;

            // Quick exit sanity checks: no terms, page size 0, etc. = no results.
            if (req == null || req.PageSize <= 0 || req.Terms == null || req.Terms.Length == 0) throw new InvalidOperationException("Invalid arguments passed to SearchEngineStub");

            fillFakeSearchResult(req, res);

            return res;
        }

        private void fillFakeSearchResult(SearchRequest req, SearchResult res)
        {
            int ofs;
            List<Hit> hits = new List<Hit>();
            List<TermOccurrence> termOccurrences;

            for (int i = 0; i < req.PageSize; i++)
            {
                ofs = 0;
                termOccurrences = new List<TermOccurrence>();

                // Simulate a random number of search term occurrences in this hit.

                int numberOfOccurrences = rng.Next(1, maxNumberOfOccurrences);
                for (int j = 0; j < numberOfOccurrences; j++)
                {
                    // Distribute the occurrences pseudo-randomly but in sequential order. Note that this will not guarantee
                    // "realistic-looking" results. In other words it could feasibly return 2 hits, both in the first 2 seconds of video - 
                    // even though the fake surrounding text wouldn't look like that was possible.
                    ofs = rng.Next(ofs + 1, ((clipLengthInSeconds / numberOfOccurrences)*(j+1)));
                    termOccurrences.Add(createFakeOccurrence(req.Terms, ofs));
                }

                hits.Add(createFakeHit(req, termOccurrences));
            }

            res.Hits = hits;
            res.TotalHitCount = hits.Count;
        }

        private Hit createFakeHit(SearchRequest req, List<TermOccurrence> termOccurrences)
        {
            Hit hit = new Hit();
            hit.GUID = testGUID;
            hit.TermOccurrences = termOccurrences;
            hit.StationID = fakeStationID;
            hit.ClipTimeZone = fakeTimeZone;
            hit.TimeStamp = fakeDateTime;
            return hit;
        }

        private TermOccurrence createFakeOccurrence(string searchTerm, int timeOffset)
        {
            TermOccurrence ret = new TermOccurrence();
            ret.TimeOffset = timeOffset;
            ret.SurroundingText = fakePrefixText + searchTerm + fakeSuffixText;
            ret.SearchTerm = searchTerm;
            return ret;
        }
    }
}
