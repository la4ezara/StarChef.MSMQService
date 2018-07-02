#region usings

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using Fourth.Starchef.Packages.Data;
using Fourth.Starchef.Packages.Model;

#endregion

namespace Fourth.Starchef.Packages.DataService
{
    public class PackageService
    {
        public PackageService()
        {
            Package = new Package();
        }

        public Package Package { get; private set; }
        public Config Config { get; set; }
        public int PackageId { get; set; }

        public PackageService Load()
        {
            using (Dal dal = new Dal(Config.ConnString))
            {
                IDataParameter[] param =
                {
                    dal.GetParameter("@package_id", PackageId),
                    dal.GetParameter("@user_id", Config.UserId)
                };

                using (IDataReader dr = dal.GetReader("sc_package_load", param, CommandType.StoredProcedure))
                {
                    if (dr.Read())
                    {
                        Package.Id = dr.GetDrValue<int>(0);
                        Package.Name = dr.GetDrValue<string>(1);
                        Package.Permission = (Permission) dr.GetByte(2);
                        Package.Pagination = (Pagination) dr.GetByte(3);
                        Package.IncludeToc = dr.GetDrValue<bool>(4);
                        Package.PageSetting = SetPageSetting(dr);
                        Package.FontName = dr.GetDrValue<string>(10);

                        GroupLogo groupLogo = new GroupLogo
                        {
                            Path = dr.GetDrValue<string>(11),
                            Width = Convert.ToDouble(dr.GetDrValue<int>(12)),
                            Height = Convert.ToDouble(dr.GetDrValue<int>(13))
                        };

                        Package.GroupLogo = groupLogo;
                        Package.LineSeparator = new GroupLogo();
                        Package.CompanyName = dr.GetDrValue<string>(14);
                        Package.CopyrightText = dr.GetDrValue<string>(15);
                    }

                    dr.NextResult();
                    if (dr.Read())
                    {
                        Package.Header = AddHeaderFooter(dr);
                    }

                    dr.NextResult();
                    if (dr.Read())
                    {
                        Package.Footer = AddHeaderFooter(dr);
                    }
                }
            }
            return this;
        }

        private HeaderFooter AddHeaderFooter(IDataReader dr)
        {
            return new HeaderFooter
            {
                Left = new HeaderFooterItem
                {
                    HeaderFooterType = GetFooterTypeValue(dr.GetDrValue<string>(0)),
                    Text = dr.GetDrValue<string>(1)
                },
                Middle = new HeaderFooterItem
                {
                    HeaderFooterType = GetFooterTypeValue(dr.GetDrValue<string>(2)),
                    Text = dr.GetDrValue<string>(3)
                },
                Right = new HeaderFooterItem
                {
                    HeaderFooterType = GetFooterTypeValue(dr.GetDrValue<string>(4)),
                    Text = dr.GetDrValue<string>(5)
                }
            };
        }

        private HeaderFooterType GetFooterTypeValue(string itemCode)
        {
            HeaderFooterType headerFooterType;
            return Enum.TryParse(itemCode, true, out headerFooterType) ? headerFooterType : HeaderFooterType.Blank;
        }

        
        private PageSetting SetPageSetting(IDataReader dr)
        {            
            var pageSetting = new PageSetting();
            Paper paperSize;

            // read from DB and set PageSetting for Package
            pageSetting.Paper = Enum.TryParse(dr.GetDrValue<string>(5), true, out paperSize) ? paperSize : Paper.A4;
            pageSetting.Margin = new Margin
            {
                Left = (double)dr.GetDrValue<decimal>(6),
                Right = (double)dr.GetDrValue<decimal>(7),
                Top = (double)dr.GetDrValue<decimal>(8),
                Bottom = (double)dr.GetDrValue<decimal>(9)
            };

            // if any of margins (Top, Bottom, Left, Right) is 0 we should take value from config
            if (pageSetting.Margin.Left == 0)
            {
                pageSetting.Margin.Left = Math.Max(Config.HeaderFooterConfig.HeaderMargin.Left,Config.HeaderFooterConfig.FooterMargin.Left);
            }

            if (pageSetting.Margin.Right == 0)
            {
                pageSetting.Margin.Right = Math.Max(Config.HeaderFooterConfig.HeaderMargin.Right, Config.HeaderFooterConfig.FooterMargin.Right); 
            }

            if (pageSetting.Margin.Top == 0)
            {
                pageSetting.Margin.Top = Config.HeaderFooterConfig.HeaderMargin.Top;
            }

            if (pageSetting.Margin.Bottom == 0)
            {
                pageSetting.Margin.Bottom = Config.HeaderFooterConfig.FooterMargin.Bottom;
            }            

            return pageSetting; 
        }

