using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;


namespace Calculator
{
	public class firstSolution
	{
		static void Main(string[] args)
		{
			Console.SetIn(new StreamReader(".\\calculator\\calculator.in"));

			// Selects the correct culture info (for this test)
			Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

			Regex numberRegex = new Regex(@"^-?\d+(\.\d+)?$");
			Regex arithmeticRegex = new Regex(@"^[\+\-\* \/]$");

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

					double result = double.Parse(HandleParentheses(equation));

					// Limits the decimal points to two
					Console.WriteLine(result.ToString("F2"));
					//Console.Write(": {0} \n", equation);
				}
			}

			/*	Recursive method that handles all instances of parentheses in the current form of 
			 *	the equation. It handles "complete" parentheses, meaning that the sub equation is 
			 *	not handled until there as many "end" parentheses as "start" parentheses. 
			 */
			string HandleParentheses(string equation)
			{
				string currentEquation = equation;

				Stack<int> parenthesesStart = new Stack<int>();
				Queue<int> parenthesesEnd = new Queue<int>();

				int startIndex = 0;
				int endIndex = 0;

				string updatedEquation = string.Empty;


				for (int i = 0; i < currentEquation.Length; i++)
				{
					if (currentEquation[i] == '(')  // Locates the start of a sub-equation
					{
						parenthesesStart.Push(i);

						// If this is the start of the first or outer parentheses then we register its index.
						if (parenthesesStart.Count == 1)
							startIndex = i + 1; // The parentheses itself is not included 
					}
					else if (currentEquation[i] == ')') // Locates the end of a sub-equation
					{
						parenthesesEnd.Enqueue(i);

						// If both start en end collections have the same value, then we have found a complete sub equation
						if (parenthesesEnd.Count == parenthesesStart.Count)
						{
							endIndex = i - startIndex; // The parentheses itself is not included 

							// Solves the complete sub equation returning a single value
							updatedEquation = string.Concat(updatedEquation,
								HandleParentheses(
									currentEquation.Substring(startIndex, endIndex)
								)
							);

							// On the return the parentheses should be solved. 
							parenthesesStart.Clear();
							parenthesesEnd.Clear();
						}
					}
					else if (parenthesesStart.Count == 0)  // Not part of parentheses, simply added to new equation.
					{
						updatedEquation = string.Concat(updatedEquation, equation[i]);
					}
				}

				// Solves the sub or complete equation
				return PrepareEquation(updatedEquation);
			}

			string PrepareEquation(string equation)
			{
				List<string> equationList = new List<string>();

				string intStringValue = string.Empty;
				string outerCharacter = string.Empty;

				bool prevTokenWasOperation = false;

				for (int i = 0; i < equation.Count(); i++)
				{
					//// The highest value the integer can have is 500.
					//// Hence the limit of the forward search to current index and two forward.
					//int iterationLimit = i + 3;

					outerCharacter = equation[i].ToString();

					// Adds operations to the list
					if (arithmeticRegex.IsMatch(outerCharacter))
					{
						// Checks if the previous token was an operation. Only add operation to list if it is
						// not a part of an integer otherwise, prepare it for next integer to be placed in list.
						if (i - 1 > -1)
						{
							if (arithmeticRegex.IsMatch(equation[i - 1].ToString()))
							{
								intStringValue = string.Concat(intStringValue, outerCharacter);
							}
							else
							{
								equationList.Add(outerCharacter);
							}
						}
						else
						{
							intStringValue = string.Concat(intStringValue, outerCharacter);
						}
					}
					else  // Adds integers to the list
					{
						string innerCharacter = string.Empty;

						for (int j = i; j < equation.Count(); j++)
						{
							innerCharacter = equation[j].ToString();

							bool isPartOfANumber = IsPartOfANumber(innerCharacter);

							// adds more chars to the int value
							if (isPartOfANumber /*&& j < iterationLimit*/)
							{
								intStringValue = string.Concat(intStringValue, innerCharacter);
								i = j;
							}
							else
							{
								// Next char is an operation, add the complete number to equationList
								equationList.Add(intStringValue);
								intStringValue = string.Empty;
								break;
							}
						}
					}
				}

				// Adds the last integer to the list. 
				if (equation.Length > 0)
					equationList.Add(intStringValue);

				// First solves multiplication and division then additions and subtractions.
				return SolveEquation(
							SolveEquation(equationList, "*", "/"),
							"+", "-").FirstOrDefault();

			}

			bool IsPartOfANumber(string value) => numberRegex.IsMatch(value) || value.Contains(".");


			List<string> SolveEquation(List<string> equationList, string rule1, string rule2)
			{
				string operation = string.Empty;

				Queue<double> variableValues = new Queue<double>();
				List<string> updatedEquation = new List<string>();

				// Adds first integer to list 
				variableValues.Enqueue(double.Parse(equationList[0]));

				for (int i = 1; i < equationList.Count; i++)
				{
					bool isPartOfANumber = IsPartOfANumber(equationList[i]);

					// Collects variables and operations to calculate 
					if (isPartOfANumber)
					{
						variableValues.Enqueue(double.Parse(equationList[i]));
					}
					else
					{
						operation = equationList[i];
					}

					if (variableValues.Count == 2)
					{
						if (operation == rule1 || operation == rule2)
						{
							variableValues.Enqueue(PerformOperation(variableValues, operation));
						}
						else
						{
							updatedEquation.Add(variableValues.Dequeue().ToString());
							updatedEquation.Add(operation);
						}
					}
				}

				// Performs the final operation and adds the final value
				//variableValues.Enqueue(PerformOperation(variableValues, operation));
				updatedEquation.Add(variableValues.Dequeue().ToString());

				return updatedEquation;
			}


			double PerformOperation(Queue<double> variables, string operation)
			{
				double result = 0;

				switch (operation)
				{
					case "*":
						result = variables.Dequeue() * variables.Dequeue();
						break;
					case "/":
						result = variables.Dequeue() / variables.Dequeue();
						break;
					case "-":
						result = variables.Dequeue() - variables.Dequeue();
						break;
					case "+":
						result = variables.Dequeue() + variables.Dequeue();
						break;
					default:
						break;
				}
				return result;
			}
		}
	}
}


