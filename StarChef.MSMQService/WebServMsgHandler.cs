using System;
using System.Xml;
using System.Messaging;
using System.Data;
using System.Data.SqlClient;
using log4net;

namespace StarChef.MSMQService
{
	/// <summary>
	/// Handles Web Service Messages found on the queue and passed in by the listener.
	/// </summary>
	public class WebServMsgHandler
	{
        private static readonly ILog Logger =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public WebServMsgHandler()
		{
			//
			// TODO: Add constructor logic here
			//
		}
		public static bool HandleWebServiceQueryMessage(System.Messaging.Message msg)
		{
			string RequestGuid = string.Empty;
			string sp_name_and_paramsXML = string.Empty;
			string UserDSN = string.Empty;

			WebServiceQueryMessage x = new WebServiceQueryMessage();
			XmlMessageFormatter xformat = new XmlMessageFormatter(new Type[]{x.GetType()});
			msg.Formatter = xformat;

			try
			{
				// Cast the incoming message to get its data
				WebServiceQueryMessage mmm = (WebServiceQueryMessage) msg.Body;
				// Extract the Guid
				RequestGuid = mmm.RequestGuid;
				// And the SP and Parms used, for error handling later
				sp_name_and_paramsXML = mmm.sp_name_and_paramsXML;
				// and the DSN to use
				UserDSN = mmm.DSN;
				// Send the query to the database
				SqlParameter[] parms = new SqlParameter[1];
				parms[0] = new SqlParameter("@request_guid", RequestGuid);
				// The scweb_exec_query procedure may raise an error before finishing
				//  so we have to handle this in the catch section below
				ExecuteQueryStoredProc(UserDSN, "scweb_exec_query", parms);
			}
			catch (SqlException sqlex)
			{
				TidyRequest(sqlex.Number, sqlex.Message, RequestGuid, sp_name_and_paramsXML, UserDSN);
				return false;
			}
			catch (Exception nonsqlex)
			{
				// A non-SqlException was thrown.
				// This means that the request was never logged as started,
				//  but it still needs to be tidied up
				TidyRequest(-1, nonsqlex.Message, RequestGuid, sp_name_and_paramsXML, UserDSN);
				Logger.Error(nonsqlex);
				return false;
			}
            return true;
		}

		public static bool HandleWebServiceCostUpdateMessage(System.Messaging.Message msg)
		{
			string RequestGuid = string.Empty;
			string costUpdateXml = string.Empty;
			string UserDSN = string.Empty;

			WebServiceQueryMessage x = new WebServiceQueryMessage();
			XmlMessageFormatter xformat = new XmlMessageFormatter(new Type[]{x.GetType()});
			msg.Formatter = xformat;

			try
			{
				// Cast the incoming message to get its data
				WebServiceQueryMessage mmm = (WebServiceQueryMessage) msg.Body;
				// Extract the Guid
				RequestGuid = mmm.RequestGuid;
				// And the cost update data
				costUpdateXml = mmm.sp_name_and_paramsXML;
				// and the DSN to use
				UserDSN = mmm.DSN;
				// Send the query to the database
				SqlParameter[] parms = new SqlParameter[2];
				parms[0] = new SqlParameter("@request_guid", RequestGuid);
				parms[1] = new SqlParameter("@xml_data", costUpdateXml);
				// The scweb_exec_query procedure may raise an error before finishing
				//  so we have to handle this in the catch section below
				ExecuteCostUpdateStoredProc(UserDSN, "scweb_exec_cost_update", parms);
			}
			catch (SqlException sqlex)
			{
				TidyRequest(sqlex.Number, sqlex.Message, RequestGuid, costUpdateXml, UserDSN);
				return false;
			}
			catch (Exception nonsqlex)
			{
				// A non-SqlException was thrown.
				// This means that the request was never logged as started,
				//  but it still needs to be tidied up
				TidyRequest(-1, nonsqlex.Message, RequestGuid, costUpdateXml, UserDSN);
				Logger.Error(nonsqlex);
				return false;
			}
			return true;
		}
		private static void TidyRequest(int exnumber, string exmessage, string RequestGuid, string sp_name_and_paramsXML, string DSN)
		{
			try
			{
				// The scweb_exec_query procedure raised an SqlException and therefore we need to log that on the database
				//  using scweb_log_request_error so that the request is tidied up
				SqlParameter[] parms = new SqlParameter[4];
				parms[0] = new SqlParameter("@request_guid", RequestGuid);
				parms[1] = new SqlParameter("@sp_name_and_paramsXML", sp_name_and_paramsXML);
				parms[2] = new SqlParameter("@err", exnumber);
				parms[3] = new SqlParameter("@errmsg", exmessage);
				ExecuteStoredProc(DSN, "scweb_log_request_error", parms);
			}
			catch (Exception anotherex)
			{
                Logger.Error(anotherex);
			}
			return;
		}

