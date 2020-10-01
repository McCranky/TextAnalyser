using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using static System.String;

namespace TextAnalyser
{
    public class Analyser
    {
        private readonly char[] _vowels = {'a', 'e', 'i', 'o', 'u', 'y'};
        private readonly char[] _consonants = {'b', 'c', 'd', 'f', 'g', 'h', 'j', 'k', 'l', 'm', 'n', 'p', 'q', 'r', 's', 't', 'v', 'z'}; // TODO w? x?
        private string text;
        private string[] sentences;
        private Dictionary<string, int> words;
        private Dictionary<char, int> characters;
        private int charactersWithSpaces,
            charactersWithoutSpaces,
            vowelsCount,
            consonantsCount,
            wordsCount,
            uniqueWordsCount;
        private double averageSentenceLength;

        public Analyser(string textToAnalyse)
        {
            this.text = textToAnalyse;
            charactersWithSpaces = charactersWithoutSpaces =
                vowelsCount = consonantsCount = wordsCount = uniqueWordsCount = 0;
            averageSentenceLength = 0;
            
            AnalyseText();
        }

        private void AnalyseText()
        {
            sentences = ExtractSentences();
            text = text.ToLower();
            
            charactersWithSpaces = CharacterCount(true);
            charactersWithoutSpaces = CharacterCount(false);
            vowelsCount = VowelsCount();
            consonantsCount = ConsonantsCount();
            
            
            words = Words(); // for -wf switch and counts
            foreach (var wordPair in words)
            {
                if (wordPair.Key.Any(char.IsNumber)) continue;
                wordsCount += wordPair.Value;
                ++uniqueWordsCount;
            }
            characters = Characters(); // for -cf switch
            averageSentenceLength = Math.Round(wordsCount / (double) sentences.Length, 2);
        }

        public void PrintAnalyses(bool characterFrequencies, bool wordFrequencies)
        {
            Console.WriteLine($"Number of characters (with spaces): {charactersWithSpaces}");
            Console.WriteLine($"Number of characters (no spaces): {charactersWithoutSpaces}");
            Console.WriteLine($"Number of vowels: {vowelsCount}");
            Console.WriteLine($"Number of consonants: {consonantsCount}");
            Console.WriteLine($"Number of words: {wordsCount}");
            Console.WriteLine($"Number of unique words: {uniqueWordsCount}");
            Console.WriteLine($"Number of sentences: {sentences.Length}");
            Console.WriteLine($"Average sentence length: {averageSentenceLength:0.00}");
            if (characterFrequencies)
            {
                Console.WriteLine();
                Console.WriteLine("Character frequencies:");
                foreach (var (key, value) in characters.OrderByDescending(c => c.Value))
                {
                    Console.WriteLine($"{key}: {value}x");
                }
            }
            if (wordFrequencies)
            {
                Console.WriteLine();
                Console.WriteLine("Word frequencies:");
                foreach (var (key, value) in words.OrderByDescending(w => w.Value))
                {
                    Console.WriteLine($"{key}: {value}x");
                }
            }
        }

        private string[] ExtractSentences()
        {
            var tempSentences = new List<string>();
            var lastIndex = 0;
            Console.WriteLine(text.ToCharArray());
            for (var i = 0; i < text.Length; i++) // Vet. Also... "5." 1.9. 
            {
                // Console.Write(text[i]);
                if (text[i] != '.') continue;
                if (i + 1 >= text.Length || 
                    char.IsUpper(text[i+1]) ||
                    text[i+1] == '.' && text[i+2] == '.' || 
                    text[i+1] == ' ' && char.IsUpper(text[i+2]) ||
                    text[i+1] == ' ' && char.IsNumber(text[i+2]))
                {
                    tempSentences.Add(text.Substring(lastIndex, i - lastIndex));
                    while (i + 1 < text.Length && text[i+1] == '.' )
                    {
                        ++i;
                    }
                    lastIndex = i + 2; // 2 because of ". " dot and space at the end of every sentence
                }
            }

            if (tempSentences.Count < 1)
            {
                tempSentences.Add(text);
            }
            return tempSentences.ToArray();
        }
        
        private bool IsIn(IEnumerable<char> arr, char letter)
        {
            return arr.Any(item => item == letter);
        }
        
        private int CharacterCount(bool spaces)
        {
            if (spaces)
            {
                return text.Length;
            }

            return text.Count(character => character != ' ');
        }
        
        private int VowelsCount()
        {
            var vowelCount = 0;
            var normalizedText = text.Normalize(NormalizationForm.FormD)
                .Where(ch => CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                .ToArray();
            
            for (var i = 0; i < normalizedText.Length; i++)
            {
                var letter = normalizedText[i];
                foreach (var vowel in _vowels)
                {
                    if (letter != vowel) continue;
                    if (letter == 'i' && i + 1 < normalizedText.Length && IsIn(new[]{'a', 'e', 'u'}, normalizedText[i + 1]))
                    {
                        ++i;
                    }
                    vowelCount++;
                }
            }

            return vowelCount;
        }
        
        private int ConsonantsCount()
        {
            var consonants = 0;
            var normalizedText = text.Normalize(NormalizationForm.FormD)
                .Where(ch => CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                .ToArray();

            for (var i = 0; i < normalizedText.Length; i++)
            {
                var letter = normalizedText[i];
                foreach (var consonant in _consonants)
                {
                    if (letter != consonant) continue;
                    if (letter == 'c' && i + 1 < normalizedText.Length && normalizedText[i + 1] == 'h')
                    {
                        i++;
                    }
                    consonants++;
                }
            }

            return consonants;
        }

        private Dictionary<string, int> Words()
        {
            var wordsDict = new Dictionary<string, int>();
            foreach (var sentence in sentences)
            {
                var tempWords = sentence.ToLower().Split(new[] {' ', ',', '\'', '.', '\"', '(', ')', '-'});
                foreach (var word in tempWords)
                {
                    if (IsNullOrWhiteSpace(word)) continue;
                    if (wordsDict.TryGetValue(word, out var count))
                    {
                        wordsDict[word] = count + 1;
                    }
                    else
                    {
                        wordsDict.Add(word, 1);
                    }
                }
            }

            return wordsDict;
        }

        private Dictionary<char, int> Characters()
        {
            var characterDict = new Dictionary<char, int>();
            foreach (var letter in text.Where(letter => char.IsLetter(letter)))
            {
                if (characterDict.TryGetValue(letter, out var count))
                {
                    characterDict[letter] = count + 1;
                }
                else
                {
                    characterDict.Add(letter, 1);
                }
            }

            return characterDict;
        }
    }
}