using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRLFramework
{
    public abstract class Repository
    {
        public Repository() { }

        protected abstract string ConnectionString { get; }

        protected Object ExecuteScript(string SP)
        {
            List<Object> lstObj = new List<object>();
            object obj = new object();
            using (SqlConnection con = OpenConnection(ConnectionString))
            {
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = con;
                    command.CommandText = SP;
                    command.CommandType = System.Data.CommandType.Text;

                    using (SqlDataReader reader = command.ExecuteReader())
                    {

                        while (reader.Read())
                        {

                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                obj = reader.GetValue(i);
                            }
                        }
                    }
                }
                con.Close();
            }
            return obj;

        }

        protected T GetSingle<T>(string StoreProcedureName, List<Parameter> ParameterList, Func<SqlDataReader, T> dReader)
        {
            using (SqlConnection con = OpenConnection(ConnectionString))
            {
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = con;
                    command.CommandText = StoreProcedureName;
                    command.CommandType = System.Data.CommandType.StoredProcedure;

                    if (ParameterList != null)
                        command.Parameters.AddRange(GetParameters(ParameterList));

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if(reader.Read())
                            return (T)dReader(reader);
                    }
                }
            }
            return default(T);
        }

        private IEnumerable<object> GetList<T>(string StoreProcedureName, List<Parameter> ParameterList, Func<SqlDataReader, T> dReader)
        {
            using (SqlConnection con = OpenConnection(ConnectionString))
            {
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = con;
                    command.CommandText = StoreProcedureName;
                    command.CommandType = System.Data.CommandType.StoredProcedure;

                    if (ParameterList != null)
                        command.Parameters.AddRange(GetParameters(ParameterList));

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while(reader.Read())
                            yield return dReader(reader);
                    }
                }
            }
        }



        //to do, wraper to iterate over list of object to create a list of product
        protected List<object> GetList<T>(string SP, Func<SqlDataReader, T> dreader)
        {
            var lstObj = GetList(SP, null, dreader);
            List<object> lstOb = new List<object>();
            if (lstObj != null)
                foreach (var obj in lstObj)
                {
                    lstOb.Add(obj);
                }
            return lstOb;
        }


        public static List<T> Result<T>(List<object> lstObj)
        {
            return lstObj.Cast<T>().ToList();
        }

        private static SqlConnection OpenConnection(string ConnectionString)
        {
            SqlConnection conn = new SqlConnection(ConnectionString);
            conn.Open();
            return conn;
        }

        private Array GetParameters(List<Parameter> listParameter)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            SqlParameter para = new SqlParameter();
            foreach (var par in listParameter)
            {
                para.ParameterName = par.ParameterName;
                para.Value = par.ParameterValue;
                parameters.Add(para);
                para = new SqlParameter();
            }

            return parameters.ToArray();
        }



    }
}
