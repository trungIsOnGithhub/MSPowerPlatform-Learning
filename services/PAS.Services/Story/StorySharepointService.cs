using Microsoft.SharePoint.Client;
using PAS.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PAS.Services
{
    public class StorySharepointService : IStorySharepointService
    {
        private readonly ISharepointContextProvider _sharepointContextProvider;
        private readonly string listName = Common.Constants.SharepointFields.StoryList.LIST_NAME;

        public StorySharepointService(
            ISharepointContextProvider sharepointContextProvider)
        {
            _sharepointContextProvider = sharepointContextProvider;
        }

        public async Task<Model.Story> GetStory(Model.Story storyModel)
        {
            try
            {
                ClientContext ctx = await _sharepointContextProvider.GetClientContext();

                var storyList = ctx.Web.Lists.GetByTitle(listName);
                var folderPath = EnsureFolder(ctx, storyList, storyModel.User.Id.ToString());

                CamlQuery camlQuery = new CamlQuery();
                camlQuery.ViewXml = "<View>" +
                        "<Query>" +
                            "<Where>" +
                                "<And>" +
                                    "<Eq>" +
                                        "<FieldRef Name='ID' />" +
                                        "<Value Type ='Text'>" + storyModel.SharepointID + "</Value>" +
                                    "</Eq>" +
                                    "<Eq>" +
                                        "<FieldRef Name='IsLocked' />" +
                                        "<Value Type ='Boolean'>0</Value>" +
                                    "</Eq>" +
                                "</And>" +
                            "</Where>" +
                        "</Query>" +
                    "<RowLimit Paged='False'>1</RowLimit>" +
                    "</View>";

                camlQuery.FolderServerRelativeUrl = folderPath;
                ListItemCollection colListItems = storyList.GetItems(camlQuery);

                ctx.Load(colListItems, items => items.Include(
                    item => item.Id,
                    item => item[Common.Constants.SharepointFields.StoryList.TITLE],
                    item => item[Common.Constants.SharepointFields.StoryList.DESCRIPTION]
                ));

                ctx.ExecuteQuery();

                if (colListItems.Count == 0)
                {
                    throw new Exception($"Cannot find Story list item with ID={storyModel.SharepointID}. It can be deleted or you don't have permission to view it.");
                }

                ListItem listItem = colListItems.FirstOrDefault();
                storyModel.Title = listItem[Common.Constants.SharepointFields.StoryList.TITLE]?.ToString();
                storyModel.Description = listItem[Common.Constants.SharepointFields.StoryList.DESCRIPTION]?.ToString();
                return storyModel;
            }
            catch (Exception e)
            {
                throw e;
            }

        }

        public async Task<List<Model.Dto.Story>> GetOldStories(int userId)
        {
            ClientContext ctx = await _sharepointContextProvider.GetClientContext();

            var storyList = ctx.Web.Lists.GetByTitle(listName);

            var folderPath = EnsureFolder(ctx, storyList, userId.ToString());

            CamlQuery camlQuery = new CamlQuery();
            camlQuery.ViewXml = "<View>" +
                    "<Query>" +
                        "<Where>" +
                            "<Eq>" +
                                "<FieldRef Name='IsLocked' />" +
                                "<Value Type ='Boolean'>1</Value>" +
                            "</Eq>" +
                        "</Where>" +
                        "<OrderBy>" + 
                            "<FieldRef Name='Created' Ascending='FALSE'/>" + 
                        "</OrderBy>" +
                    "</Query>" +
                "</View>";

            camlQuery.FolderServerRelativeUrl = folderPath;
            ListItemCollection colListItems = storyList.GetItems(camlQuery);

            ctx.Load(colListItems, items => items.Include(
                item => item.Id,
                item => item[Common.Constants.SharepointFields.StoryList.TITLE],
                item => item[Common.Constants.SharepointFields.StoryList.DESCRIPTION],
                item => item[Common.Constants.SharepointFields.StoryList.CREATED]
            ));
            ctx.ExecuteQuery();

            List<Model.Dto.Story> result = new List<Model.Dto.Story>();

            foreach (var item in colListItems)
            {
                result.Add(new Model.Dto.Story
                {
                    Id = item.Id,
                    Title = item[Common.Constants.SharepointFields.StoryList.TITLE]?.ToString(),
                    Description = item[Common.Constants.SharepointFields.StoryList.DESCRIPTION]?.ToString(),
                    /*
                     * Bug: Using DateTime.Parse with InvariantCulter can cause error on non-English machine
                     * Solution: 
                     * - Cast to DateTime object using Convert.ToDateTime
                     * Since the returned field is already serialized into DateTime (but casted as Object type)
                     * - Set parameter `provider` to null to specify the CultureInfo value into current machine value
                     */
                    // CreatedAt = DateTime.Parse(item[Common.Constants.SharepointFields.StoryList.CREATED].ToString(), null)
                    CreatedAt = (DateTime) item[Common.Constants.SharepointFields.StoryList.CREATED]
                });
            }
            return result;
        }
        
        public async Task<int> CreateStory(Model.Story storyModel)
        {
            var ctx = await _sharepointContextProvider.GetAppOnlyContext();

            var storyList = ctx.Web.Lists.GetByTitle(listName);

            var folderPath = EnsureFolder(ctx, storyList, storyModel.User.Id.ToString());

            Principal spManager = ctx.Web.EnsureUser(storyModel.Manager.LoginName);

            var storyCreateInfo = new ListItemCreationInformation();
            storyCreateInfo.FolderUrl = folderPath;
            var newStoryItem = storyList.AddItem(storyCreateInfo);
            newStoryItem[Common.Constants.SharepointFields.StoryList.TITLE] = storyModel.Title;
            newStoryItem[Common.Constants.SharepointFields.StoryList.DESCRIPTION] = storyModel.Description;
            newStoryItem[Common.Constants.SharepointFields.StoryList.ISLOCKED] = false;
            newStoryItem[Common.Constants.SharepointFields.StoryList.AUTHOR] = spManager;

            newStoryItem.Update();
            ctx.Load(newStoryItem, item => item.HasUniqueRoleAssignments);
            ctx.ExecuteQuery();

            if (newStoryItem.Id != 0)
            {
                LockedOldStories(ctx, storyList, folderPath, newStoryItem);
            }

            Principal spUser = ctx.Web.EnsureUser(storyModel.User.LoginName);
            Group BOD = ctx.Web.SiteGroups.GetByName("BOD");

            var roleDefinition = ctx.Site.RootWeb.RoleDefinitions.GetByType(RoleType.Contributor);
            var roleBindings = new RoleDefinitionBindingCollection(ctx) { roleDefinition };
            if (!newStoryItem.HasUniqueRoleAssignments)
            {
                newStoryItem.BreakRoleInheritance(false, false);
            }

            newStoryItem.RoleAssignments.Add(spUser, roleBindings);
            newStoryItem.RoleAssignments.Add(spManager, roleBindings);
            newStoryItem.RoleAssignments.Add(BOD, roleBindings);
            ctx.ExecuteQuery();
            return newStoryItem.Id;
        }

        public Model.Story UpdateStory(Model.Story storyModel)
        {
            var ctx = _sharepointContextProvider.GetClientContext().Result;

            try
            {
                var storyList = ctx.Web.Lists.GetByTitle(listName);
                var folderPath = EnsureFolder(ctx, storyList, storyModel.User.Id.ToString());
                CamlQuery camlQuery = new CamlQuery();
                camlQuery.ViewXml = "<View>" +
                        "<Query>" +
                            "<Where>" +
                                "<Eq>" +
                                    "<FieldRef Name='ID' />" +
                                    "<Value Type='Text'>" + storyModel.SharepointID + "</Value>" +
                                "</Eq>" +
                            "</Where>" +
                        "</Query>" +
                    "<RowLimit Paged='False'>1</RowLimit>" +
                    "</View>";

                camlQuery.FolderServerRelativeUrl = folderPath;
                ListItemCollection colListItems = storyList.GetItems(camlQuery);
                ctx.Load(colListItems, items => items.Include(
                    item => item.Id,
                    item => item[Common.Constants.SharepointFields.StoryList.TITLE],
                    item => item[Common.Constants.SharepointFields.StoryList.DESCRIPTION]
                ));
                ctx.ExecuteQuery();

                if (colListItems.Count == 0)
                {
                    throw new Exception($"Cannot find Story with ID={storyModel.SharepointID}");
                }

                ListItem listItem = colListItems.FirstOrDefault();
                listItem[Common.Constants.SharepointFields.StoryList.TITLE] = storyModel.Title;
                listItem[Common.Constants.SharepointFields.StoryList.DESCRIPTION] = storyModel.Description;
                listItem.Update();
                ctx.ExecuteQuery();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return storyModel;
        }

        private string EnsureFolder(ClientContext ctx, List list, string folderName)
        {
            try
            {
                string folderUrl = String.Empty;
                Web web = ctx.Web;
                ctx.Load(web, w => w.ServerRelativeUrl);
                ctx.Load(list, l => l.Title);
                ctx.Load(list.RootFolder, r => r.ServerRelativeUrl);

                FolderCollection folders = list.RootFolder.Folders;

                ctx.Load(folders, fl => fl.Include(f => f.Name).Where(f => f.Name == folderName));

                ctx.ExecuteQuery();

                if (folders.Any())
                {
                    folderUrl = $"{web.ServerRelativeUrl}/{list.RootFolder.ServerRelativeUrl}/{folderName}";
                }
                else
                {
                    var adminCtx = _sharepointContextProvider.GetAppOnlyContext().GetAwaiter().GetResult();
                    var adminCtxList = adminCtx.Web.Lists.GetByTitle(list.Title);
                    var folderRelativeUrl = CreateFolder(adminCtx, adminCtxList, folderName);
                    folderUrl = $"{web.ServerRelativeUrl}/{folderRelativeUrl}";
                }
                return folderUrl;
            }
            catch(Exception e)
            {
                throw e;
            }

        }

        private string CreateFolder(ClientContext ctx, List storyList, string userId)
        {
            try
            {
                ListItemCreationInformation itemCreateInfo = new ListItemCreationInformation()
                {
                    UnderlyingObjectType = FileSystemObjectType.Folder,
                    LeafName = userId.ToString()
                };
                var listItem = storyList.AddItem(itemCreateInfo);
                listItem[Common.Constants.SharepointFields.StoryList.TITLE] = userId;
                listItem.Update();
                ctx.ExecuteQuery();
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return $"{storyList.RootFolder.ServerRelativeUrl}/{userId}";
        }

        private bool LockedOldStories(
            ClientContext ctx,
            List storyList,
            string folderPath,
            ListItem newListItem)
        {
            try
            {
                CamlQuery camlQuery = new CamlQuery();
                camlQuery.ViewXml = "<View>" +
                        "<Query>" +
                            "<Where>" +
                                "<Neq>" +
                                    "<FieldRef Name='ID' />" +
                                    "<Value Type ='Text'>" + newListItem.Id + "</Value>" +
                                "</Neq>" +
                            "</Where>" +
                        "</Query>" +
                    "</View>";

                camlQuery.FolderServerRelativeUrl = folderPath;
                ListItemCollection storyListItems = storyList.GetItems(camlQuery);
                ctx.Load(storyListItems, items => items.Include(
                    item => item.Id,
                    item => item[Common.Constants.SharepointFields.StoryList.ISLOCKED]
                ));
                ctx.ExecuteQuery();


                if (storyListItems.Count == 0)
                {
                    return true;
                }
                else
                {
                    foreach (ListItem story in storyListItems)
                    {
                        story[Common.Constants.SharepointFields.StoryList.ISLOCKED] = true;
                        story.Update();
                    }
                    ctx.Load(storyListItems);
                    ctx.ExecuteQuery();
                    return true;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool DeleteOldStory(int storySPId)
        {
            var ctx = _sharepointContextProvider.GetClientContext().Result;
            var storyList = ctx.Web.Lists.GetByTitle(listName);
            ctx.Load(storyList);
            ctx.ExecuteQuery();

            CamlQuery camlQuery = new CamlQuery();
            camlQuery.ViewXml = "<View Scope='RecursiveAll'>" +
                    "<Query>" +
                        "<Where>" +
                            "<Eq>" +
                                "<FieldRef Name='ID' />" +
                                "<Value Type ='Text'>" + storySPId + "</Value>" +
                            "</Eq>" +
                            "<Eq>" +
                                "<FieldRef Name='IsLocked' />" +
                                "<Value Type='Boolean'>1</Value>" +
                            "</Eq>" +
                        "</Where>" +
                    "</Query>" +
                "<RowLimit Paged='False'>1</RowLimit>" +
                "</View>";

            ListItemCollection colListItems = storyList.GetItems(camlQuery);

            ctx.Load(colListItems, items => items.Include(
                item => item.Id
            ));
            ctx.ExecuteQuery();

             if (colListItems.Count == 0)
            {
                return false;
            }

            ListItem storyListItem = colListItems.FirstOrDefault();
            storyListItem.DeleteObject();
            ctx.ExecuteQuery();

            return true;
        }
    }

}
