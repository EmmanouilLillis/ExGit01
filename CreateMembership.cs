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
using Microsoft.SqlServer.Server;

namespace QBM.CompositionApi
{
    // The PostUpdateObjectExample class implements the IApiProvider interfaces for the PortalApiProject
    public class CreateMembership : IApiProviderFor<QER.CompositionApi.Portal.PortalApiProject>, IApiProvider
    {
        public void Build(IApiBuilder builder)
        {
            builder.AddMethod(Method.Define("exercise/CreateMembership")
                .Handle<PostedID, string>("POST", async (posted, qr, ct) =>
                {
                    var uidGroup = posted.Group_UID;
                    var userUid = posted.Username_UID;

                    
                    // Get key and org from aadgroup
                    var queryGetXObjectKey = Query.From("AADGroup")
                        .Select("*")
                        .Where(string.Format("UID_AADGroup = '{0}'", uidGroup));

                    var tryGetXObjectKeynOrg = await qr.Session.Source()
                        .TryGetAsync(queryGetXObjectKey, EntityLoadType.DelayedLogic, ct)
                        .ConfigureAwait(false);

                    if (tryGetXObjectKeynOrg?.Result == null)
                    {
                        return "Error: No data found for the provided AADGroup UID.";
                    }

                    var xObjectKey = await tryGetXObjectKeynOrg.Result.TryGetValueAsync<string>("XObjectKey").ConfigureAwait(false);
                    var uidOrg = await tryGetXObjectKeynOrg.Result.TryGetValueAsync<string>("UID_AccProduct").ConfigureAwait(false);

                    if (string.IsNullOrEmpty(xObjectKey.Result))
                    {
                        return "Error: XObjectKey is null or empty for the provided AADGroup UID.";
                    }

                    if (string.IsNullOrEmpty(uidOrg.Result))
                    {
                        return "Error: UID_AccProduct is null or empty for the provided AADGroup UID.";
                    }

                    //Get person uid from aaduser
                    var queryGetPersonUid = Query.From("AADUser")
                        .Select("*")
                        .Where(string.Format("UID_AADUser = '{0}'", userUid));

                    var tryGetPersonUid = await qr.Session.Source()
                        .TryGetAsync(queryGetPersonUid, EntityLoadType.DelayedLogic, ct)
                        .ConfigureAwait(false);

                    if (tryGetPersonUid?.Result == null)
                    {
                        return "Error: No data found for the provided AADUser UID.";
                    }

                    var personUid = await tryGetPersonUid.Result.TryGetValueAsync<string>("UID_Person").ConfigureAwait(false);

                    if (string.IsNullOrEmpty(personUid.Result))
                    {
                        return "Error: UID_Person is null or empty for the provided AADUser UID.";
                    }

                    // Get org from itshoporg
                    var queryGetOrg = Query.From("ITShopOrg")
                        .Select("*")
                        .Where(string.Format("UID_AccProduct = '{0}'", uidOrg.Result));

                    var tryGetOrg = await qr.Session.Source()
                        .TryGetAsync(queryGetOrg , EntityLoadType.DelayedLogic , ct )
                        .ConfigureAwait(false);

                    if (tryGetOrg?.Result == null)
                    {
                        return "Error: No data found for the provided ITShopOrg UID.";
                    }

                    var org = await tryGetOrg.Result.TryGetValueAsync<string>("UID_ITShopOrg").ConfigureAwait(false);

                    if (string.IsNullOrEmpty(org.Result))
                    {
                        return "Error: UID_ITShopOrg is null or empty for the provided ITShopOrg.";
                    }


                    //Create and insert everything
                    var newRequest = await qr.Session.Source().CreateNewAsync("PersonWantsOrg",
                        new EntityParameters
                        {
                            CreationType = EntityCreationType.DelayedLogic
                        }, ct).ConfigureAwait(false);

                    await newRequest.PutValueAsync("UID_PersonInserted", qr.Session.User().Uid, ct).ConfigureAwait(false);
                    await newRequest.PutValueAsync("UID_PersonOrdered" , personUid.Result , ct).ConfigureAwait(false);
                    await newRequest.PutValueAsync("ObjectKeyOrdered" , xObjectKey.Result , ct).ConfigureAwait(false);
                    await newRequest.PutValueAsync("UID_Org" , org.Result, ct).ConfigureAwait(false);

                    using (var u = qr.Session.StartUnitOfWork())
                    {
                        await u.PutAsync(newRequest, ct).ConfigureAwait(false);
                        await u.CommitAsync(ct).ConfigureAwait(false);
                    }

                    return "Success! The request has been made!";


                }));
        }

        public class PostedID
        {
            public string Group_UID { get; set; }

            public string Username_UID { get; set; }


        }
    }
}
