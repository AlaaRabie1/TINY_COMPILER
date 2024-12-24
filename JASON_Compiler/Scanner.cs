using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using JASON_Compiler;

public enum Token_Class
{
    Int, Main, Read, Write, Repeat, Until, If, Then, ElseIf, Else, End, Return,
    Identifier, Constant, String, Literal, Float, PlusOp, MinusOp, MultiplyOp, DivideOp,
    EqualOp, LessThanOp, GreaterThanOp, NotEqualOp, AndOp, OrOp, AssignOp,
    LParanthesis, RParanthesis, LCurlyBrace, RCurlyBrace, Semicolon, Comma, Endl
}
namespace JASON_Compiler
{


    public class Token
    {
        public string lex;
        public Token_Class token_type;
    }

    public class Scanner
    {
        public List<Token> Tokens = new List<Token>();
        Dictionary<string, Token_Class> ReservedWords = new Dictionary<string, Token_Class>();
        Dictionary<string, Token_Class> Operators = new Dictionary<string, Token_Class>();

        public Scanner()
        {
            ReservedWords.Add("int", Token_Class.Int);
            ReservedWords.Add("float", Token_Class.Float);
            ReservedWords.Add("string", Token_Class.String);
            ReservedWords.Add("main", Token_Class.Main);
            ReservedWords.Add("read", Token_Class.Read);
            ReservedWords.Add("write", Token_Class.Write);
            ReservedWords.Add("repeat", Token_Class.Repeat);
            ReservedWords.Add("until", Token_Class.Until);
            ReservedWords.Add("if", Token_Class.If);
            ReservedWords.Add("then", Token_Class.Then);
            ReservedWords.Add("elseif", Token_Class.ElseIf);
            ReservedWords.Add("else", Token_Class.Else);
            ReservedWords.Add("end", Token_Class.End);
            ReservedWords.Add("return", Token_Class.Return);
            ReservedWords.Add("endl", Token_Class.Endl);


            Operators.Add("+", Token_Class.PlusOp);
            Operators.Add("-", Token_Class.MinusOp);
            Operators.Add("*", Token_Class.MultiplyOp);
            Operators.Add("/", Token_Class.DivideOp);
            Operators.Add("=", Token_Class.EqualOp);
            Operators.Add("<", Token_Class.LessThanOp);
            Operators.Add(">", Token_Class.GreaterThanOp);
            Operators.Add("<>", Token_Class.NotEqualOp);
            Operators.Add("&&", Token_Class.AndOp);
            Operators.Add("||", Token_Class.OrOp);
            Operators.Add(":=", Token_Class.AssignOp);
            Operators.Add("(", Token_Class.LParanthesis);
            Operators.Add(")", Token_Class.RParanthesis);
            Operators.Add("{", Token_Class.LCurlyBrace);
            Operators.Add("}", Token_Class.RCurlyBrace);
            Operators.Add(";", Token_Class.Semicolon);
            Operators.Add(",", Token_Class.Comma);
            //Operators.Add(".", Token_Class.Dot);
            //Operators.Add("endl", Token_Class.Endl);



        }

