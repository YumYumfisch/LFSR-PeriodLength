# LFSR-PeriodLength

This is a tool to analyze the period lengths of Linear Feedback Shift Registers (LFSR).\
It calculates period lengths for various taps and starting values and provides taps to generate a sequence with the longest possible period for a given register count.

Tables visualizing the period lengths of LFSRs with different register counts in relation to the chosen tap and starting value can be found in [this folder](./tables/).

To calculate CSV files containing the period length for each combination of Tap-configuration and starting value, run LFSR-PeriodLength.
After obtaining these files, run CSV-PeriodLength to analyze which period lengths can be realized with which minimum amount of Flip-Flops in the LFSR.
