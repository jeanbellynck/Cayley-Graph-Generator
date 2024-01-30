def length2Bin(a: list[int]) -> int:
    b = runsFromBinary(a)
    return length2Runs(b)

def runsFromBinary(a):
    b = [[],[]]
    if len(a) == 0:
        return b
    lastDigit = -1
    k=-1
    for i, x in enumerate(a):
        if lastDigit == -1 and x == 0:
            continue
        if x == 1 and lastDigit <= 0:
            k += 1
            b[0].append(0)
            b[1].append(0)
        b[x][k] += 1
        lastDigit = x
    b[0][k] = 2 # actually infinite: numbers of zeroes to the left
    return b

def length2Runs(b: list[list[int]]) -> int:
    # b is a list of two lists, each containing the number of runs of 0s and 1s
    # of length 2, respectively
    α = b[1]
    k = b[0]
    l = len(α)
    P = [False for i in range(l)]
    for i in range(l-1,-1,-1):
        P[i] = α[i] == 1 and (i+1 >= l or k[i+1] > 1 or P[i+1])

    return sum(1 if P[i] or k[i] > 1 else 2 for i in range(l))
    
def length2(k: int) -> int:
    a = binaryDigits(k)
    return length2Bin(a)

def binaryDigits(k):
    a = []
    while k > 0:
        a.append(k % 2)
        k = k // 2
    return a

def main(N):
    L = -1
    for n in range(N):
        a = binaryDigits(n)
        b = runsFromBinary(a)
        l = length2Runs(b)
        if l <= L:
            continue
        L = l
        print(n, " = ", a, ": ", l)
    

if __name__ == "__main__":
    # read in integer from command line, asking the user and retrying if necessary
    while True:
        try:
            N = int(input("Enter a positive integer: "))
            break
        except ValueError:
            print("Invalid input. Please enter a positive integer.")

    main(N)




