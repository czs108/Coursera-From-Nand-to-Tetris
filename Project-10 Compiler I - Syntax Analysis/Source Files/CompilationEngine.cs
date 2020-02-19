using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Xml;
using System;

namespace Project10
{
    public class CompilationEngine
    {
        private const string NEW_LINE = "\n";

        private const string EMPTY = "EMPTY";

        private const string XML_EMPTY = @"<EMPTY />";

        private const string STRING_CONSTANT = "stringConstant";

        private const string INTEGER_CONSTANT = "integerConstant";

        private const string KEYWORD = "keyword";

        private const string IDENTIFIER = "identifier";

        private const string SYMBOL = "symbol";

        private const string CLASS = "class";

        private const string CLASS_VAR_DEC = "classVarDec";

        private const string PARAMETER_LIST = "parameterList";

        private const string SUBROUTINE_DEC = "subroutineDec";

        private const string SUBROUTINE_BODY = "subroutineBody";

        private const string TERM = "term";

        private const string VAR_DEC = "varDec";

        private const string EXPRESSION = "expression";

        private const string EXPRESSION_LIST = "expressionList";

        private const string STATEMENTS = "statements";

        private const string LET_STATEMENT = "letStatement";

        private const string DO_STATEMENT = "doStatement";

        private const string RETURN_STATEMENT = "returnStatement";

        private const string IF_STATEMENT = "ifStatement";

        private const string WHILE_STATEMENT = "whileStatement";

        private static readonly HashSet<char> opList = new HashSet<char>()
        {
            '+', '-', '*', '/', '&', '|', '<', '>', '='
        };

        private Tokenizer tokenizer = null;

        private XmlDocument xmlDoc = null;

        public void Compile(Tokenizer tokenizer, string outputFile)
        {
            Debug.Assert(tokenizer != null);
            Debug.Assert(!String.IsNullOrWhiteSpace(outputFile));

            this.tokenizer = tokenizer;
            if (tokenizer.HasMoreTokens())
            {
                xmlDoc = new XmlDocument();
                tokenizer.Advance();
                CompileClass();
                WriteXmlToFile(outputFile);
            }
        }

