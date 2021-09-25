using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginHomeWork2
{
	public class PluginTest : IPlugin
	{
		public void Execute(IServiceProvider serviceProvider)
		{
			IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

			if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity && ((Entity)context.InputParameters["Target"]).LogicalName.Equals(el_potentialstudent.EntityLogicalName))
			{
				el_potentialstudent student = ((Entity)context.InputParameters["Target"]).ToEntity<el_potentialstudent>(); ;

				EntityReference studentClassReference = student.el_PotentialStudentClass;

				IOrganizationServiceFactory organizationServiceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
				IOrganizationService organizationService = organizationServiceFactory.CreateOrganizationService(context.UserId);

				el_class studentClass = (organizationService.Retrieve(el_class.EntityLogicalName, studentClassReference.Id, new ColumnSet(new string[] { "owninguser" }))).ToEntity<el_class>();
				EntityReference ownerReference = studentClass.OwningUser;

                //SystemUser owner = (organizationService.Retrieve(SystemUser.EntityLogicalName, ownerReference.Id, new ColumnSet(new string[] { "internalemailaddress" }))).ToEntity<SystemUser>();

                EntityReference studentOwnerReference = student.OwningUser;

                sendEmail(studentOwnerReference.Id, ownerReference.Id, "Message from plugin", organizationService);
            }
		}

        private void sendEmail(Guid From, Guid To, string Subject, IOrganizationService organizationService)

        {

            var description = new StringBuilder();

            description.AppendLine("Sent from plugin");

            try

            {

                // Create 'From' activity party for the email

                ActivityParty fromParty = new ActivityParty

                {

                    PartyId = new EntityReference(SystemUser.EntityLogicalName, From)

                };

                // Create 'To' activity party for the email

                ActivityParty toParty = new ActivityParty

                {

                    PartyId = new EntityReference(SystemUser.EntityLogicalName, To)

                };

                // Create an e-mail message

                Email email = new Email

                {

                    To = new ActivityParty[] { toParty },

                    From = new ActivityParty[] { fromParty },

                    Subject = Subject,

                    Description = description.ToString(),

                    DirectionCode = true

                };

                Guid _emailId = organizationService.Create(email);

                SendEmailRequest sendEmailreq = new SendEmailRequest

                {

                    EmailId = _emailId,

                    TrackingToken = "",

                    IssueSend = true

                };

                SendEmailResponse sendEmailresp = (SendEmailResponse)organizationService.Execute(sendEmailreq);

                if (sendEmailresp != null)

                {

                    //Console.WriteLine("Email record created successfully");

                    //Console.ReadKey();

                }

            }

            catch (Exception ex)

            {

                //Console.Write("Errorex.Message);

                //Console.ReadKey();

                throw new InvalidPluginExecutionException(ex.Message);

            }

        }
    }
}
