using Dapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UeaseAdmin.DB;

namespace UeaseAdmin.Services
{
    public class ReportService
    {
        protected ILogger<ReportService> _logger;

        public ReportService(ILogger<ReportService> logger)
        {
            _logger = logger;
        }

        public IEnumerable<LIFF_BROWSE_LOG> BrowseLog(string lineDisplayName)
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendLine("SELECT TOP 100 * FROM LIFF_BROWSE_LOG (NOLOCK) WHERE 1=1 ");

            if (!String.IsNullOrWhiteSpace(lineDisplayName))
            {
                sql.AppendLine("AND lineDisplayName like @lineDisplayName ");
                lineDisplayName = $"%{lineDisplayName}%";
            }

            using (var conn = DBHelper.CONNDB.Open())
            {
                var dataList = conn.Query<LIFF_BROWSE_LOG>(sql.ToString(), new { lineDisplayName });
                return dataList;
            }
        }
    }

}
