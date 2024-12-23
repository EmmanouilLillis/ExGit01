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
    // The PostUpdateObjectExample class implements the IApiProvider interfaces for the PortalApiProject
    public class AzureActiveDirectoryGroup : IApiProviderFor<QER.CompositionApi.Portal.PortalApiProject>, IApiProvider
    {
        public void Build(IApiBuilder builder)
        {
            builder.AddMethod(Method.Define("exercise/UpdateAzureGroups")
                .Handle<PostedID, string>("POST", async (posted, qr, ct) =>
                {
                    string displayName = "";
                    string mailNickName = "";
                    string description = "";
                    string uid_aadgroup = posted.uid_aadgroup;

                    //Search with the uid
                    var query1 = Query.From("AADGroup")
                                      .Select("*")
                                      .Where(string.Format("UID_AADGroup = '{0}'", uid_aadgroup));

                    var tryget = await qr.Session.Source()
                                       .TryGetAsync(query1, EntityLoadType.DelayedLogic, ct)
                                       .ConfigureAwait(false);

                    // Check if the entity was successfully retrieved
                    if (tryget.Success)
                    {
                        // Loop through each column in the posted data to update the entity's properties
                        foreach (var column in posted.columns)
                        {
                            // Assign values based on column names and update the entity accordingly
                            if (column.column == "DisplayName")
                            {
                                displayName = column.value.ToString();
                                if (displayName.StartsWith("aad"))
                                {
                                    await tryget.Result.PutValueAsync("DisplayName", displayName, ct).ConfigureAwait(false);
                                }
                                else
                                {
                                    return "wrong format of name , start with 'aad'";
                                }
                                
                            }
                            else if (column.column == "MailNickName")
                            {
                                mailNickName = column.value.ToString();
                                await tryget.Result.PutValueAsync("MailNickName", mailNickName, ct).ConfigureAwait(false);
                            }
                            else if (column.column == "Description")
                            {
                                description = column.value.ToString();
                                await tryget.Result.PutValueAsync("Description", description, ct).ConfigureAwait(false);
                            }
            
                        }
                        using (var u = qr.Session.StartUnitOfWork())
                        {
                            await u.PutAsync(tryget.Result, ct).ConfigureAwait(false);
                            await u.CommitAsync(ct).ConfigureAwait(false);
                        }

                        return "success update";
                    }
                    else
                    {
                        return "Fail Update";
                    }

                }));
        }

        public class PostedID
        {
            public string uid_aadgroup { get; set; }

            public columnsarray[] columns { get; set; }


        }

        public class columnsarray
        {
            public string column { get; set; }

            public object value { get; set; }
        }
    }
}
