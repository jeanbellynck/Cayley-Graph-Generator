using System.Linq;
using System.Text;

/**
 * Used to enable smart notation for relators
 **/
public class RelatorDecoder
{
    public static string decodeRelator(string symbol) {
        return decodeOneRelator(symbol.Replace(" ", ""));
        //return "";
    }
    
    /**
     * Takes a relator written as a formula and returns extends it to a full relator
     * Applies the following rules:
     * [aC, bC] -> ((aC)(bC)(aC)^-1(bC)^-1)
     * (abC)^-1 (cBA)^1
     * (abC)^3 -> abCabCabC
     * a^-1 -> a^1
     * a^3 -> aaa
     * (abc) -> abc
     **/
    private static string decodeOneRelator(string symbol) 
    {
        int i = 0;

        while (i < symbol.Length)
        {
            if (symbol[i] == '[') // Rule: [aC, bC] -> ((aC)(bC)(aC)^-1(bC)^-1)
            {
                int closingBracketIndex = findClosingBracketIndex(symbol, '[', ']', i);
                int commaIndex = findClosingBracketIndex(symbol, '[', ',', i);
                string insideBracket1 = mySubstring(symbol, i + 1, commaIndex);
                string insideBracket2 = mySubstring(symbol, commaIndex + 1, closingBracketIndex);
                insideBracket1 = decodeOneRelator(insideBracket1);
                insideBracket2 = decodeOneRelator(insideBracket2);
                
                string newSubSymbol = "((" + insideBracket1 + ")(" + insideBracket2 + ")(" + insideBracket1 + ")^-1(" + insideBracket2 + ")^-1)";
                symbol = symbol.Substring(0, i) + newSubSymbol + symbol.Substring(closingBracketIndex + 1);
            }
            else if (symbol[i] == '(') // Rule: (abc)^3 -> abCabCabC and (abc) -> abc
            {
                symbol = applyBrackets(symbol, i);
            }
            else // Rule: a^-1 -> a^1 and a^3 -> aaa
            {
                if(i+1 < symbol.Length && symbol[i+1] == '^') {
                    symbol = symbol.Substring(0, i) + "(" + symbol[i].ToString() + ")" + symbol.Substring(i+1);
                } else {
                    i++;
                }
            }
        }

        return symbol;
    }

    /**
     * Takes in a word. Returns the position of the bracket that closes the bracket at the given index 
     **/
    private static int findClosingBracketIndex(string symbol, char opBracket, char clBracket, int openingBracketIndex)
    {
        int closingBracketIndex = openingBracketIndex+1;
        int bracketCount = 1;

        while (bracketCount > 0 && closingBracketIndex < symbol.Length)
        {
            if (symbol[closingBracketIndex] == opBracket)
            {
                bracketCount++;
            }
            else if (symbol[closingBracketIndex] == clBracket)
            {
                bracketCount--;
            }
            closingBracketIndex++;
        }
        closingBracketIndex--; // Adjust to point to the closing bracket
        return closingBracketIndex;
    } 

    private static int findPowerValue(string symbol, int powerIndex)
    {
        int i = powerIndex + 1;
        if(i < symbol.Length && symbol[i] == '-') {
            i++;
        }
        while (i < symbol.Length && char.IsDigit(symbol[i]))
        {
            i++;
        }
        return int.Parse(mySubstring(symbol, powerIndex + 1, i));
    }

    /**
     * Takes a relator consisting of only letters and returns the inverse by big letters small, small letters big and reversing the string order
     **/
    private static string invertSymbol(string symbol) {
        StringBuilder result = new StringBuilder();
        for(int i = 0; i < symbol.Length; i++) {
            if(char.IsLower(symbol[i])) {
                result.Append(char.ToUpper(symbol[i]));
            } else {
                result.Append(char.ToLower(symbol[i]));
            }
        }
        return new string(result.ToString().Reverse().ToArray());
    }

    /**
     * Does logic for brackets, edits the symbol variable
     */
    private static string applyBrackets(string symbol, int bracketIndex) {
        int closingParenthesisIndex = findClosingBracketIndex(symbol, '(', ')', bracketIndex);
        string insideBrackets = mySubstring(symbol, bracketIndex + 1, closingParenthesisIndex);
        insideBrackets = decodeOneRelator(insideBrackets);
        // If the bracket was surrounded by a power
        if (closingParenthesisIndex + 1 < symbol.Length && symbol[closingParenthesisIndex + 1] == '^')
        {
            int power = findPowerValue(symbol, closingParenthesisIndex+1);
            // If the power is negative invert the inside of the brackets
            if(power < 0) {
                power = -power;
                insideBrackets = invertSymbol(insideBrackets);
                // Remove minus sign (this code might remove the ^ symbol instead of the minus)
                symbol = mySubstring(symbol, 0, closingParenthesisIndex + 1) + symbol.Substring(closingParenthesisIndex + 2);
            }
            // Repeat the inside of the brackets
            insideBrackets = string.Concat(Enumerable.Repeat(insideBrackets, power));
            symbol = mySubstring(symbol, 0, bracketIndex) + insideBrackets + symbol.Substring(closingParenthesisIndex + 2 + power.ToString().Length);
        }
        else {
            symbol = mySubstring(symbol, 0, bracketIndex) + insideBrackets + symbol.Substring(closingParenthesisIndex + 1);
        }
        return symbol;
    }  

    private static string mySubstring(string s, int startIndex, int endIndex) {
        return s.Substring(startIndex, endIndex - startIndex);
    }
}