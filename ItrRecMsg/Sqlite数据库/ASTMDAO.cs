using SLHis.FrameWrok.DbManager;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using System.Linq;
using System.Text;

namespace ItrRecMsg
{
    public class ASTMDAO
    {
        string sqlUpdate =
    @"UPDATE ASTMData SET 
                ResultData= @ResultData
            WHERE SampleId = @SampleId ";
        string sqlInsert =
    @"INSERT INTO ASTMData 
                (SampleId,ResultData) 
                values
                (@SampleId,@ResultData) ;";

        public bool UpdateOrInsertASTM(string sampleID, string result)
        {
            bool Result = false;
            try
            {
                Result = Update(sampleID, result);
                if (Result != true)
                {
                    int newid = Insert(sampleID, result);
                    Result = newid > -1;
                }
            }
            catch (Exception ex)
            {
                //throw;
            }
            return Result;

        }

        public int Insert(string sampleID, string result)
        {
            int Result = -1;
            try
            {
                using (DbManager pDataManager = new DbManager())
                {
                    SQLiteConnection DbConnection =
                        ConnectionManager.GetInstance().GetConnection();
                    pDataManager.DBConnection = DbConnection;

                    List<DbParameter> ParList = GetSQLParameter(sampleID, result);
                    Result = Convert.ToInt32(pDataManager.SelOne(sqlInsert, ParList));
                }
            }
            catch (Exception ex)
            {
                //throw;
            }
            return Result;

        }

        #region 更新
        public bool Update(string sampleID, string result)
        {
            bool Result = false;
            try
            {
                using (DbManager pDataManager = new DbManager())
                {
                    SQLiteConnection DbConnection =
                        ConnectionManager.GetInstance().GetConnection();
                    pDataManager.DBConnection = DbConnection;
                    List<DbParameter> ParList = GetSQLParameter(sampleID, result);
                    Result = (pDataManager.ExecCommand(sqlUpdate, ParList) > 0);
                };


            }
            catch (Exception ex)
            {

                throw;
            }
            return Result;

        }
        #endregion
        public List<DbParameter> GetSQLParameter(string sampleID,string result)
        {
            List<DbParameter> ParList = new List<DbParameter>();

            ParList.Add(new SQLiteParameter("@SampleId", sampleID));
            ParList.Add(new SQLiteParameter("@ResultData", result));

            return ParList;
        }
    }
}
