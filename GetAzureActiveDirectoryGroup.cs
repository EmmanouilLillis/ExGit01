// Import necessary namespaces
using System;
using System.IO;
using System.Web;
using System.Net;
using System.Threading;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Linq;
using System.Text;
using VI.Base;
using VI.DB;
using VI.DB.Entities;
using QBM.CompositionApi.ApiManager;
using QBM.CompositionApi.Definition;
using QBM.CompositionApi.Crud;
using QER.CompositionApi.Portal;

namespace QBM.CompositionApi
{
    public class GetAzureActiveDirectoryGroup : IApiProviderFor<QER.CompositionApi.Portal.PortalApiProject>, IApiProvider
    {
        // this is a comment fot Git-03 part 1 
        public void Build(IApiBuilder builder)
        {
            builder.AddMethod(Method.Define("exercise/GetAzureGroups")
                .Handle<PostedID , ReturnedAAD>("POST", async (posted , qr, ct) =>
                {
                    string uid_aadgroup = posted.Id;

                    var query = Query.From("AADGroup")
                        .Select("*")
                        .Where(string.Format("UID_AADGroup = '{0}'", uid_aadgroup));

                    var tryGet = await qr.Session.Source()
                        .TryGetAsync(query, EntityLoadType.DelayedLogic)
                        .ConfigureAwait(false);

                    // Convert the retrieved entity to a ReturnedAAD object and return it

                    if (tryGet.Success)
                    {
                        return await ReturnedAAD.fromEntity(tryGet.Result, qr.Session)
                        .ConfigureAwait(false);
                    }
                    else
                    {
                        return await ReturnedAAD.Fail();
                    }
                    
                }));
        }

        public class PostedID
        {
            public string Id { get; set; }
        }


        // The ReturnedAAD class represents the structure of the data we want to return 
        public class ReturnedAAD
        {
            public string DisplayName { get; set; }
            public string UID_AADGroup { get; set; }
            public string MailNickName {  get; set; }
            public string Description { get; set; }
            public string UID_AADOrganization { get; set; }

            public string message { get; set; }

            // Static method to create a ReturnedAAD instance from an Entity object
            public static async Task<ReturnedAAD> fromEntity(IEntity entity, ISession session)
            {
                // Instantiate a new ReturnedAAD object and populate it with data from the entity
                var g = new ReturnedAAD
                {
                    DisplayName = await entity.GetValueAsync<string>("DisplayName").ConfigureAwait(false),
                    UID_AADGroup = await entity.GetValueAsync<string>("UID_AADGroup").ConfigureAwait(false),
                    MailNickName = await entity.GetValueAsync<string>("MailNickName").ConfigureAwait(false),
                    Description = await entity.GetValueAsync<string>("Description").ConfigureAwait(false),
                    UID_AADOrganization = await entity.GetValueAsync<string>("UID_AADOrganization").ConfigureAwait(false),
                };

                // Return the populated ReturnedAAD object
                return g;
            }

            public static async Task<ReturnedAAD> Fail()
            {
                var g = new ReturnedAAD
                {
                    message = "No assignment Found"
                    
                };
                return g;
            }
        }
    }
}

