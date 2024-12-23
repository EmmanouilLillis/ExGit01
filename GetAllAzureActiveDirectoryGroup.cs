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
using System.Diagnostics;

namespace QBM.CompositionApi
{
    public class GetAllAzureActiveDirectoryGroup : IApiProviderFor<QER.CompositionApi.Portal.PortalApiProject>, IApiProvider
    {
        public void Build(IApiBuilder builder)
        {
            builder.AddMethod(Method.Define("exercise/GetAllAzureGroups")
                .Handle<PostedID, List<ReturnedAAD>>("POST", async (posted, qr, ct) =>
                {
                    string userInserted = posted.UserInserted;
                    string userUpdated = posted.UserUpdated;

                    var query = Query.From("AADGroup").Select("*");

                    if (!string.IsNullOrWhiteSpace(userInserted) && !string.IsNullOrWhiteSpace(userUpdated))
                    {
                        query = query.Where(string.Format("XUserInserted = '{0}' AND XUserUpdated = '{1}'", userInserted , userUpdated));
                    }
                    else if (!string.IsNullOrWhiteSpace(userInserted))
                    {
                        query = query.Where(string.Format("XUserInserted = '{0}' ", userInserted));
                    }
                    else if (!string.IsNullOrWhiteSpace(userUpdated))
                    {
                        query = query.Where(string.Format("XUserUpdated = '{0}'", userUpdated));
                    }
                    else
                    {
                        return new List<ReturnedAAD> {
                            new ReturnedAAD
                            {
                                message = "Empty Fields or white space"
                            }
                        };
                    }



                    //we use getcollection for only one query
                    var getCollection = await qr.Session.Source()
                        .GetCollectionAsync(query , EntityCollectionLoadType.Default).ConfigureAwait(false);



                    if (getCollection == null || !getCollection.Any())// check if null or empty
                    {

                        return new List<ReturnedAAD> {
                            new ReturnedAAD
                            {
                                message = "No groups Found!"
                            }
                        };
                    }
                    else 
                    { 
                        var results = new List<ReturnedAAD>();
                        foreach(IEntity row in getCollection)
                        {
                            var returnedRow = await ReturnedAAD.fromEntity(row ,qr.Session ).ConfigureAwait(false);
                            results.Add(returnedRow);
                        }

                        return results;
                    }


                    //TryGetCollectionsAsync
                    /*var collectionQuery = new CollectionQuery(query , EntityCollectionLoadType.Default);


                    IReadOnlyList<CollectionQuery> collectionQueries = new List<CollectionQuery>();
                    System.Diagnostics.Debug.WriteLine($"About to fetch collection for query: {query}");


                    var tryGetCollection = await qr.Session.Source()
                        .TryGetCollectionsAsync(collectionQueries)
                        .ConfigureAwait(false);
                    System.Diagnostics.Debug.WriteLine($"Collection retrieved, item count: {tryGetCollection?.Count() ?? 0}");


                    if (tryGetCollection == null)
                    {
                        System.Diagnostics.Debug.WriteLine("No collection returned (null)");
                    }
                    else if (!tryGetCollection.Any())
                    {
                        System.Diagnostics.Debug.WriteLine("Collection returned but is empty");
                    }


                    if (tryGetCollection != null )
                    {
                        var entities = tryGetCollection;

                        // Convert the entities to ReturnedAAD objects
                        var resultList = new List<ReturnedAAD>();

                        foreach (IEntity entity in entities)  // Iterating over the IEntity collection
                        {
                            var returnedEntity = await ReturnedAAD.fromEntity(entity, qr.Session).ConfigureAwait(false);
                            resultList.Add(returnedEntity);
                        }


                        return resultList;
                    }
                    else
                    {
                        // Return an empty list with a failure message
                        return new List<ReturnedAAD>
                        {
                            new ReturnedAAD { message = "No groups found for the provided description." }
                        };
                    }*/



                }));
        }

        public class PostedID
        {
            public string UserInserted { get; set; }

            public string UserUpdated { get; set; }
        }


        // The ReturnedAAD class represents the structure of the data we want to return 
        public class ReturnedAAD
        {
            public string DisplayName { get; set; }
            public string UID_AADGroup { get; set; }
            public string MailNickName { get; set; }
            public string Description { get; set; }
            public string UID_AADOrganization { get; set; }
            public string userInserted {  get; set; }

            public string UserUpdated { get; set; }

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
                    userInserted = await entity.GetValueAsync<string>("XUserInserted").ConfigureAwait(false),
                    UserUpdated = await entity.GetValueAsync<string>("XUserUpdated").ConfigureAwait(false),
                };

                // Return the populated ReturnedAAD object
                return g;
            }
        }
    }
}

