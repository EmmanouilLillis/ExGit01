
using System.Threading.Tasks;
using System.Linq;
using System.Text;
using System.Web;

using VI.Base;
using VI.DB;
using VI.DB.Entities;
using QBM.CompositionApi.ApiManager;
using QBM.CompositionApi.Definition;
using QBM.CompositionApi.Crud;
using System;
using System.Xml.Linq;

namespace QBM.CompositionApi
{
    public class DeleteAzureActiveDirectoryGroup : IApiProviderFor<QER.CompositionApi.Portal.PortalApiProject>, IApiProvider
    {
        public void Build(IApiBuilder builder)
        {
            builder.AddMethod(Method.Define("exercise/DeleteAzureGroups")
                .Handle<PostedID , string >("DELETE", async (posted, qr, ct) =>
                {

                    string uid_aad = posted.uid_aadgroup;



                    var query1 = Query.From("AADGroup")
                                      .Select("*")
                                      .Where(string.Format("UID_AADGroup = '{0}'", uid_aad));


                    var tryget1 = await qr.Session.Source()
                                        .TryGetAsync(query1, EntityLoadType.DelayedLogic, ct)
                                        .ConfigureAwait(false);

                    // Check if the entity was successfully retrieved
                    if (tryget1.Success)
                    {
                        using (var u = qr.Session.StartUnitOfWork())
                        {
                            // Get the entity to be deleted
                            var objecttodelete = tryget1.Result;

                            // Mark the entity for deletion
                            objecttodelete.MarkForDeletion();

                            await u.PutAsync(objecttodelete, ct).ConfigureAwait(false);

                            await u.CommitAsync(ct).ConfigureAwait(false);
                        }

                        return "You succesfully deleted the group";
                    }
                    else {
                        return "No assignment found with this uid"; 
                    }

                   
                }));
        }

        public class PostedID
        {

            public string uid_aadgroup { get; set; }
        }
        
    }
}
