
using System.Collections;
using System.Collections.Generic;

public enum Comparison{ EQUAL, LESS, GREATER, LEQUAL, GEQUAL }

public class CriterionData{
       
	public const float Precision=0.001f;
        
	public string 	Name	{get;private set;}
    float MinBound, MaxBound;

    public CriterionData(string name, float Value, Comparison operation)
    {
        Name = name;
		set_criterion(Value,operation);
	}

    public bool Check(float Value)
    {
        return Value >= MinBound && Value <= MaxBound;
    }
		
	private void set_criterion(float Value,Comparison operation){
		float min=0,max=0;

		if (operation==Comparison.EQUAL){
            min = max = Value;
		}
		else if (operation==Comparison.LESS){
			min=float.NegativeInfinity;max=Value-Precision;
		}
		else if (operation==Comparison.LEQUAL){
			min=float.MinValue;max=Value;
		}
		else if (operation==Comparison.GREATER){
			min=Value+Precision;max=float.PositiveInfinity;
		}
		else{
			min=Value;max=float.PositiveInfinity;
		}
        MinBound=min;MaxBound=max;
	}
}
