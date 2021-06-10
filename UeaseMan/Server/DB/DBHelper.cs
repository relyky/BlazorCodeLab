using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using Dapper;
using Dapper.Contrib.Extensions;

namespace UeaseMan.Server.DB
{
    /// <summary>
    /// 一個資料庫一個。
    /// </summary>
    static class DBHelper
    {
        public static ConnProxy CONNDB;

        #region To write exception log

        ///// <summary>
        ///// [特殊函式] 寫ExpLog。
        ///// </summary>
        //public static void ExpLog(string className, string methodName, string errMsg)
        //{
        //    //※ 特殊函式，寫Log的交易完全獨立。
        //    try
        //    {
        //        // 一開始就需知道寫入的目標資料庫
        //        using (var conn = CONNDB.Open())
        //        using (var txn = conn.BeginTransaction(System.Data.IsolationLevel.ReadUncommitted)) // WITH NOLOCK
        //        {
        //            conn.Execute("INSERT INTO L_EXP(ClassName, MethodName, ErrMsg, ErrDbCommand, UDate, MailFlag) VALUES(@className, @methodName, @errMsg, NULL, GetDate(), 'N'); "
        //                , new { className, methodName, errMsg }
        //                , txn);
        //            txn.Commit();
        //        }
        //    }
        //    catch
        //    {
        //        Debug.WriteLine("DBHelper.ExpLog 掛了！這事應該不會發生除非DB連線失敗。");
        //        Debugger.Break();
        //    }
        //}

        //public static void ExpLog(object caller, string message,
        //    [CallerMemberName] string memberName = "",
        //    [CallerFilePath] string sourceFilePath = "",
        //    [CallerLineNumber] int sourceLineNumber = 0)
        //{
        //    string className = caller.GetType().Name;
        //    string traceMsg = message + $"\r\n{sourceFilePath}({sourceLineNumber}行)";
        //    ExpLog(className, memberName, traceMsg);
        //}

        //public static void ExpLog(object caller, string message, Exception ex,
        //    [CallerMemberName] string memberName = "",
        //    [CallerFilePath] string sourceFilePath = "",
        //    [CallerLineNumber] int sourceLineNumber = 0)
        //{
        //    string className = caller.GetType().Name;
        //    string traceMsg = message + $"\r\n{sourceFilePath}({sourceLineNumber}行)" + $"\r\n{ex}";
        //    ExpLog(className, memberName, traceMsg);
        //}

        #endregion
    }

    class ConnProxy
    {
        /// <summary>
        /// 取連線字串
        /// </summary>
        public String ConnStr
        {
            get => AsString(_connStr);
        }

        /// <summary>
        /// 連線字串，平常保持在加密狀態。
        /// </summary>
        private System.Security.SecureString _connStr;

        public ConnProxy(string connString)
        {
            /// 連線字串只有建構時可設定。
            _connStr = AsSecureString(connString);
        }

        public SqlConnection Open()
        {
            SqlConnection conn = new SqlConnection(ConnStr);
            conn.Open();
            Debug.WriteLine("Connection Open.");
            return conn;
        }

        private static System.Security.SecureString AsSecureString(String str)
        {
            System.Security.SecureString secstr = new System.Security.SecureString();
            str.ToList().ForEach(secstr.AppendChar);
            secstr.MakeReadOnly();
            return secstr;
        }

        private static String AsString(System.Security.SecureString secstr)
        {
            IntPtr valuePtr = IntPtr.Zero;
            try
            {
                valuePtr = System.Runtime.InteropServices.Marshal.SecureStringToGlobalAllocUnicode(secstr);
                return System.Runtime.InteropServices.Marshal.PtrToStringUni(valuePtr);
            }
            catch
            {
                return null;
            }
            finally
            {
                System.Runtime.InteropServices.Marshal.ZeroFreeGlobalAllocUnicode(valuePtr);
            }
        }

    }

    public static class ThisProjectClassExtensions
    {
        /// <summary>
        /// cast as Dapper CommandDefinition
        /// </summary>
        public static CommandDefinition AsDapperCommand(this SqlCommand sql)
        {
            if (sql.Parameters.Count > 0)
            {
                DynamicParameters args = new DynamicParameters();
                foreach (SqlParameter p in sql.Parameters)
                {
                    args.Add(p.ParameterName, p.Value);
                }

                return new CommandDefinition(sql.CommandText, args);
            }
            else
            {
                return new CommandDefinition(sql.CommandText);
            }
        }

