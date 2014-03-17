Designed to provide a mock SMTP server, for unit testing / BDD testing / integration testing.

SmtpSimulator.Core	provides the primary multi-threaded SMTP server engine classes
SmtpSimulator		provides an EXE wrapper for the engine classes, to be run in standalone mode

NOTE:
- For use of standard SMTP port 25, the application must be running with elevated (admin) privileges.
  For standard F5 debugging, Visual Studio will need those elevated privileges. Run VS as admin.
  The unit tests run under port 8025 to avoid this issue.

References:
 - http://www.codeproject.com/Tips/286952/create-a-simple-smtp-server-in-csharp
 - http://www.codeproject.com/Articles/456380/A-Csharp-SMTP-server-receiver