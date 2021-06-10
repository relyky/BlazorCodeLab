using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UeaseMan.Server.DB;
using UeaseMan.Shared;
using UeaseMan.Shared.ReportModel;
using Dapper;
using Dapper.Contrib.Extensions;
using System.Text;

namespace UeaseMan.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportController : ControllerBase
    {
        private readonly ILogger<ReportController> _logger;

        public ReportController(ILogger<ReportController> logger)
        {
            _logger = logger;
        }

        [HttpPost("[action]")]
        public IEnumerable<LIFF_BROWSE_LOG> BrowseLog(BrowseLogArgs args)
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendLine("SELECT TOP 100 * FROM LIFF_BROWSE_LOG (NOLOCK) WHERE 1=1 ");

            if (!String.IsNullOrWhiteSpace(args.lineDisplayName))
            {
                sql.AppendLine("AND lineDisplayName like @lineDisplayName ");
                args.lineDisplayName = $"%{args.lineDisplayName}%";
            }

            using (var conn = DBHelper.CONNDB.Open())
            {
                var dataList = conn.Query<LIFF_BROWSE_LOG>(sql.ToString(), args);
                return dataList;
            }
        }

    }
}
