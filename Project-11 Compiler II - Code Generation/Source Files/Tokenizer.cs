using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System;

namespace Project11
{
    public class Tokenizer : IDisposable
    {
        private struct Token
        {
            public string Content { get; }

            public TokenType Type { get; }

            public Token(string content, TokenType type)
            {
                Debug.Assert(!String.IsNullOrWhiteSpace(content));

                Content = content;
                Type = type;
            }
        }

        private const string RGX_KEYWORD = @"(class|constructor|function|method|field|static|var|int|char|boolean|void|true|false|null|this|let|do|if|else|while|return)";

        private const string RGX_SYMBOL = @"([{}()[\].,;+\-*/&|<>=~])";

        private const string RGX_CONST_INTEGER = @"(\d+)";

        private const string RGX_CONST_STRING = "\"([^\n]*?)\"";

        private const string RGX_IDENTIFIER = @"([A-Za-z_]\w*)";

        private static readonly string RGX_LEXICAL_ELEMENTS = $"{RGX_KEYWORD}|{RGX_SYMBOL}|{RGX_CONST_INTEGER}|{RGX_CONST_STRING}|{RGX_IDENTIFIER}";

        private static readonly Dictionary<string, KeywordType> keywordTypeMap
            = new Dictionary<string, KeywordType>()
        {
            { "class", KeywordType.Class },
            { "method", KeywordType.Method },
            { "function", KeywordType.Function },
            { "constructor", KeywordType.Constructor },
            { "int", KeywordType.Int },
            { "boolean", KeywordType.Boolean },
            { "char", KeywordType.Char },
            { "void", KeywordType.Void },
            { "var", KeywordType.Var },
            { "static", KeywordType.Static },
            { "field", KeywordType.Field },
            { "let", KeywordType.Let },
            { "do", KeywordType.Do },
            { "if", KeywordType.If },
            { "else", KeywordType.Else },
            { "while", KeywordType.While },
            { "return", KeywordType.Return },
            { "true", KeywordType.True },
            { "false", KeywordType.False },
            { "null", KeywordType.Null },
            { "this", KeywordType.This }
        };

        private StreamReader inputFile = null;

        private int tokenIdx = -1;

        private List<Token> tokens = new List<Token>();

        private Token CurrentToken
        {
            get
            {
                Debug.Assert(0 <= tokenIdx && tokenIdx < tokens.Count);

                return tokens[tokenIdx];
            }
        }

        public Tokenizer(string filename)
        {
            Debug.Assert(!String.IsNullOrWhiteSpace(filename));

            inputFile = new StreamReader(filename);
            tokens = Tokenize(inputFile);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // NOTE: Leave out the finalizer altogether if this class doesn't
        // own unmanaged resources itself, but leave the other methods
        // exactly as they are.
        ~Tokenizer()
        {
            // Finalizer calls Dispose(false)
            Dispose(false);
        }

        public bool HasMoreTokens() => tokens.Count != 0 && tokenIdx < tokens.Count;

        public void Advance()
        {
            Debug.Assert(HasMoreTokens());

            ++tokenIdx;
        }

        public void Backward()
        {
            Debug.Assert(tokenIdx != 0);

            --tokenIdx;
        }

        public TokenType TypeOfToken() => CurrentToken.Type;

        public KeywordType TypeOfKeyword() => GetKeywordType(Keyword());

        public string Keyword()
        {
            Debug.Assert(TypeOfToken() == TokenType.Keyword);

            return CurrentToken.Content;
        }

        public char Symbol()
        {
            Debug.Assert(TypeOfToken() == TokenType.Symbol);

            return CurrentToken.Content[0];
        }

        public string Identifier()
        {
            Debug.Assert(TypeOfToken() == TokenType.Identifier);

            return CurrentToken.Content;
        }

        public int IntegerValue()
        {
            Debug.Assert(TypeOfToken() == TokenType.ConstInteger);

            return Convert.ToInt32(CurrentToken.Content);
        }

        public string StringValue()
        {
            Debug.Assert(TypeOfToken() == TokenType.ConstString);

            return CurrentToken.Content;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Free managed resources
                inputFile?.Close();
                inputFile = null;
            }
        }

        private static List<Token> Tokenize(StreamReader file)
        {
            Debug.Assert(file != null);

            string content = RemoveUnusefulContent(file.ReadToEnd());
            if (String.IsNullOrEmpty(content))
            {
                return new List<Token>();
            }

            List<Token> tokens = new List<Token>();
            List<string> elements = RemoveWhitespace(
                Regex.Split(content, RGX_LEXICAL_ELEMENTS, RegexOptions.IgnoreCase));
            foreach (var element in elements)
            {
                tokens.Add(new Token(element, GetTokenType(element)));
            }

            return tokens;
        }

        private static KeywordType GetKeywordType(string keyword)
        {
            Debug.Assert(GetTokenType(keyword) == TokenType.Keyword);

            return keywordTypeMap[keyword];
        }

        private static TokenType GetTokenType(string token)
        {
            Debug.Assert(!String.IsNullOrWhiteSpace(token));

            if (Regex.IsMatch(token, $"^{RGX_KEYWORD}$"))
            {
                return TokenType.Keyword;
            }
            else if (Regex.IsMatch(token,  $"^{RGX_SYMBOL}$"))
            {
                return TokenType.Symbol;
            }
            else if (Regex.IsMatch(token, $"^{RGX_IDENTIFIER}$"))
            {
                return TokenType.Identifier;
            }
            else if (Regex.IsMatch(token, $"^{RGX_CONST_INTEGER}$"))
            {
                return TokenType.ConstInteger;
            }
            else
            {
                Debug.Assert(Regex.IsMatch($"\"{token}\"", $"^{RGX_CONST_STRING}$"));

                return TokenType.ConstString;
            }
        }

        private static List<string> RemoveWhitespace(string[] content)
        {
            Debug.Assert(content != null);

            List<string> result = new List<string>();
            foreach (var i in content)
            {
                if (!String.IsNullOrWhiteSpace(i))
                {
                    result.Add(i);
                }
            }

            return result;
        }

        private static string RemoveUnusefulContent(string content)
        {
            if (String.IsNullOrWhiteSpace(content))
            {
                return String.Empty;
            }

            content = CodeTrim.RemoveComment(content);
            content = CodeTrim.RemoveNewLine(content);
            return CodeTrim.RemoveMultispace(content);
        }
    }
}