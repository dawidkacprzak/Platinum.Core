using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading;
using NUnit.Framework;
using Platinum.Core.DatabaseIntegration;
using Platinum.Core.Types;
using Platinum.Core.Types.Exceptions;

namespace Platinum.Tests.Integration
{
    [Apartment(ApartmentState.STA)]
    public class DalTest
    {
        [TearDown]
        public void TearDown()
        {
            Thread.Sleep(1000);
        }

        [Test]
        public void DoubleBeginTransaction()
        {
            Dal db = new Dal(true);
            db.OpenConnection();
            db.BeginTransaction();

            DalException ex = Assert.Throws<DalException>(() => db.BeginTransaction());
            Assert.That(ex.Message, Is.Not.Null.Or.Empty);
            db.Dispose();
        }

        [Test]
        public void DoubleCloseConnection()
        {
            Dal db = new Dal(true);
            db.OpenConnection();
            db.CloseConnection();
            DalException ex = Assert.Throws<DalException>(() => db.CloseConnection());
            Assert.That(ex, Is.Not.Null);
            db.Dispose();
        }

        [Test]
        public void CommitTransactionSuccess()
        {
            Dal db = new Dal(true);
            db.OpenConnection();
            db.BeginTransaction();
            db.CommitTransaction();
            db.Dispose();
        }

        [Test]
        public void RollbackTransactionSuccess()
        {
            Dal db = new Dal(true);
            db.OpenConnection();
            db.BeginTransaction();
            db.RollbackTransaction();
            db.Dispose();
        }

        [Test]
        public void ConnectionDisposeTestTransaction()
        {
            Dal db = new Dal(true);
            db.OpenConnection();
            db.BeginTransaction();
            db.Dispose();
            Assert.IsFalse(db.IsTransactionOpen());
        }

        [Test]
        public void ConnectionTransactionWithoutConnection()
        {
            Dal db = new Dal(true);
            db.BeginTransaction();
            db.Dispose();
            Assert.IsFalse(db.IsTransactionOpen());
        }

        [Test]
        public void ConnectionDisposeTestTransactionFail()
        {
            Dal db = new Dal(true);
            db.OpenConnection();
            db.BeginTransaction();
            Assert.True(db.IsTransactionOpen());
            db.RollbackTransaction();
            Assert.IsFalse(db.IsTransactionOpen());
        }

        [Test]
        public void ConnectionClose()
        {
            Dal db = new Dal(true);
            db.OpenConnection();
            Assert.DoesNotThrow(() => db.CloseConnection());
        }

        [Test]
        public void ConnectionCloseFail()
        {
            Dal db = new Dal(true);
            DalException ex = Assert.Throws<DalException>(() => db.CloseConnection());
            Assert.That(ex, Is.Not.Null);
        }


        [Test]
        public void DoubleTransactionFail()
        {
            Dal db = new Dal(true);
            db.OpenConnection();
            db.BeginTransaction();
            DalException ex = Assert.Throws<DalException>(() => db.BeginTransaction());
            Assert.That(ex,Is.Not.Null);
            db.Dispose();
        }

        [Test]
        public void ConnectionToTestDatabase()
        {
            using Dal db = new Dal(true);
            db.OpenConnection();
        }

        [Test]
        public void ConnectionToProdDatabase()
        {
            using Dal db = new Dal();
            db.OpenConnection();
        }

        [Test]
        public void ConnectionToNotExistsDbCorrectConnectionString()
        {
            using Dal db =
                new Dal(
                    "Data Source=13.12.122.228;Initial Catalog=Platinum;User id=scheduler;Password=64!;Connection Timeout=5");
            DalException ex = Assert.Throws<DalException>(() => db.OpenConnection());
            Assert.That(ex, Is.Not.Null.Or.Empty);
        }

        [Test]
        public void ConnectionToNoExistDb()
        {
            DalException ex = Assert.Throws<DalException>(() => new Dal("test").OpenConnection());
            Assert.That(ex,Is.Not.Null);
        }

        [Test]
        public void ExecuteNonQueryConnectionNotOpenedByCtorSuccess()
        {
            using Dal db = new Dal();
            db.ExecuteNonQuery("Select top 1 * from offers WITH (NOLOCK) ");
        }

        [Test]
        public void ExecuteNonQueryConnectionOpenedByCtorSuccess()
        {
            using Dal db = new Dal();
            db.OpenConnection();
            db.ExecuteNonQuery("Select top 1 * from offers WITH (NOLOCK)");
        }

        [Test]
        public void ExecuteFailNonQueryConnectionNotOpenedByCtorSuccess()
        {
            using Dal db = new Dal();
            DalException ex = Assert.Throws<DalException>(() => db.ExecuteNonQuery("Select toperr 1 * from offers WITH (NOLOCK)"));
            Assert.That(ex, Is.Not.Null);
        }

        [Test]
        public void ExecuteFailNonQueryConnectionOpenedByCtorSuccess()
        {
            using Dal db = new Dal();
            db.OpenConnection();
            DalException ex = Assert.Throws<DalException>(() => db.ExecuteNonQuery("Select toperr 1 * from offers WITH (NOLOCK) "));
            Assert.That(ex, Is.Not.Null);
        }
        
