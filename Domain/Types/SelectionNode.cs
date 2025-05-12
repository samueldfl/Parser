namespace Domain.Types;

public class SelectionNode(string condition, AlgebraNode child) : AlgebraNode
{
    public string Condition { get; } = condition;

    public AlgebraNode Child { get; } = child;

    public override string ToAlgebraString() => $"σ_{Condition}({Child.ToAlgebraString()})";
}
