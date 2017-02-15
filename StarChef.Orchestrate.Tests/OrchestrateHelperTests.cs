using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using CategoryExportType = Fourth.Orchestration.Model.Menus.Events.CategoryExportType;

namespace StarChef.Orchestrate.Tests
{
    public class OrchestrateHelperTests
    {
        [Theory]
        [InlineData("NONE", CategoryExportType.NONE)]
        [InlineData("ING", CategoryExportType.ING)]
        [InlineData("INGFOOD", CategoryExportType.INGFOOD)]
        [InlineData("RECIPE", CategoryExportType.RECIPE)]
        public void MapCategoryExportType_should_parse_stringName_toEnum(string value, CategoryExportType expected)
        {
            var actual = OrchestrateHelper.MapCategoryExportType(value);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("0", CategoryExportType.NONE)]
        [InlineData("1", CategoryExportType.ING)]
        [InlineData("2", CategoryExportType.INGFOOD)]
        [InlineData("3", CategoryExportType.RECIPE)]
        public void MapCategoryExportType_should_parse_stringInt_toEnum(string value, CategoryExportType expected)
        {
            var actual = OrchestrateHelper.MapCategoryExportType(value);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("NOT_EXISTING", CategoryExportType.NONE)]
        [InlineData("999", CategoryExportType.NONE)]
        [InlineData("", CategoryExportType.NONE)]
        public void MapCategoryExportType_should_return_None_for_notExisting_values(string value, CategoryExportType expected)
        {
            var actual = OrchestrateHelper.MapCategoryExportType(value);
            Assert.Equal(expected, actual);
        }
    }
}
