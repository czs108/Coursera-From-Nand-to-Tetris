using System.Linq.Expressions;
using System.Runtime.InteropServices.ComTypes;
using System.Diagnostics;
using System.Collections.Generic;
using System;

namespace Project11
{
    public class CompilationEngine
    {
        private const string IF_TRUE_LBL = "IF_TRUE";

        private const string IF_FALSE_LBL = "IF_FALSE";

        private const string IF_END_LBL = "IF_END";

        private const string WHILE_START_LBL = "WHILE_START";

        private const string WHILE_END_LBL = "WHILE_END";

        private enum FunctionType
        {
            Method,
            Constructor,
            Function
        }

        private enum StackOperation
        {
            Push,
            Pop
        }

        private static readonly Dictionary<char, string> opMap =
            new Dictionary<char, string>()
        {
            { '+', "add" },
            { '-', "sub" },
            { '*', "Math.multiply" },
            { '/', "Math.divide" },
            { '&', "and" },
            { '|', "or" },
            { '<', "lt" },
            { '>', "gt" },
            { '=', "eq" }
        };

        private UniqueIndex labelIdx = new UniqueIndex();

        private Tokenizer tokenizer = null;

        private SymbolTable symTab = null;

        private VMWriter writer = null;

        private string className = null;

        public void Compile(Tokenizer tokenizer, VMWriter writer)
        {
            Debug.Assert(tokenizer != null);
            Debug.Assert(writer != null);

            this.tokenizer = tokenizer;
            this.writer = writer;
            if (tokenizer.HasMoreTokens())
            {
                symTab = new SymbolTable();
                tokenizer.Advance();
                CompileClass();
            }
        }

        private void CompileClass()
        {
            Debug.Assert(tokenizer.TypeOfToken() == TokenType.Keyword);
            Debug.Assert(tokenizer.TypeOfKeyword() == KeywordType.Class);

            // Name of class
            tokenizer.Advance();
            className = tokenizer.Identifier();

            // '{'
            tokenizer.Advance();

            tokenizer.Advance();
            while (true)
            {
                if (tokenizer.TypeOfToken() == TokenType.Symbol
                    && tokenizer.Symbol() == '}')
                {
                    break;
                }

                switch (tokenizer.TypeOfKeyword())
                {
                    case KeywordType.Constructor:
                    case KeywordType.Method:
                    case KeywordType.Function:
                    {
                        CompileSubroutineDec();
                        break;
                    }
                    case KeywordType.Static:
                    case KeywordType.Field:
                    {
                        CompileClassVarDec();
                        break;
                    }
                    default:
                    {
                        throw new FormatException(
                            " [!] Invalid syntax.");
                    }
                }
            }
        }

        private void CompileClassVarDec()
        {
            Debug.Assert(tokenizer.TypeOfToken() == TokenType.Keyword);
            Debug.Assert(tokenizer.TypeOfKeyword() == KeywordType.Static
                || tokenizer.TypeOfKeyword() == KeywordType.Field);

            // "static" | "field"
            IdentifierKind kind = GetKindOfClassVar();

            // Type of field
            tokenizer.Advance();
            string type = GetTypeOfIdentifier();

            do
            {
                // Name of field
                tokenizer.Advance();
                string name = tokenizer.Identifier();

                symTab.Define(name, type, kind);

                // ',' | ';'
                tokenizer.Advance();

            } while (tokenizer.Symbol() != ';');

            tokenizer.Advance();
        }

        private void CompileSubroutineDec()
        {
            Debug.Assert(tokenizer.TypeOfToken() == TokenType.Keyword);
            Debug.Assert(tokenizer.TypeOfKeyword() == KeywordType.Constructor
                || tokenizer.TypeOfKeyword() == KeywordType.Function
                || tokenizer.TypeOfKeyword() == KeywordType.Method);

            symTab.StartSubroutine();

            // "function" | "method" | "constructor"
            FunctionType type = GetTypeOfFunction();

            // Return type of function
            tokenizer.Advance();

            // Name of function
            tokenizer.Advance();
            string name = $"{className}.{tokenizer.Identifier()}";

            // '('
            tokenizer.Advance();

            // Paramter list
            CompileParamterList(type);

            // ')'
            tokenizer.Advance();

            CompileSubroutineBody(name, type);
        }

