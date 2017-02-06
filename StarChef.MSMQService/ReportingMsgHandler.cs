using System;
using System.Xml;
using System.Messaging;
using System.Data;
using System.Data.SqlClient;
using Microsoft.ApplicationBlocks.ExceptionManagement;

using StarChef.Data;
using StarChef.Reports2;

namespace StarChef.MSMQService
{
	/// <summary>
	/// Handles Web Service Messages found on the queue and passed in by the listener.
	/// </summary>
	public class ReportingMsgHandler
	{
		public ReportingMsgHandler()
		{
			//
			// TODO: Add constructor logic here
			//
		}
		public static bool HandleReportingMessage(System.Messaging.Message msg)
		{
			string ReportGuid = string.Empty;
			string parameterXML = string.Empty;
			string filterXML = string.Empty;
			string outputFilterXML = string.Empty;
			string UserDSN = string.Empty;
			Constants.ReportOutputType ReportFormat = Constants.ReportOutputType.NotSet;
			int groupFilterID = 0;
			Constants.GroupFilterType groupFilterType = Constants.GroupFilterType.NotSet;
			DateTime startDate = DateTime.MinValue;
			DateTime endDate = DateTime.MaxValue;
			int startDay = 0;
			int endDay = 0;
			Constants.ScopeType scopeFilterID = Constants.ScopeType.NotSet;

			ReportingMessage x = new ReportingMessage();
			XmlMessageFormatter xformat = new XmlMessageFormatter(new Type[]{x.GetType()});
			msg.Formatter = xformat;

			try
			{
				// Cast the incoming message to get its data
				ReportingMessage mmm = (ReportingMessage) msg.Body;
				// Extract the Guid
				ReportGuid = mmm.ReportGuid;
				// And the filter and Params used
				filterXML = mmm.filterXML;
				outputFilterXML = mmm.OutputFilterXML;
				parameterXML = mmm.parameterXML;
				// and the DSN to use
				UserDSN = mmm.DSN;
				//and the output format
				ReportFormat=mmm.Format;
				//and the group filter
				groupFilterID=mmm.GroupFilterID;
				//and the group filter type
				groupFilterType=mmm.GroupFilterType;
				//and the start date
				startDate=mmm.StartDate;
				//and the end date
				endDate=mmm.EndDate;
				//and the scope filter ID
				scopeFilterID=mmm.ScopeFilterID;
				//and the start day
				startDay=mmm.StartDay;
				//and the end day
				endDay=mmm.EndDay;

				SqlParameter[] parms = new SqlParameter[2];
				parms[0] = new SqlParameter("@report_guid",ReportGuid);
				parms[1] = new SqlParameter("@file_extension",SqlDbType.NVarChar,3);
				switch(ReportFormat)
				{
					case Constants.ReportOutputType.CSV:
						parms[1].Value = "csv";
						break;
					case Constants.ReportOutputType.Excel:
						parms[1].Value = "xls";
						break;
					case Constants.ReportOutputType.Html:
						parms[1].Value = "mht";
						break;
					case Constants.ReportOutputType.Pdf:
						parms[1].Value = "pdf";
						break;
					case Constants.ReportOutputType.RTF:
						parms[1].Value = "rtf";
						break;
				}
				
				string ReportFilePath = DbManager.ExecuteScalar(UserDSN, "sc_report_started",parms).ToString();

				System.IO.MemoryStream rptOutput = Report.RunReport(UserDSN, parameterXML, filterXML, outputFilterXML, ReportFormat, groupFilterID, groupFilterType, startDate, endDate, startDay, endDay, scopeFilterID);
				
				if(rptOutput!=null)
				{
					System.IO.FileStream file = new System.IO.FileStream(ReportFilePath,System.IO.FileMode.CreateNew);
					WriteStreamToStream(rptOutput,file);

					rptOutput.Close();
					file.Close();
					DbManager.ExecuteNonQuery(UserDSN, "sc_report_finished",new SqlParameter[] {new SqlParameter("@report_guid",ReportGuid)});
				}
				else
				{
					DbManager.ExecuteNonQuery(UserDSN, "sc_report_no_data",new SqlParameter[] {new SqlParameter("@report_guid",ReportGuid)});
				}
				return true;
			}
			catch (SqlException sqlex)
			{
				ExceptionManager.Publish(sqlex);
				DbManager.ExecuteNonQuery(UserDSN, "sc_report_error",new SqlParameter[] {new SqlParameter("@report_guid",ReportGuid)});
				return false;
			}
			catch (Exception nonsqlex)
			{
				ExceptionManager.Publish(nonsqlex);
				DbManager.ExecuteNonQuery(UserDSN, "sc_report_error",new SqlParameter[] {new SqlParameter("@report_guid",ReportGuid)});
				return false;
			}
		}

		private static void WriteStreamToStream(System.IO.Stream sourceStream, System.IO.Stream targetStream)
		{
			//What's the size of the source stream:
			int size  = (int)sourceStream.Length;
			//Create a buffer that same size:
			byte[] buffer=new byte[size];
			//Move the source stream to the beginning:
			sourceStream.Seek(0, System.IO.SeekOrigin.Begin);
			//copy the whole sourceStream into our buffer:
			sourceStream.Read(buffer, 0, size);
			//Write out buffer to the target stream:
			targetStream.Write(buffer, 0, size);
		}
	}
}
