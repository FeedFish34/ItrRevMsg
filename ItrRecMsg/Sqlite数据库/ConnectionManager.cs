using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItrRecMsg
{
    public class ConnectionManager
    {
        #region 单一实例
		static ConnectionManager _Instance = null;
        static string DBName = @"Data Source=LocalData";

        static public ConnectionManager GetInstance() {
            if (_Instance == null) {
                _Instance = new ConnectionManager();
            }
            return _Instance;
        }
        #endregion

        public SQLiteConnection GetConnection()
        {
            return new SQLiteConnection(DBName);
                    
        }

    }
}
