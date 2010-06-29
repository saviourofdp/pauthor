//
// Pauthor - An authoring library for Pivot collections
// http://getpivot.com
//
// Copyright (c) 2010, by Microsoft Corporation
// All rights reserved.
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.XPath;

namespace Microsoft.LiveLabs.Pauthor.Crawling
{
    /// <summary>
    /// XPathHelper provides a simplified interface for dealing with XML documents via XPath queries.
    /// </summary>
    public class XPathHelper
    {
        #region Public Constructors

        /// <summary>
        /// Creates a new XPathHelper with a given XML document.
        /// </summary>
        /// <remarks>
        /// The new helper is based at the root of the given document.
        /// </remarks>
        /// <param name="xmlData">an XML document</param>
        public XPathHelper(String xmlData)
        {
            using (StringReader reader = new StringReader(xmlData))
            {
                m_document = new XPathDocument(reader);
                m_namespaceManager = new XmlNamespaceManager(new NameTable());
                m_navigator = m_document.CreateNavigator();
            }
        }

        #endregion Public Constructors



        #region Public Methods

        /// <summary>
        /// Finds a single string value from an XPath query.
        /// </summary>
        /// <remarks>
        /// This method operates by identifying the first node which matches the given query, and then grabbing the
        /// value of that node.  If there are multiple nodes which match the query, the subsequent nodes are ignored.
        /// </remarks>
        /// <param name="query">an XPath query</param>
        /// <returns>the first value which matches the given query</returns>
        /// <exception cref="XPathException">if no nodes were matched by the given query</exception>
        public String FindString(String query)
        {
            String result = null;
            if (this.TryFindString(query, out result) == false)
            {
                throw new XPathException("Could not find a string value at: .../" + m_navigator.Name + "/" + query);
            }
            return result;
        }

        /// <summary>
        /// Finds a single string value from an XPath query.
        /// </summary>
        /// <remarks>
        /// This method operates by identifying the first node which matches the given query, and then grabbing the
        /// value of that node.  If there are multiple nodes which match the query, the subsequent nodes are ignored.
        /// </remarks>
        /// <param name="query">an XPath query</param>
        /// <param name="result">the first value which matches the given query, or <c>null</c> if no matches were
        ///     found</param>
        /// <returns><c>true</c> if a result was found, or <c>false</c> if nothing matched the given query</returns>
        public bool TryFindString(String query, out String result)
        {
            XPathNavigator node = m_navigator.SelectSingleNode(query, m_namespaceManager);
            if (node != null)
            {
                result = node.Value;
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }

        /// <summary>
        /// Finds a single <c>int</c> value from an XPath query.
        /// </summary>
        /// <remarks>
        /// This method operates by identifying the first node which matches the given query, and then grabbing the
        /// value of that node.  If there are multiple nodes which match the query, the subsequent nodes are ignored.
        /// </remarks>
        /// <param name="query">an XPath query</param>
        /// <returns>the first value which matches the given query</returns>
        /// <exception cref="XPathException">if no nodes were matched by the given query</exception>
        /// <exception cref="FormatException">if the value at the specified node could not be converted to an
        ///     <c>int</c></exception>
        public int FindInt(String query)
        {
            int result = 0;
            if (this.TryFindInt(query, out result) == false)
            {
                throw new XPathException("Could not find an int value at: " + query);
            }
            return result;
        }

        /// <summary>
        /// Finds a single <c>int</c> value from an XPath query.
        /// </summary>
        /// <remarks>
        /// This method operates by identifying the first node which matches the given query, and then grabbing the
        /// value of that node.  If there are multiple nodes which match the query, the subsequent nodes are ignored.
        /// </remarks>
        /// <param name="query">an XPath query</param>
        /// <param name="result">the first value which matches the given query, or <c>null</c> if no matches were
        ///     found</param>
        /// <returns><c>true</c> if a result was found, or <c>false</c> if nothing matched the given query</returns>
        /// <exception cref="FormatException">if the value at the specified node could not be converted to an
        ///     <c>int</c></exception>
        public bool TryFindInt(String query, out int result)
        {
            XPathNavigator node = m_navigator.SelectSingleNode(query, m_namespaceManager);
            if (node != null)
            {
                result = node.ValueAsInt;
                return true;
            }
            else
            {
                result = 0;
                return false;
            }
        }

        /// <summary>
        /// Finds a sequence of nodes which match a given query.
        /// </summary>
        /// <param name="query">an XPath query</param>
        /// <returns>a sequence of XPathHelpers which are rooted at nodes matching the given query</returns>
        public IEnumerable<XPathHelper> FindNodes(String query)
        {
            return from XPathNavigator node in m_navigator.Select(query,m_namespaceManager)
                select new XPathHelper(this, node);
        }

        /// <summary>
        /// Adds a namespace to this helper.
        /// </summary>
        /// <remarks>
        /// Queries submitted to this helper after a namespace has been set may use the given namespace prefix as part
        /// of the query.  Any helpers created by this one (via the <see cref="FindNodes"/> method) will inherit
        /// namespace declarations made to the parent.
        /// </remarks>
        /// <param name="prefix">the namespace prefix to be used</param>
        /// <param name="uri">the URI specifying the namespace</param>
        public void AddNamespace(String prefix, String uri)
        {
            m_namespaceManager.AddNamespace(prefix, uri);
        }

        #endregion Public Methods



        #region Private Constructors

        private XPathHelper(XPathHelper helper, XPathNavigator navigator)
        {
            m_document = helper.m_document;
            m_namespaceManager = helper.m_namespaceManager;
            m_navigator = navigator;
        }

        #endregion Private Constructors



        #region Private Instance Variables

        private XPathDocument m_document;

        private XmlNamespaceManager m_namespaceManager;

        private XPathNavigator m_navigator;

        #endregion Private Instance Variables
    }
}
