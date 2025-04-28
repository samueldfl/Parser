namespace Domain.Types;

public class Node(string label)
{
    public string Label { get; private set; } = label;

    public List<Node> Children { get; private set; } = [];
}
