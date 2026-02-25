# File Scanner App

## Overview 
This is simple WPF desktop application built in .NET 8.

This project started as a personal challenge, I have been working in web development, but I had never built a desktop app before. I wanted to see whether I could design and deliver Windows application from scratch - mainly as a learning experience and simply for fun.


## What application does
* Scans selected folders and generate a report with detailed file information
* Allows user to select a target folder and date for scan purpose
* Allows to apply filters to search criteria: 
	* to include hidden files
	* to include protected system files
	* to apply date 'After' the selected date
	* to apply date 'Before' the selected date 
* Scans all files, including subdirectories and produces a text report containing
	* File path	
	* Last modified date
	* File owner
* Provides functionality to stop the scanning process
* Allows to exports results to CSV file
* Displays scan progress
* Measures and shows number of found files meeting the search criteria and 
* Measures and provides the total execution time 

	## Important:
	### Hidden and System files:
		By default, hidden and system files are not included in the scan. 
		To include them, you must enable the corresponding checkboxes.

		Some files in Windows have both attributes (hidden and system).
		If you select only one option, those files will still be excluded.
		For example:
		If a file is marked as hidden and system, and you only enable 'include hidden files', it will not appear in the results because it is still considered a system file.
		If you want such files included, both checkboxes must be selected !

	### Date Filter behavior:
		The **Before** and **After** filters do not include the selected date.
		
		If you want to include a specific day in a **Before** search, select the next day and choose **Before**
		The same logic applies to **After** - the selected date is always excluded ! 		

## Technology and tools used
* .NET
* WPF
* C#
* Visual Studio
* Git

## Lessons learned
* Desktop UI layout requires more planning than expected
* Threading matters more than in web development
* File system access brings edge cases (hidden files, system files, permissions)
* It proves that stepping outside your usual stack is uncomfortable - but doable