        private void CompileParamterList(FunctionType type)
        {
            if (type == FunctionType.Method)
            {
                symTab.Define("this", className, IdentifierKind.Arg);
            }

            while (true)
            {
                tokenizer.Advance();
                if (tokenizer.TypeOfToken() == TokenType.Symbol)
                {
                    if (tokenizer.Symbol() == ',')
                    {
                        // ','
                        tokenizer.Advance();
                    }
                    else
                    {
                        Debug.Assert(tokenizer.Symbol() == ')');
                        break;
                    }
                }

                // Type of parameter
                string argType = GetTypeOfIdentifier();

                // Name of parameter
                tokenizer.Advance();
                string argName = tokenizer.Identifier();

                symTab.Define(argName, argType, IdentifierKind.Arg);
            }
        }

        private void CompileSubroutineBody(string name, FunctionType type)
        {
            Debug.Assert(!String.IsNullOrWhiteSpace(name));

            // variables
            tokenizer.Advance();
            if (tokenizer.TypeOfToken() != TokenType.Symbol)
            {
                while (!IsStatement())
                {
                    CompileVarDec();
                }

                int localVarCount = symTab.VarCount(IdentifierKind.Var);
                writer.WriteFunction(name, localVarCount);

                if (type == FunctionType.Constructor)
                {
                    int fieldCount = symTab.VarCount(IdentifierKind.Field);
                    writer.WritePush("constant", fieldCount);
                    writer.WriteCall("Memory.alloc", 1);
                    // set this
                    writer.WritePop("pointer", 0);
                }
                else if (type == FunctionType.Method)
                {
                    // set this
                    writer.WritePush("argument", 0);
                    writer.WritePop("pointer", 0);
                }

                // statements
                CompileStatements();
            }

            tokenizer.Advance();
        }

        private void CompileVarDec()
        {
            Debug.Assert(tokenizer.TypeOfToken() == TokenType.Keyword);
            Debug.Assert(tokenizer.TypeOfKeyword() == KeywordType.Var);

            // Type of variable
            tokenizer.Advance();
            string type = GetTypeOfIdentifier();

            do
            {
                // Name of variable
                tokenizer.Advance();
                string name = tokenizer.Identifier();

                symTab.Define(name, type, IdentifierKind.Var);

                // ',' | ';'
                tokenizer.Advance();

            } while (tokenizer.Symbol() != ';');

            tokenizer.Advance();
        }

        private void CompileStatements()
        {
            Debug.Assert(IsStatement());

            do
            {
                switch (tokenizer.TypeOfKeyword())
                {
                    case KeywordType.Let:
                    {
                        CompileLet();
                        break;
                    }
                    case KeywordType.If:
                    {
                        CompileIf();
                        break;
                    }
                    case KeywordType.Do:
                    {
                        CompileDo();
                        break;
                    }
                    case KeywordType.While:
                    {
                        CompileWhile();
                        break;
                    }
                    case KeywordType.Return:
                    {
                        CompileReturn();
                        break;
                    }
                    default:
                    {
                        Debug.Assert(false);
                        break;
                    }
                }

            } while (IsStatement());
        }

        private void CompileLet()
        {
            Debug.Assert(tokenizer.TypeOfToken() == TokenType.Keyword);
            Debug.Assert(tokenizer.TypeOfKeyword() == KeywordType.Let);

            // Name of variable
            tokenizer.Advance();
            string name = tokenizer.Identifier();

            // '=' | '['
            tokenizer.Advance();
            if (tokenizer.Symbol() == '[')
            {
                // expression
                tokenizer.Advance();
                CompileExpression();

                // ']'
                tokenizer.Advance();

                WriteVariable(name, StackOperation.Push);
                writer.WriteArithmetic("add");

                // '='
                tokenizer.Advance();
                CompileExpression();

                writer.WritePop("temp", 0);
                writer.WritePop("pointer", 1);
                writer.WritePush("temp", 0);
                writer.WritePop("that", 0);
            }
            else
            {
                // '='
                tokenizer.Advance();
                CompileExpression();

                WriteVariable(name, StackOperation.Pop);
            }

            // ';'
            tokenizer.Advance();
        }