        public void StartScanning(string SourceCode)
        {
            for (int i = 0; i < SourceCode.Length; i++)
            {
                int j = i;
                char CurrentChar = SourceCode[i];
                string CurrentLexeme = CurrentChar.ToString();

                if (CurrentChar == ' ' || CurrentChar == '\r' || CurrentChar == '\n')
                    continue;

                if (CurrentChar >= 'A' && CurrentChar <= 'z') //if you read a character
                {
                    CurrentLexeme = "";
                    while (j < SourceCode.Length && ((SourceCode[j] >= 'A' && SourceCode[j] <= 'z') || (SourceCode[j] >= '0' && SourceCode[j] <= '9')))
                    {
                        CurrentLexeme += SourceCode[j].ToString();
                        j++;
                    }
                    i = j - 1;
                    FindTokenClass(CurrentLexeme, GetOperators());
                }

                else if (CurrentChar >= '0' && CurrentChar <= '9')
                {
                    CurrentLexeme = "";

                    while (j < SourceCode.Length && ((SourceCode[j] >= '0' && SourceCode[j] <= '9') || SourceCode[j] == '.'))
                    {
                        CurrentLexeme += SourceCode[j];
                        j++;
                    }
                    i = j - 1;
                    FindTokenClass(CurrentLexeme, GetOperators());
                }
                else if (CurrentChar == '/' && i + 1 < SourceCode.Length && SourceCode[i + 1] == '*')
                {
                    int oldI = i;
                    i++;
                    j = i;

                    while (j < SourceCode.Length && j + 1 < SourceCode.Length && j + 2 < SourceCode.Length && !(SourceCode[j + 1] == '*' && SourceCode[j + 2] == '/'))
                    {
                        j++;

                    }

                    //if ending of comment not exist
                    if (j >= SourceCode.Length || j + 1 >= SourceCode.Length || j + 2 >= SourceCode.Length)
                    {
                        FindTokenClass(CurrentLexeme, GetOperators());
                        i = oldI;
                        j = i;

                    }
                    else
                    {
                        //normal comment
                        i = j + 2;
                    }



                }
                else if (CurrentChar == ':' && i + 1 < SourceCode.Length && SourceCode[i + 1] == '=')
                {
                    i = i + 1;
                    CurrentLexeme = ":=";
                    FindTokenClass(CurrentLexeme, GetOperators());
                }
                else if (CurrentChar == '"')
                {
                    CurrentLexeme = "";

                    do
                    {
                        CurrentLexeme += SourceCode[j];
                        j++;
                    } while (j < SourceCode.Length && SourceCode[j] != '"');

                    if (j >= SourceCode.Length)
                    {
                        FindTokenClass(CurrentChar.ToString(), GetOperators());
                        continue;
                    }

                    if (SourceCode[j] == '"')
                    {
                        CurrentLexeme += SourceCode[j];

                    }

                    i = j;

                    FindTokenClass(CurrentLexeme, GetOperators());
                }
                else if (CurrentChar == '&' && i + 1 < SourceCode.Length && SourceCode[i + 1] == '&')
                {
                    CurrentLexeme = "&&";

                    i = i + 1;

                    FindTokenClass(CurrentLexeme, GetOperators());
                }
                else if (CurrentChar == '|' && i + 1 < SourceCode.Length && SourceCode[i + 1] == '|')
                {
                    CurrentLexeme = "||";

                    i = i + 1;

                    FindTokenClass(CurrentLexeme, GetOperators());
                }
                else if (CurrentChar == '<' && i + 1 < SourceCode.Length && SourceCode[i + 1] == '>')
                {
                    CurrentLexeme = "<>";

                    i = i + 1;

                    FindTokenClass(CurrentLexeme, GetOperators());
                }
                else
                {
                    FindTokenClass(CurrentLexeme, GetOperators());
                }
            }

            JASON_Compiler.TokenStream = Tokens;
        }

        private Dictionary<string, Token_Class> GetOperators()
        {
            return Operators;
        }

        void FindTokenClass(string Lex, Dictionary<string, Token_Class> operators)
        {
            Token_Class TC;
            Token Tok = new Token();
            Tok.lex = Lex;
            // Is it a reserved word?
            if (ReservedWords.ContainsKey(Lex))
            {
                Tok.token_type = ReservedWords[Lex];
                Tokens.Add(Tok);
                return;
            }

            // Is it an identifier?
            if (isIdentifier(Lex))
            {
                Tok.token_type = Token_Class.Identifier;
                Tokens.Add(Tok);
                return;
            }

            // Is it a Constant?
            if (isConstant(Lex))
            {
                Tok.token_type = Token_Class.Constant;
                Tokens.Add(Tok);
                return;
            }
            // Is it a string?
            if (isString(Lex))
            {
                Tok.token_type = Token_Class.Literal;
                Tokens.Add(Tok);
                return;
            }

            // Is it an operator?
            if (operators.ContainsKey(Lex))
            {
                Tok.token_type = Operators[Lex];
                Tokens.Add(Tok);
                return;
            }

            // Is it an undefined?
            Errors.Error_List.Add("Lexical Error: " + Lex);
        }



        bool isIdentifier(string lex)
        {
            bool isValid = true;
            // Check if the lex is an identifier or not using regular expression.
            Regex identifierRegex = new Regex(@"^[a-zA-Z][a-zA-Z0-9]*$");
            if (!identifierRegex.IsMatch(lex))
            {
                isValid = false;
            }
            return isValid;
        }
        bool isConstant(string lex)
        {
            bool isValid = true;
            // Check if the lex is a constant (Number) or not using regular expression.
            Regex constantRegex = new Regex(@"^[0-9]+([.][0-9]+)?$");
            if (!constantRegex.IsMatch(lex))
            {
                isValid = false;
            }
            return isValid;
        }
        bool isString(string lex)
        {
            bool isValid = true;
            // Check if the lex is a string using regular expression.
            Regex stringRegex = new Regex("^\"[^\"]*\"$");
            if (!stringRegex.IsMatch(lex))
            {
                isValid = false;
            }
            return isValid;
        }
    }
}
