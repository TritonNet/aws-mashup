#region - References -
using Cumulus.Aws.Common.Abstraction;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
#endregion

namespace Cumulus.Aws.Common.Implementation
{
    internal class CsvConverter : ICsvConverter
    {
        #region - Public Methods | ICsvConverter -

        public IAsyncEnumerable<T> Convert<T>(Stream csvStream, char seperator) where T : new()
        {
            return new CsvRecord<T>(csvStream, seperator);
        }

        #endregion
    }

    internal class CsvRecord<T> : IAsyncEnumerable<T>, IAsyncEnumerator<T> where T : new()
    {
        #region - Private Methods -

        private StreamReader streamReader;

        private Dictionary<PropertyInfo, string> headerMapping;

        private Dictionary<int, PropertyInfo> headerIndex;

        private Type entityType;

        private char seperator;

        #endregion

        #region - Public Methods -

        public CsvRecord(Stream stream, char seperator)
        {
            this.streamReader = new StreamReader(stream);

            this.seperator = seperator;

            this.entityType = typeof(T);

            this.headerMapping = entityType
                            .GetProperties()
                            .ToDictionary(
                                propKey => propKey,
                                prop =>
                                {
                                    var columnAttr = (ColumnAttribute)Attribute.GetCustomAttribute(prop, typeof(ColumnAttribute));

                                    return columnAttr != null ? columnAttr.Name : prop.Name;
                                }
                            );
        }

        public T Current { get; private set; }

        object IAsyncEnumerator.Current { get => this.Current; }

        public void Dispose()
        {
            this.streamReader.Dispose();
        }

        public IAsyncEnumerator<T> GetEnumerator()
        {
            return this;
        }

        public async Task<bool> MoveNextAsync()
        {
            this.Current = default(T);

            if (!streamReader.EndOfStream)
            {
                if (this.headerIndex == null)
                {
                    this.headerIndex = new Dictionary<int, PropertyInfo>();

                    // First line contains the headers.
                    var headerLine = await this.streamReader.ReadLineAsync();

                    // separated by 'seperator'
                    var headerColumns = headerLine.Split(this.seperator);
                    for (int headerIndex = 0; headerIndex < headerColumns.Length; headerIndex++)
                    {
                        var property = this.headerMapping
                                                .Where(e => e.Value == headerColumns[headerIndex])
                                                .Select(e => e.Key)
                                                .FirstOrDefault();

                        if (property != null)
                            this.headerIndex[headerIndex] = property;
                    }
                }

                var valueLine = default(string);
                do
                {
                    valueLine = await this.streamReader.ReadLineAsync();
                } while (string.IsNullOrWhiteSpace(valueLine) && !streamReader.EndOfStream);

                if (string.IsNullOrWhiteSpace(valueLine))
                    return false;

                var valueColumns = valueLine.Split(this.seperator);

                var entity = new T();

                foreach (var mapping in this.headerIndex)
                {
                    if (mapping.Key < valueColumns.Length)
                    {
                        var value = valueColumns[mapping.Key].Trim('"');

                        var valueObj = TypeDescriptor.GetConverter(mapping.Value.PropertyType).ConvertFromString(value);

                        mapping.Value.SetValue(entity, valueObj);
                    }
                }

                this.Current = entity;

                return true;
            }

            return false;
        }

        public void Reset()
        {
            this.streamReader.BaseStream.Seek(0, SeekOrigin.Begin);
        }

        IAsyncEnumerator IAsyncEnumerable.GetEnumerator()
        {
            return this;
        }

        #endregion
    }
}