        /// <summary>
        /// 取代 Dapper.Contrib 之 Get 指令無法多 p-key 取值的狀況
        /// </summary>
        public static TTable GetEx<TTable>(this SqlConnection conn, object args, SqlTransaction txn = null)
        {
            // 依 Property 動態加入 P-Key 查詢條件
            List<String> conds = new List<string>();
            foreach (PropertyInfo pi in args.GetType().GetProperties())
            {
                conds.Add($"{pi.Name} = @{pi.Name} ");
            }

            String tableName = typeof(TTable).Name;
            StringBuilder sql = new StringBuilder($@"SELECT TOP 1 * FROM {tableName} WHERE {String.Join("AND ", conds)}; ");
            var info = conn.Query<TTable>(sql.ToString(), args, txn).FirstOrDefault();
            return info;
        }

        /// <summary>
        /// 載入多筆資料。與GetEx相比可以取回多筆資料。
        /// </summary>
        public static IEnumerable<TTable> LoadEx<TTable>(this SqlConnection conn, object args, SqlTransaction txn = null)
        {
            // 依 Property 動態加入 P-Key 查詢條件
            List<String> conds = new List<string>();
            foreach (PropertyInfo pi in args.GetType().GetProperties())
            {
                conds.Add($"{pi.Name} = @{pi.Name} ");
            }

            String tableName = typeof(TTable).Name;
            StringBuilder sql = new StringBuilder($@"SELECT * FROM {tableName} WHERE {String.Join("AND ", conds)}; ");
            var dataList = conn.Query<TTable>(sql.ToString(), args, txn).ToList();
            return dataList;
        }

        /// <summary>
        /// 可刪除多筆資料。
        /// </summary>
        public static int DeleteEx<TTable>(this SqlConnection conn, object args, SqlTransaction txn = null)
        {
            // 依 Property 動態加入 P-Key 查詢條件
            List<String> conds = new List<string>();
            foreach (PropertyInfo pi in args.GetType().GetProperties())
            {
                conds.Add($"{pi.Name} = @{pi.Name} ");
            }

            String tableName = typeof(TTable).Name;
            StringBuilder sql = new StringBuilder($@"DELETE FROM {tableName} WHERE {String.Join("AND ", conds)}; ");
            int ret = conn.Execute(sql.ToString(), args, txn);
            return ret;
        }

        /// <summary>
        /// UPDATE TABLE。為了一種經常性的應用：標記已刪除等。或只更一筆資料中的幾個（二、三個）欄位。
        /// ※注意：不可用於更新P-Key的值。
        /// </summary>
        /// <param name="newValues">新的值，請用Anonymous Type。</param>
        /// <param name="keys">P-Keys，請用Anonymous Type。</param>
        /// <returns>updCount，更新筆數。</returns>
        public static int UpdateEx<TTable>(this SqlConnection conn, object newValues, object keys, SqlTransaction txn = null)
        {
            DynamicParameters param = new DynamicParameters();

            // 依 Property 動態加入更新欄位
            List<String> fields = new List<string>();
            foreach (PropertyInfo pi in newValues.GetType().GetProperties())
            {
                fields.Add($"{pi.Name} = @{pi.Name} ");
                param.Add(pi.Name, pi.GetValue(newValues));
            }

            // 依 Property 動態加入 P-Key 查詢條件
            List<String> conds = new List<string>();
            foreach (PropertyInfo pi in keys.GetType().GetProperties())
            {
                conds.Add($"{pi.Name} = @{pi.Name} ");
                param.Add(pi.Name, pi.GetValue(keys));
            }

            String tableName = typeof(TTable).Name;
            StringBuilder sql = new StringBuilder($@"UPDATE {tableName} SET {String.Join(", ", fields)} WHERE {String.Join("AND ", conds)}; ");
            int updCount = conn.Execute(sql.ToString(), param, txn);
            return updCount;
        }
    }

}