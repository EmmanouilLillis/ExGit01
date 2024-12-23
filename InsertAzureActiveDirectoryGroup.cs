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
using System.Xml.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.Remoting.Messaging;

namespace QBM.CompositionApi
{
    public class InsertAzureActiveDirectoryGroup : IApiProviderFor<QER.CompositionApi.Portal.PortalApiProject>, IApiProvider
    {
        public void Build(IApiBuilder builder)
        {
            builder.AddMethod(Method.Define("exercise/InsertAzureGroups")
                .Handle<PostedID, string  >("POST", async (posted, qr, ct) =>
                {
                    // Variables to hold column data from the posted request
                    string displayName  = "";
                    string mailNickName = "";
                    string description = "";
                    string uid_aadorgan = "";

                    // Loop through each column in the posted data to extract values
                    foreach (var column in posted.columns)
                    {
                        // Check each column name and assign its value to the corresponding variable
                        if (column.column == "DisplayName")
                        {
                            displayName = column.value;
                        }

                        if (column.column == "MailNickName")
                        {
                            mailNickName = column.value;
                        }

                        if (column.column == "Description")
                        {
                            description = column.value;
                        }
                        if (column.column == "UID_AADOrganization")
                        {
                            uid_aadorgan = column.value;
                        }
                    }
                    // Create a new 'AADGroup' entity
                    var newID = await qr.Session.Source().CreateNewAsync("AADGroup",
                        new EntityParameters
                        {
                            CreationType = EntityCreationType.DelayedLogic
                        }, ct).ConfigureAwait(false);

                    // Set the values for the new 'AADGroup' entity
                    if (displayName.StartsWith("aad"))
                    {
                        await newID.PutValueAsync("DisplayName", displayName, ct).ConfigureAwait(false);
                        await newID.PutValueAsync("MailNickName", mailNickName, ct).ConfigureAwait(false);
                        await newID.PutValueAsync("Description", description, ct).ConfigureAwait(false);
                        await newID.PutValueAsync("UID_AADOrganization", uid_aadorgan, ct).ConfigureAwait(false);


                        // Start Unit of Work to save the new entity to the database

                        using (var u = qr.Session.StartUnitOfWork())
                        {
                            await u.PutAsync(newID, ct).ConfigureAwait(false);  
                            await u.CommitAsync(ct).ConfigureAwait(false);  
                        }
                        
                        return "the AAD Groups is created";
                    }
                    else
                    {
                        return "You have given wrong format of name , try again";
                    }

                }));
        }

        
        public class PostedID
        {
            public columnsarray[] columns { get; set; }  
        }

        
        public class columnsarray
        {
            public string column { get; set; }  
            public string value { get; set; }   
        }
    }
}