        [Test]
        public void ExecuteNonQueryWithParametersPass()
        {
            using Dal db = new Dal();
            db.OpenConnection();
            int rowCount = db.ExecuteNonQuery("Select top 1 * from offers WITH (NOLOCK) where id >= @id",new List<SqlParameter>()
            {
                new SqlParameter()
                {
                    ParameterName = "id",
                    SqlDbType =  SqlDbType.Int,
                    Value = 10
                }
            });
            Assert.AreEqual(rowCount,-1);
        }
        
        [Test]
        public void ExecuteFailNonQueryConnectionDropDatabase()
        {
            using Dal db = new Dal();
            db.OpenConnection();
            DalException ex = Assert.Throws<DalException>(() => db.ExecuteNonQuery("Drop database masterr"));
            Assert.That(ex, Is.Not.Null);
            Assert.That(ex.Message, Contains.Substring("cannot drop databases"));
        }
        
        [Test]
        public void ExecuteReaderTestDropQueryFail()
        {
            using Dal db = new Dal();
            db.OpenConnection();
            DalException ex = Assert.Throws<DalException>(() => db.ExecuteReader("Drop database masterr"));
            Assert.That(ex, Is.Not.Null);
            Assert.That(ex.Message, Contains.Substring("cannot drop databases"));
        }
        
        [Test]
        public void ExecuteReaderDoesNotThrow()
        {
            using Dal db = new Dal(true);
            db.OpenConnection(); 
            Assert.DoesNotThrow(() => db.ExecuteReader("SELECT TOP 1 * FROM websiteCategories WITH (NOLOCK)"));
        }
        
        [Test]
        public void ExecuteReaderContainRows()
        {
            using Dal db = new Dal(true);
            db.OpenConnection();
            using DbDataReader reader = db.ExecuteReader("SELECT TOP 1 * FROM websiteCategories WITH (NOLOCK)");
            Assert.IsTrue(reader.HasRows);
        }
        
        [Test]
        public void ExecuteReaderDoesNotContainRowsAndDoNotThrowException()
        {
            using Dal db = new Dal(true);
            db.OpenConnection();
            using DbDataReader reader = db.ExecuteReader("SELECT TOP 1 * FROM websiteCategories WITH (NOLOCK) WHERE Id = -9");
            Assert.IsTrue(!reader.HasRows);
        }
        
        [Test]
        public void ExecuteReaderExceptionBadQuery()
        {
            using Dal db = new Dal(true);
            db.OpenConnection();
            
            DalException ex = Assert.Throws<DalException>(()=> db.ExecuteReader("SELECTMISSPELL TOP 1 * FROM websiteCategories WHERE Id = -9"));
            Assert.That(ex, Is.Not.Null);
        }
        
        [Test]
        public void ExecuteReaderWithParameters()
        {
            List<SqlParameter> queryParameters = new List<SqlParameter>();
            SqlParameter parameter = new SqlParameter();
            parameter.ParameterName = "id";
            parameter.SqlDbType = SqlDbType.Int;
            parameter.Value = 1;
            queryParameters.Add(parameter);
            
            using Dal db = new Dal();
            db.OpenConnection();
            
            Assert.DoesNotThrow(() => db.ExecuteReader("select * from offers WITH (NOLOCK) where id = @id",queryParameters));
        }

        [Test]
        public void ExecuteScalarNoParamsNotThrow()
        {
            using (Dal db = new Dal())
            {
                db.ExecuteScalar("SELECT COUNT(*) From offers WITH (NOLOCK) ");
            }
        }
        
        [TestCase("test", "098f6bcd4621d373cade4e832627b4f6")]
        [TestCase("xxx", "f561aaf6ef0bf14d4208bb46a4ccb3ad")]
        [TestCase("", "d41d8cd98f00b204e9800998ecf8427e")]
        [TestCase("https://offer.test.xyz", "813fb2bbc24e7c74d4004814924312ba")]
        public void CreateMd5Success(string input,string output)
        {
            using (IDal db = new Dal())
            {
                Assert.AreEqual(output, db.CreateMd5(input).ToLower());
            }
        }
        
        [Test]
        public void ExecuteScalarNoParamsThrowSecurityError()
        {
            using (Dal db = new Dal())
            {
                DalException ex = Assert.Throws<DalException>(()=>db.ExecuteScalar("DROP DATABASE TEST"));
                Assert.IsTrue(ex.Message.Contains("Security"));
            }
        }
        
        [Test]
        public void ExecuteScalarParamsNotThrow()
        {
            List<SqlParameter> queryParameters = new List<SqlParameter>();
            SqlParameter parameter = new SqlParameter();
            parameter.ParameterName = "id";
            parameter.SqlDbType = SqlDbType.Int;
            parameter.Value = 1;
            queryParameters.Add(parameter);
            using (Dal db = new Dal())
            {
                db.ExecuteScalar("SELECT COUNT(*) FROM offers WITH (NOLOCK) where Id = @id", queryParameters);
            }
        }
                
        [Test]
        public void ExecuteScalarNoParamsThrowBadQuery()
        {
            using (Dal db = new Dal())
            {
                DalException ex = Assert.Throws<DalException>(()=>db.ExecuteScalar("Select bad query"));
                Assert.IsTrue(ex.Message.Contains("Invalid"));
            }
        }
    }
}