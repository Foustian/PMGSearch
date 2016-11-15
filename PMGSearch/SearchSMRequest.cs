using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Xml;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Configuration;
using System.Xml.Linq;

namespace PMGSearch
{

    [Serializable]
    public class SearchSMRequest : IEquatable<SearchSMRequest>
    {

        public object SearchSMRequest_CloneObject()
        {
            return this.MemberwiseClone();
        }

        public int PageSize
        {
            get { return _pageSize; }
            set { _pageSize = value; }
        } int _pageSize = 25;

        public int PageNumber
        {
            get { return _pageNum; }
            set { _pageNum = value; }
        } int _pageNum = 0;

        public DateTime? StartDate
        {
            get { return _startDate; }
            set { _startDate = value; }
        } DateTime? _startDate;

        public DateTime? EndDate
        {
            get { return _endDate; }
            set { _endDate = value; }
        } DateTime? _endDate;

        public String SearchTerm
        {
            get { return _SearchTerm; }
            set { _SearchTerm = value; }

        }String _SearchTerm = null;

        public List<String> SocialMediaSources
        {
            get { return _SocialMediaSources; }
            set { _SocialMediaSources = value; }
        } List<String> _SocialMediaSources;

        public String Author
        {
            get { return _Author; }
            set { _Author = value; }
        } String _Author;

        public String Title
        {
            get { return _Title; }
            set { _Title = value; }
        } String _Title;

        public List<String> ExcludeDomains
        {
            get { return _ExcludeDomains; }
            set { _ExcludeDomains = value; }
        } List<String> _ExcludeDomains;

        public List<string> SourceType
        {
            get { return _SourceType; }
            set { _SourceType = value; }
        } List<string> _SourceType;

        public List<String> SourceRank
        {
            get { return _SourceRank; }
            set { _SourceRank = value; }
        } List<String> _SourceRank;

        public Boolean IsPmgLogging
        {
            get { return _isPMGLogging; }
            set { _isPMGLogging = value; }
        }bool _isPMGLogging;

        public String PmgLogFileLocation
        {
            get { return _pmgLogFileLocation; }
            set { _pmgLogFileLocation = value; }
        }string _pmgLogFileLocation;

        public String SortFields
        {
            get { return _sortFields; }
            set { _sortFields = value; }
        } string _sortFields;

        public Boolean Facet
        {
            get { return _Facet; }
            set { _Facet = value; }
        }Boolean _Facet;

        public String FacetRangeOther
        {
            get { return _FacetRangeOther; }
            set { _FacetRangeOther = value; }
        }String _FacetRangeOther;

        public DateTime? FacetRangeStarts
        {
            get { return _FacetRangeStarts; }
            set { _FacetRangeStarts = value; }
        }DateTime? _FacetRangeStarts;

        public DateTime? FacetRangeEnds
        {
            get { return _FacetRangeEnds; }
            set { _FacetRangeEnds = value; }
        }DateTime? _FacetRangeEnds;

        public RangeGap FacetRangeGap
        {
            get { return _FacetRangeGap; }
            set { _FacetRangeGap = value; }
        }RangeGap _FacetRangeGap;

        public int FacetRangeGapDuration
        {
            get { return _FacetRangeGapDuration; }
            set { _FacetRangeGapDuration = value; }
        }int _FacetRangeGapDuration;

        public String FacetRange
        {
            get;
            set;
        }String _FacetRange;

        public ReponseType wt
        {
            get { return _wt; }
            set { _wt = value; }
        }ReponseType _wt;

        public List<String> ids
        {
            get { return _ids; }
            set { _ids = value; }
        } List<String> _ids;

        public Boolean isShowContent
        {
            get { return _isShowContent; }
            set { _isShowContent = value; }
        }Boolean _isShowContent = false;

        public Boolean IsOutRequest
        {
            get { return _IsOutRequest; }
            set { _IsOutRequest = value; }
        } Boolean _IsOutRequest;

