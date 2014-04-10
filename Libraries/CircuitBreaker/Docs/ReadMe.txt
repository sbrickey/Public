Based on the Circuit Breaker architecture described in their blog, I looked at a simple C# implementation.

The Core provides a basic interface and implementation class, while the sample app provides a running example with
 keyboard input mapped to disruptions (delay or outage).

References:
- http://techblog.netflix.com/2011/12/making-netflix-api-more-resilient.html