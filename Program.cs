using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

Console.SetIn(new StreamReader(".\\calculator\\calculator_edge.txt"));


/*
 * This solution will not handle the equation using the infix notation. It will instead handle it using 
 * postfix notation. After converting the infix equation to post fix. The Reverse Polish method will be 
 * used to solve the mathematical equations.
 */


// Selects the correct culture info (for this test)
Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

// Matches "numbers" and "." or any combination of both.
Regex numberRegex = new Regex(@"^-?\d+(\.\d+)?$");
// Matches - + * / ( ) 
Regex arithmeticRegex = new Regex(@"^[\+\-\*\/\(\)]$");

string input;
List<string> equationToSolve = new List<string>();


// Register inputs
while ((input = Console.ReadLine()) != null)
{
	// Removes any white-space and adds equation list
	equationToSolve.Add(input.Replace(" ", ""));
}

foreach (string equation in equationToSolve)
{

	if (equation.Length > 0)
	{
		Queue<string> postfixEquation = PrepareEquation(Tokenizer(equation)); 
		
		double result = double.Parse( SolveEquation(postfixEquation) );

		// Limits the decimal points to two
		Console.WriteLine(result.ToString("F2"));
	}
}

// Check if value is a number
bool IsPartOfANumber(string value) => 
	numberRegex.IsMatch(value) || value.Contains(".");


// Converts from a single string into a list of strings.
// Separating full integers, parentheses and operators.
List<string> Tokenizer(string tokens) 
{
	List<string> tokenList = new List<string>();

	string intStringValue = string.Empty;

	string outerCharacter = string.Empty;

	for (int i = 0; i < tokens.Length; i++)
	{
		outerCharacter = tokens[i].ToString();
		
		// Adds operators and parentheses to the token list
		if (arithmeticRegex.IsMatch(outerCharacter))
		{
			// Separates operator and numeric "-"
			if (tokens[i] == '-' && IsUnaryMinus(i, tokens))
			{
				intStringValue = "-";
				continue;
			}
			else
			{
				tokenList.Add(outerCharacter);
			}
		}
		else  // Adds numerical tokens to the output stack
		{
			string innerCharacter = string.Empty;

			for (int j = i; j < tokens.Count(); j++)
			{
				innerCharacter = tokens[j].ToString();

				bool isPartOfANumber = IsPartOfANumber(innerCharacter);

				// adds more chars to the numerical value
				if (isPartOfANumber)
				{
					intStringValue = string.Concat(intStringValue, innerCharacter);
					i = j;
				}
				else
				{
					// Next char is an operation, add the complete number to output stack
					tokenList.Add(intStringValue);
					intStringValue = string.Empty;
					break;
				}
			}
		}
	}

	// Adds the final numerical token to the list 
	if (intStringValue != "") tokenList.Add(intStringValue);

	return tokenList;
}

// Checks if the current minus is an operation or part of number
// If equation starts with "-" or the token before is not a number,
// then it is part of a number.
bool IsUnaryMinus(int index, string tokens)
{
	if (index == 0) return true;

	string prevToken = string.Empty;
	
	if (index - 1 > -1)
		prevToken = tokens[index - 1].ToString();

	return
		prevToken == "(" ||
		prevToken == "+" ||
		prevToken == "-" ||
		prevToken == "*" ||
		prevToken == "/";
}

// Uses the Shunting-Yard Algorithm to go from infix to postfix
Queue<string> PrepareEquation(List<string> token)
{
	Queue<string> outputStack = new Queue<string>();
	Stack<string> operatorStack = new Stack<string>();
	 

	string currentToken = string.Empty;

	for (int i = 0; i < token.Count; i++)
	{
		currentToken = token[i];

		if (currentToken == "(")  
		{
			// Add the start parentheses. Indicates where to stop the popping loop when the end parentheses is encountered.
			operatorStack.Push(currentToken);
		}
		else if (currentToken == ")") // All values "within" the parentheses are popped.
		{
			string tempToken = string.Empty;

			// Pops all tokens found within the parentheses (but not the parentheses them self).
			// And place them in the output queue.
			while (operatorStack.Count > 0)
			{
				tempToken = operatorStack.Pop();
				
				if (tempToken != "(")
					outputStack.Enqueue(tempToken);
				else 
					break;
			} 
		}  
		else if (arithmeticRegex.IsMatch(currentToken)) // Adds operations to the list
		{
			// Only pop from operator stack if top operator token has equal or higher precedence then current operator.
			// "(" does not count as an operator and will result in a push to operator no matter the operator.
			while (operatorStack.Count > 0)
			{
				var topOperatorToken = operatorStack.Peek();
				
				// If top operator in stack is (, then nothing is done.
				if (topOperatorToken == "(")
					break;
			
				if (ShouldPop(currentToken, topOperatorToken))
					outputStack.Enqueue(operatorStack.Pop());
				else break;
			}
						
			operatorStack.Push(currentToken);
			
		}
		else  // Adds numerical tokens to the output stack
		{
			outputStack.Enqueue(currentToken);
		}
	}

	// Pops the reminding operator to output stack.  
	while (operatorStack.Count > 0) outputStack.Enqueue(operatorStack.Pop());

	return outputStack; 
}

// Sets the precedence value
int OperatorTokenValue(string token)
{
	int value = 0;

	switch (token)
	{
		case "*":
			value = 2; break;
		case "/":
			value = 2; break; 
		case "-":
			value = 1; break; 
		case "+":
			value = 1; break; 
		default:
			break;
	}

	return value;
}

// Checks if top stack operator has higher precedence 
bool ShouldPop(string currentOperator, string stackOperator)
{
	int current = OperatorTokenValue(currentOperator);
	int stack = OperatorTokenValue(stackOperator);

	return stack >= current; 
}

// Solves an equation in reverse polish order. (postfix)
string SolveEquation(Queue<string> tokens)
{
	Stack<double> numberTokens = new Stack<double>();
	double result = 0;
	
	if (tokens.Count > 1)
	{
		// Keeps pushing numerical values until a operation is encountered.
		// When an operation is encountered the two topmost values are taken and
		// the calculation is performed. The value is then pushed back onto the stack for 
		// use in the next operation (if any).
		while (tokens.Count > 0)
		{
			string token = tokens.Dequeue();

			if (numberRegex.IsMatch(token) && !(token.Equals("-")))
			{
				numberTokens.Push(double.Parse(token));
			}
			else
			{
				result =
					PerformOperation(
						numberTokens.Pop(), // Right variable
						numberTokens.Pop(), // Left variable
						token				// Operator
					);

				numberTokens.Push(result);
			}
		}
	}
	else
	{	// Handles instances with only a singer numerical input.
		return tokens.Dequeue().ToString();
	}

	return numberTokens.Pop().ToString();
}

double PerformOperation(double rightValue, double leftValue, string operation)
{
	double result = 0;

	switch (operation)
	{
		case "*":
			result = leftValue * rightValue;
			break;
		case "/":
			result = leftValue / rightValue;
			break;
		case "-":
			result = leftValue - rightValue;
			break;
		case "+":
			result = leftValue + rightValue;
			break;
		default:
			break;
	}
	return result;
}