        public Boolean IsSentiment
        {
            get { return _isSentiment; }
            set { _isSentiment = value; }
        }Boolean _isSentiment = false;

        public Guid ClientGuid
        {
            get { return _clientGuid; }
            set { _clientGuid = value; }
        }Guid _clientGuid;

        public float? LowThreshold
        {
            get { return _lowThreshold; }
            set { _lowThreshold = value; }
        }float? _lowThreshold = null;

        public float? HighThreshold
        {
            get { return _highThreshold; }
            set { _highThreshold = value; }
        }float? _highThreshold = null;

        public int? FragSize
        {
            get { return _fragSize; }
            set { _fragSize = value; }
        }int? _fragSize = null;

        /*public List<String> lstfacetRange
        {

            get { return _lstfacetRange; }
            set { _lstfacetRange = value; }
        } List<String> _lstfacetRange;*/

        public Boolean IsReturnHighlight
        {
            get { return _isReturnHighlight; }
            set { _isReturnHighlight = value; }
        }Boolean _isReturnHighlight = false;


        public String FacetField
        {
            get;
            set;
        }String _FacetField;

        public Boolean IsTaggingExcluded
        {
            get { return _IsTaggingExcluded; }
            set { _IsTaggingExcluded = value; }
        }Boolean _IsTaggingExcluded = false;

        public List<string> ExcludedSourceType
        {
            get { return _ExcludedSourceType; }
            set { _ExcludedSourceType = value; }
        } List<string> _ExcludedSourceType;

        public Int64? Start
        {
            get { return _Start; }
            set { _Start = value; }
        }Int64? _Start = null;

        public string FromRecordID
        {
            get { return _FromRecordID; }
            set { _FromRecordID = value; }
        }string _FromRecordID;

        public Boolean IsTitleNContentSearch
        {
            get { return _IsTitleNContentSearch; }
            set { _IsTitleNContentSearch = value; }
        }bool _IsTitleNContentSearch = false;

        public string RawParam
        {
            get { return _RawParam; }
            set { _RawParam = value; }
        }string _RawParam = null;

        public string MediaCategory { get; set; }

        // Used when searching Facebook to indicate the inclusion/exclusion of default pages
        public Boolean IncludeDefaultFBPages
        {
            get { return _IncludeDefaultPages; }
            set { _IncludeDefaultPages = value; }
        } Boolean _IncludeDefaultPages = true;

        // List of Facebook pages to search
        public List<string> FBPageIDs { get; set; }

        // List of Facebook pages to exclude
        public List<string> ExcludeFBPageIDs { get; set; }

        public List<string> AuthorNames { get; set; }

