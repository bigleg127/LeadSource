using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System.Threading.Tasks;

namespace LeadSource
{
    public class LeadSource:IPlugin
    {

        public void Execute(IServiceProvider service)
        {
            var tracingService = (ITracingService)service.GetService(typeof(ITracingService));
            var context = (IPluginExecutionContext)service.GetService(typeof(IPluginExecutionContext));
            var serviceFactory = (IOrganizationServiceFactory)service.GetService(typeof(IOrganizationServiceFactory));
            var orgservice = serviceFactory.CreateOrganizationService(context.UserId);

            if (!context.InputParameters.Contains("Target") || !(context.InputParameters["Target"] is Entity)) return;
            var lead = (Entity)context.InputParameters["Target"];

            if (tracingService == null) return;
            if (lead.LogicalName != "lead") return;
            if (lead.Contains("subject")) return;
            var leadsourceid = new EntityReference("ergo_leadsource", "ergo_name", "BAU");
            
            if(!leadSourceExists(lead.Attributes["subject"].ToString(), orgservice))
            {
                
                var leadSource = new Entity("ergo_leadsourcesubcategory");
                leadSource.Attributes["ergo_name"] = lead.Attributes["subject"].ToString();
                leadSource.Attributes["ergo_leadsourceid"] = leadsourceid;
                orgservice.Create(leadSource);
            }

            var leadsourcesubcategoryid = new EntityReference("ergo_leadsourcesubcategory", "ergo_name", lead.Attributes["subject"].ToString());

            lead.Attributes["ergo_leadsourceid"] = leadsourceid;
            lead.Attributes["ergo_leadsourcesubcategoryid"] = leadsourcesubcategoryid;

            orgservice.Update(lead);
        }

        private static bool leadSourceExists(string topic, IOrganizationService orgservice )
        {
            QueryExpression query = new QueryExpression("ergo_leadsourcesubcategory");
            query.Criteria.AddCondition("ergo_name", ConditionOperator.Equal, topic);
            var resultset = orgservice.RetrieveMultiple(query);

            if (resultset.Entities.Any()) return true;
            else return false;
            
        }

    }
}
