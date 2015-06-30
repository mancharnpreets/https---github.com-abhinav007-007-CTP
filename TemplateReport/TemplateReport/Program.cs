using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.Linq;
using System.IO;
using System.Configuration;
namespace TemplateReport
{
    class Program
    {
        static void Main(string[] args)
        {
           // GetPageTemplateName("{'Schema': 'ArticleLanding01', 'PageId': 'article-page', 'PageClass': 'two-col article-landing-large-withoutnav', 'Labels': 'tcm:510-113927',  'hero-carousel': [ 'tcm:510-957773', 'tcm:510-957778' ],  'teaser-row-small': [ 'tcm:510-957788', 'tcm:510-984993', 'tcm:510-901613' ] }");
            DataClassesDataContext tables = new DataClassesDataContext();

            Console.WriteLine("Total Records : " + tables.PAGE_CONTENTs.Count());
            List<PTPublicationInfo> mainList = tables.PAGE_CONTENTs.ToList().Where(x => !string.IsNullOrEmpty(GetPageTemplateName(x.CONTENT))).Select(m => new PTPublicationInfo { PublicationID = m.PUBLICATION_ID, PublicationTitle = GetPublicationName(m.PUBLICATION_ID, tables), PageTemplateName = GetPageTemplateName(m.CONTENT) })
                        .Distinct(new MyComparer()).ToList();


            Console.WriteLine("Total Records after removing duplicates : " + mainList.Count);
            List<string> templates = mainList.Select(x => x.PageTemplateName).Distinct().ToList();
            templates.Sort();
            Console.WriteLine("Total Templates Used: " + templates.Count);

            List<string> publications = tables.PUBLICATIONs.Select(x => x.PUBLICATION_TITLE).Distinct().ToList();
            publications.Sort();
            Console.WriteLine("Total Publications Used: " + publications.Count);

            List<PTPublicationInfo> ddd  = mainList.Where(x => string.IsNullOrEmpty(x.PublicationTitle)).ToList();
            StringBuilder builder = new StringBuilder();
            builder.Append("<table border='2' cellpadding=1 cellspacing=1><tr><td></td>");
            foreach (string template in templates)
            {
                builder.AppendFormat("<td><b>{0}</b></td>", template);
            }
            builder.Append("</tr>");
//            int count = 0;
            foreach (string publication in publications)
            {
                builder.AppendFormat("<tr><td><font color='Blue'><b>{0}</b></font></td>", publication);
                foreach (string template in templates)
                {                  
                  if (mainList.Any(x => x.PublicationTitle.ToLower() == publication.ToLower() && x.PageTemplateName.ToLower() == template.ToLower()))
                  {
                      builder.AppendFormat("<td ALIGN='center' BGCOLOR='#009933'><font color='White'><b>{0}<br/>{1}</b></font></td>", publication, template);
                      //count = count + 1;
                  }
                  else
                  {
                      builder.AppendFormat("<td ALIGN='center' BGCOLOR='#CC0000'><font color='White'><b>{0}<br/>{1}</b></font></td>", publication, template);
                  }
                }
                builder.Append("</tr>");
            }

            builder.Append("</table>");

            if (File.Exists(ConfigurationManager.AppSettings["OutputHTMLLocation"]))
            {
                File.Delete(ConfigurationManager.AppSettings["OutputHTMLLocation"]);
            }
            StreamWriter writer = new StreamWriter(ConfigurationManager.AppSettings["OutputHTMLLocation"]);
            writer.Write(builder.ToString());
            writer.Close();

            Console.WriteLine("File Created at Path : " + ConfigurationManager.AppSettings["OutputHTMLLocation"]);
            Console.WriteLine("Press any key to continue.......");
            Console.ReadKey();
        }


        private static string GetPageTemplateName(string content)
        {
            if (content.Contains("Schema"))
            {
                content = content.Substring(content.IndexOf("Schema") + 10);
                if (content.Contains("PageId"))
                {
                    content = content.Substring(0, content.IndexOf("PageId") - 4);
                    return content;
                }

                return string.Empty;
            }
            else
            {
                return string.Empty;
            }
        }

        private static string GetPublicationName(int ID, DataClassesDataContext tables)
        {
            return tables.PUBLICATIONs.Where(x => x.PUBLICATION_ID == ID).FirstOrDefault().PUBLICATION_TITLE;
        }
    }

    public class PTPublicationInfo
    {
        public int PublicationID { get; set; }

        public string PublicationTitle { get; set; }

        public string PageTemplateName { get; set; }
    }

    public class MyComparer : IEqualityComparer<PTPublicationInfo>
    {
        public bool Equals(PTPublicationInfo x, PTPublicationInfo y)
        {
            return (x.PublicationID == y.PublicationID && x.PageTemplateName == y.PageTemplateName);
        }

        public int GetHashCode(PTPublicationInfo obj)
        {
            return string.Format("{0}{1}", obj.PublicationID, obj.PageTemplateName).GetHashCode();
        }
    }
}
