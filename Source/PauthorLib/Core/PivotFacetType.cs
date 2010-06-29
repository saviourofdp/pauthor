//
// Pauthor - An authoring library for Pivot collections
// http://getpivot.com
//
// Copyright (c) 2010, by Microsoft Corporation
// All rights reserved.
//

using System;
using System.Globalization;
using System.Text;

namespace Microsoft.LiveLabs.Pauthor.Core
{
    /// <summary>
    /// PivotFacetType represents each possible type for a facet category.
    /// </summary>
    /// <remarks>
    /// It defines a constant representing each value type, as well as functions to determine the type named in a given
    /// string or implied by the Type of a given object.
    /// </remarks>
    public class PivotFacetType
    {
        /// <summary>
        /// A constant representing the type for a single line of text.
        /// </summary>
        public static readonly PivotFacetType String = new StringFacetType("String");

        /// <summary>
        /// A constant representing the type for a long body (i.e., multiple lines) of text.
        /// </summary>
        public static readonly PivotFacetType LongString = new LongStringFacetType("LongString");

        /// <summary>
        /// A constant representing the type for numeric values (both integer and real numbers).
        /// </summary>
        public static readonly PivotFacetType Number = new NumberFacetType("Number");

        /// <summary>
        /// A constant representing the type for date and times.
        /// </summary>
        public static readonly PivotFacetType DateTime = new DateTimeFacetType("DateTime");

        /// <summary>
        /// A constant representing the type for hyperlinks.
        /// </summary>
        public static readonly PivotFacetType Link = new LinkFacetType("Link");

        /// <summary>
        /// A list of all the supported types for Pivot facet categories.
        /// </summary>
        public static readonly PivotFacetType[] AllFacetTypes = new PivotFacetType[]
        {
            DateTime, Link, LongString, Number, String
        };

        /// <summary>
        /// Determines the appropriate facet type based upon the Type of a given instance.
        /// </summary>
        /// <remarks>
        /// This method will inspect the given object's type and attempt to match it to the expected Type(s) for each
        /// supported Pivot facet category type. For strings, it will automatically differentiate between the LongString
        /// and String types based upon the actual length of the given text.
        /// </remarks>
        /// <param name="value">the object to use in inferring the facet category type</param>
        /// <returns>the appropriate constant for the type most closely matching the given object</returns>
        /// <exception cref="ArgumentException">
        /// if no suitable type could be determined for the given object
        /// </exception>
        public static PivotFacetType IdentifyType(IComparable value)
        {
            if (value == null) throw new ArgumentNullException("Cannot determine the type of a null value");

            if (DateTime.IsValidValue(value)) return DateTime;
            if (Link.IsValidValue(value)) return Link;
            if (Number.IsValidValue(value)) return Number;

            if (String.IsValidValue(value))
            {
                String testString = (String)value;
                if (testString.Length > LongStringThreshold) return LongString;
                return String;
            }

            throw new ArgumentException("No facet type matches Type: " + value.GetType().Name);
        }

        /// <summary>
        /// Returns the PivotFacetType constant named in a given string.
        /// </summary>
        /// <remarks>
        /// The given name should exactly match the name of one of the constants defined by this class. The comparison
        /// is case sensitive.
        /// </remarks>
        /// <param name="facetTypeName">the name of the desired facet type</param>
        /// <returns>the constant which matches the given name</returns>
        /// <exception cref="ArgumentException">if the given name did not match any supported type</exception>
        public static PivotFacetType Parse(String facetTypeName)
        {
            foreach (PivotFacetType facetType in AllFacetTypes)
            {
                if (facetType.Name == facetTypeName) return facetType;
            }

            throw new ArgumentException("No facet type matches name: \"" + facetTypeName + "\"");
        }

        /// <summary>
        /// Determines whether a given object is of an appropriate Type for this facet type.
        /// </summary>
        /// <remarks>
        /// Each facet type is different, so here is a table which describes the permitted object Type(s) for each facet
        /// type:
        /// <list type="table">
        /// <listheader><term>Facet Type</term><description>Allowed Object Types</description></listheader>
        /// <item><term>String</term><description>System.String</description></item>
        /// <item><term>LongString</term><description>System.String</description></item>
        /// <item><term>Number</term><description>int, long, float, uint, System.Int32, etc.</description></item>
        /// <item><term>DateTime</term><description>System.DateTime</description></item>
        /// <item><term>Link</term><description>Microsoft.LiveLabs.Pauthor.Core.PivotLink</description></item>
        /// </list>
        /// </remarks>
        /// <param name="value">the value to be validated</param>
        /// <returns>true if the given value is permitted for this facet type, false otherwise</returns>
        public virtual bool IsValidValue(IComparable value)
        {
            throw new NotImplementedException("Subclasses are expected to override this method");
        }

