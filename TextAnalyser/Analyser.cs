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
        private readonly string _text;
        private Dictionary<string, int> words;
        private Dictionary<char, int> characters;
        private int charactersWithSpaces,
            charactersWithoutSpaces,
            vowelsCount,
            consonantsCount,
            wordsCount,
            sentencesCount;
        private double averageSentenceLength;

        public Analyser(string textToAnalyse)
        {
            this._text = textToAnalyse.ToLower();
            charactersWithSpaces = charactersWithoutSpaces =
                vowelsCount = consonantsCount = wordsCount = sentencesCount = 0;
            averageSentenceLength = 0;
            
            AnalyseText();
        }

        private void AnalyseText()
        {
            charactersWithSpaces = CharacterCount(true);
            charactersWithoutSpaces = CharacterCount(false);
            vowelsCount = VowelsCount();
            consonantsCount = ConsonantsCount();
            words = Words(); // for -wf switch and counts
            foreach (var wordPair in words)
            {
                wordsCount += wordPair.Value;
            }
            characters = Characters(); // for -cf switch
            sentencesCount = SentenceCount();
            averageSentenceLength = Math.Round(wordsCount / (double) sentencesCount, 2);
        }

        public void PrintAnalyses(bool characterFrequencies, bool wordFrequencies)
        {
            Console.WriteLine($"Number of characters (with spaces): {charactersWithSpaces}");
            Console.WriteLine($"Number of characters (no spaces): {charactersWithoutSpaces}");
            Console.WriteLine($"Number of vowels: {vowelsCount}");
            Console.WriteLine($"Number of consonants: {consonantsCount}");
            Console.WriteLine($"Number of words: {wordsCount}");
            Console.WriteLine($"Number of unique words: {words.Count}");
            Console.WriteLine($"Number of sentences: {sentencesCount}");
            Console.WriteLine($"Average sentence length: {averageSentenceLength.ToString("0.00")}");
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
        
        private bool IsIn(IEnumerable<char> arr, char letter)
        {
            return arr.Any(item => item == letter);
        }
        
        private int CharacterCount(bool spaces)
        {
            if (spaces)
            {
                return _text.Length;
            }

            return _text.Count(character => character != ' ');
        }
        
        private int VowelsCount()
        {
            var vowelCount = 0;
            var normalizedText = _text.Normalize(NormalizationForm.FormD)
                .Where(ch => CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                .ToArray();
            
            for (var i = 0; i < normalizedText.Length; i++)
            {
                var letter = normalizedText[i];
                foreach (var vowel in _vowels)
                {
                    if (letter != vowel) continue;
                    if (letter == 'i' && i + 1 < normalizedText.Length && IsIn(new char[]{'a', 'e', 'u'}, normalizedText[i + 1]))
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
            var normalizedText = _text.Normalize(NormalizationForm.FormD)
                .Where(ch => CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                .ToArray();
            // Console.WriteLine(normalizedText);

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

        private int SentenceCount()
        {
            return _text.Split('.').Count(sentence => sentence != "");
        }

        private Dictionary<string, int> Words()
        {
            var wordsDict = new Dictionary<string, int>();
            var sentences = _text.ToLower().Split('.'); // TODO case if '.' is used in date or ...
            foreach (var sentence in sentences)
            {
                if (IsNullOrEmpty(sentence)) continue;
                var subSentences = sentence.Split(',');
                foreach (var subSentence in subSentences)
                {
                    if (IsNullOrEmpty(subSentence)) continue;
                    var wordsInSubsentence = subSentence.Split(' ');
                    foreach (var word in wordsInSubsentence)
                    {
                        if (IsNullOrEmpty(word)) continue;
                        var apostropheWords = word.Split('\'');
                        foreach (var apostrophe in apostropheWords)
                        {
                            if (IsNullOrEmpty(apostrophe)) continue;
                            if (wordsDict.TryGetValue(apostrophe, out var count))
                            {
                                wordsDict[apostrophe] = count + 1;
                            }
                            else
                            {
                                wordsDict.Add(apostrophe, 1);
                            }
                        }
                    }
                }
            }

            return wordsDict;
        }

        private Dictionary<char, int> Characters()
        {
            var characterDict = new Dictionary<char, int>();
            foreach (var letter in _text.Where(letter => char.IsLetter(letter)))
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