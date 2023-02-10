using System;
 
class GFG {
 
    // A utility function to print a
    // substring str[low...( high - (low+1))]
    static void printSubStr(string str, int low,
                            int high)
    {
        Console.WriteLine(str.Substring(low,
                                        high - low + 1));
    }
 
    // This function prints the longest
    // palindrome substring of str[].
    // It also returns the length of the
    // longest palindrome
    static int longestPalSubstr(string str)
    {
 
        // Get length of input string
        int n = str.Length;
 
        // Table[i, j] will be false if substring
        // str[i..j] is not palindrome. Else
        // table[i, j] will be true
        bool[, ] table = new bool[n, n];
 
        // All substrings of length 1 are palindromes
        int maxLength = 1;
        for (int i = 0; i < n; ++i)
            table[i, i] = true;
 
        // Check for sub-string of length 2.
        int start = 0;
 
        for (int i = 0; i < n - 1; ++i) {
            if (str[i] == str[i + 1]) {
                table[i, i + 1] = true;
                start = i;
                maxLength = 2;
            }
        }
 
        // Check for lengths greater than 2.
        // k is length of substring
        for (int k = 3; k <= n; ++k) {
 
            // Fix the starting index
            for (int i = 0; i < n - k + 1; ++i) {
 
                // Get the ending index of substring from
                // starting index i and length k
                int j = i + k - 1;
 
                // Checking for sub-string from ith index
                // to jth index iff str.charAt(i+1) to
                // str.charAt(j-1) is a palindrome
                if (table[i + 1, j - 1] && str[i] == str[j]) {
                    table[i, j] = true;
                    if (k > maxLength) {
                        start = i;
                        maxLength = k;
                    }
                }
            }
        }
        Console.Write("Longest palindrome substring is: ");
        printSubStr(str, start, start + maxLength - 1);
 
        // Return length of LPS
        return maxLength;
    }
 
    // Driver code
    public static void Main(string[] args)
    {
        string str = "forgeeksskeegfor";
 
        Console.WriteLine("Length is: " + longestPalSubstr(str));
    }
}