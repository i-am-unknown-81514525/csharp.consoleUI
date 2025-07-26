def wrap2(src: str, amount: int, weakSplitOptions: str = " .,/?!<>:;", stripWith: str = " ") -> str:
    if amount < 1:
        raise ValueError("The amount cannot be <= 0 as there are no method to make a valid split")
    output = ""
    currLine = ""
    tmp = ""
    lineCount = 0
    lineCountWithTmp = 0
    appliedNewLine = True
    justEol = True
    for v in src:
        if (lineCountWithTmp >= amount):
            if lineCount != 0:
                if not appliedNewLine:
                    output += "\n"
                appliedNewLine = False
                output += currLine.strip(stripWith)
                lineCountWithTmp -= lineCount
                lineCount = 0
                currLine = ""
                justEol = True
            else:
                if not appliedNewLine:
                    output += "\n"
                appliedNewLine = False
                output += tmp[:amount].strip(stripWith)
                tmp = tmp[amount:]
                lineCountWithTmp -= amount
                justEol = True
        if v == "\n":
            if not justEol and not appliedNewLine:
                output += "\n"
            # appliedNewLine = False
            output += currLine
            output += tmp
            output += "\n"
            currLine = ""
            tmp = ""
            appliedNewLine = True
            lineCount = 0
            lineCountWithTmp = 0
            justEol = True
            continue
        if not((lineCountWithTmp == 0 or lineCountWithTmp == amount) and v in stripWith): # or lineCountWithTmp + 1 == amount
            lineCountWithTmp += 1
            tmp += v
        justEol = False
        if v in weakSplitOptions:
            currLine += tmp
            tmp = ""
            lineCount = lineCountWithTmp
    if (lineCountWithTmp - lineCount > 0):
        currLine += tmp
        lineCount = lineCountWithTmp
    if (lineCount > 0):
        if not appliedNewLine:
            output += "\n"
        output += currLine.strip(stripWith)
    return output
    





if __name__ == "__main__":
    print("\n\n\n\nSTART")
    print(wrap2("a\nbcde\nfg",1))
    print("END\n\n\n\nSTART")
    print(wrap2("a\nbcde\n\nfg",3))
    print("END\n\n\n\nSTART")
    print(wrap2("a\nbcde\nfg",3))
    print("END\n\n\n\nSTART")
    print(wrap2("thisisareallylongword", 10))
    print("END\n\n\n\nSTART")
    print(wrap2("a\n\nb", 5))
    print("END\n\n\n\nSTART")
    print(wrap2("A line with some spaces", 15))
    print("END\n\n\n\nSTART")
    print(wrap2("12345 1234567", 5))
    print("END\n\n\n\nSTART")
    print(wrap2("abcd e", 5))
    print("END\n\n\n\nSTART")
    print(wrap2("abc def", 7))
    print("END\n\n\n\nSTART")
    print(repr(wrap2("trailing spaces  ", 20)))