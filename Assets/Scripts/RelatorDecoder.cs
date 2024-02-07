using System.Collections.Generic;
using System.Linq;
using System.Text;

/**
 * Used to enable smart notation for relators
 **/
public class RelatorDecoder
{
    public static string[] decodeRelators(string relatorString) {
        string[] commaSeperatedStrings = relatorString.Replace(" ", "").Split(new char[] { ',', ';' });
        List<string> relators = new List<string>();

        // Quickly differentiates between seperator commas and commutator commas by counting square brackets
        int i = 0;
        string relation = "";
        int openSquareBracketCount = 0;
        int closeSquareBracketCount = 0;
        while(i < commaSeperatedStrings.Length) {
            relation = relation + commaSeperatedStrings[i];
            openSquareBracketCount += commaSeperatedStrings[i].Count(c => c == '[');
            closeSquareBracketCount += commaSeperatedStrings[i].Count(c => c == ']');
            if(openSquareBracketCount == closeSquareBracketCount) {
                relators.Add(relation);
                relation = "";
                openSquareBracketCount = 0;
                closeSquareBracketCount = 0;
            } else {
                relation = relation + ",";
            }
            i++;
        }
        
        // Checks for = signs and transforms them into relators
        for(int j = 0; j < relators.Count; j++) {
            if(relators[j].Contains("=")) {
                string[] sidesOfEquals = relators[j].Split('=');
                relators[j] = sidesOfEquals[0] + "(" + sidesOfEquals[1] + ")^-1";
            }
            relators[j] = decodeOneRelator(relators[j]);
        }
        return relators.ToArray();
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
   public static string invertSymbol(string symbol) {
        var result = (from generator in symbol select invertGenerator(generator));
        return new string(result.Reverse().ToArray());
    }

    public static char invertGenerator(char symbol) {
        return char.IsLower(symbol) ? char.ToUpper(symbol) : char.ToLower(symbol);
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
                symbol = mySubstring(symbol, 0, closingParenthesisIndex + 2) + symbol.Substring(closingParenthesisIndex + 3);
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
