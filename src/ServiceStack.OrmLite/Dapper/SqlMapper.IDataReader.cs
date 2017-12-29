﻿using System;
using System.Collections.Generic;
using System.Data;

namespace ServiceStack.OrmLite.Dapper
{
    partial class SqlMapper
    {
        /// <summary>
        /// Parses a data reader to a sequence of data of the supplied type. Used for deserializing a reader without a connection, etc.
        /// </summary>
        public static IEnumerable<T> Parse<T>(this IDataReader reader)
        {
            if(reader.Read())
            {
                var deser = GetDeserializer(typeof(T), reader, 0, -1, false);
                do
                {
                    yield return (T)deser(reader);
                } while (reader.Read());
            }
        }

        /// <summary>
        /// Parses a data reader to a sequence of data of the supplied type (as object). Used for deserializing a reader without a connection, etc.
        /// </summary>
        public static IEnumerable<object> Parse(this IDataReader reader, Type type)
        {
            if (reader.Read())
            {
                var deser = GetDeserializer(type, reader, 0, -1, false);
                do
                {
                    yield return deser(reader);
                } while (reader.Read());
            }
        }

        /// <summary>
        /// Parses a data reader to a sequence of dynamic. Used for deserializing a reader without a connection, etc.
        /// </summary>
        public static IEnumerable<dynamic> Parse(this IDataReader reader)
        {
            if (reader.Read())
            {
                var deser = GetDapperRowDeserializer(reader, 0, -1, false);
                do
                {
                    yield return deser(reader);
                } while (reader.Read());
            }
        }
        
        /// <summary>
        /// Gets the row parser for a specific row on a data reader. This allows for type switching every row based on, for example, a TypeId column.
        /// You could return a collection of the base type but have each more specific.
        /// </summary>
        /// <param name="reader">The data reader to get the parser for the current row from</param>
        /// <param name="type">The type to get the parser for</param>
        /// <param name="startIndex">The start column index of the object (default 0)</param>
        /// <param name="length">The length of columns to read (default -1 = all fields following startIndex)</param>
        /// <param name="returnNullIfFirstMissing">Return null if we can't find the first column? (default false)</param>
        /// <returns>A parser for this specific object from this row.</returns>
        public static Func<IDataReader, object> GetRowParser(this IDataReader reader, Type type,
            int startIndex = 0, int length = -1, bool returnNullIfFirstMissing = false)
        {
            return GetDeserializer(type, reader, startIndex, length, returnNullIfFirstMissing);
        }

        /// <summary>
        /// Gets the row parser for a specific row on a data reader. This allows for type switching every row based on, for example, a TypeId column.
        /// You could return a collection of the base type but have each more specific.
        /// </summary>
        /// <param name="reader">The data reader to get the parser for the current row from</param>
        /// <param name="concreteType">The type to get the parser for</param>
        /// <param name="startIndex">The start column index of the object (default 0)</param>
        /// <param name="length">The length of columns to read (default -1 = all fields following startIndex)</param>
        /// <param name="returnNullIfFirstMissing">Return null if we can't find the first column? (default false)</param>
        /// <returns>A parser for this specific object from this row.</returns>
        /// <example>
        /// var result = new List&lt;BaseType&gt;();
        /// using (var reader = connection.ExecuteReader(@"
        ///   select 'abc' as Name, 1 as Type, 3.0 as Value
        ///   union all
        ///   select 'def' as Name, 2 as Type, 4.0 as Value"))
        /// {
        ///     if (reader.Read())
        ///     {
        ///         var toFoo = reader.GetRowParser&lt;BaseType&gt;(typeof(Foo));
        ///         var toBar = reader.GetRowParser&lt;BaseType&gt;(typeof(Bar));
        ///         var col = reader.GetOrdinal("Type");
        ///         do
        ///         {
        ///             switch (reader.GetInt32(col))
        ///             {
        ///                 case 1:
        ///                     result.Add(toFoo(reader));
        ///                     break;
        ///                 case 2:
        ///                     result.Add(toBar(reader));
        ///                     break;
        ///             }
        ///         } while (reader.Read());
        ///     }
        /// }
        ///  
        /// abstract class BaseType
        /// {
        ///     public abstract int Type { get; }
        /// }
        /// class Foo : BaseType
        /// {
        ///     public string Name { get; set; }
        ///     public override int Type =&gt; 1;
        /// }
        /// class Bar : BaseType
        /// {
        ///     public float Value { get; set; }
        ///     public override int Type =&gt; 2;
        /// }
        /// </example>
        public static Func<IDataReader, T> GetRowParser<T>(this IDataReader reader, Type concreteType = null,
            int startIndex = 0, int length = -1, bool returnNullIfFirstMissing = false)
        {
            if (concreteType == null) concreteType = typeof(T);
            var func = GetDeserializer(concreteType, reader, startIndex, length, returnNullIfFirstMissing);
            if (concreteType.IsValueType)
            {
                return _ => (T)func(_);
            }
            else
            {
                return (Func<IDataReader, T>)(Delegate)func;
            }
        }
    }
}
