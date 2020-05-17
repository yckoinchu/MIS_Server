using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MIS_Server.Models;    // 指專案底下所有建置的 models

namespace MIS_Server.Controllers    // users' applications
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
        public ActionResult GetGoogleDriveFiles()   // set DIM as the root directory for MIS department
        {
            var result = GoogleDriveFilesRepository.getAllDriveFiles();
            List<GoogleDriveFiles> googleDriveFiles = new List<GoogleDriveFiles>();
            foreach(var item in result)
            {
                if(item.Parents == null)
                {
                    continue;
                }
                if(item.Parents[0] == "0AN0RxaVnRLmVUk9PVA" && item.Name == "DIM")  // root 設定在 DIM 目錄
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
            return RedirectToAction("GetGoogleDriveFiles");     // 建目錄後，應該以更新資訊的方式呈現。目前還沒做到
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
            return RedirectToAction("GetGoogleDriveFiles"); // 上傳後，應該以更新資訊的方式呈現。目前還沒做到
        }

        public void DownloadFile(string id)
        {
            string file = GoogleDriveFilesRepository.DownloadGoogleFile(id);


            Response.ContentType = "application/zip";   // using System.Web; 壓縮檔
            Response.AddHeader("Content-Disposition", "attachment; filename=" + Path.GetFileName(file));  // 傳回 'file' 所在路徑的檔案名稱和副檔名
            Response.WriteFile(System.Web.HttpContext.Current.Server.MapPath("~/GoogleDriveFiles/" + Path.GetFileName(file)));  // 將file直接寫入HTTP 回應的輸出資料流
            Response.End();
            Response.Flush();
        }
    }
}