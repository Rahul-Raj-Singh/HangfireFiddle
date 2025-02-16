- Simple Web API app using Hangfire.


- The app is acting as both Hangfire client and sever.  
However, we can have separate client and server apps.   
One app acting as client can schedule the job and another app acting as Hangfire server can execute it. 



- Used SQL Server as backing data store.


- Contains example to - 
  - Schedule immediate and delayed jobs. (POST /schedule)
  - Schedule recurring job. (POST /schedule-recurring)
  - access Dashboard. (GET /hangfire) 🔥


- Hangfire triggers default retry mechanism whenever there's any exception in a job. The job is retried 10 times with exponential backoff. We can control this using `[AutomaticRetry(Attempts = 0)]`


- To ensure exactly one job is executing at any point in time, we can use `[DisableConcurrentExecution(timeoutInSeconds: 0)]`. Say, job-1 is running, and job-2 is executed. With this attribute job-2 will wait for `timeoutInSeconds` before it starts itself. If job-1 is not yet complete job-2 will fail.