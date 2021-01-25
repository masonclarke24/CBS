using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace TechnicalServices
{
    public class StoredProcedureHelpers
    {
        public SqlParameter[] GetSqlParameters(object dbTable)
        {
            if (dbTable is null)
                throw new ArgumentNullException(nameof(dbTable));

            List<SqlParameter> requiredParameters = new List<SqlParameter>();
            Type dbTableType = dbTable.GetType();

            requiredParameters.AddRange(GetSqlParameters(dbTableType, dbTable));

            return requiredParameters.ToArray();
        }

        private SqlParameter[] GetSqlParameters(Type type, object owner)
        {
            List<SqlParameter> requiredParameters = new List<SqlParameter>();
            foreach (var property in type.GetProperties())
            {
                var attributes = property.GetCustomAttributes(typeof(DbTranslatableAttribute), false);

                foreach (DbTranslatableAttribute attribute in attributes)
                {
                    if (property.PropertyType.GetProperties().Length > 0)
                        GetSqlParameters(property.PropertyType);
                    
                    requiredParameters.Add(new SqlParameter($"@{attribute.ColumnName}", property.GetValue(owner)));
                }
            }

            return requiredParameters.ToArray();
        }
    }
}
