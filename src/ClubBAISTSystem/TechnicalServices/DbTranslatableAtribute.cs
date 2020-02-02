using System;
using System.Runtime.CompilerServices;

namespace TechnicalServices
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    sealed class DbTranslatableAttribute : Attribute
    {
        public string ColumnName { get; private set; }

        // This is a positional argument
        public DbTranslatableAttribute([CallerMemberName]string columnName = null)
        {
            ColumnName = columnName;
        }
    }
}
