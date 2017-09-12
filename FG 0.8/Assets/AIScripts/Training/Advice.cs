using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// This class basically stores "advice" that might be given to the AI agent. 
/// This comes in the form of an action / result pair, meaning "if you do this action, the following result should occur"
/// </summary>
public class Advice
{
    public Action recommendedAction;
    public Result purportedResult;

    public Advice(Action action, Result result)
    {
        recommendedAction = action;
        purportedResult = result;
    }
}
