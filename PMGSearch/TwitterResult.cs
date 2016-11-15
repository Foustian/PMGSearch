using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sentiment.HelperClasses;

namespace PMGSearch
{
    [Serializable]
    public class TwitterResult
    {
        public Int64 iqseqid
        {
            get { return _iqseqid; }
            set { _iqseqid = value; }
        }Int64 _iqseqid;

        public Int64 tweet_id
        {
            get { return _tweet_id; }
            set { _tweet_id = value; }
        }Int64 _tweet_id;

        public String actor_displayName
        {
            get { return _actor_displayName; }
            set { _actor_displayName = value; }
        }String _actor_displayName;

        public String actor_prefferedUserName
        {
            get { return _actor_prefferedUserName; }
            set { _actor_prefferedUserName = value; }
        }String _actor_prefferedUserName;

        public String tweet_body
        {
            get { return _tweet_body; }
            set { _tweet_body = value; }
        }String _tweet_body;

        public String actor_image
        {
            get { return _actor_image; }
            set { _actor_image = value; }
        }String _actor_image;

        public String actor_link
        {
            get { return _actor_link; }
            set { _actor_link = value; }
        }String _actor_link;

        public Int64 followers_count
        {
            get { return _followers_count; }
            set { _followers_count = value; }
        }Int64 _followers_count;

        public Int64 friends_count
        {
            get { return _friends_count; }
            set { _friends_count = value; }
        }Int64 _friends_count;

        public Int64 Klout_score
        {
            get { return _Klout_score; }
            set { _Klout_score = value; }
        }Int64 _Klout_score;


        public String tweet_postedDateTime
        {
            get { return _tweet_postedDateTime; }
            set { _tweet_postedDateTime = value; }
        }String _tweet_postedDateTime;


        public Sentiments Sentiments
        {
            get { return _sentiments; }
            set { _sentiments = value; }
        }Sentiments _sentiments = new Sentiments();

        public string Highlight { get; set; }

    }
}
