Prioity of allocating component space
1. Static size
2. Fraction prioity size, lower value -> first to allocate space, uint start from 0

If fraction of the highest value(lowest prioity) doesn't add up to 1, and there are space allocate for the prioity, then it would rescale to 1
otherwise the remaining space would allocate for the lower prioity
If the component allocate for static size / fraction, the eariler component would be consider first

The program would decide how to render such (horizonally/vertically etc.)