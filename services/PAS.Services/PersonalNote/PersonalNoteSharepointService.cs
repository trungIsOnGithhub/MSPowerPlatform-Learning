using Microsoft.SharePoint.Client;
using PAS.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PAS.Services
{
    public class PersonalNoteSharepointService : IPersonalNoteSharepointService
    {
        private readonly ISharepointContextProvider _sharepointContextProvider;
        private string sharepointListName = Common.Constants.SharepointFields.PersonalNotesList.LIST_NAME;

        public PersonalNoteSharepointService(
            ISharepointContextProvider sharepointContextProvider)
        {
            _sharepointContextProvider = sharepointContextProvider;
        }

        public List<Model.PersonalNote> GetPersonalNotesById(List<Model.PersonalNote> personalNoteList)
        {
            var ctx = _sharepointContextProvider.GetClientContext().Result;

            var personalNoteSharepointList = GetList(ctx);

            List<int> ids = personalNoteList.Select(note => note.SharepointId).ToList();
            var colListItems = GetListItemsByIds(ctx, personalNoteSharepointList, ids);

            var result = new List<Model.PersonalNote>();
            personalNoteList.ForEach(note =>
            {
                var noteListItem = colListItems.FirstOrDefault(item => item.Id == note.SharepointId);
                if (noteListItem != null)
                {
                    note.Content = noteListItem[Common.Constants.SharepointFields.PersonalNotesList.CONTENT]?.ToString();
                    result.Add(note);
                }
            });

            return result;
        }

        public Model.PersonalNote SavePersonalNote(Model.PersonalNote model)
        {
            var ctx = _sharepointContextProvider.GetAppOnlyContext().Result;

            var personalNoteList = GetList(ctx);

            try
            {
                var spUser = ctx.Web.EnsureUser(model.CreatedBy.LoginName);
                var createionInfo = new ListItemCreationInformation();
                var personalNoteItem = personalNoteList.AddItem(createionInfo);
                personalNoteItem[Common.Constants.SharepointFields.PersonalNotesList.CONTENT] = model.Content;
                personalNoteItem[Common.Constants.SharepointFields.PersonalNotesList.STORY_ID] = model.Story.SharepointID;
                personalNoteItem["Author"] = spUser;

                personalNoteItem.Update();

                ctx.Load(personalNoteItem, item => item.HasUniqueRoleAssignments);
                ctx.ExecuteQuery();

                model.SharepointId = personalNoteItem.Id;

                if (!personalNoteItem.HasUniqueRoleAssignments)
                {
                    personalNoteItem.BreakRoleInheritance(false, false);
                }

                var roleDefinition = ctx.Site.RootWeb.RoleDefinitions.GetByType(RoleType.Contributor);
                var roleBindings = new RoleDefinitionBindingCollection(ctx) { roleDefinition };
                personalNoteItem.RoleAssignments.Add(spUser, roleBindings);
                
                personalNoteItem.Update();
                
                ctx.ExecuteQuery();
            }
            catch(Exception ex)
            {
                throw ex;
            }
            return model;
        }

        public Model.PersonalNote UpdatePersonalNote(
            int sharepointId, 
            Model.PersonalNote newContent)
        {
            try
            {
                var ctx = _sharepointContextProvider.GetClientContext().Result;

                var personalNoteList = GetList(ctx);

                var colListItems = GetListItemsByIds(ctx, personalNoteList, new List<int> { sharepointId });

                if(colListItems.Count == 0)
                {
                    throw new Exception("Cannot find Private note sharepoint list item ID=" + sharepointId);
                }
                else
                {
                    var personalNoteItem = colListItems.FirstOrDefault();
                    personalNoteItem[Common.Constants.SharepointFields.PersonalNotesList.CONTENT] = newContent.Content.ToString();
                    personalNoteItem.Update();
                    ctx.ExecuteQuery();
                }
                return newContent;
            }
            catch(Exception e)
            {
                throw e;
            }

        }


        public void DeletePersonalNote(Model.PersonalNote model)
        {
            var ctx = _sharepointContextProvider.GetClientContext().Result;

            var personalNoteList =  GetList(ctx);

            var colListItems = GetListItemsByIds(ctx, personalNoteList, new List<int> { model.SharepointId });

            if(colListItems.Count == 0)
            {
                throw new NotFoundException($"Sharepoint couldn't find Private Note SharepointID={model.SharepointId}");
            }
            else
            {
                var personalNoteItem = colListItems.FirstOrDefault();
                personalNoteItem.DeleteObject();
                ctx.ExecuteQuery();
            }
        }

        private List GetList(ClientContext ctx)
        {
            var personalNoteList = ctx.Web.Lists.GetByTitle(sharepointListName);
            return personalNoteList;
        }

        private ListItemCollection GetListItemsByIds(
            ClientContext ctx, 
            List spList, 
            List<int> ids)
        {
            string valuesString = string.Empty;
            ids.ForEach(id => valuesString += $"<Value Type='Number'>{id}</Value>");

            CamlQuery camlQuery = new CamlQuery();
            camlQuery.ViewXml = "<View>" +
                    "<Query>" +
                        "<Where>" +
                                "<In>" +
                                    "<FieldRef Name='ID' />" +
                                    "<Values>" +
                                        valuesString +
                                    "</Values>" +
                                "</In>" +
                    "</Where>" +
                    "</Query>" +
                "</View>";

            ListItemCollection colListItems = spList.GetItems(camlQuery);
            ctx.Load(colListItems, items => items.Include(
                item => item[Common.Constants.SharepointFields.PersonalNotesList.CONTENT],
                item => item.Id,
                items => items.HasUniqueRoleAssignments
            ));
            ctx.ExecuteQuery();
            return colListItems;
        }
    }
}
