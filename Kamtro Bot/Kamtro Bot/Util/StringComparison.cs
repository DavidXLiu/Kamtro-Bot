using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kamtro_Bot.Util
{
    /// <summary>
    /// All methods in this class are static methods that are used to compare two strings. - Arcy
    /// </summary>
    public static class UtilStringComparison
    {
        /// <summary>
        /// Compares how close the second string is to the first string. Returns a float with a maximum of 1, where 1 is a perfect match. - Arcy
        /// </summary>
        /// <param name="str1"></param>
        /// <param name="str2"></param>
        /// <returns></returns>
        public static float CompareWordScore(string str1, string str2, bool ignoreVariantCase = true)
        {
            float totalScore = 0f;
            float charScore = 1f / str1.Length;

            // Check which string is longer
            if (str1.Length >= str2.Length)
            {
                for (int i = 0; i < str2.Length; i++)
                {
                    // Check if case matters
                    if (ignoreVariantCase)
                    {
                        if (str1.ToLower()[i] == str2.ToLower()[i])
                        {
                            // Add character score if match
                            totalScore += charScore;
                        }
                        else
                        {
                            // Subtract character score if not match
                            totalScore -= charScore;
                        }
                    }
                    else
                    {
                        if (str1[i] == str2[i])
                        {
                            // Add character score if match
                            totalScore += charScore;
                        }
                        else
                        {
                            // Subtract character score if not match
                            totalScore -= charScore;
                        }
                    }
                }

                // Deduct from score for every additional letter. Deduction is half from normal
                totalScore -= (str1.Length - str2.Length) * (charScore / 2f);
            }
            else
            {
                for (int i = 0; i < str1.Length; i++)
                {
                    // Check if case matters
                    if (ignoreVariantCase)
                    {
                        if (str1.ToLower()[i] == str2.ToLower()[i])
                        {
                            // Add character score if match
                            totalScore += charScore;
                        }
                        else
                        {
                            // Subtract character score if not match
                            totalScore -= charScore;
                        }
                    }
                    else
                    {
                        if (str1[i] == str2[i])
                        {
                            // Add character score if match
                            totalScore += charScore;
                        }
                        else
                        {
                            // Subtract character score if not match
                            totalScore -= charScore;
                        }
                    }
                }

                // Deduct from score for every additional letter. Deduction is half from normal
                totalScore -= (str2.Length - str1.Length) * (charScore / 2f);
            }

            return totalScore;
        }

        /// <summary>
        /// Used for finding what alias was used for a command. Checks for what <see cref="string"/> was used in the list of strings provided and returns it.
        /// </summary>
        /// <returns></returns>
        public static string FindAlias(string message, string[] aliases)
        {
            foreach (string s in aliases)
            {
                if (message.ToLower().StartsWith(Program.Settings.Prefix + s.ToLower()))
                {
                    return s;
                }
            }

            // No alias found
            return null;
        }
    }
}
