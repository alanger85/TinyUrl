# TinyUrl
A web application that creates unique short urls(8 characters long)
**Instructions:**
Just configure mongo db connections details in appsettings.json, default example:
"TinyUrlDatabase": {
        "ConnectionString": "mongodb://localhost:27017",
        "DatabaseName": "TinyUrlFromGit" 
        
    }

**Cache Management Solution:**
Cache layer has sliding expiration, each cache record has a time span to extend expiration, 
each cache hit extends the expiration by the time span specified.
also added max item cache size, if exeeded no new record will be added.
Advanteges: 
1. hot items will stay in cache for longer periods, prevent going to store.
2. items wont exceed max item cache size, memory usage is limited.
Disaventages: 
1. Difficult to determine optimal max item cache size, if configured to low then store could be hit alot.
