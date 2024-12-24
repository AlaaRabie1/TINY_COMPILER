using JASON_Compiler;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JASON_Compiler
{
    public class Node
    {
        public List<Node> Children = new List<Node>();

        public string Name;
        public Node(string N)
        {
            this.Name = N;
        }
    }
    public class Parser
    {
        int InputPointer = 0;
        List<Token> TokenStream;
        public Node root;

        public Node StartParsing(List<Token> TokenStream)
        {
            this.InputPointer = 0;
            this.TokenStream = TokenStream;
            root = new Node("Program");
            root.Children.Add(Program());
            return root;
        }
        Node Program()
        {
            Node program = new Node("Program");
            program.Children.Add(Function_Statements());
            program.Children.Add(Main_Function());
            return program;
        }
        Node Main_Function()
        {
            Node main_function = new Node("Main_Function");
            if (InputPointer < TokenStream.Count && InputPointer + 1 < TokenStream.Count)
            {
                if (TokenStream[InputPointer].token_type == Token_Class.Int && TokenStream[InputPointer + 1].token_type == Token_Class.Main ||
               TokenStream[InputPointer].token_type == Token_Class.Float && TokenStream[InputPointer + 1].token_type == Token_Class.Main ||
               TokenStream[InputPointer].token_type == Token_Class.String && TokenStream[InputPointer + 1].token_type == Token_Class.Main)
                {
                    main_function.Children.Add(Datatype());
                    main_function.Children.Add(match(Token_Class.Main));
                    main_function.Children.Add(match(Token_Class.LParanthesis));
                    main_function.Children.Add(match(Token_Class.RParanthesis));
                    main_function.Children.Add(Function_Body());
                }
                else
                {
                    Errors.Error_List.Add("Parsing Error: Expected 'Main' function, but none found.\r\n");
                    InputPointer++;
                    return null;
                }

            }
            else
            {
                Errors.Error_List.Add("Parsing Error: Expected 'Main' function, but none found.\r\n");
                InputPointer++;
                return null;
            }
            return main_function;
        }
        Node Function_Statements()
        {
            Node function_statements = new Node("Function_Statements");
            if (TokenStream[InputPointer].token_type == Token_Class.Int && TokenStream[InputPointer + 1].token_type != Token_Class.Main ||
               TokenStream[InputPointer].token_type == Token_Class.Float && TokenStream[InputPointer + 1].token_type != Token_Class.Main ||
               TokenStream[InputPointer].token_type == Token_Class.String && TokenStream[InputPointer + 1].token_type != Token_Class.Main)
            {
                function_statements.Children.Add(Function_Statement());
                function_statements.Children.Add(FStatements());

            }
            else
            {
                return null;
            }
            return function_statements;
        }
        Node FStatements()
        {
            Node fStatements = new Node("FStatements");
            if (InputPointer < TokenStream.Count && InputPointer + 1 < TokenStream.Count)
            {

                if (TokenStream[InputPointer].token_type == Token_Class.Int && TokenStream[InputPointer + 1].token_type != Token_Class.Main ||
               TokenStream[InputPointer].token_type == Token_Class.Float && TokenStream[InputPointer + 1].token_type != Token_Class.Main ||
               TokenStream[InputPointer].token_type == Token_Class.String && TokenStream[InputPointer + 1].token_type != Token_Class.Main)
                {
                    fStatements.Children.Add(Function_Statement());
                    fStatements.Children.Add(FStatements());
                }
            }
            else
            {
                return null;
            }
            
            return fStatements;
        }
        Node Function_Statement()
        {
            Node function_statement = new Node("Function_Statement");
            function_statement.Children.Add(Function_Declaration());
            function_statement.Children.Add(Function_Body());
            return function_statement;
        }

        Node Function_Body()
        {

            Node function_body = new Node("Function_Body");
            if (TokenStream[InputPointer].token_type == Token_Class.LCurlyBrace)
            {
                function_body.Children.Add(match(Token_Class.LCurlyBrace));
                function_body.Children.Add(Statements());
                function_body.Children.Add(Return_Statement());
                function_body.Children.Add(match(Token_Class.RCurlyBrace));
            }
            return function_body;

        }
        Node Function_Declaration()
        {
            Node function_declaration = new Node("Function_Declaration");

            function_declaration.Children.Add(Datatype());
            function_declaration.Children.Add(FunctionName());
            function_declaration.Children.Add(match(Token_Class.LParanthesis));
            function_declaration.Children.Add(ParamDeclSec());
            function_declaration.Children.Add(match(Token_Class.RParanthesis));
            return function_declaration;

        }
        Node ParamDeclSec()
        {
            Node paramDeclSec = new Node("ParamDeclSec");
            if (TokenStream[InputPointer].token_type == Token_Class.Int ||
                TokenStream[InputPointer].token_type == Token_Class.Float ||
                TokenStream[InputPointer].token_type == Token_Class.String)
            {
                paramDeclSec.Children.Add(Parameter());
                paramDeclSec.Children.Add(PDecls());
            }
            else
            {
                return null;
            }
            return paramDeclSec;
        }
        Node PDecls()
        {
            Node pDecls = new Node("PDecls");
            if (TokenStream[InputPointer].token_type == Token_Class.Comma)
            {
                pDecls.Children.Add(match(Token_Class.Comma));
                pDecls.Children.Add(Parameter());
                pDecls.Children.Add(PDecls());
            }
            else
            {
                // Epsilon
                return null;
            }
            return pDecls;
        }
        Node Parameter()
        {
            Node parameter = new Node("Parameter");
            parameter.Children.Add(Datatype());
            parameter.Children.Add(match(Token_Class.Identifier));
            return parameter;

        }
        Node FunctionName()
        {
            Node functionName = new Node("FunctionName");
            if (TokenStream[InputPointer].token_type == Token_Class.Identifier)
            {
                functionName.Children.Add(match(Token_Class.Identifier));
            }
            return functionName;
        }
        Node Repeat_Statement()
        {
            Node repeat_statement = new Node("Repeat_Statement");

            if (TokenStream[InputPointer].token_type == Token_Class.Repeat)
            {
                repeat_statement.Children.Add(match(Token_Class.Repeat));
                repeat_statement.Children.Add(Statements());
                repeat_statement.Children.Add(match(Token_Class.Until));
                repeat_statement.Children.Add(Condition_Statement());
            }
            return repeat_statement;

        }
        Node Statements()
        {
            if (InputPointer < TokenStream.Count)
            {
                Node statements = new Node("Statements");

                // Parse a single Statement
                Node statement = Statement();
                if (statement != null)
                {
                    statements.Children.Add(statement);
                }
                else
                {
                    return null; // If no valid statement, return null
                }

                // Parse the remaining States (which can be another sequence of statements or epsilon)
                Node states = States();
                if (states != null)
                {
                    statements.Children.Add(states);
                }

                return statements;
            }

            return null; // Unexpected end of input
        }
        Node States()
        {
            Node states = new Node("States");

            // Try parsing another Statement
            Node statement = Statement();
            if (statement != null)
            {
                states.Children.Add(statement);
                // Recursively parse more States
                Node moreStates = States();
                if (moreStates != null)
                {
                    states.Children.Add(moreStates);
                }
            }
            // If no more statements, epsilon is implicitly handled by returning an empty node
            return states;
        }
        Node Statement()
        {
            // Check for Write_Statement
            if (TokenStream[InputPointer].token_type == Token_Class.Write)
            {
                return Write_Statement();
            }
            // Check for Read_Statement
            else if (TokenStream[InputPointer].token_type == Token_Class.Read)
            {
                return Read_Statement();
            }
            // Check for AssignmentState
            else if (TokenStream[InputPointer].token_type == Token_Class.Identifier &&
                     TokenStream[InputPointer + 1].token_type == Token_Class.AssignOp)
            {
                return Assignment_Statement();
            }
            // Check for Declaration_Statement (assuming it's a variable declaration)
            else if (TokenStream[InputPointer].token_type == Token_Class.Int ||
                     TokenStream[InputPointer].token_type == Token_Class.Float ||
                     TokenStream[InputPointer].token_type == Token_Class.String)
            {
                return Declaration_Statement();
            }
            // Check for If_Statement
            else if (TokenStream[InputPointer].token_type == Token_Class.If)
            {
                return If_Statement();
            }
            // Check for Repeat_Statement
            else if (TokenStream[InputPointer].token_type == Token_Class.Repeat)
            {
                return Repeat_Statement();
            }

            return null; // No valid Statement found
        }
        Node Declaration_Statement()
        {
            Node declarationStatement = new Node("Declaration_Statement");

            // Match the datatype (int, float, or string)
            Node datatype = Datatype();
            if (datatype != null)
            {
                declarationStatement.Children.Add(datatype);
            }
            else
            {
                return null; // If no valid datatype, return null
            }

            // Parse the identifier list
            Node identifierList = Identifier_list();
            if (identifierList != null)
            {
                declarationStatement.Children.Add(identifierList);
            }
            else
            {
                return null; // If no valid identifier list, return null
            }

            // Match the semicolon
            Node semicolon = match(Token_Class.Semicolon);
            if (semicolon != null)
            {
                declarationStatement.Children.Add(semicolon);
            }
            else
            {
                return null; // If no semicolon, return null
            }

            return declarationStatement;
        }
        Node Identifier_list()
        {
            Node identifierList = new Node("Identifier_list");

            // Parse the first Id
            Node id = Id();
            if (id != null)
            {
                identifierList.Children.Add(id);
            }
            else
            {
                return null; // If no valid Id, return null
            }

            // Parse the continuation (Identifier_list’)
            Node identifierListPrime = Identifier_list_prime();
            if (identifierListPrime != null)
            {
                identifierList.Children.Add(identifierListPrime);
            }

            return identifierList;
        }
        Node Identifier_list_prime()
        {
            Node identifierListPrime = new Node("Identifier_list'");

            // Check if the next token is a comma
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Comma)
            {
                // Match the comma
                identifierListPrime.Children.Add(match(Token_Class.Comma));

                // Parse the next Id
                Node id = Id();
                if (id != null)
                {
                    identifierListPrime.Children.Add(id);
                }
                else
                {
                    return null; // If no valid Id after comma, return null
                }

                // Recursively parse the continuation
                Node moreIdentifiers = Identifier_list_prime();
                if (moreIdentifiers != null)
                {
                    identifierListPrime.Children.Add(moreIdentifiers);
                }
            }
            // Epsilon is implicitly handled by returning the empty node
            return identifierListPrime;
        }
        Node Id()
        {
            Node id = new Node("Id");

            // Check for standalone identifier
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Identifier)
            {
                id.Children.Add(match(Token_Class.Identifier));
            }
            // Check for an assignment statement
            else if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Identifier &&
                     TokenStream[InputPointer + 1].token_type == Token_Class.AssignOp)
            {
                Node assignment = Assignment_Statement();
                if (assignment != null)
                {
                    id.Children.Add(assignment);
                }
                else
                {
                    return null; // If no valid assignment statement, return null
                }
            }
            else
            {
                return null; // If neither identifier nor assignment statement, return null
            }

            return id;
        }


        Node Term()
        {
            if (InputPointer < TokenStream.Count)
            {
                Node term = new Node("Term");

                // Check the type of the current token and build the appropriate subtree
                if (TokenStream[InputPointer].token_type == Token_Class.Constant)
                {
                    // Match a number (Constant)
                    term.Children.Add(match(Token_Class.Constant));
                }
                else if (TokenStream[InputPointer].token_type == Token_Class.Identifier)
                {
                    // Lookahead to distinguish between Identifier and Function_Call
                    if (InputPointer + 1 < TokenStream.Count && TokenStream[InputPointer + 1].token_type == Token_Class.LParanthesis)
                    {
                        // Parse Function_Call
                        term.Children.Add(Function_Call());
                    }
                    else
                    {
                        // Match a standalone identifier
                        term.Children.Add(match(Token_Class.Identifier));
                    }
                }
                else
                {
                    // If the token doesn't match Term's CFG, no explicit error handling here
                    // Because the mismatch will be logged during the call to `match` in Function_Call or elsewhere
                    return null;
                }

                return term;
            }

            return null; // Implicit error if end of input is reached without finding a Term
        }
        Node Function_Call()
        {
            Node functionCall = new Node("Function_Call");

            // Match the identifier
            Node identifier = match(Token_Class.Identifier);
            if (identifier != null)
            {
                functionCall.Children.Add(identifier);
            }
            else
            {
                return null; // If no identifier, return null
            }

            // Match '('
            Node leftParenthesis = match(Token_Class.LParanthesis);
            if (leftParenthesis != null)
            {
                functionCall.Children.Add(leftParenthesis);
            }
            else
            {
                return null; // If no '(', return null
            }

            // Parse the Id_list (it could be epsilon)
            Node idList = Id_list();
            if (idList != null)
            {
                functionCall.Children.Add(idList);
            }

            // Match ')'
            Node rightParenthesis = match(Token_Class.RParanthesis);
            if (rightParenthesis != null)
            {
                functionCall.Children.Add(rightParenthesis);
            }
            else
            {
                return null; // If no ')', return null
            }

            return functionCall;
        }

        Node Id_list()
        {
            Node idList = new Node("Id_list");

            // Check if the next token is an identifier
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Identifier)
            {
                // Match the identifier
                idList.Children.Add(match(Token_Class.Identifier));

                // Parse the continuation (Id_list’)
                Node idListPrime = Id_list_prime();
                if (idListPrime != null)
                {
                    idList.Children.Add(idListPrime);
                }
            }
            // Epsilon is implicitly handled by returning the empty node
            return idList;
        }
        Node Id_list_prime()
        {
            Node idListPrime = new Node("Id_list'");

            // Check if the next token is a comma
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Comma)
            {
                // Match the comma
                idListPrime.Children.Add(match(Token_Class.Comma));

                // Match the next identifier
                Node identifier = match(Token_Class.Identifier);
                if (identifier != null)
                {
                    idListPrime.Children.Add(identifier);

                    // Recursively parse the continuation
                    Node moreIdentifiers = Id_list_prime();
                    if (moreIdentifiers != null)
                    {
                        idListPrime.Children.Add(moreIdentifiers);
                    }
                }
                else
                {
                    return null; // If no valid identifier after the comma, return null
                }
            }
            // Epsilon is implicitly handled by returning the empty node
            return idListPrime;
        }

        Node Datatype()
        {
            if (InputPointer < TokenStream.Count)
            {
                Node datatype = new Node("Datatype");

                // Match one of the valid datatypes: int, float, or string
                if (TokenStream[InputPointer].token_type == Token_Class.Int)
                {
                    datatype.Children.Add(match(Token_Class.Int));
                }
                else if (TokenStream[InputPointer].token_type == Token_Class.Float)
                {
                    datatype.Children.Add(match(Token_Class.Float));
                }
                else if (TokenStream[InputPointer].token_type == Token_Class.String)
                {
                    datatype.Children.Add(match(Token_Class.String));
                }
                else
                {
                    // Return null if no valid datatype is found (error is handled by match)
                    return null;
                }

                return datatype;
            }

            // Return null if end of input is reached unexpectedly
            return null;
        }
        Node Expression()
        {
            if (InputPointer < TokenStream.Count)
            {
                Node expression = new Node("Expression");

                if (TokenStream[InputPointer].token_type == Token_Class.String)
                {
                    // Match stringLine
                    expression.Children.Add(match(Token_Class.String));
                }
                else if ((TokenStream[InputPointer].token_type == Token_Class.Constant &&
                         TokenStream[InputPointer + 1].token_type == Token_Class.Semicolon) ||
                         (TokenStream[InputPointer].token_type == Token_Class.Identifier &&
                         TokenStream[InputPointer + 1].token_type == Token_Class.Semicolon) ||
                         (TokenStream[InputPointer].token_type == Token_Class.Identifier &&
                         TokenStream[InputPointer + 1].token_type == Token_Class.LParanthesis))
                {
                        // Match Term
                        expression.Children.Add(Term());
                }
                else if (TokenStream[InputPointer].token_type == Token_Class.LParanthesis ||
                         TokenStream[InputPointer].token_type == Token_Class.Identifier ||
                         TokenStream[InputPointer].token_type == Token_Class.Constant)
                {
                    // Match Equation
                    expression.Children.Add(Equation());
                }
                else
                {
                    return null; // No valid Expression found
                }

                return expression;
            }

            return null; // Unexpected end of input
        }
        Node Assignment_Statement()
        {
            Node assignment_statement = new Node("Assignment_Statement");

            // Match identifier
            assignment_statement.Children.Add(match(Token_Class.Identifier));

            // Match assignmentOp
            assignment_statement.Children.Add(match(Token_Class.AssignOp));

            // Match Expression
            Node expr = Expression();
            if (expr != null)
            {
                assignment_statement.Children.Add(expr);
            }
            else
            {
                return null; // Invalid assignment (missing or malformed Expression)
            }

            // Match semicolon
            assignment_statement.Children.Add(match(Token_Class.Semicolon));

            return assignment_statement;
        }
        Node Equation()
        {
            Node equation = new Node("Equation");

            if (InputPointer < TokenStream.Count &&
                TokenStream[InputPointer].token_type == Token_Class.LParanthesis)
            {
                // Match '('
                equation.Children.Add(match(Token_Class.LParanthesis));

                // Match inner Equation
                Node innerEquation = Equation();
                if (innerEquation != null)
                {
                    equation.Children.Add(innerEquation);
                }
                else
                {
                    return null; // Invalid Equation inside parentheses
                }

                // Match ')'
                if (InputPointer < TokenStream.Count &&
                    TokenStream[InputPointer].token_type == Token_Class.RParanthesis)
                {
                    equation.Children.Add(match(Token_Class.RParanthesis));
                }
                else
                {
                    return null; // Missing closing parenthesis
                }

                // Optionally match Operator_Equation
                Node operatorEquation = Operator_Equation();
                if (operatorEquation != null)
                {
                    equation.Children.Add(operatorEquation);
                }
            }
            else
            {
                // Match Term
                Node term = Term();
                if (term != null)
                {
                    equation.Children.Add(term);

                    // Optionally match Operator_Equation
                    Node operatorEquation = Operator_Equation();
                    if (operatorEquation != null)
                    {
                        equation.Children.Add(operatorEquation);
                    }
                }
                else
                {
                    return null; // Invalid Equation
                }
            }

            return equation;
        }

        Node Operator_Equation()
        {
            // Check if the input has an arithmetic operator
            if (InputPointer < TokenStream.Count &&
                (TokenStream[InputPointer].token_type == Token_Class.PlusOp ||
                 TokenStream[InputPointer].token_type == Token_Class.MinusOp ||
                 TokenStream[InputPointer].token_type == Token_Class.DivideOp ||
                 TokenStream[InputPointer].token_type == Token_Class.MultiplyOp))
            {
                Node operatorEquation = new Node("Operator_Equation");

                // Match Arthematic_Operator
                Node arithmeticOperator = Arthematic_Operator();
                if (arithmeticOperator != null)
                {
                    operatorEquation.Children.Add(arithmeticOperator);
                }
                else
                {
                    return null; // No valid operator found
                }

                // Match Equation
                Node equation = Equation();
                if (equation != null)
                {
                    operatorEquation.Children.Add(equation);
                }
                else
                {
                    return null; // Invalid Equation
                }

                return operatorEquation; // Return matched Operator_Equation
            }

            // Epsilon (empty production)
            return null;
        }

        Node Arthematic_Operator()
        {
            if (InputPointer < TokenStream.Count)
            {
                Node op = new Node("Arthematic_Operator");

                if (TokenStream[InputPointer].token_type == Token_Class.PlusOp)
                {
                    op.Children.Add(match(Token_Class.PlusOp));
                }
                else if (TokenStream[InputPointer].token_type == Token_Class.MinusOp)
                {
                    op.Children.Add(match(Token_Class.MinusOp));
                }
                else if (TokenStream[InputPointer].token_type == Token_Class.DivideOp)
                {
                    op.Children.Add(match(Token_Class.DivideOp));
                }
                else if (TokenStream[InputPointer].token_type == Token_Class.MultiplyOp)
                {
                    op.Children.Add(match(Token_Class.MultiplyOp));
                }
                else
                {
                    return null; // No valid operator found
                }

                return op;
            }

            return null; // Unexpected end of input
        }

        Node Write_Statement()
        {
            Node write_statement = new Node("Write_Statement");

            if (TokenStream[InputPointer].token_type == Token_Class.Write)
            {
                write_statement.Children.Add(match(Token_Class.Write));
                write_statement.Children.Add(Exp_end());
                write_statement.Children.Add(match(Token_Class.Semicolon));
                return write_statement;
            }
            return null;// how to handle erros
        }
        Node Exp_end()
        {
            Node _end = new Node("Exp_end");

            if (TokenStream[InputPointer].token_type == Token_Class.Endl)
            {
                _end.Children.Add(match(Token_Class.Endl));
                _end.Children.Add(match(Token_Class.Semicolon));
                return _end;
            }
            else
            {
                _end.Children.Add(Expression());
                return _end;

            }
            return null;
            // how to handle erros
        }
        Node Read_Statement() // Q i will read only identifier>
        {
            Node read = new Node("read_Statement");

            if (TokenStream[InputPointer].token_type == Token_Class.Read)
            {
                read.Children.Add(match(Token_Class.Read));
                read.Children.Add(match(Token_Class.Identifier));
                read.Children.Add(match(Token_Class.Semicolon));
                return read;
            }
            return null;// how to handle erros
        }

        Node Return_Statement()
        {
            Node _return = new Node("return_Statement");

            if (TokenStream[InputPointer].token_type == Token_Class.Return)
            {
                _return.Children.Add(match(Token_Class.Return));
                _return.Children.Add(Expression());
                _return.Children.Add(match(Token_Class.Semicolon));
                return _return;
            }
            return null;// how to handle erros
        }
        Node Condition()
        {
            Node cond = new Node("Condition");

            if (TokenStream[InputPointer].token_type == Token_Class.Identifier)
            {
                cond.Children.Add(match(Token_Class.Identifier));
                cond.Children.Add(Condition_Operator());
                cond.Children.Add(Term());
                return cond;
            }
            return null;// how to handle erros
        }
        Node Condition_Operator()
        {
            Node cond = new Node("Condition_OP");

            if (TokenStream[InputPointer].token_type == Token_Class.LessThanOp)
            {
                cond.Children.Add(match(Token_Class.LessThanOp));
                return cond;
            }
            else if (TokenStream[InputPointer].token_type == Token_Class.GreaterThanOp)
            {
                cond.Children.Add(match(Token_Class.GreaterThanOp));
                return cond;
            }
            else if (TokenStream[InputPointer].token_type == Token_Class.EqualOp)
            {
                cond.Children.Add(match(Token_Class.EqualOp));
                return cond;
            }
            else if (TokenStream[InputPointer].token_type == Token_Class.NotEqualOp)
            {
                cond.Children.Add(match(Token_Class.NotEqualOp));
                return cond;
            }

            return null;// how to handle erros
        }
        Node Boolean_Operator()
        {
            Node Bool = new Node("Boolen_OP");

            if (TokenStream[InputPointer].token_type == Token_Class.AndOp)
            {
                Bool.Children.Add(match(Token_Class.AndOp));
                return Bool;
            }
            else if (TokenStream[InputPointer].token_type == Token_Class.OrOp)
            {
                Bool.Children.Add(match(Token_Class.OrOp));
                return Bool;
            }
            // ==???
            return null;// how to handle erros
        }

        Node Condition_Statement()
        {
            Node cond = new Node("Condition_Statement");
            if (TokenStream[InputPointer].token_type == Token_Class.Identifier)
            {
                cond.Children.Add(Condition());
                cond.Children.Add(Boolean_Statements());

                return cond;
            }
            return null;// how to handle erros
        }


        Node Boolean_Condition()
        {
            Node Bool = new Node("Boolean_Condition ");

            if (TokenStream[InputPointer].token_type == Token_Class.AndOp || TokenStream[InputPointer].token_type == Token_Class.OrOp)
            {
                Bool.Children.Add(Boolean_Operator());
                Bool.Children.Add(Condition());
                return Bool;
            }
            // ==???
            return null;// how to handle erros
        }
        Node Boolean_Statements()
        {
            Node Bool = new Node("Boolean_Statements");

            if (TokenStream[InputPointer].token_type == Token_Class.AndOp || TokenStream[InputPointer].token_type == Token_Class.OrOp)
            {
                Bool.Children.Add(Boolean_Condition());
                Bool.Children.Add(BConditions());
                return Bool;
            }
            else return Bool;
            // ==???
            //return null;// how to handle erros
        }
        Node BConditions()
        {
            Node Bool = new Node("BConditions");

            if (TokenStream[InputPointer].token_type == Token_Class.AndOp || TokenStream[InputPointer].token_type == Token_Class.OrOp)
            {
                Bool.Children.Add(Boolean_Condition());
                Bool.Children.Add(BConditions());
                return Bool;
            }
            else return Bool;
            // ==???
            //return null;// how to handle erros
        }

        Node If_Statement()
        {
            Node IF = new Node("If_Statement");

            if (TokenStream[InputPointer].token_type == Token_Class.If)
            {
                //if  Condition_Statement  then Statements Ending
                IF.Children.Add(match(Token_Class.If));
                IF.Children.Add(Condition_Statement());
                IF.Children.Add(match(Token_Class.Then));
                IF.Children.Add(Statements());
                IF.Children.Add(Ending());
                return IF;
            }

            // ==???
            return null;// how to handle erros
        }
        Node Ending()
        {
            Node IF = new Node("Ending");
            // Else_If_Statement  |  Else_Statement  |  end 

            if (TokenStream[InputPointer].token_type == Token_Class.ElseIf)
            {

                IF.Children.Add(Else_If_Statement());
                return IF;
            }
            else if (TokenStream[InputPointer].token_type == Token_Class.Else)
            {
                IF.Children.Add(Else_Statement());
                return IF;

            }
            else if (TokenStream[InputPointer].token_type == Token_Class.End)
            {
                IF.Children.Add(match(Token_Class.End));
                return IF;

            }

            // ==???
            return null;// how to handle erros
        }

        Node Else_If_Statement()
        {
            Node IF = new Node("Else_If_Statement");

            if (TokenStream[InputPointer].token_type == Token_Class.ElseIf)
            {
                IF.Children.Add(match(Token_Class.ElseIf));
                IF.Children.Add(Condition_Statement());
                IF.Children.Add(match(Token_Class.Then));
                IF.Children.Add(Statements());
                IF.Children.Add(Ending());
                return IF;
            }

            // ==???
            return null;// how to handle erros
        }

        Node Else_Statement()
        {
            Node IF = new Node("Else_Statement");

            if (TokenStream[InputPointer].token_type == Token_Class.Else)
            {
                IF.Children.Add(match(Token_Class.Else));
                IF.Children.Add(Statements());
                IF.Children.Add(Ending());
                return IF;
            }

            // ==???
            return null;// how to handle erros
        }

        public Node match(Token_Class ExpectedToken)
        {

            if (InputPointer < TokenStream.Count)
            {
                if (ExpectedToken == TokenStream[InputPointer].token_type)
                {
                    InputPointer++;
                    Node newNode = new Node(ExpectedToken.ToString());

                    return newNode;

                }

                else
                {
                    Errors.Error_List.Add("Parsing Error: Expected "
                        + ExpectedToken.ToString() + " and " +
                        TokenStream[InputPointer].token_type.ToString() +
                        "  found\r\n");
                    InputPointer++;
                    return null;
                }
            }
            else
            {
                Errors.Error_List.Add("Parsing Error: Expected "
                        + ExpectedToken.ToString() + "\r\n");
                InputPointer++;
                return null;
            }
        }

        public static TreeNode PrintParseTree(Node root)
        {
            TreeNode tree = new TreeNode("Parse Tree");
            TreeNode treeRoot = PrintTree(root);
            if (treeRoot != null)
                tree.Nodes.Add(treeRoot);
            return tree;
        }
        static TreeNode PrintTree(Node root)
        {
            if (root == null || root.Name == null)
                return null;
            TreeNode tree = new TreeNode(root.Name);
            if (root.Children.Count == 0)
                return tree;
            foreach (Node child in root.Children)
            {
                if (child == null)
                    continue;
                tree.Nodes.Add(PrintTree(child));
            }
            return tree;
        }
    }
}
