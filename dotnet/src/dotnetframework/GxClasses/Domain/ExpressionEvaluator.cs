using System;
using GeneXus.Metadata;
using GeneXus.Application;
using GeneXus.Configuration;
using System.Reflection;
using log4net;
using System.Collections;
namespace GeneXus.Utils
{
	
	/// <summary>
	/// The format of the expression allows numeric constants, PI, operators +, -, *, / and the following
	/// mathematical functions (pow, sqrt, cos, sin, tan, acos, asin, atan, floor, round, exp, ln, abs, int, frac, max, min)
	/// The functions max, min, and pow receive 2 arguments
	/// The rnd () function receives no arguments and returns a random number between 0 and 1
	///	The other functions receive 1 argument
	/// </summary>
	public class ExpressionEvaluator
	{
		static readonly ILog log = log4net.LogManager.GetLogger(typeof(GeneXus.Utils.ExpressionEvaluator));
		internal IGxContext context;
		internal bool throwExceptions;
		
		internal GXProperties parms = new GXProperties();
		internal string ns;
		internal string expr;
		internal short errCode;
		internal string errDescription;
		internal string parentAssembly;
		
		short PARAMETER_ERROR = 2;
		short EXPRESSION_ERROR = 3;
		short EVALUATION_ERROR = 4;
		short EXTERNAL_FUNCTION_ERROR = 5;
		internal bool iifContext = false;
		public GXProperties Variables
		{
			get { return parms; }
		}
		public string Expression
		{
			get { return expr; }
			set { expr = value; }
		}
		public short ErrCode
		{
			get { return errCode; }
		}
		public string ErrDescription
		{
			get { return errDescription; }
		}
		public decimal Evaluate()
		{
			try
			{
				return eval(expr).D;
			}
			catch (OverflowException e)
			{
				throwException(EVALUATION_ERROR, "Overflow: "+e.Message);
				return 0;
			}
		}
		public GxSimpleCollection<string> GetUsedVariables()
		{
			FastTokenizer ft = new FastTokenizer(getTokenizerExpression(expr));
			GxSimpleCollection<string> ls = new GxSimpleCollection<string>();

			string tk;
			tk = ft.NextToken();
			while (!ft.EOF())
			{
				if (tk.IndexOf("(") == -1 && System.Char.IsDigit(tk[0]))
					ls.Add(tk);
				tk = ft.NextToken();
			}
			return ls;
		}

		/// <summary>Set the variables and their values.
		/// The format is: VarName1=Valor1;VarName2=Valor2;....;VarNameN=ValorN
		/// </summary>
		virtual public String Parms
		{
			set
			{
				parms.Clear();
				Tokenizer tokenizer = new Tokenizer(value, ";", false);
				while (tokenizer.HasMoreTokens())
				{
					String parm = tokenizer.NextToken().Trim();
					if (String.IsNullOrEmpty(parm))
						continue;
					int index = parm.IndexOf((System.Char) '=');
					if (index == - 1 || System.Char.IsDigit(parm[0]))
					{
						throwException(PARAMETER_ERROR, "Parm " + parm + " does not comply with format: 'ParmName=ParmValue'");
					}
					else
					{
						try
						{
							String parmName = parm.Substring(0, (index) - (0)).Trim().ToLower();
							String parmValue = parm.Substring(index + 1).Trim();
							if (parmValue.Length > 0 && !System.Char.IsDigit(parmValue[0]))
							{
								if(parms.ContainsKey(parmValue))
								{
									parms[parmName] = parms[parmValue];
								}
								else
								{
									throwException(PARAMETER_ERROR, "Unknown parameter '" + parmValue + "'");
								}
								continue;
							}
							parms[parmName] = parmValue;
						}
						
						catch (System.Exception e)
						{
							throwException(PARAMETER_ERROR, "Parm " + parm + " cannot be evaluated: " + e.Message);
						}
					}
				}
			}
		}
		virtual public bool ThrowExceptions
		{
			set
			{
				this.throwExceptions = value;
			}
			
		}
		public ExpressionEvaluator(IGxContext context)
		{
			this.context = context;
			Parms = "";
			string nspace;
			if (Config.GetValueOf("AppMainNamespace", out nspace))
				ns = nspace;
			parentAssembly = Assembly.GetCallingAssembly().FullName;
		}
		public ExpressionEvaluator(IGxContext context, int handle, String varParms)
		{
			this.context = context;
			Parms = varParms;
			string nspace;
			if ( Config.GetValueOf("AppMainNamespace", out nspace ) )
				ns = nspace;
			parentAssembly = Assembly.GetCallingAssembly().FullName;
		}
		public static Decimal eval(IGxContext context, int handle, String expression, out short err, out String errMsg, String parms)
		{
			err = 1;
			errMsg = "";
			try
			{
				if (String.IsNullOrEmpty(expression.Trim()))
				{
					return 0;
				}
				ExpressionEvaluator eval = new ExpressionEvaluator(context, handle, parms);
				return eval.eval(expression).D;
			}
			catch (System.FormatException e)
			{
				err = 0;
				errMsg = "Invalid number: " + e.Message;
				return 0;
			}
			catch (System.Exception e)
			{
				err = 0;
				errMsg = e.Message;
				return 0;
			}
		}
		
