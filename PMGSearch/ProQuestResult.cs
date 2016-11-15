using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sentiment.HelperClasses;

namespace PMGSearch
{
    [Serializable]
    public class ProQuestResult
    {
        public Int64 IQSeqID
        {
            get { return _IQSeqID; }
            set { _IQSeqID = value; }
        }Int64 _IQSeqID;

        public string MediaCategory
        {
            get { return _MediaCategory; }
            set { _MediaCategory = value; }
        }string _MediaCategory;

        public string Title
        {
            get { return _Title; }
            set { _Title = value; }
        }string _Title;

        public string Publication
        {
            get { return _Publication; }
            set { _Publication = value; }
        }string _Publication;

        public string Abstract
        {
            get { return _Abstract; }
            set { _Abstract = value; }
        }string _Abstract;

        public string AbstractHTML
        {
            get { return _AbstractHTML; }
            set { _AbstractHTML = value; }
        }string _AbstractHTML;

        public string Content
        {
            get { return _Content; }
            set { _Content = value; }
        }string _Content;

        public string ContentHTML
        {
            get { return _ContentHTML; }
            set { _ContentHTML = value; }
        }string _ContentHTML;

        public string Language
        {
            get { return _Language; }
            set { _Language = value; }
        }string _Language;

        public Int16 LanguageNum
        {
            get { return _LanguageNum; }
            set { _LanguageNum = value; }
        }Int16 _LanguageNum;

        public DateTime AvailableDate
        {
            get { return _AvailableDate; }
            set { _AvailableDate = value; }
        }DateTime _AvailableDate;

        public DateTime MediaDate
        {
            get { return _MediaDate; }
            set { _MediaDate = value; }
        }DateTime _MediaDate;

        public List<string> Authors
        {
            get { return _Authors; }
            set { _Authors = value; }
        }List<string> _Authors;

        public Sentiments Sentiments
        {
            get { return _sentiments; }
            set { _sentiments = value; }
        }Sentiments _sentiments = new Sentiments();

        public List<string> Highlights
        {
            get { return _Highlights; }
            set { _Highlights = value; }
        }List<string> _Highlights;

        public int Mentions
        {
            get { return _Mentions; }
            set { _Mentions = value; }
        } int _Mentions;

        public string Copyright { get; set; }

        public bool IsSearchTermInHeadline { get; set; }

        public bool IsLeadParagraph { get; set; }
    }
}