        public PackageService WithSections()
        {
            ICollection<Section> sections = new Collection<Section>();
            using (Dal dal = new Dal(Config.ConnString))
            {
                IDataParameter[] param =
                {
                    dal.GetParameter("@package_id", Package.Id)
                };

                using (IDataReader dr = dal.GetReader("sc_package_load_sections", param, CommandType.StoredProcedure))
                {
                    while (dr.Read())
                    {
                        Section section = new Section();
                        section.Id = dr.GetDrValue<int>(0);
                        section.Name = dr.GetDrValue<string>(1);
                        section.Order = dr.GetDrValue<short>(2);
                        sections.Add(section);
                    }
                }
            }

            Package.Sections = sections;
            return this;
        }

        public PackageService WithDocumentItems()
        {
            foreach (Section section in Package.Sections)
            {
                using (Dal dal = new Dal(Config.ConnString))
                {
                    IDataParameter[] param =
                    {
                        dal.GetParameter("@section_id", section.Id)
                    };

                    using (
                        IDataReader dr = dal.GetReader("sc_package_load_documents", param, CommandType.StoredProcedure))
                    {
                        while (dr.Read())
                        {
                            Item item = new Item();

                            item.ItemType = ItemType.AuxiliaryDocument;
                            item.Name = dr.GetDrValue<string>(0);
                            item.Order = dr.GetDrValue<short>(1);

                            AuxDocument auxDocument = new AuxDocument();
                            auxDocument.Id = dr.GetDrValue<int>(2);
                            auxDocument.DocumentType = MapDocumentType.Of(dr.GetDrValue<string>(3).Trim());
                            auxDocument.SourceFile = dr.GetDrValue<string>(4);
                            auxDocument.ConvertedFile = dr.GetDrValue<string>(5);
                            auxDocument.ConversionRequired = dr.GetDrValue<bool>(6);

                            item.AuxiliaryDocument = auxDocument;

                            section.Items.Add(item);
                        }
                    }
                }
            }
            return this;
        }

        public int ProcessStartedLog(string logText, int userId)
        {
            using (Dal dal = new Dal(Config.ConnString))
            {
                IDataParameter[] param =
                {
                    dal.GetParameter("@package_id", PackageId),
                    dal.GetParameter("@log_entry",logText),
                    dal.GetParameter("@user_id",userId)
                };

                return dal.ExecuteScalar<int>("sc_package_add_generate_log", param, CommandType.StoredProcedure);
            }
        }

        public void UpdateProcessLog(int logId,string logText)
        {
            using (Dal dal = new Dal(Config.ConnString))
            {
                IDataParameter[] param =
                {
                    dal.GetParameter("@log_id", logId),
                    dal.GetParameter("@log_entry",logText),
                };
                dal.ExecuteSql("sc_package_update_generate_log", param, CommandType.StoredProcedure);
            }
        }

        public void ProcessFailedLog(int logId, string logText,string exception)
        {
            using (Dal dal = new Dal(Config.ConnString))
            {
                IDataParameter[] param =
                {
                    dal.GetParameter("@log_id", logId),
                    dal.GetParameter("@log_entry",logText),
                    dal.GetParameter("@completed_processing",true),
                    dal.GetParameter("@generated_successfully",false),
                    dal.GetParameter("@exception",exception)
                };
                dal.ExecuteSql("sc_package_update_generate_log", param, CommandType.StoredProcedure);
            }
        }
        
        public PackageService WithReportItems()
        {
            foreach (Section section in Package.Sections)
            {
                using (Dal dal = new Dal(Config.ConnString))
                {
                    IDataParameter[] param =
                    {
                        dal.GetParameter("@section_id", section.Id)
                    };

                    using (
                        IDataReader dr = dal.GetReader("sc_package_load_reports", param, CommandType.StoredProcedure))
                    {
                        while (dr.Read())
                        {
                            Item item = new Item();

                            item.ItemType = ItemType.Report;
                            item.Name = dr.GetDrValue<string>(0);
                            item.Order = dr.GetDrValue<short>(1);

                            ReportFilter reportFilter = new ReportFilter();
                            reportFilter.ReportId = dr.GetDrValue<int>(2);
                            reportFilter.GroupFilterId = dr.GetDrValue<int>(3);
                            reportFilter.GroupFilterType = dr.GetDrValue<int>(4);
                            reportFilter.FilterType = dr.GetDrValue<int>(5);
                            reportFilter.Filters = dr.GetDrValue<string>(6);
                            reportFilter.StartDate = dr.GetDrValue<string>(7);
                            reportFilter.EndDate = dr.GetDrValue<string>(8);
                            reportFilter.ReportingEngine = (ReportingEngine) dr.GetDrValue<byte>(9);
                            item.ReportFilter = reportFilter;

                            section.Items.Add(item);
                        }
                    }
                }
            }
            return this;
        }

        
    }
}