		EvalValue eval(String expression)
		{
			errCode = 0;
			errDescription = "";
			if (expression.Trim().Length == 0)
				return throwException(EXPRESSION_ERROR, "Empty expression");

			if (!matchParentesis(expression))
			{
				return throwException(EXPRESSION_ERROR, "The expression '" + expression + "' has unbalanced parenthesis");
			}
			String delim = "'!+-/*><=" + GE + LE + AND + OR + NE;
			bool useParentheses = false;
			if (iifContext && (expression.Contains("" + AND) || expression.Contains("" + OR)))
			{
				delim = "" + AND + OR;
				useParentheses = true;
			}
			Tokenizer tokenizer = new Tokenizer(getTokenizerExpression(expression), delim, true, useParentheses);

			return evaluate(expression, tokenizer);
		}
		
		private EvalValue throwException(short errCod, string errDesc)
		{
			if (throwExceptions)
			{
				throw new System.ArgumentException(errDesc);
			}
			else
			{
				GXLogging.Error(log, errDesc);
				errCode = errCod;
				errDescription = errDesc;
				return 0;
			}
		}
		
		private const char GE = (char) (0x01);
		private const char LE = (char) (0x02);
		private const char AND = (char)(0x03);
		private const char OR = (char)(0x04);
		private const char NE = (char)(0x05);
		private String getTokenizerExpression(String expression)
		{
			int index;
			while ((index = expression.IndexOf("==")) != - 1)
				expression = expression.Substring(0, (index) - (0)) + expression.Substring(index + 1);
			while ((index = expression.IndexOf(">=")) != - 1)
				expression = expression.Substring(0, (index) - (0)) + GE + expression.Substring(index + 2);
			while ((index = expression.IndexOf("<=")) != - 1)
				expression = expression.Substring(0, (index) - (0)) + LE + expression.Substring(index + 2);
			while ((index = expression.IndexOf("&&")) != -1)
				expression = expression.Substring(0, (index) - (0)) + AND + expression.Substring(index + 2);
			while ((index = expression.IndexOf("||")) != -1)
				expression = expression.Substring(0, (index) - (0)) + OR + expression.Substring(index + 2);
			while ((index = expression.IndexOf("!=")) != -1)
				expression = expression.Substring(0, (index) - (0)) + NE + expression.Substring(index + 2);
			while ((index = expression.IndexOf("<>")) != -1)
				expression = expression.Substring(0, (index) - (0)) + NE + expression.Substring(index + 2);
			while ((index = indexOfKeyword(expression, "and", index)) != -1)
				expression = expression.Substring(0, (index) - (0)) + AND + expression.Substring(index + 3);
			while ((index = indexOfKeyword(expression, "or", index)) != -1)
				expression = expression.Substring(0, (index) - (0)) + OR + expression.Substring(index + 2);

			return expression;
		}

