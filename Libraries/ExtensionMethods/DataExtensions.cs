namespace SBrickey.Libraries.ExtensionMethods
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Text;

    public static class DataExtensions
    {
        public static T GetValue<T>(this IDataReader dr, string name) where T : class
        {
            var i = GetOrdinal(dr, name);
            if (!i.HasValue)
                return null;

            if (dr.IsDBNull(i.Value))
                return null;

            return dr[i.Value] as T;
        }
        public static T GetValue<T>(this DataRow dr, string name) where T : class
        {
            var i = dr.GetOrdinal(name);
            if (!i.HasValue)
                return null;

            if (dr.IsNull(i.Value))
                return null;

            return dr[i.Value] as T;
        }

        public static T GetValueOrException<T>(this IDataReader dr, string name) where T : struct
        {
            var i = GetOrdinal(dr, name);

            if (!i.HasValue)
                throw new KeyNotFoundException("Column [" + name + "] not found");

            if (dr.IsDBNull(i.Value))
                throw new ArgumentNullException(name);

            var val = dr[i.Value];
            return (T)System.Convert.ChangeType(val, typeof(T));
        }
        public static T GetValueOrException<T>(this DataRow dr, string name) where T : struct
        {
            var i = dr.GetOrdinal(name);

            if (!i.HasValue)
                throw new KeyNotFoundException("Column [" + name + "] not found");

            if (dr.IsNull(i.Value))
                throw new ArgumentNullException(name);

            var val = dr[i.Value];
            return (T)System.Convert.ChangeType(val, typeof(T));
        }

        public static Nullable<T> GetValueOrNull<T>(this IDataReader dr, string name) where T : struct
        {
            var i = GetOrdinal(dr, name);
            if (!i.HasValue)
                return null;
            if (dr.IsDBNull(i.Value))
                return null;

            var val = dr[i.Value];
            return (T)System.Convert.ChangeType(val, typeof(T));
        }
        public static Nullable<T> GetValueOrNull<T>(this DataRow dr, string name) where T : struct
        {
            var i = dr.GetOrdinal(name);
            if (!i.HasValue)
                return null;
            if (dr.IsNull(i.Value))
                return null;

            var val = dr[i.Value];
            return (T)System.Convert.ChangeType(val, typeof(T));
        }


        public static int? GetOrdinal(this IDataReader dr, string name)
        {
            var col = dr.GetSchemaTable()
                     .Columns.Cast<DataColumn>()
                     .Where(c => c.ColumnName == name)
                     .ToList();

            if (!col.Any())
                return null;
            //throw new KeyNotFoundException("Column [" + name + "] not found");

            if (col.Count > 1)
                throw new DuplicateNameException("Duplicate columns with name [" + name + "] were found");

            return col.Single().Ordinal;
        }
        public static int? GetOrdinal(this DataRow dr, string name)
        {
            var col = dr.Table
                     .Columns.Cast<DataColumn>()
                     .Where(c => c.ColumnName == name)
                     .ToList();

            if (!col.Any())
                return null;
                //throw new KeyNotFoundException("Column [" + name + "] not found");

            if (col.Count > 1)
                throw new DuplicateNameException("Duplicate columns with name [" + name + "] were found");

            return col.Single().Ordinal;
        }

    } // class
} // namespace