        /// <summary>
        /// Determines whether a given object is of an appropriate Type for this facet type and throws an exception if
        /// not.
        /// </summary>
        /// <remarks>
        /// See <see cref="IsValidValue"/> for details on what Type(s) are supported for each facet type.
        /// </remarks>
        /// <param name="value">the value to be validated</param>
        /// <exception cref="ArgumentException">if the given value was not appropriate for this facet type</exception>
        public virtual void AssertValidValue(IComparable value)
        {
            throw new NotImplementedException("Subclasses are expected to override this method");
        }

        /// <summary>
        /// Determines whether this facet type may be displayed in the facet pane.
        /// </summary>
        /// <remarks>
        /// This method will only return true for String, Number, and DateTime facet types.
        /// </remarks>
        /// <returns>true if this facet type can be displayed in the facet pane</returns>
        public virtual bool IsValidInFilterPane()
        {
            throw new NotImplementedException("Subclasses are expected to override this method");
        }

        /// <summary>
        /// Determines whether this facet type may be used in the free text search.
        /// </summary>
        /// <remarks>
        /// This method will only return true for String, LongString, and Link facet types.
        /// </remarks>
        /// <returns>true if this facet type can be used in free text search</returns>
        public virtual bool IsValidInWordWheel()
        {
            throw new NotImplementedException("Subclasses are expected to override this method");
        }

        /// <summary>
        /// Determines whether this facet type may have a custom sort order.
        /// </summary>
        /// <remarks>
        /// This method will only return true for the String facet type.
        /// </remarks>
        /// <returns>true if this facet type supports a custom sort order</returns>
        public virtual bool IsValidInSortOrder()
        {
            throw new NotImplementedException("Subclasses are expected to override this method");
        }

        /// <summary>
        /// Creates a string representations of a given value in accordance with the standard formatting for the
        /// PivotFacetType constant upon which this method is called.
        /// </summary>
        /// <remarks>
        /// The output from this method will always be parseable via the <see cref="ParseValue"/> method on the same
        /// PivotFacetType constant.
        /// </remarks>
        /// <param name="value">the value to be formatted</param>
        /// <returns>a string representation of the given value</returns>
        /// <exception cref="ArgumentException">
        /// if the given object is not an valid Type for this facet type
        /// </exception>
        public virtual String FormatValue(IComparable value)
        {
            this.AssertValidValue(value);
            return value.ToString();
        }

        /// <summary>
        /// Converts a string representation of a value into an appropriate C# object.
        /// </summary>
        /// <remarks>
        /// For the most part, these conversions are pretty clear (e.g., calling this method on the DateTime
        /// PivotFacetType constant will parse a value into a System.DateTime). The Number constant behaves slightly
        /// differently. If the given string appears to be a real number (i.e., it contains a decimal point), then a
        /// System.Double will be returned. If not, a System.Int64 will be returned.
        /// </remarks>
        /// <param name="value">the string to be parsed</param>
        /// <returns></returns>
        public virtual IComparable ParseValue(String value)
        {
            throw new NotImplementedException("Subclasses are expected to override this method");
        }

        /// <summary>
        /// Returns the name of this PivotFacetType constant.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.Name;
        }

        private PivotFacetType(String name)
        {
            this.Name = name;
        }

        private const int LongStringThreshold = 25;

        private String Name
        {
            get;
            set;
        }

        /// <summary>
        /// Defines the behavior of the DateTime facet type.
        /// </summary>
        private class DateTimeFacetType : PivotFacetType
        {
            public DateTimeFacetType(String name)
                : base(name)
            {
                // Do nothing.
            }

            public override bool IsValidValue(IComparable value)
            {
                return value is DateTime;
            }