		private int indexOfKeyword( string expression, string keyw, int nextIndex)
		{
			string exp = expression.ToLower();
			int index = exp.IndexOf(keyw.ToLower(), nextIndex + 1);
			while (index >= 0)
			{
				if (validKeyword(expression, index, keyw))
					return index;
				index = exp.IndexOf(keyw.ToLower(), index+1);
			}
			return index;
		}

		bool validKeyword(string expression, int index, string keyw)
		{
			if (Char.IsLetterOrDigit(expression[index - 1]) || "()!".IndexOf(expression[index - 1]) != -1)
				return false;
			if (index + keyw.Length < expression.Length)
				if (Char.IsLetterOrDigit(expression[index + keyw.Length]) || "()!".IndexOf(expression[index + keyw.Length]) != -1)
					return false;
			return true;
		}

		private EvalValue evaluate(String fullExpression, Tokenizer tokenizer)
		{
			return evaluate(fullExpression, tokenizer, false);
		}

		private EvalValue evaluate(String fullExpression, Tokenizer tokenizer, bool stopOnLowPrecedence)
		{
			EvalValue retVal = eval(tokenizer);
			EvalValue termino = 0;
			while (tokenizer.HasMoreTokens())
			{
				if (stopOnLowPrecedence)
				{
					string nextOp;
					if (tokenizer.Peek(out nextOp) && nextOp.Length >= 1 && (nextOp[0] == '+' || nextOp[0] == '-'))
						break;
				}

				string soperador = tokenizer.NextToken();
				if (String.IsNullOrEmpty( soperador.Trim())) continue;	

				char operador = soperador[0];
				
				switch (operador)
				{
					case '+':
						termino = evaluate(fullExpression, tokenizer, true);
						retVal = retVal + termino;
						break;
					
					case '-':
						termino = evaluate(fullExpression, tokenizer, true);
						retVal = retVal - termino;
						break;
					
					case '*': 
						termino = eval(tokenizer);
						retVal = retVal * termino;
						break;
					
					case '/': 
						termino = eval(tokenizer);
						if (termino == 0 && errCode == 0)
							throwException(EVALUATION_ERROR, "Division by zero");
						if (errCode == 0)
							retVal = retVal / termino;
						break;
					
					case '>':  return retVal > evaluate(fullExpression, tokenizer)?1:0;

					case '<': return retVal < evaluate(fullExpression, tokenizer) ? 1 : 0;

					case '=': return retVal == evaluate(fullExpression, tokenizer) ? 1:0;

					case GE: return retVal >= evaluate(fullExpression, tokenizer) ? 1 : 0;

					case LE: return retVal <= evaluate(fullExpression, tokenizer) ? 1 : 0;

					case AND: return (retVal != 0) && (evaluate(fullExpression, tokenizer) != 0) ? 1 : 0;

					case OR: return (retVal != 0) || (evaluate(fullExpression, tokenizer) != 0) ? 1 : 0;

					case NE: return retVal != evaluate(fullExpression, tokenizer) ? 1 : 0;

					default:
						throwException( EVALUATION_ERROR, "Unknown operator '" + soperador + "' found in expression '" + fullExpression + "'");
						break;
				}
			}
			return retVal;
		}
		