        private void CompileIf()
        {
            Debug.Assert(tokenizer.TypeOfToken() == TokenType.Keyword);
            Debug.Assert(tokenizer.TypeOfKeyword() == KeywordType.If);

            // '('
            tokenizer.Advance();

            // expression
            tokenizer.Advance();
            CompileExpression();

            int idx = labelIdx.Next();
            writer.WriteIf($"{IF_TRUE_LBL}{idx}");
            writer.WriteGoto($"{IF_FALSE_LBL}{idx}");

            // '{'
            tokenizer.Advance();

            writer.WriteLabel($"{IF_TRUE_LBL}{idx}");

            // statements
            tokenizer.Advance();
            CompileStatements();

            // '}'
            tokenizer.Advance();

            if (tokenizer.TypeOfToken() == TokenType.Keyword
                && tokenizer.TypeOfKeyword() == KeywordType.Else)
            {
                // "else"
                writer.WriteGoto($"{IF_END_LBL}{idx}");
                writer.WriteLabel($"{IF_FALSE_LBL}{idx}");

                // '{'
                tokenizer.Advance();

                // statements
                tokenizer.Advance();
                CompileStatements();

                // '}'
                tokenizer.Advance();

                writer.WriteLabel($"{IF_END_LBL}{idx}");
            }
            else
            {
                writer.WriteLabel($"{IF_FALSE_LBL}{idx}");
            }
        }

        private void CompileWhile()
        {
            Debug.Assert(tokenizer.TypeOfToken() == TokenType.Keyword);
            Debug.Assert(tokenizer.TypeOfKeyword() == KeywordType.While);

            int idx = labelIdx.Next();
            writer.WriteLabel($"{WHILE_START_LBL}{idx}");

            // '('
            tokenizer.Advance();

            // expression
            tokenizer.Advance();
            CompileExpression();

            writer.WriteArithmetic("not");
            writer.WriteIf($"{WHILE_END_LBL}{idx}");

            // '{'
            tokenizer.Advance();

            // statements
            tokenizer.Advance();
            CompileStatements();

            writer.WriteGoto($"{WHILE_START_LBL}{idx}");
            writer.WriteLabel($"{WHILE_END_LBL}{idx}");

            // '}'
            tokenizer.Advance();
        }

        private void CompileDo()
        {
            Debug.Assert(tokenizer.TypeOfToken() == TokenType.Keyword);
            Debug.Assert(tokenizer.TypeOfKeyword() == KeywordType.Do);

            bool isMethod = false;
            string objName = className;

            tokenizer.Advance();
            tokenizer.Advance();
            if (tokenizer.TypeOfToken() == TokenType.Symbol
                && tokenizer.Symbol() == '.')
            {
                // Name of class or variable
                tokenizer.Backward();
                objName = tokenizer.Identifier();
                if (symTab.Contain(objName))
                {
                    isMethod = true;
                }

                // '.'
                tokenizer.Advance();
                tokenizer.Advance();
            }
            else
            {
                tokenizer.Backward();
            }

            CompileSubroutineCall(objName, isMethod);
            writer.WritePop("temp", 0);

            tokenizer.Advance();
        }

        private void CompileReturn()
        {
            Debug.Assert(tokenizer.TypeOfToken() == TokenType.Keyword);
            Debug.Assert(tokenizer.TypeOfKeyword() == KeywordType.Return);

            // expression
            tokenizer.Advance();
            if (tokenizer.TypeOfToken() != TokenType.Symbol)
            {
                CompileExpression();
            }
            else
            {
                writer.WritePush("constant", 0);
            }

            writer.WriteReturn();

            tokenizer.Advance();
        }

