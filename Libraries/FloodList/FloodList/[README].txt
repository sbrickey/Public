FIFO based lists with limited buffer space. Additions beyond buffer are either dropped, or replace oldest data.
Conceptually similar to RS485

uses:
 high frequency data provider
 high frequency data consumer
 NON CRITICAL polling (buffer overflow will cause data loss; expected if data provider supplies data faster than consumer polls)
 low memory footprint

attributes:
 MTA support
 minimal locking

includes:
	unit tests
		sample providers and consumers, including data metrics
		unit tests with/without thread affinity
		provide metric results into output window (via system.diag.debug.writeline)

notes:
	performance is best when:
		FloodList does NOT have thread affinity
		providers/consumers DOES have thread affinity, on separate CORES (not HT threads)