        private void CompileClass()
        {
            Debug.Assert(tokenizer.TypeOfToken() == TokenType.Keyword);
            Debug.Assert(tokenizer.TypeOfKeyword() == KeywordType.Class);

            // <class>
            XmlElement newClass = CreateXmlElement(CLASS);

            // "class"
            newClass.AppendChild(CreateXmlElement(KEYWORD, "class"));

            // Name of class
            tokenizer.Advance();
            newClass.AppendChild(CreateXmlElement(IDENTIFIER, tokenizer.Identifier()));

            // '{'
            tokenizer.Advance();
            newClass.AppendChild(CreateXmlElement(SYMBOL, "{"));

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
                        newClass.AppendChild(CompileSubroutineDec());
                        break;
                    }
                    case KeywordType.Static:
                    case KeywordType.Field:
                    {
                        newClass.AppendChild(CompileClassVarDec());
                        break;
                    }
                    default:
                    {
                        throw new FormatException(
                            " [!] Invalid syntax.");
                    }
                }
            }

            // '}'
            newClass.AppendChild(CreateXmlElement(SYMBOL, "}"));

            xmlDoc.AppendChild(newClass);
        }

        private XmlElement CompileClassVarDec()
        {
            Debug.Assert(tokenizer.TypeOfToken() == TokenType.Keyword);
            Debug.Assert(tokenizer.TypeOfKeyword() == KeywordType.Static
                || tokenizer.TypeOfKeyword() == KeywordType.Field);

            // <classVarDec>
            XmlElement varDec = CreateXmlElement(CLASS_VAR_DEC);

            // "static" | "field"
            varDec.AppendChild(CreateXmlElement(KEYWORD, tokenizer.Keyword()));

            // Type of field
            tokenizer.Advance();
            varDec.AppendChild(CreateXmlTypeElement());

            do
            {
                // Name of field
                tokenizer.Advance();
                varDec.AppendChild(CreateXmlElement(IDENTIFIER, tokenizer.Identifier()));

                // ',' | ';'
                tokenizer.Advance();
                varDec.AppendChild(CreateXmlElement(SYMBOL, $"{tokenizer.Symbol()}"));

            } while (tokenizer.Symbol() != ';');

            tokenizer.Advance();
            return varDec;
        }

        private XmlElement CompileSubroutineDec()
        {
            Debug.Assert(tokenizer.TypeOfToken() == TokenType.Keyword);
            Debug.Assert(tokenizer.TypeOfKeyword() == KeywordType.Constructor
                || tokenizer.TypeOfKeyword() == KeywordType.Function
                || tokenizer.TypeOfKeyword() == KeywordType.Method);

            // <subroutineDec>
            XmlElement routineDec = CreateXmlElement(SUBROUTINE_DEC);

            // "function" | "method" | "constructor"
            routineDec.AppendChild(CreateXmlElement(KEYWORD, tokenizer.Keyword()));

            // Return type of function
            tokenizer.Advance();
            routineDec.AppendChild(CreateXmlTypeElement());

            // Name of function
            tokenizer.Advance();
            routineDec.AppendChild(CreateXmlElement(IDENTIFIER, tokenizer.Identifier()));

            // '('
            tokenizer.Advance();
            routineDec.AppendChild(CreateXmlElement(SYMBOL, "("));

            // Paramter list
            routineDec.AppendChild(CompileParamterList());

            // ')'
            routineDec.AppendChild(CreateXmlElement(SYMBOL, ")"));

            tokenizer.Advance();
            routineDec.AppendChild(CompileSubroutineBody());

            return routineDec;
        }

        private XmlElement CompileParamterList()
        {
            // <parameterList>
            XmlElement paramList = CreateXmlElement(PARAMETER_LIST);

            paramList.AppendChild(CreateXmlElement(EMPTY));

            while (true)
            {
                tokenizer.Advance();
                if (tokenizer.TypeOfToken() == TokenType.Symbol)
                {
                    if (tokenizer.Symbol() == ',')
                    {
                        // ','
                        paramList.AppendChild(CreateXmlElement(SYMBOL, ","));
                        tokenizer.Advance();
                    }
                    else
                    {
                        Debug.Assert(tokenizer.Symbol() == ')');
                        break;
                    }
                }

                // Type of parameter
                paramList.AppendChild(CreateXmlTypeElement());

                // Name of parameter
                tokenizer.Advance();
                paramList.AppendChild(CreateXmlElement(IDENTIFIER, tokenizer.Identifier()));
            }

            return paramList;
        }

        private XmlElement CompileSubroutineBody()
        {
            // <subroutineBody>
            XmlElement routineBody = CreateXmlElement(SUBROUTINE_BODY);

            // '{'
            routineBody.AppendChild(CreateXmlElement(SYMBOL, "{"));

            // variables
            tokenizer.Advance();
            if (tokenizer.TypeOfToken() != TokenType.Symbol)
            {
                while (!IsStatement())
                {
                    routineBody.AppendChild(CompileVarDec());
                }

                // statements
                routineBody.AppendChild(CompileStatements());
            }

            // '}'
            routineBody.AppendChild(CreateXmlElement(SYMBOL, "}"));

            tokenizer.Advance();
            return routineBody;
        }

        private XmlElement CompileVarDec()
        {
            Debug.Assert(tokenizer.TypeOfToken() == TokenType.Keyword);
            Debug.Assert(tokenizer.TypeOfKeyword() == KeywordType.Var);

            // <varDec>
            XmlElement varDec = CreateXmlElement(VAR_DEC);

            // "var"
            varDec.AppendChild(CreateXmlElement(KEYWORD, "var"));

            // Type of variable
            tokenizer.Advance();
            varDec.AppendChild(CreateXmlTypeElement());

            do
            {
                // Name of variable
                tokenizer.Advance();
                varDec.AppendChild(CreateXmlElement(IDENTIFIER, tokenizer.Identifier()));

                // ',' | ';'
                tokenizer.Advance();
                varDec.AppendChild(CreateXmlElement(SYMBOL, $"{tokenizer.Symbol()}"));

            } while (tokenizer.Symbol() != ';');

            tokenizer.Advance();
            return varDec;
        }

        private XmlElement CompileStatements()
        {
            Debug.Assert(IsStatement());

            // <statements>
            XmlElement statements = CreateXmlElement(STATEMENTS);

            do
            {
                switch (tokenizer.TypeOfKeyword())
                {
                    case KeywordType.Let:
                    {
                        statements.AppendChild(CompileLet());
                        break;
                    }
                    case KeywordType.If:
                    {
                        statements.AppendChild(CompileIf());
                        break;
                    }
                    case KeywordType.Do:
                    {
                        statements.AppendChild(CompileDo());
                        break;
                    }
                    case KeywordType.While:
                    {
                        statements.AppendChild(CompileWhile());
                        break;
                    }
                    case KeywordType.Return:
                    {
                        statements.AppendChild(CompileReturn());
                        break;
                    }
                    default:
                    {
                        Debug.Assert(false);
                        break;
                    }
                }

            } while (IsStatement());

            return statements;
        }

        private XmlElement CompileLet()
        {
            Debug.Assert(tokenizer.TypeOfToken() == TokenType.Keyword);
            Debug.Assert(tokenizer.TypeOfKeyword() == KeywordType.Let);

            // <letStatement>
            XmlElement letStat = CreateXmlElement(LET_STATEMENT);

            // "let"
            letStat.AppendChild(CreateXmlElement(KEYWORD, "let"));

            // Name of variable
            tokenizer.Advance();
            letStat.AppendChild(CreateXmlElement(IDENTIFIER, tokenizer.Identifier()));

            // '=' | '['
            tokenizer.Advance();
            if (tokenizer.Symbol() == '[')
            {
                // '['
                letStat.AppendChild(CreateXmlElement(SYMBOL, "["));

                // expression
                tokenizer.Advance();
                letStat.AppendChild(CompileExpression());

                // ']'
                letStat.AppendChild(CreateXmlElement(SYMBOL, "]"));
                tokenizer.Advance();
            }

            // '='
            letStat.AppendChild(CreateXmlElement(SYMBOL, "="));

            tokenizer.Advance();
            letStat.AppendChild(CompileExpression());

            // ';'
            letStat.AppendChild(CreateXmlElement(SYMBOL, ";"));

            tokenizer.Advance();
            return letStat;
        }

        private XmlElement CompileIf()
        {
            Debug.Assert(tokenizer.TypeOfToken() == TokenType.Keyword);
            Debug.Assert(tokenizer.TypeOfKeyword() == KeywordType.If);

            XmlElement ifStat = CompileConditionalStatements();

            if (tokenizer.TypeOfToken() == TokenType.Keyword
                && tokenizer.TypeOfKeyword() == KeywordType.Else)
            {
                // "else"
                ifStat.AppendChild(CreateXmlElement(KEYWORD, "else"));

                // '{'
                tokenizer.Advance();
                ifStat.AppendChild(CreateXmlElement(SYMBOL, "{"));

                // statements
                tokenizer.Advance();
                ifStat.AppendChild(CompileStatements());

                // '}'
                ifStat.AppendChild(CreateXmlElement(SYMBOL, "}"));
                tokenizer.Advance();
            }

            return ifStat;
        }

        private XmlElement CompileWhile()
        {
            Debug.Assert(tokenizer.TypeOfToken() == TokenType.Keyword);
            Debug.Assert(tokenizer.TypeOfKeyword() == KeywordType.While);

            return CompileConditionalStatements();
        }

        private XmlElement CompileDo()
        {
            Debug.Assert(tokenizer.TypeOfToken() == TokenType.Keyword);
            Debug.Assert(tokenizer.TypeOfKeyword() == KeywordType.Do);

            // <doStatement>
            XmlElement doStat = CreateXmlElement(DO_STATEMENT);

            // "do"
            doStat.AppendChild(CreateXmlElement(KEYWORD, "do"));

            tokenizer.Advance();
            tokenizer.Advance();
            if (tokenizer.TypeOfToken() == TokenType.Symbol
                && tokenizer.Symbol() == '.')
            {
                // Name of class or variable
                tokenizer.Backward();
                doStat.AppendChild(CreateXmlElement(IDENTIFIER, tokenizer.Identifier()));

                // '.'
                tokenizer.Advance();
                doStat.AppendChild(CreateXmlElement(SYMBOL, "."));

                tokenizer.Advance();
            }
            else
            {
                tokenizer.Backward();
            }

            CompileSubroutineCall(doStat);

            // ';'
            doStat.AppendChild(CreateXmlElement(SYMBOL, ";"));

            tokenizer.Advance();
            return doStat;
        }

        private XmlElement CompileReturn()
        {
            Debug.Assert(tokenizer.TypeOfToken() == TokenType.Keyword);
            Debug.Assert(tokenizer.TypeOfKeyword() == KeywordType.Return);

            // <returnStatement>
            XmlElement retStat = CreateXmlElement(RETURN_STATEMENT);

            // "return"
            retStat.AppendChild(CreateXmlElement(KEYWORD, "return"));

            // expression
            tokenizer.Advance();
            if (tokenizer.TypeOfToken() != TokenType.Symbol)
            {
                retStat.AppendChild(CompileExpression());
            }

            // ';'
            retStat.AppendChild(CreateXmlElement(SYMBOL, ";"));

            tokenizer.Advance();
            return retStat;
        }

        private XmlElement CompileExpression()
        {
            // <expression>
            XmlElement exp = CreateXmlElement(EXPRESSION);

            while (true)
            {
                exp.AppendChild(CompileTerm());

                if (tokenizer.TypeOfToken() != TokenType.Symbol
                    || !opList.Contains(tokenizer.Symbol()))
                {
                    break;
                }
                else
                {
                    // operator
                    exp.AppendChild(CreateXmlElement(SYMBOL, $"{tokenizer.Symbol()}"));
                    tokenizer.Advance();
                }
            }

            return exp;
        }

        private XmlElement CompileTerm()
        {
            // <term>
            XmlElement term = CreateXmlElement(TERM);

            switch (tokenizer.TypeOfToken())
            {
                case TokenType.Identifier:
                {
                    CompileIdentifierTerm(term);
                    break;
                }
                case TokenType.Symbol:
                {
                    CompileSymbolTerm(term);
                    break;
                }
                case TokenType.Keyword:
                {
                    term.AppendChild(CreateXmlElement(KEYWORD, tokenizer.Keyword()));
                    tokenizer.Advance();
                    break;
                }
                case TokenType.ConstInteger:
                {
                    term.AppendChild(CreateXmlElement(INTEGER_CONSTANT, tokenizer.IntegerValue().ToString()));
                    tokenizer.Advance();
                    break;
                }
                case TokenType.ConstString:
                {
                    term.AppendChild(CreateXmlElement(STRING_CONSTANT, tokenizer.StringValue()));
                    tokenizer.Advance();
                    break;
                }
                default:
                {
                    throw new FormatException(
                        " [!] Invalid syntax.");
                }
            }

            return term;
        }

        private XmlElement CompileExpressionList()
        {
            // <expressionList>
            XmlElement expList = CreateXmlElement(EXPRESSION_LIST);

            expList.AppendChild(CreateXmlElement(EMPTY));

            while (true)
            {
                if (tokenizer.TypeOfToken() == TokenType.Symbol)
                {
                    if (tokenizer.Symbol() == ',')
                    {
                        // ','
                        expList.AppendChild(CreateXmlElement(SYMBOL, ","));
                        tokenizer.Advance();
                    }
                    else if (tokenizer.Symbol() == ')')
                    {
                        break;
                    }
                }

                // expression
                expList.AppendChild(CompileExpression());
            }

            return expList;
        }

        private void CompileIdentifierTerm(XmlElement parent)
        {
            Debug.Assert(parent != null);
            Debug.Assert(tokenizer.TypeOfToken() == TokenType.Identifier);

            string name = tokenizer.Identifier();
            tokenizer.Advance();
            if (tokenizer.TypeOfToken() != TokenType.Symbol)
            {
                parent.AppendChild(CreateXmlElement(IDENTIFIER, name));
                return;
            }

            char sym = tokenizer.Symbol();
            tokenizer.Advance();
            switch (sym)
            {
                case '.':
                {
                    // Name of class or variable
                    parent.AppendChild(CreateXmlElement(IDENTIFIER, name));
                    parent.AppendChild(CreateXmlElement(SYMBOL, "."));
                    CompileSubroutineCall(parent);
                    break;
                }
                case '(':
                {
                    tokenizer.Backward();
                    tokenizer.Backward();
                    CompileSubroutineCall(parent);
                    break;
                }
                case '[':
                {
                    // Name of array
                    parent.AppendChild(CreateXmlElement(IDENTIFIER, name));
                    parent.AppendChild(CreateXmlElement(SYMBOL, "["));
                    parent.AppendChild(CompileExpression());
                    parent.AppendChild(CreateXmlElement(SYMBOL, "]"));
                    tokenizer.Advance();
                    break;
                }
                default:
                {
                    tokenizer.Backward();
                    parent.AppendChild(CreateXmlElement(IDENTIFIER, name));
                    break;
                }
            }
        }

        private void CompileSymbolTerm(XmlElement parent)
        {
            Debug.Assert(parent != null);
            Debug.Assert(tokenizer.TypeOfToken() == TokenType.Symbol);

            // '(' | '-' | '~'
            char sym = tokenizer.Symbol();
            parent.AppendChild(CreateXmlElement(SYMBOL, $"{sym}"));
            tokenizer.Advance();

            switch (sym)
            {
                case '(':
                {
                    parent.AppendChild(CompileExpression());
                    parent.AppendChild(CreateXmlElement(SYMBOL, ")"));
                    tokenizer.Advance();
                    break;
                }
                case '-':
                case '~':
                {
                    parent.AppendChild(CompileTerm());
                    break;
                }
                default:
                {
                    throw new FormatException(
                        " [!] Invalid syntax.");
                }
            }
        }

        private void CompileSubroutineCall(XmlElement parent)
        {
            Debug.Assert(parent != null);
            Debug.Assert(tokenizer.TypeOfToken() == TokenType.Identifier);

            // Name of function
            parent.AppendChild(CreateXmlElement(IDENTIFIER, tokenizer.Identifier()));

            // '('
            tokenizer.Advance();
            parent.AppendChild(CreateXmlElement(SYMBOL, "("));

            // Parameter list
            tokenizer.Advance();
            parent.AppendChild(CompileExpressionList());

            // ')'
            parent.AppendChild(CreateXmlElement(SYMBOL, ")"));

            tokenizer.Advance();
        }

        private XmlElement CompileConditionalStatements()
        {
            Debug.Assert(tokenizer.TypeOfToken() == TokenType.Keyword);
            Debug.Assert(tokenizer.TypeOfKeyword() == KeywordType.If
                || tokenizer.TypeOfKeyword() == KeywordType.While);

            // <ifStatement> | <whileStatement>
            XmlElement stat = null;
            if (tokenizer.TypeOfKeyword() == KeywordType.If)
            {
                stat = CreateXmlElement(IF_STATEMENT);
                stat.AppendChild(CreateXmlElement(KEYWORD, "if"));
            }
            else
            {
                stat = CreateXmlElement(WHILE_STATEMENT);
                stat.AppendChild(CreateXmlElement(KEYWORD, "while"));
            }

            // '('
            tokenizer.Advance();
            stat.AppendChild(CreateXmlElement(SYMBOL, "("));

            // expression
            tokenizer.Advance();
            stat.AppendChild(CompileExpression());

            // ')'
            stat.AppendChild(CreateXmlElement(SYMBOL, ")"));

            // '{'
            tokenizer.Advance();
            stat.AppendChild(CreateXmlElement(SYMBOL, "{"));

            // statements
            tokenizer.Advance();
            stat.AppendChild(CompileStatements());

            // '}'
            stat.AppendChild(CreateXmlElement(SYMBOL, "}"));

            tokenizer.Advance();
            return stat;
        }

        private XmlElement CreateXmlTypeElement()
        {
            if (tokenizer.TypeOfToken() == TokenType.Keyword)
            {
                return CreateXmlElement(KEYWORD, tokenizer.Keyword());
            }
            else if (tokenizer.TypeOfToken() == TokenType.Identifier)
            {
                return CreateXmlElement(IDENTIFIER, tokenizer.Identifier());
            }
            else
            {
                throw new FormatException(
                    " [!] Invalid syntax.");
            }
        }

        private XmlElement CreateXmlElement(string name, string innerText = null)
        {
            Debug.Assert(!String.IsNullOrWhiteSpace(name));

            XmlElement element = xmlDoc.CreateElement(name);
            if (!String.IsNullOrEmpty(innerText))
            {
                element.InnerText = $" {innerText} ";
            }

            return element;
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

        private void WriteXmlToFile(string outputFile)
        {
            string tempFile = $"{outputFile}.temp";
            using (XmlWriter writer = CreateXmlWriter(tempFile))
            {
                xmlDoc.WriteContentTo(writer);
            }

            using (StreamReader reader = new StreamReader(tempFile))
            {
                using (StreamWriter writer = new StreamWriter(outputFile))
                {
                    writer.NewLine = NEW_LINE;
                    while (reader.Peek() >= 0)
                    {
                        string line = reader.ReadLine();
                        if (line.Trim() != XML_EMPTY)
                        {
                            writer.WriteLine(line);
                        }
                    }
                }
            }

            File.Delete(tempFile);
        }

        private static XmlWriter CreateXmlWriter(string filename)
        {
            Debug.Assert(!String.IsNullOrWhiteSpace(filename));

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            settings.Indent = true;
            settings.NewLineChars = NEW_LINE;
            return XmlWriter.Create(filename, settings);
        }
    }
}