        public bool Equals(SearchSMRequest other)
        {
            if (this.SearchTerm != other.SearchTerm)
                return false;

            if (this.StartDate != other.StartDate)
                return false;

            if (this.EndDate != other.EndDate)
                return false;

            if (this.SocialMediaSources != null && other.SocialMediaSources != null)
            {
                if (this.SocialMediaSources.Count() != other.SocialMediaSources.Count())
                    return false;

                this.SocialMediaSources = this.SocialMediaSources.OrderBy(a => a).ToList();
                other.SocialMediaSources = other.SocialMediaSources.OrderBy(a => a).ToList();
                for (int i = 0; i < this.SocialMediaSources.Count(); i++)
                {
                    string Obj1 = this.SocialMediaSources.ElementAt(i);
                    string Obj2 = other.SocialMediaSources.ElementAt(i);
                    if (Obj1 != Obj2)
                        return false;
                }
            }
            else if ((this.SocialMediaSources != null && other.SocialMediaSources == null) || (this.SocialMediaSources == null && other.SocialMediaSources != null))
                return false;

            if (this.Title != other.Title)
                return false;

            if (this.Author != other.Author)
                return false;

            if (this.AuthorNames != null && other.AuthorNames != null)
            {
                if (this.AuthorNames.Count() != other.AuthorNames.Count())
                    return false;

                this.AuthorNames = this.AuthorNames.OrderBy(a => a).ToList();
                other.AuthorNames = other.AuthorNames.OrderBy(a => a).ToList();
                for (int i = 0; i < this.AuthorNames.Count(); i++)
                {
                    string Obj1 = this.AuthorNames.ElementAt(i);
                    string Obj2 = other.AuthorNames.ElementAt(i);
                    if (Obj1 != Obj2)
                        return false;
                }
            }
            else if ((this.AuthorNames != null && other.AuthorNames == null) || (this.AuthorNames == null && other.AuthorNames != null))
                return false;

            if (this.SourceRank != null && other.SourceRank != null)
            {
                if (this.SourceRank.Count() != other.SourceRank.Count())
                    return false;

                this.SourceRank = this.SourceRank.OrderBy(a => a).ToList();
                other.SourceRank = other.SourceRank.OrderBy(a => a).ToList();
                for (int i = 0; i < this.SourceRank.Count(); i++)
                {
                    string Obj1 = this.SourceRank.ElementAt(i);
                    string Obj2 = other.SourceRank.ElementAt(i);
                    if (Obj1 != Obj2)
                        return false;
                }
            }
            else if ((this.SourceRank != null && other.SourceRank == null) || (this.SourceRank == null && other.SourceRank != null))
                return false;


            if (this.SourceType != null && other.SourceType != null)
            {
                if (this.SourceType.Count() != other.SourceType.Count())
                    return false;

                this.SourceType = this.SourceType.OrderBy(a => a).ToList();
                other.SourceType = other.SourceType.OrderBy(a => a).ToList();
                for (int i = 0; i < this.SourceType.Count(); i++)
                {
                    string Obj1 = this.SourceType.ElementAt(i);
                    string Obj2 = other.SourceType.ElementAt(i);
                    if (Obj1 != Obj2)
                        return false;
                }
            }
            else if ((this.SourceType != null && other.SourceType == null) || this.SourceType == null && other.SourceType != null)
                return false;

            if (this.MediaCategory != other.MediaCategory)
                return false;

            if (this.IncludeDefaultFBPages != other.IncludeDefaultFBPages)
                return false;

            if (this.FBPageIDs != null && other.FBPageIDs != null)
            {
                if (this.FBPageIDs.Count() != other.FBPageIDs.Count())
                    return false;

                this.FBPageIDs = this.FBPageIDs.OrderBy(a => a).ToList();
                other.FBPageIDs = other.FBPageIDs.OrderBy(a => a).ToList();
                for (int i = 0; i < this.FBPageIDs.Count(); i++)
                {
                    string Obj1 = this.FBPageIDs.ElementAt(i);
                    string Obj2 = other.FBPageIDs.ElementAt(i);
                    if (Obj1 != Obj2)
                        return false;
                }
            }
            else if ((this.FBPageIDs != null && other.FBPageIDs == null) || (this.FBPageIDs == null && other.FBPageIDs != null))
                return false;

            if (this.ExcludeFBPageIDs != null && other.ExcludeFBPageIDs != null)
            {
                if (this.ExcludeFBPageIDs.Count() != other.ExcludeFBPageIDs.Count())
                    return false;

                this.ExcludeFBPageIDs = this.ExcludeFBPageIDs.OrderBy(a => a).ToList();
                other.ExcludeFBPageIDs = other.ExcludeFBPageIDs.OrderBy(a => a).ToList();
                for (int i = 0; i < this.ExcludeFBPageIDs.Count(); i++)
                {
                    string Obj1 = this.ExcludeFBPageIDs.ElementAt(i);
                    string Obj2 = other.ExcludeFBPageIDs.ElementAt(i);
                    if (Obj1 != Obj2)
                        return false;
                }
            }
            else if ((this.ExcludeFBPageIDs != null && other.ExcludeFBPageIDs == null) || (this.ExcludeFBPageIDs == null && other.ExcludeFBPageIDs != null))
                return false;

            return true;
        }
    }

}