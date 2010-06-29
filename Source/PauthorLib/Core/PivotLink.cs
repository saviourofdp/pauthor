//
// Pauthor - An authoring library for Pivot collections
// http://getpivot.com
//
// Copyright (c) 2010, by Microsoft Corporation
// All rights reserved.
//

using System;
using System.Text;

namespace Microsoft.LiveLabs.Pauthor.Core
{
    /// <summary>
    /// PivotLink represents a hyperlink used within a Pivot collection.
    /// </summary>
    /// <remarks>
    /// Each hyperlink contains both the URL to which it refers as well as the user-facing text which represents the
    /// link.
    /// </remarks>
    public class PivotLink : IComparable
    {
        /// <summary>
        /// The delimiter used to separate the title from the URL portions in the string representation of a link.
        /// </summary>
        public const String Delimiter = "||";

        /// <summary>
        /// Converts a string representation of a link into an actual PivotLink instance.
        /// </summary>
        /// <remarks>
        /// The string representation is expected to have the title of the link, followed by the delimiter ("||"),
        /// followed by the URL. Either
        /// portion is allowed to be empty; in which case, that portion will be assigned null in the returned object. If
        /// the delimiter is missing, then the entire value will be assumed to be the title of the link, and the url
        /// will be assigned null. If null is given, then null will be returned.
        /// </remarks>
        /// <param name="value">the string representation to parse</param>
        /// <returns>a PivotLink object containing the specified hyperlink</returns>
        public static PivotLink ParseValue(String value)
        {
            if (value == null) return null;

            int index = value.IndexOf(Delimiter);
            if (index == -1) return new PivotLink(value, null);

            String title = "";
            String url = "";
            if (index > 0)
            {
                title = value.Substring(0, index);
            }

            index = index + Delimiter.Length;
            if (index < value.Length)
            {
                url = value.Substring(index);
            }

            return new PivotLink(title, url);
        }

        /// <summary>
        /// Creates a new link object with the given title and URL.
        /// </summary>
        /// <param name="title">the title of the new hyperlink</param>
        /// <param name="url">the URL to which the new hyperlink refers</param>
        public PivotLink(String title, String url)
        {
            this.Title = title;
            this.Url = url;
        }

        /// <summary>
        /// The user-facing title of this link.
        /// </summary>
        /// <remarks>
        /// This property may take any value, but if assigned an empty string, it will automatically be converted to
        /// null. The initial value of this property is assigned by the constructor.
        /// </remarks>
        public String Title
        {
            get { return m_title; }

            set
            {
                m_title = String.IsNullOrEmpty(value) ? null : value;
            }
        }

        /// <summary>
        /// The URL to which this hyperlink refers.
        /// </summary>
        /// <remarks>
        /// This property may take any value (i.e., it is not validated as a proper URL), but if assigned an empty
        /// string, it will automatically be converted to null. The initial value of this property is assigned by the
        /// constructor.
        /// </remarks>
        public String Url
        {
            get { return m_url; }

            set
            {
                m_url = String.IsNullOrEmpty(value) ? null : value;
            }
        }

        /// <summary>
        /// Compares the current instance with another object of the same type and returns an integer that indicates
        /// whether the current instance precedes, follows, or occurs in the same position in the sort order as the
        /// other object.
        /// </summary>
        /// <remarks>
        /// This method first compares the titles of the two links, and then compares the URLs.
        /// </remarks>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <returns>A value that indicates the relative order of the objects being compared.</returns>
        /// <exception cref="ArgumentException">if the given value is not a PivotLink</exception>
        public int CompareTo(Object obj)
        {
            if ((obj is PivotLink) == false) throw new ArgumentException("Invalid type: " + obj.GetType());

            PivotLink that = (PivotLink)obj;
            int result = String.Compare(this.Title, that.Title);
            if (result != 0) return result;
            return String.Compare(this.Url, that.Url);
        }

        /// <summary>
        /// Creates a string representation of this hyperlink by concatenating the title, the delimter, and the URL.
        /// </summary>
        /// <returns>a string representation of this hyperlink</returns>
        public override string ToString()
        {
            return m_title + Delimiter + m_url;
        }

        private String m_title;

        private String m_url;
    }
}
