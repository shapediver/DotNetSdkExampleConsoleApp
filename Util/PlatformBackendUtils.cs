using ShapeDiver.SDK;
using PDTO = ShapeDiver.SDK.PlatformBackend.DTO;
using GDTO = ShapeDiver.SDK.GeometryBackend.DTO;
using ShapeDiver.SDK.PlatformBackend;
using ShapeDiver.SDK.GeometryBackend;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ShapeDiver.SDK.PlatformBackend.DTO;

namespace DotNetSdkSampleConsoleApp.Util
{
    /// <summary>
    /// Utility functionality related to the platform backend API.
    /// </summary>
    internal static class PlatformBackendUtils
    {

        static Dictionary<PermissionModelEnum, ModelGetEmbeddableFields> MapModelPermToEmbedField = new Dictionary<PermissionModelEnum, ModelGetEmbeddableFields>()
        {
            { PermissionModelEnum.EmbedAccessDomains, ModelGetEmbeddableFields.Accessdomains },
            { PermissionModelEnum.EmbedBackendProperties, ModelGetEmbeddableFields.Backend_Properties },
            { PermissionModelEnum.EmbedBackendSystems, ModelGetEmbeddableFields.Backend_System },
            { PermissionModelEnum.EmbedBookmark, ModelGetEmbeddableFields.Bookmark },
            { PermissionModelEnum.EmbedDecoration, ModelGetEmbeddableFields.Decoration },
            { PermissionModelEnum.EmbedOrganization, ModelGetEmbeddableFields.Organization },
            { PermissionModelEnum.EmbedTags, ModelGetEmbeddableFields.Tags },
            { PermissionModelEnum.EmbedUser, ModelGetEmbeddableFields.User },
            // Note: excluding embeddable fields for tickets here
            //{ PermissionModelEnum.GetTicket, ModelGetEmbeddableFields.Ticket },
            //{ PermissionModelEnum.GetTicketBackend, ModelGetEmbeddableFields.Backend_Ticket },
            //{ PermissionModelEnum.GetTicketAuthor, ModelGetEmbeddableFields.Author_Ticket },
        };

        /// <summary>
        /// Map permissions for a model to embeddable fields for model GET calls.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        internal static List<ModelGetEmbeddableFields> MapModelPermissionsToEmbedFields(ModelPublicDto model)
        {
            var fields = model.Permissions
                .Where(p => MapModelPermToEmbedField.ContainsKey(p))
                .Select(p => MapModelPermToEmbedField[p]).ToList();
            if (fields.Contains(ModelGetEmbeddableFields.Accessdomains))
                fields.Add(ModelGetEmbeddableFields.Global_Accessdomains);
            return fields;
        }

    }
}