        private void CompileExpression()
        {
            CompileTerm();

            while (tokenizer.TypeOfToken() == TokenType.Symbol
                && opMap.ContainsKey(tokenizer.Symbol()))
            {
                // operator
                char op = tokenizer.Symbol();
                tokenizer.Advance();

                CompileTerm();

                if (op == '*' || op == '/')
                {
                    writer.WriteCall(opMap[op], 2);
                }
                else
                {
                    writer.WriteArithmetic(opMap[op]);
                }
            }
        }

        private void CompileTerm()
        {
            switch (tokenizer.TypeOfToken())
            {
                case TokenType.Identifier:
                {
                    CompileIdentifierTerm();
                    break;
                }
                case TokenType.Symbol:
                {
                    CompileSymbolTerm();
                    break;
                }
                case TokenType.Keyword:
                {
                    CompileKeywordTerm();
                    break;
                }
                case TokenType.ConstInteger:
                {
                    writer.WritePush("constant", tokenizer.IntegerValue());
                    tokenizer.Advance();
                    break;
                }
                case TokenType.ConstString:
                {
                    WriteConstString(tokenizer.StringValue());
                    tokenizer.Advance();
                    break;
                }
                default:
                {
                    throw new FormatException(
                        " [!] Invalid syntax.");
                }
            }
        }

        private int CompileExpressionList()
        {
            int count = 0;
            while (true)
            {
                if (tokenizer.TypeOfToken() == TokenType.Symbol)
                {
                    if (tokenizer.Symbol() == ',')
                    {
                        // ','
                        tokenizer.Advance();
                    }
                    else if (tokenizer.Symbol() == ')')
                    {
                        break;
                    }
                }

                // expression
                CompileExpression();
                ++count;
            }

            return count;
        }

        private void CompileKeywordTerm()
        {
            Debug.Assert(tokenizer.TypeOfToken() == TokenType.Keyword);

            switch (tokenizer.Keyword())
            {
                case "null":
                case "false":
                {
                    writer.WritePush("constant", 0);
                    break;
                }
                case "true":
                {
                    writer.WritePush("constant", 0);
                    writer.WriteArithmetic("not");
                    break;
                }
                case "this":
                {
                    writer.WritePush("pointer", 0);
                    break;
                }
                default:
                {
                    throw new FormatException(
                        " [!] Invalid syntax.");
                }
            }

            tokenizer.Advance();
        }

        private void CompileIdentifierTerm()
        {
            Debug.Assert(tokenizer.TypeOfToken() == TokenType.Identifier);

            string name = tokenizer.Identifier();
            tokenizer.Advance();
            if (tokenizer.TypeOfToken() != TokenType.Symbol)
            {
                WriteVariable(name, StackOperation.Push);
                return;
            }

            char sym = tokenizer.Symbol();
            tokenizer.Advance();
            switch (sym)
            {
                case '(':
                {
                    tokenizer.Backward();
                    tokenizer.Backward();
                    // continue...
                    goto case '.';
                }
                case '.':
                {
                    bool isMethod = false;
                    if (symTab.Contain(name))
                    {
                        isMethod = true;
                    }

                    CompileSubroutineCall(name, isMethod);
                    break;
                }
                case '[':
                {
                    // Name of array
                    CompileExpression();
                    WriteVariable(name, StackOperation.Push);
                    writer.WriteArithmetic("add");
                    writer.WritePop("pointer", 1);
                    writer.WritePush("that", 0);
                    tokenizer.Advance();
                    break;
                }
                default:
                {
                    tokenizer.Backward();
                    WriteVariable(name, StackOperation.Push);
                    break;
                }
            }
        }