		private EvalValue eval(Tokenizer tokenizer)
		{
			String token = getNextToken(tokenizer);
			
			if (token.ToUpper().Equals("!".ToUpper()))
			{
				// Expresion '!expr'
				return eval(tokenizer) == 0 ? 1 : 0;
			}
			if (token.ToUpper().Equals("-".ToUpper()))
			{
				// Expresion '-expr'
				return - eval(tokenizer);
			}
			if (token.ToUpper().Equals("+".ToUpper()))
			{
				// Expresion '+expr'
				return eval(tokenizer);
			}
			
			// Finished processing
			// So check if it is an expression with parentheses, a number, or a function
			if (token.StartsWith("(") && token.EndsWith(")"))
			{
				// Si es una expresion entre parentesis
				return eval(token.Substring(1, (token.Length - 1) - (1)).Trim());
			}
			if (System.Char.IsDigit(token[0]) || token[0] == '.')
			{
				// If it is a constant (number)
				try
				{
					return decimal.Parse(token, System.Globalization.CultureInfo.InvariantCulture);
				}
				catch (FormatException e)
				{
					throwException(EVALUATION_ERROR, e.Message);
					return 0;
				}
			}
			if (token.StartsWith("'"))
			{
				// If it is a constant (char)
				string s = "";
				string tk = tokenizer.NextToken();
				while (tk != "'")
				{
					s += tk;
					tk = tokenizer.NextToken();
				}
				return new EvalValue( s);
			}
			if (token.ToUpper().Equals("PI".ToUpper()))
			{
				return (decimal)System.Math.PI;
			}
			
			if (parms.ContainsKey(token))
			{
				// If it is a variable, return its value
				try
				{
					return eval(parms[token]);
					
				}
				catch (FormatException e)
				{
					throwException(EVALUATION_ERROR, "Variable " + token + " cannot be evaluated: " + e.Message);
				}
			}
			
			//If it is none of this, it must be a function
			
			int indexLeftParen = token.IndexOf((System.Char) '(');
			int indexRightParen = token.LastIndexOf((System.Char) ')');
			if (indexLeftParen == -1 || indexRightParen == -1 || indexRightParen <= indexLeftParen)
			{
				// If it is not a function, it is a variable without reference
				return throwException(EVALUATION_ERROR, "Invalid variable reference: " + token);
			}
			
			String funcName = token.Substring(0, (indexLeftParen) - (0));
			return evalFuncCall(funcName, token.Substring(indexLeftParen + 1, (indexRightParen) - (indexLeftParen + 1)));
		}
		
