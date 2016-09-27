using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Web.Hosting;
using WebApplication1.Models;
using System.Text;
using System.Xml;
namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        int[] Counts = new int[3] { 0, 0, 0 };    // for counting files {small}{medium}{big}
        
        const int SmallFile = 10485760; //  10MB
        const int MediumFile = 52428800;    // 50MB
        const int BigFile = 104857600;  //  100MB

        List<Content> content = new List<Content>(); // container for List Contents
        DirectoryInfo directoryInfo;    // contains all browsing info
        FileInfo[] files;   // container for folders
        DirectoryInfo[] directories;    // container for directories
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";    
            return View();
        }

        public ActionResult Get()
        {
            ViewBag.Title = "Browse Folder";

            string path = System.Configuration.ConfigurationManager.AppSettings["CurrentBrowsingPath"];
            string value = "";        

            StringBuilder builderPath = new StringBuilder(path.ToString());  // using for back <- operation            
           
            ViewBag.Variable = RouteData.Values["id"];
            string id = ViewBag.Variable;

            if (ViewBag.Variable == "Back")
            {
                value = System.Configuration.ConfigurationManager.AppSettings["CurrentBrowsingPath"];
                char spliter = '\\';
                String[] substrings = value.Split(spliter);
                for (int i = 0; i < substrings.Length - 2; i++)
                {
                    ViewBag.ForwardPath += substrings[i] + "\\";
                }

                System.Configuration.ConfigurationManager.AppSettings["CurrentBrowsingPath"] = ViewBag.ForwardPath;
                directoryInfo = new DirectoryInfo(System.Configuration.ConfigurationManager.AppSettings["CurrentBrowsingPath"]);
            }
            else
            {
                directoryInfo = new DirectoryInfo(builderPath + ViewBag.Variable);
                builderPath.Append(id);
                builderPath.Append("\\");
                System.Configuration.ConfigurationManager.AppSettings["CurrentBrowsingPath"] = builderPath.ToString();
            }
           
            ViewBag.All = System.Configuration.ConfigurationManager.AppSettings["CurrentBrowsingPath"];          
        
            files = directoryInfo.GetFiles();
            directories = directoryInfo.GetDirectories(".", SearchOption.TopDirectoryOnly);            
           

            FolderGroupingBySize(directories);
            FileGroupingBySize(files, content);

            return View(content);
        }    
        public void FolderGroupingBySize(DirectoryInfo[] directories)
        {
            // Searching all Directories and sorting by size

            content = directories.Where(x => DirectorySize(x) <= 10)
                .Select(x=>new Content
            {
                Directory = x.ToString(),
                SmallSize = ++Counts[0]
            }).ToList();

            content.AddRange(directories.Where(x => DirectorySize(x) >= 10 && DirectorySize(x) <= 50)
                .Select(x => new Content
            {
                Directory = x.ToString(),
                MediumSize = ++Counts[1]
            }).ToList());

            content.AddRange(directories.Where(x => DirectorySize(x) >= 100)
               .Select(x => new Content
               {
                   Directory = x.ToString(),
                   BigSize = ++Counts[2]
               }).ToList());
        }
        public void FileGroupingBySize(FileInfo[] files, List<Content> content)
        {
            // Searching all Files and sorting by size

            content.AddRange(files.Where(x => x.Length <= SmallFile)
                .Select(x => new Content
                {
                    File = x.ToString(),
                    SmallSize = ++Counts[0]
                }).ToList());

            content.AddRange(files.Where(x => x.Length > SmallFile && x.Length <= MediumFile)
                .Select(x => new Content
                {
                    File = x.ToString(),
                    MediumSize = ++Counts[1]
                }).ToList());

            content.AddRange(files.Where(x => x.Length >= BigFile)
                .Select(x => new Content
                {
                    File = x.ToString(),
                    BigSize = ++Counts[2]
                }).ToList());
        }

        public static float DirectorySize(DirectoryInfo directoryInfo)
        {
            float size = 0;

            // Calculating file size.
            FileInfo[] files = directoryInfo.GetFiles();
            foreach (FileInfo file in files)
            {
                size += file.Length;
            }

            // Calculating subdirector size.
            DirectoryInfo[] directories = directoryInfo.GetDirectories();
            foreach (DirectoryInfo directory in directories)
            {
                size += DirectorySize(directory);
            }  
            return (size / 1024) / 1024; // return size in MB
        }      
    }
}
