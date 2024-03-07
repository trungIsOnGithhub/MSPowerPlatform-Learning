using Microsoft.SharePoint.Client;
using System.Linq;

namespace PAS.Common.Extensions
{
    public static class ClientContextExtensions
    {
        /// <summary>
        /// Requires pre-load RoleDefinitions.Name property.
        /// </summary>
        public static bool ContainsRoleDefinition(this Web web, string nameOfDefinition)
        {
            return web.RoleDefinitions.Any(rd => rd.Name.ToLower() == nameOfDefinition.ToLower());
        }

        /// <summary>
        /// Requires pre-load RoleDefinitions.Name property.
        /// </summary>
        public static RoleDefinition GetRoleDefinition(this Web web, string nameOfDefinition)
        {
            return web.RoleDefinitions.FirstOrDefault(rd => rd.Name.ToLower() == nameOfDefinition.ToLower());
        }

        public static bool IsNull(this ClientObject clientObject)
        {
            //check object
            if (clientObject == null)
            {
                //client object is null, so yes, we're null (we can't even check the server object null property)
                return true;
            }
            else if (!clientObject.ServerObjectIsNull.HasValue)
            {
                //server object null property is itself null, so no, we're not null
                return false;
            }
            else
            {
                //server object null check has a value, so that determines if we're null
                return clientObject.ServerObjectIsNull.Value;
            }
        }
    }
}