		private decimal evalFuncCall(String funcName, String expr)
		{
			EvalValue result=0;
			if (funcName.ToUpper().Equals("rnd".ToUpper()))
			{
				// random function
				result = (decimal)NumberUtil.Random();
			}
			else		
				// single variable function
				if (funcName.ToUpper().Equals("abs".ToUpper()))
			{
				result=System.Math.Abs(eval(expr).D);
			}
			else	if (funcName.ToUpper().Equals("int".ToUpper()))
			{
				result = System.Math.Floor(eval(expr).D);
			}
			else	if (funcName.ToUpper().Equals("frac".ToUpper()))
			{
				EvalValue value = eval(expr);
				EvalValue int_part = System.Math.Floor(value.D);
				result= value - int_part;
			}
            else if (funcName.ToUpper().Equals("sin".ToUpper()))
			{
				result= (decimal)System.Math.Sin(Convert.ToDouble( (decimal)eval(expr)));
			}
			else	if (funcName.ToUpper().Equals("asin".ToUpper()))
			{
				result = (decimal)System.Math.Asin(Convert.ToDouble((decimal)eval(expr)));
			}
			else	if (funcName.ToUpper().Equals("cos".ToUpper()))
			{
				result = (decimal)System.Math.Cos(Convert.ToDouble((decimal)eval(expr)));
			}
			else	if (funcName.ToUpper().Equals("acos".ToUpper()))
			{
				decimal x = eval(expr).D;
				if (x > 1 || x < -1)
					throwException(EVALUATION_ERROR, "Invalid range");
				result = (decimal)System.Math.Acos(Convert.ToDouble(x));
			}
			else	if (funcName.ToUpper().Equals("tan".ToUpper()))
			{
				result = (decimal)System.Math.Tan(Convert.ToDouble((decimal)eval(expr)));
			}
			else	if (funcName.ToUpper().Equals("atan".ToUpper()))
			{
				result = (decimal)System.Math.Atan(Convert.ToDouble((decimal)eval(expr)));
			}
			else	if (funcName.ToUpper().Equals("floor".ToUpper()))
			{
				result = (decimal)System.Math.Floor(Convert.ToDouble((decimal)eval(expr)));
			}
			else	if (funcName.ToUpper().Equals("round".ToUpper()))
			{
				result=  System.Math.Round(eval(expr));
			}
			else	if (funcName.ToUpper().Equals("ln".ToUpper()) || funcName.ToUpper().Equals("log".ToUpper()))
			{
				decimal val = eval(expr);
				if (val <= 0)
				{
					return throwException(EVALUATION_ERROR, "Illegal argument (" + val + ") to function log(" + expr + ")");
				}
				result= (decimal)System.Math.Log(Convert.ToDouble( val));
			}
			else	if (funcName.ToUpper().Equals("exp".ToUpper()))
			{
				result = (decimal)System.Math.Exp(Convert.ToDouble((decimal)eval(expr)));
			}
			else		if (funcName.ToUpper().Equals("sqrt".ToUpper()))
			{
				result = (decimal)System.Math.Sqrt(Convert.ToDouble((decimal)eval(expr)));
			}
			else		
				// internal functions of 2 variables
				if (funcName.ToUpper().Equals("pow".ToUpper()) || funcName.ToUpper().Equals("max".ToUpper()) || funcName.ToUpper().Equals("min".ToUpper()))
			{
				Tokenizer paramTokenizer = new Tokenizer(expr, ",", true);
				String sarg1, sarg2;
				try
				{
					sarg1 = getNextToken(paramTokenizer);
					paramTokenizer.NextToken();
					sarg2 = getNextToken(paramTokenizer);
				}
				catch (System.ArgumentOutOfRangeException )
				{
					return throwException(EVALUATION_ERROR, "The function " + funcName + " needs 2 arguments");
				}
				
				decimal arg1 = eval(sarg1);
				decimal arg2 = eval(sarg2);
				
				if (funcName.ToUpper().Equals("pow".ToUpper()))
				{
					result= (decimal)System.Math.Pow(Convert.ToDouble( arg1), Convert.ToDouble( arg2));
				}
				else if (funcName.ToUpper().Equals("max".ToUpper()))
				{
					result= System.Math.Max(arg1, arg2);
				}
				else if (funcName.ToUpper().Equals("min".ToUpper()))
				{
					result= System.Math.Min(arg1, arg2);
				}
			}
			else
			
				if (funcName.ToUpper().Equals("iif".ToUpper()))
			{
				// iif
				Tokenizer paramTokenizer = new Tokenizer(expr, ",", true);
				String sarg1, sarg2, sarg3;
				try
				{
					sarg1 = getNextToken(paramTokenizer);
					paramTokenizer.NextToken();
					sarg2 = getNextToken(paramTokenizer);
					paramTokenizer.NextToken();
					sarg3 = getNextToken(paramTokenizer);
				}
				catch (System.ArgumentOutOfRangeException)
				{
					return throwException(EVALUATION_ERROR, "The function " + funcName + " needs 3 arguments");
				}
				iifContext = true;
				bool iif_result = (eval(sarg1) != 0);
				if (ErrCode != 0)
					return result;
				if (iif_result)
					result= eval(sarg2);
				else
					result= eval(sarg3);
				iifContext = false;
			}
			else
			{
			
				result= evalExternalFunctionCall(funcName, expr);
			}
			return result;
		}
		private void DynamicExecute(string funcName, object[] callParms)
		{
			if (!String.IsNullOrEmpty(parentAssembly))
				ClassLoader.Execute( parentAssembly, ns, funcName, new object[] { context }, "execute", callParms);
			else 
				ClassLoader.Execute( Assembly.GetExecutingAssembly().FullName, ns, funcName, new object[]{context}, "execute",  callParms);
		}
		private decimal evalExternalFunctionCall(String funcName, String expr)
		{
			Tokenizer paramTokenizer = new Tokenizer(expr, ",", true);
			System.Collections.ArrayList functionParms = System.Collections.ArrayList.Synchronized(new System.Collections.ArrayList(10));
			while (paramTokenizer.HasMoreTokens())
			{
				String arg = getNextToken(paramTokenizer).Trim();
				if ((arg.StartsWith("\"") || arg.StartsWith("'")) && (arg.EndsWith("\"") || arg.EndsWith("'")))
				{
					// it is a string
					functionParms.Add(arg.Substring(1, (arg.Length - 1) - (1)));
				}
				else
				{
					// evaluate the parameter
					EvalValue eValue = eval(arg);
					// The corresponding type of parameter is passed
					if (eValue.S == null)
						functionParms.Add(eValue.D);
					else
						functionParms.Add(eValue.S);
				}
				
				if (paramTokenizer.HasMoreTokens())
				{
					// Consume the ','
					paramTokenizer.NextToken();
				}
			}
			functionParms.Add((decimal) 0); 
			System.Object[] callParms = new System.Object[functionParms.Count];
			functionParms.CopyTo(callParms);
			
			bool pAdded = false;
			try
			{
				
				funcName = funcName.ToLower();

				DynamicExecute(funcName, callParms);

			}
			catch (System.Exception e)
			{
				bool retrySuccessful = false;
				
				if (!pAdded && e.ToString().IndexOf("ClassNotFoundException") != - 1)
				{
					try
					{
						
						DynamicExecute("p" + funcName, callParms);
						retrySuccessful = true;
					}
					catch (System.Exception)
					{
					}
				}
				if (!retrySuccessful)
				{
					
					return throwException(EXTERNAL_FUNCTION_ERROR, e.Message);
				}
			}

			return Convert.ToDecimal(callParms[callParms.Length - 1], System.Globalization.CultureInfo.InvariantCulture);
		}
		
