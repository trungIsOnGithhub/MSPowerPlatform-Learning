using PAS.Common;
using PAS.Model.Domain;
using Microsoft.SharePoint.Client;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System;
using File = Microsoft.SharePoint.Client.File;
using PAS.Services.Extension;

namespace PAS.Services
{
    public class SharepointFileService : ISharepointFileService
    {
        private readonly ISharepointContextProvider ctxProvider;

        public SharepointFileService (ISharepointContextProvider ctxProvider)
        {
            this.ctxProvider = ctxProvider;
        }
       
        public async Task<List<SharepointImage>> GetAllFolderFiles(string folderRelativeUrl)
        {
            var ctx = await ctxProvider.GetClientContext();
            ctx.Load(ctx.Web, w => w.ServerRelativeUrl);
            await ctx.ExecuteQueryAsync();

            var imageCollection = new List<SharepointImage>();
            var folderRelativePath = ctx.Web.ServerRelativeUrl + folderRelativeUrl;

            if (ctx.Web.TryGetFileByServerRelativeUrl(folderRelativePath, out File _file))
            {
                Folder relatedFolder = ctx.Web.GetFolderByServerRelativeUrl(folderRelativePath);
                ctx.Load(relatedFolder, f => f.Files);
                await ctx.ExecuteQueryAsync();

                foreach (var file in relatedFolder.Files)
                {
                    var image = new SharepointImage
                    {
                        Title = file.Name,
                        SPId = file.UniqueId.ToString(),
                        SPRelativeUrl = file.ServerRelativeUrl,
                        Base64String = null
                    };
                    imageCollection.Add(image);
                }
            }

            return imageCollection;
        }

        public async Task<SharepointImage> ReadSharepointFile(string fileRelativeUrl)
        {
            var ctx = await ctxProvider.GetClientContext();

            File relatedFile = ctx.Web.GetFileByServerRelativeUrl(fileRelativeUrl);
            ctx.Load(relatedFile);
            await ctx.ExecuteQueryAsync();
            return await ConvertImageToBase64String(ctx, relatedFile);
        }

        private async Task<SharepointImage> ConvertImageToBase64String(ClientContext ctx, File file)
        {
            var binaryData = file.OpenBinaryStream();
            await ctx.ExecuteQueryAsync();
            var mStream = new MemoryStream();

            if (binaryData != null)
            {
                binaryData.Value.CopyTo(mStream);
                byte[] bytes = mStream.ToArray();

                var image = new SharepointImage
                {
                    Title = file.Name,
                    SPId = file.UniqueId.ToString(),
                    SPRelativeUrl = file.ServerRelativeUrl,
                    Base64String = Convert.ToBase64String(bytes)
                };
                return image;
            }
            return null;
        }
    }
}
