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
using VI.DB.DataAccess;
using VI.DB.Sync;
using System.Runtime.ConstrainedExecution;

namespace QBM.CompositionApi
{
    public class PostPredefinedSQL : IApiProviderFor<QER.CompositionApi.Portal.PortalApiProject> 
    {
        public void Build(IApiBuilder builder)
        {
            builder.AddMethod(Method.Define("exercise/PredefineSQL")
                  .Handle<PostedSQL, List<PersonData>>("POST", async (posted, qr, ct) =>
                  {
                      var strUID_Person = qr.Session.User().Uid;

                      // Initialize a list to hold the results (person data)
                      var results = new List<PersonData>();

                      // Resolve IStatementRunner to execute SQL queries
                      var runner = qr.Session.Resolve<IStatementRunner>();

                      // Execute the predefined SQL query with parameters
                      using (var reader = runner.SqlExecute(posted.IdentQBMLimitedSQL, new[]
                      {
                          QueryParameter.Create("uidDepartment", posted.UidDepartment),
                          QueryParameter.Create("uidPersonHead", posted.UidPersonHead),
                          QueryParameter.Create("userinserted", posted.XuserInserted)
                      }))
                      {
                          while (reader.Read())
                          {
                              var personData = new PersonData();

                              // Loop through all columns in the row
                              for (int i = 0; i < reader.FieldCount; i++)
                              {
                                  var columnName = reader.GetName(i);  // Get column name dynamically
                                  var columnValue = reader.IsDBNull(i) ? null : reader.GetValue(i).ToString();

                                  // You can add the column name and value to a dictionary, or assign specific values to your object
                                  if (columnName == "InternalName")
                                  {
                                      personData.InternalName = columnValue; 
                                  }
                              }


                              // Add the person data to the result list
                              results.Add(personData);
                          }
                      }
                      // check if results found 
                      if (results.Count > 0)
                      {
                          return results;
                      }
                      else
                      {
                          return new List<PersonData>
                          {
                            new PersonData
                            {
                                Message = "No data found for the provided parameters."
                            }
                          };
                      }
                  }
            ));
        }

        public class PersonData
        {
            public string InternalName { get; set; }

            public string Message { get; set; }
        }
        public class PostedSQL
        {
            // The identifier of the predefined SQL statement to execute
            public string IdentQBMLimitedSQL { get; set; }

            public string UidDepartment { get; set; }
            public string UidPersonHead { get; set; }
            public string XuserInserted { get; set; }

        }
    }
}
