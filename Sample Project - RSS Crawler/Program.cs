//
// Pauthor - An authoring library for Pivot collections
// http://getpivot.com
//
// Copyright (c) 2010, by Microsoft Corporation
// All rights reserved.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.LiveLabs.Pauthor.Streaming;
using Microsoft.LiveLabs.Pauthor.Streaming.Filters;

namespace Microsoft.LiveLabs.RssCrawler
{
    public class Program
    {
        public static void Main(string[] args)
        {
            RssCollectionSource source = new RssCollectionSource(args[0]);
            HtmlImageCreationSourceFilter sourceFilter1 = new HtmlImageCreationSourceFilter(source);
            sourceFilter1.HtmlTemplate = "<html><body><h1>{name}</h1>{description}" +
                "<p style=\"position:absolute;bottom:10px;left:10px\">{category:join:, }</p></body></html>";
            sourceFilter1.Width = 600;
            sourceFilter1.Height = 600;

            LocalCxmlCollectionTarget target = new LocalCxmlCollectionTarget(args[1]);
            DeepZoomTargetFilter targetFilter1 = new DeepZoomTargetFilter(target);

            targetFilter1.Write(sourceFilter1);
        }
    }
}
