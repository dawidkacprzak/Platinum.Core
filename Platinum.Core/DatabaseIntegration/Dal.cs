using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;
using Microsoft.Extensions.Logging;
using Platinum.Core.Types;
using Platinum.Core.Types.Exceptions;

namespace Platinum.Core.DatabaseIntegration
{
    public class Dal : IDal
    {
        private string connectionString;
        private readonly SqlConnection connection;
        private SqlTransaction transaction;

        public Dal(bool test = false)
        {
            if (test)
            {
                connectionString =
                    @"Data Source=;Initial Catalog=Platinum;User id=scheduler;Password; Connection Timeout=5";
            }
            else
            {
#if DEBUG
                connectionString =
                    @"Data Source=;Initial Catalog=Platinum;User id=scheduler;Password; Connection Timeout=5";
#endif
#if RELEASE
                connectionString =
                    @"Data Source=;Initial Catalog=Platinum;User id=scheduler;Password; Connection Timeout=5";
#endif
            }

            connection = new SqlConnection(connectionString);
        }

        public Dal(string connectionString)
        {
            this.connectionString = connectionString;

            try
            {
                connection = new SqlConnection(connectionString);
            }
            catch (ArgumentException)
            {
                throw new DalException("Connection string is in invalid format");
            }
        }

        public void OpenConnection()
        {
            try
            {
                connection.Open();
            }
            catch (SqlException ex)
            {
                throw new DalException(ex.Message);
            }
        }

        public void CloseConnection()
        {
            if (connection.State == ConnectionState.Closed)
            {
                throw new DalException("Cannot close connection if it is not open");
            }

            connection.Close();
        }

        public void BeginTransaction()
        {
            if (connection.State == ConnectionState.Closed) OpenConnection();

            if (transaction != null)
            {
                throw new DalException("Cannot begin transaction if there is opened transaction");
            }

            transaction = connection.BeginTransaction();
        }

        public void RollbackTransaction()
        {
            if (transaction != null)
            {
                transaction.Rollback();
                transaction = null;
            }
        }

        public void CommitTransaction()
        {
            if (transaction != null)
            {
                transaction.Commit();
                transaction = null;
            }
        }

        public bool IsTransactionOpen()
        {
            return transaction != null;
        }

        public int ExecuteNonQuery(string query)
        {
            return ExecuteNonQuery(query, null);
        }

        public int ExecuteNonQuery(string query, List<SqlParameter> parameters)
        {
            if (connection.State == ConnectionState.Closed) OpenConnection();
            SqlCommand command;
            command = IsTransactionOpen()
                ? new SqlCommand(query, connection, transaction)
                : new SqlCommand(query, connection);
            command.CommandText = query;
            command.CommandTimeout = 120;
            if (query.ToLower().Contains("drop database"))
            {
                throw new DalException("Query cannot drop databases. - Security");
            }

            if (parameters != null)
            {
                foreach (SqlParameter par in parameters)
                {
                    command.Parameters.Add(par);
                }
            }

            try
            {
                return command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new DalException(ex.Message + " query: " + query);
            }
        }

        public DbDataReader ExecuteReader(string query)
        {
            return ExecuteReader(query, null);
        }

        public DbDataReader ExecuteReader(string query, List<SqlParameter> parameters)
        {
            if (connection.State == ConnectionState.Closed) OpenConnection();
            SqlCommand command = connection.CreateCommand();
            command.CommandText = query;

            if (command.CommandText.ToLower().Contains("drop database"))
            {
                throw new DalException("Query cannot drop databases. - Security");
            }

            if (parameters != null)
            {
                foreach (SqlParameter par in parameters)
                {
                    command.Parameters.Add(par);
                }
            }

            try
            {
                return command.ExecuteReader();
            }
            catch (Exception ex)
            {
                throw new DalException(ex.Message);
            }
        }

        public object ExecuteScalar(string query)
        {
            return ExecuteScalar(query, null);
        }

        public object ExecuteScalar(string query, List<SqlParameter> parameters)
        {
            if (connection.State == ConnectionState.Closed) OpenConnection();
            SqlCommand command = IsTransactionOpen()
                ? new SqlCommand(query, connection, transaction)
                : new SqlCommand(query, connection);

            if (command.CommandText.ToLower().Contains("drop database"))
            {
                throw new DalException("Query cannot drop databases. - Security");
            }

            if (parameters != null)
            {
                foreach (SqlParameter par in parameters)
                {
                    command.Parameters.Add(par);
                }
            }

            try
            {
                return command.ExecuteScalar();
            }
            catch (Exception ex)
            {
                throw new DalException(ex.Message);
            }
        }

        string IDal.CreateMd5(string input)
        {
            return CreateMd5(input);
        }

        public void Dispose()
        {
            if (transaction != null)
            {
                transaction.Rollback();
                transaction = null;
            }

            connection?.Dispose();
        }

        public static string CreateMd5(string input)
        {
            using System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = Encoding.ASCII.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            StringBuilder sb = new StringBuilder();
            foreach (byte t in hashBytes)
            {
                sb.Append(t.ToString("X2"));
            }

            return sb.ToString();
        }
    }
}
