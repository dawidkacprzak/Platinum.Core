using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;

namespace Platinum.Core.Types
{
    public interface IDal : IDisposable
    {
        void OpenConnection();
        void CloseConnection();
        void BeginTransaction();
        void RollbackTransaction();
        void CommitTransaction();
        bool IsTransactionOpen();
        int ExecuteNonQuery(string query);
        int ExecuteNonQuery(string query, List<SqlParameter> parameters);
        DbDataReader ExecuteReader(string query, List<SqlParameter> parameters);
        DbDataReader ExecuteReader(string query);
        object ExecuteScalar(string query, List<SqlParameter> parameters); 
        object ExecuteScalar(string query);
        string CreateMd5(string input);
    }
}