using Google.Apis.Auth.OAuth2;
using Google.Apis.Download;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Web;

namespace MIS_Server.Models
{
    public class GoogleDriveFilesRepository
    {
        
        //defined scope. 變數的命名第一個字元要小寫
        public static string[] scopes = { DriveService.Scope.Drive }; // application 短期存取(scope) 使用者的資料

        //create Drive API service.
        public static DriveService getDriveService()     // 函數的命名第一個字元要小寫
        {
            //get Credentials from client_secret.json file 
            UserCredential credential;
            var secretPath = System.Web.Hosting.HostingEnvironment.MapPath("~/Content/");   // path of id and key 
            using (var streamDevice = new FileStream(Path.Combine(secretPath, "client_secret.json"), FileMode.Open, FileAccess.Read)) // 讀取檔案的串流設備
            {
                String credentialPath = Path.Combine(secretPath, "DriveServiceCredentials.json"); // 憑證的路徑

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(  // 製作憑證
                    GoogleClientSecrets.Load(streamDevice).Secrets,
                    scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credentialPath, true)).Result;
            }

            //create Drive API service.
            DriveService driveService = new DriveService(new BaseClientService.Initializer()  // 提供雲端硬碟服務的物件(做設定)
            {
                HttpClientInitializer = credential,
                ApplicationName = "GoogleDriveRestAPI-v3",
            });
            return driveService;
        }

        //get all files from Google Drive.
        public static List<GoogleDriveFiles> getAllDriveFiles()
        {
            DriveService driveService = getDriveService();

            // declare parameters of request.
            FilesResource.ListRequest fileListRequestParameters = driveService.Files.List();

            //listRequest.PageSize = 10;
            //listRequest.PageToken = 10;
            fileListRequestParameters.Fields = "nextPageToken, files(id, name, size, version, createdTime)";

            // get file list through interface.
            IList<Google.Apis.Drive.v3.Data.File> files = fileListRequestParameters.Execute().Files;  // 來自 nuget 的 Google.Apis.Drive.v3.Data.File
            List<GoogleDriveFiles> fileList = new List<GoogleDriveFiles>();

            if (files != null && files.Count > 0)
            {
                foreach (var file in files)
                {
                    GoogleDriveFiles File = new GoogleDriveFiles
                    {
                        Id = file.Id,
                        Name = file.Name,
                        Size = file.Size,
                        Version = file.Version,
                        CreatedTime = file.CreatedTime
                    };
                    fileList.Add(File);
                }
            }
            return fileList;
        }

        //file Upload to the Google Drive.
        public static void FileUpload(HttpPostedFileBase file)
        {
            if (file != null && file.ContentLength > 0)
            {
                
                // step 1: 上傳檔案及所在的目錄
                string pathForDownloadRepositories = Path.Combine(HttpContext.Current.Server.MapPath("~/GoogleDriveFiles"),
                Path.GetFileName(file.FileName));
                file.SaveAs(pathForDownloadRepositories);   // 先做儲存再上傳

                // step 2: 整理上傳的資料格式
                var FileMetaData = new Google.Apis.Drive.v3.Data.File();
                FileMetaData.Name = Path.GetFileName(file.FileName);
                FileMetaData.MimeType = MimeMapping.GetMimeMapping(pathForDownloadRepositories);

                // step 3: 呼叫上傳
                FilesResource.CreateMediaUpload request;  // requeset 是原創作者的命名,真實的用途是「上傳的媒介」 
                DriveService driveService = getDriveService();
                using (var streamDevice = new System.IO.FileStream(pathForDownloadRepositories, System.IO.FileMode.Open))
                {
                    request = driveService.Files.Create(FileMetaData, streamDevice, FileMetaData.MimeType);
                    request.Fields = "id";  // get file id, id = file.Id
                    request.Upload();
                }
            }
        }

        //Download file from Google Drive by fileId.
        public static string DownloadGoogleFile(string fileId)
        {
            DriveService driveService = getDriveService();

            string FolderPath = System.Web.HttpContext.Current.Server.MapPath("/GoogleDriveFiles/");
            FilesResource.GetRequest request = driveService.Files.Get(fileId);

            string FileName = request.Execute().Name;   // Name 是 Execute() 回傳的資料
            string credentialPath = System.IO.Path.Combine(FolderPath, FileName);

            MemoryStream stream1 = new MemoryStream();

            // Add a handler which will be notified on progress changes.
            // It will notify on each chunk download and when the
            // download is completed or failed.
            request.MediaDownloader.ProgressChanged += (Google.Apis.Download.IDownloadProgress progress) =>
            {
                switch (progress.Status)
                {
                    case DownloadStatus.Downloading:
                        {
                            Console.WriteLine(progress.BytesDownloaded);
                            break;
                        }
                    case DownloadStatus.Completed:
                        {
                            Console.WriteLine("Download complete.");
                            SaveStream(stream1, credentialPath);
                            break;
                        }
                    case DownloadStatus.Failed:
                        {
                            Console.WriteLine("Download failed.");
                            break;
                        }
                }
            };
            request.Download(stream1);
            return credentialPath;
        }

        // file save to server path
        private static void SaveStream(MemoryStream stream, string credentialPath)
        {
            using (System.IO.FileStream file = new FileStream(credentialPath, FileMode.Create, FileAccess.ReadWrite))
            {
                stream.WriteTo(file);
            }
        }

        //Delete file from the Google drive
        public static void DeleteFile(GoogleDriveFiles files)
        {
            DriveService driveService = getDriveService();
            try
            {
                // Initial validation.
                if (driveService == null)
                    throw new ArgumentNullException("driveService");

                if (files == null)
                    throw new ArgumentNullException(files.Id);

                // Make the request.
                driveService.Files.Delete(files.Id).Execute();
            }
            catch (Exception ex)
            {
                throw new Exception("Request Files.Delete failed.", ex);
            }
        }
    }
}