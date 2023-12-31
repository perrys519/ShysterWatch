ShysterWatch 

Introduction

ShysterWatch is a toolkit designed for monitoring and analyzing health-related websites, particularly those associated with pseudoscientific health practices. It offers a range of functionalities to scrape, track, and analyze web content from various trade groups and pseudo-regulators.

Features

-Membership Database Scraping: Extracts information from membership databases of trade groups/pseudo-regulators (CNHC, GCC, BCA, MCC, GoSC, SoH).

-Websites.xml Conversion: Converts the scraped data into a generic Websites.xml file listing all related websites.

-Web Spider: Downloads a limited number of pages from each website, implementing version control to track changes over time.

-Search and Monitoring Tool: Utilizes Windows Indexing Services to search for specific claims on websites, highlight them, and monitor changes over time.

-Health Term Prevalence Tool: Analyzes the prevalence of specific health-related terms across all monitored websites.

-AI-Assisted Analysis Tool: Extracts website text and uses OpenAI's API to identify false and misleading health claims. Currently operational for CNHC websites.

How to add a new ShysterGroup

Most likely, you're going to want to use this within a group that isn't already there. So you'll need to program in a new ShysterGroup to represent the trade group of psuedo-regulator whose members you want to monitor. 

To do that, first create a new class to represent the group in the ShyterGroup folder. This should inherit from ShysterGroup. You should be able to figure out exactly how to do it by looking at the other classes there. The most important property to override is FolderName, which simply defines the name of the subfolder that this ShysterGroup's data is held in.

You then need to set up a class that inherits from MembershipDatabase to represent the structure of the membership data that you will get from the scraper. These files should sit in a folder specific to the ShysterWatch Group in the MembershipDatabases folder.

Then you need to create a Scraper class in the Scrapers folder that inherits from MembershipDatabaseScraper. This contains all the functionality to scrape the membership information from that group's website.

As you implement these classes, go back to your ShysterGroup class and reference the relevant classes by overriding MembershipDb, Scraper. 

Doing the above should allow you to scrape the website and download all of the websites.

To use the False Claims Finder, you may want to build a AiAnalysisHelper class specific to the ShysterGroup that inherits from AiAnalysisHelper. This goes in the AiAnalysisHelpers folder and allows you to customise the reporting. There is not much to do here, but you do need to reference this by overriding AiAnalysisHelper in the ShysterGroup class once it's done.