		private String getNextToken(Tokenizer tokenizer)
		{
			String token = "";
			do 
			{
				token += tokenizer.NextToken();
			}
			while (!matchParentesis(token) || (String.IsNullOrEmpty(token.Trim()) && tokenizer.HasMoreTokens()));
			if (tokenizer.useParentheses() && !token.StartsWith("("))
			{
				return "(" + token.Trim() + ")";
			}
			return token.Trim();
		}
		
		private bool matchParentesis(String token)
		{
			int cantLeft = 0, cantRight = 0;
			char[] cars = new char[token.Length];
			token.CopyTo(0, cars, 0, cars.Length);
			for (int i = 0; i < cars.Length; i++)
			{
				char c = cars[i];
				if (c == '(')
				{
					cantLeft++;
				}
				if (c == ')')
				{
					cantRight++;
				}
			}
			return cantLeft == cantRight;
		}
	}

	/// <summary>
	/// The class performs token processing in strings
	/// </summary>
	public class Tokenizer
	{
		/// Position over the string
		private long currentPos;

		/// Include demiliters in the results.
		private bool includeDelims;

		/// Char representation of the String to tokenize.
		private char[] chars;
			
		//The tokenizer uses the default delimiter set: the space character, the tab character, the newline character, and the carriage-return character and the form-feed character
		private string delimiters = " \t\n\r\f";

		private bool parentheses;

		/// <summary>
		/// Initializes a new class instance with a specified string to process
		/// </summary>
		/// <param name="source">String to tokenize</param>
		public Tokenizer(String source)
		{			
			this.chars = source.ToCharArray();
		}

