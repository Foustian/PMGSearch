using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using PMGSearch;
using System.Xml.Linq;
using System.Xml;
using System.IO;
using System.Security.Policy;
using System.Diagnostics;
using System.Net;

namespace PMGSearchTestHarness
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //SearchEngine se = new SearchEngine("http://207.245.94.77:8080/RESTSearch/RESTSearch");
            //SearchEngine se = new SearchEngine("http://10.100.1.38:8080/RESTSearch/RESTSearch");
            //SearchEngine se = new SearchEngine(new Uri("http://192.168.1.59:8080/solr/core0/select/?wt=xslt&tr=solr.xsl&hl=on&hl.fl=CC&hl.snippets=99&"));
            //SearchRequest req = new SearchRequest();
            //req.Terms = this.textBox1.Text.Trim();
            //req.PageSize = 2;
            //req.PageNumber = 1;


            //FOXNEWS_20130505_1300
            //R202_20130505_1300
            //WFSB_20130505_1200
            //FOXBN_20130505_1200
            //WLWT_20130505_1200
            //WKYT_20130505_1200
            //WFXB_20130505_1200
            //WDBJ_20130505_1200
            //I25_20130505_1200
            //WESH_20130505_1100


            TestTV();
            //TestNM();
            //TestTW();


            //req.StartDate = /*DateTime.Now.AddDays(-14)*/new DateTime(2010,12,01);
            //req.EndDate = /*DateTime.Now*/new DateTime(2010, 12, 27);

            //req.GUIDList = "41bf1ad5-44b6-40f5-8708-c9ddd5a7ddf3, 9f391195-8597-401a-9b8a-8391fcea07d9, 4cf9d1f3-6552-4919-9ab5-6a97fd65c1f6, 01d8d91b-698a-49b3-9209-709265f3fbf8, ee48ab25-8a01-4b87-ad10-b6128fe46c84, b974d505-cde8-4928-8124-7598f744e22d, 7c8d0c8d-fa79-42e9-b647-1c76bcf62235, 88544875-fbf9-41b9-b4b3-1ce022da6a27, 5d10f70e-49b7-4830-bf61-02e30293648b, dfefb176-f692-46ac-a1a2-39b915abcb64, d7469445-15ee-41f0-a30a-aa7bd561698d, 3620eab3-2255-4580-912c-89398c4f4118, 7341d4cc-1f12-465a-8f6d-58830c3a3b32, a9dde1c4-32ff-430b-a015-a37298dc1e40, 6b41f4ee-3c45-4cbe-9c3a-a365fc3d97f4, a8229a87-4c39-451f-8f6b-35b306c414ba, 381ee289-cbda-4b97-a15b-35b360933f3b, 180ffb49-8e3c-428c-acdb-5204edd1d9e7, 8ccb549a-3424-4dd2-991a-db2e5f512a67, 4fcaecf3-9cbc-4c11-a5c6-7b3e57ecdf64, 52e77edd-0ae2-41d1-aaf5-1ee43d3906f0, 74b5716c-a99e-4d19-b060-05061e5853e0, 6d3bca37-874a-469b-826c-b98cacc2347c, 8490bb7e-3dc4-46b9-8bd0-69a3a71e423e, af80a801-9bff-461e-9c80-f8da885f6e45, 93c8fa49-e3ef-4ff3-b109-f8642d551c49, bc552b3e-59fe-4611-91da-1a2bb06eab7d, 61945945-60d2-4a51-9bb7-4b7e70e7dfcb, 63d33bdc-a600-41d1-8d12-1659ad15d3b6, 520af23a-17af-4695-90ea-066ad1b44385, f6b151c9-a8c9-431a-923b-e7f6b08fe5ea, c9f4ccef-4dec-4a10-942c-e8312723ae0c, 42ee22f7-886a-4a31-9a9f-970344022b21, a8792c14-169b-453a-b5ea-d6b6995cdb2c, b5494162-2a92-4552-a913-8cb1fe86950a, 0c776c56-45de-4a3d-9af0-57f0ce445189, 577307a7-2a53-48a8-a514-d5c4c88752ab, c5822d67-4e58-4f1d-a992-f706719eb801, b785010f-2cb6-4a6c-bdac-81d465181bc2, 69d69a48-c051-4aaf-a0ee-5e0afdb80097, 653d1361-e06e-4c5d-a072-0df32ed30ca3, 2115ad99-23bc-4c65-ba53-252117ee1c81, e9f6b986-21f8-4636-88ec-251afa665eef, 7303a674-2850-4afa-9cfd-b134e05b026e, 379b4d98-2dcc-4ef5-b738-612231a3f225, 1fe0065c-e539-4399-b861-4346723d4f36, 04c8347b-f4d1-4aac-80f0-28298ea370eb, 3843c387-546a-4e18-bd24-0e822981aec6, 0ce529ab-1ee3-4029-9919-9181bc1a6974, 69f71ba5-3198-4c5d-8d61-8fecfa611262, 8c5e28eb-3d45-416a-a39c-4118ebabaa08, 8da40046-5e0a-4829-9582-7ed6be169a8f, 99bd20a0-8298-4280-b4f6-7f9a2ce3ce80, 99d77013-7bda-49d6-8a59-4c47a1dbc1a7, a20698d4-a82c-4166-b068-79c8e2507b9c, fa18457d-ea72-4022-a42f-fe1e58ce1a35, a2c4a731-ddca-4bc3-9541-8ec646d58344, d416d0ec-0632-4b25-8e3c-138879feef63, c3ed9103-2dfc-46d4-b3d3-61e289493f32, 151bd8e4-1908-44a0-ad4b-402a0178abcd, 04f22d0b-23a5-4f71-a458-40f4364dbf06, 4acff30d-9dd6-4b40-b3f1-f4b981ff3bf5, e461d7fe-f755-450c-83d4-f4f8c063ed55, a78f1ce1-7d18-47d2-95c6-ea891e881c39, dcdbb628-ecd2-4fb3-bc1d-b036caf02da8, 26213004-5aa5-4867-9c76-00ea1b21025c, b15b14e9-c23f-434c-9691-33c4eab971bb, 58b959fa-ba87-4602-bce2-d58d545e8877, 293d0007-56c2-43a4-8f2d-be394c7d3a03, 1b49d743-e9e7-4d79-9e20-d4b85c9409eb, c0fa7822-9232-47d5-af2d-90b424ad0739, 2ce3f55a-2517-43f6-8f1b-88bfb83d710a, ee33f3ce-3d7e-4e97-a2ff-e29b256cf74b, 540ba032-8c34-4d98-bcec-c58c978f6693, 4ea6605d-117f-4c8e-9a7b-56abe60f2dce, b5dfa25a-7b0b-41d5-b801-e715533c7676, 4f5cc886-0794-41f9-8001-b84ff3c47c99, dad36b3a-a78b-4a19-84e1-b825c5f39d26, fcb08123-d118-41fc-8e34-5d92ad6002f0, 98196064-fa51-4bfd-9d85-e19ce3d4eada, 9265c837-309b-480a-b928-0308fc89db95, 163db260-6c1e-4621-a800-54dbba129f7e, 6568ebf1-7dc9-4c4b-adf9-cc83d6dff297, f44f6502-c443-4bd5-b16b-9729e3281ab4, c99e45a5-eb5d-44eb-9c90-f60bab816585, bc0f024d-96ad-46a8-8e28-b40b77d610e2, f42888a8-c9d6-4974-a613-a064bafe0767, 49752739-1a88-4b2c-b60d-73630a5b573b, b5858983-cba7-4304-9b64-73b04716ad11, 7b0deb5a-8ff8-4248-a6af-72e87198dc7c, 06661481-c44b-4465-8778-6007842b98ee, 962ad23a-b397-41d1-abca-7c6fd1693332, 85376891-f805-429d-863c-0034a4a3c19c, 6b1ec8c5-b8b7-47a1-90bb-4719d92f0220, 37129fc9-20ac-47af-8aa5-bd2005e138ac, a8c3809a-861d-4516-91a2-6fa2591034e7, 906691f1-8b41-43ba-aba9-12109e0aaa9d, 8883a625-e037-4eee-b84a-ca38fd22bfa7, 796faa6d-ff9b-4f05-a0b6-38a713f88072, 6f1766da-5742-4864-8e41-53bb6794b651";
            //req.GUIDList = "58b959fa-ba87-4602-bce2-d58d545e8877,a2c4a731-ddca-4bc3-9541-8ec646d58344,520af23a-17af-4695-90ea-066ad1b44385,61945945-60d2-4a51-9bb7-4b7e70e7dfcb,61945945-60d2-4a51-9bb7-4b7e70e7dfcb";
            try
            {
                //displayResults(sr);
            }
            catch (Exception ex)
            {
                //this.richTextBox1.Text = "ERROR: \n\n" + ex.ToString();
            }

        }

        private void TestTV()
        {
            try
            {
                SearchEngine se = new SearchEngine(new Uri("http://10.100.1.62:8080/solr/ctv362-2010/select?shards=10.100.1.62:8080/solr/ctv362-2011,10.100.1.62:8080/solr/ctv362-2012,10.100.1.62:8080/solr/ctv362-2013&"));
                SearchRequest req = new SearchRequest();
                req.Terms = "";
                req.PageNumber = 0;
                req.IsSentiment = true;
                req.LowThreshold = (float)-0.125000;
                req.HighThreshold = (float)0.125000;
                // req.SortFields = "datetime-";
                req.FragSize = 500;
                req.ClientGuid = new Guid("7722A116-C3BC-40AE-8070-8C59EE9E3D2A");
                //req.IQCCKeyList = "KSAN_20130605_0800";
                var sr = se.Search(req);
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void TestTW()
        {
            try
            {
                SearchEngine se = new SearchEngine(new Uri("http://10.100.1.42:8080/solr/core0/select?"));
                SearchTwitterRequest req = new SearchTwitterRequest();
                req.SearchTerm = "\"Taco Bell\" OR (\"Graduate to Go\" AND \"Taco Bell\") OR (\"Taco Bell Scholarship\"~50) OR (\"Taco Bell Graduate\"~50) OR (\"Taco Bell Education\"~50)";
                req.PageNumber = 0;
                req.IsSentiment = false;
                req.LowThreshold = (float)-0.125000;
                req.HighThreshold = (float)0.125000;
                req.IsHighlighting = true;
                req.IsOutRequest = true;
                // req.SortFields = "datetime-";
                req.FragSize = 500;
                req.IDs = new List<string>() { "336999407109144577" };
                bool error=false;
                var sr = se.SearchTwitter(req, false, out error);
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void TestNM()
        {
            try
            {
                SearchEngine se = new SearchEngine(new Uri("http://10.100.1.55:8080/solr/core0/select?"));
                SearchNewsRequest req = new SearchNewsRequest();
                req.SearchTerm = "\"teen driving\"";
                req.PageNumber = 0;
                req.IsSentiment = true;
                req.LowThreshold = (float)-0.125000;
                req.HighThreshold = (float)0.125000;
                // req.SortFields = "datetime-";
                req.FragSize = 500;
                req.ClientGuid = new Guid("7722A116-C3BC-40AE-8070-8C59EE9E3D2A");
                req.IDs = new List<string>() { "_8928503080"/*,"14313333782"*/ };
                var sr = se.SearchNews(req);
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void TestSM()
        {
            try
            {
                SearchEngine se = new SearchEngine(new Uri("http://10.100.1.58:8080/solr/core0/select?"));
                SearchSMRequest req = new SearchSMRequest();
                req.SearchTerm = "\"deer valley\" NOT \"deer valley road\"~5 NOT \"deer valley rd\"~5 NOT \"deer valley airport\"~10 NOT \"deer valley high\"~3 NOT \"deer valley hs\"~3 NOT \"deer valley antioch\"~200 NOT \"deer valley phoenix\"~30 NOT \"deer valley arizona\"~30 NOT \"deer valley sk\"~30 NOT \"deer valley saskatchewan\"~30 NOT \"boys volleyball\"~3 NOT \"deer valley peoria\"~50 NOT \"deer valley glendale\"~50 NOT \"deer valley FAA\"~20 NOT \"deer valley control tower\"~10 NOT \"deer valley avenue\"~5 NOT \"deer valley ave\"~5 NOT \"deer valley corporation\"~3 NOT \"deer valley homebuilders\"~3 NOT \"deer valley financial\"~3 NOT \"deer valley grad\"~5 NOT \"deer valley graduate\"~5 NOT \"deer valley academy\"~5 NOT \"deer valley pittsburg\"~200 NOT \"deer valley golf\"~50 NOT \"deer valley golfers\"~50 NOT \"deer valley palm ridge\"~30 NOT \"deer valley lawyer\"~5 NOT \"deer valley intersection\"~5 NOT \"deer valley student\"~5 NOT \"deer valley area\" NOT \"deer valley az\"~3 NOT \"deer valley playoffs\"~100 NOT \"deer valley semifinals\"~100 NOT \"deer valley championship\"~100#";
                req.PageNumber = 0;
                req.IsSentiment = true;
                req.LowThreshold = (float)-0.125000;
                req.HighThreshold = (float)0.125000;
                // req.SortFields = "datetime-";
                req.FragSize = 500;
                req.ClientGuid = new Guid("7722A116-C3BC-40AE-8070-8C59EE9E3D2A");
                req.ids = new List<string>() { "_8521240162"/*,"14313333782"*/ };
                SearchSMResult sr = se.SearchSocialMedia(req);
            }
            catch (Exception)
            {
                
                throw;
            }
        }

        private void displayResults(SearchResult res)
        {
            //this.richTextBox1.Text = "";
            //this.richTextBox1.Text += "Searching for '" + res.OriginalRequest.Terms + "'\n";
            //this.richTextBox1.Text += "Total hit count for search: " + res.TotalHitCount + "\n";
            //this.richTextBox1.Text += "Total hits on page #" + res.OriginalRequest.PageNumber + ": " + res.Hits.Count + "\n\n";

            //int hn = 0;
            //foreach (Hit hit in res.Hits)
            //{
            //    hn++;
            //    if (hn > 1) this.richTextBox1.Text += "----------------------------\n\n";
            //    this.richTextBox1.Text += "Hit #" + hn + ": GUID = " + hit.GUID + ", date = "+hit.TimeStamp.ToLongDateString()+"\n";
            //    int ocn = 0;
            //    foreach (TermOccurrence occurrence in hit.TermOccurrences)
            //    {
            //        ocn++;
            //        this.richTextBox1.Text += "     Occurrence #" + ocn + " at time offset [" + formatOffset(occurrence.TimeOffset) + "], surrounding text:\n";
            //        this.richTextBox1.Text += occurrence.SurroundingText + "\n\n";
            //    }
            //}

            //List<Hit> _ListOfHits = res.Hits;
            //_ListOfHits.Sort(delegate(Hit _Hit1, Hit _Hit2)
            //{
            //    return _Hit2.DateTime.CompareTo(_Hit1.DateTime) * (-1);
            //});

            dataGridView1.DataSource = res.Hits;
            lblCount.Text = res.Hits.Count.ToString();
        }

        private string formatOffset(int offs)
        {
            int h = 0;
            int m = 0;
            offs -= ((h = offs / 3600) * 3600);
            offs -= ((m = offs / 60) * 60);
            return ("" + h).PadLeft(2, '0') + ":" + ("" + m).PadLeft(2, '0') + ":" + ("" + offs).PadLeft(2, '0');
        }

        private void btnGenerateCSV_Click(object sender, EventArgs e)
        {
            ExcelToCSV frmExcelToCSV = new ExcelToCSV();
            if (frmExcelToCSV.ShowDialog() == DialogResult.OK)
            {
                txtGUID.Text = frmExcelToCSV.GUIDList;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SearchEngine se = new SearchEngine(new Uri("http://10.100.1.42:8080/solr/core0/select/"));

            SearchTwitterRequest req = new SearchTwitterRequest();
            req.SearchTerm = "weather";
            req.PageSize = 50;
            req.PageNumber = 0;
            req.IDs = new List<string> { "302462281634181120" };

            bool IsError=false;
            SearchTwitterResult _SearchResult = se.SearchTwitter(req,false,out IsError);

            txtGUID.Text = _SearchResult.ResponseXml;

        }



        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void btnTestSM_Click(object sender, EventArgs e)
        {
            SearchSMRequest smRequest = new SearchSMRequest();
            //smRequest.Author = "Scott Sheppard";
            smRequest.StartDate = new DateTime(2012, 05, 22);
            smRequest.EndDate = new DateTime(2012, 05, 23);
            smRequest.IsPmgLogging = false;
            smRequest.PageNumber = 0;
            smRequest.PageSize = 10;
            smRequest.PmgLogFileLocation = string.Empty;
            //smRequest.SearchTerm = "The";
            //smRequest.SocialMediaSource = "It is Alive in the Lab";
            smRequest.SortFields = "Date";
            //smRequest.SourceCategory=
            //smRequest.SourceRank = 10;
            List<String> lstsrcType = new List<String>();
            lstsrcType.Add("Blog");
            lstsrcType.Add("Social*Video");
            smRequest.SourceType = lstsrcType;

            smRequest.Facet = true;
            smRequest.FacetRangeOther = "all";
            smRequest.FacetRange = "itemHarvestDate_DT";
            smRequest.FacetRangeStarts = new DateTime(2012, 05, 20);
            smRequest.FacetRangeEnds = new DateTime(2012, 05, 23);
            smRequest.FacetRangeGap = RangeGap.DAY;
            smRequest.wt = ReponseType.json;
            smRequest.FacetRangeGapDuration = 1;

            List<string> lststring = new List<string>();
            lststring.Add("Blog");
            lststring.Add("Forum");
            lststring.Add("Comment");
            lststring.Add("Social*Video");
            lststring.Add("Social*Network");            

            //smRequest.lstfacetRange = lststring;

            SearchEngine se = new SearchEngine(new Uri("http://10.100.1.58:8080/solr/core0/select/"));

            //SearchSMResult searchSMResult = se.SearchSocialMedia(smRequest);
            //string json = se.SearchSocialMediaChart(smRequest);
            //txtGUID.Text = searchSMResult.ResponseXml;

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }


    }
}