        private void CompileSymbolTerm()
        {
            Debug.Assert(tokenizer.TypeOfToken() == TokenType.Symbol);

            // '(' | '-' | '~'
            char sym = tokenizer.Symbol();
            tokenizer.Advance();
            switch (sym)
            {
                case '(':
                {
                    CompileExpression();
                    tokenizer.Advance();
                    break;
                }
                case '-':
                {
                    CompileTerm();
                    writer.WriteArithmetic("neg");
                    break;
                }
                case '~':
                {
                    CompileTerm();
                    writer.WriteArithmetic("not");
                    break;
                }
                default:
                {
                    throw new FormatException(
                        " [!] Invalid syntax.");
                }
            }
        }

        private void CompileSubroutineCall(string objName, bool isMethod)
        {
            Debug.Assert(!String.IsNullOrWhiteSpace(objName));
            Debug.Assert(tokenizer.TypeOfToken() == TokenType.Identifier);

            // Name of function
            string name = $"{objName}.{tokenizer.Identifier()}";
            if (isMethod)
            {
                WriteVariable(objName, StackOperation.Push);
            }

            // '('
            tokenizer.Advance();

            // Parameter list
            tokenizer.Advance();
            int argCount = CompileExpressionList();

            writer.WriteCall(name, argCount);

            tokenizer.Advance();
        }

        private void WriteConstString(string content)
        {
            Debug.Assert(content != null);

            writer.WritePush("constant", content.Length);
            writer.WriteCall("String.new", 1);
            foreach (char c in content)
            {
                writer.WritePush("constant", Convert.ToInt32(c));
                writer.WriteCall("String.appendChar", 2);
            }
        }

        private void WriteVariable(string name, StackOperation op)
        {
            Debug.Assert(!String.IsNullOrWhiteSpace(name));

            string segment = null;
            switch (symTab.KindOf(name))
            {
                case IdentifierKind.Static:
                {
                    segment = "static";
                    break;
                }
                case IdentifierKind.Field:
                {
                    segment = "this";
                    break;
                }
                case IdentifierKind.Arg:
                {
                    segment = "argument";
                    break;
                }
                case IdentifierKind.Var:
                {
                    segment = "local";
                    break;
                }
                default:
                {
                    Debug.Assert(false);
                    break;
                }
            }

            if (op == StackOperation.Push)
            {
                writer.WritePush(segment, symTab.IndexOf(name));
            }
            else
            {
                writer.WritePop(segment, symTab.IndexOf(name));
            }
        }

        private bool IsStatement()
        {
            return tokenizer.TypeOfToken() == TokenType.Keyword
                && (tokenizer.TypeOfKeyword() == KeywordType.Let
                || tokenizer.TypeOfKeyword() == KeywordType.If
                || tokenizer.TypeOfKeyword() == KeywordType.While
                || tokenizer.TypeOfKeyword() == KeywordType.Do
                || tokenizer.TypeOfKeyword() == KeywordType.Return);
        }

        private FunctionType GetTypeOfFunction()
        {
            Debug.Assert(tokenizer.TypeOfToken() == TokenType.Keyword);

            switch (tokenizer.Keyword())
            {
                case "method":
                    return FunctionType.Method;
                case "function":
                    return FunctionType.Function;
                case "constructor":
                    return FunctionType.Constructor;
                default:
                {
                    throw new FormatException(
                        " [!] Invalid syntax.");
                }
            }
        }

        private string GetTypeOfIdentifier()
        {
            if (tokenizer.TypeOfToken() == TokenType.Keyword)
            {
                return tokenizer.Keyword();
            }
            else if (tokenizer.TypeOfToken() == TokenType.Identifier)
            {
                return tokenizer.Identifier();
            }
            else
            {
                throw new FormatException(
                    " [!] Invalid syntax.");
            }
        }

        private IdentifierKind GetKindOfClassVar()
        {
            Debug.Assert(tokenizer.TypeOfToken() == TokenType.Keyword);

            switch (tokenizer.Keyword())
            {
                case "static":
                    return IdentifierKind.Static;
                case "field":
                    return IdentifierKind.Field;
                default:
                {
                    throw new FormatException(
                        " [!] Invalid syntax.");
                }
            }
        }
    }
}