		/// <summary>
		/// Initializes a new class instance with a specified string to process
		/// and the specified token delimiters to use
		/// </summary>
		/// <param name="source">String to tokenize</param>
		/// <param name="delimiters">String containing the delimiters</param>
		public Tokenizer(String source, String delimiters):this(source)
		{			
			this.delimiters = delimiters;
		}

		/// <summary>
		/// Initializes a new class instance with a specified string to process, the specified token 
		/// delimiters to use, and whether the delimiters must be included in the results.
		/// </summary>
		/// <param name="source">String to tokenize</param>
		/// <param name="delimiters">String containing the delimiters</param>
		/// <param name="includeDelims">Determines if delimiters are included in the results.</param>
		public Tokenizer(String source, String delimiters, bool includeDelims):this(source,delimiters)
		{
			this.includeDelims = includeDelims;
		}

		public Tokenizer(String str, String delim, bool returnDelims, bool useParentheses) : this(str, delim, returnDelims)
		{
			this.parentheses = useParentheses;
		}

		/// <summary>
		/// Returns the next token from the token list
		/// </summary>
		/// <returns>The string value of the token</returns>
		public String NextToken()
		{				
			return NextToken(this.delimiters);
		}

		public bool Peek(out string nextToken)
		{
			long pos = this.currentPos;
			try
			{
				nextToken = this.NextToken().Trim();
				return true;
			}
			catch (System.ArgumentOutOfRangeException)
			{
				nextToken = String.Empty;
				return false;
			}
			finally
			{
				this.currentPos = pos;
			}
		}

		/// <summary>
		/// Returns the next token from the source string, using the provided
		/// token delimiters
		/// </summary>
		/// <param name="delimiters">String containing the delimiters to use</param>
		/// <returns>The string value of the token</returns>
		private String NextToken(String delimiters)
		{
			//According to documentation, the usage of the received delimiters should be temporary (only for this call).
			//However, it seems it is not true, so the following line is necessary.
			this.delimiters = delimiters;

			//at the end 
			if (this.currentPos == this.chars.Length)
				throw new System.ArgumentOutOfRangeException();
				//if over a delimiter and delimiters must be returned
			else if (   (System.Array.IndexOf(delimiters.ToCharArray(),chars[this.currentPos], 0, delimiters.Length) != -1)
				&& this.includeDelims )                	
				return "" + this.chars[this.currentPos++];
				//need to get the token wo delimiters.
			else
				return nextToken(delimiters.ToCharArray());
		}

		//Returns the nextToken wo delimiters
		private String nextToken(char[] delimiters)
		{
			string token="";
			long pos = this.currentPos;

			//skip possible delimiters
			while (System.Array.IndexOf(delimiters,this.chars[currentPos],0,delimiters.Length) != -1)
				//The last one is a delimiter (i.e there is no more tokens)
				if (++this.currentPos == this.chars.Length)
				{
					this.currentPos = pos;
					throw new System.ArgumentOutOfRangeException();
				}
			
			//getting the token
			while (System.Array.IndexOf(delimiters,this.chars[this.currentPos],0, delimiters.Length) == -1)
			{
				token+=this.chars[this.currentPos];
				//the last one is not a delimiter
				if (++this.currentPos == this.chars.Length)
					break;
			}
			return token;
		}
				
		/// <summary>
		/// Determines if there are more tokens to return from the source string
		/// </summary>
		/// <returns>True or false, depending if there are more tokens</returns>
		public bool HasMoreTokens()
		{
			string dummyToken;
			return Peek(out dummyToken);
		}

		public bool useParentheses()
		{
			return parentheses;
		}
	}

	public class FastTokenizer
	{
		int pos;
		string s;
		string delimiters = "!+-/*><=()"+GE+LE+AND+OR+NE;
		string includeDelimiters = "(";
		private const char GE = (char)(0x01);
		private const char LE = (char)(0x02);
		private const char AND = (char)(0x03);
		private const char OR = (char)(0x04);
		private const char NE = (char)(0x05);
		bool eof;