            public override void AssertValidValue(IComparable value)
            {
                if (this.IsValidValue(value) == false)
                {
                    throw new ArgumentException("Invalid type for DateTime facet: " + value.GetType().Name);
                }
            }

            public override bool IsValidInFilterPane()
            {
                return true;
            }

            public override bool IsValidInWordWheel()
            {
                return false;
            }

            public override bool IsValidInSortOrder()
            {
                return false;
            }

            public override String FormatValue(IComparable value)
            {
                this.AssertValidValue(value);
                return ((DateTime)value).ToString("s");
            }

            public override IComparable ParseValue(String value)
            {
                return System.DateTime.Parse(value, CultureInfo.CurrentCulture);
            }
        }

        /// <summary>
        /// Defines the behavior of the Link facet type.
        /// </summary>
        private class LinkFacetType : PivotFacetType
        {
            public LinkFacetType(String name)
                : base(name)
            {
                // Do nothing.
            }

            public override bool IsValidValue(IComparable value)
            {
                return value is PivotLink;
            }

            public override void AssertValidValue(IComparable value)
            {
                if (this.IsValidValue(value) == false)
                {
                    throw new ArgumentException("Invalid type for Link facet: " + value.GetType().Name);
                }
            }

            public override bool IsValidInFilterPane()
            {
                return false;
            }

            public override bool IsValidInWordWheel()
            {
                return true;
            }

            public override bool IsValidInSortOrder()
            {
                return false;
            }

            public override IComparable ParseValue(String value)
            {
                return PivotLink.ParseValue(value);
            }
        }

        /// <summary>
        /// Defines the behavior of the LongString facet type.
        /// </summary>
        private class LongStringFacetType : StringFacetType
        {
            public LongStringFacetType(String name)
                : base(name)
            {
                // Do nothing.
            }

            public override void AssertValidValue(IComparable value)
            {
                if (this.IsValidValue(value) == false)
                {
                    throw new ArgumentException("Invalid type for LongString facet: " + value.GetType().Name);
                }
            }

            public override bool IsValidInFilterPane()
            {
                return false;
            }

            public override bool IsValidInSortOrder()
            {
                return false;
            }
        }

        /// <summary>
        /// Defines the behavior of the Number facet type.
        /// </summary>
        private class NumberFacetType : PivotFacetType
        {
            public NumberFacetType(String name)
                : base(name)
            {
                // Do nothing.
            }

            public override bool IsValidValue(IComparable value)
            {
                if (value is Byte) return true;
                if (value is Int16) return true;
                if (value is Int32) return true;
                if (value is Int64) return true;
                if (value is UInt16) return true;
                if (value is UInt32) return true;
                if (value is UInt64) return true;
                if (value is Single) return true;
                if (value is Double) return true;
                if (value is Decimal) return true;
                return false;
            }

            public override void AssertValidValue(IComparable value)
            {
                if (this.IsValidValue(value) == false)
                {
                    throw new ArgumentException("Invalid type for Number facet: " + value.GetType().Name);
                }
            }

            public override bool IsValidInFilterPane()
            {
                return true;
            }

            public override bool IsValidInWordWheel()
            {
                return false;
            }

            public override bool IsValidInSortOrder()
            {
                return false;
            }

            public override IComparable ParseValue(String value)
            {
                if (value.Contains("."))
                {
                    return System.Double.Parse(value);
                }
                else
                {
                    return Int64.Parse(value);
                }
            }
        }

        /// <summary>
        /// Defines the behavior of the String facet type.
        /// </summary>
        private class StringFacetType : PivotFacetType
        {
            public StringFacetType(String name)
                : base(name)
            {
                // Do nothing.
            }

            public override bool IsValidValue(IComparable value)
            {
                if ((value is String) == false) return false;
                if (((String)value).Length == 0) return false;
                return true;
            }

            public override void AssertValidValue(IComparable value)
            {
                if (this.IsValidValue(value) == false)
                {
                    throw new ArgumentException("Invalid type for String facet: " + value.GetType().Name);
                }
            }

            public override bool IsValidInFilterPane()
            {
                return true;
            }

            public override bool IsValidInWordWheel()
            {
                return true;
            }

            public override bool IsValidInSortOrder()
            {
                return true;
            }

            public override IComparable ParseValue(String value)
            {
                return value;
            }
        }
    }
}