		private static int ExecuteStoredProc(string connectionString, string spName, params SqlParameter[] parameterValues)
		{
			//create & open a SqlConnection, and dispose of it after we are done.
			using (SqlConnection cn = new SqlConnection(connectionString))
			{
				cn.Open();

				SqlCommand cmd = new SqlCommand(spName, cn);
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.CommandTimeout = 60;

				// add params
				foreach (SqlParameter param in parameterValues)
					cmd.Parameters.Add(param);

				// run proc
				int retval = cmd.ExecuteNonQuery();

				return retval;
			}
		}
		private static int ExecuteQueryStoredProc(string connectionString, string spName, params SqlParameter[] parameterValues)
		{
			//create & open a SqlConnection, and dispose of it after we are done.
			using (SqlConnection cn = new SqlConnection(connectionString))
			{
				cn.Open();

				SqlCommand cmd = new SqlCommand(spName, cn);
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.CommandTimeout = 60;

				// add params
				foreach (SqlParameter param in parameterValues)
					cmd.Parameters.Add(param);

				// run proc
				SqlDataReader dr = cmd.ExecuteReader(System.Data.CommandBehavior.SingleResult);

				// Receive the data back, building it into a single string
				string sResult = string.Empty;
				while (dr.Read())
					sResult += dr[0];

				// tidy up
				cmd.Parameters.Clear();
				cmd.Dispose();
				dr.Close();
				cn.Close();

				// reopen connection to store results on database
				cn.Open();
				cmd = new SqlCommand("scweb_request_store_data", cn);
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.CommandTimeout = 60;

				// add params
				cmd.Parameters.Clear();
				cmd.Parameters.Add(new SqlParameter("@request_guid", parameterValues[0].Value)); // request_guid
				cmd.Parameters.Add(new SqlParameter("@data", sResult)); // data

				// run proc
				int retval = cmd.ExecuteNonQuery();

				// tidy up
				dr.Close();
				cn.Close();

				return retval;
			}
			
		}
		private static int ExecuteCostUpdateStoredProc(string connectionString, string spName, params SqlParameter[] parameterValues)
		{
			//create & open a SqlConnection, and dispose of it after we are done.
			using (SqlConnection cn = new SqlConnection(connectionString))
			{
				cn.Open();

				SqlCommand cmd = new SqlCommand(spName, cn);
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.CommandTimeout=60;
				
				// add params
				foreach (SqlParameter param in parameterValues)
					cmd.Parameters.Add(param);

				// run proc
				XmlDocument xResult=new XmlDocument();
				xResult.Load(cmd.ExecuteXmlReader());

				// tidy up
				cmd.Parameters.Clear();
				cmd.Dispose();
				cn.Close();

				XmlNode AffectedProducts=xResult.GetElementsByTagName("affectedproducts")[0];

				if(Convert.ToInt32(AffectedProducts.Attributes["count"].Value)>0)
				{
					// reopen connection to calculate dish pricing
					cn.Open();
					cmd = new SqlCommand("scweb_calculate_dish_pricing", cn);
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.CommandTimeout=600;
				
					// add params and execute
					cmd.Parameters.Clear();
					cmd.Parameters.Add(new SqlParameter("@parameter_xml", AffectedProducts.OuterXml));
					cmd.ExecuteNonQuery();

					// tidy up
					cmd.Parameters.Clear();
					cmd.Dispose();
					cn.Close();
				}

				XmlNode ErrorLines=xResult.GetElementsByTagName("errorlines")[0];
				if(Convert.ToInt32(ErrorLines.Attributes["count"].Value)>0)
				{
					//We have errors
					// reopen connection to log errors
					cn.Open();
					cmd = new SqlCommand("scweb_log_update_errors", cn);
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.CommandTimeout = 60;

					// add params and execute
					cmd.Parameters.Clear();
					cmd.Parameters.Add(new SqlParameter("@request_guid", parameterValues[0].Value)); // request_guid
					cmd.Parameters.Add(new SqlParameter("@errorlinesXML", "<error number=\"51001\" description=\"Ingredients not found\">"+ErrorLines.InnerXml+"</error>"));
					cmd.Parameters.Add(new SqlParameter("@costupdatexml", parameterValues[1].Value));
					cmd.ExecuteNonQuery();

					// tidy up
					cmd.Parameters.Clear();
					cmd.Dispose();
					cn.Close();
				}

				// reopen connection to store results on database
				cn.Open();
				cmd = new SqlCommand("scweb_request_store_data", cn);
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.CommandTimeout = 60;

				// add params
				cmd.Parameters.Clear();
				cmd.Parameters.Add(new SqlParameter("@request_guid", parameterValues[0].Value)); // request_guid
				cmd.Parameters.Add(new SqlParameter("@data", AffectedProducts.OuterXml)); // data

				// run proc
				int retval = cmd.ExecuteNonQuery();

				// tidy up
				cn.Close();

				return retval;
			}
			
		}

	}
}