		public FastTokenizer(string s)
		{
			this.s = s;
		}
		public bool EOF()
		{
			return eof;
		}
		public string NextToken()
		{
			string tk = "";
			if (pos >= s.Length)
				eof = true;
			while (pos < s.Length)
			{
				char c = s[pos++];
				
				if (delimiters.IndexOf(c) != -1)
				{
					
					if (includeDelimiters.IndexOf(c) != -1)
						tk += c;

					if (!String.IsNullOrEmpty(tk))
						return tk;
				}
				else
					tk += c;
			}
			return tk;
		}
	}
	
	class EvalValue
	{
		string stringValue;
		Decimal decimalValue;
		public EvalValue(Decimal d)
		{
			decimalValue = d;
		}
		public EvalValue(string s)
		{
			stringValue = s;
		}
		public decimal D
		{
			get {return decimalValue;}
		}
		public string S
		{
			get {return stringValue;}
		}

		public static EvalValue operator +(EvalValue a, EvalValue b)
		{
			if (a.stringValue == null)
				return new EvalValue(a.D + b.D);
			else
				return new EvalValue(a.S + b.S);
		}
		public static EvalValue operator -(EvalValue a, EvalValue b)
		{
			if (a.stringValue == null)
				return new EvalValue(a.D - b.D);
			else
				throw new ArgumentException("Invalid operation: string - string");
		}
		public static EvalValue operator *(EvalValue a, EvalValue b)
		{
			if (a.stringValue == null)
				return new EvalValue(a.D * b.D);
			else
				throw new ArgumentException( "Invalid operation: string * string");
		}
		public static EvalValue operator /(EvalValue a, EvalValue b)
		{
			if (a.stringValue == null)
				return new EvalValue(a.D / b.D);
			else
				throw new ArgumentException("Invalid operation: string / string");
		}
		public static bool operator >(EvalValue a, EvalValue b)
		{
			if (a.stringValue == null)
				return a.D > b.D;
			else
				throw new ArgumentException("Invalid operation: string > string");
		}
		public static bool operator <(EvalValue a, EvalValue b)
		{
			if (a.stringValue == null)
				return a.D < b.D;
			else
				throw new ArgumentException("Invalid operation: string < string");
		}
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            EvalValue b = obj as EvalValue;
            if (stringValue == null)
                return D == b.D;
            else
                return S == b.S;
        }
		public static bool operator ==(EvalValue a, EvalValue b)
		{
			if (a.stringValue == null)
				return a.D == b.D;
			else
				return a.S == b.S;
		}
		public static bool operator !=(EvalValue a, EvalValue b)
		{
			if (a.stringValue == null)
				return a.D != b.D;
			else
				return a.S != b.S;
		}
		public static bool operator >=(EvalValue a, EvalValue b)
		{
			if (a.stringValue == null)
				return a.D >= b.D;
			else
				throw new ArgumentException("Invalid operation: string >= string");
		}
		public static bool operator <=(EvalValue a, EvalValue b)
		{
			if (a.stringValue == null)
				return a.D <= b.D;
			else
				throw new ArgumentException("Invalid operation: string <= string");
		}
		public static implicit operator decimal(EvalValue a)
		{
			if (a.stringValue == null)
				return a.D;
			else
				return Convert.ToDecimal(a.S, System.Globalization.CultureInfo.InvariantCulture);
		}
		public static implicit operator string(EvalValue a)
		{
			if (a.stringValue == null)
				return Convert.ToString(a.D);
			else
				return a.S;
		}
		public static implicit operator EvalValue(int a)
		{
			return new EvalValue( a);
		}
		public static implicit operator EvalValue(decimal a)
		{
			return new EvalValue(a);
		}
	}
}

