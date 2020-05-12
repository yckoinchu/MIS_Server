using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MIS_Server.Models;

namespace MIS_Server.Controllers
{
    public class MisGoogleDriveController : Controller
    {
        //#region 建構式

        //    public readonly GoogleDriveFilesRepository _Gr;

        //    public MisGoogleDriveController()
        //    {
        //        _Gr = new GoogleDriveFilesRepository();
        //    }

        //#endregion

        [HttpGet]
        public ActionResult GetGoogleDriveFiles()
        {
            var result = GoogleDriveFilesRepository.getAllDriveFiles();
            List<GoogleDriveFiles> googleDriveFiles = new List<GoogleDriveFiles>();
            foreach(var item in result)
            {
                if(item.Parents == null)
                {
                    continue;
                }
                if(item.Parents[0] == "0AN0RxaVnRLmVUk9PVA" && item.Name == "DIM")
                {
                    googleDriveFiles.Add(item);
                }

            }
            return View(googleDriveFiles);
        }

        public ActionResult getFilesInFolder(string folderId)
        {
            return View(GoogleDriveFilesRepository.getFilesInFolder(folderId));
        }

        [HttpPost]
        public ActionResult CreateFolder(String FolderName)
        {
            GoogleDriveFilesRepository.CreateFolder(FolderName);
            return RedirectToAction("GetGoogleDriveFiles");
        }

        [HttpPost]
        public ActionResult FileUploadInFolder(GoogleDriveFiles FolderId, HttpPostedFileBase file)
        {
            GoogleDriveFilesRepository.FileUploadInFolder(FolderId.Id, file);
            return RedirectToAction("GetGoogleDriveFiles");
        }

        [HttpPost]
        public ActionResult DeleteFile(GoogleDriveFiles file)
        {
            GoogleDriveFilesRepository.DeleteFile(file);
            return RedirectToAction("GetGoogleDriveFiles");
        }

        [HttpPost]
        public ActionResult UploadFile(HttpPostedFileBase file)
        {
            GoogleDriveFilesRepository.FileUpload(file);
            return RedirectToAction("GetGoogleDriveFiles"); // GetGoogleDriveFiles
        }

        public void DownloadFile(string id)
        {
            string FilePath = GoogleDriveFilesRepository.DownloadGoogleFile(id);


            Response.ContentType = "application/zip";
            Response.AddHeader("Content-Disposition", "attachment; filename=" + Path.GetFileName(FilePath));
            Response.WriteFile(System.Web.HttpContext.Current.Server.MapPath("~/GoogleDriveFiles/" + Path.GetFileName(FilePath)));
            Response.End();
            Response.Flush();
        }
    }
}