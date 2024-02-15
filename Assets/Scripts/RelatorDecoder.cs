using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dreamteck.Splines;

/**
 * Used to enable smart notation for relators
 **/
public class RelatorDecoder {

    public static string[] DecodeRelatorStrings(string relatorString, bool separate = true) => DecodeRelatorStrings(new[] { relatorString }, separate);

    public static string[] DecodeRelatorStrings(IEnumerable<string> relatorStrings, bool separate = true) {
        if (separate) 
            relatorStrings = relatorStrings.SelectMany(SeparateRelators);

        return relatorStrings.SelectMany(DecodeEquation).ToArray();
    }

    public static List<string> SeparateRelators(string relatorString)
    {
        string[] commaSeperatedStrings = relatorString.Replace(" ", "").Split(new char[] { ',', ';' });
        List<string> relators = new List<string>();

        // Quickly differentiates between seperator commas and commutator commas by counting square brackets
        StringBuilder relation = new ();
        int openSquareBracketCount = 0;
        int closeSquareBracketCount = 0;
        foreach (var substring in commaSeperatedStrings) {
            relation.Append(substring);

            openSquareBracketCount += substring.Count(c => c == '[');
            closeSquareBracketCount += substring.Count(c => c == ']');

            if (openSquareBracketCount > closeSquareBracketCount) {
                relation.Append(',');
                continue;
            }

            relators.Add(relation.ToString());
            relation.Clear();
            openSquareBracketCount = 0;
            closeSquareBracketCount = 0;
        }

        return relators;
    }

    static string[] DecodeEquation(string equation) {
        string[] sidesOfEquals = equation.Split('=');
        var leftSide = DecodeOneRelator(sidesOfEquals.FirstOrDefault());
        var res = from rightSide in sidesOfEquals.Skip(1).DefaultIfEmpty("") select leftSide + invertSymbol(DecodeOneRelator(rightSide));
        return res.ToArray();
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
    static string DecodeOneRelator(string symbol) 
    {
        if (string.IsNullOrEmpty(symbol))
            return "";

        int i = 0;

        while (i < symbol.Length) {
            switch (symbol[i])
            {
                // Rule: [aC, bC] -> ((aC)(bC)(aC)^-1(bC)^-1)
                case '[':
                {
                    int closingBracketIndex = findClosingBracketIndex(symbol, '[', ']', i);
                    int commaIndex = findClosingBracketIndex(symbol, '[', ',', i);
                    string insideBracket1 = mySubstring(symbol, i + 1, commaIndex);
                    string insideBracket2 = mySubstring(symbol, commaIndex + 1, closingBracketIndex);
                    insideBracket1 = DecodeOneRelator(insideBracket1);
                    insideBracket2 = DecodeOneRelator(insideBracket2);
                
                    string newSubSymbol = "((" + insideBracket1 + ")(" + insideBracket2 + ")(" + insideBracket1 + ")^-1(" + insideBracket2 + ")^-1)";
                    symbol = symbol[..i] + newSubSymbol + symbol[(closingBracketIndex + 1)..];
                    break;
                }
                // Rule: (abc)^3 -> abCabCabC and (abc) -> abc
                case '(':
                    symbol = applyBrackets(symbol, i);
                    break;
                // Rule: a^-1 -> a^1 and a^3 -> aaa
                default: 
                    if (i + 1 < symbol.Length && symbol[i + 1] == '^')
                        symbol = symbol[..i] + "(" + symbol[i] + ")" + symbol[(i + 1)..];
                    else
                        i++;
                    break;
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
        insideBrackets = DecodeOneRelator(insideBrackets);
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
