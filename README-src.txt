INTRODUCTION

    This package contains the full source for the Pivot Collection Tool
    for Command Line (a.k.a. Pauthor).  This document describes the
    contents of the various directories and how to build the 
    application.

PROJECTS

    This package contains two source projects:

        * Source\PauthorLib: This project contains all the core
            functionality of the Pauthor application as well as all the
            interfaces necessary to develop your own extensions.

        * Source\Pauthor: This project contains a shell console
            application which wraps the library and makes some of its
            functions available on the command line.

    This package also contains a sample project demonstrating how to
    use the library:

        * Sample Project - RSS Crawler: This project is a sample of how
            to use the Pauthor library which converts simple RSS feeds
            into Pivot collections.

RESOURCES

    In addition to the source code, this package contains the following
    resources:

        * API Reference.chm: A Windows help archive which describes all
            the public interfaces, classes, properties and methods
            defined in the PauthorLib project.

        * Sample Collection: A sample collection stored in multiple
            formats. This can be used as an example of the proper
            formats as well as an easy way to test your own extensions
            to the Pauthor library.

BUILDING

    All of the source projects share a common solution in the Source
    directory. When built, their output will be placed in the "bin"
    directory. The sample project has its own solution. To build
    either, simply open the appropriate solution file in Visual Studio
    and select the "Build" option.

