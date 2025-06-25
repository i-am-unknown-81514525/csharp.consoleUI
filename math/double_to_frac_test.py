def frac(v: float, lim: int = 64) -> tuple[int, int]:
    l = int(v) # numerator = (BigInteger)value;
    r = 1 # denominator = 1;
    k = v % 1 # double remain = value % 1;
    if v < 0:
        k = -(abs(v)%1)
    it = 0 # int it = 0;
    while k != 0 and it < lim: # while (remain != 0 && it < 64)
        k *= 2 # remain *= 2;
        l <<= 1 # numerator <<= 1;
        r <<= 1 # denominator <<= 1;
        if k >= 1: # if (remain >= 1)
            k -= 1 # remain -= 1;
            l += 1 # numerator += 1;
        if k <= -1: # if (remain <= -1)
            k += 1 # remain += 1;
            l -= 1 # numerator -= 1;
        it += 1 # it += 1;
    return l, r