# UnicodeHelper

*Note: This is a work-in-progress. In particular, some methods on UString have not been implemented yet.* 

UnicodeHelper is a .Net library to get Unicode property information about codepoints, and to better handle the 
upper planes (1-16) of Unicode where having to deal with surrogate pairs can get messy using normal .Net strings. 
Current version supoorts **Unicode 17.0.0**.

A goal for this library is to have *every* implemented method be extensively tested, so it should already be fairly stable.

